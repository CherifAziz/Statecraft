using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public sealed class LeaderStatView : VisualElement
    {
        private readonly Label valueLabel;
        private readonly VisualElement fill;

        public LeaderStatView(string statName)
        {
            AddToClassList("stat-row");

            var heading = UiFactory.Container("stat-heading");
            heading.Add(UiFactory.Label(statName.ToUpperInvariant(), "stat-name"));
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

        public void SetValue(int value)
        {
            var clampedValue = Mathf.Clamp(value, 0, 100);
            valueLabel.text = clampedValue.ToString();
            fill.style.width = Length.Percent(clampedValue);
        }
    }
}
