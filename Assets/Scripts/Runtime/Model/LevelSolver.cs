using System.Collections.Generic;
using BallGame.Configs;
using UnityEngine;

namespace BallGame.Model
{
    // Smallest start volume whose play reaches the door with SizeReserve of it left. A bigger ball has a wider lane,
    // so success is not monotonic in size: scan the size upwards, then refine by bisection.
    public sealed class LevelSolver
    {
        private const int BisectionIterations = 6;

        private const int MaxSteps = 100;

        private const float MinProgress = 0.01f;

        private const float ReachEpsilon = 0.001f;

        private const float StartRadiusStep = 0.1f;

        private readonly GameBalanceConfig _config;

        private readonly List<int> _infected = new();

        private readonly GameRules _rules;

        private LevelModel _best;

        private LevelModel _simulation;

        private LevelModel _trial;

        public LevelSolver(GameBalanceConfig config, GameRules rules)
        {
            _config = config;
            _rules = rules;
        }

        public bool TrySolve(LevelModel level, out float startVolume)
        {
            _simulation = level.Clone();
            _trial = level.Clone();
            _best = level.Clone();

            var failed = 0f;

            for (var radius = _config.MinStartRadius; radius <= _config.MaxStartRadius; radius += StartRadiusStep)
            {
                var volume = VolumeMath.VolumeFromRadius(radius);

                if (HasReserve(level, volume))
                {
                    startVolume = failed > 0f ? Refine(level, failed, volume) : volume;
                    return true;
                }

                failed = volume;
            }

            startVolume = 0f;
            return false;
        }

        private bool HasReserve(LevelModel level, float startVolume)
        {
            return TryPlay(level, startVolume, out var finalVolume) && finalVolume >= startVolume * _config.SizeReserve;
        }

        private float Refine(LevelModel level, float failed, float passed)
        {
            for (var i = 0; i < BisectionIterations; i++)
            {
                var middle = (failed + passed) * 0.5f;

                if (HasReserve(level, middle))
                {
                    passed = middle;
                }
                else
                {
                    failed = middle;
                }
            }

            return passed;
        }

        // Greedy: best progress per spent volume, or the most obstacles cleared while nothing opens the path yet.
        private bool TryPickShot(Vector2 position, float volume, out float bestShotVolume)
        {
            bestShotVolume = 0f;

            if (!_rules.CanAffordShot(volume))
            {
                return false;
            }

            var maxShot = Mathf.Min(VolumeMath.RadiusFromVolume(volume - VolumeMath.VolumeFromRadius(_config.CriticalRadius)), _config.SolverMaxShotRadius);
            var minShot = Mathf.Min(_config.MinShotRadius, maxShot);
            var bestProgressScore = 0f;
            var bestClearScore = 0f;
            var hasProgress = false;

            for (var i = 0; i < _config.SolverShotCandidates; i++)
            {
                var shotRadius = Mathf.Lerp(minShot, maxShot, i / (float)(_config.SolverShotCandidates - 1));
                var shotVolume = VolumeMath.VolumeFromRadius(shotRadius);
                var radiusAfterShot = VolumeMath.RadiusFromVolume(volume - shotVolume);

                _trial.CopyStateFrom(_simulation);

                if (!_rules.TryResolveShot(_trial, position, radiusAfterShot, shotRadius, _infected, out _))
                {
                    continue;
                }

                foreach (var index in _infected)
                {
                    _trial.Kill(index);
                }

                var progress = _rules.FreeDistance(_trial, position, radiusAfterShot);
                var progressScore = progress / shotVolume;
                var clearScore = _infected.Count / shotVolume;

                var isBetter = progress > MinProgress
                    ? !hasProgress || progressScore > bestProgressScore
                    : !hasProgress && clearScore > bestClearScore;

                if (!isBetter)
                {
                    continue;
                }

                hasProgress |= progress > MinProgress;
                bestProgressScore = progressScore;
                bestClearScore = clearScore;
                bestShotVolume = shotVolume;
                _best.CopyStateFrom(_trial);
            }

            return bestShotVolume > 0f;
        }

        private bool TryPlay(LevelModel level, float volume, out float finalVolume)
        {
            _simulation.CopyStateFrom(level);
            finalVolume = volume;

            var position = level.StartPoint;

            for (var step = 0; step < MaxSteps; step++)
            {
                position += level.LaneDirection * _rules.FreeDistance(_simulation, position, VolumeMath.RadiusFromVolume(volume));

                if (Vector2.Distance(position, level.DoorPoint) <= ReachEpsilon)
                {
                    finalVolume = volume;
                    return true;
                }

                if (!TryPickShot(position, volume, out var shotVolume))
                {
                    return false;
                }

                _simulation.CopyStateFrom(_best);
                volume -= shotVolume;
            }

            return false;
        }
    }
}
