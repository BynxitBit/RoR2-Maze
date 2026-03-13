using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2Maze.GameMode;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoR2Maze
{
    internal sealed class ContentProvider : IContentPackProvider
    {
        public string identifier => MazePlugin.PluginGUID + "." + nameof(ContentProvider);

        internal static readonly ContentPack ContentPack = new ContentPack();

        /// <summary>
        /// One SceneDef per stage-loop slot (1–5), following the same pattern as ProceduralStages.
        /// All share the same cachedName so FindSceneIndex can resolve them by stage clear count.
        /// </summary>
        internal static readonly SceneDef[] StageDefs = new SceneDef[5];

        /// <summary>
        /// Must match the Unity scene file name inside the exported AssetBundle.
        /// ALPHA: no AssetBundle yet - stage injection is disabled until Thunderkit export.
        /// </summary>
        internal const string SceneName = "ror2maze_stage";

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            ContentPack.identifier = identifier;

            for (int i = 1; i <= 5; i++)
            {
                var def = ScriptableObject.CreateInstance<SceneDef>();
                def.cachedName              = SceneName;
                def.sceneType               = SceneType.Stage;
                def.isOfflineScene          = false;
                def.nameToken               = "THE ENDLESS MAZE";
                def.subtitleToken           = "Nothing escapes.";
                def.shouldIncludeInLogbook  = false;
                def.validForRandomSelection = false;
                def.stageOrder              = i;

                // Point destinations at the next vanilla stage group so the run
                // continues normally after the maze.
                int nextSlot = (i % 5) + 1;
                var req = Addressables.LoadAssetAsync<SceneCollection>(
                    $"RoR2/Base/SceneGroups/sgStage{nextSlot}.asset");
                while (!req.IsDone) yield return null;
                def.destinationsGroup = req.Result;

                StageDefs[i - 1] = def;
                args.progressReceiver.Report(i / 5f);
            }

            ContentPack.sceneDefs.Add(StageDefs);
            Log.Info("ContentProvider: maze SceneDefs registered.");

            // ── Assign startingSceneGroup now that Addressables are available ────
            // MazeRun.Prefab was built and registered with R2API in Plugin.Awake().
            // ALPHA: use vanilla Stage-1 as placeholder until the maze AssetBundle exists.
            var sg1 = Addressables.LoadAssetAsync<SceneCollection>(
                "RoR2/Base/SceneGroups/sgStage1.asset");
            while (!sg1.IsDone) yield return null;
            MazeRun.Prefab.GetComponent<MazeRun>().startingSceneGroup = sg1.Result;
            Log.Info("ContentProvider: MazeRun startingSceneGroup assigned.");
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(ContentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
