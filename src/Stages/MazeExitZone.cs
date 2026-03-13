using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2Maze.Stages
{
    /// <summary>
    /// Place on a trigger-collider GameObject in the maze scene to mark the stage exit.
    /// Only the server processes the trigger; the RPC-based SceneExitController handles
    /// advancing all clients.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class MazeExitZone : MonoBehaviour
    {
        [Tooltip("Seconds of invincibility granted when reaching the exit (prevents death during transition).")]
        public float exitGracePeriod = 3f;

        private bool _triggered;

        private void Awake() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (_triggered || !NetworkServer.active) return;

            var body = other.GetComponentInParent<CharacterBody>();
            if (body == null || !body.isPlayerControlled) return;

            _triggered = true;
            Log.Info("[MazeExitZone] Player reached the exit!");
            AdvanceStage();
        }

        private static void AdvanceStage()
        {
            // Find the scene exit controller that manages the stage transition.
            // In a hand-authored scene this should be placed alongside the teleporter prefab
            // (or a stripped-down version of it). If none is found we log a warning.
            var exitCtrl = FindObjectOfType<SceneExitController>();
            if (exitCtrl != null)
            {
                exitCtrl.SetState(SceneExitController.ExitState.Finished);
            }
            else
            {
                Log.Warning("[MazeExitZone] No SceneExitController in scene – stage cannot advance. " +
                            "Make sure the maze scene contains a SceneExitController prefab.");
            }
        }
    }
}
