using System;
using System.Collections.Generic;
using System.Globalization;
using Statecraft.Data;
using Statecraft.UI.Components;
using Statecraft.UI.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Screens
{
    public sealed class LeaderScreen : VisualElement
    {
        private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("fr-FR");

        private readonly StatecraftTypography typography;
        private readonly Label countryName;
        private readonly Label countryMetadata;
        private readonly Label countryEmblemFallback;
        private readonly Label leaderName;
        private readonly Label leaderTitle;
        private readonly Label portraitMonogram;
        private readonly VisualElement backgroundArtwork;
        private readonly VisualElement ambientLight;
        private readonly VisualElement surfaceTexture;
        private readonly VisualElement editorialSurfaceTexture;
        private readonly VisualElement foregroundOverlay;
        private readonly VisualElement countryEmblem;
        private readonly VisualElement portraitArtwork;
        private readonly VisualElement portraitFrame;
        private readonly VisualElement portraitOrnament;
        private readonly VerticalGradientElement identityGradient;
        private readonly VerticalGradientElement editorialBackdrop;
        private readonly LeaderDivider identityOrnament;
        private readonly Button backButton;
        private readonly Dictionary<string, LeaderStatView> statViews = new();
        private readonly List<LeaderSkillCard> skillCards = new();
        private readonly List<LeaderDivider> dividers = new();
        private readonly List<LeaderCornerSet> cornerSets = new();
        private readonly List<Label> primaryLabels = new();
        private readonly List<Label> secondaryLabels = new();
        private readonly List<Label> accentLabels = new();
        private readonly List<Label> prestigeLabels = new();
        private readonly List<Label> prestigeStrongLabels = new();
        private readonly List<Label> utilityRegularLabels = new();
        private readonly List<Label> utilityMediumLabels = new();
        private readonly List<Label> utilitySemiboldLabels = new();
        private readonly List<VisualElement> accentElements = new();
        private readonly List<VisualElement> themedBorders = new();
        private readonly LeaderBackgroundFx backgroundFx;
        private int portraitRevealVersion;

        public LeaderScreen(Action back, StatecraftTypography typography)
        {
            this.typography = typography;
            name = "leader-screen";
            AddToClassList("screen");
            AddToClassList("leader-screen");

            backgroundArtwork = Layer("leader-background-artwork");
            ambientLight = Layer("leader-ambient-light");
            surfaceTexture = Layer("leader-surface-texture");
            foregroundOverlay = Layer("leader-foreground-overlay");
            backgroundFx = new LeaderBackgroundFx(this, backgroundArtwork, ambientLight);

            var header = UiFactory.Container("leader-header");
            var headerContent = UiFactory.Container("leader-header-content");
            var countryIdentity = UiFactory.Container("leader-country-identity");
            countryEmblem = UiFactory.Container("leader-emblem");
            countryEmblemFallback = UtilityMedium(TrackSecondary(UiFactory.Label("—", "leader-emblem-fallback")));
            countryEmblem.Add(countryEmblemFallback);

            var countrySeparator = Accent("leader-country-separator");
            var countryCopy = UiFactory.Container("leader-country-copy");
            countryName = TrackPrestigeStrongPrimary(UiFactory.Label("COUNTRY", "leader-country-name"));
            countryMetadata = UtilityRegular(TrackSecondary(UiFactory.Label("CAPITAL • POPULATION • GDP", "leader-country-meta")));
            countryCopy.Add(countryName);
            countryCopy.Add(countryMetadata);
            countryIdentity.Add(countryEmblem);
            countryIdentity.Add(countrySeparator);
            countryIdentity.Add(countryCopy);

            var countryRule = CreateDivider("leader-country-rule");
            backButton = UiFactory.Button("←  CARTE DU MONDE", back, "back-button");
            headerContent.Add(countryIdentity);
            headerContent.Add(countryRule);
            headerContent.Add(backButton);
            header.Add(headerContent);

            var stage = UiFactory.Container("leader-stage");
            var visualColumn = UiFactory.Container("leader-visual-column");
            var portraitStage = UiFactory.Container("portrait-stage");
            portraitArtwork = Layer("portrait-artwork");
            portraitOrnament = Layer("portrait-ornament");
            portraitFrame = Layer("portrait-frame");
            portraitMonogram = TrackPrestigeStrongPrimary(UiFactory.Label("—", "portrait-monogram"));

            portraitStage.Add(portraitArtwork);
            portraitStage.Add(portraitOrnament);
            portraitStage.Add(portraitFrame);
            portraitStage.Add(portraitMonogram);

            var leaderIdentity = UiFactory.Container("leader-identity");
            identityGradient = new VerticalGradientElement("leader-identity-gradient");
            identityOrnament = new LeaderDivider("leader-identity-ornament");
            var identityCopy = UiFactory.Container("leader-identity-copy");
            leaderName = TrackPrestigeStrongPrimary(UiFactory.Label("LEADER", "leader-name"));
            leaderTitle = TrackPrestigeAccent(UiFactory.Label("Title", "leader-title"));
            var leaderFunction = UtilitySemibold(TrackSecondary(UiFactory.Label("CHEF D'ÉTAT", "leader-function")));
            identityCopy.Add(leaderName);
            identityCopy.Add(leaderTitle);
            identityCopy.Add(leaderFunction);
            leaderIdentity.Add(identityGradient);
            leaderIdentity.Add(identityOrnament);
            leaderIdentity.Add(identityCopy);
            var heroCorners = new LeaderCornerSet("leader-corner-set--hero");
            cornerSets.Add(heroCorners);
            portraitStage.Add(heroCorners);
            portraitStage.Add(leaderIdentity);
            visualColumn.Add(portraitStage);

            var editorialPanel = UiFactory.Container("leader-editorial-panel");
            editorialBackdrop = new VerticalGradientElement("leader-editorial-backdrop");
            editorialSurfaceTexture = Layer("leader-editorial-texture");
            editorialPanel.Add(editorialBackdrop);
            editorialPanel.Add(editorialSurfaceTexture);
            var panelCorners = new LeaderCornerSet("leader-corner-set--panel");
            cornerSets.Add(panelCorners);
            editorialPanel.Add(panelCorners);
            var editorialContent = UiFactory.Container("leader-editorial-content");

            var statsSection = UiFactory.Container("leader-stats-section");
            statsSection.Add(CreateEditorialHeading(
                "ATTRIBUTS DU DIRIGEANT",
                "Signature exécutive",
                "editorial-heading--stats"));

            var statsGrid = UiFactory.Container("stats-grid");
            AddStat(statsGrid, "charisma", "Charisme", "CH");
            AddStat(statsGrid, "diplomacy", "Diplomatie", "DI");
            AddStat(statsGrid, "authority", "Autorité", "AU");
            AddStat(statsGrid, "strategy", "Stratégie", "ST");
            AddStat(statsGrid, "economy", "Économie", "ÉC");
            AddStat(statsGrid, "eloquence", "Éloquence", "ÉL");
            statsSection.Add(statsGrid);

            var sectionDivider = Accent("editorial-section-divider");
            var skillsSection = UiFactory.Container("leader-skills-section");
            skillsSection.Add(CreateEditorialHeading(
                "COMPÉTENCES",
                "Doctrine du mandat",
                "editorial-heading--skills"));

            var skillGrid = UiFactory.Container("leader-skill-grid");
            foreach (var skill in CreateDemoSkills())
            {
                var card = new LeaderSkillCard();
                card.ApplyTypography(typography);
                card.Bind(skill);
                card.Selected += SelectSkillCard;
                skillCards.Add(card);
                skillGrid.Add(card);
            }

            skillsSection.Add(skillGrid);
            editorialContent.Add(statsSection);
            editorialContent.Add(sectionDivider);
            editorialContent.Add(skillsSection);
            editorialPanel.Add(editorialContent);
            stage.Add(visualColumn);
            stage.Add(editorialPanel);

            themedBorders.Add(editorialPanel);

            Add(backgroundArtwork);
            Add(ambientLight);
            Add(surfaceTexture);
            Add(foregroundOverlay);
            Add(header);
            Add(stage);

            ApplyUtilityTypography();
        }

        public void Bind(CountryDefinition country)
        {
            portraitRevealVersion++;
            var leader = country.Leader;
            var theme = country.Theme;
            countryName.text = country.DisplayName.ToUpperInvariant();
            countryMetadata.text = $"{country.Capital.ToUpperInvariant()}  •  {FormatPopulation(country.Population)} HAB.  •  {FormatGdp(country.GdpUsd)} PIB";
            countryEmblemFallback.text = country.VisualIdentifier;
            leaderName.text = leader.DisplayName.ToUpperInvariant();
            leaderTitle.text = leader.Title;
            portraitMonogram.text = GetMonogram(leader.DisplayName);

            BindPortrait(leader);
            countryEmblemFallback.style.display = theme.Emblem == null
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            var stats = leader.Stats;
            BindStat(theme, "charisma", stats.charisma);
            BindStat(theme, "diplomacy", stats.diplomacy);
            BindStat(theme, "authority", stats.authority);
            BindStat(theme, "strategy", stats.strategy);
            BindStat(theme, "economy", stats.economy);
            BindStat(theme, "eloquence", stats.eloquence);

            var skillData = CreateDemoSkills(theme);
            for (var index = 0; index < skillCards.Count; index++)
            {
                skillCards[index].Bind(skillData[index]);
            }

            CountryThemeApplicator.Apply(theme, new CountryThemeBindings
            {
                Root = this,
                BackgroundArtwork = backgroundArtwork,
                ForegroundOverlay = foregroundOverlay,
                Emblem = countryEmblem,
                PortraitFrame = portraitFrame,
                SurfaceTexture = surfaceTexture,
                EditorialSurfaceTexture = editorialSurfaceTexture,
                IdentityGradient = identityGradient,
                EditorialBackdrop = editorialBackdrop,
                Surfaces = Array.Empty<VisualElement>(),
                Accents = accentElements,
                Ornaments = new[] { portraitOrnament },
                Borders = themedBorders,
                PrimaryLabels = primaryLabels,
                SecondaryLabels = secondaryLabels,
                AccentLabels = accentLabels,
                PrestigeLabels = prestigeLabels,
                PrestigeStrongLabels = prestigeStrongLabels,
                PrestigeFallbackFont = typography != null ? typography.UtilityMedium : null,
                PrestigeStrongFallbackFont = typography != null ? typography.UtilitySemibold : null,
                Dividers = dividers,
                IdentityOrnament = identityOrnament,
                CornerSets = cornerSets,
                Buttons = new[] { backButton },
                SkillCards = skillCards
            });
            backgroundFx.Apply(theme);

            SelectSkillCard(skillCards[0]);
        }

        private VisualElement CreateEditorialHeading(string eyebrow, string title, string modifierClass)
        {
            var heading = UiFactory.Container("editorial-heading");
            heading.AddToClassList(modifierClass);
            heading.Add(UtilitySemibold(TrackAccent(UiFactory.Label(eyebrow, "eyebrow"))));
            heading.Add(TrackPrestigeStrongPrimary(UiFactory.Label(title, "editorial-title")));
            heading.Add(CreateDivider("editorial-title-rule"));
            return heading;
        }

        private void BindPortrait(LeaderDefinition leader)
        {
            if (leader.Portrait != null)
            {
                portraitArtwork.style.backgroundImage = new StyleBackground(leader.Portrait);
                portraitMonogram.style.display = DisplayStyle.None;
                portraitOrnament.style.display = DisplayStyle.None;
                portraitArtwork.RemoveFromClassList("portrait-artwork--visible");

                var revealVersion = portraitRevealVersion;
                portraitArtwork.schedule.Execute(() =>
                {
                    if (revealVersion == portraitRevealVersion)
                    {
                        portraitArtwork.AddToClassList("portrait-artwork--visible");
                    }
                }).StartingIn(20);
                return;
            }

            portraitArtwork.style.backgroundImage = new StyleBackground(StyleKeyword.None);
            portraitArtwork.RemoveFromClassList("portrait-artwork--visible");
            portraitMonogram.style.display = DisplayStyle.Flex;
            portraitOrnament.style.display = DisplayStyle.Flex;
        }

        private void AddStat(VisualElement statsGrid, string statId, string displayName, string fallbackIcon)
        {
            var statView = new LeaderStatView(displayName, fallbackIcon);
            statView.ApplyTypography(typography);
            statViews.Add(statId, statView);
            primaryLabels.Add(statView.ValueLabel);
            secondaryLabels.Add(statView.NameLabel);
            accentLabels.Add(statView.IconLabel);
            accentElements.Add(statView.Fill);
            accentElements.Add(statView.Marker);
            themedBorders.Add(statView.IconFrame);
            statsGrid.Add(statView);
        }

        private void BindStat(CountryTheme theme, string statId, int value)
        {
            var statView = statViews[statId];
            statView.SetArtwork(theme.GetLeaderStatArtwork(statId));
            statView.SetValue(value);
        }

        private LeaderDivider CreateDivider(string modifierClass)
        {
            var divider = new LeaderDivider(modifierClass);
            dividers.Add(divider);
            return divider;
        }

        private VisualElement Accent(string className)
        {
            var element = Layer(className);
            accentElements.Add(element);
            return element;
        }

        private void SelectSkillCard(LeaderSkillCard selectedCard)
        {
            foreach (var card in skillCards)
            {
                card.SetSelected(card == selectedCard);
            }
        }

        private Label TrackPrimary(Label label)
        {
            primaryLabels.Add(label);
            return label;
        }

        private Label TrackSecondary(Label label)
        {
            secondaryLabels.Add(label);
            return label;
        }

        private Label TrackAccent(Label label)
        {
            accentLabels.Add(label);
            return label;
        }

        private Label TrackPrestigeAccent(Label label)
        {
            prestigeLabels.Add(label);
            return TrackAccent(label);
        }

        private Label TrackPrestigeStrongPrimary(Label label)
        {
            prestigeStrongLabels.Add(label);
            return TrackPrimary(label);
        }

        private Label UtilityRegular(Label label)
        {
            utilityRegularLabels.Add(label);
            return label;
        }

        private Label UtilityMedium(Label label)
        {
            utilityMediumLabels.Add(label);
            return label;
        }

        private Label UtilitySemibold(Label label)
        {
            utilitySemiboldLabels.Add(label);
            return label;
        }

        private void ApplyUtilityTypography()
        {
            if (typography == null)
            {
                return;
            }

            SetFonts(utilityRegularLabels, typography.UtilityRegular);
            SetFonts(utilityMediumLabels, typography.UtilityMedium);
            SetFonts(utilitySemiboldLabels, typography.UtilitySemibold);
            backButton.style.unityFont = typography.UtilitySemibold != null
                ? new StyleFont(typography.UtilitySemibold)
                : new StyleFont(StyleKeyword.Null);
        }

        private static void SetFonts(IEnumerable<Label> labels, Font font)
        {
            foreach (var label in labels)
            {
                label.style.unityFont = font != null
                    ? new StyleFont(font)
                    : new StyleFont(StyleKeyword.Null);
            }
        }

        private static VisualElement Layer(string className)
        {
            var layer = UiFactory.Container(className);
            layer.pickingMode = PickingMode.Ignore;
            return layer;
        }

        private static IReadOnlyList<LeaderSkillCardData> CreateDemoSkills(CountryTheme theme = null)
        {
            return new[]
            {
                new LeaderSkillCardData("executive-mandate", "Mandat exécutif", "Passif", "Renforce la présence institutionnelle et la stabilité intérieure.", theme?.GetLeaderSkillArtwork("executive-mandate")),
                new LeaderSkillCardData("state-address", "Adresse à la nation", "Influence", "Structure les prises de parole et renforce l'adhésion publique.", theme?.GetLeaderSkillArtwork("state-address")),
                new LeaderSkillCardData("inner-council", "Conseil restreint", "Stratégie", "Prépare les arbitrages sensibles au plus haut niveau de l'État.", theme?.GetLeaderSkillArtwork("inner-council")),
                new LeaderSkillCardData("legacy-doctrine", "Doctrine d'héritage", "Signature", "Emplacement réservé à une doctrine de mandat future.", theme?.GetLeaderSkillArtwork("legacy-doctrine"), isLocked: true)
            };
        }

        private static string GetMonogram(string displayName)
        {
            var words = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return "—";
            }

            return words.Length == 1
                ? words[0][0].ToString().ToUpperInvariant()
                : $"{words[0][0]}{words[^1][0]}".ToUpperInvariant();
        }

        private static string FormatPopulation(long population)
        {
            return population >= 1_000_000
                ? $"{(population / 1_000_000d).ToString("0.#", DisplayCulture)} M"
                : $"{(population / 1_000d).ToString("0.#", DisplayCulture)} K";
        }

        private static string FormatGdp(double gdpUsd)
        {
            return gdpUsd >= 1_000_000_000_000d
                ? $"{(gdpUsd / 1_000_000_000_000d).ToString("0.##", DisplayCulture)} T$"
                : $"{(gdpUsd / 1_000_000_000d).ToString("0", DisplayCulture)} Md$";
        }
    }
}
