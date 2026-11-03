using System;
using UnityEngine;

namespace BallGame.Model
{
    public sealed class LevelModel
    {
        private readonly bool[] _alive;

        private readonly Obstacle[] _obstacles;

        public LevelModel(Vector2 startPoint, Vector2 doorPoint, Obstacle[] obstacles)
        {
            StartPoint = startPoint;
            DoorPoint = doorPoint;
            _obstacles = obstacles;
            _alive = new bool[obstacles.Length];
            Array.Fill(_alive, true);
        }

        private LevelModel(LevelModel source)
        {
            StartPoint = source.StartPoint;
            DoorPoint = source.DoorPoint;
            _obstacles = source._obstacles;
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
    }
}
