using BallGame.Configs;
using BallGame.Model;
using NUnit.Framework;
using UnityEditor;

namespace BallGame.Tests
{
    public sealed class LevelSolverTests
    {
        private const int CheckedLevels = 5;

        private const float RequiredReserve = 0.2f;

        [Test]
        public void ShippedLevels_ReachDoorWithSizeReserve()
        {
            var balance = AssetDatabase.LoadAssetAtPath<GameBalanceConfig>("Assets/Configs/GameBalanceConfig.asset");

            var levels = new[]
            {
                AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Configs/Levels/Level01.asset"),
                AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Configs/Levels/Level02.asset")
            };

            var rules = new GameRules(balance);
            var factory = new LevelFactory(balance, levels, rules);
            var solver = new LevelSolver(balance, rules);

            Assert.That(balance.SizeReserve, Is.GreaterThanOrEqualTo(RequiredReserve));

            for (var index = 0; index < CheckedLevels; index++)
            {
                Assert.That(factory.TryCreate(index, out var setup), Is.True, $"Level {index + 1} is unsolvable");
                Assert.That(solver.TryPlay(setup.Level, setup.StartVolume, out var finalVolume), Is.True, $"Level {index + 1} is not reached");
                Assert.That(finalVolume, Is.GreaterThanOrEqualTo(setup.StartVolume * balance.SizeReserve), $"Level {index + 1} reserve");
            }
        }
    }
}
