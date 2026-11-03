using System;
using System.Collections;
using BallGame.Configs;
using UnityEngine;

namespace BallGame.Player
{
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField]
        private PlayerBall _player;

        private GameBalanceConfig _config;

        public event Action MoveCompleted;

        public void Initialize(GameBalanceConfig config)
        {
            _config = config;
        }

        public void Move(Vector2 target)
        {
            StartCoroutine(MoveRoutine(target));
        }

        // Equal hops along the lane centre, each one a half-sine arc.
        private IEnumerator MoveRoutine(Vector2 target)
        {
            var start = _player.Position;
            var hops = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(start, target) / _config.HopLength));

            for (var hop = 0; hop < hops; hop++)
            {
                var from = Vector2.Lerp(start, target, hop / (float)hops);
                var to = Vector2.Lerp(start, target, (hop + 1) / (float)hops);

                for (var progress = 0f; progress < 1f; progress += Time.deltaTime / _config.HopDuration)
                {
                    _player.SetPosition(Vector2.Lerp(from, to, progress), Mathf.Sin(progress * Mathf.PI) * _config.HopHeight);
                    yield return null;
                }

                _player.SetPosition(to);
            }

            MoveCompleted?.Invoke();
        }
    }
}
