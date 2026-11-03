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
        [Tooltip("Smaller shots are cancelled and their volume is returned to the player.")]
        private float _minShotRadius = 0.08f;

        [SerializeField]
        [Min(0.01f)]
        private float _minStartRadius = 0.6f;

        [SerializeField]
        [Min(0.01f)]
        private float _maxStartRadius = 1.4f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Extra volume on top of the minimal volume the solver needs (0.2 = 20%).")]
        private float _sizeReserve = 0.2f;

        [Header("Charge")]
        [SerializeField]
        [Min(0.001f)]
        private float _baseChargeRate = 0.08f;

        [SerializeField]
        [Tooltip("Charge rate multiplier by hold time in seconds.")]
        private AnimationCurve _chargeCurve = AnimationCurve.EaseInOut(0f, 1f, 1.5f, 3f);

        [Header("Infection")]
        [SerializeField]
        [Min(0f)]
        [Tooltip("Initial infection energy = factor * shot volume (r³). Every infected obstacle costs energy.")]
        private float _infectionEnergyFactor = 60f;

        [SerializeField]
        [Min(0f)]
        private float _chainHopCost = 0.6f;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Energy cost per meter of gap between neighbouring obstacles.")]
        private float _chainGapCostFactor = 1f;

        [SerializeField]
        [Min(0f)]
        private float _maxChainGap = 0.6f;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Energy units per second; defines how fast the infection wave spreads visually.")]
        private float _infectWaveSpeed = 8f;

        [SerializeField]
        [Min(0f)]
        private float _explodeDelay = 0.35f;

        [Header("Projectile")]
        [SerializeField]
        [Min(0.1f)]
        private float _projectileSpeed = 14f;

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
        private float _moveSafetyGap = 0.15f;

        [SerializeField]
        [Min(0f)]
        private float _laneSideMargin = 0.1f;

        [Header("Door")]
        [SerializeField]
        [Min(0f)]
        private float _doorOpenDistance = 5f;

        [Header("Solver")]
        [SerializeField]
        [Min(2)]
        [Tooltip("Number of shot sizes the solver tries at each blocked step.")]
        private int _solverShotCandidates = 8;

        [SerializeField]
        [Min(1)]
        private int _maxRegenerateAttempts = 10;

        [SerializeField]
        [Range(0.1f, 1f)]
        [Tooltip("Solver never charges a shot bigger than this fraction of the player radius, so its strategy stays human-like.")]
        private float _solverMaxShotRatio = 0.5f;

        public float BaseChargeRate => _baseChargeRate;

        public float ChainGapCostFactor => _chainGapCostFactor;

        public float ChainHopCost => _chainHopCost;

        public AnimationCurve ChargeCurve => _chargeCurve;

        public float CriticalRadius => _criticalRadius;

        public float DoorOpenDistance => _doorOpenDistance;

        public float ExplodeDelay => _explodeDelay;

        public float HopDuration => _hopDuration;

        public float HopHeight => _hopHeight;

        public float HopLength => _hopLength;

        public float InfectWaveSpeed => _infectWaveSpeed;

        public float InfectionEnergyFactor => _infectionEnergyFactor;

        public float LaneSideMargin => _laneSideMargin;

        public float MaxChainGap => _maxChainGap;

        public int MaxRegenerateAttempts => _maxRegenerateAttempts;

        public float MaxStartRadius => _maxStartRadius;

        public float MinShotRadius => _minShotRadius;

        public float MinStartRadius => _minStartRadius;

        public float MoveSafetyGap => _moveSafetyGap;

        public float ProjectileSpeed => _projectileSpeed;

        public float SizeReserve => _sizeReserve;

        public float SolverMaxShotRatio => _solverMaxShotRatio;

        public int SolverShotCandidates => _solverShotCandidates;

        private void OnValidate()
        {
            _minStartRadius = Mathf.Max(_minStartRadius, _criticalRadius);
            _maxStartRadius = Mathf.Max(_maxStartRadius, _minStartRadius);
        }
    }
}