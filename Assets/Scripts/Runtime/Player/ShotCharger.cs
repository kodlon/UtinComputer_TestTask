using BallGame.Configs;
using BallGame.Model;
using BallGame.Shooting;
using UnityEngine;

namespace BallGame.Player
{
    // Moves volume from the player into the growing shot while the press is held.
    public sealed class ShotCharger
    {
        private readonly GameBalanceConfig _config;

        private readonly PlayerBall _player;

        private readonly Projectile _projectile;

        private readonly GameRules _rules;

        private Vector2 _direction;

        private float _holdTime;

        private float _shotVolume;

        public ShotCharger(GameBalanceConfig config, GameRules rules, PlayerBall player, Projectile projectile)
        {
            _config = config;
            _rules = rules;
            _player = player;
            _projectile = projectile;
        }

        public bool IsOvercharged => _player.Radius <= _config.CriticalRadius;

        public float ShotRadius => VolumeMath.RadiusFromVolume(_shotVolume);

        public void Begin(Vector2 direction)
        {
            _direction = direction;
            _holdTime = 0f;
            _shotVolume = 0f;
            _projectile.Show(ShotPosition(), 0f);
            _player.SetCharging(true);
        }

        public void Cancel()
        {
            _shotVolume = 0f;
            _projectile.Hide();
            _player.SetCharging(false);
        }

        public void Tick(float deltaTime)
        {
            _holdTime += deltaTime;

            var transfer = _rules.ChargeRate(_holdTime) * deltaTime * _player.Volume;
            _player.SetVolume(_player.Volume - transfer);
            _shotVolume += transfer;

            _projectile.Place(ShotPosition(), ShotRadius);
        }

        // An accidental short tap is cancelled and the volume goes back to the player.
        public bool TryRelease(out float shotRadius)
        {
            shotRadius = ShotRadius;

            if (_holdTime >= _config.MinChargeTime)
            {
                _shotVolume = 0f;
                _player.SetCharging(false);
                return true;
            }

            _player.SetVolume(_player.Volume + _shotVolume);
            Cancel();

            return false;
        }

        private Vector2 ShotPosition()
        {
            return _rules.ShotOrigin(_player.Position, _direction, _player.Radius, ShotRadius);
        }
    }
}
