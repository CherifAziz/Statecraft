using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Statecraft.Simulation.Tests
{
    public sealed class SimulationFoundationTests
    {
        private ISimulationCountryDefinition[] definitions;
        private ISimulationCountryDefinition france;
        private ISimulationCountryDefinition tunisia;

        [SetUp]
        public void SetUp()
        {
            definitions = Resources.LoadAll<ScriptableObject>("GameData")
                .OfType<ISimulationCountryDefinition>()
                .OrderBy(definition => definition.Id, StringComparer.Ordinal)
                .ToArray();
            france = definitions.Single(definition => definition.Id == "france");
            tunisia = definitions.Single(definition => definition.Id == "tunisia");
        }

        [Test]
        public void CreateNewGame_FranceCreatesActiveSessionWithCatalogStates()
        {
            var session = GameSessionFactory.CreateNewGame(france, definitions);

            Assert.That(session.IsActive, Is.True);
            Assert.That(session.CountryStates.Count, Is.EqualTo(definitions.Length));
            Assert.That(session.CountryStates.Keys, Is.EquivalentTo(definitions.Select(definition => definition.Id)));
        }

        [Test]
        public void CreateNewGame_TunisiaCreatesActiveSessionWithCatalogStates()
        {
            var session = GameSessionFactory.CreateNewGame(tunisia, definitions);

            Assert.That(session.IsActive, Is.True);
            Assert.That(session.CountryStates.Count, Is.EqualTo(definitions.Length));
        }

        [Test]
        public void PlayerCountry_MatchesRequestedDefinition()
        {
            var franceSession = GameSessionFactory.CreateNewGame(france, definitions);
            var tunisiaSession = GameSessionFactory.CreateNewGame(tunisia, definitions);

            Assert.That(franceSession.PlayerCountry, Is.SameAs(france));
            Assert.That(franceSession.PlayerCountryId, Is.EqualTo("france"));
            Assert.That(tunisiaSession.PlayerCountry, Is.SameAs(tunisia));
            Assert.That(tunisiaSession.PlayerCountryId, Is.EqualTo("tunisia"));
        }

        [Test]
        public void RuntimeMutations_NeverModifyDefinitionOrSetupAssets()
        {
            var sourcePopulation = france.Population;
            var sourceGdp = france.GdpUsd;
            var sourceApproval = france.SimulationSetup.PublicApproval;
            var sourceTreasury = france.SimulationSetup.TreasuryUsd;
            var session = GameSessionFactory.CreateNewGame(france, definitions);

            session.PlayerCountryState.ModifyPopulation(50_000L);
            session.PlayerCountryState.ModifyGdp(10_000_000d);
            session.PlayerCountryState.ModifyPublicApproval(-8f);
            session.PlayerCountryState.ModifyTreasury(-1_000_000d);

            Assert.That(france.Population, Is.EqualTo(sourcePopulation));
            Assert.That(france.GdpUsd, Is.EqualTo(sourceGdp));
            Assert.That(france.SimulationSetup.PublicApproval, Is.EqualTo(sourceApproval));
            Assert.That(france.SimulationSetup.TreasuryUsd, Is.EqualTo(sourceTreasury));
        }

        [Test]
        public void CountryState_InitializesPopulationFromDefinition()
        {
            var session = GameSessionFactory.CreateNewGame(france, definitions);

            Assert.That(session.PlayerCountryState.Population, Is.EqualTo(68_600_000L));
        }

        [Test]
        public void CountryState_InitializesGdpFromDefinition()
        {
            var session = GameSessionFactory.CreateNewGame(tunisia, definitions);

            Assert.That(session.PlayerCountryState.GdpUsd, Is.EqualTo(52_000_000_000d));
        }

        [Test]
        public void FranceGameplaySetup_SeedsDocumentedTemporaryValues()
        {
            var state = GameSessionFactory.CreateNewGame(france, definitions).PlayerCountryState;

            Assert.That(state.TreasuryUsd, Is.EqualTo(85_000_000_000d));
            Assert.That(state.PublicApproval, Is.EqualTo(54f));
            Assert.That(state.Stability, Is.EqualTo(72f));
            Assert.That(state.PoliticalCapital, Is.EqualTo(63f));
        }

        [Test]
        public void TunisiaGameplaySetup_SeedsDocumentedTemporaryValues()
        {
            var state = GameSessionFactory.CreateNewGame(tunisia, definitions).PlayerCountryState;

            Assert.That(state.TreasuryUsd, Is.EqualTo(12_000_000_000d));
            Assert.That(state.PublicApproval, Is.EqualTo(61f));
            Assert.That(state.Stability, Is.EqualTo(64f));
            Assert.That(state.PoliticalCapital, Is.EqualTo(70f));
        }

        [Test]
        public void PublicApproval_ClampsToZeroAndOneHundred()
        {
            var state = GameSessionFactory.CreateNewGame(france, definitions).PlayerCountryState;

            Assert.That(state.ModifyPublicApproval(1000f), Is.EqualTo(100f));
            Assert.That(state.ModifyPublicApproval(-1000f), Is.EqualTo(0f));
        }

        [Test]
        public void Stability_ClampsToZeroAndOneHundred()
        {
            var state = GameSessionFactory.CreateNewGame(france, definitions).PlayerCountryState;

            Assert.That(state.ModifyStability(1000f), Is.EqualTo(100f));
            Assert.That(state.ModifyStability(-1000f), Is.EqualTo(0f));
        }

        [Test]
        public void PoliticalCapital_ClampsToZeroAndOneHundred()
        {
            var state = GameSessionFactory.CreateNewGame(france, definitions).PlayerCountryState;

            Assert.That(state.ModifyPoliticalCapital(1000f), Is.EqualTo(100f));
            Assert.That(state.ModifyPoliticalCapital(-1000f), Is.EqualTo(0f));
        }

        [Test]
        public void Treasury_CanMovePositiveAndNegative()
        {
            var state = GameSessionFactory.CreateNewGame(tunisia, definitions).PlayerCountryState;

            Assert.That(state.ModifyTreasury(-15_000_000_000d), Is.EqualTo(-3_000_000_000d));
            Assert.That(state.ModifyTreasury(5_000_000_000d), Is.EqualTo(2_000_000_000d));
        }

        [Test]
        public void PopulationAndGdp_CannotFallBelowZero()
        {
            var state = GameSessionFactory.CreateNewGame(tunisia, definitions).PlayerCountryState;

            Assert.That(state.ModifyPopulation(long.MinValue), Is.EqualTo(0L));
            Assert.That(state.ModifyGdp(-double.MaxValue), Is.EqualTo(0d));
        }

        [Test]
        public void NonFiniteNumericMutations_AreRejected()
        {
            var state = GameSessionFactory.CreateNewGame(france, definitions).PlayerCountryState;

            Assert.Throws<ArgumentOutOfRangeException>(() => state.ModifyTreasury(double.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => state.ModifyPublicApproval(float.NaN));
        }

        [Test]
        public void GameClock_AdvancesOneDay()
        {
            var clock = new GameClock(GameClock.V1InitialDate);

            Assert.That(clock.AdvanceOneDay(), Is.EqualTo(new DateTime(2026, 9, 2)));
        }

        [Test]
        public void GameClock_AdvancesThirtyDaysAcrossMonthBoundary()
        {
            var clock = new GameClock(GameClock.V1InitialDate);

            Assert.That(clock.AdvanceDays(30), Is.EqualTo(new DateTime(2026, 10, 1)));
        }

        [Test]
        public void GameClock_RejectsNegativeDays()
        {
            var clock = new GameClock(GameClock.V1InitialDate);

            Assert.Throws<ArgumentOutOfRangeException>(() => clock.AdvanceDays(-1));
            Assert.That(clock.CurrentDate, Is.EqualTo(GameClock.V1InitialDate));
        }

        [Test]
        public void RestartingSession_RestoresFreshInitialState()
        {
            var runtime = new GameRuntime(definitions);
            var firstSession = runtime.StartNewGame(france);
            firstSession.PlayerCountryState.ModifyPublicApproval(-8f);
            firstSession.AdvanceDays(3);

            runtime.EndSession();
            var restartedSession = runtime.StartNewGame(france);

            Assert.That(restartedSession.PlayerCountryState.PublicApproval, Is.EqualTo(54f));
            Assert.That(restartedSession.Clock.CurrentDate, Is.EqualTo(GameClock.V1InitialDate));
            Assert.That(firstSession.IsActive, Is.False);
        }

        [Test]
        public void StartingNewMandate_ReplacesPreviousSessionWithCleanPlayerState()
        {
            var runtime = new GameRuntime(definitions);
            var franceSession = runtime.StartNewGame(france);
            franceSession.PlayerCountryState.ModifyPublicApproval(-8f);

            var tunisiaSession = runtime.StartNewGame(tunisia);

            Assert.That(franceSession.IsActive, Is.False);
            Assert.That(tunisiaSession.IsActive, Is.True);
            Assert.That(runtime.CurrentSession, Is.SameAs(tunisiaSession));
            Assert.That(tunisiaSession.PlayerCountryId, Is.EqualTo("tunisia"));
            Assert.That(tunisiaSession.PlayerCountryState.PublicApproval, Is.EqualTo(61f));
        }

        [Test]
        public void FranceAndTunisia_RuntimeStatesAreIndependent()
        {
            var session = GameSessionFactory.CreateNewGame(france, definitions);
            var franceState = session.GetCountryState("france");
            var tunisiaState = session.GetCountryState("tunisia");
            var tunisiaApproval = tunisiaState.PublicApproval;

            franceState.ModifyPublicApproval(-8f);

            Assert.That(franceState.PublicApproval, Is.EqualTo(46f));
            Assert.That(tunisiaState.PublicApproval, Is.EqualTo(tunisiaApproval));
            Assert.That(franceState, Is.Not.SameAs(tunisiaState));
        }

        [Test]
        public void CountryStateChange_FiresStateAndSessionNotifications()
        {
            var session = GameSessionFactory.CreateNewGame(france, definitions);
            var countryNotifications = 0;
            var sessionNotifications = 0;
            CountryStateChange observedChange = default;
            session.PlayerCountryState.Changed += (_, change) =>
            {
                countryNotifications++;
                observedChange = change;
            };
            session.StateChanged += _ => sessionNotifications++;

            session.PlayerCountryState.ModifyPublicApproval(-8f);

            Assert.That(countryNotifications, Is.EqualTo(1));
            Assert.That(sessionNotifications, Is.EqualTo(1));
            Assert.That(observedChange, Is.EqualTo(CountryStateChange.PublicApproval));
        }

        [Test]
        public void GameClockChange_FiresDateAndSessionNotifications()
        {
            var session = GameSessionFactory.CreateNewGame(france, definitions);
            var dateNotifications = 0;
            var sessionNotifications = 0;
            session.Clock.DateChanged += _ => dateNotifications++;
            session.StateChanged += _ => sessionNotifications++;

            session.AdvanceDays(3);

            Assert.That(session.Clock.CurrentDate, Is.EqualTo(new DateTime(2026, 9, 4)));
            Assert.That(dateNotifications, Is.EqualTo(1));
            Assert.That(sessionNotifications, Is.EqualTo(1));
        }
    }
}
