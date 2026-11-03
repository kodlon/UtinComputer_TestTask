using UnityEngine;

namespace BallGame.Common
{
    public static class GizmoDraw
    {
        private const int CircleSegments = 24;

        private const float GroundOffset = 0.05f;

        public static void CircleXZ(Vector2 center, float radius)
        {
            var previous = (center + new Vector2(radius, 0f)).ToXZ(GroundOffset);

            for (var i = 1; i <= CircleSegments; i++)
            {
                var angle = i * Mathf.PI * 2f / CircleSegments;
                var next = (center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius).ToXZ(GroundOffset);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        public static void CorridorXZ(Vector2 from, Vector2 to, float halfWidth)
        {
            var direction = (to - from).normalized;
            var side = new Vector2(-direction.y, direction.x) * halfWidth;

            Gizmos.DrawLine((from + side).ToXZ(GroundOffset), (to + side).ToXZ(GroundOffset));
            Gizmos.DrawLine((from - side).ToXZ(GroundOffset), (to - side).ToXZ(GroundOffset));
            Gizmos.DrawLine(from.ToXZ(GroundOffset), to.ToXZ(GroundOffset));
        }
    }
}
