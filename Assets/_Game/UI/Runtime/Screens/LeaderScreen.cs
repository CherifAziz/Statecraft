using System;
using System.Collections.Generic;
using Statecraft.Data;
using Statecraft.UI.Components;
using Statecraft.UI.Themes;
using UnityEngine.UIElements;

namespace Statecraft.UI.Screens
{
    public sealed class LeaderScreen : VisualElement
    {
        private readonly Label countryName;
        private readonly Label countryMetadata;
        private readonly Label countryEmblemFallback;
        private readonly Label leaderName;
        private readonly Label leaderTitle;
        private readonly Label portraitMonogram;
        private readonly VisualElement backgroundArtwork;
        private readonly VisualElement ambientLight;
        private readonly VisualElement surfaceTexture;
        private readonly VisualElement foregroundOverlay;
        private readonly VisualElement countryEmblem;
        private readonly VisualElement portraitStage;
        private readonly VisualElement portraitArtwork;
        private readonly VisualElement portraitFrame;
        private readonly VisualElement portraitOrnament;
        private readonly VisualElement leaderIdentity;
        private readonly VisualElement countryRule;
        private readonly VisualElement statsDivider;
        private readonly VisualElement skillsDivider;
        private readonly Button backButton;
        private readonly Dictionary<string, LeaderStatView> statViews = new();
        private readonly List<LeaderSkillCard> skillCards = new();
        private readonly List<Label> primaryLabels = new();
        private readonly List<Label> secondaryLabels = new();
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
            var countryIdentity = UiFactory.Container("leader-country-identity");
            countryRule = UiFactory.Container("leader-country-rule");
            var countryCopy = UiFactory.Container("leader-country-copy");
            countryName = TrackPrimary(UiFactory.Label("COUNTRY", "leader-country-name"));
            countryMetadata = TrackSecondary(UiFactory.Label("CAPITAL • POPULATION • GDP", "leader-country-meta"));
            countryCopy.Add(countryName);
            countryCopy.Add(countryMetadata);
            countryIdentity.Add(countryRule);
            countryIdentity.Add(countryCopy);

            backButton = UiFactory.Button("←  RETOUR À LA CARTE", back, "back-button");
            header.Add(countryIdentity);
            header.Add(backButton);

            var stage = UiFactory.Container("leader-stage");
            var visualColumn = UiFactory.Container("leader-visual-column");
            portraitStage = UiFactory.Container("portrait-stage");
            portraitArtwork = Layer("portrait-artwork");
            portraitOrnament = Layer("portrait-ornament");
            portraitFrame = Layer("portrait-frame");
            countryEmblem = UiFactory.Container("leader-emblem");
            countryEmblemFallback = TrackSecondary(UiFactory.Label("—", "leader-emblem-fallback"));
            countryEmblem.Add(countryEmblemFallback);
            portraitMonogram = TrackPrimary(UiFactory.Label("—", "portrait-monogram"));

            leaderIdentity = UiFactory.Container("leader-identity");
            leaderIdentity.Add(TrackSecondary(UiFactory.Label("CHEF D'ÉTAT", "leader-function")));
            leaderName = TrackPrimary(UiFactory.Label("Leader", "leader-name"));
            leaderTitle = TrackSecondary(UiFactory.Label("Title", "leader-title"));
            leaderIdentity.Add(leaderName);
            leaderIdentity.Add(leaderTitle);

            portraitStage.Add(portraitArtwork);
            portraitStage.Add(portraitOrnament);
            portraitStage.Add(portraitMonogram);
            portraitStage.Add(countryEmblem);
            portraitStage.Add(leaderIdentity);
            portraitStage.Add(portraitFrame);
            visualColumn.Add(portraitStage);

            var editorialColumn = UiFactory.Container("leader-editorial-column");
            var statsSection = UiFactory.Container("leader-stats-section");
            var statsHeading = UiFactory.Container("editorial-heading");
            var statsHeadingCopy = UiFactory.Container("editorial-heading-copy");
            statsHeadingCopy.Add(TrackSecondary(UiFactory.Label("ATTRIBUTS DU DIRIGEANT", "eyebrow")));
            statsHeadingCopy.Add(TrackPrimary(UiFactory.Label("Signature exécutive", "editorial-title")));
            statsHeading.Add(statsHeadingCopy);
            statsHeading.Add(TrackSecondary(UiFactory.Label("01 / PROFIL", "editorial-index")));
            statsDivider = UiFactory.Container("editorial-divider");

            var statsGrid = UiFactory.Container("stats-grid");
            AddStat(statsGrid, "Charisma");
            AddStat(statsGrid, "Diplomacy");
            AddStat(statsGrid, "Authority");
            AddStat(statsGrid, "Strategy");
            AddStat(statsGrid, "Economy");
            AddStat(statsGrid, "Eloquence");

