using System;
using System.Collections.Generic;

namespace BallGame.Model
{
    public sealed class SolveResult
    {
        public static readonly SolveResult Unsolvable = new();

        public SolveResult(float minVolume, float startVolume, float reserve, IReadOnlyList<SolverShot> shots)
        {
            IsSolvable = true;
            MinVolume = minVolume;
            StartVolume = startVolume;
            Reserve = reserve;
            Shots = shots;
        }

        private SolveResult()
        {
            Shots = Array.Empty<SolverShot>();
        }

        public bool IsSolvable { get; }

        public float MinVolume { get; }

        // Volume left above the critical size at the door, relative to the volume the planned run needed.
        public float Reserve { get; }

        public IReadOnlyList<SolverShot> Shots { get; }

        public float StartRadius => VolumeMath.RadiusFromVolume(StartVolume);

        public float StartVolume { get; }
    }
}
