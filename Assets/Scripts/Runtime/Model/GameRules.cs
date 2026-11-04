using System.Collections.Generic;
using BallGame.Configs;
using UnityEngine;

namespace BallGame.Model
{
    public sealed class GameRules
    {
        private readonly GameBalanceConfig _config;

        public GameRules(GameBalanceConfig config)
        {
            _config = config;
        }

        public bool CanAffordShot(float playerVolume)
        {
            return playerVolume - VolumeMath.VolumeFromRadius(_config.CriticalRadius) >= VolumeMath.VolumeFromRadius(_config.MinShotRadius);
        }

        public float FreeDistance(LevelModel level, Vector2 position, float playerRadius)
        {
            if (!TryFindBlocker(level, position, playerRadius, out _, out var distance))
            {
                return Vector2.Distance(position, level.DoorPoint);
            }

            return Mathf.Max(0f, distance - _config.MoveSafetyGap);
        }

        // Grows a bit faster than the shot volume (power above 3): one shot over a whole dense group beats several small
        // ones, while a too big shot wastes its reach on the empty glades and a lone obstacle needs only the smallest shot.
        public float InfectionRadius(float shotRadius)
        {
            return _config.InfectionRadiusFactor * Mathf.Pow(shotRadius, _config.InfectionRadiusPower);
        }

        public Vector2 ShotOrigin(LevelModel level, Vector2 playerPosition, float playerRadius, float shotRadius)
        {
            return playerPosition + level.LaneDirection * (playerRadius + shotRadius);
        }

        // Plays the shot on the trial copy: what it infects and how far the ball then moves.
        public bool TryPredictShot(
            LevelModel level,
            LevelModel trial,
            Vector2 playerPosition,
            float playerRadius,
            float shotRadius,
            List<int> infected,
            out float advance)
        {
            trial.CopyStateFrom(level);

            if (!TryResolveShot(trial, playerPosition, playerRadius, shotRadius, infected, out _))
            {
                advance = 0f;
                return false;
            }

            foreach (var index in infected)
            {
                trial.Kill(index);
            }

            advance = FreeDistance(trial, playerPosition, playerRadius);
            return true;
        }

        // Doesn't kill: the game does it when the projectile arrives, the solver on a trial copy.
        public bool TryResolveShot(
            LevelModel level,
            Vector2 playerPosition,
            float playerRadius,
            float shotRadius,
            List<int> infected,
            out Vector2 impactPoint)
        {
            infected.Clear();

            if (!TryFindBlocker(level, playerPosition, playerRadius, out var hitIndex, out _))
            {
                impactPoint = level.DoorPoint;
                return false;
            }

            var hit = level.GetObstacle(hitIndex);
            var fromHit = ShotOrigin(level, playerPosition, playerRadius, shotRadius) - hit.Position;
            impactPoint = hit.Position + Vector2.ClampMagnitude(fromHit, hit.Radius + shotRadius);

            var reach = InfectionRadius(shotRadius);

            for (var i = 0; i < level.ObstacleCount; i++)
            {
                var obstacle = level.GetObstacle(i);

                if (level.IsAlive(i) && Vector2.Distance(impactPoint, obstacle.Position) - obstacle.Radius <= reach)
                {
                    infected.Add(i);
                }
            }

            return true;
        }

        private bool TryFindBlocker(LevelModel level, Vector2 position, float playerRadius, out int obstacleIndex, out float distance)
        {
            var direction = level.LaneDirection;
            var radius = playerRadius * _config.BallColliderScale;

            obstacleIndex = -1;
            distance = Vector2.Dot(level.DoorPoint - position, direction);

            for (var i = 0; i < level.ObstacleCount; i++)
            {
                if (!level.IsAlive(i))
                {
                    continue;
                }

                var obstacle = level.GetObstacle(i);
                var toCenter = obstacle.Position - position;
                var along = Vector2.Dot(toCenter, direction);
                var contact = radius + obstacle.Radius;
                var overlapSqr = contact * contact - (toCenter.sqrMagnitude - along * along);

                if (overlapSqr <= 0f)
                {
                    continue;
                }

                var overlap = Mathf.Sqrt(overlapSqr);
                var hitDistance = Mathf.Max(0f, along - overlap);

                if (along + overlap >= 0f && hitDistance < distance)
                {
                    distance = hitDistance;
                    obstacleIndex = i;
                }
            }

            return obstacleIndex >= 0;
        }
    }
}
