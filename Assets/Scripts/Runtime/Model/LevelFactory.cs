using BallGame.Configs;
using UnityEngine;

namespace BallGame.Model
{
    // Levels past the configured ones reuse the last config with a new seed. An unsolvable seed is replaced by the next one.
    public sealed class LevelFactory
    {
        private const int SeedStepPerExtraLevel = 100;

        private readonly GameBalanceConfig _balanceConfig;

        private readonly LevelConfig[] _levels;

        private readonly LevelSolver _solver;

        public LevelFactory(GameBalanceConfig balanceConfig, LevelConfig[] levels, GameRules rules)
        {
            _balanceConfig = balanceConfig;
            _levels = levels;
            _solver = new LevelSolver(balanceConfig, rules);
        }

        public bool TryCreate(int levelIndex, out LevelSetup setup)
        {
            var config = _levels[Mathf.Min(levelIndex, _levels.Length - 1)];
            var extraLevels = Mathf.Max(0, levelIndex - (_levels.Length - 1));
            var baseSeed = config.Seed + extraLevels * SeedStepPerExtraLevel;

            for (var attempt = 0; attempt < _balanceConfig.MaxRegenerateAttempts; attempt++)
            {
                var seed = baseSeed + attempt;
                var level = LevelGenerator.Generate(config, seed);

                if (_solver.TrySolve(level, out var startVolume))
                {
                    setup = new LevelSetup(config, level, seed, startVolume);
                    return true;
                }

                Debug.LogWarning($"[Solver] seed {seed} is unsolvable within the size limits, regenerating.");
            }

            setup = null;
            return false;
        }
    }
}
