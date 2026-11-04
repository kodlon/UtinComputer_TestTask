using UnityEngine;

namespace BallGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeArea : MonoBehaviour
    {
        private Rect _appliedArea;

        private void Awake()
        {
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _appliedArea)
                Apply();
        }

        private void Apply()
        {
            _appliedArea = Screen.safeArea;

            var screenSize = new Vector2(Screen.width, Screen.height);
            var rectTransform = (RectTransform)transform;
            rectTransform.anchorMin = _appliedArea.min / screenSize;
            rectTransform.anchorMax = _appliedArea.max / screenSize;
        }
    }
}