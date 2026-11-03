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
        [Tooltip("Smallest meaningful shot: when the player can't afford it, the game is lost.")]
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
        [Tooltip("Share of the current player volume moved into the shot per second, so charging feels the same at any size.")]
        private float _chargeRate = 0.06f;

        [SerializeField]
        [Tooltip("Charge rate multiplier by hold time in seconds.")]
        private AnimationCurve _chargeCurve = new(
            new Keyframe(0f, 1f, 0f, 0.67f),
            new Keyframe(1.5f, 2f, 0.67f, 4f),
            new Keyframe(3f, 8f, 4f, 12f),
            new Keyframe(4f, 20f, 12f, 12f));

        [SerializeField]
        [Min(0f)]
        [Tooltip("Shorter presses are cancelled and their volume is returned to the player.")]
        private float _minChargeTime = 0.08f;

        [Header("Infection")]
        [SerializeField]
        [Min(0f)]
        [Tooltip("Initial infection energy = factor * r^1.5. Every infected obstacle costs energy.")]
        private float _infectionEnergyFactor = 7f;

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
        private int _solverShotCandidates = 3;

        [SerializeField]
        [Min(1)]
        private int _maxRegenerateAttempts = 10;

        [SerializeField]
        [Min(0.01f)]
        [Tooltip("Solver shots stay within [min, max] radius: no pixel-perfect micro shots, no dumping the whole ball into one shot.")]
        private float _solverMinShotRadius = 0.3f;

        [SerializeField]
        [Min(0.01f)]
        private float _solverMaxShotRadius = 0.6f;


        public float ChainGapCostFactor => _chainGapCostFactor;

        public float ChainHopCost => _chainHopCost;

        public AnimationCurve ChargeCurve => _chargeCurve;

        public float ChargeRate => _chargeRate;

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

        public float MinChargeTime => _minChargeTime;

        public float MinShotRadius => _minShotRadius;

        public float MinStartRadius => _minStartRadius;

        public float MoveSafetyGap => _moveSafetyGap;

        public float ProjectileSpeed => _projectileSpeed;

        public float SizeReserve => _sizeReserve;

        public float SolverMaxShotRadius => _solverMaxShotRadius;

        public float SolverMinShotRadius => _solverMinShotRadius;

        public int SolverShotCandidates => _solverShotCandidates;

        private void OnValidate()
        {
            _minStartRadius = Mathf.Max(_minStartRadius, _criticalRadius);
            _maxStartRadius = Mathf.Max(_maxStartRadius, _minStartRadius);
            _solverMaxShotRadius = Mathf.Max(_solverMaxShotRadius, _solverMinShotRadius);
        }
    }
}