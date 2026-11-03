using BallGame.Common;
using BallGame.Model;
using UnityEngine;

namespace BallGame.Player
{
    public sealed class PlayerBall : MonoBehaviour
    {
        public Vector2 Position => transform.position.ToPlanar();

        public float Radius { get; private set; }

        public float Volume { get; private set; }

        public void Initialize(Vector2 position, float volume)
        {
            transform.position = position.ToXZ();
            SetVolume(volume);
        }

        public void SetPosition(Vector2 position, float hopHeight = 0f)
        {
            transform.position = position.ToXZ(Radius + hopHeight);
        }

        public void SetVolume(float volume)
        {
            Volume = Mathf.Max(0f, volume);
            Radius = VolumeMath.RadiusFromVolume(Volume);

            // Keep the ball resting on the ground while it shrinks.
            transform.position = Position.ToXZ(Radius);
            transform.localScale = Vector3.one * (Radius * 2f);
        }
    }
}
