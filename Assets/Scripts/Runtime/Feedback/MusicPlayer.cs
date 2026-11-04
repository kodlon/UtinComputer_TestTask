using UnityEngine;

namespace BallGame.Feedback
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class MusicPlayer : MonoBehaviour
    {
        private static MusicPlayer _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);

                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            GetComponent<AudioSource>().Play();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}