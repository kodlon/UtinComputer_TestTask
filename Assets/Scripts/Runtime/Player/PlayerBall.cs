using BallGame.Common;
using BallGame.Model;
using DG.Tweening;
using UnityEngine;

namespace BallGame.Player
{
    public sealed class PlayerBall : MonoBehaviour
    {
        private const float ChargeWobbleDuration = 0.12f;

        [SerializeField]
        private Transform _visual;

        private static readonly Vector3 ChargeWobbleScale = new(1.06f, 0.94f, 1.06f);

        private Tween _chargeTween;

        public Vector2 Position => transform.position.ToPlanar();

        public float Radius { get; private set; }

        public Transform Visual => _visual;

        public float Volume { get; private set; }

        public void Initialize(Vector2 position, float volume)
        {
            transform.position = position.ToXZ();
            SetVolume(volume);
        }

        public void SetCharging(bool isCharging)
        {
            _chargeTween?.Kill();
            _visual.localScale = Vector3.one;

            if (isCharging)
            {
                _chargeTween = _visual.DOScale(ChargeWobbleScale, ChargeWobbleDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetLink(gameObject);
            }
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = position.ToXZ(Radius);
        }

        public void SetVolume(float volume)
        {
            Volume = Mathf.Max(0f, volume);
            Radius = VolumeMath.RadiusFromVolume(Volume);

            transform.position = Position.ToXZ(Radius);
            transform.localScale = Vector3.one * (Radius * 2f);
        }
    }
}