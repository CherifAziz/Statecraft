using System.Collections.Generic;
using UnityEngine;

namespace Statecraft.Data
{
    [CreateAssetMenu(fileName = "CountryCatalog", menuName = "Statecraft/Data/Country Catalog")]
    public sealed class CountryCatalog : ScriptableObject
    {
        [SerializeField] private List<CountryDefinition> countries = new();

        public IReadOnlyList<CountryDefinition> Countries => countries;

#if UNITY_EDITOR
        public void Configure(IEnumerable<CountryDefinition> countryDefinitions)
        {
            countries = new List<CountryDefinition>(countryDefinitions);
        }
#endif
    }
}
