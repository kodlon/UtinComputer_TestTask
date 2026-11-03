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
            var minShotVolume = VolumeMath.VolumeFromRadius(_config.MinShotRadius);
            var criticalVolume = VolumeMath.VolumeFromRadius(_config.CriticalRadius);

            return playerVolume - minShotVolume > criticalVolume;
        }

        public float ChargeRate(float holdTime)
        {
            return _config.BaseChargeRate * _config.ChargeCurve.Evaluate(holdTime);
        }

        public float CorridorHalfWidth(float playerRadius)
        {
            return playerRadius + _config.LaneSideMargin;
        }

        public float FreeDistance(LevelModel level, Vector2 position, float playerRadius)
        {
            var toDoor = level.DoorPoint - position;
            var distanceToDoor = toDoor.magnitude;

            if (distanceToDoor <= Mathf.Epsilon)
            {
                return 0f;
            }

            var direction = toDoor / distanceToDoor;
            var halfWidth = CorridorHalfWidth(playerRadius);

            if (!TryFindFirstHit(level, position, direction, halfWidth, distanceToDoor, out _, out var hitDistance))
            {
                return distanceToDoor;
            }

            return Mathf.Max(0f, hitDistance - _config.MoveSafetyGap);
        }

        public float InfectionEnergy(float shotRadius)
        {
            return _config.InfectionEnergyFactor * shotRadius;
        }

        // Swept circle vs. obstacle circles: returns the first alive obstacle the moving circle touches.
        public bool TryFindFirstHit(
            LevelModel level,
            Vector2 origin,
            Vector2 direction,
            float radius,
            float maxDistance,
            out int obstacleIndex,
            out float hitDistance)
        {
            obstacleIndex = -1;
            hitDistance = maxDistance;

            for (var i = 0; i < level.ObstacleCount; i++)
            {
                if (!level.IsAlive(i))
                {
                    continue;
                }

                var obstacle = level.GetObstacle(i);
                var toCenter = obstacle.Position - origin;
                var along = Vector2.Dot(toCenter, direction);

                if (along < 0f)
                {
                    continue;
                }

                var contactDistance = radius + obstacle.Radius;
                var contactDistanceSqr = contactDistance * contactDistance;
                var perpendicularSqr = toCenter.sqrMagnitude - along * along;

                if (perpendicularSqr >= contactDistanceSqr)
                {
                    continue;
                }

                var distance = Mathf.Max(0f, along - Mathf.Sqrt(contactDistanceSqr - perpendicularSqr));

                if (distance >= hitDistance)
                {
                    continue;
                }

                hitDistance = distance;
                obstacleIndex = i;
            }

            return obstacleIndex >= 0;
        }
    }
}
