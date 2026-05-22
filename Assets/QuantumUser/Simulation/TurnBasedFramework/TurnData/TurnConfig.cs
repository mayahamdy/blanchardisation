using System;
namespace Quantum
{
    /// <summary>
    /// Asset that defines the configuration for a turn, including duration, timer usage, and skip permission.
    /// </summary>
    public class TurnConfig : AssetObject
    {
        public Boolean UsesTimer;
        public Int32 TurnDurationInTicks;
        public Boolean IsSkippable;
    }
}