using RoR2;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2Maze.Vision
{
    /// <summary>
    /// Per-player component that tracks lantern fuel and drives two visual effects
    /// for the local player only:
    ///   1. A warm Point Light whose range shrinks with fuel (visible "lantern glow").
    ///   2. A full-screen red overlay that pulses when fuel drops below 30 %
    ///      (low-fuel warning — does not require a custom shader).
    ///
    /// A proper radial-vignette effect (dark everywhere except VisionRadius around the
    /// player) requires a custom Thunderkit shader and will replace these placeholders
    /// once the AssetBundle pipeline is set up.
    /// </summary>
    public sealed class LanternBehaviour : MonoBehaviour
    {
        /// <summary>Current fuel in [0, 1]. 1 = full, 0 = empty.</summary>
        public float Fuel { get; private set; } = 1f;

        /// <summary>World-space radius the player can "see" based on current fuel.</summary>
        public float VisionRadius =>
            Mathf.Lerp(LanternSystem.MinVisionRadius, LanternSystem.MaxVisionRadius, Fuel);

        private Light? _light;
        private GameObject? _canvasGo;
        private Image? _overlay;

        private void Start()
        {
            // Only create visuals for the local player's body.
            // Util.LookUpBodyNetworkUser returns the NetworkUser owning this body;
            // localUser is non-null only on the client that owns that user.
            var body = GetComponent<CharacterBody>();
            bool isLocal = body != null && Util.LookUpBodyNetworkUser(body)?.localUser != null;
            Log.Info($"[LanternBehaviour] Start: isLocal={isLocal} ({gameObject.name})");
            if (!isLocal) return;

            // ── Point light ──────────────────────────────────────────────────────
            var lightGo = new GameObject("LanternLight");
            lightGo.transform.SetParent(transform, worldPositionStays: false);
            lightGo.transform.localPosition = new Vector3(0f, 1.5f, 0f);

            _light = lightGo.AddComponent<Light>();
            _light.type      = LightType.Point;
            _light.color     = new Color(1f, 0.82f, 0.45f);   // warm amber
            _light.intensity = 2.5f;
            _light.range     = VisionRadius;

            // ── Screen-space warning overlay ─────────────────────────────────────
            _canvasGo = new GameObject("LanternCanvas");

            var canvas = _canvasGo.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            var overlayGo = new GameObject("WarningOverlay");
            overlayGo.transform.SetParent(_canvasGo.transform, worldPositionStays: false);

            _overlay = overlayGo.AddComponent<Image>();
            _overlay.color = Color.clear;
            _overlay.raycastTarget = false;

            var rt = (RectTransform)overlayGo.transform;
            rt.anchorMin  = Vector2.zero;
            rt.anchorMax  = Vector2.one;
            rt.offsetMin  = Vector2.zero;
            rt.offsetMax  = Vector2.zero;
        }

        private void Update()
        {
            Fuel = Mathf.Clamp01(Fuel - LanternSystem.DrainPerSecond * Time.deltaTime);

            if (_light != null)
                _light.range = VisionRadius;

            if (_overlay != null)
            {
                // Pulse a dim red warning when fuel is below 30 %.
                // Intensity scales up as fuel approaches 0.
                float warningStrength = Fuel < 0.3f
                    ? Mathf.Pow(1f - Fuel / 0.3f, 1.5f)
                    : 0f;
                float pulse = warningStrength > 0f
                    ? Mathf.Abs(Mathf.Sin(Time.time * 3f)) * warningStrength * 0.35f
                    : 0f;
                _overlay.color = new Color(0.7f, 0f, 0f, pulse);
            }
        }

        /// <summary>Add fuel from a wall station or pickup.</summary>
        public void Refuel(float amount)
        {
            Fuel = Mathf.Clamp01(Fuel + amount);
            Log.Info($"[LanternBehaviour] Refuelled +{amount:P0}. Fuel now {Fuel:P0}.");
        }

        private void OnDestroy()
        {
            if (_canvasGo != null)
                Destroy(_canvasGo);
        }
    }
}
