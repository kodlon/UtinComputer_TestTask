using System;
using System.Collections.Generic;
using UnityEngine;

namespace BallGame.Model
{
    public sealed class LevelModel
    {
        private readonly bool[] _alive;

        private readonly int[][] _neighbours;

        private readonly Obstacle[] _obstacles;

        public LevelModel(Vector2 startPoint, Vector2 doorPoint, Obstacle[] obstacles, float neighbourGap)
        {
            StartPoint = startPoint;
            DoorPoint = doorPoint;
            _obstacles = obstacles;
            _neighbours = BuildNeighbours(obstacles, neighbourGap);
            _alive = new bool[obstacles.Length];
            Array.Fill(_alive, true);
        }

        private LevelModel(LevelModel source)
        {
            StartPoint = source.StartPoint;
            DoorPoint = source.DoorPoint;
            _obstacles = source._obstacles;
            _neighbours = source._neighbours;
            _alive = (bool[])source._alive.Clone();
        }

        public Vector2 DoorPoint { get; }

        public Vector2 LaneDirection => (DoorPoint - StartPoint).normalized;

        public float LaneLength => Vector2.Distance(StartPoint, DoorPoint);

        public int ObstacleCount => _obstacles.Length;

        public Vector2 StartPoint { get; }

        public LevelModel Clone()
        {
            return new LevelModel(this);
        }

        // Allocation-free reset for models cloned from the same level.
        public void CopyStateFrom(LevelModel source)
        {
            Array.Copy(source._alive, _alive, _alive.Length);
        }

        public ReadOnlySpan<int> GetNeighbours(int index)
        {
            return _neighbours[index];
        }

        public Obstacle GetObstacle(int index)
        {
            return _obstacles[index];
        }

        public bool IsAlive(int index)
        {
            return _alive[index];
        }

        public void Kill(int index)
        {
            _alive[index] = false;
        }

        private static float Gap(Obstacle a, Obstacle b)
        {
            return Vector2.Distance(a.Position, b.Position) - a.Radius - b.Radius;
        }

        // Geometry never changes, so neighbours within the max infection gap are found once.
        private static int[][] BuildNeighbours(Obstacle[] obstacles, float maxGap)
        {
            var neighbours = new int[obstacles.Length][];
            var buffer = new List<int>();

            for (var i = 0; i < obstacles.Length; i++)
            {
                buffer.Clear();

                for (var j = 0; j < obstacles.Length; j++)
                {
                    if (i != j && Gap(obstacles[i], obstacles[j]) <= maxGap)
                    {
                        buffer.Add(j);
                    }
                }

                neighbours[i] = buffer.ToArray();
            }

            return neighbours;
        }
    }
}
