using UnityEngine;

namespace BallGame.Configs
{
    [CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "BallGame/Game Balance Config")]
    public sealed class GameBalanceConfig : ScriptableObject
    {
        [Header("Size")]
        [SerializeField]
        [Min(0.01f)]
        private float _criticalRadius = 0.25f;

        [SerializeField]
        [Min(0.01f)]
        private float _minStartRadius = 0.5f;

        [SerializeField]
        [Min(0.01f)]
        private float _maxStartRadius = 1.4f;

        [SerializeField]
        [Range(0f, 0.9f)]
        [Tooltip("Share of the start volume left at the door when the level is played the way the solver plays it.")]
        private float _sizeReserve = 0.2f;

        [Header("Shot")]
        [SerializeField]
        private AnimationCurve _shotRadiusByHoldTime = new(
            new Keyframe(0f, 0f, 0.8f, 0.8f),
            new Keyframe(0.5f, 0.35f, 0.5f, 0.5f),
            new Keyframe(1.5f, 0.8f, 0.6f, 0.6f),
            new Keyframe(2.5f, 1.5f, 0.8f, 0.8f));

        [SerializeField]
        [Min(0.01f)]
        private float _minShotRadius = 0.35f;

        [SerializeField]
        [Min(0.1f)]
        private float _infectionRadiusFactor = 8f;

        [SerializeField]
        [Min(0.1f)]
        private float _projectileSpeed = 14f;

        [SerializeField]
        [Min(0.1f)]
        private float _infectionWaveSpeed = 6f;

        [SerializeField]
        [Min(0f)]
        private float _explodeDelay = 0.35f;

        [Header("Movement")]
        [SerializeField]
        [Min(0.1f)]
        private float _hopLength = 1.2f;

        [SerializeField]
        [Min(0f)]
        private float _hopHeight = 0.4f;

        [SerializeField]
        [Min(0.05f)]
        private float _hopDuration = 0.25f;

        [SerializeField]
        [Min(0f)]
        private float _moveSafetyGap = 1.5f;

        [SerializeField]
        [Range(0.5f, 1f)]
        [Tooltip("Ball collider relative to its visible size: obstacles that only graze the lane don't stop it.")]
        private float _ballColliderScale = 0.95f;

        [SerializeField]
        [Min(0f)]
        private float _doorOpenDistance = 5f;

        [Header("Solver")]
        [SerializeField]
        [Min(2)]
        private int _solverShotCandidates = 6;

        [SerializeField]
        [Min(0.01f)]
        private float _solverMaxShotRadius = 0.8f;

        [SerializeField]
        [Min(1)]
        private int _maxRegenerateAttempts = 10;

        public float BallColliderScale => _ballColliderScale;

        public float CriticalRadius => _criticalRadius;

        public float DoorOpenDistance => _doorOpenDistance;

        public float ExplodeDelay => _explodeDelay;

        public float HopDuration => _hopDuration;

        public float HopHeight => _hopHeight;

        public float HopLength => _hopLength;

        public float InfectionRadiusFactor => _infectionRadiusFactor;

        public float InfectionWaveSpeed => _infectionWaveSpeed;

        public int MaxRegenerateAttempts => _maxRegenerateAttempts;

        public float MaxStartRadius => _maxStartRadius;

        public float MinShotRadius => _minShotRadius;

        public float MinStartRadius => _minStartRadius;

        public float MoveSafetyGap => _moveSafetyGap;

        public float ProjectileSpeed => _projectileSpeed;

        public AnimationCurve ShotRadiusByHoldTime => _shotRadiusByHoldTime;

        public float SizeReserve => _sizeReserve;

        public float SolverMaxShotRadius => _solverMaxShotRadius;

        public int SolverShotCandidates => _solverShotCandidates;

        private void OnValidate()
        {
            _minStartRadius = Mathf.Max(_minStartRadius, _criticalRadius);
            _maxStartRadius = Mathf.Max(_maxStartRadius, _minStartRadius);
        }
    }
}
