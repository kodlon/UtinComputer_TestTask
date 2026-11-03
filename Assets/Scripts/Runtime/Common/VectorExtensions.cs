using UnityEngine;

namespace BallGame.Common
{
    public static class VectorExtensions
    {
        public static Vector2 ToPlanar(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        public static Vector3 ToXZ(this Vector2 vector, float y = 0f)
        {
            return new Vector3(vector.x, y, vector.y);
        }
    }
}
