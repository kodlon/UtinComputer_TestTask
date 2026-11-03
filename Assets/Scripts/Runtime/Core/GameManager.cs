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

        // Until the solver lands (stage 6) the lane is previewed with the max start radius.
        private float PlayerRadius => _balanceConfig.MaxStartRadius;

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
            _rules = new GameRules(_balanceConfig);
        }

        private void Start()
        {
            _level = LevelGenerator.Generate(_levelConfig, _levelConfig.Seed);

            _obstacleField.Build(_level, _levelConfig);
            _door.Place(_level.DoorPoint, -_level.LaneDirection);
            _lanePath.Setup(_level.DoorPoint);
            _lanePath.Refresh(_level.StartPoint, PlayerRadius * 2f);
        }

        private void OnDrawGizmos()
        {
            if (_level == null)
            {
                return;
            }

            var halfWidth = _rules.CorridorHalfWidth(PlayerRadius);
            var freeDistance = _rules.FreeDistance(_level, _level.StartPoint, PlayerRadius);
            var stopPoint = _level.StartPoint + _level.LaneDirection * freeDistance;

            Gizmos.color = Color.yellow;
            GizmoDraw.CorridorXZ(_level.StartPoint, _level.DoorPoint, halfWidth);

            Gizmos.color = Color.green;
            GizmoDraw.CircleXZ(stopPoint, PlayerRadius);

            var hasHit = _rules.TryFindFirstHit(
                _level,
                _level.StartPoint,
                _level.LaneDirection,
                halfWidth,
                _level.LaneLength,
                out var hitIndex,
                out _);

            if (hasHit)
            {
                var obstacle = _level.GetObstacle(hitIndex);
                Gizmos.color = Color.red;
                GizmoDraw.CircleXZ(obstacle.Position, obstacle.Radius + 0.05f);
            }
        }
    }
}
