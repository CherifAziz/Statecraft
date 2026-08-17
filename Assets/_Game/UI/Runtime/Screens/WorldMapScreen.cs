using System;
using System.Collections.Generic;
using Statecraft.Data;
using Statecraft.Map.Data;
using Statecraft.Map.Rendering;
using Statecraft.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Screens
{
    public sealed class WorldMapScreen : VisualElement
    {
        private readonly Action<CountryDefinition> openCountry;
        private readonly Dictionary<string, CountryDefinition> availableCountries;
        private readonly WorldMapView mapView;
        private readonly Label hoverLabel;
        private readonly Label selectionName;
        private readonly Label selectionCapital;
        private readonly Label selectionLeader;
        private readonly Label selectionStatus;
        private readonly Button incarnateButton;
        private CountryDefinition selectedCountry;
        private WorldMapCountryData selectedMapCountry;

        public WorldMapScreen(
            WorldMapData mapData,
            IReadOnlyList<CountryDefinition> countries,
            Action<CountryDefinition> openCountry)
        {
            if (mapData == null)
            {
                throw new ArgumentNullException(nameof(mapData));
            }

            this.openCountry = openCountry;
            availableCountries = BuildCountryRegistry(mapData, countries);
            var presentations = BuildPresentations(availableCountries);

            name = "world-map-screen";
            AddToClassList("screen");
            AddToClassList("map-screen");

            var header = UiFactory.Container("world-map-header");
            var brand = UiFactory.Container("world-map-brand");
            brand.Add(UiFactory.Label("STATECRAFT", "world-map-brand-title"));
            brand.Add(UiFactory.Label("CARTE DU MONDE", "world-map-brand-subtitle"));
            header.Add(brand);
            header.Add(UiFactory.Label("GÉOMÉTRIE MONDIALE · NATURAL EARTH 1:50m", "world-map-source-label"));

            var body = UiFactory.Container("world-map-body");
            var mapPanel = UiFactory.Container("world-map-panel");
            var mapToolbar = UiFactory.Container("world-map-toolbar");
            var mapTitleBlock = UiFactory.Container("world-map-title-block");
            mapTitleBlock.Add(UiFactory.Label("THÉÂTRE MONDIAL", "world-map-eyebrow"));
            mapTitleBlock.Add(UiFactory.Label(
                $"{mapData.FeatureCount} TERRITOIRES · {mapData.PolygonCount} POLYGONES",
                "world-map-count"));
            hoverLabel = UiFactory.Label("SURVOL —", "world-map-hover-label");
            mapToolbar.Add(mapTitleBlock);
            mapToolbar.Add(hoverLabel);

            mapView = new WorldMapView(mapData, presentations)
            {
                DebugOverlayEnabled = false
            };
            mapView.HoveredCountryChanged += OnHoveredCountryChanged;
            mapView.CountryClicked += OnCountryClicked;

            var mapFooter = UiFactory.Container("world-map-footer");
            mapFooter.Add(CreateLegendItem("world-map-legend-swatch--available", "DISPONIBLE"));
            mapFooter.Add(CreateLegendItem("world-map-legend-swatch--unavailable", "INDISPONIBLE"));
            mapFooter.Add(CreateLegendItem("world-map-legend-swatch--selected", "SÉLECTION"));
            mapFooter.Add(UiFactory.Label(
                "MOLETTE : ZOOM  ·  GLISSER SUR L’OCÉAN / CLIC MILIEU : DÉPLACER",
                "world-map-controls"));

            mapPanel.Add(mapToolbar);
            mapPanel.Add(mapView);
            mapPanel.Add(mapFooter);

            var countryPanel = UiFactory.Container("world-map-country-panel");
            countryPanel.Add(UiFactory.Label("SÉLECTION", "world-map-eyebrow"));
            selectionName = UiFactory.Label("Aucun pays sélectionné", "world-map-selection-name");
            countryPanel.Add(selectionName);
            countryPanel.Add(UiFactory.Container("world-map-selection-rule"));

            var selectionDetails = UiFactory.Container("world-map-selection-details");
            selectionCapital = CreateSelectionLine(selectionDetails, "CAPITALE");
            selectionLeader = CreateSelectionLine(selectionDetails, "DIRIGEANT");
            selectionStatus = CreateSelectionLine(selectionDetails, "STATUT");
            countryPanel.Add(selectionDetails);

            incarnateButton = UiFactory.Button("INCARNER", OpenSelectedCountry, "world-map-incarnate-button");
            incarnateButton.style.display = DisplayStyle.None;
            countryPanel.Add(incarnateButton);

            body.Add(mapPanel);
            body.Add(countryPanel);
            Add(header);
            Add(body);
        }

        public WorldMapView MapView => mapView;

        public void ClearSelection()
        {
            selectedCountry = null;
            selectedMapCountry = null;
            mapView.SetSelectedCountry(null);
            selectionName.text = "Aucun pays sélectionné";
            SetSelectionLine(selectionCapital, string.Empty, false);
            SetSelectionLine(selectionLeader, string.Empty, false);
            SetSelectionLine(selectionStatus, string.Empty, false);
            incarnateButton.SetEnabled(false);
            incarnateButton.style.display = DisplayStyle.None;
            incarnateButton.style.backgroundColor = StyleKeyword.Null;
            incarnateButton.style.color = StyleKeyword.Null;
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

        private static VisualElement CreateLegendItem(string swatchClass, string text)
        {
            var item = UiFactory.Container("world-map-legend-item");
            var swatch = UiFactory.Container("world-map-legend-swatch");
            swatch.AddToClassList(swatchClass);
            item.Add(swatch);
            item.Add(UiFactory.Label(text, "world-map-legend-text"));
            return item;
        }

        private static Label CreateSelectionLine(VisualElement parent, string label)
        {
            var row = UiFactory.Container("world-map-selection-row");
            row.Add(UiFactory.Label(label, "world-map-selection-key"));
            var value = UiFactory.Label(string.Empty, "world-map-selection-value");
            row.Add(value);
            row.style.display = DisplayStyle.None;
            parent.Add(row);
            return value;
        }

        private void OnHoveredCountryChanged(WorldMapCountryData country)
        {
            hoverLabel.text = country == null
                ? "SURVOL —"
                : $"SURVOL  {country.DisplayName.ToUpperInvariant()}  ·  {country.GeographicId}";
        }

        private void OnCountryClicked(WorldMapCountryData mapCountry)
        {
            selectedMapCountry = mapCountry;
            mapView.SetSelectedCountry(mapCountry);
            selectionName.text = mapCountry.DisplayName.ToUpperInvariant();

            if (availableCountries.TryGetValue(mapCountry.GeographicId, out selectedCountry))
            {
                SetSelectionLine(selectionCapital, selectedCountry.Capital, true);
                SetSelectionLine(selectionLeader, selectedCountry.Leader.DisplayName, true);
                SetSelectionLine(selectionStatus, "DISPONIBLE", true);
                selectionStatus.style.color = selectedCountry.Theme.AccentColor;
                incarnateButton.style.display = DisplayStyle.Flex;
                incarnateButton.SetEnabled(true);
                incarnateButton.style.backgroundColor = selectedCountry.Theme.AccentColor;
                incarnateButton.style.color = selectedCountry.Theme.BackgroundColor;
                return;
            }

            selectedCountry = null;
            SetSelectionLine(selectionCapital, string.Empty, false);
            SetSelectionLine(selectionLeader, string.Empty, false);
            SetSelectionLine(selectionStatus, "INDISPONIBLE", true);
            selectionStatus.style.color = StyleKeyword.Null;
            incarnateButton.SetEnabled(false);
            incarnateButton.style.display = DisplayStyle.None;
        }

        private static void SetSelectionLine(Label valueLabel, string value, bool visible)
        {
            valueLabel.text = value;
            valueLabel.parent.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void OpenSelectedCountry()
        {
            if (selectedCountry != null && selectedMapCountry != null)
            {
                openCountry(selectedCountry);
            }
        }
    }
}
