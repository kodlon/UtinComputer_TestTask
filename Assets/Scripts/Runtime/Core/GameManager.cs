using BallGame.Common;
using BallGame.Configs;
using BallGame.Level;
using BallGame.Model;
using UnityEngine;

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

        private LevelModel _level;

        private GameRules _rules;

        private SolveResult _solveResult;

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
            _rules = new GameRules(_balanceConfig);
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
            _lanePath.Refresh(_level.StartPoint, _solveResult.StartRadius * 2f);
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
                    Debug.Log($"[Solver] seed {seed}: {result.Shots.Count} shots, " +
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
