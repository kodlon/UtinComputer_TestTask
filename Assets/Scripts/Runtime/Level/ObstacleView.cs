using BallGame.Common;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class ObstacleView : MonoBehaviour
    {
        // Unity capsule mesh is 2 units tall and 1 unit wide at scale 1.
        private const float CapsuleMeshHeight = 2f;

        public void Setup(Vector2 position, float radius, float height)
        {
            transform.SetLocalPositionAndRotation(position.ToXZ(height * 0.5f), Quaternion.identity);
            transform.localScale = new Vector3(radius * 2f, height / CapsuleMeshHeight, radius * 2f);
        }
    }
}
