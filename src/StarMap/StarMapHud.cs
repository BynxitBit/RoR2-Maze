using System.Collections;
using System.Collections.Generic;
using RoR2Maze.Maze;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2Maze.StarMap
{
    /// <summary>
    /// Screen-space HUD overlay that renders the current maze as a pixel-art map.
    /// Fades in, holds, then fades out and self-destructs.
    ///
    /// Colour legend:
    ///   Dark blue-grey = walls
    ///   Mid grey       = open floor
    ///   Amber/gold     = solution path
    ///   Green          = player start
    ///   Red            = exit
    /// </summary>
    internal sealed class StarMapHud : MonoBehaviour
    {
        private const float FadeDuration = 0.6f;
        private const int   CellPx      = 7;   // interior pixels per cell

        // Colours
        private static readonly Color ColWall     = new Color(0.12f, 0.12f, 0.22f, 1f);
        private static readonly Color ColFloor    = new Color(0.38f, 0.38f, 0.50f, 1f);
        private static readonly Color ColPath     = new Color(0.92f, 0.80f, 0.28f, 1f);
        private static readonly Color ColStart    = new Color(0.25f, 0.85f, 0.30f, 1f);
        private static readonly Color ColExit     = new Color(0.90f, 0.28f, 0.28f, 1f);

        private float      _holdDuration;
        private MazeGrid?  _grid;
        private Image?     _backdrop;
        private RawImage?  _mapImage;
        private Text?      _title;

        internal static void Show(float totalDuration, MazeGrid grid)
        {
            var go  = new GameObject("StarMapHud");
            var hud = go.AddComponent<StarMapHud>();
            hud._holdDuration = Mathf.Max(0f, totalDuration - FadeDuration * 2f);
            hud._grid         = grid;
        }

        private void Start()
        {
            // ── Canvas ───────────────────────────────────────────────────────────
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            gameObject.AddComponent<CanvasScaler>();

            // ── Dark backdrop panel ──────────────────────────────────────────────
            var panelGo = new GameObject("Panel");
            panelGo.transform.SetParent(transform, worldPositionStays: false);
            _backdrop = panelGo.AddComponent<Image>();
            _backdrop.color        = new Color(0f, 0f, 0.06f, 0f);
            _backdrop.raycastTarget = false;

            var panelRt = (RectTransform)panelGo.transform;
            panelRt.anchorMin = new Vector2(0.15f, 0.15f);
            panelRt.anchorMax = new Vector2(0.85f, 0.85f);
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            // ── Title ────────────────────────────────────────────────────────────
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, worldPositionStays: false);
            _title = titleGo.AddComponent<Text>();
            _title.text      = "★  STAR MAP  ★";
            _title.font      = Font.CreateDynamicFontFromOSFont(
                                   new[] { "Arial", "Liberation Sans", "DejaVu Sans" }, 22);
            _title.fontSize  = 22;
            _title.fontStyle = FontStyle.Bold;
            _title.alignment = TextAnchor.UpperCenter;
            _title.color     = new Color(0.95f, 0.88f, 0.4f, 0f);
            _title.raycastTarget = false;

            var titleRt = (RectTransform)titleGo.transform;
            titleRt.anchorMin = new Vector2(0f, 0.82f);
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = Vector2.zero;
            titleRt.offsetMax = Vector2.zero;

            // ── Maze map ─────────────────────────────────────────────────────────
            var mapGo = new GameObject("MazeMap");
            mapGo.transform.SetParent(panelGo.transform, worldPositionStays: false);
            _mapImage = mapGo.AddComponent<RawImage>();
            _mapImage.color        = new Color(1f, 1f, 1f, 0f);
            _mapImage.raycastTarget = false;

            var mapRt = (RectTransform)mapGo.transform;
            mapRt.anchorMin = new Vector2(0.05f, 0.05f);
            mapRt.anchorMax = new Vector2(0.95f, 0.80f);
            mapRt.offsetMin = Vector2.zero;
            mapRt.offsetMax = Vector2.zero;

            if (_grid != null)
                _mapImage.texture = BuildTexture(_grid);

            StartCoroutine(Lifetime());
        }

        // ── Texture generation ───────────────────────────────────────────────────

        private static Texture2D BuildTexture(MazeGrid grid)
        {
            int stride = CellPx + 1;                          // pixels per cell + 1 wall pixel
            int texW   = grid.Width  * stride + 1;
            int texH   = grid.Height * stride + 1;

            var pixels = new Color[texW * texH];

            // Fill everything with wall colour.
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = ColWall;

            // Mark which cells are on the solution path for quick lookup.
            var onPath = new HashSet<(int, int)>(grid.Solution);

            // Paint each cell's interior and its open passages.
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    Color cellCol = onPath.Contains((x, y)) ? ColPath : ColFloor;
                    if ((x, y) == grid.Start) cellCol = ColStart;
                    if ((x, y) == grid.Exit)  cellCol = ColExit;

                    // Interior pixels of this cell.
                    int px0 = x * stride + 1;
                    int py0 = y * stride + 1;
                    FillRect(pixels, texW, px0, py0, CellPx, CellPx, cellCol);

                    // Open passage to East neighbour.
                    if ((grid.Walls[x, y] & Wall.East) == 0)
                    {
                        Color passCol = (onPath.Contains((x, y)) && onPath.Contains((x + 1, y)))
                            ? ColPath : ColFloor;
                        FillRect(pixels, texW, px0 + CellPx, py0, 1, CellPx, passCol);
                    }

                    // Open passage to North neighbour.
                    if ((grid.Walls[x, y] & Wall.North) == 0)
                    {
                        Color passCol = (onPath.Contains((x, y)) && onPath.Contains((x, y + 1)))
                            ? ColPath : ColFloor;
                        FillRect(pixels, texW, px0, py0 + CellPx, CellPx, 1, passCol);
                    }
                }
            }

            var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, mipChain: false);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static void FillRect(Color[] pixels, int texW,
            int x, int y, int w, int h, Color col)
        {
            for (int dy = 0; dy < h; dy++)
                for (int dx = 0; dx < w; dx++)
                    pixels[(y + dy) * texW + (x + dx)] = col;
        }

        // ── Fade lifecycle ───────────────────────────────────────────────────────

        private IEnumerator Lifetime()
        {
            yield return Fade(0f, 1f, FadeDuration);
            yield return new WaitForSeconds(_holdDuration);
            yield return Fade(1f, 0f, FadeDuration);
            Destroy(gameObject);
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(t / duration)));
                yield return null;
            }
            SetAlpha(to);
        }

        private void SetAlpha(float a)
        {
            if (_backdrop  != null) _backdrop.color  = WithAlpha(_backdrop.color,  a * 0.90f);
            if (_title     != null) _title.color     = WithAlpha(_title.color,     a);
            if (_mapImage  != null) _mapImage.color  = WithAlpha(_mapImage.color,  a);
        }

        private static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }
}
