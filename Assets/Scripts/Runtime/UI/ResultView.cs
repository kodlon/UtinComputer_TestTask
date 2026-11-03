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
        private Button _restartButton;

        [SerializeField]
        private TMP_Text _subtitle;

        [SerializeField]
        private TMP_Text _title;

        public event Action RestartClicked;

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

            Show(title, subtitle);
        }

        public void ShowWin(float sizeLeft)
        {
            Show("Level complete!", $"Size left: {sizeLeft:P0}");
        }

        private void Awake()
        {
            _restartButton.onClick.AddListener(OnRestartClicked);
        }

        private void OnDestroy()
        {
            _restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        private void OnRestartClicked()
        {
            RestartClicked?.Invoke();
        }

        private void Show(string title, string subtitle)
        {
            _title.text = title;
            _subtitle.text = subtitle;
            gameObject.SetActive(true);
        }
    }
}
