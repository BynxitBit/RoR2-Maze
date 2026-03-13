using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2Maze.GameMode;
using System.Collections;
using IOPath = System.IO.Path;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoR2Maze
{
    internal sealed class ContentProvider : IContentPackProvider
    {
        public string identifier => MazePlugin.PluginGUID + "." + nameof(ContentProvider);

        internal static readonly ContentPack ContentPack = new ContentPack();

        /// <summary>
        /// One SceneDef per stage-loop slot (1–5).
        /// All share cachedName so FindSceneIndex resolves them by stage clear count.
        /// </summary>
        internal static readonly SceneDef[] StageDefs = new SceneDef[5];

        /// <summary>
        /// Must match the Unity scene file name inside the exported AssetBundle.
        /// </summary>
        internal const string SceneName = "ror2maze_stage";

        /// <summary>
        /// File name of the AssetBundle exported from Thunderkit.
        /// Place it next to RoR2Maze.dll in BepInEx/plugins/BynxitBit-RoR2Maze/.
        /// </summary>
        private const string BundleName = "mazestagebundle";

        /// <summary>
        /// The runtime SceneCollection that MazeRun uses to start the first maze stage.
        /// Built from our SceneDefs once they're created.
        /// </summary>
        internal static SceneCollection? MazeStartGroup { get; private set; }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            ContentPack.identifier = identifier;

            // ── 1. Load the AssetBundle ──────────────────────────────────────────
            // The bundle must sit next to the DLL. If it's missing we log a warning
            // and continue — vanilla-stage DebugMode still works without it.
            string dllDir     = IOPath.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            string bundlePath = IOPath.Combine(dllDir, BundleName);
            AssetBundle? bundle = null;

            if (System.IO.File.Exists(bundlePath))
            {
                var bundleReq = AssetBundle.LoadFromFileAsync(bundlePath);
                while (!bundleReq.isDone) yield return null;
                bundle = bundleReq.assetBundle;
                Log.Info($"ContentProvider: AssetBundle '{BundleName}' loaded.");
            }
            else
            {
                Log.Warning($"ContentProvider: '{BundleName}' not found at {bundlePath}. " +
                            "Maze scene unavailable — DebugMode still works on vanilla stages.");
            }

            // ── 2. Create SceneDefs ──────────────────────────────────────────────
            for (int i = 1; i <= 5; i++)
            {
                var def = ScriptableObject.CreateInstance<SceneDef>();
                def.cachedName             = SceneName;
                def.sceneType              = SceneType.Stage;
                def.isOfflineScene         = false;
                def.nameToken              = "THE ENDLESS MAZE";
                def.subtitleToken          = "Nothing escapes.";
                def.shouldIncludeInLogbook = false;
                def.validForRandomSelection = false;
                def.stageOrder             = i;

                // After clearing the maze the run continues to the next vanilla loop slot.
                int nextSlot = (i % 5) + 1;
                var destReq = Addressables.LoadAssetAsync<SceneCollection>(
                    $"RoR2/Base/SceneGroups/sgStage{nextSlot}.asset");
                while (!destReq.IsDone) yield return null;
                def.destinationsGroup = destReq.Result;

                StageDefs[i - 1] = def;
                args.progressReceiver.Report(i / 6f);
            }

            ContentPack.sceneDefs.Add(StageDefs);
            Log.Info("ContentProvider: maze SceneDefs registered.");

            // ── 3. Build a SceneCollection for MazeRun.startingSceneGroup ────────
            // We create a runtime SceneCollection that contains only our loop-slot 1 def.
            // This tells MazeRun which scene to load when the player presses Play.
            MazeStartGroup = ScriptableObject.CreateInstance<SceneCollection>();
            MazeStartGroup._sceneEntries = new[]
            {
                new SceneCollection.SceneEntry { sceneDef = StageDefs[0], weightMinusOne = 0f }
            };

            MazeRun.Prefab.GetComponent<MazeRun>().startingSceneGroup = MazeStartGroup;
            Log.Info("ContentProvider: MazeRun startingSceneGroup → maze SceneCollection.");

            args.progressReceiver.Report(1f);
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
