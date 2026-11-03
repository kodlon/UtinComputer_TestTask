using UnityEngine;

namespace BallGame.Model
{
    public readonly struct SolverShot
    {
        public SolverShot(Vector2 playerPosition, float shotRadius, Vector2 hitPoint)
        {
            PlayerPosition = playerPosition;
            ShotRadius = shotRadius;
            HitPoint = hitPoint;
        }

        public Vector2 HitPoint { get; }

        public Vector2 PlayerPosition { get; }

        public float ShotRadius { get; }
    }
}
