namespace BallGame.Model
{
    public readonly struct InfectionHit
    {
        public InfectionHit(int obstacleIndex, float spentEnergy)
        {
            ObstacleIndex = obstacleIndex;
            SpentEnergy = spentEnergy;
        }

        public int ObstacleIndex { get; }

        // Energy spent along the chain to reach this obstacle; drives the visual wave delay.
        public float SpentEnergy { get; }
    }
}
