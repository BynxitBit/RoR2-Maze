using UnityEngine;

namespace RoR2Maze.Puzzles
{
    // Base MonoBehaviour for all maze puzzles.
    // Attach to a puzzle root GameObject placed in the Thunderkit scene.
    public abstract class PuzzleBase : MonoBehaviour
    {
        public bool IsSolved { get; private set; }

        // Implement in each concrete puzzle to define what "activating" does
        // (e.g., player steps on plate, pulls lever, enters code).
        public abstract void OnActivate();

        // Call from OnActivate when the puzzle solution conditions are met.
        protected virtual void Solve()
        {
            if (IsSolved) return;
            IsSolved = true;
            OnSolved();
        }

        // Override to trigger effects: open doors, reveal map, spawn reward.
        protected virtual void OnSolved() { }
    }
}
