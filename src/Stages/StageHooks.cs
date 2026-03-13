using RoR2;
using RoR2Maze.Maze;
using UnityEngine.SceneManagement;

namespace RoR2Maze.Stages
{
    internal static class StageHooks
    {
        internal static void Init()
        {
            On.RoR2.Run.PickNextStageScene += Run_PickNextStageScene;
            On.RoR2.SceneCatalog.FindSceneIndex += SceneCatalog_FindSceneIndex;
            On.RoR2.SceneDirector.DefaultPlayerSpawnPointGenerator += SceneDirector_DefaultPlayerSpawnPointGenerator;
            On.RoR2.ClassicStageInfo.Start += ClassicStageInfo_Start;
            SceneManager.sceneUnloaded += _ => MazeBuilder.Clear();

            Log.Info("StageHooks initialized.");
        }

        // ── Stage injection ──────────────────────────────────────────────────────

        /// <summary>
        /// Handles stage selection for all run types.
        /// - MazeRun: uses only maze stages (once AssetBundle exists).
        /// - Other runs: maze stages are NOT injected (vanilla pool unchanged for now).
        ///   TODO: add a config-driven injection weight for Classic/Eclipse runs.
        /// </summary>
        private static void Run_PickNextStageScene(
            On.RoR2.Run.orig_PickNextStageScene orig, Run self, WeightedSelection<SceneDef> choices)
        {
            if (self is GameMode.MazeRun)
            {
                // Route MazeRun to the correct per-loop maze SceneDef.
                var mazeDef = MazeStage.GetDefForClearCount(self.stageClearCount + 1);
                choices.Clear();
                choices.AddChoice(mazeDef, 1f);
                // orig is intentionally not called — we fully control stage selection.
                return;
            }

            orig(self, choices);
        }

        /// <summary>
        /// Resolve our shared scene name to the correct per-slot SceneDef.
        /// </summary>
        private static SceneIndex SceneCatalog_FindSceneIndex(
            On.RoR2.SceneCatalog.orig_FindSceneIndex orig, string sceneName)
        {
            if (sceneName != ContentProvider.SceneName)
                return orig(sceneName);

            var def = MazeStage.GetDefForClearCount(Run.instance?.stageClearCount ?? 0);
            return def != null ? def.sceneDefIndex : orig(sceneName);
        }

        // ── Scene setup ──────────────────────────────────────────────────────────

        /// <summary>
        /// Skip drop-pods in the maze; place players directly via the node graph.
        /// </summary>
        private static void SceneDirector_DefaultPlayerSpawnPointGenerator(
            On.RoR2.SceneDirector.orig_DefaultPlayerSpawnPointGenerator orig, SceneDirector self)
        {
            if (!MazeStage.IsCurrentScene) { orig(self); return; }

            self.RemoveAllExistingSpawnPoints();
            self.PlacePlayerSpawnsViaNodegraph();
        }

        /// <summary>
        /// ClassicStageInfo caches the scene name internally; rename around the call
        /// so any director-side mods that key on cachedName don't confuse our stage
        /// with a vanilla one (same pattern as ProceduralStages).
        /// </summary>
        internal static void ClassicStageInfo_Start(
            On.RoR2.ClassicStageInfo.orig_Start orig, ClassicStageInfo self)
        {
            if (!MazeStage.IsCurrentScene) { orig(self); return; }

            var def = SceneCatalog.currentSceneDef;
            var realName = def.cachedName;
            def.cachedName = System.Guid.NewGuid().ToString();
            try   { orig(self); }
            finally { def.cachedName = realName; }
        }
    }
}
