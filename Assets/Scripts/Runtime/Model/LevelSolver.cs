using System;
using System.Collections.Generic;
using BallGame.Configs;
using UnityEngine;

namespace BallGame.Model
{
    // Plays the level with the same GameRules as the player. The start volume is the smallest one whose plan finishes
    // with a real reserve: volume left above the critical size >= SizeReserve * volume the run actually needed.
    // A bigger ball has a wider lane to clear, so the reserve is checked on the finished run, not added on top.
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

            var minVolume = FindSmallestVolume(level, CanWin);
            var startVolume = FindSmallestVolume(level, HasRequiredReserve);

            if (minVolume < 0f || startVolume < 0f)
            {
                return SolveResult.Unsolvable;
            }

            startVolume = Mathf.Max(startVolume, VolumeMath.VolumeFromRadius(_config.MinStartRadius));

            // Not strictly monotonic in volume (a bigger ball has a wider lane), so verify the final value.
            var shots = new List<SolverShot>();

            if (!Simulate(level, startVolume, shots, out var finalVolume) || Reserve(startVolume, finalVolume) < _config.SizeReserve)
            {
                return SolveResult.Unsolvable;
            }

            return new SolveResult(minVolume, startVolume, Reserve(startVolume, finalVolume), shots);
        }

        // Bisection for the smallest start volume in [critical, max start] that satisfies the predicate; -1 if none.
        private float FindSmallestVolume(LevelModel level, Func<LevelModel, float, bool> isEnough)
        {
            var low = VolumeMath.VolumeFromRadius(_config.CriticalRadius);
            var high = VolumeMath.VolumeFromRadius(_config.MaxStartRadius);

            if (!isEnough(level, high))
            {
                return -1f;
            }

            for (var i = 0; i < BisectionIterations; i++)
            {
                var middle = (low + high) * 0.5f;

                if (isEnough(level, middle))
                {
                    high = middle;
                }
                else
                {
                    low = middle;
                }
            }

            return high;
        }

        private bool CanWin(LevelModel level, float volume)
        {
            return Simulate(level, volume, null, out _);
        }

        private bool HasRequiredReserve(LevelModel level, float volume)
        {
            return Simulate(level, volume, null, out var finalVolume) && Reserve(volume, finalVolume) >= _config.SizeReserve;
        }

        // Volume left above the critical size relative to the volume the run needed (spent + critical).
        private float Reserve(float startVolume, float finalVolume)
        {
            var criticalVolume = VolumeMath.VolumeFromRadius(_config.CriticalRadius);
            var needed = startVolume - finalVolume + criticalVolume;

            return (finalVolume - criticalVolume) / needed;
        }

        private bool Simulate(LevelModel level, float volume, List<SolverShot> shots, out float finalVolume)
        {
            _simulation.CopyStateFrom(level);
            shots?.Clear();
            finalVolume = volume;

            var direction = level.LaneDirection;
            var position = level.StartPoint;

            for (var step = 0; step < MaxSteps; step++)
            {
                var radius = VolumeMath.RadiusFromVolume(volume);
                position += direction * _rules.FreeDistance(_simulation, position, radius);

                if (Vector2.Distance(position, level.DoorPoint) <= ReachEpsilon)
                {
                    finalVolume = volume;
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

            // Absolute limits keep the spending independent of the start size, so the extra start volume stays as a real reserve.
            var maxShot = Mathf.Min(VolumeMath.RadiusFromVolume(_rules.MaxShotVolume(volume)), _config.SolverMaxShotRadius);
            var minShot = Mathf.Min(Mathf.Max(_config.MinShotRadius, _config.SolverMinShotRadius), maxShot);
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
