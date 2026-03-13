using RoR2;
using UnityEngine;

namespace RoR2Maze.Traps
{
    // Base MonoBehaviour for all maze traps.
    // Attach to a trigger-collider GameObject placed in the Thunderkit scene.
    [RequireComponent(typeof(Collider))]
    public abstract class TrapBase : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Only activate for player-controlled bodies.
            var body = other.GetComponentInParent<CharacterBody>();
            if (body == null || !body.isPlayerControlled) return;
            OnPlayerEntered(body);
        }

        // Implement the trap effect: deal damage, teleport, knock back, etc.
        protected abstract void OnPlayerEntered(CharacterBody body);
    }
}
