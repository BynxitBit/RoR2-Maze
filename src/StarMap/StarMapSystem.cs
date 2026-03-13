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
            StarMapHud.Show(ShowDuration);
            Log.Info($"[StarMapSystem] Star map shown for {ShowDuration}s.");
        }
    }
}
