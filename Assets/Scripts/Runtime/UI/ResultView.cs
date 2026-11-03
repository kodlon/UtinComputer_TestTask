using System;
using BallGame.Core;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    public sealed class ResultView : MonoBehaviour
    {
        private const float ButtonDelay = 0.25f;

        private const float FadeDuration = 0.25f;

        private const float PopDuration = 0.4f;

        [SerializeField]
        private CanvasGroup _group;

        [SerializeField]
        private Button _nextButton;

        [SerializeField]
        private Button _retryButton;

        [SerializeField]
        private TMP_Text _subtitle;

        [SerializeField]
        private TMP_Text _title;

        public event Action NextClicked;

        public event Action RetryClicked;

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ShowLose(LoseReason reason)
        {
            var (title, subtitle) = reason switch
            {
                LoseReason.Overcharged => ("Overcharged!", "The ball poured itself into the shot."),
                LoseReason.OutOfShots => ("Out of shots", "Not enough size left to clear the path."),
                _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
            };

            Show(title, subtitle, isWin: false);
        }

        public void ShowWin(int levelNumber, float sizeLeft)
        {
            Show($"Level {levelNumber} complete!", $"Size left: {sizeLeft:P0}", isWin: true);
        }

        private void Awake()
        {
            _nextButton.onClick.AddListener(OnNextClicked);
            _retryButton.onClick.AddListener(OnRetryClicked);
        }

        private void OnDestroy()
        {
            _nextButton.onClick.RemoveListener(OnNextClicked);
            _retryButton.onClick.RemoveListener(OnRetryClicked);
        }

        // The scene reloads a frame later, so a double tap must not fire twice (it would skip a level).
        private void OnNextClicked()
        {
            SetButtonsInteractable(false);
            NextClicked?.Invoke();
        }

        private void OnRetryClicked()
        {
            SetButtonsInteractable(false);
            RetryClicked?.Invoke();
        }

        private void SetButtonsInteractable(bool isInteractable)
        {
            _nextButton.interactable = isInteractable;
            _retryButton.interactable = isInteractable;
        }

        private void Show(string title, string subtitle, bool isWin)
        {
            _title.text = title;
            _subtitle.text = subtitle;
            _nextButton.gameObject.SetActive(isWin);
            _retryButton.gameObject.SetActive(!isWin);
            SetButtonsInteractable(true);
            gameObject.SetActive(true);
            PlayAppear(isWin ? _nextButton.transform : _retryButton.transform);
        }

        private void PlayAppear(Transform button)
        {
            _group.alpha = 0f;
            _title.transform.localScale = Vector3.one * 0.6f;
            button.localScale = Vector3.zero;

            DOTween.Sequence()
                .Append(DOTween.To(() => _group.alpha, alpha => _group.alpha = alpha, 1f, FadeDuration))
                .Join(_title.transform.DOScale(1f, PopDuration).SetEase(Ease.OutBack))
                .Insert(ButtonDelay, button.DOScale(1f, PopDuration).SetEase(Ease.OutBack))
                .SetLink(gameObject);
        }
    }
}