            statsSection.Add(statsHeading);
            statsSection.Add(statsDivider);
            statsSection.Add(statsGrid);

            var skillsSection = UiFactory.Container("leader-skills-section");
            var skillsHeading = UiFactory.Container("editorial-heading");
            var skillsHeadingCopy = UiFactory.Container("editorial-heading-copy");
            skillsHeadingCopy.Add(TrackSecondary(UiFactory.Label("COMPÉTENCES", "eyebrow")));
            skillsHeadingCopy.Add(TrackPrimary(UiFactory.Label("Doctrine du mandat", "editorial-title")));
            skillsHeading.Add(skillsHeadingCopy);
            skillsHeading.Add(TrackSecondary(UiFactory.Label("02 / APTITUDES", "editorial-index")));
            skillsDivider = UiFactory.Container("editorial-divider");

            var skillGrid = UiFactory.Container("leader-skill-grid");
            foreach (var skill in CreateDemoSkills())
            {
                var card = new LeaderSkillCard();
                card.Bind(skill);
                card.Selected += SelectSkillCard;
                skillCards.Add(card);
                skillGrid.Add(card);
            }

            skillsSection.Add(skillsHeading);
            skillsSection.Add(skillsDivider);
            skillsSection.Add(skillGrid);

            editorialColumn.Add(statsSection);
            editorialColumn.Add(skillsSection);
            stage.Add(visualColumn);
            stage.Add(editorialColumn);

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
            leaderName.text = leader.DisplayName;
            leaderTitle.text = leader.Title;
            portraitMonogram.text = GetMonogram(leader.DisplayName);

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
            }
            else
            {
                portraitArtwork.style.backgroundImage = StyleKeyword.None;
                portraitArtwork.RemoveFromClassList("portrait-artwork--visible");
                portraitMonogram.style.display = DisplayStyle.Flex;
                portraitOrnament.style.display = DisplayStyle.Flex;
            }

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

            var accents = new List<VisualElement> { countryRule, statsDivider, skillsDivider };
            foreach (var statView in statViews.Values)
            {
                accents.Add(statView.Fill);
            }

            CountryThemeApplicator.Apply(country.Theme, new CountryThemeBindings
            {
                Root = this,
                BackgroundArtwork = backgroundArtwork,
                ForegroundOverlay = foregroundOverlay,
                Emblem = countryEmblem,
                PortraitFrame = portraitFrame,
                SurfaceTexture = surfaceTexture,
                Surfaces = new[] { leaderIdentity },
                Accents = accents,
                Ornaments = new[] { portraitOrnament },
                Borders = new[] { portraitStage },
                PrimaryLabels = primaryLabels,
                SecondaryLabels = secondaryLabels,
                Buttons = new[] { backButton },
                SkillCards = skillCards
            });
            backgroundFx.Apply(country.Theme);

            SelectSkillCard(skillCards[0]);
        }

        private void AddStat(VisualElement statsGrid, string statName)
        {
            var statView = new LeaderStatView(statName);
            statViews.Add(statName, statView);
            primaryLabels.Add(statView.ValueLabel);
            secondaryLabels.Add(statView.NameLabel);
            statsGrid.Add(statView);
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

        private static VisualElement Layer(string className)
        {
            var layer = UiFactory.Container(className);
            layer.pickingMode = PickingMode.Ignore;
            return layer;
        }

        private static IReadOnlyList<LeaderSkillCardData> CreateDemoSkills()
        {
            return new[]
            {
                new LeaderSkillCardData("executive-mandate", "Mandat exécutif", "Passif", "Renforce la présence institutionnelle du dirigeant."),
                new LeaderSkillCardData("state-address", "Adresse à la nation", "Influence", "Structure les prises de parole destinées à l'opinion publique."),
                new LeaderSkillCardData("inner-council", "Conseil restreint", "Stratégie", "Prépare les arbitrages sensibles au plus haut niveau de l'État."),
                new LeaderSkillCardData("legacy-doctrine", "Doctrine d'héritage", "Signature", "Un emplacement réservé à une doctrine de mandat future.", isLocked: true)
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
                ? $"{population / 1_000_000d:0.#} M"
                : $"{population / 1_000d:0.#} K";
        }

        private static string FormatGdp(double gdpUsd)
        {
            return gdpUsd >= 1_000_000_000_000d
                ? $"{gdpUsd / 1_000_000_000_000d:0.##} T$"
                : $"{gdpUsd / 1_000_000_000d:0} Md$";
        }
    }
}
