using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Statecraft.UI.Themes
{
    public sealed class CountryThemeBindings
    {
        public VisualElement Root { get; set; }
        public VisualElement HeroArtwork { get; set; }
        public IReadOnlyList<VisualElement> Surfaces { get; set; }
        public IReadOnlyList<VisualElement> Accents { get; set; }
        public IReadOnlyList<VisualElement> Ornaments { get; set; }
        public IReadOnlyList<VisualElement> Borders { get; set; }
        public IReadOnlyList<Label> PrimaryLabels { get; set; }
        public IReadOnlyList<Label> SecondaryLabels { get; set; }
        public IReadOnlyList<Button> Buttons { get; set; }
    }

    public static class CountryThemeApplicator
    {
        public static void Apply(CountryTheme theme, CountryThemeBindings bindings)
        {
            bindings.Root.style.backgroundColor = theme.BackgroundColor;

            if (theme.BackgroundArtwork != null)
            {
                bindings.Root.style.backgroundImage = new StyleBackground(theme.BackgroundArtwork);
            }
            else
            {
                bindings.Root.style.backgroundImage = StyleKeyword.None;
            }

            if (bindings.HeroArtwork != null)
            {
                bindings.HeroArtwork.style.backgroundColor = theme.PrimaryColor;
                bindings.HeroArtwork.style.backgroundImage = theme.LeaderScreenArtwork != null
                    ? new StyleBackground(theme.LeaderScreenArtwork)
                    : new StyleBackground(StyleKeyword.None);
            }

            SetBackground(bindings.Surfaces, theme.SurfaceColor);
            SetBackground(bindings.Accents, theme.AccentColor);
            SetBackground(bindings.Ornaments, theme.OrnamentColor);

            foreach (var element in bindings.Borders)
            {
                element.style.borderTopColor = theme.BorderColor;
                element.style.borderRightColor = theme.BorderColor;
                element.style.borderBottomColor = theme.BorderColor;
                element.style.borderLeftColor = theme.BorderColor;
            }

            foreach (var label in bindings.PrimaryLabels)
            {
                label.style.color = theme.PrimaryTextColor;
            }

            foreach (var label in bindings.SecondaryLabels)
            {
                label.style.color = theme.SecondaryTextColor;
            }

            foreach (var button in bindings.Buttons)
            {
                button.style.backgroundColor = theme.AccentColor;
                button.style.color = theme.BackgroundColor;
                button.style.borderTopColor = theme.OrnamentColor;
                button.style.borderRightColor = theme.OrnamentColor;
                button.style.borderBottomColor = theme.OrnamentColor;
                button.style.borderLeftColor = theme.OrnamentColor;
            }
        }

        private static void SetBackground(IEnumerable<VisualElement> elements, UnityEngine.Color color)
        {
            foreach (var element in elements)
            {
                element.style.backgroundColor = color;
            }
        }
    }
}
