using System;
using System.Collections.Generic;
using System.Diagnostics;
using BallGame.CameraRig;
using BallGame.Configs;
using BallGame.Input;
using BallGame.Level;
using BallGame.Model;
using BallGame.Player;
using BallGame.Shooting;
using BallGame.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace BallGame.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        private const float MinMoveDistance = 0.05f;

        private const float ReachEpsilon = 0.001f;

        private const int SeedStepPerExtraLevel = 100;

        private const int TargetFrameRate = 60;

        private const float FullShakeInfectedCount = 12f;

        [Header("Configs")]
        [SerializeField]
        private GameBalanceConfig _balanceConfig;

        [SerializeField]
        private LevelConfig[] _levels;

        [Header("Level")]
        [SerializeField]
        private Door _door;

        [SerializeField]
        private LanePath _lanePath;

        [SerializeField]
        private Monument _monument;

        [SerializeField]
        private ObstacleField _obstacleField;

        [Header("Gameplay")]
        [SerializeField]
        private CameraFollow _cameraFollow;

        [SerializeField]
        private CameraShake _cameraShake;

        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private PlayerBall _player;

        [SerializeField]
        private PlayerMover _playerMover;

        [SerializeField]
        private Projectile _projectile;

        [Header("UI")]
        [SerializeField]
        private HudView _hud;

        [SerializeField]
        private ResultView _result;

        private readonly List<int> _infected = new();

        private Vector2 _impactPoint;

        private ShotCharger _charger;

        private LevelModel _level;

        private LevelConfig _levelConfig;

        private int _levelIndex;

        private int _levelSeed;

        private GameRules _rules;

        private float _startVolume;

        private GameState _state = GameState.Boot;

        public event Action<GameState> StateChanged;

        private int LevelNumber => _levelIndex + 1;

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
            _rules = new GameRules(_balanceConfig);
            _charger = new ShotCharger(_balanceConfig, _rules, _player, _projectile);
            _playerMover.Initialize(_balanceConfig);
        }

        private void OnEnable()
        {
            _inputReader.PressCancelled += OnPressCancelled;
            _inputReader.PressStarted += OnPressStarted;
            _inputReader.PressReleased += OnPressReleased;
            _playerMover.MoveCompleted += OnMoveCompleted;
            _projectile.Arrived += OnProjectileArrived;
            _obstacleField.InfectionWaveCompleted += OnInfectionWaveCompleted;
            _result.NextClicked += OnNextClicked;
            _result.RetryClicked += OnRetryClicked;
        }

        private void OnDisable()
        {
            _inputReader.PressCancelled -= OnPressCancelled;
            _inputReader.PressStarted -= OnPressStarted;
            _inputReader.PressReleased -= OnPressReleased;
            _playerMover.MoveCompleted -= OnMoveCompleted;
            _projectile.Arrived -= OnProjectileArrived;
            _obstacleField.InfectionWaveCompleted -= OnInfectionWaveCompleted;
            _result.NextClicked -= OnNextClicked;
            _result.RetryClicked -= OnRetryClicked;
        }

        private void Start()
        {
            _levelIndex = LevelProgress.CurrentIndex;
            _levelConfig = _levels[Mathf.Min(_levelIndex, _levels.Length - 1)];

            if (!TryCreateLevel(out _level, out _startVolume, out _levelSeed))
            {
                enabled = false;
                return;
            }

            _obstacleField.Build(_level, _levelConfig, _levelSeed);
            _lanePath.Setup(_level.DoorPoint);
            _projectile.Hide();
            _player.Initialize(_level.StartPoint, _startVolume);
            _door.Place(_level.DoorPoint, -_level.LaneDirection, _player.transform, _balanceConfig.DoorOpenDistance);
            _monument.Place(_levelConfig.MonumentPoint, -_level.LaneDirection);
            _cameraFollow.SetTarget(_player.transform, _levelConfig.FieldBounds);
            _hud.Initialize(LevelNumber, _player.Radius, _balanceConfig.CriticalRadius);
            _result.Hide();

            StartMove();
        }

        private void Update()
        {
            if (_state != GameState.Charging)
            {
                return;
            }

            _charger.Tick(Time.deltaTime);

            if (_charger.IsOvercharged)
            {
                Lose(LoseReason.Overcharged);
            }
        }

        private void LateUpdate()
        {
            if (_level != null)
            {
                _lanePath.Refresh(_player.Position, _player.Radius * 2f);
                _hud.SetRadius(_player.Radius);
            }
        }

        private void OnPressStarted()
        {
            if (_state != GameState.Ready)
            {
                return;
            }

            _charger.Begin(_level);
            SetState(GameState.Charging);
        }

        private void OnPressReleased()
        {
            if (_state != GameState.Charging)
            {
                return;
            }

            if (!_charger.TryRelease(out var shotRadius))
            {
                EnterReady();
                return;
            }

            _hud.HideHint();
            _rules.TryResolveShot(_level, _player.Position, _player.Radius, shotRadius, _infected, out _impactPoint);
            Log($"[Game] Shot radius {shotRadius:0.00}: {_infected.Count} infected");

            SetState(GameState.ShotInFlight);
            _projectile.Launch(_impactPoint, _balanceConfig.ProjectileSpeed);
        }

        private void OnPressCancelled()
        {
            if (_state != GameState.Charging)
            {
                return;
            }

            _charger.Refund();
            EnterReady();
        }

        private void OnProjectileArrived()
        {
            if (_infected.Count == 0)
            {
                StartMove();
                return;
            }

            foreach (var index in _infected)
            {
                _level.Kill(index);
            }

            SetState(GameState.Resolving);
            _cameraShake.Shake(_infected.Count / FullShakeInfectedCount);
            _obstacleField.PlayInfection(_infected, _impactPoint, _balanceConfig.InfectionWaveSpeed, _balanceConfig.ExplodeDelay);
        }

        private void OnInfectionWaveCompleted()
        {
            StartMove();
        }

        private void StartMove()
        {
            SetState(GameState.Moving);

            var freeDistance = _rules.FreeDistance(_level, _player.Position, _player.Radius);
            var target = _player.Position + _level.LaneDirection * freeDistance;

            if (freeDistance < MinMoveDistance)
            {
                if (Vector2.Distance(target, _level.DoorPoint) <= ReachEpsilon)
                {
                    _player.SetPosition(_level.DoorPoint);
                }

                OnMoveCompleted();
                return;
            }

            _playerMover.Move(target);
        }

        private void OnMoveCompleted()
        {
            if (Vector2.Distance(_player.Position, _level.DoorPoint) <= ReachEpsilon)
            {
                Win();
                return;
            }

            if (!_rules.CanAffordShot(_player.Volume))
            {
                Lose(LoseReason.OutOfShots);
                return;
            }

            EnterReady();
        }

        private void EnterReady()
        {
            SetState(GameState.Ready);

            if (_inputReader.IsPressed)
            {
                OnPressStarted();
            }
        }

        private void Win()
        {
            SetState(GameState.Win);
            _inputReader.SetEnabled(false);
            _door.Celebrate();
            _monument.Celebrate();
            _hud.HideHint();
            _result.ShowWin(LevelNumber, _player.Volume / _startVolume);
            Log($"[Game] Win with {_player.Volume / _startVolume:P0} of the start volume");
        }

        private void Lose(LoseReason reason)
        {
            // State first: disabling input fires PressCancelled, which must not see the Charging state.
            SetState(GameState.Lose);
            _charger.Cancel();
            _inputReader.SetEnabled(false);
            _hud.HideHint();
            _result.ShowLose(reason);
            Log($"[Game] Lose: {reason}");
        }

        private void OnNextClicked()
        {
            LevelProgress.Advance();
            ReloadScene();
        }

        private void OnRetryClicked()
        {
            ReloadScene();
        }

        private static void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        [ContextMenu("Reset Level Progress")]
        private void ResetLevelProgress()
        {
            LevelProgress.Reset();
        }

        private void SetState(GameState state)
        {
            Log($"[Game] {_state} -> {state}");
            _state = state;
            StateChanged?.Invoke(state);
        }

        private bool TryCreateLevel(out LevelModel level, out float startVolume, out int seed)
        {
            var solver = new LevelSolver(_balanceConfig, _rules);
            var extraLevels = Mathf.Max(0, _levelIndex - (_levels.Length - 1));
            var baseSeed = _levelConfig.Seed + extraLevels * SeedStepPerExtraLevel;

            for (var attempt = 0; attempt < _balanceConfig.MaxRegenerateAttempts; attempt++)
            {
                seed = baseSeed + attempt;
                level = LevelGenerator.Generate(_levelConfig, seed);

                if (solver.TrySolve(level, out startVolume))
                {
                    Log($"[Solver] level {LevelNumber}, seed {seed}: start radius {VolumeMath.RadiusFromVolume(startVolume):0.00}");
                    return true;
                }

                Debug.LogWarning($"[Solver] seed {seed} is unsolvable within the size limits, regenerating.");
            }

            Debug.LogError($"[Solver] no solvable level after {_balanceConfig.MaxRegenerateAttempts} attempts. Tune LevelConfig.");
            level = null;
            startVolume = 0f;
            seed = 0;
            return false;
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        private static void Log(string message)
        {
            Debug.Log(message);
        }
    }
}
