namespace Quantum
{
    /// <summary>
    /// Checks after each turn whether all players have exhausted their strokes or completed the course, then ends gameplay.
    /// </summary>
    public unsafe class GameEnd : SystemSignalsOnly, ISignalOnTurnEnded
    {
        /// <summary>
        /// Triggers an end-game check after every turn ends.
        /// </summary>
        void ISignalOnTurnEnded.OnTurnEnded(Frame frame, TurnData turn, ETurnEndReason reason)
        {
            CheckEndGame(frame);
        }

        /// <summary>
        /// Iterates all balls and returns early if any player still has strokes remaining and has not completed the course.
        /// </summary>
        private void CheckEndGame(Frame frame)
        {
            GameConfig gameConfig = ConfigAssetsHelper.GetGameConfig(frame);
            foreach (var (entity, fields) in frame.Unsafe.GetComponentBlockIterator<BallFields>())
            {
                if (fields->TurnStats.Number < gameConfig.MaxStrokes && fields->HasCompletedCourse == false)
                {
                    return;
                }
            }

            EndGameplay(frame);
        }

        /// <summary>
        /// Resets the current turn to an inactive state and fires the GameplayEnded event.
        /// </summary>
        private void EndGameplay(Frame frame)
        {
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            turnContainer->CurrentTurn.Reset(default, default, status: ETurnStatus.Inactive, frame);
            frame.Events.GameplayEnded();
        }
    }
}