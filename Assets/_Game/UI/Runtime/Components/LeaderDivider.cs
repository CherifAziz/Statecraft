using Statecraft.UI.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public sealed class LeaderDivider : VisualElement
    {
        private readonly VisualElement leftLine;
        private readonly VisualElement artwork;
        private readonly VisualElement fallback;
        private readonly VisualElement rightLine;

        public LeaderDivider(string modifierClass)
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("leader-divider");
            AddToClassList(modifierClass);

            leftLine = UiFactory.Container("leader-divider-line");
            artwork = UiFactory.Container("leader-divider-artwork");
            fallback = UiFactory.Container("leader-divider-fallback");
            rightLine = UiFactory.Container("leader-divider-line");

            leftLine.pickingMode = PickingMode.Ignore;
            artwork.pickingMode = PickingMode.Ignore;
            fallback.pickingMode = PickingMode.Ignore;
            rightLine.pickingMode = PickingMode.Ignore;

            Add(leftLine);
            Add(artwork);
            Add(fallback);
            Add(rightLine);
        }

        public void ApplyTheme(Sprite ornament, CountryTheme theme)
        {
            leftLine.style.backgroundColor = theme.AccentColor;
            rightLine.style.backgroundColor = theme.AccentColor;
            fallback.style.backgroundColor = theme.OrnamentColor;

            if (ornament != null)
            {
                artwork.style.backgroundImage = new StyleBackground(ornament);
                artwork.style.display = DisplayStyle.Flex;
                fallback.style.display = DisplayStyle.None;
            }
            else
            {
                artwork.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                artwork.style.display = DisplayStyle.None;
                fallback.style.display = DisplayStyle.Flex;
            }
        }
    }
}
