using System;
using Photon.Deterministic;

namespace Quantum
{
    /// <summary>
    /// Deterministic command representing a player's request to skip their current turn.
    /// </summary>
    [Serializable]
    public struct SkipCommandData
    {
    }

    public class SkipCommand : DeterministicCommand
    {
        public SkipCommandData Data;

        /// <summary>
        /// Serializes the skip command data into the bit stream (currently no fields to write).
        /// </summary>
        public override void Serialize(BitStream stream)
        {
        }
    }
}