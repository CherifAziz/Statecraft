using UnityEngine;

namespace Statecraft.Simulation
{
    [CreateAssetMenu(
        fileName = "CountrySimulationSetup",
        menuName = "Statecraft/Simulation/Country Setup")]
    public sealed class CountrySimulationSetup : ScriptableObject
    {
        [Header("Temporary gameplay tuning — not factual country data")]
        [SerializeField] private double treasuryUsd;
        [Range(CountryState.MinimumPercentage, CountryState.MaximumPercentage)]
        [SerializeField] private float publicApproval;
        [Range(CountryState.MinimumPercentage, CountryState.MaximumPercentage)]
        [SerializeField] private float stability;
        [Range(CountryState.MinimumPercentage, CountryState.MaximumPercentage)]
        [SerializeField] private float politicalCapital;

        public double TreasuryUsd => treasuryUsd;
        public float PublicApproval => publicApproval;
        public float Stability => stability;
        public float PoliticalCapital => politicalCapital;

#if UNITY_EDITOR
        public void Configure(
            double initialTreasuryUsd,
            float initialPublicApproval,
            float initialStability,
            float initialPoliticalCapital)
        {
            treasuryUsd = initialTreasuryUsd;
            publicApproval = initialPublicApproval;
            stability = initialStability;
            politicalCapital = initialPoliticalCapital;
        }
#endif
    }
}
