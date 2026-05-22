namespace Quantum
{
    /// <summary>
    /// Manages turn transitions between countdown and play phases, and routes turn-end signals to activate the next player's turn.
    /// </summary>
    public unsafe class TurnSystem : SystemSignalsOnly, ISignalOnTurnEnded, ISignalOnSkipCommandReceived,
        ISignalOnBallShot, ISignalOnStartCommandReceived
    {
        /// <summary>
        /// Starts the initial waiting turn when the simulation first initialises.
        /// </summary>
        public override void OnInit(Frame frame)
        {
            TurnConfig waitingConfig = ConfigAssetsHelper.GetWaitingConfig(frame);
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            turnContainer->CurrentTurn.Reset(waitingConfig, ETurnType.Waiting, ETurnStatus.Active, frame);
        }
        
        void ISignalOnStartCommandReceived.OnStartCommandReceived(Frame frame, PlayerRef player)
        {
            TurnConfig startCountdownConfig = ConfigAssetsHelper.GetStartCountdownConfig(frame);
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            turnContainer->CurrentTurn.Reset(startCountdownConfig, ETurnType.Countdown, ETurnStatus.Active, frame);
        }

        /// <summary>
        /// Transitions the ball and the global turn into the resolving state once a shot has been fired.
        /// </summary>
        void ISignalOnBallShot.OnBallShot(Frame frame, EntityRef b)
        {
            BallFields* fields = frame.Unsafe.GetPointer<BallFields>(b);
            fields->TurnStats.SetStatus(ETurnStatus.Resolving, frame);
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            turnContainer->CurrentTurn.SetStatus(ETurnStatus.Resolving, frame);
        }

        /// <summary>
        /// Immediately ends the current turn with a skip reason when the player sends a skip command.
        /// </summary>
        void ISignalOnSkipCommandReceived.OnSkipCommandReceived(Frame frame, PlayerRef player, SkipCommandData data)
        {
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            frame.Signals.OnTurnEnded(turnContainer->CurrentTurn, ETurnEndReason.Skip);
        }

        /// <summary>
        /// Routes the end-of-turn logic: for single-player, keeps the game going without cycling to other players.
        /// </summary>
        void ISignalOnTurnEnded.OnTurnEnded(Frame frame, TurnData turn, ETurnEndReason reason)
        {
            frame.Events.TurnEnded(turn, reason);
            EntityRef ball = turn.Entity;

            BallFields* fields = null;
            if (turn.Type == ETurnType.Play)
            {
                fields = frame.Unsafe.GetPointer<BallFields>(ball);
            }

            switch (turn.Type)
            {
                case ETurnType.Countdown:
                    // For single-player, immediately activate the ball's turn after countdown
                    ActivateTurn(frame, ball);
                    break;

                case ETurnType.Play:
                    fields->TurnStats.AccumulateStats(frame, turn);
                    fields->TurnStats.SetStatus(ETurnStatus.Inactive);
                    TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
                    // For single-player, restart countdown for the same player
                    turnContainer->CurrentTurn.Reset(ConfigAssetsHelper.GetStartCountdownConfig(frame),
                        ETurnType.Countdown,
                        ETurnStatus.Active, frame);
                    break;
            }
        }

        /// <summary>
        /// Marks the ball's stats as active and resets the global turn to the play phase for the given ball's player.
        /// </summary>
        private void ActivateTurn(Frame frame, EntityRef b)
        {
            BallFields* fields = frame.Unsafe.GetPointer<BallFields>(b);
            fields->TurnStats.SetStatus(ETurnStatus.Active);
            Actor* actor = frame.Unsafe.GetPointer<Actor>(b);
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            turnContainer->CurrentTurn.Reset(ConfigAssetsHelper.GetPlayTurnConfig(frame), ETurnType.Play,
                ETurnStatus.Active, b,
                actor->Player, frame);
        }

        /// <summary>
        /// Finds the ball entity with the fewest accumulated strokes that has not yet completed the course, to determine who plays next.
        /// </summary>
        private bool HasNextTurn(Frame frame, EntityRef previous, out EntityRef next)
        {
            next = EntityRef.None;
            foreach (var (entity, fields) in frame.Unsafe.GetComponentBlockIterator<BallFields>())
            {
                int turnNumber = 0;
                if (next != EntityRef.None)
                {
                    turnNumber = frame.Get<BallFields>(next).TurnStats.Number;
                }

                if (fields->HasCompletedCourse == false && (next == EntityRef.None || fields->TurnStats.Number < turnNumber))
                {
                    next = entity;
                }
            }

            return next != EntityRef.None;
        }
    }
}