using RoR2;
using System.Linq;

namespace RoR2Maze.Stages
{
    internal static class MazeStage
    {
        /// <summary>
        /// True when the currently-loaded scene is one of our maze SceneDefs,
        /// OR when the debug override is active (allows testing on vanilla stages).
        /// </summary>
        internal static bool IsCurrentScene =>
            MazePlugin.DebugMazeMode.Value ||
            (SceneCatalog.currentSceneDef != null &&
             ContentProvider.StageDefs.Contains(SceneCatalog.currentSceneDef));

        /// <summary>
        /// Returns the SceneDef for the given stage-clear count (one per loop slot).
        /// </summary>
        internal static SceneDef GetDefForClearCount(int clearCount) =>
            ContentProvider.StageDefs[clearCount % 5];

        internal static void Init()
        {
            Log.Info("MazeStage initialized.");
        }
    }
}
