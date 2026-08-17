using Statecraft.UI.Themes;
using UnityEngine;

namespace Statecraft.Data
{
    [CreateAssetMenu(fileName = "Country", menuName = "Statecraft/Data/Country")]
    public sealed class CountryDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string visualIdentifier;
        [SerializeField] private string mapGeographicId;
        [SerializeField] private Sprite flag = null;
        [SerializeField] private long population;
        [SerializeField] private double gdpUsd;
        [SerializeField] private string capital;
        [SerializeField] private CountryTheme theme;
        [SerializeField] private LeaderDefinition leader;

        public string Id => id;
        public string DisplayName => displayName;
        public string VisualIdentifier => visualIdentifier;
        public string MapGeographicId => mapGeographicId;
        public Sprite Flag => flag;
        public long Population => population;
        public double GdpUsd => gdpUsd;
        public string Capital => capital;
        public CountryTheme Theme => theme;
        public LeaderDefinition Leader => leader;

#if UNITY_EDITOR
        public void Configure(
            string countryId,
            string countryName,
            string countryVisualIdentifier,
            string countryMapGeographicId,
            long countryPopulation,
            double countryGdpUsd,
            string countryCapital,
            CountryTheme countryTheme,
            LeaderDefinition countryLeader)
        {
            id = countryId;
            displayName = countryName;
            visualIdentifier = countryVisualIdentifier;
            mapGeographicId = countryMapGeographicId;
            population = countryPopulation;
            gdpUsd = countryGdpUsd;
            capital = countryCapital;
            theme = countryTheme;
            leader = countryLeader;
        }
#endif
    }
}
