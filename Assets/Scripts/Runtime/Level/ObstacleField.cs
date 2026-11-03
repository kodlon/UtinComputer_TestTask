using System;
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

        [SerializeField]
        private ParticleSystem _explosion;

        [SerializeField]
        [Min(1)]
        private int _explosionParticles = 12;

        private readonly List<ObstacleView> _views = new();

        private LevelModel _level;

        private int _pendingExplosions;

        public event Action InfectionWaveCompleted;

        public void Build(LevelModel level, LevelConfig config)
        {
            _level = level;

            for (var i = 0; i < level.ObstacleCount; i++)
            {
                var obstacle = level.GetObstacle(i);
                var height = config.ObstacleHeight * obstacle.Radius / config.ObstacleRadius;

                var view = Instantiate(_obstaclePrefab, transform);
                view.Setup(obstacle.Position, obstacle.Radius, height, OnObstacleExploded);
                _views.Add(view);
            }
        }

        // Each obstacle is infected after a delay proportional to the energy spent to reach it, so the wave follows the chain.
        public void PlayInfection(IReadOnlyList<InfectionHit> hits, float waveSpeed, float explodeDelay)
        {
            _pendingExplosions += hits.Count;

            for (var i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];
                _views[hit.ObstacleIndex].Infect(hit.SpentEnergy / waveSpeed, explodeDelay);
            }
        }

        private void OnObstacleExploded(ObstacleView view)
        {
            _explosion.transform.position = view.transform.position;
            _explosion.Emit(_explosionParticles);

            _pendingExplosions--;

            if (_pendingExplosions == 0)
            {
                InfectionWaveCompleted?.Invoke();
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
