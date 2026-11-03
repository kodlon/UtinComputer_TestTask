using System;
using BallGame.Common;
using BallGame.Configs;
using DG.Tweening;
using UnityEngine;

namespace BallGame.Player
{
    public sealed class PlayerMover : MonoBehaviour
    {
        private const float LandingDuration = 0.12f;

        private const float StretchShare = 0.4f;

        private static readonly Vector3 LandingSquash = new(1.2f, 0.8f, 1.2f);

        private static readonly Vector3 TakeOffStretch = new(0.88f, 1.15f, 0.88f);

        [SerializeField]
        private PlayerBall _player;

        private GameBalanceConfig _config;

        public event Action HopLanded;

        public event Action MoveCompleted;

        public void Initialize(GameBalanceConfig config)
        {
            _config = config;
        }

        // Equal hops along the lane centre with squash & stretch on take-off and landing.
        public void Move(Vector2 target)
        {
            var start = _player.Position;
            var hops = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(start, target) / _config.HopLength));
            var duration = _config.HopDuration;
            var visual = _player.Visual;
            var sequence = DOTween.Sequence().SetLink(gameObject);

            for (var hop = 1; hop <= hops; hop++)
            {
                var landing = Vector2.Lerp(start, target, hop / (float)hops).ToXZ(_player.Radius);

                sequence.Append(_player.transform.DOJump(landing, _config.HopHeight, 1, duration).SetEase(Ease.Linear));
                sequence.Join(visual.DOScale(TakeOffStretch, duration * StretchShare).SetEase(Ease.OutQuad));
                sequence.Insert(sequence.Duration() - duration * (1f - StretchShare), visual.DOScale(Vector3.one, duration * (1f - StretchShare)).SetEase(Ease.InQuad));
                sequence.AppendCallback(OnHopLanded);
                sequence.Append(visual.DOScale(LandingSquash, LandingDuration * 0.4f).SetEase(Ease.OutQuad));
                sequence.Append(visual.DOScale(Vector3.one, LandingDuration * 0.6f).SetEase(Ease.OutBack));
            }

            sequence.OnComplete(OnMoveCompleted);
        }

        private void OnHopLanded()
        {
            HopLanded?.Invoke();
        }

        private void OnMoveCompleted()
        {
            _player.SetPosition(_player.Position);
            MoveCompleted?.Invoke();
        }
    }
}
