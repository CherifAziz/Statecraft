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
        private readonly Label artworkPlaceholder;
        private readonly VisualElement lockIndicator;
        private readonly VisualElement lockShackle;
        private readonly VisualElement lockBody;
        private readonly Label typeLabel;
        private readonly Label nameLabel;
        private readonly Label descriptionLabel;
        private readonly Label stateLabel;
        private CountryTheme theme;
        private StatecraftTypography typography;
        private bool isSelected;
        private bool isHovered;

        public LeaderSkillCard()
        {
            AddToClassList("leader-skill-card");

            artwork = UiFactory.Container("skill-artwork");
            artworkPlaceholder = UiFactory.Label("◇", "skill-artwork-placeholder");
            artwork.Add(artworkPlaceholder);

            lockIndicator = UiFactory.Container("skill-lock-indicator");
            lockShackle = UiFactory.Container("skill-lock-shackle");
            lockBody = UiFactory.Container("skill-lock-body");
            lockIndicator.Add(lockShackle);
            lockIndicator.Add(lockBody);
            artwork.Add(lockIndicator);

            var body = UiFactory.Container("skill-card-body");
            nameLabel = UiFactory.Label("Compétence", "skill-card-name");
            var header = UiFactory.Container("skill-card-header");
            typeLabel = UiFactory.Label("TYPE", "skill-type");
            stateLabel = UiFactory.Label("OUVERT", "skill-state");
            header.Add(typeLabel);
            header.Add(stateLabel);

            descriptionLabel = UiFactory.Label("Description", "skill-description");
            var spacer = UiFactory.Container("skill-card-spacer");
            accentLine = UiFactory.Container("skill-accent-line");
            body.Add(nameLabel);
            body.Add(header);
            body.Add(descriptionLabel);
            body.Add(spacer);
            body.Add(accentLine);

            Add(artwork);
            Add(body);

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
            stateLabel.text = IsLocked ? "VERROUILLÉ" : "OUVERT";
            tooltip = data.ShortDescription;
            EnableInClassList("leader-skill-card--locked", IsLocked);
            lockIndicator.style.display = IsLocked ? DisplayStyle.Flex : DisplayStyle.None;

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
            lockIndicator.style.borderTopColor = theme.AccentColor;
            lockIndicator.style.borderRightColor = theme.AccentColor;
            lockIndicator.style.borderBottomColor = theme.AccentColor;
            lockIndicator.style.borderLeftColor = theme.AccentColor;
            lockShackle.style.borderTopColor = theme.AccentColor;
            lockShackle.style.borderRightColor = theme.AccentColor;
            lockShackle.style.borderLeftColor = theme.AccentColor;
            lockBody.style.backgroundColor = theme.AccentColor;
            ApplyPrestigeFont(theme.LeaderPrestigeFont ?? typography?.UtilityMedium);
            RefreshVisualState();
        }

        public void ApplyTypography(StatecraftTypography statecraftTypography)
        {
            typography = statecraftTypography;
            if (typography == null)
            {
                return;
            }

            ApplyFont(typeLabel, typography.UtilitySemibold);
            ApplyFont(stateLabel, typography.UtilitySemibold);
            ApplyFont(descriptionLabel, typography.UtilityRegular);
            ApplyPrestigeFont(theme?.LeaderPrestigeFont ?? typography.UtilityMedium);
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
                ? Color.Lerp(theme.BackgroundColor, theme.AccentColor, isSelected ? 0.07f : 0.025f)
                : theme.BackgroundColor;
            style.backgroundColor = WithAlpha(surface, isSelected ? 0.94f : isHovered ? 0.82f : 0.74f);
            accentLine.style.backgroundColor = emphasized ? theme.AccentColor : theme.OrnamentColor;
            artwork.style.unityBackgroundImageTintColor = IsLocked
                ? new Color(0.48f, 0.48f, 0.48f, 1f)
                : emphasized ? Color.white : new Color(0.96f, 0.96f, 0.96f, 1f);
        }

        private void ApplyPrestigeFont(Font font)
        {
            nameLabel.style.unityFont = font != null
                ? new StyleFont(font)
                : new StyleFont(StyleKeyword.Null);
        }

        private static void ApplyFont(Label label, Font font)
        {
            label.style.unityFont = font != null
                ? new StyleFont(font)
                : new StyleFont(StyleKeyword.Null);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
