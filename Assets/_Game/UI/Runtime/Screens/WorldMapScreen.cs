using System;
using System.Collections.Generic;
using Statecraft.Data;
using Statecraft.UI.Components;
using UnityEngine.UIElements;

namespace Statecraft.UI.Screens
{
    public sealed class WorldMapScreen : VisualElement
    {
        private readonly Action<CountryDefinition> openCountry;
        private readonly Button continueButton;
        private readonly Label selectionLabel;
        private readonly Dictionary<CountryDefinition, Button> countryButtons = new();
        private CountryDefinition selectedCountry;

        public WorldMapScreen(IReadOnlyList<CountryDefinition> countries, Action<CountryDefinition> openCountry)
        {
            this.openCountry = openCountry;
            name = "world-map-screen";
            AddToClassList("screen");
            AddToClassList("map-screen");

            var header = UiFactory.Container("top-bar");
            var brand = UiFactory.Container("brand-block");
            brand.Add(UiFactory.Label("STATECRAFT", "brand-title"));
            brand.Add(UiFactory.Label("WORLD SITUATION ROOM", "eyebrow"));
            header.Add(brand);
            header.Add(UiFactory.Label("SESSION // 00:00:00", "session-label"));

            var body = UiFactory.Container("map-body");
            var mapPanel = BuildMapPlaceholder();
            var countryPanel = UiFactory.Container("country-panel");
            countryPanel.Add(UiFactory.Label("SÉLECTION NATIONALE", "eyebrow"));
            countryPanel.Add(UiFactory.Label("Choisissez votre mandat", "panel-title"));
            countryPanel.Add(UiFactory.Label(
                "Une même institution. Une identité propre à chaque nation.",
                "body-copy"));

            var countryList = UiFactory.Container("country-list");
            foreach (var country in countries)
            {
                countryList.Add(CreateCountryButton(country));
            }

            selectionLabel = UiFactory.Label("AUCUN PAYS SÉLECTIONNÉ", "selection-label");
            continueButton = UiFactory.Button("OUVRIR LE CABINET", OpenSelectedCountry, "primary-button");
            continueButton.SetEnabled(false);

            countryPanel.Add(countryList);
            countryPanel.Add(selectionLabel);
            countryPanel.Add(continueButton);

            body.Add(mapPanel);
            body.Add(countryPanel);

            Add(header);
            Add(body);
        }

        public void ClearSelection()
        {
            selectedCountry = null;
            selectionLabel.text = "AUCUN PAYS SÉLECTIONNÉ";
            continueButton.SetEnabled(false);

            foreach (var button in countryButtons.Values)
            {
                button.RemoveFromClassList("country-card--selected");
                button.style.borderTopColor = StyleKeyword.Null;
                button.style.borderRightColor = StyleKeyword.Null;
                button.style.borderBottomColor = StyleKeyword.Null;
                button.style.borderLeftColor = StyleKeyword.Null;
            }
        }

        private VisualElement BuildMapPlaceholder()
        {
            var panel = UiFactory.Container("map-panel");
            var mapHeader = UiFactory.Container("map-header");
            mapHeader.Add(UiFactory.Label("CARTE MONDIALE // APERÇU", "eyebrow"));
            mapHeader.Add(UiFactory.Label("15 AUG 2026", "map-date"));

            var canvas = UiFactory.Container("world-map-placeholder");
            canvas.Add(UiFactory.Container("map-grid"));
            canvas.Add(CreateLandmass("landmass landmass--americas"));
            canvas.Add(CreateLandmass("landmass landmass--eurasia"));
            canvas.Add(CreateLandmass("landmass landmass--africa"));
            canvas.Add(CreateLandmass("landmass landmass--oceania"));

            var legend = UiFactory.Container("map-legend");
            legend.Add(UiFactory.Container("legend-dot"));
            legend.Add(UiFactory.Label("NATIONS DISPONIBLES", "legend-text"));

            panel.Add(mapHeader);
            panel.Add(canvas);
            panel.Add(legend);
            return panel;
        }

        private static VisualElement CreateLandmass(string classes)
        {
            var element = new VisualElement();
            foreach (var className in classes.Split(' '))
            {
                element.AddToClassList(className);
            }

            return element;
        }

        private Button CreateCountryButton(CountryDefinition country)
        {
            var button = new Button(() => SelectCountry(country));
            button.AddToClassList("country-card");

            var monogram = UiFactory.Label(country.VisualIdentifier, "country-monogram");
            var content = UiFactory.Container("country-card-content");
            content.Add(UiFactory.Label(country.DisplayName, "country-name"));
            content.Add(UiFactory.Label(country.Capital.ToUpperInvariant(), "country-capital"));

            button.Add(monogram);
            button.Add(content);
            button.Add(UiFactory.Label("→", "country-arrow"));
            countryButtons.Add(country, button);
            return button;
        }

        private void SelectCountry(CountryDefinition country)
        {
            selectedCountry = country;
            selectionLabel.text = $"MANDAT : {country.DisplayName.ToUpperInvariant()}";
            continueButton.SetEnabled(true);

            foreach (var button in countryButtons.Values)
            {
                button.RemoveFromClassList("country-card--selected");
                button.style.borderTopColor = StyleKeyword.Null;
                button.style.borderRightColor = StyleKeyword.Null;
                button.style.borderBottomColor = StyleKeyword.Null;
                button.style.borderLeftColor = StyleKeyword.Null;
            }

            if (countryButtons.TryGetValue(country, out var selectedButton))
            {
                selectedButton.AddToClassList("country-card--selected");
                selectedButton.style.borderTopColor = country.Theme.AccentColor;
                selectedButton.style.borderRightColor = country.Theme.AccentColor;
                selectedButton.style.borderBottomColor = country.Theme.AccentColor;
                selectedButton.style.borderLeftColor = country.Theme.AccentColor;
            }

            continueButton.style.backgroundColor = country.Theme.AccentColor;
            continueButton.style.color = country.Theme.BackgroundColor;
        }

        private void OpenSelectedCountry()
        {
            if (selectedCountry != null)
            {
                openCountry(selectedCountry);
            }
        }
    }
}
