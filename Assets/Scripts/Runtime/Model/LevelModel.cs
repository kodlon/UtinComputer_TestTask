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
            LaneDirection = (doorPoint - startPoint).normalized;
            _obstacles = obstacles;
            _alive = new bool[obstacles.Length];
            Array.Fill(_alive, true);
        }

        private LevelModel(LevelModel source)
        {
            StartPoint = source.StartPoint;
            DoorPoint = source.DoorPoint;
            LaneDirection = source.LaneDirection;
            _obstacles = source._obstacles;
            _alive = (bool[])source._alive.Clone();
        }

        public Vector2 DoorPoint { get; }

        public Vector2 LaneDirection { get; }

        public int ObstacleCount => _obstacles.Length;

        public Vector2 StartPoint { get; }

        public LevelModel Clone()
        {
            return new LevelModel(this);
        }

        public void CopyStateFrom(LevelModel source)
        {
            Array.Copy(source._alive, _alive, _alive.Length);
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
