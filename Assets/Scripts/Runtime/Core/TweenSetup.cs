using DG.Tweening;
using UnityEngine;

namespace BallGame.Core
{
    public static class TweenSetup
    {
        private const int SequenceCapacity = 300;

        private const int TweenCapacity = 500;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly)
                .SetCapacity(TweenCapacity, SequenceCapacity);
        }
    }
}