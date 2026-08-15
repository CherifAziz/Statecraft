using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public static class UiFactory
    {
        public static Label Label(string text, string className = null)
        {
            var label = new Label(text);
            if (!string.IsNullOrWhiteSpace(className))
            {
                label.AddToClassList(className);
            }

            return label;
        }

        public static VisualElement Container(string className)
        {
            var element = new VisualElement();
            element.AddToClassList(className);
            return element;
        }

        public static Button Button(string text, System.Action clicked, string className = null)
        {
            var button = new Button(clicked) { text = text };
            if (!string.IsNullOrWhiteSpace(className))
            {
                button.AddToClassList(className);
            }

            return button;
        }
    }
}
