using System.Collections.Generic;
using BallGame.Configs;
using UnityEngine;

namespace BallGame.Model
{
    // Plays the level with the same GameRules as the player and finds the smallest start volume that wins.
    public sealed class LevelSolver
    {
        private const int BisectionIterations = 14;

        private const int MaxSteps = 100;

        private const float MinProgress = 0.01f;

        private const float ReachEpsilon = 0.001f;

        private readonly GameBalanceConfig _config;

        private readonly List<InfectionHit> _infected = new();

        private readonly GameRules _rules;

        private LevelModel _best;

        private LevelModel _simulation;

        private LevelModel _trial;

        public LevelSolver(GameBalanceConfig config, GameRules rules)
        {
            _config = config;
            _rules = rules;
        }

        public SolveResult Solve(LevelModel level)
        {
            _simulation = level.Clone();
            _trial = level.Clone();
            _best = level.Clone();

            var low = VolumeMath.VolumeFromRadius(_config.CriticalRadius);
            var high = VolumeMath.VolumeFromRadius(_config.MaxStartRadius);

            if (!Simulate(level, high, null))
            {
                return SolveResult.Unsolvable;
            }

            for (var i = 0; i < BisectionIterations; i++)
            {
                var middle = (low + high) * 0.5f;

                if (Simulate(level, middle, null))
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            var minVolume = high;
            var startRadius = VolumeMath.RadiusFromVolume(minVolume * (1f + _config.SizeReserve));

            if (startRadius > _config.MaxStartRadius)
            {
                return SolveResult.Unsolvable;
            }

            var startVolume = VolumeMath.VolumeFromRadius(Mathf.Max(startRadius, _config.MinStartRadius));

            // Solvability is not strictly monotonic in volume (a bigger ball has a wider corridor), so verify the final value.
            var shots = new List<SolverShot>();

            return Simulate(level, startVolume, shots)
                ? new SolveResult(true, minVolume, startVolume, shots)
                : SolveResult.Unsolvable;
        }

        private bool Simulate(LevelModel level, float volume, List<SolverShot> shots)
        {
            _simulation.CopyStateFrom(level);
            shots?.Clear();

            var direction = level.LaneDirection;
            var position = level.StartPoint;

            for (var step = 0; step < MaxSteps; step++)
            {
                var radius = VolumeMath.RadiusFromVolume(volume);
                position += direction * _rules.FreeDistance(_simulation, position, radius);

                if (Vector2.Distance(position, level.DoorPoint) <= ReachEpsilon)
                {
                    return true;
                }

                if (!TryPickShot(position, volume, out var shotRadius, out var hitPoint))
                {
                    return false;
                }

                _simulation.CopyStateFrom(_best);
                volume -= VolumeMath.VolumeFromRadius(shotRadius);
                shots?.Add(new SolverShot(position, shotRadius, hitPoint));
            }

            return false;
        }

        // Greedy choice: the shot with the best progress per spent volume. When no shot opens the path yet,
        // fall back to the one that clears the most obstacles per volume, so multi-shot openings are still found.
        private bool TryPickShot(Vector2 position, float volume, out float bestShotRadius, out Vector2 bestHitPoint)
        {
            bestShotRadius = 0f;
            bestHitPoint = default;

            if (!_rules.CanAffordShot(volume))
            {
                return false;
            }

            var playerRadius = VolumeMath.RadiusFromVolume(volume);
            var maxShot = Mathf.Min(VolumeMath.RadiusFromVolume(_rules.MaxShotVolume(volume)), playerRadius * _config.SolverMaxShotRatio);
            var minShot = Mathf.Min(Mathf.Max(_config.MinShotRadius, playerRadius * _config.SolverMinShotRatio), maxShot);
            var candidates = _config.SolverShotCandidates;

            var bestProgressScore = 0f;
            var bestClearScore = 0f;
            var hasProgress = false;

            for (var i = 0; i < candidates; i++)
            {
                var shotRadius = Mathf.Lerp(minShot, maxShot, i / (float)(candidates - 1));
                var shotVolume = VolumeMath.VolumeFromRadius(shotRadius);
                var radiusAfterShot = VolumeMath.RadiusFromVolume(volume - shotVolume);

                _trial.CopyStateFrom(_simulation);

                if (!_rules.TryResolveShot(_trial, position, radiusAfterShot, shotRadius, _infected, out var hitPoint))
                {
                    continue;
                }

                foreach (var hit in _infected)
                {
                    _trial.Kill(hit.ObstacleIndex);
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
                bestShotRadius = shotRadius;
                bestHitPoint = hitPoint;
                _best.CopyStateFrom(_trial);
            }

            return bestShotRadius > 0f;
        }
    }
}
