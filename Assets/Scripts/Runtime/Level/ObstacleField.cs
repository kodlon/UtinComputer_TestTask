using System.Collections.Generic;
using BallGame.Common;
using BallGame.Configs;
using BallGame.Model;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class ObstacleField : MonoBehaviour
    {
        [SerializeField]
        private ObstacleView _obstaclePrefab;

        private readonly List<ObstacleView> _views = new();

        private LevelModel _level;

        public void Build(LevelModel level, LevelConfig config)
        {
            _level = level;

            for (var i = 0; i < level.ObstacleCount; i++)
            {
                var obstacle = level.GetObstacle(i);
                var height = config.ObstacleHeight * obstacle.Radius / config.ObstacleRadius;

                var view = Instantiate(_obstaclePrefab, transform);
                view.Setup(obstacle.Position, obstacle.Radius, height);
                _views.Add(view);
            }
        }

        private void OnDrawGizmos()
        {
            if (_level == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;

            for (var i = 0; i < _level.ObstacleCount; i++)
            {
                if (_level.IsAlive(i))
                {
                    var obstacle = _level.GetObstacle(i);
                    GizmoDraw.CircleXZ(obstacle.Position, obstacle.Radius);
                }
            }
        }
    }
}
