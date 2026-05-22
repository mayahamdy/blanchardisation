using Quantum;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Assigns a player-specific colour to the ball's material when the entity is initialised.
    /// </summary>
    public class BallColorView : QuantumEntityViewComponent
    {
        public MeshRenderer Renderer;
        private Material _material;

        /// <summary>
        /// Retrieves the player's Actor component and assigns the corresponding player colour to the ball's instanced material.
        /// </summary>
        public void OnInit(QuantumGame game)
        {
            if (game.Frames.Verified.TryGet<Actor>(EntityRef, out var actor) == false)
            {
                Debug.LogWarning("Actor not found.");
                return;
            }

            _material = Renderer.material;
            _material.color = actor.Player == 0 ? Color.blue : Color.red;
        }

        /// <summary>
        /// Destroys the instanced material to prevent memory leaks when the entity view is removed.
        /// </summary>
        private void OnDestroy()
        {
            if (_material == null)
                return;

            Destroy(_material);
            _material = null;
        }
    }
}