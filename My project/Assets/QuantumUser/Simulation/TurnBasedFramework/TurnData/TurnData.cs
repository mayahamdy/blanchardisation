namespace Quantum
{
    /// <summary>
    /// Partial struct extending the generated TurnData with helper methods for updating, resetting, and transitioning turn state.
    /// </summary>
    partial struct TurnData
    {
        /// <summary>
        /// Checks the turn timer each tick and fires the OnTurnEnded signal if the timer has just stopped.
        /// </summary>
        public void Update(Frame frame)
        {
            TurnConfig config = frame.FindAsset<TurnConfig>(ConfigRef.Id);
            if (config == null || config.UsesTimer == false || Status != ETurnStatus.Active)
                return;

            if (Timer.HasStoppedThisFrame(frame))
            {
                frame.Signals.OnTurnEnded(this, ETurnEndReason.Time);
            }
        }

        /// <summary>
        /// Carries the remaining time from the source turn over into this turn and increments the stroke counter.
        /// </summary>
        public void AccumulateStats(Frame frame, TurnData from)
        {
            Timer = FrameTimer.FromFrames(frame, Timer.RemainingFrames(frame) + from.Timer.RemainingFrames(frame));
            Number++;
        }

        /// <summary>
        /// Changes the turn type and fires a TurnTypeChanged event if the type actually changed.
        /// </summary>
        public void SetType(ETurnType newType, Frame frame = null)
        {
            if (Type == newType)
                return;

            ETurnType previousType = Type;
            Type = newType;
            frame?.Events.TurnTypeChanged(this, previousType);
        }

        /// <summary>
        /// Changes the turn status, fires TurnStatusChanged, and additionally fires TurnActivated when the status becomes Active.
        /// </summary>
        public void SetStatus(ETurnStatus newStatus, Frame frame = null)
        {
            if (Status == newStatus)
                return;

            ETurnStatus previousStatus = Status;
            Status = newStatus;
            frame?.Events.TurnStatusChanged(this, previousStatus);
            if (Status == ETurnStatus.Active)
            {
                frame?.Events.TurnActivated(this);
            }
        }

        /// <summary>
        /// Resets the turn to the given config, type, and status while preserving the current entity and player.
        /// </summary>
        public void Reset(TurnConfig config, ETurnType type, ETurnStatus status, Frame frame = null)
        {
            ResetData(type, status, Entity, Player, config, frame);
        }

        /// <summary>
        /// Resets the turn's entity and player while preserving the current type, status, and config.
        /// </summary>
        public void Reset(EntityRef entity, PlayerRef owner, Frame frame = null)
        {
            ResetData(Type, Status, entity, owner, ConfigRef, frame);
        }

        /// <summary>
        /// Fully resets all turn data fields to the provided values and fires the appropriate change events.
        /// </summary>
        public void Reset(TurnConfig config, ETurnType type, ETurnStatus status, EntityRef entity, PlayerRef owner,
            Frame frame = null)
        {
            ResetData(type, status, entity, owner, config, frame);
        }

        /// <summary>
        /// Core implementation that writes all turn fields, restarts the timer, and fires type, status, and timer-reset events as needed.
        /// </summary>
        private void ResetData(ETurnType type, ETurnStatus status, EntityRef entity, PlayerRef owner,
            AssetRef<TurnConfig> config, Frame frame = null)
        {
            if (entity != EntityRef.None)
            {
                Entity = entity;
            }

            if (owner != PlayerRef.None)
            {
                Player = owner;
            }

            if (config != null)
            {
                ConfigRef = config;
            }

            ETurnType previousType = Type;
            Type = type;
            ETurnStatus previousStatus = Status;
            Status = status;

            if (frame == null || ConfigRef == null)
            {
                Timer = (default);
            }
            else
            {
                TurnConfig configAsset = frame.FindAsset(ConfigRef);
                Timer = FrameTimer.FromFrames(frame, configAsset.TurnDurationInTicks);
            }

            if (Type != previousType)
            {
                frame?.Events.TurnTypeChanged(this, previousType);
            }

            if (Status != previousStatus)
            {
                frame?.Events.TurnStatusChanged(this, previousStatus);
                if (Status == ETurnStatus.Active)
                {
                    frame?.Events.TurnActivated(this);
                }
            }

            frame?.Events.TurnTimerReset(this);
        }
    }
}