using BallGame.Configs;

namespace BallGame.Model
{
    public sealed class LevelSetup
    {
        public LevelConfig Config { get; }

        public LevelModel Level { get; }

        public int Seed { get; }

        public float StartVolume { get; }

        public LevelSetup(LevelConfig config, LevelModel level, int seed, float startVolume)
        {
            Config = config;
            Level = level;
            Seed = seed;
            StartVolume = startVolume;
        }
    }
}