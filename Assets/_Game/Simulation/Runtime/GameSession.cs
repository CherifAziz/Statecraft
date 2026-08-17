using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Statecraft.Simulation
{
    public sealed class GameSession
    {
        private readonly Dictionary<string, CountryState> countryStates;
        private readonly ReadOnlyDictionary<string, CountryState> readOnlyCountryStates;

        internal GameSession(
            CountryState playerCountryState,
            Dictionary<string, CountryState> states,
            GameClock clock)
        {
            PlayerCountryState = playerCountryState ?? throw new ArgumentNullException(nameof(playerCountryState));
            countryStates = states ?? throw new ArgumentNullException(nameof(states));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            readOnlyCountryStates = new ReadOnlyDictionary<string, CountryState>(countryStates);
            IsActive = true;

            foreach (var countryState in countryStates.Values)
            {
                countryState.Changed += OnCountryStateChanged;
            }

            Clock.DateChanged += OnDateChanged;
        }

        public bool IsActive { get; private set; }
        public string PlayerCountryId => PlayerCountryState.CountryId;
        public ISimulationCountryDefinition PlayerCountry => PlayerCountryState.Definition;
        public CountryState PlayerCountryState { get; }
        public GameClock Clock { get; }
        public IReadOnlyDictionary<string, CountryState> CountryStates => readOnlyCountryStates;

        public event Action<GameSession> StateChanged;

        public bool TryGetCountryState(string countryId, out CountryState state)
        {
            return countryStates.TryGetValue(countryId, out state);
        }

        public CountryState GetCountryState(string countryId)
        {
            if (string.IsNullOrWhiteSpace(countryId))
            {
                throw new ArgumentException("A country ID is required.", nameof(countryId));
            }

            return countryStates.TryGetValue(countryId, out var state)
                ? state
                : throw new KeyNotFoundException($"No runtime state exists for country '{countryId}'.");
        }

        public DateTime AdvanceDays(int days)
        {
            EnsureActive();
            return Clock.AdvanceDays(days);
        }

        internal void End()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            StateChanged?.Invoke(this);
        }

        private void OnCountryStateChanged(CountryState state, CountryStateChange change)
        {
            StateChanged?.Invoke(this);
        }

        private void OnDateChanged(GameClock clock)
        {
            StateChanged?.Invoke(this);
        }

        private void EnsureActive()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("The game session is no longer active.");
            }
        }
    }
}
