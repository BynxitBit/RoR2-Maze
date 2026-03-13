using RoR2;

namespace RoR2Maze.StarMap
{
    internal static class StarMapSystem
    {
        /// <summary>How long the star-map overlay stays visible at stage start (seconds).</summary>
        public const float ShowDuration = 8f;

        internal static void Init()
        {
            Stage.onStageStartGlobal += OnStageStart;
            Log.Info("StarMapSystem initialized.");
        }

        private static void OnStageStart(Stage stage)
        {
            if (!Stages.MazeStage.IsCurrentScene) return;
            Log.Info("[StarMapSystem] Showing star map overlay.");
            ShowOverlay();
        }

        private static void ShowOverlay()
        {
            // TODO: Instantiate the HUD overlay prefab built in Thunderkit.
            //   1. Load the overlay prefab from the AssetBundle.
            //   2. Instantiate it under HUD.instance.mainContainer.
            //   3. Auto-destroy after ShowDuration seconds.
            //   4. Populate the maze waypoint graph from MazeGenerator.CurrentGraph.
            Log.Info($"[StarMapSystem] Overlay placeholder – will display for {ShowDuration}s once prefab is available.");
        }
    }
}
