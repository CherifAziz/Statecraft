using System.Collections.Generic;
using Statecraft.UI.Components;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Themes
{
    public sealed class CountryThemeBindings
    {
        public VisualElement Root { get; set; }
        public VisualElement BackgroundArtwork { get; set; }
        public VisualElement ForegroundOverlay { get; set; }
        public VisualElement Emblem { get; set; }
        public VisualElement PortraitFrame { get; set; }
        public VisualElement SurfaceTexture { get; set; }
        public VisualElement EditorialSurfaceTexture { get; set; }
        public VerticalGradientElement IdentityGradient { get; set; }
        public VerticalGradientElement EditorialBackdrop { get; set; }
        public IReadOnlyList<VisualElement> Surfaces { get; set; }
        public IReadOnlyList<VisualElement> Accents { get; set; }
        public IReadOnlyList<VisualElement> Ornaments { get; set; }
        public IReadOnlyList<VisualElement> Borders { get; set; }
        public IReadOnlyList<Label> PrimaryLabels { get; set; }
        public IReadOnlyList<Label> SecondaryLabels { get; set; }
        public IReadOnlyList<Label> AccentLabels { get; set; }
        public IReadOnlyList<Label> PrestigeLabels { get; set; }
        public IReadOnlyList<Button> Buttons { get; set; }
        public IReadOnlyList<LeaderSkillCard> SkillCards { get; set; }
    }

    public static class CountryThemeApplicator
    {
        public static void Apply(CountryTheme theme, CountryThemeBindings bindings)
        {
            bindings.Root.style.backgroundColor = theme.BackgroundColor;

            bindings.Root.style.backgroundImage = StyleKeyword.None;
            bindings.BackgroundArtwork.style.backgroundColor = theme.PrimaryColor;
            SetArtwork(bindings.BackgroundArtwork, theme.LeaderBackgroundArtwork);
            SetArtwork(bindings.ForegroundOverlay, theme.LeaderForegroundOverlay);
            SetArtwork(bindings.Emblem, theme.Emblem);
            SetArtwork(bindings.PortraitFrame, theme.PortraitFrame);
            SetTexture(bindings.SurfaceTexture, theme.SurfaceTexture);
            SetTexture(bindings.EditorialSurfaceTexture, theme.SurfaceTexture);
            bindings.IdentityGradient.SetColors(
                WithAlpha(theme.BackgroundColor, 0f),
                WithAlpha(theme.BackgroundColor, 0.64f),
                WithAlpha(theme.BackgroundColor, 0.97f),
                0.46f);
            bindings.EditorialBackdrop.SetColors(
                WithAlpha(theme.BackgroundColor, 0.98f),
                WithAlpha(theme.SurfaceColor, 0.965f));

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

            foreach (var label in bindings.AccentLabels)
            {
                label.style.color = theme.AccentColor;
            }

            foreach (var label in bindings.PrestigeLabels)
            {
                label.style.unityFont = theme.LeaderPrestigeFont != null
                    ? new StyleFont(theme.LeaderPrestigeFont)
                    : new StyleFont(StyleKeyword.Null);
            }

            foreach (var button in bindings.Buttons)
            {
                button.style.backgroundColor = Color.clear;
                button.style.color = theme.PrimaryTextColor;
                button.style.borderTopColor = theme.BorderColor;
                button.style.borderRightColor = theme.BorderColor;
                button.style.borderBottomColor = theme.AccentColor;
                button.style.borderLeftColor = theme.BorderColor;
            }

            foreach (var skillCard in bindings.SkillCards)
            {
                skillCard.ApplyTheme(theme);
            }
        }

        private static void SetArtwork(VisualElement element, Sprite artwork)
        {
            element.style.backgroundImage = artwork != null
                ? new StyleBackground(artwork)
                : new StyleBackground(StyleKeyword.None);
        }

        private static void SetTexture(VisualElement element, Texture2D texture)
        {
            element.style.backgroundImage = texture != null
                ? new StyleBackground(texture)
                : new StyleBackground(StyleKeyword.None);
        }

        private static void SetBackground(IEnumerable<VisualElement> elements, UnityEngine.Color color)
        {
            foreach (var element in elements)
            {
                element.style.backgroundColor = color;
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
