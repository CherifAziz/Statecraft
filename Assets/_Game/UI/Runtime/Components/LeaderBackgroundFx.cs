using Statecraft.UI.Themes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public sealed class LeaderBackgroundFx
    {
        private const long UpdateIntervalMs = 33;
        private const float DriftCycleSeconds = 44f;
        private const float LightCycleSeconds = 18f;
        private const float PointerSmoothing = 3.2f;
        private const float MicroZoom = 0.0025f;

        private readonly VisualElement root;
        private readonly VisualElement background;
        private readonly VisualElement ambientLight;
        private readonly IVisualElementScheduledItem scheduledUpdate;

        private CountryTheme theme;
        private Vector2 pointerOffset;
        private Vector2 pointerTarget;
        private float startedAt;

        public LeaderBackgroundFx(VisualElement root, VisualElement background, VisualElement ambientLight)
        {
            this.root = root;
            this.background = background;
            this.ambientLight = ambientLight;

            root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            root.RegisterCallback<PointerLeaveEvent>(_ => pointerTarget = Vector2.zero);
            root.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                startedAt = Time.unscaledTime;
                if (IsActive)
                {
                    scheduledUpdate.Resume();
                }
            });
            root.RegisterCallback<DetachFromPanelEvent>(_ => scheduledUpdate.Pause());

            scheduledUpdate = root.schedule.Execute(Update).Every(UpdateIntervalMs);
        }

        public void Apply(CountryTheme countryTheme)
        {
            theme = countryTheme;
            ambientLight.style.backgroundColor = countryTheme.AccentColor;
            startedAt = Time.unscaledTime;
            pointerOffset = Vector2.zero;
            pointerTarget = Vector2.zero;

            if (!IsActive)
            {
                scheduledUpdate.Pause();
                ResetVisuals();
                return;
            }

            scheduledUpdate.Resume();
        }

        private bool IsActive => theme != null
            && theme.LeaderBackgroundArtwork != null
            && theme.LeaderBackgroundFxEnabled;

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!IsActive || root.worldBound.width <= 0f || root.worldBound.height <= 0f)
            {
                return;
            }

            var bounds = root.worldBound;
            var normalizedX = Mathf.Clamp((evt.position.x - bounds.center.x) / (bounds.width * 0.5f), -1f, 1f);
            var normalizedY = Mathf.Clamp((evt.position.y - bounds.center.y) / (bounds.height * 0.5f), -1f, 1f);
            pointerTarget = new Vector2(-normalizedX, -normalizedY) * theme.LeaderParallaxStrength;
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            var deltaTime = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            var smoothing = 1f - Mathf.Exp(-PointerSmoothing * deltaTime);
            pointerOffset = Vector2.Lerp(pointerOffset, pointerTarget, smoothing);

            var elapsed = Time.unscaledTime - startedAt;
            var driftPhase = elapsed * Mathf.PI * 2f / DriftCycleSeconds;
            var drift = new Vector2(
                Mathf.Sin(driftPhase),
                Mathf.Cos(driftPhase * 0.73f) * 0.45f) * theme.LeaderDriftStrength;
            var movement = pointerOffset + drift;

            var zoomPhase = (Mathf.Sin(driftPhase * 0.5f) + 1f) * 0.5f;
            var driftRatio = Mathf.Clamp01(theme.LeaderDriftStrength / 2f);
            var zoom = 1f + zoomPhase * MicroZoom * driftRatio;
            background.style.translate = new Translate(movement.x, movement.y);
            background.style.scale = new Scale(new Vector3(zoom, zoom, 1f));

            var lightPhase = (Mathf.Sin(elapsed * Mathf.PI * 2f / LightCycleSeconds) + 1f) * 0.5f;
            ambientLight.style.opacity = 0.008f + lightPhase * theme.LeaderLightBreathingStrength;
        }

        private void ResetVisuals()
        {
            background.style.translate = new Translate(0f, 0f);
            background.style.scale = new Scale(Vector3.one);
            ambientLight.style.opacity = 0f;
        }
    }
}
