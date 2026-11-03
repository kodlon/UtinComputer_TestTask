using System;
using System.Collections.Generic;
using BallGame.Configs;
using UnityEngine;

namespace BallGame.Model
{
    public sealed class GameRules
    {
        private readonly GameBalanceConfig _config;

        private readonly List<int> _openSet = new();

        private float[] _bestEnergy = Array.Empty<float>();

        private bool[] _visited = Array.Empty<bool>();

        public GameRules(GameBalanceConfig config)
        {
            _config = config;
        }

        public bool CanAffordShot(float playerVolume)
        {
            return MaxShotVolume(playerVolume) > VolumeMath.VolumeFromRadius(_config.MinShotRadius);
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

        // Energy is the infection reach in hops, so the infected area grows with energy². E ∝ r^1.5 makes that area
        // proportional to the shot volume: efficiency per spent size is flat and only obstacle density gives a bonus.
        public float InfectionEnergy(float shotRadius)
        {
            return _config.InfectionEnergyFactor * shotRadius * Mathf.Sqrt(shotRadius);
        }

        // Largest shot volume that still leaves the player above the critical radius.
        public float MaxShotVolume(float playerVolume)
        {
            return playerVolume - VolumeMath.VolumeFromRadius(_config.CriticalRadius);
        }

        public Vector2 ShotOrigin(Vector2 playerPosition, Vector2 direction, float playerRadius, float shotRadius)
        {
            return playerPosition + direction * (playerRadius + shotRadius);
        }

        // Dijkstra-like spread that keeps the highest remaining energy per obstacle.
        // Dense groups have small gaps, so hops are cheap and the chain travels further.
        public void SpreadInfection(LevelModel level, int firstIndex, float energy, List<InfectionHit> infected)
        {
            infected.Clear();
            PrepareBuffers(level.ObstacleCount);

            var firstEnergy = energy - _config.ChainHopCost;

            if (firstEnergy < 0f)
            {
                return;
            }

            _bestEnergy[firstIndex] = firstEnergy;
            _openSet.Add(firstIndex);

            while (_openSet.Count > 0)
            {
                var current = PopHighestEnergy();
                var currentEnergy = _bestEnergy[current];
                var source = level.GetObstacle(current);

                _visited[current] = true;
                infected.Add(new InfectionHit(current, energy - currentEnergy));

                foreach (var i in level.GetNeighbours(current))
                {
                    if (_visited[i] || !level.IsAlive(i))
                    {
                        continue;
                    }

                    var neighbour = level.GetObstacle(i);
                    var gap = Vector2.Distance(source.Position, neighbour.Position) - source.Radius - neighbour.Radius;

                    if (gap > _config.MaxChainGap)
                    {
                        continue;
                    }

                    var remaining = currentEnergy - _config.ChainHopCost - Mathf.Max(gap, 0f) * _config.ChainGapCostFactor;

                    if (remaining < 0f || remaining <= _bestEnergy[i])
                    {
                        continue;
                    }

                    if (_bestEnergy[i] < 0f)
                    {
                        _openSet.Add(i);
                    }

                    _bestEnergy[i] = remaining;
                }
            }
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

        // Shared by the game and the solver so both resolve a shot identically. Does not kill obstacles.
        // The shot hits the first obstacle blocking the player's lane, so an obstacle at the lane edge can't be skipped.
        public bool TryResolveShot(
            LevelModel level,
            Vector2 playerPosition,
            float playerRadius,
            float shotRadius,
            List<InfectionHit> infected,
            out Vector2 hitPoint)
        {
            infected.Clear();

            var direction = level.LaneDirection;
            var halfWidth = CorridorHalfWidth(playerRadius);
            var maxDistance = Vector2.Distance(playerPosition, level.DoorPoint);

            if (!TryFindFirstHit(level, playerPosition, direction, halfWidth, maxDistance, out var hitIndex, out _))
            {
                hitPoint = default;
                return false;
            }

            var obstacle = level.GetObstacle(hitIndex);
            var origin = ShotOrigin(playerPosition, direction, playerRadius, shotRadius);
            hitPoint = obstacle.Position + (origin - obstacle.Position).normalized * (obstacle.Radius + shotRadius);
            SpreadInfection(level, hitIndex, InfectionEnergy(shotRadius), infected);

            return true;
        }

        private int PopHighestEnergy()
        {
            var bestSlot = 0;

            for (var slot = 1; slot < _openSet.Count; slot++)
            {
                if (_bestEnergy[_openSet[slot]] > _bestEnergy[_openSet[bestSlot]])
                {
                    bestSlot = slot;
                }
            }

            var index = _openSet[bestSlot];
            var lastSlot = _openSet.Count - 1;
            _openSet[bestSlot] = _openSet[lastSlot];
            _openSet.RemoveAt(lastSlot);

            return index;
        }

        private void PrepareBuffers(int obstacleCount)
        {
            if (_bestEnergy.Length < obstacleCount)
            {
                _bestEnergy = new float[obstacleCount];
                _visited = new bool[obstacleCount];
            }

            Array.Fill(_bestEnergy, -1f, 0, obstacleCount);
            Array.Fill(_visited, false, 0, obstacleCount);
            _openSet.Clear();
        }
    }
}
