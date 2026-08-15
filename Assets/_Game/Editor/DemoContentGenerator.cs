using System.IO;
using Statecraft.Data;
using Statecraft.UI.Themes;
using UnityEditor;
using UnityEngine;

namespace Statecraft.Editor
{
    public static class DemoContentGenerator
    {
        private const string Root = "Assets/_Game/Resources/GameData";
        private const string CatalogPath = Root + "/CountryCatalog.asset";

        [InitializeOnLoadMethod]
        private static void GenerateOnFirstImport()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && AssetDatabase.LoadAssetAtPath<CountryCatalog>(CatalogPath) == null)
                {
                    Generate();
                }
            };
        }

        [MenuItem("Statecraft/Regenerate Demo Content")]
        public static void Generate()
        {
            EnsureFolder("Assets/_Game/Resources");
            EnsureFolder(Root);

            var franceTheme = CreateOrLoad<CountryTheme>(Root + "/FranceTheme.asset");
            franceTheme.Configure(
                Html("#11110F"), Html("#F0E9D8"), Html("#C6A15B"), Html("#090A09"),
                Html("#171713"), Html("#F5EEDF"), Html("#B9B09E"), Html("#75613B"),
                Html("#D4B66D"), "elysee-republique", "ceremonial-fade");

            var tunisiaTheme = CreateOrLoad<CountryTheme>(Root + "/TunisiaTheme.asset");
            tunisiaTheme.Configure(
                Html("#7D0D20"), Html("#EFE4D1"), Html("#B97845"), Html("#24070C"),
                Html("#3A0B14"), Html("#FFF3DE"), Html("#D4BFA9"), Html("#8E4A38"),
                Html("#C88B57"), "carthage-mediterranee", "mosaic-reveal");

            var franceLeader = CreateOrLoad<LeaderDefinition>(Root + "/FranceLeader.asset");
            franceLeader.Configure("fr-leader-01", "Emmanuel Macron", "Président de la République", new LeaderStats
            {
                charisma = 78,
                diplomacy = 86,
                authority = 72,
                strategy = 81,
                economy = 75,
                eloquence = 89
            });

            var tunisiaLeader = CreateOrLoad<LeaderDefinition>(Root + "/TunisiaLeader.asset");
            tunisiaLeader.Configure("tn-leader-01", "Amine Ben Salem", "Président de la République", new LeaderStats
            {
                charisma = 82,
                diplomacy = 74,
                authority = 79,
                strategy = 76,
                economy = 71,
                eloquence = 84
            });

            var france = CreateOrLoad<CountryDefinition>(Root + "/France.asset");
            france.Configure("france", "France", "FR", 68_600_000, 3_200_000_000_000d, "Paris", franceTheme, franceLeader);

            var tunisia = CreateOrLoad<CountryDefinition>(Root + "/Tunisia.asset");
            tunisia.Configure("tunisia", "Tunisie", "TN", 12_300_000, 52_000_000_000d, "Tunis", tunisiaTheme, tunisiaLeader);

            var catalog = CreateOrLoad<CountryCatalog>(CatalogPath);
            catalog.Configure(new[] { france, tunisia });

            MarkDirty(franceTheme, tunisiaTheme, franceLeader, tunisiaLeader, france, tunisia, catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Statecraft demo content is ready.");
        }

        private static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

        private static Color Html(string value)
        {
            ColorUtility.TryParseHtmlString(value, out var color);
            return color;
        }

        private static void MarkDirty(params Object[] assets)
        {
            foreach (var asset in assets)
            {
                EditorUtility.SetDirty(asset);
            }
        }
    }
}
