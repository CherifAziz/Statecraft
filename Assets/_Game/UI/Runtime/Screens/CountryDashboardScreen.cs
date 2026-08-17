using System;
using System.Collections.Generic;
using Statecraft.Data;
using Statecraft.Simulation;
using Statecraft.UI.Components;
using Statecraft.UI.Formatting;
using Statecraft.UI.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Screens
{
    public sealed class CountryDashboardScreen : VisualElement
    {
        private readonly StatecraftTypography typography;
        private readonly VisualElement surfaceTexture;
        private readonly VisualElement emblem;
        private readonly Label emblemFallback;
        private readonly Label countryName;
        private readonly Label mandateName;
        private readonly Label dateValue;
        private readonly Label populationValue;
        private readonly Label gdpValue;
        private readonly Label treasuryValue;
        private readonly Label approvalValue;
        private readonly Label stabilityValue;
        private readonly Label politicalCapitalValue;
        private readonly VisualElement approvalFill;
        private readonly VisualElement stabilityFill;
        private readonly VisualElement politicalCapitalFill;
        private readonly VisualElement headerSignal;
        private readonly VisualElement sectionRule;
        private readonly VisualElement briefingRule;
        private readonly VisualElement briefingMarker;
        private readonly VisualElement statusDot;
        private readonly VisualElement briefingSurface;
        private readonly VisualElement navigationRule;
        private readonly Label navigationActive;
        private readonly Button worldMapButton;
        private readonly Button leaderButton;
        private readonly List<Label> primaryLabels = new();
        private readonly List<Label> secondaryLabels = new();
        private readonly List<Label> accentLabels = new();
        private readonly List<Label> prestigeLabels = new();
        private readonly List<VisualElement> themedBorders = new();
        private readonly List<VisualElement> trackElements = new();
        private readonly List<VisualElement> fillElements = new();
        private GameSession session;
        private CountryState state;

        public CountryDashboardScreen(
            Action showWorldMap,
            Action showLeader,
            StatecraftTypography typography)
        {
            this.typography = typography;
            name = "country-dashboard-screen";
            AddToClassList("screen");
            AddToClassList("country-dashboard");

            surfaceTexture = Layer("dashboard-surface-texture");
            var ambientField = Layer("dashboard-ambient-field");
            var topLine = Layer("dashboard-top-line");

            var header = UiFactory.Container("dashboard-header");
            var identity = UiFactory.Container("dashboard-identity");
            emblem = UiFactory.Container("dashboard-emblem");
            emblemFallback = UtilityMedium(Secondary(UiFactory.Label("—", "dashboard-emblem-fallback")));
            emblem.Add(emblemFallback);

            headerSignal = Layer("dashboard-header-signal");
            var identityCopy = UiFactory.Container("dashboard-identity-copy");
            identityCopy.Add(UtilitySemibold(Accent(UiFactory.Label("PRÉSIDENCE", "dashboard-eyebrow"))));
            countryName = Prestige(Primary(UiFactory.Label("PAYS", "dashboard-country-name")));
            mandateName = UtilityRegular(Secondary(UiFactory.Label("Mandat du dirigeant", "dashboard-mandate-name")));
            identityCopy.Add(countryName);
            identityCopy.Add(mandateName);
            identity.Add(emblem);
            identity.Add(headerSignal);
            identity.Add(identityCopy);

            var dateBlock = UiFactory.Container("dashboard-date-block");
            dateBlock.Add(UtilitySemibold(Accent(UiFactory.Label("DATE DU MANDAT", "dashboard-date-label"))));
            dateValue = UtilityMedium(Primary(UiFactory.Label("—", "dashboard-date-value")));
            dateValue.name = "dashboard-date-value";
            dateBlock.Add(dateValue);
            header.Add(identity);
            header.Add(dateBlock);

            var main = UiFactory.Container("dashboard-main");
            var indicators = UiFactory.Container("dashboard-indicators");
            var sectionHeading = UiFactory.Container("dashboard-section-heading");
            var sectionCopy = UiFactory.Container("dashboard-section-copy");
            sectionCopy.Add(UtilitySemibold(Accent(UiFactory.Label("INDICATEURS D'ÉTAT", "dashboard-section-eyebrow"))));
            sectionCopy.Add(Prestige(Primary(UiFactory.Label("Situation nationale", "dashboard-section-title"))));
            sectionHeading.Add(sectionCopy);
            sectionRule = Layer("dashboard-section-rule");
            sectionHeading.Add(sectionRule);
            indicators.Add(sectionHeading);

            var macroRow = UiFactory.Container("dashboard-macro-row");
            var populationIndicator = CreateMacroIndicator("POPULATION", "DÉMOGRAPHIE NATIONALE", out populationValue, "dashboard-population-value");
            populationIndicator.AddToClassList("dashboard-macro-indicator--first");
            macroRow.Add(populationIndicator);
            macroRow.Add(CreateMacroIndicator("PIB", "PRODUCTION ANNUELLE", out gdpValue, "dashboard-gdp-value"));
            macroRow.Add(CreateMacroIndicator("TRÉSORERIE", "RÉSERVES DE L'ÉTAT", out treasuryValue, "dashboard-treasury-value"));
            indicators.Add(macroRow);

            var politicalHeading = UiFactory.Container("dashboard-political-heading");
            politicalHeading.Add(UtilitySemibold(Secondary(UiFactory.Label("ÉQUILIBRES DU MANDAT", "dashboard-political-heading-label"))));
            var politicalHeadingLine = Layer("dashboard-political-heading-line");
            themedBorders.Add(politicalHeadingLine);
            politicalHeading.Add(politicalHeadingLine);
            indicators.Add(politicalHeading);

            var politicalRow = UiFactory.Container("dashboard-political-row");
            politicalRow.Add(CreatePoliticalIndicator("APPROBATION", out approvalValue, out approvalFill, "dashboard-approval-value"));
            politicalRow.Add(CreatePoliticalIndicator("STABILITÉ", out stabilityValue, out stabilityFill, "dashboard-stability-value"));
            politicalRow.Add(CreatePoliticalIndicator("CAPITAL POLITIQUE", out politicalCapitalValue, out politicalCapitalFill, "dashboard-political-capital-value"));
            indicators.Add(politicalRow);

            briefingSurface = UiFactory.Container("dashboard-briefing");
            var briefingHeader = UiFactory.Container("dashboard-briefing-header");
            briefingHeader.Add(UtilitySemibold(Accent(UiFactory.Label("ÉTAT DU PAYS", "dashboard-briefing-eyebrow"))));
            briefingMarker = Layer("dashboard-briefing-marker");
            briefingHeader.Add(briefingMarker);
            briefingSurface.Add(briefingHeader);
            briefingSurface.Add(Prestige(Primary(UiFactory.Label("Votre mandat\ndébute.", "dashboard-briefing-title"))));
            briefingRule = Layer("dashboard-briefing-rule");
            briefingSurface.Add(briefingRule);
            briefingSurface.Add(UtilityRegular(Secondary(UiFactory.Label(
                "Les institutions sont en place.\nAucune crise active.",
                "dashboard-briefing-copy"))));
            var briefingFooter = UiFactory.Container("dashboard-briefing-footer");
            statusDot = Layer("dashboard-status-dot");
            briefingFooter.Add(statusDot);
            briefingFooter.Add(UtilitySemibold(Secondary(UiFactory.Label("SESSION ACTIVE", "dashboard-session-status"))));
            briefingSurface.Add(briefingFooter);
            themedBorders.Add(briefingSurface);

            main.Add(indicators);
            main.Add(briefingSurface);

            var navigation = UiFactory.Container("dashboard-navigation");
            navigationRule = Layer("dashboard-navigation-rule");
            navigation.Add(navigationRule);
            var navigationContent = UiFactory.Container("dashboard-navigation-content");
            var navigationBrand = UiFactory.Container("dashboard-navigation-brand");
            navigationBrand.Add(Prestige(Primary(UiFactory.Label("STATECRAFT", "dashboard-brand"))));
            navigationBrand.Add(UtilityRegular(Secondary(UiFactory.Label("CABINET PRÉSIDENTIEL", "dashboard-brand-meta"))));
            navigationContent.Add(navigationBrand);
            var navigationLinks = UiFactory.Container("dashboard-navigation-links");
            navigationActive = UtilitySemibold(Accent(UiFactory.Label("PAYS", "dashboard-navigation-active")));
            leaderButton = UiFactory.Button("DIRIGEANT", showLeader, "dashboard-navigation-button");
            worldMapButton = UiFactory.Button("CARTE DU MONDE", showWorldMap, "dashboard-navigation-button");
            ApplyButtonFont(leaderButton);
            ApplyButtonFont(worldMapButton);
            navigationLinks.Add(navigationActive);
            navigationLinks.Add(leaderButton);
            navigationLinks.Add(worldMapButton);
            navigationContent.Add(navigationLinks);
            navigation.Add(navigationContent);

            Add(ambientField);
            Add(surfaceTexture);
            Add(topLine);
            Add(header);
            Add(main);
            Add(navigation);
            SetUnboundValues();
        }

        public GameSession BoundSession => session;
        public CountryState BoundCountryState => state;

        public bool Bind(GameSession gameSession, CountryDefinition country)
        {
            Unbind();
            if (gameSession == null || !gameSession.IsActive || country == null || country.Theme == null ||
                gameSession.PlayerCountryState == null ||
                !string.Equals(gameSession.PlayerCountryId, country.Id, StringComparison.Ordinal))
            {
                SetUnboundValues();
                return false;
            }

            session = gameSession;
            state = gameSession.PlayerCountryState;
            state.Changed += OnCountryStateChanged;
            session.Clock.DateChanged += OnDateChanged;

            countryName.text = country.DisplayName.ToUpperInvariant();
            mandateName.text = country.Leader != null
                ? $"Mandat de {country.Leader.DisplayName}"
                : "Mandat présidentiel";
            emblemFallback.text = country.VisualIdentifier;
            ApplyTheme(country.Theme);
            RefreshAllValues();
            return true;
        }

        public void Unbind()
        {
            if (state != null)
            {
                state.Changed -= OnCountryStateChanged;
            }

            if (session != null)
            {
                session.Clock.DateChanged -= OnDateChanged;
            }

            state = null;
            session = null;
        }

        private VisualElement CreateMacroIndicator(
            string label,
            string metadata,
            out Label valueLabel,
            string valueName)
        {
            var indicator = UiFactory.Container("dashboard-macro-indicator");
            themedBorders.Add(indicator);
            indicator.Add(UtilitySemibold(Secondary(UiFactory.Label(label, "dashboard-indicator-label"))));
            valueLabel = Prestige(Primary(UiFactory.Label("—", "dashboard-macro-value")));
            valueLabel.name = valueName;
            indicator.Add(valueLabel);
            indicator.Add(UtilityRegular(Secondary(UiFactory.Label(metadata, "dashboard-indicator-meta"))));
            return indicator;
        }

        private VisualElement CreatePoliticalIndicator(
            string label,
            out Label valueLabel,
            out VisualElement fill,
            string valueName)
        {
            var indicator = UiFactory.Container("dashboard-political-indicator");
            var heading = UiFactory.Container("dashboard-political-indicator-heading");
            heading.Add(UtilitySemibold(Secondary(UiFactory.Label(label, "dashboard-political-label"))));
            valueLabel = Prestige(Primary(UiFactory.Label("—", "dashboard-political-value")));
            valueLabel.name = valueName;
            heading.Add(valueLabel);
            indicator.Add(heading);
            var track = UiFactory.Container("dashboard-political-track");
            fill = Layer("dashboard-political-fill");
            track.Add(fill);
            trackElements.Add(track);
            fillElements.Add(fill);
            indicator.Add(track);
            return indicator;
        }

        private void RefreshAllValues()
        {
            if (state == null || session == null)
            {
                return;
            }

            RefreshPopulation();
            RefreshGdp();
            RefreshTreasury();
            RefreshApproval();
            RefreshStability();
            RefreshPoliticalCapital();
            RefreshDate();
        }

        private void OnCountryStateChanged(CountryState changedState, CountryStateChange change)
        {
            if (!ReferenceEquals(changedState, state))
            {
                return;
            }

            switch (change)
            {
                case CountryStateChange.Population:
                    RefreshPopulation();
                    break;
                case CountryStateChange.GdpUsd:
                    RefreshGdp();
                    break;
                case CountryStateChange.TreasuryUsd:
                    RefreshTreasury();
                    break;
                case CountryStateChange.PublicApproval:
                    RefreshApproval();
                    break;
                case CountryStateChange.Stability:
                    RefreshStability();
                    break;
                case CountryStateChange.PoliticalCapital:
                    RefreshPoliticalCapital();
                    break;
            }
        }

        private void OnDateChanged(GameClock clock)
        {
            if (session != null && ReferenceEquals(clock, session.Clock))
            {
                RefreshDate();
            }
        }

        private void RefreshPopulation()
        {
            populationValue.text = StatecraftValueFormatter.FormatPopulation(state.Population);
        }

        private void RefreshGdp()
        {
            gdpValue.text = StatecraftValueFormatter.FormatUsd(state.GdpUsd);
        }

        private void RefreshTreasury()
        {
            treasuryValue.text = StatecraftValueFormatter.FormatUsd(state.TreasuryUsd);
        }

        private void RefreshApproval()
        {
            SetPercentage(approvalValue, approvalFill, state.PublicApproval);
        }

        private void RefreshStability()
        {
            SetPercentage(stabilityValue, stabilityFill, state.Stability);
        }

        private void RefreshPoliticalCapital()
        {
            SetPercentage(politicalCapitalValue, politicalCapitalFill, state.PoliticalCapital);
        }

        private void RefreshDate()
        {
            dateValue.text = StatecraftValueFormatter.FormatGameDate(session.Clock.CurrentDate);
        }

        private void ApplyTheme(CountryTheme theme)
        {
            style.backgroundColor = theme.BackgroundColor;
            surfaceTexture.style.backgroundImage = theme.SurfaceTexture != null
                ? new StyleBackground(theme.SurfaceTexture)
                : new StyleBackground(StyleKeyword.None);
            emblem.style.backgroundImage = theme.Emblem != null
                ? new StyleBackground(theme.Emblem)
                : new StyleBackground(StyleKeyword.None);
            emblem.style.unityBackgroundImageTintColor = theme.OrnamentColor;
            emblemFallback.style.display = theme.Emblem == null ? DisplayStyle.Flex : DisplayStyle.None;

            foreach (var label in primaryLabels)
            {
                label.style.color = theme.PrimaryTextColor;
            }

            foreach (var label in secondaryLabels)
            {
                label.style.color = theme.SecondaryTextColor;
            }

            foreach (var label in accentLabels)
            {
                label.style.color = theme.AccentColor;
            }

            var prestigeFont = theme.LeaderPrestigeStrongFont ?? theme.LeaderPrestigeFont ??
                (typography != null ? typography.PrestigeSemibold : null);
            foreach (var label in prestigeLabels)
            {
                label.style.unityFont = prestigeFont != null
                    ? new StyleFont(prestigeFont)
                    : new StyleFont(StyleKeyword.Null);
            }

            foreach (var element in themedBorders)
            {
                element.style.borderLeftColor = WithAlpha(theme.BorderColor, 0.5f);
                element.style.borderRightColor = WithAlpha(theme.BorderColor, 0.5f);
                element.style.borderTopColor = WithAlpha(theme.BorderColor, 0.5f);
                element.style.borderBottomColor = WithAlpha(theme.BorderColor, 0.5f);
            }

            foreach (var track in trackElements)
            {
                track.style.backgroundColor = WithAlpha(theme.PrimaryTextColor, 0.1f);
            }

            foreach (var fill in fillElements)
            {
                fill.style.backgroundColor = theme.AccentColor;
            }

            headerSignal.style.backgroundColor = theme.AccentColor;
            sectionRule.style.backgroundColor = WithAlpha(theme.AccentColor, 0.48f);
            briefingRule.style.backgroundColor = WithAlpha(theme.AccentColor, 0.4f);
            briefingMarker.style.backgroundColor = theme.AccentColor;
            statusDot.style.backgroundColor = theme.AccentColor;
            briefingSurface.style.backgroundColor = WithAlpha(theme.SurfaceColor, 0.58f);
            navigationRule.style.backgroundColor = WithAlpha(theme.BorderColor, 0.55f);
            navigationActive.style.borderBottomColor = theme.AccentColor;
            worldMapButton.style.color = theme.SecondaryTextColor;
            leaderButton.style.color = theme.SecondaryTextColor;
            worldMapButton.style.borderBottomColor = WithAlpha(theme.BorderColor, 0.35f);
            leaderButton.style.borderBottomColor = WithAlpha(theme.BorderColor, 0.35f);
        }

        private void SetUnboundValues()
        {
            countryName.text = "AUCUN MANDAT";
            mandateName.text = "Session inactive";
            dateValue.text = "—";
            populationValue.text = "—";
            gdpValue.text = "—";
            treasuryValue.text = "—";
            SetPercentage(approvalValue, approvalFill, 0f, false);
            SetPercentage(stabilityValue, stabilityFill, 0f, false);
            SetPercentage(politicalCapitalValue, politicalCapitalFill, 0f, false);
        }

        private static void SetPercentage(Label label, VisualElement fill, float value, bool showValue = true)
        {
            label.text = showValue ? StatecraftValueFormatter.FormatPercentage(value) : "—";
            fill.style.width = Length.Percent(Mathf.Clamp(value, 0f, 100f));
        }

        private Label Primary(Label label)
        {
            primaryLabels.Add(label);
            return label;
        }

        private Label Secondary(Label label)
        {
            secondaryLabels.Add(label);
            return label;
        }

        private Label Accent(Label label)
        {
            accentLabels.Add(label);
            return label;
        }

        private Label Prestige(Label label)
        {
            prestigeLabels.Add(label);
            return label;
        }

        private Label UtilityRegular(Label label)
        {
            ApplyFont(label, typography != null ? typography.UtilityRegular : null);
            return label;
        }

        private Label UtilityMedium(Label label)
        {
            ApplyFont(label, typography != null ? typography.UtilityMedium : null);
            return label;
        }

        private Label UtilitySemibold(Label label)
        {
            ApplyFont(label, typography != null ? typography.UtilitySemibold : null);
            return label;
        }

        private void ApplyButtonFont(Button button)
        {
            button.style.unityFont = typography != null && typography.UtilitySemibold != null
                ? new StyleFont(typography.UtilitySemibold)
                : new StyleFont(StyleKeyword.Null);
        }

        private static void ApplyFont(Label label, Font font)
        {
            label.style.unityFont = font != null
                ? new StyleFont(font)
                : new StyleFont(StyleKeyword.Null);
        }

        private static VisualElement Layer(string className)
        {
            var layer = UiFactory.Container(className);
            layer.pickingMode = PickingMode.Ignore;
            return layer;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
