using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Singleton that dispatches Quantum commands such as play and skip on behalf of the local player.
    /// </summary>
    public class CommandDispatcher : QuantumSceneViewComponent
    {
        public static CommandDispatcher Instance { get; private set; }

        /// <summary>
        /// Enforces the singleton pattern, destroying any duplicate instance that appears after the first.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }
        
        /// <summary>
        /// Sends a StartCommand to start the game.
        /// </summary>
        public void SendStartCommand()
        {
            Game.SendCommand(new StartCommand());
        }

        /// <summary>
        /// Sends a SkipCommand to the Quantum simulation on behalf of the local player.
        /// </summary>
        public void SendSkipCommand()
        {
            Game.SendCommand(new SkipCommand());
        }

        /// <summary>
        /// Sends a PlayCommand carrying the given direction and force to the Quantum simulation.
        /// </summary>
        public void SendPlayCommand(FPVector3 direction, FP force)
        {
            Game.SendCommand(new PlayCommand { Data = { Direction = direction, Force = force } });
        }
    }
}