using UnityEngine;

namespace BallGame.UI
{
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField]
        private GameObject _hint;

        [SerializeField]
        private RectTransform _sizeFill;

        private float _criticalRadius;

        private float _startRadius;

        public void HideHint()
        {
            _hint.SetActive(false);
        }

        public void Initialize(float startRadius, float criticalRadius)
        {
            _startRadius = startRadius;
            _criticalRadius = criticalRadius;
            _hint.SetActive(true);
            SetRadius(startRadius);
        }

        // Empty bar means the critical radius: one more charge and the ball is lost.
        public void SetRadius(float radius)
        {
            var fill = Mathf.InverseLerp(_criticalRadius, _startRadius, radius);
            _sizeFill.anchorMax = new Vector2(fill, _sizeFill.anchorMax.y);
        }
    }
}
