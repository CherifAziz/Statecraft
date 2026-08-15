using System;
using Statecraft.UI.Components;
using UnityEngine.UIElements;

namespace Statecraft.UI.Screens
{
    public sealed class BootScreen : VisualElement
    {
        public BootScreen(Action enter)
        {
            name = "boot-screen";
            AddToClassList("screen");
            AddToClassList("boot-screen");

            var frame = UiFactory.Container("boot-frame");
            frame.Add(UiFactory.Label("CABINET // STRATEGIC COMMAND", "eyebrow"));
            frame.Add(UiFactory.Label("STATECRAFT", "boot-title"));
            frame.Add(UiFactory.Label("Le pouvoir est une architecture.", "boot-subtitle"));
            frame.Add(UiFactory.Container("boot-rule"));
            frame.Add(UiFactory.Button("ENTRER", enter, "primary-button"));

            Add(frame);
            Add(UiFactory.Label("PROTOTYPE • FONDATION 01", "build-label"));
        }
    }
}
