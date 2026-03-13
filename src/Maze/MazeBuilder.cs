using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Maze.Maze
{
    /// <summary>
    /// Converts a MazeGrid into world geometry using Unity primitives.
    /// Each cell maps to a CellSize × CellSize area on the XZ plane.
    ///
    /// TODO: replace CreatePrimitive walls/floor with Addressable RoR2 mesh prefabs
    ///       once the desired asset paths are confirmed in-editor.
    /// </summary>
    internal static class MazeBuilder
    {
        // ── Dimensions ───────────────────────────────────────────────────────────

        /// <summary>
        /// World-space size of one maze cell (corridor width).
        /// At 20 u/cell a 26×18 grid = 520×360 u — close to a full RoR2 stage footprint.
        /// Characters sprint at ~7 u/s, so crossing the maze diagonally takes ~90 s straight-line,
        /// ~5–8 min with actual maze navigation.
        /// </summary>
        public const float CellSize = 20f;

        private const float WallHeight    = 8f;
        private const float WallThickness = 1.5f;
        private const float FloorThick    = 0.5f;

        // ── State ────────────────────────────────────────────────────────────────

        private static GameObject? _root;

        /// <summary>True while maze geometry exists in the scene.</summary>
        internal static bool IsBuilt => _root != null;

        /// <summary>World position a player should spawn at (centre of Start cell, above floor).</summary>
        internal static Vector3 StartPosition { get; private set; }

        private static Vector3 _origin;

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Spawn all maze geometry. Safe to call again — clears the previous build first.
        /// </summary>
        internal static void Build(MazeGrid grid, Vector3 origin)
        {
            Clear();
            _origin = origin;
            _root   = new GameObject("MazeGeometry");

            // Floor slab
            float halfW  = grid.Width  * CellSize * 0.5f;
            float halfH  = grid.Height * CellSize * 0.5f;
            SpawnBox("Floor",
                origin + new Vector3(halfW, -FloorThick * 0.5f, halfH),
                new Vector3(grid.Width * CellSize + WallThickness,
                            FloorThick,
                            grid.Height * CellSize + WallThickness));

            // Walls — draw only North + East per cell, plus the outer South and West edges.
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    if ((grid.Walls[x, y] & Wall.North) != 0)
                        SpawnWall(origin, x, y, Wall.North);

                    if ((grid.Walls[x, y] & Wall.East) != 0)
                        SpawnWall(origin, x, y, Wall.East);

                    if (y == 0 && (grid.Walls[x, y] & Wall.South) != 0)
                        SpawnWall(origin, x, y, Wall.South);

                    if (x == 0 && (grid.Walls[x, y] & Wall.West) != 0)
                        SpawnWall(origin, x, y, Wall.West);
                }
            }

            // Exit zone trigger
            var exitCenter = CellCenter(grid.Exit.x, grid.Exit.y);
            var exitGo     = new GameObject("MazeExit");
            exitGo.transform.SetParent(_root.transform);
            exitGo.transform.position = exitCenter;
            var col = exitGo.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size      = new Vector3(CellSize * 0.8f, WallHeight, CellSize * 0.8f);
            exitGo.AddComponent<Stages.MazeExitZone>();

            // Compute player start position (above floor, centre of Start cell).
            StartPosition = CellCenter(grid.Start.x, grid.Start.y) + new Vector3(0f, 1.5f, 0f);

            Log.Info($"[MazeBuilder] {grid.Width}×{grid.Height} maze built at {origin}. " +
                     $"Start={StartPosition}  Exit={exitCenter}");
        }

        /// <summary>Destroy all maze geometry and reset state.</summary>
        internal static void Clear()
        {
            if (_root != null)
            {
                Object.Destroy(_root);
                _root = null;
            }
        }

        /// <summary>World-space centre of a cell (at floor level).</summary>
        internal static Vector3 CellCenter(int x, int y) =>
            _origin + new Vector3(x * CellSize + CellSize * 0.5f, 0f, y * CellSize + CellSize * 0.5f);

        // ── Teleport ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Teleport a CharacterBody to the maze start. Server-only in multiplayer.
        /// </summary>
        internal static void TeleportToStart(CharacterBody body)
        {
            if (!IsBuilt) return;

            // TeleportHelper handles both server and client-predicted teleports.
            TeleportHelper.TeleportBody(body, StartPosition);
            Log.Info($"[MazeBuilder] Teleported {body.name} to maze start.");
        }

        // ── Wall helpers ─────────────────────────────────────────────────────────

        // Wall length extends by WallThickness on each end so adjacent walls meet
        // flush at corners without leaving gaps.
        private static float WallLen => CellSize + WallThickness;

        private static void SpawnWall(Vector3 origin, int x, int y, Wall side)
        {
            float cx = origin.x + x * CellSize;
            float cy = origin.y;
            float cz = origin.z + y * CellSize;
            float halfC = CellSize * 0.5f;
            float halfH = WallHeight * 0.5f;

            Vector3 pos, scale;
            switch (side)
            {
                case Wall.North:
                    pos   = new Vector3(cx + halfC,          cy + halfH, cz + CellSize);
                    scale = new Vector3(WallLen, WallHeight, WallThickness);
                    break;
                case Wall.South:
                    pos   = new Vector3(cx + halfC,          cy + halfH, cz);
                    scale = new Vector3(WallLen, WallHeight, WallThickness);
                    break;
                case Wall.East:
                    pos   = new Vector3(cx + CellSize, cy + halfH, cz + halfC);
                    scale = new Vector3(WallThickness, WallHeight, WallLen);
                    break;
                case Wall.West:
                    pos   = new Vector3(cx,            cy + halfH, cz + halfC);
                    scale = new Vector3(WallThickness, WallHeight, WallLen);
                    break;
                default:
                    return;
            }

            SpawnBox($"Wall_{x}_{y}_{side}", pos, scale);
        }

        private static void SpawnBox(string name, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name       = name;
            go.isStatic   = true;   // enables static batching — critical for ~1000+ objects
            go.transform.SetParent(_root!.transform);
            go.transform.position   = pos;
            go.transform.localScale = scale;
        }
    }
}
