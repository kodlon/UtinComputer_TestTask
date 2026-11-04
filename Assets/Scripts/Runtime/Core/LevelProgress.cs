using UnityEngine;

namespace BallGame.Core
{
    public static class LevelProgress
    {
        private const string LevelIndexKey = "BallGame.LevelIndex";

        public static int CurrentIndex => PlayerPrefs.GetInt(LevelIndexKey, 0);

        public static void Advance()
        {
            PlayerPrefs.SetInt(LevelIndexKey, CurrentIndex + 1);
            PlayerPrefs.Save();
        }

        public static void Reset()
        {
            PlayerPrefs.DeleteKey(LevelIndexKey);
            PlayerPrefs.Save();
        }
    }
}