using System.Collections.Generic;
using BallGame.Configs;
using BallGame.Model;
using NUnit.Framework;
using UnityEngine;

namespace BallGame.Tests
{
    public sealed class GameRulesTests
    {
        private const int MaxShots = 100;

        private const float ObstacleRadius = 0.35f;

        private const float PlayerRadius = 1f;

        private static readonly Vector2 DoorPoint = new(0f, 30f);

        private GameBalanceConfig _config;

        private GameRules _rules;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<GameBalanceConfig>();
            _rules = new GameRules(_config);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test]
        public void FreeDistance_StopsBeforeFirstObstacleInLane_AndReachesDoorWhenClear()
        {
            var level = CreateLevel(Obstacle(3f, 5f), Obstacle(0f, 8f));
            var contactDistance = 8f - ObstacleRadius - PlayerRadius * _config.BallColliderScale;

            Assert.That(
                _rules.FreeDistance(level, Vector2.zero, PlayerRadius),
                Is.EqualTo(contactDistance - _config.MoveSafetyGap).Within(0.001f));

            level.Kill(1);

            Assert.That(_rules.FreeDistance(level, Vector2.zero, PlayerRadius), Is.EqualTo(DoorPoint.y).Within(0.001f));
        }

        [Test]
        public void ShotSize_SmallestClearsLoneObstacle_OneShotOverDenseGroupBeatsSmallOnes()
        {
            var lone = CreateLevel(Obstacle(0f, 5f));
            var infected = new List<int>();

            _rules.TryPredictShot(lone, lone.Clone(), Vector2.zero, PlayerRadius, _config.MinShotRadius, infected, out var advance);

            Assert.That(infected, Has.Count.EqualTo(1));
            Assert.That(advance, Is.EqualTo(DoorPoint.y).Within(0.001f));

            var smallShotsVolume = VolumeToClearWithSmallestShots(CreateDenseGroup());
            var oneShotVolume = VolumeToClearWithOneShot(CreateDenseGroup());

            Assert.That(oneShotVolume, Is.LessThan(smallShotsVolume));
        }

        [Test]
        public void TryResolveShot_InfectsAliveObstaclesWithinReachOnly()
        {
            var level = CreateLevel(Obstacle(0f, 5f), Obstacle(1f, 5.5f), Obstacle(0f, 12f), Obstacle(-1f, 5.5f));
            var infected = new List<int>();
            level.Kill(3);

            _rules.TryResolveShot(level, Vector2.zero, PlayerRadius, 0.5f, infected, out _);

            Assert.That(infected, Is.EquivalentTo(new[] { 0, 1 }));
        }

        private static LevelModel CreateDenseGroup()
        {
            var obstacles = new List<Obstacle>();

            for (var y = 4f; y <= 10f; y += 1.1f)
                for (var x = -3.3f; x <= 3.3f; x += 1.1f)
                    obstacles.Add(Obstacle(x, y));

            return CreateLevel(obstacles.ToArray());
        }

        private static LevelModel CreateLevel(params Obstacle[] obstacles) => new(Vector2.zero, DoorPoint, obstacles);

        private static Obstacle Obstacle(float x, float y) => new(new Vector2(x, y), ObstacleRadius);

        private float VolumeToClearWithOneShot(LevelModel level)
        {
            var position = level.LaneDirection * _rules.FreeDistance(level, Vector2.zero, PlayerRadius);
            var trial = level.Clone();
            var infected = new List<int>();

            for (var shotRadius = _config.MinShotRadius; shotRadius < 2f; shotRadius += 0.01f)
            {
                _rules.TryPredictShot(level, trial, position, PlayerRadius, shotRadius, infected, out var advance);

                if (Vector2.Distance(position + level.LaneDirection * advance, DoorPoint) < 0.001f)
                    return VolumeMath.VolumeFromRadius(shotRadius);
            }

            return float.PositiveInfinity;
        }

        private float VolumeToClearWithSmallestShots(LevelModel level)
        {
            var position = level.LaneDirection * _rules.FreeDistance(level, Vector2.zero, PlayerRadius);
            var trial = level.Clone();
            var infected = new List<int>();
            var volume = 0f;

            for (var shot = 0; shot < MaxShots && Vector2.Distance(position, DoorPoint) > 0.001f; shot++)
            {
                _rules.TryPredictShot(level, trial, position, PlayerRadius, _config.MinShotRadius, infected, out var advance);
                level.CopyStateFrom(trial);
                position += level.LaneDirection * advance;
                volume += VolumeMath.VolumeFromRadius(_config.MinShotRadius);
            }

            return volume;
        }
    }
}
