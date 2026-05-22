namespace Quantum
{
    /// <summary>
    /// Calculates and stores each player's score at the end of their play turn based on strokes used.
    /// </summary>
    public unsafe class ScoreSystem : SystemSignalsOnly, ISignalOnTurnEnded
    {
        /// <summary>
        /// Calculates and stores the player's score at the end of a play turn based on strokes used and course completion.
        /// </summary>
        void ISignalOnTurnEnded.OnTurnEnded(Frame frame, TurnData turn, ETurnEndReason reason)
        {
            if (turn.Type != ETurnType.Play)
                return;

            BallFields* fields = frame.Unsafe.GetPointer<BallFields>(turn.Entity);
            GameConfig gameConfig = ConfigAssetsHelper.GetGameConfig(frame);
            int maxScore = gameConfig.MaxStrokes + 1;
            fields->Score = fields->HasCompletedCourse ? maxScore - fields->TurnStats.Number : 0;
        }
    }
}