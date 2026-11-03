using BallGame.Common;
using DG.Tweening;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class Monument : MonoBehaviour
    {
        private const float CelebratePunch = 0.6f;

        private const float CelebrateDuration = 0.7f;

        [SerializeField]
        private Transform _crown;

        [SerializeField]
        private Transform _flame;

        [SerializeField]
        [Min(0.1f)]
        private float _crownSpinDuration = 14f;

        [SerializeField]
        [Min(0f)]
        private float _flameBobHeight = 0.25f;

        [SerializeField]
        [Min(0.1f)]
        private float _flameBobDuration = 1.2f;

        public void Celebrate()
        {
            _flame.DOPunchScale(Vector3.one * CelebratePunch, CelebrateDuration, 6).SetLink(gameObject);
            _crown.DOPunchScale(Vector3.one * (CelebratePunch * 0.3f), CelebrateDuration, 6).SetLink(gameObject);
        }

        public void Place(Vector2 position, Vector2 facing)
        {
            transform.SetPositionAndRotation(position.ToXZ(), Quaternion.LookRotation(facing.ToXZ()));
        }

        private void Start()
        {
            _crown.DOLocalRotate(new Vector3(0f, 360f, 0f), _crownSpinDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1)
                .SetLink(gameObject);

            _flame.DOLocalMoveY(_flame.localPosition.y + _flameBobHeight, _flameBobDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }
    }
}
