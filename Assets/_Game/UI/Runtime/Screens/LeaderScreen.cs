using System;
using System.Collections.Generic;
using System.Globalization;
using Statecraft.Data;
using Statecraft.UI.Components;
using Statecraft.UI.Themes;
using UnityEngine.UIElements;

namespace Statecraft.UI.Screens
{
    public sealed class LeaderScreen : VisualElement
    {
        private static readonly CultureInfo DisplayCulture = CultureInfo.GetCultureInfo("fr-FR");

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
        private readonly VisualElement portraitStage;
        private readonly VisualElement portraitArtwork;
        private readonly VisualElement portraitFrame;
        private readonly VisualElement portraitOrnament;
        private readonly VisualElement editorialPanel;
        private readonly VerticalGradientElement identityGradient;
        private readonly VerticalGradientElement editorialBackdrop;
        private readonly Button backButton;
        private readonly Dictionary<string, LeaderStatView> statViews = new();
        private readonly List<LeaderSkillCard> skillCards = new();
        private readonly List<Label> primaryLabels = new();
        private readonly List<Label> secondaryLabels = new();
        private readonly List<Label> accentLabels = new();
        private readonly List<Label> prestigeLabels = new();
        private readonly List<VisualElement> accentElements = new();
        private readonly List<VisualElement> themedBorders = new();
        private readonly LeaderBackgroundFx backgroundFx;
        private int portraitRevealVersion;

        public LeaderScreen(Action back)
        {
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
            countryEmblemFallback = TrackSecondary(UiFactory.Label("—", "leader-emblem-fallback"));
            countryEmblem.Add(countryEmblemFallback);

            var countrySeparator = Accent("leader-country-separator");
            var countryCopy = UiFactory.Container("leader-country-copy");
            countryName = TrackPrestigePrimary(UiFactory.Label("COUNTRY", "leader-country-name"));
            countryMetadata = TrackSecondary(UiFactory.Label("CAPITAL • POPULATION • GDP", "leader-country-meta"));
            countryCopy.Add(countryName);
            countryCopy.Add(countryMetadata);
            countryIdentity.Add(countryEmblem);
            countryIdentity.Add(countrySeparator);
            countryIdentity.Add(countryCopy);

            var countryRule = CreateSplitRule("leader-country-rule");
            backButton = UiFactory.Button("←  CARTE DU MONDE", back, "back-button");
            headerContent.Add(countryIdentity);
            headerContent.Add(countryRule);
            headerContent.Add(backButton);
            header.Add(headerContent);

            var stage = UiFactory.Container("leader-stage");
            var visualColumn = UiFactory.Container("leader-visual-column");
            portraitStage = UiFactory.Container("portrait-stage");
            portraitArtwork = Layer("portrait-artwork");
            portraitOrnament = Layer("portrait-ornament");
            portraitFrame = Layer("portrait-frame");
            portraitMonogram = TrackPrimary(UiFactory.Label("—", "portrait-monogram"));

            portraitStage.Add(portraitArtwork);
            portraitStage.Add(portraitOrnament);
            portraitStage.Add(portraitFrame);
            portraitStage.Add(portraitMonogram);

            var leaderIdentity = UiFactory.Container("leader-identity");
            identityGradient = new VerticalGradientElement("leader-identity-gradient");
            var identitySeparator = CreateSplitRule("leader-identity-separator");
            var identityCopy = UiFactory.Container("leader-identity-copy");
            leaderName = TrackPrestigePrimary(UiFactory.Label("LEADER", "leader-name"));
            leaderTitle = TrackPrestigeAccent(UiFactory.Label("Title", "leader-title"));
            var leaderFunction = TrackSecondary(UiFactory.Label("CHEF D'ÉTAT", "leader-function"));
            identityCopy.Add(leaderName);
            identityCopy.Add(leaderTitle);
            identityCopy.Add(leaderFunction);
            leaderIdentity.Add(identityGradient);
            leaderIdentity.Add(identitySeparator);
            leaderIdentity.Add(identityCopy);
            portraitStage.Add(leaderIdentity);
            AddCornerOrnaments(portraitStage, "hero-corner");
            visualColumn.Add(portraitStage);

            editorialPanel = UiFactory.Container("leader-editorial-panel");
            editorialBackdrop = new VerticalGradientElement("leader-editorial-backdrop");
            editorialSurfaceTexture = Layer("leader-editorial-texture");
            editorialPanel.Add(editorialBackdrop);
            editorialPanel.Add(editorialSurfaceTexture);
            AddCornerOrnaments(editorialPanel, "panel-corner");
            var editorialContent = UiFactory.Container("leader-editorial-content");

            var statsSection = UiFactory.Container("leader-stats-section");
            statsSection.Add(CreateEditorialHeading(
                "ATTRIBUTS DU DIRIGEANT",
                "Signature exécutive",
                "editorial-heading--stats"));

            var statsGrid = UiFactory.Container("stats-grid");
            AddStat(statsGrid, "Charisma", "Charisme", "CH");
            AddStat(statsGrid, "Diplomacy", "Diplomatie", "DI");
            AddStat(statsGrid, "Authority", "Autorité", "AU");
            AddStat(statsGrid, "Strategy", "Stratégie", "ST");
            AddStat(statsGrid, "Economy", "Économie", "ÉC");
            AddStat(statsGrid, "Eloquence", "Éloquence", "ÉL");
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

            themedBorders.Add(portraitStage);
            themedBorders.Add(editorialPanel);

            Add(backgroundArtwork);
            Add(ambientLight);
            Add(surfaceTexture);
            Add(foregroundOverlay);
            Add(header);
            Add(stage);
        }

