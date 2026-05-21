using System;

namespace Quantum
{
    /// <summary>
    /// Asset holding gameplay configuration values such as stroke limits, strike force ranges, and layer names.
    /// </summary>
    public partial class GameConfig : AssetObject
    {
        public Int32 MaxStrokes;
        public Boolean SpawnBallsNearHole;
        public Int32 MinStrikeAngle;
        public Int32 MaxStrikeAngle;
        public Int32 MinStrikeForce;
        public Int32 MaxStrikeForce;
        public Int32 ForceBarVelocity;
        public Int32 HitHoleVelocityThreshold;
        public Int32 HoleBumpForce;
        public string HoleTriggerLayer;
        public string RoughFieldLayer;
    }
}