using Photon.Deterministic;
using Quantum.Physics3D;

namespace Quantum
{
    /// <summary>
    /// Handles play command reception, applies the golf strike to the ball, and detects end-of-movement and hole triggers.
    /// </summary>
    public unsafe class PlaySystem : SystemMainThread, ISignalOnPlayCommandReceived, ISignalOnTrigger3D
    {
        /// <summary>
        /// Finds the ball belonging to the commanding player and applies the strike to it.
        /// </summary>
        void ISignalOnPlayCommandReceived.OnPlayCommandReceived(Frame frame, PlayerRef player, PlayCommandData data)
        {
            foreach (var (entity, actor) in frame.Unsafe.GetComponentBlockIterator<Actor>())
            {
                if (actor->Player == player)
                {
                    StrikeBall(frame, entity, data);
                }
            }
        }

        /// <summary>
        /// Clamps the shot values, records the ball's last position, enables its physics body, and applies the strike force.
        /// </summary>
        private void StrikeBall(Frame frame, EntityRef ball, PlayCommandData data)
        {
            GameConfig gameConfig = ConfigAssetsHelper.GetGameConfig(frame);
            ClampShotValues(ref data, gameConfig);
            Transform3D transform = frame.Get<Transform3D>(ball);
            BallFields* ballFields = frame.Unsafe.GetPointer<BallFields>(ball);
            ballFields->LastPosition = transform.Position;

            PhysicsBody3D* body = frame.Unsafe.GetPointer<PhysicsBody3D>(ball);
            body->Enabled = true;
            body->AddForce(data.Direction.Normalized * data.Force);
            frame.Signals.OnBallShot(ball); 
            frame.Events.BallShot(ball);
        }

        /// <summary>
        /// Restricts the shot force and direction Y component to the configured min/max ranges.
        /// </summary>
        private void ClampShotValues(ref PlayCommandData data, GameConfig config)
        {
            data.Force = FPMath.Clamp(data.Force, config.MinStrikeForce, config.MaxStrikeForce);
            FP minY = data.Direction.XZ.Magnitude * FPMath.Tan(config.MinStrikeAngle * FP.Deg2Rad);
            FP maxY = data.Direction.XZ.Magnitude * FPMath.Tan(config.MaxStrikeAngle * FP.Deg2Rad);
            data.Direction.Y = FPMath.Clamp(data.Direction.Y, minY, maxY);
            data.Direction = data.Direction.Normalized;
        }

        /// <summary>
        /// Each tick, checks whether the resolving ball has come to rest and ends the play phase if so.
        /// </summary>
        public override void Update(Frame frame)
        {
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            if (turnContainer->CurrentTurn.Status == ETurnStatus.Resolving)
            {
                CheckEndOfMovement(frame, turnContainer->CurrentTurn.Entity);
            }
        }

        /// <summary>
        /// Monitors ball velocity and a settling timer to determine when the ball has stopped moving, then ends the play.
        /// </summary>
        private void CheckEndOfMovement(Frame frame, EntityRef ball)
        {
            PhysicsBody3D* body = frame.Unsafe.GetPointer<PhysicsBody3D>(ball);
            BallFields* fields = frame.Unsafe.GetPointer<BallFields>(ball);
            Transform3D* transform = frame.Unsafe.GetPointer<Transform3D>(ball);
            BallSpec ballSpec = frame.FindAsset<BallSpec>(fields->Spec.Id);

            // check if below threshold
            if (body->Velocity.Magnitude > ballSpec.EndOfMovementVelocityThreshold)
            {
                fields->EndOfMovementTimer = FrameTimer.FromFrames(frame, ballSpec.EndOfMovementWaitingInTicks);
                return;
            }

            // check end of movement timer
            if (fields->EndOfMovementTimer.IsRunning(frame))
            {
                return;
            }

            if (IsOnRoughField(frame, ball))
            {
                transform->Position = fields->LastPosition;
            }

            EndPlay(frame, ball);
        }

        /// <summary>
        /// Detects when the ball enters the hole trigger and either bumps it out if too fast or completes the course.
        /// </summary>
        void ISignalOnTrigger3D.OnTrigger3D(Frame frame, TriggerInfo3D info)
        {
            GameConfig gameConfig = ConfigAssetsHelper.GetGameConfig(frame);

            if (frame.Layers.GetLayerName(info.StaticData.Layer).Equals(gameConfig.HoleTriggerLayer))
            {
                if (frame.Has<BallFields>(info.Entity))
                {
                    OnHitHole(frame, info.Entity, gameConfig);
                }
            }
        }

        /// <summary>
        /// Overlaps the ball's collider against static geometry to check whether it has landed on the rough field layer.
        /// </summary>
        private bool IsOnRoughField(Frame frame, EntityRef ball)
        {
            GameConfig gameConfig = ConfigAssetsHelper.GetGameConfig(frame);
            Transform3D transform = frame.Get<Transform3D>(ball);
            PhysicsCollider3D ballCollider = frame.Get<PhysicsCollider3D>(ball);

            HitCollection3D hits = frame.Physics3D.OverlapShape(transform, ballCollider.Shape, -1, QueryOptions.HitStatics);
            for (int i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];
                MapStaticCollider3D collider = frame.Map.StaticColliders3D[hit.StaticColliderIndex];

                if (collider.StaticData.Layer == frame.Layers.GetLayerIndex(gameConfig.RoughFieldLayer))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles a ball entering the hole trigger, applying a bump force if velocity is too high or completing the course otherwise.
        /// </summary>
        private void OnHitHole(Frame frame, EntityRef ball, GameConfig gameConfig)
        {
            BallFields* fields = frame.Unsafe.GetPointer<BallFields>(ball);
            if (fields->HasCompletedCourse)
                return;

            PhysicsBody3D* body = frame.Unsafe.GetPointer<PhysicsBody3D>(ball);
            if (body->Velocity.Magnitude > gameConfig.HitHoleVelocityThreshold)
            {
                body->AddForce(FPVector3.Up * gameConfig.HoleBumpForce);
            }
            else
            {
                fields->HasCompletedCourse = true;
                EndPlay(frame, ball);
            }
        }

        /// <summary>
        /// Disables the ball's physics body, records its final position, and signals that the turn has ended.
        /// </summary>
        private void EndPlay(Frame frame, EntityRef ballEntityRef)
        {
            PhysicsBody3D* body = frame.Unsafe.GetPointer<PhysicsBody3D>(ballEntityRef);
            BallFields* fields = frame.Unsafe.GetPointer<BallFields>(ballEntityRef);
            Transform3D* transform = frame.Unsafe.GetPointer<Transform3D>(ballEntityRef);

            body->Enabled = false;
            fields->LastPosition = transform->Position;
            TurnContainer* turnContainer = frame.Unsafe.GetPointerSingleton<TurnContainer>();
            frame.Signals.OnTurnEnded(turnContainer->CurrentTurn, ETurnEndReason.Resolved);
        }
    }
}