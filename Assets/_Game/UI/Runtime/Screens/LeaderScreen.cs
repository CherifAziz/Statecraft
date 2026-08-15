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
        private readonly Label leaderName;
        private readonly Label leaderTitle;
        private readonly Label portraitMonogram;
        private readonly VisualElement themeArtwork;
        private readonly VisualElement portrait;
        private readonly VisualElement portraitOrnament;
        private readonly VisualElement statsPanel;
        private readonly VisualElement skillsPanel;
        private readonly VisualElement headerBorder;
        private readonly Button backButton;
        private readonly Dictionary<string, LeaderStatView> statViews = new();
        private readonly List<Label> primaryLabels = new();
        private readonly List<Label> secondaryLabels = new();

        public LeaderScreen(Action back)
        {
            name = "leader-screen";
            AddToClassList("screen");
            AddToClassList("leader-screen");

            themeArtwork = UiFactory.Container("theme-artwork");

            var header = UiFactory.Container("leader-header");
            headerBorder = UiFactory.Container("header-ornament");
            var identity = UiFactory.Container("leader-country-identity");
            countryName = TrackPrimary(UiFactory.Label("COUNTRY", "leader-country-name"));
            countryMetadata = TrackSecondary(UiFactory.Label("CAPITAL • POPULATION • GDP", "leader-country-meta"));
            identity.Add(countryName);
            identity.Add(countryMetadata);
            backButton = UiFactory.Button("←  RETOUR À LA CARTE", back, "back-button");
            header.Add(identity);
            header.Add(backButton);

            var content = UiFactory.Container("leader-content");
            var portraitColumn = UiFactory.Container("portrait-column");
            portrait = UiFactory.Container("portrait-placeholder");
            portraitOrnament = UiFactory.Container("portrait-ornament");
            portraitMonogram = TrackPrimary(UiFactory.Label("—", "portrait-monogram"));
            portrait.Add(portraitOrnament);
            portrait.Add(portraitMonogram);

            var leaderIdentity = UiFactory.Container("leader-identity");
            leaderIdentity.Add(TrackSecondary(UiFactory.Label("CHEF D'ÉTAT", "eyebrow")));
            leaderName = TrackPrimary(UiFactory.Label("Leader", "leader-name"));
            leaderTitle = TrackSecondary(UiFactory.Label("Title", "leader-title"));
            leaderIdentity.Add(leaderName);
            leaderIdentity.Add(leaderTitle);
            portraitColumn.Add(portrait);
            portraitColumn.Add(leaderIdentity);

            statsPanel = UiFactory.Container("leader-panel");
            statsPanel.Add(TrackSecondary(UiFactory.Label("ATTRIBUTS DU DIRIGEANT", "eyebrow")));
            statsPanel.Add(TrackPrimary(UiFactory.Label("Profil exécutif", "section-title")));
            AddStat("Charisma");
            AddStat("Diplomacy");
            AddStat("Authority");
            AddStat("Strategy");
            AddStat("Economy");
            AddStat("Eloquence");

            skillsPanel = UiFactory.Container("leader-panel");
            skillsPanel.Add(TrackSecondary(UiFactory.Label("COMPÉTENCES", "eyebrow")));
            skillsPanel.Add(TrackPrimary(UiFactory.Label("Doctrine du mandat", "section-title")));
            var skillGrid = UiFactory.Container("skill-grid");
            for (var index = 0; index < 4; index++)
            {
                var slot = UiFactory.Container("skill-slot");
                slot.Add(TrackSecondary(UiFactory.Label("+", "skill-plus")));
                slot.Add(TrackSecondary(UiFactory.Label("EMPLACEMENT VIDE", "skill-label")));
                skillGrid.Add(slot);
            }
            skillsPanel.Add(skillGrid);

            content.Add(portraitColumn);
            content.Add(statsPanel);
            content.Add(skillsPanel);

            Add(themeArtwork);
            Add(headerBorder);
            Add(header);
            Add(content);
        }

        public void Bind(CountryDefinition country)
        {
            var leader = country.Leader;
            countryName.text = country.DisplayName.ToUpperInvariant();
            countryMetadata.text = $"{country.Capital.ToUpperInvariant()}  •  {FormatPopulation(country.Population)} HAB.  •  {FormatGdp(country.GdpUsd)} PIB";
            leaderName.text = leader.DisplayName;
            leaderTitle.text = leader.Title;
            portraitMonogram.text = GetMonogram(leader.DisplayName);

            if (leader.Portrait != null)
            {
                portrait.style.backgroundImage = new StyleBackground(leader.Portrait);
                portraitMonogram.style.display = DisplayStyle.None;
            }
            else
            {
                portraitMonogram.style.display = DisplayStyle.Flex;
            }

            var stats = leader.Stats;
            statViews["Charisma"].SetValue(stats.charisma);
            statViews["Diplomacy"].SetValue(stats.diplomacy);
            statViews["Authority"].SetValue(stats.authority);
            statViews["Strategy"].SetValue(stats.strategy);
            statViews["Economy"].SetValue(stats.economy);
            statViews["Eloquence"].SetValue(stats.eloquence);

            var accents = new List<VisualElement> { headerBorder };
            var borders = new List<VisualElement> { portrait, statsPanel, skillsPanel };
            foreach (var statView in statViews.Values)
            {
                accents.Add(statView.Fill);
            }

            foreach (var skillSlot in skillsPanel.Query<VisualElement>(className: "skill-slot").ToList())
            {
                borders.Add(skillSlot);
            }

            CountryThemeApplicator.Apply(country.Theme, new CountryThemeBindings
            {
                Root = this,
                HeroArtwork = themeArtwork,
                Surfaces = new[] { statsPanel, skillsPanel },
                Accents = accents,
                Ornaments = new[] { portraitOrnament },
                Borders = borders,
                PrimaryLabels = primaryLabels,
                SecondaryLabels = secondaryLabels,
                Buttons = new[] { backButton }
            });
        }

        private void AddStat(string statName)
        {
            var statView = new LeaderStatView(statName);
            statViews.Add(statName, statView);
            primaryLabels.Add(statView.ValueLabel);
            statsPanel.Add(statView);
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
