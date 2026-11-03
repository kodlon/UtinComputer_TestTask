using DG.Tweening;
using TMPro;
using UnityEngine;

namespace BallGame.UI
{
    public sealed class HudView : MonoBehaviour
    {
        private const float HintPulseDuration = 0.6f;

        private const float HintPulseScale = 1.06f;

        private const float LevelPopDuration = 0.5f;

        [SerializeField]
        private GameObject _hint;

        [SerializeField]
        private TMP_Text _levelLabel;

        [SerializeField]
        private RectTransform _sizeFill;

        private float _criticalRadius;

        private float _startRadius;

        public void HideHint()
        {
            _hint.transform.DOKill();
            _hint.SetActive(false);
        }

        public void Initialize(int levelNumber, float startRadius, float criticalRadius)
        {
            _levelLabel.text = $"LEVEL {levelNumber}";
            _startRadius = startRadius;
            _criticalRadius = criticalRadius;
            _hint.SetActive(true);
            SetRadius(startRadius);

            _levelLabel.transform.localScale = Vector3.zero;
            _levelLabel.transform.DOScale(1f, LevelPopDuration).SetEase(Ease.OutBack).SetLink(gameObject);
            _hint.transform.DOScale(HintPulseScale, HintPulseDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(_hint);
        }

        // Empty bar means the critical radius: one more charge and the ball is lost.
        public void SetRadius(float radius)
        {
            var fill = Mathf.InverseLerp(_criticalRadius, _startRadius, radius);
            _sizeFill.anchorMax = new Vector2(fill, _sizeFill.anchorMax.y);
        }
    }
}
