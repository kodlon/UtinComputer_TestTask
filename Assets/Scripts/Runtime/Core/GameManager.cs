using System.Collections.Generic;
using System.Diagnostics;
using BallGame.CameraRig;
using BallGame.Common;
using BallGame.Configs;
using BallGame.Input;
using BallGame.Level;
using BallGame.Model;
using BallGame.Player;
using BallGame.Shooting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BallGame.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        [Header("Configs")]
        [SerializeField]
        private GameBalanceConfig _balanceConfig;

        [SerializeField]
        private LevelConfig _levelConfig;

        [Header("Level")]
        [SerializeField]
        private Door _door;

        [SerializeField]
        private LanePath _lanePath;

        [SerializeField]
        private ObstacleField _obstacleField;

        [Header("Gameplay")]
        [SerializeField]
        private CameraFollow _cameraFollow;

        [SerializeField]
        private InputReader _inputReader;

        [SerializeField]
        private PlayerBall _player;

        [SerializeField]
        private Projectile _projectile;

        private readonly List<InfectionHit> _infected = new();

        private ShotCharger _charger;

        private LevelModel _level;

        private GameRules _rules;

        private SolveResult _solveResult;

        private GameState _state = GameState.Boot;

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
            _rules = new GameRules(_balanceConfig);
            _charger = new ShotCharger(_balanceConfig, _rules, _player, _projectile);
        }

        private void OnEnable()
        {
            _inputReader.PressStarted += OnPressStarted;
            _inputReader.PressReleased += OnPressReleased;
            _player.RadiusChanged += OnPlayerRadiusChanged;
            _projectile.Arrived += OnProjectileArrived;
            _obstacleField.InfectionWaveCompleted += OnInfectionWaveCompleted;
        }

        private void OnDisable()
        {
            _inputReader.PressStarted -= OnPressStarted;
            _inputReader.PressReleased -= OnPressReleased;
            _player.RadiusChanged -= OnPlayerRadiusChanged;
            _projectile.Arrived -= OnProjectileArrived;
            _obstacleField.InfectionWaveCompleted -= OnInfectionWaveCompleted;
        }

        private void Start()
        {
            if (!TryCreateLevel(out _level, out _solveResult))
            {
                enabled = false;
                return;
            }

            _obstacleField.Build(_level, _levelConfig);
            _door.Place(_level.DoorPoint, -_level.LaneDirection);
            _lanePath.Setup(_level.DoorPoint);
            _projectile.Hide();
            _player.Initialize(_level.StartPoint, _solveResult.StartVolume);
            _cameraFollow.SetTarget(_player.transform);

            SetState(GameState.Ready);
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

        private void OnPressStarted()
        {
            if (_state != GameState.Ready)
            {
                return;
            }

            _charger.Begin(_level.LaneDirection);
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
                SetState(GameState.Ready);
                return;
            }

            // The outcome is resolved by the model right away; the projectile only visualises it. A miss flies to the door.
            var hasHit = _rules.TryResolveShot(_level, _player.Position, _player.Radius, shotRadius, _infected, out var hitPoint);
            Log($"[Game] Shot radius {shotRadius:0.00}: {(hasHit ? $"{_infected.Count} infected" : "miss")}");

            SetState(GameState.ShotInFlight);
            _projectile.Launch(hasHit ? hitPoint : _level.DoorPoint, _balanceConfig.ProjectileSpeed);
        }

        private void OnProjectileArrived()
        {
            if (_infected.Count == 0)
            {
                SetState(GameState.Ready);
                return;
            }

            foreach (var hit in _infected)
            {
                _level.Kill(hit.ObstacleIndex);
            }

            SetState(GameState.Resolving);
            _obstacleField.PlayInfection(_infected, _balanceConfig.InfectWaveSpeed, _balanceConfig.ExplodeDelay);
        }

        // Stage 8: back to aiming; moving the player forward arrives in stage 9.
        private void OnInfectionWaveCompleted()
        {
            SetState(GameState.Ready);
        }

        private void OnPlayerRadiusChanged(float radius)
        {
            _lanePath.Refresh(_player.Position, radius * 2f);
        }

        private void Lose(LoseReason reason)
        {
            // State first: disabling input fires PressReleased, which must not see the Charging state.
            SetState(GameState.Lose);
            _charger.Cancel();
            _inputReader.SetEnabled(false);
            Log($"[Game] Lose: {reason}");
        }

        private void SetState(GameState state)
        {
            Log($"[Game] {_state} -> {state}");
            _state = state;
        }

        private bool TryCreateLevel(out LevelModel level, out SolveResult result)
        {
            var solver = new LevelSolver(_balanceConfig, _rules);

            for (var attempt = 0; attempt < _balanceConfig.MaxRegenerateAttempts; attempt++)
            {
                var seed = _levelConfig.Seed + attempt;
                level = LevelGenerator.Generate(_levelConfig, seed, _balanceConfig.MaxChainGap);
                result = solver.Solve(level);

                if (result.IsSolvable)
                {
                    Log($"[Solver] seed {seed}: {result.Shots.Count} shots, " +
                        $"V_min {result.MinVolume:0.000}, V0 {result.StartVolume:0.000}, " +
                        $"r0 {result.StartRadius:0.00}, reserve {result.Reserve:P0}");
                    return true;
                }

                Debug.LogWarning($"[Solver] seed {seed} is unsolvable within the size limits, regenerating.");
            }

            Debug.LogError($"[Solver] no solvable level after {_balanceConfig.MaxRegenerateAttempts} attempts. Tune LevelConfig.");
            level = null;
            result = SolveResult.Unsolvable;
            return false;
        }

        [Conditional("UNITY_EDITOR")]
        [Conditional("DEVELOPMENT_BUILD")]
        private static void Log(string message)
        {
            Debug.Log(message);
        }

        private void OnDrawGizmos()
        {
            if (_level == null || _solveResult == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            GizmoDraw.CorridorXZ(_level.StartPoint, _level.DoorPoint, _rules.CorridorHalfWidth(_solveResult.StartRadius));

            foreach (var shot in _solveResult.Shots)
            {
                Gizmos.color = Color.green;
                GizmoDraw.CircleXZ(shot.PlayerPosition, 0.15f);

                Gizmos.color = Color.magenta;
                GizmoDraw.CircleXZ(shot.HitPoint, shot.ShotRadius);
                Gizmos.DrawLine(shot.PlayerPosition.ToXZ(0.05f), shot.HitPoint.ToXZ(0.05f));
            }
        }
    }
}
