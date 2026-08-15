using System;
using Statecraft.UI.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public sealed class LeaderSkillCardData
    {
        public LeaderSkillCardData(
            string id,
            string displayName,
            string type,
            string shortDescription,
            Sprite artwork = null,
            bool isLocked = false)
        {
            Id = id;
            DisplayName = displayName;
            Type = type;
            ShortDescription = shortDescription;
            Artwork = artwork;
            IsLocked = isLocked;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Type { get; }
        public string ShortDescription { get; }
        public Sprite Artwork { get; }
        public bool IsLocked { get; }
    }

    public sealed class LeaderSkillCard : Button
    {
        private readonly VisualElement artwork;
        private readonly VisualElement accentLine;
        private readonly VerticalGradientElement artworkShade;
        private readonly Label artworkPlaceholder;
        private readonly Label typeLabel;
        private readonly Label nameLabel;
        private readonly Label descriptionLabel;
        private readonly Label stateLabel;
        private CountryTheme theme;
        private bool isSelected;
        private bool isHovered;

        public LeaderSkillCard()
        {
            AddToClassList("leader-skill-card");

            artwork = UiFactory.Container("skill-artwork");
            artworkPlaceholder = UiFactory.Label("◇", "skill-artwork-placeholder");
            artworkShade = new VerticalGradientElement("skill-artwork-shade");
            artwork.Add(artworkPlaceholder);

            var artworkCopy = UiFactory.Container("skill-artwork-copy");
            nameLabel = UiFactory.Label("Compétence", "skill-card-name");
            var header = UiFactory.Container("skill-card-header");
            typeLabel = UiFactory.Label("TYPE", "skill-type");
            stateLabel = UiFactory.Label("OUVERT", "skill-state");
            header.Add(typeLabel);
            header.Add(stateLabel);
            artworkCopy.Add(nameLabel);
            artworkCopy.Add(header);
            artwork.Add(artworkShade);
            artwork.Add(artworkCopy);

            descriptionLabel = UiFactory.Label("Description", "skill-description");
            accentLine = UiFactory.Container("skill-accent-line");

            Add(artwork);
            Add(descriptionLabel);
            Add(accentLine);

            clicked += HandleClicked;
            RegisterCallback<PointerEnterEvent>(_ => SetHovered(true));
            RegisterCallback<PointerLeaveEvent>(_ => SetHovered(false));
        }

        public event Action<LeaderSkillCard> Selected;
        public string SkillId { get; private set; }
        public bool IsLocked { get; private set; }

        public void Bind(LeaderSkillCardData data)
        {
            SkillId = data.Id;
            IsLocked = data.IsLocked;
            typeLabel.text = data.Type.ToUpperInvariant();
            nameLabel.text = data.DisplayName.ToUpperInvariant();
            descriptionLabel.text = data.ShortDescription;
            stateLabel.text = IsLocked ? "◆  VERROUILLÉ" : "OUVERT";
            tooltip = data.ShortDescription;
            EnableInClassList("leader-skill-card--locked", IsLocked);

            if (data.Artwork != null)
            {
                artwork.style.backgroundImage = new StyleBackground(data.Artwork);
                artworkPlaceholder.style.display = DisplayStyle.None;
            }
            else
            {
                artwork.style.backgroundImage = StyleKeyword.None;
                artworkPlaceholder.style.display = DisplayStyle.Flex;
            }

            SetSelected(false);
        }

        public void ApplyTheme(CountryTheme countryTheme)
        {
            theme = countryTheme;
            typeLabel.style.color = theme.SecondaryTextColor;
            nameLabel.style.color = theme.PrimaryTextColor;
            descriptionLabel.style.color = theme.SecondaryTextColor;
            stateLabel.style.color = IsLocked ? theme.SecondaryTextColor : theme.AccentColor;
            artwork.style.backgroundColor = theme.PrimaryColor;
            artworkPlaceholder.style.color = theme.OrnamentColor;
            artworkShade.SetColors(Color.clear, WithAlpha(theme.BackgroundColor, 0.96f));
            RefreshVisualState();
        }

        public void SetSelected(bool selected)
        {
            isSelected = selected && !IsLocked;
            EnableInClassList("leader-skill-card--selected", isSelected);
            RefreshVisualState();
        }

        private void HandleClicked()
        {
            if (!IsLocked)
            {
                Selected?.Invoke(this);
            }
        }

        private void SetHovered(bool hovered)
        {
            isHovered = hovered && !IsLocked;
            EnableInClassList("leader-skill-card--hovered", isHovered);
            RefreshVisualState();
        }

        private void RefreshVisualState()
        {
            if (theme == null)
            {
                return;
            }

            var emphasized = isSelected || isHovered;
            var border = emphasized
                ? WithAlpha(theme.AccentColor, isSelected ? 1f : 0.82f)
                : WithAlpha(theme.BorderColor, 0.48f);
            style.borderTopColor = border;
            style.borderRightColor = border;
            style.borderBottomColor = border;
            style.borderLeftColor = border;
            var surface = emphasized
                ? Color.Lerp(theme.SurfaceColor, theme.AccentColor, isSelected ? 0.12f : 0.06f)
                : theme.SurfaceColor;
            style.backgroundColor = WithAlpha(surface, isSelected ? 0.78f : isHovered ? 0.68f : 0.5f);
            accentLine.style.backgroundColor = emphasized ? theme.AccentColor : theme.OrnamentColor;
            artwork.style.unityBackgroundImageTintColor = IsLocked
                ? new Color(0.54f, 0.54f, 0.54f, 1f)
                : emphasized ? Color.white : new Color(0.91f, 0.91f, 0.91f, 1f);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
