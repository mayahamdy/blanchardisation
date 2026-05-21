using System;
using Photon.Deterministic;

namespace Quantum
{
    /// <summary>
    /// Deterministic command representing a player's request to start the game.
    /// </summary>
    ///
    public class StartCommand : DeterministicCommand
    {
        /// <summary>
        /// Serializes the start command data into the bit stream (currently no fields to write).
        /// </summary>
        public override void Serialize(BitStream stream)
        {
        }
    }
}