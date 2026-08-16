using UnityEngine;
using UnityEngine.UIElements;
using Statecraft.UI.Themes;

namespace Statecraft.UI.Components
{
    public sealed class LeaderStatView : VisualElement
    {
        private readonly Label valueLabel;
        private readonly Label nameLabel;
        private readonly Label iconLabel;
        private readonly VisualElement iconArtwork;
        private readonly VisualElement fill;
        private readonly VisualElement iconFrame;
        private readonly VisualElement marker;

        public LeaderStatView(string statName, string icon)
        {
            AddToClassList("stat-row");

            iconFrame = UiFactory.Container("stat-icon-frame");
            iconArtwork = UiFactory.Container("stat-icon-artwork");
            iconLabel = UiFactory.Label(icon, "stat-icon");
            iconFrame.Add(iconArtwork);
            iconFrame.Add(iconLabel);

            var body = UiFactory.Container("stat-body");
            var heading = UiFactory.Container("stat-heading");
            nameLabel = UiFactory.Label(statName.ToUpperInvariant(), "stat-name");
            heading.Add(nameLabel);
            valueLabel = UiFactory.Label("0", "stat-value");
            heading.Add(valueLabel);

            var track = UiFactory.Container("stat-track");
            fill = UiFactory.Container("stat-fill");
            marker = UiFactory.Container("stat-marker");
            fill.Add(marker);
            track.Add(fill);

            body.Add(heading);
            body.Add(track);
            Add(iconFrame);
            Add(body);
        }

        public VisualElement Fill => fill;
        public Label ValueLabel => valueLabel;
        public Label NameLabel => nameLabel;
        public Label IconLabel => iconLabel;
        public VisualElement IconFrame => iconFrame;
        public VisualElement Marker => marker;

        public void SetArtwork(Sprite artwork)
        {
            var hasArtwork = artwork != null;
            iconArtwork.style.backgroundImage = hasArtwork
                ? new StyleBackground(artwork)
                : new StyleBackground(StyleKeyword.None);
            iconArtwork.style.display = hasArtwork ? DisplayStyle.Flex : DisplayStyle.None;
            iconLabel.style.display = hasArtwork ? DisplayStyle.None : DisplayStyle.Flex;
            iconFrame.EnableInClassList("stat-icon-frame--artwork", hasArtwork);
        }

        public void ApplyTypography(StatecraftTypography typography)
        {
            if (typography == null)
            {
                return;
            }

            SetFont(nameLabel, typography.UtilityMedium);
            SetFont(valueLabel, typography.UtilitySemibold);
            SetFont(iconLabel, typography.UtilitySemibold);
        }

        public void SetValue(int value)
        {
            var clampedValue = Mathf.Clamp(value, 0, 100);
            valueLabel.text = clampedValue.ToString();
            fill.style.width = Length.Percent(clampedValue);
        }

        private static void SetFont(Label label, Font font)
        {
            label.style.unityFont = font != null
                ? new StyleFont(font)
                : new StyleFont(StyleKeyword.Null);
        }
    }
}
