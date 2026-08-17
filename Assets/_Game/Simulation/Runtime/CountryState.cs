using System;

namespace Statecraft.Simulation
{
    public enum CountryStateChange
    {
        Population,
        GdpUsd,
        TreasuryUsd,
        PublicApproval,
        Stability,
        PoliticalCapital
    }

    public sealed class CountryState
    {
        public const float MinimumPercentage = 0f;
        public const float MaximumPercentage = 100f;

        private long population;
        private double gdpUsd;
        private double treasuryUsd;
        private float publicApproval;
        private float stability;
        private float politicalCapital;

        internal CountryState(ISimulationCountryDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            var setup = definition.SimulationSetup ?? throw new ArgumentException(
                $"Country definition '{definition.Id}' has no simulation setup.",
                nameof(definition));

            population = Math.Max(0L, definition.Population);
            gdpUsd = NormalizeNonNegative(definition.GdpUsd, nameof(definition.GdpUsd));
            treasuryUsd = RequireFinite(setup.TreasuryUsd, nameof(setup.TreasuryUsd));
            publicApproval = NormalizePercentage(setup.PublicApproval, nameof(setup.PublicApproval));
            stability = NormalizePercentage(setup.Stability, nameof(setup.Stability));
            politicalCapital = NormalizePercentage(setup.PoliticalCapital, nameof(setup.PoliticalCapital));
        }

        public ISimulationCountryDefinition Definition { get; }
        public string CountryId => Definition.Id;
        public long Population => population;
        public double GdpUsd => gdpUsd;
        public double TreasuryUsd => treasuryUsd;
        public float PublicApproval => publicApproval;
        public float Stability => stability;
        public float PoliticalCapital => politicalCapital;

        public event Action<CountryState, CountryStateChange> Changed;

        public long ModifyPopulation(long delta)
        {
            long nextValue;
            if (delta < 0L && delta < -population)
            {
                nextValue = 0L;
            }
            else if (delta > 0L && population > long.MaxValue - delta)
            {
                nextValue = long.MaxValue;
            }
            else
            {
                nextValue = population + delta;
            }

            if (nextValue != population)
            {
                population = nextValue;
                NotifyChanged(CountryStateChange.Population);
            }

            return population;
        }

        public double ModifyGdp(double delta)
        {
            var nextValue = AddFinite(gdpUsd, delta, nameof(delta));
            nextValue = Math.Max(0d, nextValue);
            if (!nextValue.Equals(gdpUsd))
            {
                gdpUsd = nextValue;
                NotifyChanged(CountryStateChange.GdpUsd);
            }

            return gdpUsd;
        }

        public double ModifyTreasury(double delta)
        {
            var nextValue = AddFinite(treasuryUsd, delta, nameof(delta));
            if (!nextValue.Equals(treasuryUsd))
            {
                treasuryUsd = nextValue;
                NotifyChanged(CountryStateChange.TreasuryUsd);
            }

            return treasuryUsd;
        }

        public float ModifyPublicApproval(float delta)
        {
            var nextValue = AddPercentage(publicApproval, delta, nameof(delta));
            if (!nextValue.Equals(publicApproval))
            {
                publicApproval = nextValue;
                NotifyChanged(CountryStateChange.PublicApproval);
            }

            return publicApproval;
        }

        public float ModifyStability(float delta)
        {
            var nextValue = AddPercentage(stability, delta, nameof(delta));
            if (!nextValue.Equals(stability))
            {
                stability = nextValue;
                NotifyChanged(CountryStateChange.Stability);
            }

            return stability;
        }

        public float ModifyPoliticalCapital(float delta)
        {
            var nextValue = AddPercentage(politicalCapital, delta, nameof(delta));
            if (!nextValue.Equals(politicalCapital))
            {
                politicalCapital = nextValue;
                NotifyChanged(CountryStateChange.PoliticalCapital);
            }

            return politicalCapital;
        }

        private static double NormalizeNonNegative(double value, string parameterName)
        {
            return Math.Max(0d, RequireFinite(value, parameterName));
        }

        private static float NormalizePercentage(float value, string parameterName)
        {
            RequireFinite(value, parameterName);
            return Math.Max(MinimumPercentage, Math.Min(MaximumPercentage, value));
        }

        private static float AddPercentage(float current, float delta, string parameterName)
        {
            RequireFinite(delta, parameterName);
            var result = (double)current + delta;
            return (float)Math.Max(MinimumPercentage, Math.Min(MaximumPercentage, result));
        }

        private static double AddFinite(double current, double delta, string parameterName)
        {
            RequireFinite(delta, parameterName);
            var result = current + delta;
            if (double.IsNaN(result) || double.IsInfinity(result))
            {
                throw new ArgumentOutOfRangeException(parameterName, "The mutation must produce a finite value.");
            }

            return result;
        }

        private static double RequireFinite(double value, string parameterName)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "The value must be finite.");
            }

            return value;
        }

        private static float RequireFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "The value must be finite.");
            }

            return value;
        }

        private void NotifyChanged(CountryStateChange change)
        {
            Changed?.Invoke(this, change);
        }
    }
}
