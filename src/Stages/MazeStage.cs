using RoR2;
using System.Linq;

namespace RoR2Maze.Stages
{
    internal static class MazeStage
    {
        /// <summary>
        /// ALPHA: always true until a real maze scene exists.
        /// Replace with SceneDef check once the Thunderkit scene is exported.
        /// </summary>
        internal static bool IsCurrentScene => true;

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
