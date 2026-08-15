using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public sealed class LeaderStatView : VisualElement
    {
        private readonly Label valueLabel;
        private readonly Label nameLabel;
        private readonly Label iconLabel;
        private readonly VisualElement fill;
        private readonly VisualElement iconFrame;
        private readonly VisualElement marker;

        public LeaderStatView(string statName, string icon)
        {
            AddToClassList("stat-row");

            iconFrame = UiFactory.Container("stat-icon-frame");
            iconLabel = UiFactory.Label(icon, "stat-icon");
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

        public void SetValue(int value)
        {
            var clampedValue = Mathf.Clamp(value, 0, 100);
            valueLabel.text = clampedValue.ToString();
            fill.style.width = Length.Percent(clampedValue);
        }
    }
}
