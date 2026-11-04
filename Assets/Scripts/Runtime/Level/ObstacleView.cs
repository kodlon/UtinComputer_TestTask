using System;
using BallGame.Common;
using DG.Tweening;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class ObstacleView : MonoBehaviour
    {
        private const float CapsuleMeshHeight = 2f;

        private const float InfectedSwell = 1.2f;

        private const float PopDuration = 0.12f;

        [SerializeField]
        private Material _infectedMaterial;

        [SerializeField]
        private MeshRenderer _renderer;

        [SerializeField]
        private Material _targetedMaterial;

        private Vector3 _baseScale;

        private Action<ObstacleView> _exploded;

        private Material _idleMaterial;

        public void Infect(float delay, float explodeDelay)
        {
            DOTween.Sequence()
                .AppendInterval(delay)
                .AppendCallback(OnInfected)
                .Append(transform.DOScale(_baseScale * InfectedSwell, explodeDelay).SetEase(Ease.OutBack))
                .Join(transform.DOShakeRotation(explodeDelay, 8f, 12))
                .Append(transform.DOScale(Vector3.zero, PopDuration).SetEase(Ease.InBack))
                .SetLink(gameObject)
                .OnComplete(OnExploded);
        }

        public void SetTargeted(bool isTargeted)
        {
            _renderer.sharedMaterial = isTargeted ? _targetedMaterial : _idleMaterial;
        }

        public void Setup(
            Vector2 position,
            float radius,
            float height,
            Quaternion tilt,
            Material material,
            Action<ObstacleView> exploded)
        {
            _exploded = exploded;
            _baseScale = new Vector3(radius * 2f, height / CapsuleMeshHeight, radius * 2f);
            _idleMaterial = material;
            _renderer.sharedMaterial = material;

            var center = position.ToXZ() + tilt * (Vector3.up * (height * 0.5f));
            transform.SetLocalPositionAndRotation(center, tilt);
            transform.localScale = _baseScale;
        }

        private void OnExploded()
        {
            gameObject.SetActive(false);
            _exploded?.Invoke(this);
        }

        private void OnInfected()
        {
            _renderer.sharedMaterial = _infectedMaterial;
        }
    }
}