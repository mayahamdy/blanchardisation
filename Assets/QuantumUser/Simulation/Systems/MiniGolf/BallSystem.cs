using Photon.Deterministic;

namespace Quantum
{
    /// <summary>
    /// System the detects when the ball hits obstacles
    /// </summary>
    public unsafe class BallSystem : SystemSignalsOnly, ISignalOnCollisionEnter3D
    {
        /// <summary>
        /// Triggered when the ball hit an obstacle 
        /// </summary>
        void ISignalOnCollisionEnter3D.OnCollisionEnter3D(Frame frame, CollisionInfo3D info)
        {
            if (frame.Has<BallFields>(info.Entity) == false)
                return;

            PhysicsBody3D body = frame.Get<PhysicsBody3D>(info.Entity);
            var collisionAngle = FPVector3.Angle(info.ContactNormal, body.Velocity);
            //Threshold to activate BallHit event and avoid multiple SFX at the same time
            var minCollisionAngleThreshold = 15;
            if (FPMath.Abs(collisionAngle - 90) < minCollisionAngleThreshold)
                return;

            frame.Events.BallHit(info.Entity);
        }
    }
}