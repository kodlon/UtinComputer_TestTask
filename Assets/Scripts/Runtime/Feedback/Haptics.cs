#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;
#endif

namespace BallGame.Feedback
{
    // Referencing Handheld.Vibrate also makes Unity add the VIBRATE permission to the manifest.
    public static class Haptics
    {
        public static bool IsEnabled { get; set; } = true;

        public static void Heavy()
        {
            Vibrate(60, 255);
        }

        public static void Light()
        {
            Vibrate(12, 60);
        }

        public static void Medium()
        {
            Vibrate(30, 150);
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private const int VibrationEffectApiLevel = 26;

        private static int _apiLevel;

        private static AndroidJavaClass _effectClass;

        private static AndroidJavaObject _vibrator;

        private static void Vibrate(long milliseconds, int amplitude)
        {
            if (!IsEnabled)
            {
                return;
            }

            try
            {
                if (_vibrator == null)
                {
                    using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    using var version = new AndroidJavaClass("android.os.Build$VERSION");
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                    _apiLevel = version.GetStatic<int>("SDK_INT");
                    _effectClass = _apiLevel >= VibrationEffectApiLevel ? new AndroidJavaClass("android.os.VibrationEffect") : null;
                }

                if (_effectClass == null)
                {
                    Handheld.Vibrate();
                    return;
                }

                using var effect = _effectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, amplitude);
                _vibrator.Call("vibrate", effect);
            }
            catch (Exception exception)
            {
                IsEnabled = false;
                Debug.LogWarning($"[Haptics] disabled: {exception.Message}");
            }
        }
#else
        private static void Vibrate(long milliseconds, int amplitude) { }
#endif
    }
}