using BallGame.Common;
using DG.Tweening;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class Door : MonoBehaviour
    {
        private const float PunchDuration = 0.4f;

        private static readonly Vector3 OpenPunch = new(0.08f, 0.12f, 0.08f);

        [SerializeField]
        private ParticleSystem _confetti;

        [SerializeField]
        private Transform _leftLeaf;

        [SerializeField]
        private Transform _rightLeaf;

        [SerializeField]
        [Range(0f, 180f)]
        private float _openAngle = 100f;

        [SerializeField]
        [Min(0.01f)]
        private float _openDuration = 0.6f;

        private bool _isOpen;

        private float _openDistance;

        private Transform _visitor;

        public void Celebrate()
        {
            _confetti.Play();
        }

        public void Place(Vector2 position, Vector2 facing, Transform visitor, float openDistance)
        {
            transform.SetPositionAndRotation(position.ToXZ(), Quaternion.LookRotation(facing.ToXZ()));
            _visitor = visitor;
            _openDistance = openDistance;
        }

        private void Update()
        {
            if (_isOpen || _visitor == null)
            {
                return;
            }

            var distance = Vector2.Distance(_visitor.position.ToPlanar(), transform.position.ToPlanar());

            if (distance <= _openDistance)
            {
                _isOpen = true;
                Open();
            }
        }

        // Leaves swing away from the approaching ball: the door faces the ball, so "away" is local -Z.
        private void Open()
        {
            _leftLeaf.DOLocalRotate(new Vector3(0f, _openAngle, 0f), _openDuration).SetEase(Ease.OutBack).SetLink(gameObject);
            _rightLeaf.DOLocalRotate(new Vector3(0f, -_openAngle, 0f), _openDuration).SetEase(Ease.OutBack).SetLink(gameObject);
            transform.DOPunchScale(OpenPunch, PunchDuration, 6).SetLink(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            GizmoDraw.CircleXZ(transform.position.ToPlanar(), _openDistance);
        }
    }
}
