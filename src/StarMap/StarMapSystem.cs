using RoR2;
using RoR2Maze.Maze;
using UnityEngine;

namespace RoR2Maze.StarMap
{
    internal static class StarMapSystem
    {
        /// <summary>How long the star-map overlay stays visible at stage start (seconds).</summary>
        public const float ShowDuration = 8f;

        /// <summary>
        /// Maze dimensions. 26×18 at CellSize=20 gives a ~520×360 u footprint,
        /// comparable to a full RoR2 stage. Increase for later loops.
        /// </summary>
        private const int MazeWidth  = 26;
        private const int MazeHeight = 18;

        /// <summary>The maze grid for the current stage. Null between stages.</summary>
        internal static MazeGrid? CurrentGrid { get; private set; }

        internal static void Init()
        {
            Stage.onStageStartGlobal += OnStageStart;
            Log.Info("StarMapSystem initialized.");
        }

        private static void OnStageStart(Stage stage)
        {
            if (!Stages.MazeStage.IsCurrentScene) return;

            // Use the RoR2 run seed so all clients generate the same maze.
            int seed = Run.instance != null ? (int)Run.instance.seed : stage.GetHashCode();
            CurrentGrid = MazeGrid.Generate(MazeWidth, MazeHeight, seed);

            Log.Info($"[StarMapSystem] Maze {MazeWidth}x{MazeHeight} generated (seed {seed}). " +
                     $"Solution length: {CurrentGrid.Solution.Count} cells.");

            // Build the 3D maze at the world origin.
            // For the real maze scene this will be the scene-defined spawn origin;
            // for DebugMazeMode (vanilla stages) we use (0,0,0) and teleport the player.
            MazeBuilder.Build(CurrentGrid, Vector3.zero);

            StarMapHud.Show(ShowDuration, CurrentGrid);
        }
    }
}
