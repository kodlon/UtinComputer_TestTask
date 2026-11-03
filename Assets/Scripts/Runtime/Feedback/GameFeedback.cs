using BallGame.Core;
using BallGame.Level;
using BallGame.Player;
using UnityEngine;

namespace BallGame.Feedback
{
    public sealed class GameFeedback : MonoBehaviour
    {
        private const float PopInterval = 0.05f;

        [Header("Listened components")]
        [SerializeField]
        private Door _door;

        [SerializeField]
        private GameManager _gameManager;

        [SerializeField]
        private ObstacleField _obstacleField;

        [SerializeField]
        private PlayerMover _playerMover;

        [Header("Audio sources")]
        [SerializeField]
        private AudioSource _chargeSource;

        [SerializeField]
        private AudioSource[] _popSources;

        [SerializeField]
        private AudioSource _sfxSource;

        [Header("Clips")]
        [SerializeField]
        private AudioClip _chargeTick;

        [SerializeField]
        private AudioClip _doorOpen;

        [SerializeField]
        private AudioClip _land;

        [SerializeField]
        private AudioClip _lose;

        [SerializeField]
        private AudioClip[] _pops;

        [SerializeField]
        private AudioClip _shot;

        [SerializeField]
        private AudioClip _win;

        [Header("Tuning")]
        [SerializeField]
        private Vector2 _chargePitch = new(0.9f, 2f);

        [SerializeField]
        private Vector2 _chargeTickInterval = new(0.2f, 0.06f);

        [SerializeField]
        [Min(0.1f)]
        private float _chargeRampTime = 3f;

        [SerializeField]
        private Vector2 _popPitch = new(0.85f, 1.25f);

        [SerializeField]
        [Range(0f, 1f)]
        private float _landVolume = 0.4f;

        private float _chargeTime;

        private bool _isCharging;

        private float _nextTickTime;

        private float _lastPopTime = float.NegativeInfinity;

        private int _nextPopSource;

        private void OnEnable()
        {
            _gameManager.StateChanged += OnStateChanged;
            _obstacleField.ObstacleExploded += OnObstacleExploded;
            _playerMover.HopLanded += OnHopLanded;
            _door.Opened += OnDoorOpened;
        }

        private void OnDisable()
        {
            _gameManager.StateChanged -= OnStateChanged;
            _obstacleField.ObstacleExploded -= OnObstacleExploded;
            _playerMover.HopLanded -= OnHopLanded;
            _door.Opened -= OnDoorOpened;
        }

        private void Update()
        {
            if (!_isCharging)
            {
                return;
            }

            _chargeTime += Time.deltaTime;

            if (_chargeTick == null || _chargeTime < _nextTickTime)
            {
                return;
            }

            var ramp = Mathf.Clamp01(_chargeTime / _chargeRampTime);
            _chargeSource.pitch = Mathf.Lerp(_chargePitch.x, _chargePitch.y, ramp);
            _chargeSource.PlayOneShot(_chargeTick);
            _nextTickTime = _chargeTime + Mathf.Lerp(_chargeTickInterval.x, _chargeTickInterval.y, ramp);
        }

        private void OnStateChanged(GameState state)
        {
            SetCharging(state == GameState.Charging);

            switch (state)
            {
                case GameState.ShotInFlight:
                    Play(_shot);
                    Haptics.Light();
                    break;

                case GameState.Resolving:
                    Haptics.Medium();
                    break;

                case GameState.Win:
                    Play(_win);
                    Haptics.Heavy();
                    break;

                case GameState.Lose:
                    Play(_lose);
                    Haptics.Heavy();
                    break;
            }
        }

        private void OnDoorOpened()
        {
            Play(_doorOpen);
        }

        private void OnHopLanded()
        {
            Play(_land, _landVolume);
        }

        private void OnObstacleExploded()
        {
            if (_pops.Length == 0 || _popSources.Length == 0 || Time.time - _lastPopTime < PopInterval)
            {
                return;
            }

            _lastPopTime = Time.time;

            var source = _popSources[_nextPopSource];
            _nextPopSource = (_nextPopSource + 1) % _popSources.Length;

            source.pitch = Random.Range(_popPitch.x, _popPitch.y);
            source.PlayOneShot(_pops[Random.Range(0, _pops.Length)]);
        }

        private void Play(AudioClip clip, float volume = 1f)
        {
            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip, volume);
            }
        }

        private void SetCharging(bool isCharging)
        {
            if (_isCharging == isCharging)
            {
                return;
            }

            _isCharging = isCharging;
            _chargeTime = 0f;
            _nextTickTime = 0f;

            if (!isCharging)
            {
                _chargeSource.Stop();
            }
        }
    }
}
