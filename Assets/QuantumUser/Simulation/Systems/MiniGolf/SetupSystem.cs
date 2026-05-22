using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    /// <summary>
    /// Spawns and initialises ball entities for each player when they join, and places them at available spawn points.
    /// </summary>
    public unsafe class SetupSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        /// <summary>
        /// Creates a ball entity for a newly added player, initialises its components, and places it at a valid spawn point.
        /// </summary>
        void ISignalOnPlayerAdded.OnPlayerAdded(Frame frame, PlayerRef player, bool firstTime)
        {
            if (player >= 2)
            {
                Debug.LogWarning($"[Quantum Golf] Player {player}  is not allowed. Only 2 players are allowed.]");
                return;
            }

            if (firstTime)
            {
                EntityPrototype prototypeAsset = frame.FindAsset<EntityPrototype>(frame.RuntimeConfig.PrototypeRef);
                EntityRef ball = frame.Create(prototypeAsset);

                Actor* actor = frame.Unsafe.GetPointer<Actor>(ball);
                actor->Active = true;
                actor->Player = player;

                PhysicsBody3D* body = frame.Unsafe.GetPointer<PhysicsBody3D>(ball);
                body->Enabled = false;

                // init turn data
                BallFields* fields = frame.Unsafe.GetPointer<BallFields>(ball);
                fields->TurnStats.Reset(ball, actor->Player);
                fields->TurnStats.SetStatus(ETurnStatus.Inactive);

                SpawnBall(frame, ball);
            }
        }

        /// <summary>
        /// Finds the first available spawn point of the appropriate type and moves the ball entity to that position.
        /// </summary>
        public static void SpawnBall(Frame frame, EntityRef ball)
        {
            GameConfig gameConfig = ConfigAssetsHelper.GetGameConfig(frame);
            bool spawnNearHole = gameConfig.SpawnBallsNearHole;

            var spawnsFilter = frame.Filter<Transform3D, SpawnPoint>();
            while (spawnsFilter.NextUnsafe(out var spawnEntity, out var spawnTransform, out var spawnPoint))
            {
                // check if spawn point is valid
                if (spawnPoint->IsAvailable == false)
                    continue;

                if (spawnNearHole && spawnPoint->Type != ESpawnPointType.NearHole)
                    continue;

                if (spawnNearHole == false && spawnPoint->Type == ESpawnPointType.NearHole)
                    continue;

                // update ball
                Transform3D* ballTransform = frame.Unsafe.GetPointer<Transform3D>(ball);
                ballTransform->Position = spawnTransform->Position;
                ballTransform->Rotation = FPQuaternion.Identity;

                BallFields* fields = frame.Unsafe.GetPointer<BallFields>(ball);
                fields->LastPosition = spawnTransform->Position;

                // update spawn point
                spawnPoint->IsAvailable = false;

                break;
            }
        }
    }
}