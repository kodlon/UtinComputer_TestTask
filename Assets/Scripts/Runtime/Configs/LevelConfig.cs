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
        [Tooltip("Ground plane XZ: x = width, y = length.")]
        private Vector2 _fieldSize = new(12f, 32f);

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

        [Header("Obstacles")]
        [SerializeField]
        [Min(0.1f)]
        private float _obstacleSpacing = 1.1f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Max random offset from the grid cell center.")]
        private float _obstacleJitter = 0.15f;

        [SerializeField]
        [Min(0.05f)]
        private float _obstacleRadius = 0.35f;

        [SerializeField]
        [Range(0f, 0.5f)]
        [Tooltip("Random radius variation (0.15 = ±15%).")]
        private float _obstacleRadiusVariation = 0.15f;

        [SerializeField]
        [Min(0.1f)]
        private float _obstacleHeight = 1.2f;

        public float DoorClearRadius => _doorClearRadius;

        public Vector2 DoorPoint => _doorPoint;

        public Vector2 FieldSize => _fieldSize;

        public float LaneLength => Vector2.Distance(_startPoint, _doorPoint);

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