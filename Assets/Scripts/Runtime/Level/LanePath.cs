using BallGame.Common;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class LanePath : MonoBehaviour
    {
        [SerializeField]
        private Transform _strip;

        [SerializeField]
        [Min(0f)]
        private float _groundOffset = 0.01f;

        private Vector2 _end;

        public void Setup(Vector2 end)
        {
            _end = end;
        }

        public void Refresh(Vector2 from, float width)
        {
            var delta = _end - from;
            var length = delta.magnitude;
            var isVisible = length > Mathf.Epsilon;

            _strip.gameObject.SetActive(isVisible);

            if (!isVisible)
            {
                return;
            }

            // The quad lies in its local XY plane: tilt it flat, then align its local Y with the lane.
            var rotation = Quaternion.LookRotation(delta.ToXZ()) * Quaternion.Euler(90f, 0f, 0f);
            _strip.SetPositionAndRotation(((from + _end) * 0.5f).ToXZ(_groundOffset), rotation);
            _strip.localScale = new Vector3(width, length, 1f);
        }
    }
}
