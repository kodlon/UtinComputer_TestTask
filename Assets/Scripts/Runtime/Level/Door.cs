using System.Collections;
using BallGame.Common;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class Door : MonoBehaviour
    {
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
                StartCoroutine(OpenRoutine());
            }
        }

        // Leaves swing away from the approaching ball: the door faces the ball, so "away" is local -Z.
        private IEnumerator OpenRoutine()
        {
            var leftOpen = Quaternion.Euler(0f, _openAngle, 0f);
            var rightOpen = Quaternion.Euler(0f, -_openAngle, 0f);

            for (var time = 0f; time < _openDuration; time += Time.deltaTime)
            {
                var progress = Mathf.SmoothStep(0f, 1f, time / _openDuration);
                _leftLeaf.localRotation = Quaternion.Slerp(Quaternion.identity, leftOpen, progress);
                _rightLeaf.localRotation = Quaternion.Slerp(Quaternion.identity, rightOpen, progress);
                yield return null;
            }

            _leftLeaf.localRotation = leftOpen;
            _rightLeaf.localRotation = rightOpen;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            GizmoDraw.CircleXZ(transform.position.ToPlanar(), _openDistance);
        }
    }
}
