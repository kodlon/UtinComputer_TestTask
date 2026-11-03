using BallGame.Common;
using UnityEngine;

namespace BallGame.Shooting
{
    public sealed class Projectile : MonoBehaviour
    {
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Place(Vector2 position, float radius)
        {
            transform.position = position.ToXZ(radius);
            transform.localScale = Vector3.one * (radius * 2f);
        }

        public void Show(Vector2 position, float radius)
        {
            gameObject.SetActive(true);
            Place(position, radius);
        }
    }
}
