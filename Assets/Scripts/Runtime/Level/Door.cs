using BallGame.Common;
using UnityEngine;

namespace BallGame.Level
{
    public sealed class Door : MonoBehaviour
    {
        public void Place(Vector2 position, Vector2 facing)
        {
            transform.SetPositionAndRotation(position.ToXZ(), Quaternion.LookRotation(facing.ToXZ()));
        }
    }
}
