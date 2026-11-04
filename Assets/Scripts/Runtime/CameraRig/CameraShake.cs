using DG.Tweening;
using UnityEngine;

namespace BallGame.CameraRig
{
    public sealed class CameraShake : MonoBehaviour
    {
        private const int Vibrato = 18;

        [SerializeField]
        [Min(0f)]
        private float _duration = 0.25f;

        [SerializeField]
        [Min(0f)]
        private float _maxStrength = 0.35f;

        private Tween _shake;

        public void Shake(float strength01)
        {
            _shake?.Complete();
            transform.localPosition = Vector3.zero;

            _shake = transform.DOShakePosition(_duration, _maxStrength * Mathf.Clamp01(strength01), Vibrato)
                .SetLink(gameObject)
                .OnComplete(ResetPosition);
        }

        private void ResetPosition()
        {
            transform.localPosition = Vector3.zero;
        }
    }
}