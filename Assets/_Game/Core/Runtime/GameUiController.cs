using System.Collections.Generic;
using Statecraft.Data;
using Statecraft.UI.Screens;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.Core
{
    public sealed class GameUiController : MonoBehaviour
    {
        private const string CatalogResourcePath = "GameData/CountryCatalog";
        private const string StyleResourcePath = "UI/Statecraft";
        private const string LeaderStyleResourcePath = "UI/LeaderScreen";
        private const string RuntimeThemeResourcePath = "UI/StatecraftTheme";

        private PanelSettings panelSettings;
        private UIDocument document;
        private BootScreen bootScreen;
        private WorldMapScreen worldMapScreen;
        private LeaderScreen leaderScreen;
        private IReadOnlyList<CountryDefinition> countries;

        private void Start()
        {
            ConfigureDocument();
            LoadContent();
            BuildScreens();
            ShowBoot();
        }

        private void ConfigureDocument()
        {
            panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.name = "Statecraft Runtime Panel Settings";
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            panelSettings.match = 0.5f;
            panelSettings.themeStyleSheet = Resources.Load<ThemeStyleSheet>(RuntimeThemeResourcePath);

            if (panelSettings.themeStyleSheet == null)
            {
                Debug.LogError($"Missing runtime theme at Resources/{RuntimeThemeResourcePath}.tss.");
            }

            document = gameObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.sortingOrder = 100;

            var root = document.rootVisualElement;
            root.name = "statecraft-root";
            root.AddToClassList("app-root");
            root.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                root.EnableInClassList("compact", evt.newRect.width < 1400f);
            });

            var styleSheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (styleSheet != null)
            {
                root.styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogError($"Missing UI style sheet at Resources/{StyleResourcePath}.uss.");
            }

            var leaderStyleSheet = Resources.Load<StyleSheet>(LeaderStyleResourcePath);
            if (leaderStyleSheet != null)
            {
                root.styleSheets.Add(leaderStyleSheet);
            }
            else
            {
                Debug.LogError($"Missing UI style sheet at Resources/{LeaderStyleResourcePath}.uss.");
            }
        }

        private void LoadContent()
        {
            var catalog = Resources.Load<CountryCatalog>(CatalogResourcePath);
            if (catalog == null || catalog.Countries.Count == 0)
            {
                Debug.LogError($"Missing country catalog at Resources/{CatalogResourcePath}.asset.");
                countries = new List<CountryDefinition>();
                return;
            }

            countries = catalog.Countries;
        }

        private void BuildScreens()
        {
            var root = document.rootVisualElement;

            bootScreen = new BootScreen(ShowWorldMap);
            worldMapScreen = new WorldMapScreen(countries, OpenCountry);
            leaderScreen = new LeaderScreen(ShowWorldMap);

            root.Add(bootScreen);
            root.Add(worldMapScreen);
            root.Add(leaderScreen);
        }

        private void OpenCountry(CountryDefinition country)
        {
            leaderScreen.Bind(country);
            ShowOnly(leaderScreen);
        }

        private void ShowBoot()
        {
            ShowOnly(bootScreen);
        }

        private void ShowWorldMap()
        {
            worldMapScreen.ClearSelection();
            ShowOnly(worldMapScreen);
        }

        private void ShowOnly(VisualElement visibleScreen)
        {
            bootScreen.style.display = visibleScreen == bootScreen ? DisplayStyle.Flex : DisplayStyle.None;
            worldMapScreen.style.display = visibleScreen == worldMapScreen ? DisplayStyle.Flex : DisplayStyle.None;
            leaderScreen.style.display = visibleScreen == leaderScreen ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnDestroy()
        {
            if (panelSettings != null)
            {
                Destroy(panelSettings);
            }
        }
    }
}
