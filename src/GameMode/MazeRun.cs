using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Maze.GameMode
{
    /// <summary>
    /// The Maze game mode Run. Selects only maze stages once the AssetBundle is ready.
    /// Registered with RoR2's content system so it appears in the Alternate Game Modes menu.
    /// </summary>
    public sealed class MazeRun : Run
    {
        public static GameObject Prefab { get; private set; }

        /// <summary>
        /// Creates the MazeRun prefab at runtime.
        /// Call from ContentProvider.LoadStaticContentAsync so Addressables are available.
        /// </summary>
        internal static GameObject BuildPrefab()
        {
            Prefab = new GameObject(nameof(MazeRun));
            Object.DontDestroyOnLoad(Prefab);
            Prefab.SetActive(false);

            // NetworkIdentity is required by UNET for all networked game objects.
            Prefab.AddComponent<NetworkIdentity>();

            var run = Prefab.AddComponent<MazeRun>();
            run.nameToken    = "MAZE_RUN_NAME";
            run.userPickable = true;
            // startingSceneGroup is assigned by ContentProvider after Addressables load.

            // R2API.ContentManagement reads this component for the menu hover text.
            var info = Prefab.AddComponent<GameModeInfo>();
            info.buttonHoverDescription = "MAZE_RUN_DESCRIPTION";

            return Prefab;
        }

        // ── Stage selection ──────────────────────────────────────────────────────

        // Stage injection is handled in StageHooks.Run_PickNextStageScene by
        // checking "self is MazeRun", so this class stays clean of hook boilerplate.
        //
        // TODO (post-Thunderkit): override here if you want MazeRun to completely
        // replace the vanilla pool instead of just injecting into it:
        //
        // public override void PickNextStageScene(WeightedSelection<SceneDef> choices)
        // {
        //     choices.Clear();
        //     choices.AddChoice(ContentProvider.StageDefs[(stageClearCount + 1) % 5], 1f);
        //     nextStageScene = choices.Evaluate(0f);
        // }
    }
}
