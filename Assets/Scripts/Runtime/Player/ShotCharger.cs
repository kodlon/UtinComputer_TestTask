using BallGame.Configs;
using BallGame.Model;
using BallGame.Shooting;

namespace BallGame.Player
{
    public sealed class ShotCharger
    {
        private readonly GameBalanceConfig _config;

        private readonly PlayerBall _player;

        private readonly Projectile _projectile;

        private readonly GameRules _rules;

        private float _holdTime;

        private LevelModel _level;

        private float _volumeBeforeShot;

        public ShotCharger(GameBalanceConfig config, GameRules rules, PlayerBall player, Projectile projectile)
        {
            _config = config;
            _rules = rules;
            _player = player;
            _projectile = projectile;
        }

        public bool IsOvercharged => _player.Radius <= _config.CriticalRadius;

        public bool IsShotReady => ShotRadius >= _config.MinShotRadius;

        public float ShotRadius { get; private set; }

        public void Begin(LevelModel level)
        {
            _level = level;
            _holdTime = 0f;
            _volumeBeforeShot = _player.Volume;
            _projectile.Show(_player.Position, 0f);
            _player.SetCharging(true);
            SetShotRadius(0f);
        }

        public void Cancel()
        {
            _projectile.Hide();
            _player.SetCharging(false);
        }

        public void Refund()
        {
            _player.SetVolume(_volumeBeforeShot);
            Cancel();
        }

        public void Tick(float deltaTime)
        {
            _holdTime += deltaTime;
            SetShotRadius(_config.ShotRadiusByHoldTime.Evaluate(_holdTime));
        }

        public bool TryRelease(out float shotRadius)
        {
            shotRadius = ShotRadius;

            if (!IsShotReady)
            {
                Refund();
                return false;
            }

            _player.SetCharging(false);
            return true;
        }

        private void SetShotRadius(float radius)
        {
            ShotRadius = radius;
            _player.SetVolume(_volumeBeforeShot - VolumeMath.VolumeFromRadius(radius));
            _projectile.Place(_rules.ShotOrigin(_level, _player.Position, _player.Radius, radius), radius);
        }
    }
}
