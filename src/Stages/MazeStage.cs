using RoR2;
using System.Linq;

namespace RoR2Maze.Stages
{
    internal static class MazeStage
    {
        /// <summary>
        /// True when the currently loaded scene is one of our maze SceneDefs.
        /// During development you can force this to <c>true</c> by temporarily
        /// returning <c>true</c> here (useful for testing systems on vanilla stages).
        /// </summary>
        internal static bool IsCurrentScene =>
            SceneCatalog.currentSceneDef != null &&
            System.Array.IndexOf(ContentProvider.StageDefs, SceneCatalog.currentSceneDef) >= 0;

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
