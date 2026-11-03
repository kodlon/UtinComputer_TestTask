using System;
using BallGame.Common;
using UnityEngine;

namespace BallGame.Shooting
{
    public sealed class Projectile : MonoBehaviour
    {
        private bool _isFlying;

        private float _speed;

        private Vector3 _target;

        public event Action Arrived;

        public void Hide()
        {
            _isFlying = false;
            gameObject.SetActive(false);
        }

        public void Launch(Vector2 target, float speed)
        {
            _target = target.ToXZ(transform.position.y);
            _speed = speed;
            _isFlying = true;
        }

        public void Place(Vector2 position, float radius)
        {
            transform.position = position.ToXZ(radius);
            transform.localScale = Vector3.one * (radius * 2f);
        }

        public void Show(Vector2 position, float radius)
        {
            gameObject.SetActive(true);
            Place(position, radius);
        }

        private void Update()
        {
            if (!_isFlying)
            {
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);

            if (transform.position != _target)
            {
                return;
            }

            Hide();
            Arrived?.Invoke();
        }
    }
}
