using System;
using System.Collections.Generic;

namespace BallGame.Model
{
    public sealed class SolveResult
    {
        public static readonly SolveResult Unsolvable = new(false, 0f, 0f, Array.Empty<SolverShot>());

        public SolveResult(bool isSolvable, float minVolume, float startVolume, IReadOnlyList<SolverShot> shots)
        {
            IsSolvable = isSolvable;
            MinVolume = minVolume;
            StartVolume = startVolume;
            Shots = shots;
        }

        public bool IsSolvable { get; }

        public float MinVolume { get; }

        public float Reserve => MinVolume > 0f ? StartVolume / MinVolume - 1f : 0f;

        public IReadOnlyList<SolverShot> Shots { get; }

        public float StartRadius => VolumeMath.RadiusFromVolume(StartVolume);

        public float StartVolume { get; }
    }
}
