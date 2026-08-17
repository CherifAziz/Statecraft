using System;
using System.Collections.Generic;
using Statecraft.Data;
using Statecraft.Map.Data;
using Statecraft.Map.Rendering;
using Statecraft.UI.Components;
using Statecraft.UI.Formatting;
using Statecraft.UI.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Screens
{
    public sealed class WorldMapScreen : VisualElement
    {
        private const string DossierVisibleClass = "world-map-dossier-content--visible";
        private const string HoverVisibleClass = "world-map-hover-readout--visible";
        private readonly Action<CountryDefinition> openCountry;
        private readonly StatecraftTypography typography;
        private readonly Dictionary<string, CountryDefinition> availableCountries;
        private readonly WorldMapView mapView;
        private readonly VisualElement hoverReadout;
        private readonly VisualElement hoverAccent;
        private readonly Label hoverName;
        private readonly Label hoverAvailability;
        private readonly VisualElement countryDossier;
        private readonly VisualElement dossierTint;
        private readonly VisualElement dossierEmblem;
        private readonly VisualElement dossierContent;
        private readonly Label dossierEyebrow;
        private readonly Label selectionName;
        private readonly VisualElement dossierRuleLine;
        private readonly VisualElement dossierRuleDiamond;
        private readonly VisualElement emptyState;
        private readonly VisualElement availableState;
        private readonly VisualElement unavailableState;
        private readonly Label selectionCapital;
        private readonly Label selectionPopulation;
        private readonly Label selectionGdp;
        private readonly Label selectionLeader;
        private readonly Label selectionFunction;
        private readonly Label selectionStatus;
        private readonly Label unavailableStatus;
        private readonly Button incarnateButton;
        private CountryDefinition selectedCountry;
        private WorldMapCountryData selectedMapCountry;
        private int dossierRevealVersion;

        public WorldMapScreen(
            WorldMapData mapData,
            IReadOnlyList<CountryDefinition> countries,
            Action<CountryDefinition> openCountry,
            StatecraftTypography typography)
        {
            if (mapData == null)
            {
                throw new ArgumentNullException(nameof(mapData));
            }

            this.openCountry = openCountry;
            this.typography = typography;
            availableCountries = BuildCountryRegistry(mapData, countries);
            var presentations = BuildPresentations(availableCountries);

            name = "world-map-screen";
            AddToClassList("screen");
            AddToClassList("map-screen");

            var header = UiFactory.Container("world-map-header");
            var titleBlock = UiFactory.Container("world-map-header-title-block");
            titleBlock.Add(UtilitySemibold(UiFactory.Label("STATECRAFT", "world-map-brand-kicker")));
            titleBlock.Add(PrestigeSemibold(UiFactory.Label("CARTE DU MONDE", "world-map-title")));

            var headerContext = UiFactory.Container("world-map-header-context");
            headerContext.Add(UiFactory.Container("world-map-header-context-mark"));
            headerContext.Add(UtilityMedium(UiFactory.Label("VUE STRATÉGIQUE", "world-map-header-context-label")));
            header.Add(titleBlock);
            header.Add(headerContext);

            var body = UiFactory.Container("world-map-body");
            var mapStage = UiFactory.Container("world-map-stage");
            mapView = new WorldMapView(mapData, presentations)
            {
                DebugOverlayEnabled = false
            };
            mapView.HoveredCountryChanged += OnHoveredCountryChanged;
            mapView.CountryClicked += OnCountryClicked;
            mapStage.Add(mapView);

            mapStage.Add(CreateCorner("world-map-corner--top-left"));
            mapStage.Add(CreateCorner("world-map-corner--top-right"));
            mapStage.Add(CreateCorner("world-map-corner--bottom-left"));
            mapStage.Add(CreateCorner("world-map-corner--bottom-right"));

            hoverReadout = UiFactory.Container("world-map-hover-readout");
            hoverAccent = UiFactory.Container("world-map-hover-accent");
            var hoverCopy = UiFactory.Container("world-map-hover-copy");
            hoverName = PrestigeMedium(UiFactory.Label(string.Empty, "world-map-hover-name"));
            hoverAvailability = UtilityMedium(UiFactory.Label(string.Empty, "world-map-hover-status"));
            hoverCopy.Add(hoverName);
            hoverCopy.Add(hoverAvailability);
            hoverReadout.Add(hoverAccent);
            hoverReadout.Add(hoverCopy);
            mapStage.Add(hoverReadout);

            mapStage.Add(UtilityRegular(UiFactory.Label(
                "MOLETTE  ZOOM     ·     GLISSER SUR L’OCÉAN  DÉPLACER",
                "world-map-controls")));

            countryDossier = UiFactory.Container("world-map-country-dossier");
            dossierTint = UiFactory.Container("world-map-dossier-tint");
            dossierTint.pickingMode = PickingMode.Ignore;
            dossierEmblem = UiFactory.Container("world-map-dossier-emblem");
            dossierEmblem.pickingMode = PickingMode.Ignore;
            dossierContent = UiFactory.Container("world-map-dossier-content");

            dossierEyebrow = UtilitySemibold(UiFactory.Label("SÉLECTION", "world-map-dossier-eyebrow"));
            selectionName = PrestigeSemibold(UiFactory.Label(string.Empty, "world-map-selection-name"));
            var dossierRule = UiFactory.Container("world-map-dossier-rule");
            dossierRuleLine = UiFactory.Container("world-map-dossier-rule-line");
            dossierRuleDiamond = UiFactory.Container("world-map-dossier-rule-diamond");
            dossierRule.Add(dossierRuleLine);
            dossierRule.Add(dossierRuleDiamond);

            emptyState = UiFactory.Container("world-map-empty-state");
            emptyState.Add(UtilityRegular(UiFactory.Label(
                "Sélectionnez un État pour consulter son profil.",
                "world-map-empty-instruction")));

            availableState = UiFactory.Container("world-map-available-state");
            var metrics = UiFactory.Container("world-map-metrics");
            selectionCapital = CreateMetric(metrics, "CAPITALE");
            selectionPopulation = CreateMetric(metrics, "POPULATION");
            selectionGdp = CreateMetric(metrics, "PIB");
            availableState.Add(metrics);
            availableState.Add(UiFactory.Container("world-map-dossier-hairline"));

            var leadership = UiFactory.Container("world-map-leadership");
            selectionLeader = CreateField(leadership, "DIRIGEANT");
            selectionFunction = CreateField(leadership, "FONCTION");
            availableState.Add(leadership);
            selectionStatus = CreateField(availableState, "STATUT", "world-map-status-value");

            unavailableState = UiFactory.Container("world-map-unavailable-state");
            unavailableStatus = CreateField(unavailableState, "STATUT", "world-map-status-value");
            unavailableState.Add(PrestigeMedium(UiFactory.Label("Contenu à venir", "world-map-coming-soon")));
            unavailableState.Add(UtilityRegular(UiFactory.Label(
                "Cet État n’est pas encore accessible dans cette version de Statecraft.",
                "world-map-coming-soon-copy")));

            incarnateButton = UiFactory.Button(
                "INCARNER LE DIRIGEANT   →",
                OpenSelectedCountry,
                "world-map-incarnate-button");
            if (typography != null && typography.UtilitySemibold != null)
            {
                incarnateButton.style.unityFont = new StyleFont(typography.UtilitySemibold);
            }

            dossierContent.Add(dossierEyebrow);
            dossierContent.Add(selectionName);
            dossierContent.Add(dossierRule);
            dossierContent.Add(emptyState);
            dossierContent.Add(availableState);
            dossierContent.Add(unavailableState);
            dossierContent.Add(incarnateButton);
            countryDossier.Add(dossierTint);
            countryDossier.Add(dossierEmblem);
            countryDossier.Add(dossierContent);

            body.Add(mapStage);
            body.Add(countryDossier);
            Add(header);
            Add(body);

            BindEmptyDossier();
            dossierContent.schedule.Execute(() => dossierContent.AddToClassList(DossierVisibleClass)).StartingIn(60);
        }

        public WorldMapView MapView => mapView;

        public void ClearSelection()
        {
            selectedCountry = null;
            selectedMapCountry = null;
            mapView.SetSelectedCountry(null);
            RevealDossier(BindEmptyDossier);
        }

        private static Dictionary<string, CountryDefinition> BuildCountryRegistry(
            WorldMapData mapData,
            IReadOnlyList<CountryDefinition> countries)
        {
            var registry = new Dictionary<string, CountryDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (var country in countries)
            {
                if (string.IsNullOrWhiteSpace(country.MapGeographicId))
                {
                    continue;
                }

                if (!mapData.TryGetCountry(country.MapGeographicId, out _))
                {
                    throw new InvalidOperationException(
                        $"Country '{country.Id}' references unknown map ID '{country.MapGeographicId}'.");
                }

                registry.Add(country.MapGeographicId, country);
            }

            return registry;
        }

        private static Dictionary<string, WorldMapCountryPresentation> BuildPresentations(
            IReadOnlyDictionary<string, CountryDefinition> availableCountries)
        {
            var presentations = new Dictionary<string, WorldMapCountryPresentation>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in availableCountries)
            {
                presentations.Add(pair.Key, new WorldMapCountryPresentation(true, pair.Value.Theme.AccentColor));
            }

            return presentations;
        }

        private Label CreateMetric(VisualElement parent, string label)
        {
            var metric = UiFactory.Container("world-map-metric");
            metric.Add(UtilityMedium(UiFactory.Label(label, "world-map-metric-key")));
            var value = UtilityRegular(UiFactory.Label(string.Empty, "world-map-metric-value"));
            metric.Add(value);
            parent.Add(metric);
            return value;
        }

        private Label CreateField(VisualElement parent, string label, string valueClass = "world-map-field-value")
        {
            var field = UiFactory.Container("world-map-field");
            field.Add(UtilityMedium(UiFactory.Label(label, "world-map-field-key")));
            var value = UtilityRegular(UiFactory.Label(string.Empty, valueClass));
            field.Add(value);
            parent.Add(field);
            return value;
        }

        private static VisualElement CreateCorner(string modifierClass)
        {
            var corner = UiFactory.Container("world-map-corner");
            corner.AddToClassList(modifierClass);
            corner.pickingMode = PickingMode.Ignore;
            return corner;
        }

        private void OnHoveredCountryChanged(WorldMapCountryData country)
        {
            if (country == null)
            {
                hoverReadout.RemoveFromClassList(HoverVisibleClass);
                return;
            }

            hoverName.text = country.DisplayName.ToUpperInvariant();
            if (availableCountries.TryGetValue(country.GeographicId, out var availableCountry))
            {
                hoverAvailability.text = "ÉTAT DISPONIBLE";
                hoverAvailability.style.color = availableCountry.Theme.AccentColor;
                hoverAccent.style.backgroundColor = availableCountry.Theme.AccentColor;
            }
            else
            {
                hoverAvailability.text = "INDISPONIBLE";
                hoverAvailability.style.color = StyleKeyword.Null;
                hoverAccent.style.backgroundColor = StyleKeyword.Null;
            }

            hoverReadout.AddToClassList(HoverVisibleClass);
        }

        private void OnCountryClicked(WorldMapCountryData mapCountry)
        {
            selectedMapCountry = mapCountry;
            mapView.SetSelectedCountry(mapCountry);

            if (availableCountries.TryGetValue(mapCountry.GeographicId, out selectedCountry))
            {
                mapView.FocusCountry(mapCountry);
                RevealDossier(() => BindAvailableDossier(selectedCountry));
                return;
            }

            selectedCountry = null;
            RevealDossier(() => BindUnavailableDossier(mapCountry));
        }

        private void RevealDossier(Action bind)
        {
            var revealVersion = ++dossierRevealVersion;
            dossierContent.RemoveFromClassList(DossierVisibleClass);
            dossierContent.schedule.Execute(() =>
            {
                if (revealVersion != dossierRevealVersion)
                {
                    return;
                }

                bind();
                dossierContent.AddToClassList(DossierVisibleClass);
            }).StartingIn(55);
        }

        private void BindEmptyDossier()
        {
            dossierEyebrow.text = "SÉLECTION";
            selectionName.text = "Le monde attend\nvotre mandat.";
            emptyState.style.display = DisplayStyle.Flex;
            availableState.style.display = DisplayStyle.None;
            unavailableState.style.display = DisplayStyle.None;
            incarnateButton.style.display = DisplayStyle.None;
            ApplyNeutralDossierStyle();
        }

        private void BindAvailableDossier(CountryDefinition country)
        {
            dossierEyebrow.text = "ÉTAT SÉLECTIONNÉ";
            selectionName.text = country.DisplayName.ToUpperInvariant();
            selectionCapital.text = country.Capital;
            selectionPopulation.text = StatecraftValueFormatter.FormatPopulation(country.Population);
            selectionGdp.text = StatecraftValueFormatter.FormatUsd(country.GdpUsd);
            selectionLeader.text = country.Leader != null ? country.Leader.DisplayName : "—";
            selectionFunction.text = country.Leader != null ? country.Leader.Title : "—";
            selectionStatus.text = "DISPONIBLE";
            emptyState.style.display = DisplayStyle.None;
            availableState.style.display = DisplayStyle.Flex;
            unavailableState.style.display = DisplayStyle.None;
            incarnateButton.style.display = DisplayStyle.Flex;
            incarnateButton.SetEnabled(true);
            ApplyCountryDossierStyle(country.Theme);
        }

        private void BindUnavailableDossier(WorldMapCountryData country)
        {
            dossierEyebrow.text = "ÉTAT SÉLECTIONNÉ";
            selectionName.text = country.DisplayName.ToUpperInvariant();
            unavailableStatus.text = "INDISPONIBLE";
            emptyState.style.display = DisplayStyle.None;
            availableState.style.display = DisplayStyle.None;
            unavailableState.style.display = DisplayStyle.Flex;
            incarnateButton.style.display = DisplayStyle.None;
            incarnateButton.SetEnabled(false);
            ApplyNeutralDossierStyle();
        }

        private void ApplyCountryDossierStyle(CountryTheme theme)
        {
            dossierTint.style.backgroundColor = theme.SurfaceColor;
            dossierTint.style.opacity = 0.22f;
            dossierEyebrow.style.color = theme.AccentColor;
            dossierRuleLine.style.backgroundColor = theme.AccentColor;
            dossierRuleDiamond.style.backgroundColor = theme.AccentColor;
            selectionStatus.style.color = theme.AccentColor;
            countryDossier.style.borderLeftColor = WithAlpha(theme.BorderColor, 0.68f);
            incarnateButton.style.backgroundColor = theme.AccentColor;
            incarnateButton.style.color = theme.BackgroundColor;
            incarnateButton.style.borderLeftColor = theme.AccentColor;
            incarnateButton.style.borderRightColor = theme.AccentColor;
            incarnateButton.style.borderTopColor = theme.AccentColor;
            incarnateButton.style.borderBottomColor = theme.AccentColor;

            if (theme.Emblem != null)
            {
                dossierEmblem.style.backgroundImage = new StyleBackground(theme.Emblem);
                dossierEmblem.style.unityBackgroundImageTintColor = theme.OrnamentColor;
                dossierEmblem.style.display = DisplayStyle.Flex;
            }
            else
            {
                dossierEmblem.style.display = DisplayStyle.None;
            }
        }

        private void ApplyNeutralDossierStyle()
        {
            dossierTint.style.opacity = 0f;
            dossierEmblem.style.display = DisplayStyle.None;
            dossierEyebrow.style.color = StyleKeyword.Null;
            dossierRuleLine.style.backgroundColor = StyleKeyword.Null;
            dossierRuleDiamond.style.backgroundColor = StyleKeyword.Null;
            selectionStatus.style.color = StyleKeyword.Null;
            countryDossier.style.borderLeftColor = StyleKeyword.Null;
            incarnateButton.style.backgroundColor = StyleKeyword.Null;
            incarnateButton.style.color = StyleKeyword.Null;
        }

        private void OpenSelectedCountry()
        {
            if (selectedCountry != null && selectedMapCountry != null)
            {
                openCountry(selectedCountry);
            }
        }

        private Label PrestigeMedium(Label label)
        {
            return ApplyFont(label, typography != null ? typography.PrestigeMedium : null);
        }

        private Label PrestigeSemibold(Label label)
        {
            return ApplyFont(label, typography != null ? typography.PrestigeSemibold : null);
        }

        private Label UtilityRegular(Label label)
        {
            return ApplyFont(label, typography != null ? typography.UtilityRegular : null);
        }

        private Label UtilityMedium(Label label)
        {
            return ApplyFont(label, typography != null ? typography.UtilityMedium : null);
        }

        private Label UtilitySemibold(Label label)
        {
            return ApplyFont(label, typography != null ? typography.UtilitySemibold : null);
        }

        private static Label ApplyFont(Label label, Font font)
        {
            if (font != null)
            {
                label.style.unityFont = new StyleFont(font);
            }

            return label;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
