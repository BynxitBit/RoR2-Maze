using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2Maze.StarMap
{
    /// <summary>
    /// Runtime-built HUD overlay shown briefly at maze-stage start.
    /// Fades in, holds for the configured duration, then fades out and self-destructs.
    ///
    /// ALPHA: displays placeholder text. Will be replaced with the actual waypoint
    /// graph once MazeGenerator produces path data.
    /// </summary>
    internal sealed class StarMapHud : MonoBehaviour
    {
        private const float FadeDuration = 0.6f;

        private float _holdDuration;
        private Image? _panel;
        private Text? _title;
        private Text? _body;

        /// <summary>Spawn the overlay and begin its lifetime coroutine.</summary>
        internal static void Show(float totalDuration)
        {
            var go = new GameObject("StarMapHud");
            var hud = go.AddComponent<StarMapHud>();
            hud._holdDuration = Mathf.Max(0f, totalDuration - FadeDuration * 2f);
        }

        private void Awake()
        {
            // ── Canvas ───────────────────────────────────────────────────────────
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            gameObject.AddComponent<CanvasScaler>();

            // ── Dark panel (centred, 60 % of screen) ────────────────────────────
            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(transform, worldPositionStays: false);
            _panel = panelGo.AddComponent<Image>();
            _panel.color       = new Color(0f, 0f, 0.06f, 0f);
            _panel.raycastTarget = false;

            var panelRt = (RectTransform)panelGo.transform;
            panelRt.anchorMin = new Vector2(0.2f, 0.25f);
            panelRt.anchorMax = new Vector2(0.8f, 0.75f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            // ── Title ────────────────────────────────────────────────────────────
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, worldPositionStays: false);
            _title = titleGo.AddComponent<Text>();
            _title.text      = "★  STAR MAP  ★";
            _title.font      = GetDefaultFont();
            _title.fontSize  = 26;
            _title.fontStyle = FontStyle.Bold;
            _title.alignment = TextAnchor.UpperCenter;
            _title.color     = new Color(0.95f, 0.88f, 0.4f, 0f);   // amber
            _title.raycastTarget = false;

            var titleRt = (RectTransform)titleGo.transform;
            titleRt.anchorMin = new Vector2(0f, 0.65f);
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = new Vector2(12f, 0f);
            titleRt.offsetMax = new Vector2(-12f, -8f);

            // ── Body ─────────────────────────────────────────────────────────────
            var bodyGo = new GameObject("Body");
            bodyGo.transform.SetParent(panelGo.transform, worldPositionStays: false);
            _body = bodyGo.AddComponent<Text>();
            _body.text = "Study the path carefully.\n\nThe exit lies ahead — remember the way.";
            _body.font      = GetDefaultFont();
            _body.fontSize  = 16;
            _body.alignment = TextAnchor.MiddleCenter;
            _body.color     = new Color(0.85f, 0.85f, 0.85f, 0f);
            _body.raycastTarget = false;

            var bodyRt = (RectTransform)bodyGo.transform;
            bodyRt.anchorMin = Vector2.zero;
            bodyRt.anchorMax = new Vector2(1f, 0.65f);
            bodyRt.offsetMin = new Vector2(12f, 12f);
            bodyRt.offsetMax = new Vector2(-12f, 0f);

            StartCoroutine(Lifetime());
        }

        private IEnumerator Lifetime()
        {
            yield return Fade(0f, 1f, FadeDuration);
            yield return new WaitForSeconds(_holdDuration);
            yield return Fade(1f, 0f, FadeDuration);
            Destroy(gameObject);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }
            SetAlpha(to);
        }

        private void SetAlpha(float a)
        {
            if (_panel != null)
                _panel.color = WithAlpha(_panel.color, a * 0.88f);
            if (_title != null)
                _title.color = WithAlpha(_title.color, a);
            if (_body != null)
                _body.color = WithAlpha(_body.color, a);
        }

        private static Color WithAlpha(Color c, float a) =>
            new Color(c.r, c.g, c.b, a);

        private static Font GetDefaultFont() =>
            Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Liberation Sans", "DejaVu Sans", "Helvetica" }, 16);
    }
}
