using BallGame.Common;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class LanePath : MonoBehaviour
    {
        // Shorter lanes are hidden: near the door the direction degenerates and LookRotation would complain.
        private const float MinVisibleLength = 0.05f;

        [SerializeField]
        private Transform _strip;

        [SerializeField]
        [Min(0f)]
        private float _groundOffset = 0.01f;

        private Vector2 _end;

        public void Refresh(Vector2 from, float width)
        {
            var delta = _end - from;
            var length = delta.magnitude;
            var isVisible = length > MinVisibleLength;

            if (_strip.gameObject.activeSelf != isVisible)
                _strip.gameObject.SetActive(isVisible);

            if (!isVisible)
                return;

            var rotation = Quaternion.LookRotation(delta.ToXZ()) * Quaternion.Euler(90f, 0f, 0f);
            _strip.SetPositionAndRotation(((from + _end) * 0.5f).ToXZ(_groundOffset), rotation);
            _strip.localScale = new Vector3(width, length, 1f);
        }

        public void Setup(Vector2 end)
        {
            _end = end;
        }
    }
}