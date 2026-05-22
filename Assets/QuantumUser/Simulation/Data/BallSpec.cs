using System;
using Photon.Deterministic;

namespace Quantum
{
    [Serializable]
    /// <summary>
    /// Asset defining ball physics behaviour parameters such as the end-of-movement velocity threshold and waiting ticks.
    /// </summary>
    public class BallSpec : AssetObject
    {
        public FP EndOfMovementVelocityThreshold;
        public Int32 EndOfMovementWaitingInTicks;
    }
}