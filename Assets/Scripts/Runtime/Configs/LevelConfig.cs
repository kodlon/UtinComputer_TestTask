using UnityEngine;

namespace BallGame.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "BallGame/Level Config")]
    public sealed class LevelConfig : ScriptableObject
    {
        [Header("Generation")]
        [SerializeField]
        private int _seed = 12345;

        [Header("Layout")]
        [SerializeField]
        private Vector2 _startPoint = new(-3.5f, 0f);

        [SerializeField]
        private Vector2 _doorPoint = new(3.5f, 28f);

        [SerializeField]
        [Min(0f)]
        private float _startClearRadius = 2.5f;

        [SerializeField]
        [Min(0f)]
        private float _doorClearRadius = 3f;

        [Header("Monument")]
        [SerializeField]
        [Min(0f)]
        private float _monumentDistance = 3.4f;

        [SerializeField]
        [Min(0f)]
        private float _monumentClearRadius = 2.2f;

        [Header("Field")]
        [SerializeField]
        [Min(0f)]
        private float _sidePadding = 7.5f;

        [SerializeField]
        [Min(0f)]
        private float _backPadding = 6f;

        [SerializeField]
        [Min(0f)]
        private float _frontPadding = 14f;

        [Header("Obstacles")]
        [SerializeField]
        [Min(0.1f)]
        private float _obstacleSpacing = 1.1f;

        [SerializeField]
        [Min(0f)]
        private float _obstacleJitter = 0.15f;

        [SerializeField]
        [Min(0.05f)]
        private float _obstacleRadius = 0.35f;

        [SerializeField]
        [Range(0f, 0.5f)]
        private float _obstacleRadiusVariation = 0.15f;

        [SerializeField]
        [Min(0.1f)]
        private float _obstacleHeight = 1.2f;

        public float DoorClearRadius => _doorClearRadius;

        public Vector2 DoorPoint => _doorPoint;

        public Rect FieldBounds
        {
            get
            {
                var min = Vector2.Min(_startPoint, _doorPoint) - new Vector2(_sidePadding, _backPadding);
                var max = Vector2.Max(_startPoint, _doorPoint) + new Vector2(_sidePadding, _frontPadding);
                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }

        public float MonumentClearRadius => _monumentClearRadius;

        public Vector2 MonumentPoint => _doorPoint + (_doorPoint - _startPoint).normalized * _monumentDistance;

        public float ObstacleHeight => _obstacleHeight;

        public float ObstacleJitter => _obstacleJitter;

        public float ObstacleRadius => _obstacleRadius;

        public float ObstacleRadiusVariation => _obstacleRadiusVariation;

        public float ObstacleSpacing => _obstacleSpacing;

        public int Seed => _seed;

        public float StartClearRadius => _startClearRadius;

        public Vector2 StartPoint => _startPoint;
    }
}
