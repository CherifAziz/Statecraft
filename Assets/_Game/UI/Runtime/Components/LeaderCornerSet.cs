using System.Collections.Generic;
using Statecraft.UI.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public sealed class LeaderCornerSet : VisualElement
    {
        private readonly List<VisualElement> corners = new();
        private readonly List<VisualElement> fallbackLines = new();

        public LeaderCornerSet(string modifierClass)
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("leader-corner-set");
            AddToClassList(modifierClass);

            AddCorner("top-left");
            AddCorner("top-right");
            AddCorner("bottom-left");
            AddCorner("bottom-right");
        }

        public void ApplyTheme(CountryTheme theme)
        {
            var hasArtwork = theme.LeaderCornerOrnament != null;
            foreach (var corner in corners)
            {
                corner.style.backgroundImage = hasArtwork
                    ? new StyleBackground(theme.LeaderCornerOrnament)
                    : new StyleBackground(StyleKeyword.None);
                corner.EnableInClassList("leader-corner--artwork", hasArtwork);
            }

            foreach (var line in fallbackLines)
            {
                line.style.display = hasArtwork ? DisplayStyle.None : DisplayStyle.Flex;
                line.style.backgroundColor = theme.OrnamentColor;
            }
        }

        private void AddCorner(string position)
        {
            var corner = UiFactory.Container("leader-corner");
            corner.AddToClassList($"leader-corner--{position}");
            corner.pickingMode = PickingMode.Ignore;

            var horizontal = UiFactory.Container("leader-corner-fallback-horizontal");
            var vertical = UiFactory.Container("leader-corner-fallback-vertical");
            horizontal.pickingMode = PickingMode.Ignore;
            vertical.pickingMode = PickingMode.Ignore;
            fallbackLines.Add(horizontal);
            fallbackLines.Add(vertical);
            corner.Add(horizontal);
            corner.Add(vertical);
            corners.Add(corner);
            Add(corner);
        }
    }
}
