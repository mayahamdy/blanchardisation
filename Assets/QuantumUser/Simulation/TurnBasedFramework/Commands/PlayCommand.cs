using System;
using Photon.Deterministic;

namespace Quantum
{
    /// <summary>
    /// Deterministic command carrying the direction and force data for a player's golf strike.
    /// </summary>
    [Serializable]
    public struct PlayCommandData
    {
        public FP Force;
        public FPVector3 Direction;
    }

    public class PlayCommand : DeterministicCommand
    {
        public PlayCommandData Data;

        /// <summary>
        /// Serializes the force and direction fields of the play command data into the bit stream.
        /// </summary>
        public override void Serialize(BitStream stream)
        {
            stream.Serialize(ref Data.Force);
            stream.Serialize(ref Data.Direction);
        }
    }
}