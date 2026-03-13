using UnityEngine;

namespace RoR2Maze.Vision
{
    /// <summary>
    /// Per-player component that tracks lantern fuel and drives the vision-radius effect.
    /// Attach to a player CharacterBody's GameObject when entering a maze stage.
    /// </summary>
    public sealed class LanternBehaviour : MonoBehaviour
    {
        /// <summary>Current fuel level in [0, 1]. 1 = full, 0 = empty.</summary>
        public float Fuel { get; private set; } = 1f;

        /// <summary>Current vision radius derived from fuel level.</summary>
        public float VisionRadius =>
            Mathf.Lerp(LanternSystem.MinVisionRadius, LanternSystem.MaxVisionRadius, Fuel);

        private void Update()
        {
            Fuel = Mathf.Clamp01(Fuel - LanternSystem.DrainPerSecond * Time.deltaTime);

            // TODO: push VisionRadius to the post-process vignette / fog-sphere shader here.
            // Example: VignetteController.instance?.SetRadius(VisionRadius);
        }

        /// <summary>
        /// Add fuel (e.g. from a wall station or item pickup).
        /// </summary>
        /// <param name="amount">Fuel to restore, in [0, 1].</param>
        public void Refuel(float amount)
        {
            Fuel = Mathf.Clamp01(Fuel + amount);
            Log.Info($"[LanternBehaviour] Refuelled +{amount:P0}. Fuel now {Fuel:P0}.");
        }
    }
}
