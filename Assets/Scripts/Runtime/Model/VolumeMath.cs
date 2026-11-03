using UnityEngine;

namespace BallGame.Model
{
    // V = r³: the 4/3π constant cancels out in every ratio the game uses.
    public static class VolumeMath
    {
        private const float OneThird = 1f / 3f;

        public static float RadiusFromVolume(float volume)
        {
            return Mathf.Pow(Mathf.Max(volume, 0f), OneThird);
        }

        public static float VolumeFromRadius(float radius)
        {
            return radius * radius * radius;
        }
    }
}
