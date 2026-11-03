using System;
using BallGame.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BallGame.UI
{
    public sealed class ResultView : MonoBehaviour
    {
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

        private void OnNextClicked()
        {
            NextClicked?.Invoke();
        }

        private void OnRetryClicked()
        {
            RetryClicked?.Invoke();
        }

        private void Show(string title, string subtitle, bool isWin)
        {
            _title.text = title;
            _subtitle.text = subtitle;
            _nextButton.gameObject.SetActive(isWin);
            _retryButton.gameObject.SetActive(!isWin);
            gameObject.SetActive(true);
        }
    }
}
