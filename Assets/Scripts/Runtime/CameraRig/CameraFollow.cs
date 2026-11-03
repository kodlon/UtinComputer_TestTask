using BallGame.Common;
using UnityEngine;

namespace BallGame.CameraRig
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private Vector3 _offset = new(2.5f, 15f, -5.5f);

        [SerializeField]
        [Min(0f)]
        private float _smoothTime = 0.35f;

        private Rect _anchorBounds;

        private Transform _target;

        private Vector3 _velocity;

        // The followed point is clamped so the screen corners never leave the obstacle field, whatever the aspect ratio.
        public void SetTarget(Transform target, Rect fieldBounds)
        {
            _target = target;
            _velocity = Vector3.zero;

            var anchor = Anchor();
            transform.position = anchor + _offset;
            _anchorBounds = ComputeAnchorBounds(anchor.ToPlanar(), fieldBounds);
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
        private Vector3 Anchor()
        {
            var anchor = _target.position;
            anchor.y = 0f;

            return anchor;
        }

        private Vector3 DesiredPosition()
        {
            var anchor = Anchor().ToPlanar();
            anchor.x = Mathf.Clamp(anchor.x, _anchorBounds.xMin, _anchorBounds.xMax);
            anchor.y = Mathf.Clamp(anchor.y, _anchorBounds.yMin, _anchorBounds.yMax);

            return anchor.ToXZ() + _offset;
        }

        // Where the four screen corners hit the ground, relative to the anchor, defines how far the anchor may move.
        private Rect ComputeAnchorBounds(Vector2 anchor, Rect field)
        {
            var ground = new Plane(Vector3.up, Vector3.zero);
            var minOffset = new Vector2(float.MaxValue, float.MaxValue);
            var maxOffset = new Vector2(float.MinValue, float.MinValue);

            for (var corner = 0; corner < 4; corner++)
            {
                var ray = _camera.ViewportPointToRay(new Vector3(corner % 2, corner / 2, 0f));

                if (!ground.Raycast(ray, out var distance))
                {
                    continue;
                }

                var offset = ray.GetPoint(distance).ToPlanar() - anchor;
                minOffset = Vector2.Min(minOffset, offset);
                maxOffset = Vector2.Max(maxOffset, offset);
            }

            var min = field.min - minOffset;
            var max = field.max - maxOffset;

            // A field narrower than the view can't be fully covered: keep the camera centred on it.
            if (min.x > max.x)
            {
                min.x = max.x = (min.x + max.x) * 0.5f;
            }

            if (min.y > max.y)
            {
                min.y = max.y = (min.y + max.y) * 0.5f;
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
    }
}
