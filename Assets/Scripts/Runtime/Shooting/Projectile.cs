using System;
using BallGame.Common;
using DG.Tweening;
using UnityEngine;

namespace BallGame.Shooting
{
    public sealed class Projectile : MonoBehaviour
    {
        private const float MinFlightDuration = 0.18f;

        [SerializeField]
        private TrailRenderer _trail;

        private Tween _flight;

        public event Action Arrived;

        public void Hide()
        {
            _flight?.Kill();
            _trail.emitting = false;
            gameObject.SetActive(false);
        }

        public void Launch(Vector2 target, float speed)
        {
            _trail.Clear();
            _trail.widthMultiplier = transform.localScale.x;
            _trail.emitting = true;

            var destination = target.ToXZ(transform.position.y);
            var duration = Mathf.Max(Vector3.Distance(transform.position, destination) / speed, MinFlightDuration);

            _flight = transform.DOMove(destination, duration)
                .SetEase(Ease.InQuad)
                .SetLink(gameObject)
                .OnComplete(OnArrived);
        }

        public void Place(Vector2 position, float radius)
        {
            transform.position = position.ToXZ(radius);
            transform.localScale = Vector3.one * (radius * 2f);
        }

        public void Show(Vector2 position, float radius)
        {
            _trail.emitting = false;
            _trail.Clear();
            gameObject.SetActive(true);
            Place(position, radius);
        }

        private void OnArrived()
        {
            Hide();
            Arrived?.Invoke();
        }
    }
}
