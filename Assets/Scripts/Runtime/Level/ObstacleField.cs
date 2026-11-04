using System;
using System.Collections.Generic;
using BallGame.Common;
using BallGame.Configs;
using BallGame.Model;
using UnityEngine;
using Random = System.Random;

namespace BallGame.Level
{
    public sealed class ObstacleField : MonoBehaviour
    {
        [SerializeField]
        private ObstacleView _obstaclePrefab;

        [SerializeField]
        private Material[] _idleMaterials;

        [SerializeField]
        [Range(0f, 30f)]
        private float _maxTilt = 10f;

        [SerializeField]
        private ParticleSystem _explosion;

        [SerializeField]
        [Min(1)]
        private int _explosionParticles = 12;

        private readonly List<ObstacleView> _views = new();

        private bool[] _isTargeted;

        private bool[] _nextTargeted;

        private int _pendingExplosions;

        public event Action InfectionWaveCompleted;

        public event Action ObstacleExploded;

        public void Build(LevelModel level, LevelConfig config, int seed)
        {
            var random = new Random(seed);

            for (var i = 0; i < level.ObstacleCount; i++)
            {
                var obstacle = level.GetObstacle(i);
                var height = config.ObstacleHeight * obstacle.Radius / config.ObstacleRadius;
                var tilt = Quaternion.Euler(NextSigned(random) * _maxTilt, (float)random.NextDouble() * 360f, NextSigned(random) * _maxTilt);
                var material = _idleMaterials[random.Next(_idleMaterials.Length)];

                var view = Instantiate(_obstaclePrefab, transform);

                view.Setup(obstacle.Position, obstacle.Radius, height, tilt,
                    material, OnObstacleExploded);

                _views.Add(view);
            }

            _isTargeted = new bool[_views.Count];
            _nextTargeted = new bool[_views.Count];
        }

        public void ClearTargets()
        {
            ShowTargets(Array.Empty<int>());
        }

        public void PlayInfection(IReadOnlyList<int> indices, Vector2 impactPoint, float waveSpeed, float explodeDelay)
        {
            _pendingExplosions += indices.Count;

            foreach (var index in indices)
            {
                var view = _views[index];
                var distance = Vector2.Distance(view.transform.position.ToPlanar(), impactPoint);
                view.Infect(distance / waveSpeed, explodeDelay);
            }
        }

        public void ShowTargets(IReadOnlyList<int> indices)
        {
            Array.Clear(_nextTargeted, 0, _nextTargeted.Length);

            foreach (var index in indices)
                _nextTargeted[index] = true;

            for (var i = 0; i < _views.Count; i++)
            {
                if (_isTargeted[i] != _nextTargeted[i])
                {
                    _isTargeted[i] = _nextTargeted[i];
                    _views[i].SetTargeted(_isTargeted[i]);
                }
            }
        }

        private static float NextSigned(Random random) => (float)(random.NextDouble() * 2.0 - 1.0);

        private void OnObstacleExploded(ObstacleView view)
        {
            _explosion.transform.position = view.transform.position;
            _explosion.Emit(_explosionParticles);
            ObstacleExploded?.Invoke();

            _pendingExplosions--;

            if (_pendingExplosions == 0)
                InfectionWaveCompleted?.Invoke();
        }
    }
}