        public void Bind(CountryDefinition country)
        {
            portraitRevealVersion++;
            var leader = country.Leader;
            countryName.text = country.DisplayName.ToUpperInvariant();
            countryMetadata.text = $"{country.Capital.ToUpperInvariant()}  •  {FormatPopulation(country.Population)} HAB.  •  {FormatGdp(country.GdpUsd)} PIB";
            countryEmblemFallback.text = country.VisualIdentifier;
            leaderName.text = leader.DisplayName.ToUpperInvariant();
            leaderTitle.text = leader.Title;
            portraitMonogram.text = GetMonogram(leader.DisplayName);

            BindPortrait(leader);
            countryEmblemFallback.style.display = country.Theme.Emblem == null
                ? DisplayStyle.Flex
                : DisplayStyle.None;

            var stats = leader.Stats;
            statViews["Charisma"].SetValue(stats.charisma);
            statViews["Diplomacy"].SetValue(stats.diplomacy);
            statViews["Authority"].SetValue(stats.authority);
            statViews["Strategy"].SetValue(stats.strategy);
            statViews["Economy"].SetValue(stats.economy);
            statViews["Eloquence"].SetValue(stats.eloquence);

            var skillData = CreateDemoSkills(country.Theme);
            for (var index = 0; index < skillCards.Count; index++)
            {
                skillCards[index].Bind(skillData[index]);
            }

            CountryThemeApplicator.Apply(country.Theme, new CountryThemeBindings
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
                Buttons = new[] { backButton },
                SkillCards = skillCards
            });
            backgroundFx.Apply(country.Theme);

            SelectSkillCard(skillCards[0]);
        }

        private VisualElement CreateEditorialHeading(string eyebrow, string title, string modifierClass)
        {
            var heading = UiFactory.Container("editorial-heading");
            heading.AddToClassList(modifierClass);
            heading.Add(TrackAccent(UiFactory.Label(eyebrow, "eyebrow")));
            heading.Add(TrackPrestigePrimary(UiFactory.Label(title, "editorial-title")));
            heading.Add(CreateSplitRule("editorial-title-rule"));
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

            portraitArtwork.style.backgroundImage = StyleKeyword.None;
            portraitArtwork.RemoveFromClassList("portrait-artwork--visible");
            portraitMonogram.style.display = DisplayStyle.Flex;
            portraitOrnament.style.display = DisplayStyle.Flex;
        }

        private void AddStat(VisualElement statsGrid, string key, string displayName, string icon)
        {
            var statView = new LeaderStatView(displayName, icon);
            statViews.Add(key, statView);
            primaryLabels.Add(statView.ValueLabel);
            secondaryLabels.Add(statView.NameLabel);
            accentLabels.Add(statView.IconLabel);
            accentElements.Add(statView.Fill);
            accentElements.Add(statView.Marker);
            themedBorders.Add(statView.IconFrame);
            statsGrid.Add(statView);
        }

        private void AddCornerOrnaments(VisualElement parent, string scopeClass)
        {
            AddCornerOrnament(parent, scopeClass, "top-left");
            AddCornerOrnament(parent, scopeClass, "top-right");
            AddCornerOrnament(parent, scopeClass, "bottom-left");
            AddCornerOrnament(parent, scopeClass, "bottom-right");
        }

        private void AddCornerOrnament(VisualElement parent, string scopeClass, string position)
        {
            var corner = UiFactory.Container("corner-ornament");
            corner.AddToClassList(scopeClass);
            corner.AddToClassList($"corner-ornament--{position}");
            corner.pickingMode = PickingMode.Ignore;
            corner.Add(Accent("corner-line-horizontal"));
            corner.Add(Accent("corner-line-vertical"));
            corner.Add(Accent("corner-diamond"));
            parent.Add(corner);
        }

        private VisualElement CreateSplitRule(string className)
        {
            var rule = UiFactory.Container(className);
            rule.pickingMode = PickingMode.Ignore;
            rule.Add(Accent("split-rule-line"));
            rule.Add(Accent("split-rule-diamond"));
            rule.Add(Accent("split-rule-line"));
            return rule;
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

        private Label TrackPrestigePrimary(Label label)
        {
            prestigeLabels.Add(label);
            return TrackPrimary(label);
        }

        private Label TrackPrestigeAccent(Label label)
        {
            prestigeLabels.Add(label);
            return TrackAccent(label);
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
                new LeaderSkillCardData("executive-mandate", "Mandat exécutif", "Passif", "Renforce la présence institutionnelle du dirigeant et la stabilité intérieure.", theme?.GetLeaderSkillArtwork("executive-mandate")),
                new LeaderSkillCardData("state-address", "Adresse à la nation", "Influence", "Structure les prises de parole destinées à l'opinion publique et renforce l'adhésion.", theme?.GetLeaderSkillArtwork("state-address")),
                new LeaderSkillCardData("inner-council", "Conseil restreint", "Stratégie", "Prépare les arbitrages sensibles au plus haut niveau de l'État.", theme?.GetLeaderSkillArtwork("inner-council")),
                new LeaderSkillCardData("legacy-doctrine", "Doctrine d'héritage", "Signature", "Un emplacement réservé à une doctrine de mandat future.", theme?.GetLeaderSkillArtwork("legacy-doctrine"), isLocked: true)
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
