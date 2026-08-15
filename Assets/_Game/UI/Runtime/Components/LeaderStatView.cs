using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public sealed class LeaderStatView : VisualElement
    {
        private readonly Label valueLabel;
        private readonly Label nameLabel;
        private readonly VisualElement fill;

        public LeaderStatView(string statName)
        {
            AddToClassList("stat-row");

            var heading = UiFactory.Container("stat-heading");
            nameLabel = UiFactory.Label(statName.ToUpperInvariant(), "stat-name");
            heading.Add(nameLabel);
            valueLabel = UiFactory.Label("0", "stat-value");
            heading.Add(valueLabel);

            var track = UiFactory.Container("stat-track");
            fill = UiFactory.Container("stat-fill");
            track.Add(fill);

            Add(heading);
            Add(track);
        }

        public VisualElement Fill => fill;
        public Label ValueLabel => valueLabel;
        public Label NameLabel => nameLabel;

        public void SetValue(int value)
        {
            var clampedValue = Mathf.Clamp(value, 0, 100);
            valueLabel.text = clampedValue.ToString();
            fill.style.width = Length.Percent(clampedValue);
        }
    }
}
