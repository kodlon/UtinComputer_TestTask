using UnityEngine;

namespace BallGame.Model
{
    public readonly struct Obstacle
    {
        public Obstacle(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public Vector2 Position { get; }

        public float Radius { get; }
    }
}
