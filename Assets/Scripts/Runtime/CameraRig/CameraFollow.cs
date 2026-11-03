using UnityEngine;

namespace BallGame.CameraRig
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _offset = new(2.5f, 15f, -5.5f);

        [SerializeField]
        [Min(0f)]
        private float _smoothTime = 0.35f;

        private Transform _target;

        private Vector3 _velocity;

        public void SetTarget(Transform target)
        {
            _target = target;
            _velocity = Vector3.zero;
            transform.position = DesiredPosition();
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, DesiredPosition(), ref _velocity, _smoothTime);
        }

        // Follow the ground point under the target so the camera doesn't bob with hops or shrinking.
        private Vector3 DesiredPosition()
        {
            var anchor = _target.position;
            anchor.y = 0f;

            return anchor + _offset;
        }
    }
}
