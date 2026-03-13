using RoR2;
using RoR2Maze.Maze;
using UnityEngine;

namespace RoR2Maze.Vision
{
    internal static class LanternSystem
    {
        public const float MaxVisionRadius = 20f;
        public const float MinVisionRadius =  4f;

        /// <summary>
        /// Fuel consumed per second at full drain rate.
        /// Default: 0.01 → full lantern lasts ~100 s.
        /// </summary>
        public const float DrainPerSecond = 0.01f;

        internal static void Init()
        {
            On.RoR2.CharacterBody.Start += CharacterBody_Start;
            Log.Info("LanternSystem initialized.");
        }

        /// <summary>
        /// Attach a LanternBehaviour to every player body that spawns during a maze stage.
        /// Handles both initial spawns and respawns.
        /// </summary>
        private static void CharacterBody_Start(
            On.RoR2.CharacterBody.orig_Start orig, CharacterBody self)
        {
            orig(self);

            if (!Stages.MazeStage.IsCurrentScene) return;
            if (!self.isPlayerControlled) return;
            if (self.GetComponent<LanternBehaviour>() != null) return;

            self.gameObject.AddComponent<LanternBehaviour>();
            Log.Info($"[LanternSystem] Lantern attached to {self.name}.");

            // Teleport the local player to the maze start on their first spawn.
            // Runs only for the local user; skipped on respawns because LanternBehaviour
            // is already present on any re-spawned body instance check above.
            MazeBuilder.TeleportToStart(self);
        }
    }
}
