using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Statecraft.UI.Themes
{
    [Serializable]
    public sealed class LeaderSkillArtworkBinding
    {
        [SerializeField] private string skillId = string.Empty;
        [SerializeField] private Sprite artwork = null;

        public string SkillId => skillId;
        public Sprite Artwork => artwork;
    }

    [CreateAssetMenu(fileName = "CountryTheme", menuName = "Statecraft/UI/Country Theme")]
    public sealed class CountryTheme : ScriptableObject
    {
        [Header("Palette")]
        [SerializeField] private Color primaryColor;
        [SerializeField] private Color secondaryColor;
        [SerializeField] private Color accentColor;
        [SerializeField] private Color backgroundColor;
        [SerializeField] private Color surfaceColor;
        [SerializeField] private Color primaryTextColor;
        [SerializeField] private Color secondaryTextColor;
        [SerializeField] private Color borderColor;
        [SerializeField] private Color ornamentColor;

        [Header("Optional Leader Screen art layers")]
        [FormerlySerializedAs("backgroundArtwork")]
        [SerializeField] private Sprite leaderBackgroundArtwork = null;
        [FormerlySerializedAs("leaderScreenArtwork")]
        [SerializeField] private Sprite leaderForegroundOverlay = null;
        [SerializeField] private Sprite emblem = null;
        [SerializeField] private Sprite portraitFrame = null;
        [SerializeField] private Texture2D surfaceTexture = null;

        [Header("Optional Leader Skill art")]
        [SerializeField] private LeaderSkillArtworkBinding[] leaderSkillArtworks = Array.Empty<LeaderSkillArtworkBinding>();

        [Header("Optional Leader Background FX")]
        [SerializeField] private bool leaderBackgroundFxEnabled = false;
        [Range(0f, 6f)]
        [SerializeField] private float leaderParallaxStrength = 0f;
        [Range(0f, 4f)]
        [SerializeField] private float leaderDriftStrength = 0f;
        [Range(0f, 0.05f)]
        [SerializeField] private float leaderLightBreathingStrength = 0f;

        [Header("Identity")]
        [SerializeField] private string visualIdentifier;
        [SerializeField] private string transitionStyleIdentifier;

        [Header("Reserved audio hooks")]
        [SerializeField] private AudioClip ambience = null;
        [SerializeField] private AudioClip transitionSound = null;

        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public Color AccentColor => accentColor;
        public Color BackgroundColor => backgroundColor;
        public Color SurfaceColor => surfaceColor;
        public Color PrimaryTextColor => primaryTextColor;
        public Color SecondaryTextColor => secondaryTextColor;
        public Color BorderColor => borderColor;
        public Color OrnamentColor => ornamentColor;
        public Sprite LeaderBackgroundArtwork => leaderBackgroundArtwork;
        public Sprite LeaderForegroundOverlay => leaderForegroundOverlay;
        public Sprite Emblem => emblem;
        public Sprite PortraitFrame => portraitFrame;
        public Texture2D SurfaceTexture => surfaceTexture;
        public bool LeaderBackgroundFxEnabled => leaderBackgroundFxEnabled;
        public float LeaderParallaxStrength => leaderParallaxStrength;
        public float LeaderDriftStrength => leaderDriftStrength;
        public float LeaderLightBreathingStrength => leaderLightBreathingStrength;
        public string VisualIdentifier => visualIdentifier;
        public string TransitionStyleIdentifier => transitionStyleIdentifier;
        public AudioClip Ambience => ambience;
        public AudioClip TransitionSound => transitionSound;

        public Sprite GetLeaderSkillArtwork(string skillId)
        {
            if (leaderSkillArtworks == null)
            {
                return null;
            }

            foreach (var binding in leaderSkillArtworks)
            {
                if (binding != null && string.Equals(binding.SkillId, skillId, StringComparison.Ordinal))
                {
                    return binding.Artwork;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        public void Configure(
            Color primary,
            Color secondary,
            Color accent,
            Color background,
            Color surface,
            Color primaryText,
            Color secondaryText,
            Color border,
            Color ornament,
            string identifier,
            string transitionIdentifier)
        {
            primaryColor = primary;
            secondaryColor = secondary;
            accentColor = accent;
            backgroundColor = background;
            surfaceColor = surface;
            primaryTextColor = primaryText;
            secondaryTextColor = secondaryText;
            borderColor = border;
            ornamentColor = ornament;
            visualIdentifier = identifier;
            transitionStyleIdentifier = transitionIdentifier;
        }
#endif
    }
}
