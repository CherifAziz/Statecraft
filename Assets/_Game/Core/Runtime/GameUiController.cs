using System.Collections.Generic;
using Statecraft.Data;
using Statecraft.Map.Data;
using Statecraft.Simulation;
using Statecraft.UI.Screens;
using Statecraft.UI.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.Core
{
    public sealed class GameUiController : MonoBehaviour
    {
        private const string CatalogResourcePath = "GameData/CountryCatalog";
        private const string StyleResourcePath = "UI/Statecraft";
        private const string LeaderStyleResourcePath = "UI/LeaderScreen";
        private const string DashboardStyleResourcePath = "UI/CountryDashboard";
        private const string RuntimeThemeResourcePath = "UI/StatecraftTheme";
        private const string TypographyResourcePath = "UI/StatecraftTypography";
        private const string PanelSettingsResourcePath = "UI/StatecraftPanelSettings";

        private PanelSettings panelSettings;
        private UIDocument document;
        private BootScreen bootScreen;
        private WorldMapScreen worldMapScreen;
        private LeaderScreen leaderScreen;
        private CountryDashboardScreen dashboardScreen;
        private StatecraftTypography typography;
        private IReadOnlyList<CountryDefinition> countries;
        private WorldMapData worldMapData;
        private GameRuntime gameRuntime;
        private bool mapConsultationDuringMandate;

        public GameRuntime Runtime => gameRuntime;

        private void Start()
        {
            ConfigureDocument();
            LoadContent();
            BuildScreens();
            ShowBoot();
        }

        private void ConfigureDocument()
        {
            var panelSettingsTemplate = Resources.Load<PanelSettings>(PanelSettingsResourcePath);
            if (panelSettingsTemplate == null)
            {
                Debug.LogError($"Missing panel settings at Resources/{PanelSettingsResourcePath}.asset.");
                return;
            }

            panelSettings = Instantiate(panelSettingsTemplate);
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
                root.EnableInClassList("short-desktop", evt.newRect.height < 1000f);
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

            var dashboardStyleSheet = Resources.Load<StyleSheet>(DashboardStyleResourcePath);
            if (dashboardStyleSheet != null)
            {
                root.styleSheets.Add(dashboardStyleSheet);
            }
            else
            {
                Debug.LogError($"Missing UI style sheet at Resources/{DashboardStyleResourcePath}.uss.");
            }
        }

        private void LoadContent()
        {
            typography = Resources.Load<StatecraftTypography>(TypographyResourcePath);
            if (typography == null)
            {
                Debug.LogError($"Missing typography settings at Resources/{TypographyResourcePath}.asset.");
            }

            var catalog = Resources.Load<CountryCatalog>(CatalogResourcePath);
            if (catalog == null || catalog.Countries.Count == 0)
            {
                Debug.LogError($"Missing country catalog at Resources/{CatalogResourcePath}.asset.");
                countries = new List<CountryDefinition>();
                gameRuntime = new GameRuntime(System.Array.Empty<ISimulationCountryDefinition>());
                return;
            }

            countries = catalog.Countries;
            gameRuntime = new GameRuntime(countries);

            try
            {
                worldMapData = WorldMapData.LoadFromResources();
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"Unable to load world map data: {exception.Message}");
            }
        }

        private void BuildScreens()
        {
            var root = document.rootVisualElement;

            bootScreen = new BootScreen(ShowWorldMapFromBoot);
            worldMapScreen = new WorldMapScreen(worldMapData, countries, OpenCountry, typography);
            leaderScreen = new LeaderScreen(ShowWorldMap, StartMandate, ShowDashboard, typography);
            dashboardScreen = new CountryDashboardScreen(ShowWorldMapFromDashboard, ShowPlayerLeader, typography);

            root.Add(bootScreen);
            root.Add(worldMapScreen);
            root.Add(leaderScreen);
            root.Add(dashboardScreen);
        }

        private void OpenCountry(CountryDefinition country)
        {
            var mode = mapConsultationDuringMandate && gameRuntime.HasActiveSession &&
                string.Equals(gameRuntime.CurrentSession.PlayerCountryId, country.Id, System.StringComparison.Ordinal)
                    ? LeaderScreenMode.ActiveSession
                    : LeaderScreenMode.PreGame;
            leaderScreen.Bind(country, mode);
            ShowOnly(leaderScreen);
        }

        private void StartMandate(CountryDefinition country)
        {
            if (country == null)
            {
                return;
            }

            var session = gameRuntime.StartNewGame(country);
            mapConsultationDuringMandate = false;
            if (dashboardScreen.Bind(session, country))
            {
                ShowOnly(dashboardScreen);
            }
        }

        private void ShowDashboard()
        {
            if (!gameRuntime.HasActiveSession)
            {
                ShowWorldMap();
                return;
            }

            var playerCountry = FindCountry(gameRuntime.CurrentSession.PlayerCountryId);
            if (playerCountry == null || !dashboardScreen.Bind(gameRuntime.CurrentSession, playerCountry))
            {
                ShowWorldMap();
                return;
            }

            ShowOnly(dashboardScreen);
        }

        private void ShowPlayerLeader()
        {
            if (!gameRuntime.HasActiveSession)
            {
                ShowWorldMap();
                return;
            }

            var playerCountry = FindCountry(gameRuntime.CurrentSession.PlayerCountryId);
            if (playerCountry == null)
            {
                ShowWorldMap();
                return;
            }

            mapConsultationDuringMandate = true;
            leaderScreen.Bind(playerCountry, LeaderScreenMode.ActiveSession);
            ShowOnly(leaderScreen);
        }

        private CountryDefinition FindCountry(string countryId)
        {
            foreach (var country in countries)
            {
                if (country != null && string.Equals(country.Id, countryId, System.StringComparison.Ordinal))
                {
                    return country;
                }
            }

            return null;
        }

        private void ShowBoot()
        {
            ShowOnly(bootScreen);
        }

        private void ShowWorldMapFromBoot()
        {
            mapConsultationDuringMandate = false;
            ShowWorldMap();
        }

        private void ShowWorldMapFromDashboard()
        {
            mapConsultationDuringMandate = true;
            ShowWorldMap();
        }

        private void ShowWorldMap()
        {
            ShowOnly(worldMapScreen);
        }

        private void ShowOnly(VisualElement visibleScreen)
        {
            bootScreen.style.display = visibleScreen == bootScreen ? DisplayStyle.Flex : DisplayStyle.None;
            worldMapScreen.style.display = visibleScreen == worldMapScreen ? DisplayStyle.Flex : DisplayStyle.None;
            leaderScreen.style.display = visibleScreen == leaderScreen ? DisplayStyle.Flex : DisplayStyle.None;
            dashboardScreen.style.display = visibleScreen == dashboardScreen ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OnDestroy()
        {
            dashboardScreen?.Unbind();
            if (panelSettings != null)
            {
                Destroy(panelSettings);
            }
        }
    }
}
