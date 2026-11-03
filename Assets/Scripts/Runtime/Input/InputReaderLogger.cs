using UnityEngine;

namespace BallGame.Input
{
    // TEMP: stage 4 verification only, delete before commit.
    public sealed class InputReaderLogger : MonoBehaviour
    {
        [SerializeField]
        private InputReader _inputReader;

        private float _pressTime;

        private void OnEnable()
        {
            _inputReader.PressStarted += OnPressStarted;
            _inputReader.PressReleased += OnPressReleased;
        }

        private void OnDisable()
        {
            _inputReader.PressStarted -= OnPressStarted;
            _inputReader.PressReleased -= OnPressReleased;
        }

        private void OnPressStarted()
        {
            _pressTime = Time.time;
            Debug.Log("[Input] PressStarted");
        }

        private void OnPressReleased()
        {
            Debug.Log($"[Input] PressReleased after {Time.time - _pressTime:0.00}s");
        }
    }
}
