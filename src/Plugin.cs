using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2.ContentManagement;
using RoR2Maze.GameMode;
using RoR2Maze.Stages;
using RoR2Maze.Vision;
using RoR2Maze.StarMap;

namespace RoR2Maze
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.bepis.r2api.content_management")]
    public class MazePlugin : BaseUnityPlugin
    {
        public const string PluginGUID    = PluginAuthor + "." + PluginName;
        public const string PluginAuthor  = "BynxitBit";
        public const string PluginName    = "RoR2Maze";
        public const string PluginVersion = "0.1.0";

        public static MazePlugin Instance { get; private set; }

        /// <summary>
        /// ALPHA: when true, maze systems activate on every stage so you can test
        /// LanternSystem and StarMapSystem without a Thunderkit-exported scene.
        /// Set to false (default) before shipping.
        /// </summary>
        public static ConfigEntry<bool> DebugMazeMode { get; private set; }

        public void Awake()
        {
            Instance = this;
            Log.Init(Logger);

            DebugMazeMode = Config.Bind(
                section: "Debug",
                key: "ForceActiveOnAllStages",
                defaultValue: false,
                description: "ALPHA: enable maze systems on every vanilla stage for testing.");

            Log.Info($"{PluginGUID} v{PluginVersion} loading…");

            // Build the MazeRun prefab and register it with R2API NOW - before R2API
            // generates its own ContentPack (which happens during collectContentPackProviders,
            // before LoadStaticContentAsync runs). Calling AddGameMode inside
            // LoadStaticContentAsync is too late; R2API has already frozen its list.
            var runPrefab = MazeRun.BuildPrefab();
            ContentAddition.AddGameMode(runPrefab, "MAZE_RUN_DESCRIPTION");
            Log.Info("MazeRun game mode registered with R2API.");

            // Register our SceneDefs with RoR2's content system.
            ContentManager.collectContentPackProviders += AddContentPackProvider;

            // Wire up all gameplay systems.
            LanguageTokens.Init();
            MazeStage.Init();
            StageHooks.Init();
            LanternSystem.Init();
            StarMapSystem.Init();

            Log.Info($"{PluginGUID} ready.");
        }

        private static void AddContentPackProvider(
            ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new ContentProvider());
        }
    }
}
