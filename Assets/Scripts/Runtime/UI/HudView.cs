using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    public sealed class HudView : MonoBehaviour
    {
        private const float DangerBlinkDuration = 0.25f;

        private const float HintPulseDuration = 0.6f;

        private const float HintPulseScale = 1.06f;

        private const float LevelPopDuration = 0.5f;

        [SerializeField]
        private GameObject _hint;

        [SerializeField]
        private TMP_Text _levelLabel;

        [SerializeField]
        private RectTransform _sizeFill;

        [SerializeField]
        private Image _sizeFillImage;

        [SerializeField]
        private Color _dangerColor = new(1f, 0.3f, 0.25f);

        [SerializeField]
        [Range(0f, 1f)]
        private float _dangerThreshold = 0.25f;

        private float _criticalRadius;

        private Tween _dangerBlink;

        private bool _isInDanger;

        private Color _normalColor;

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
            _normalColor = _sizeFillImage.color;
            _hint.SetActive(true);
            SetRadius(startRadius);

            _levelLabel.transform.localScale = Vector3.zero;
            _levelLabel.transform.DOScale(1f, LevelPopDuration).SetEase(Ease.OutBack).SetLink(gameObject);
            _hint.transform.DOScale(HintPulseScale, HintPulseDuration).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(_hint);
        }

        public void SetRadius(float radius)
        {
            var fill = Mathf.InverseLerp(_criticalRadius, _startRadius, radius);
            _sizeFill.anchorMax = new Vector2(fill, _sizeFill.anchorMax.y);
            SetDanger(fill < _dangerThreshold);
        }

        private void SetDanger(bool isInDanger)
        {
            if (_isInDanger == isInDanger)
            {
                return;
            }

            _isInDanger = isInDanger;
            _dangerBlink?.Kill();
            _sizeFillImage.color = _normalColor;

            if (isInDanger)
            {
                _dangerBlink = DOTween.To(() => _sizeFillImage.color, color => _sizeFillImage.color = color, _dangerColor, DangerBlinkDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetLink(gameObject);
            }
        }
    }
}
