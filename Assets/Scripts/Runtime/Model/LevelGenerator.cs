using System.Collections.Generic;
using BallGame.Configs;
using UnityEngine;
using Random = System.Random;

namespace BallGame.Model
{
    public static class LevelGenerator
    {
        public static LevelModel Generate(LevelConfig config, int seed, float neighbourGap)
        {
            var random = new Random(seed);
            var obstacles = new List<Obstacle>();

            var spacing = config.ObstacleSpacing;
            var fieldCenter = (config.StartPoint + config.DoorPoint) * 0.5f;
            var fieldMin = fieldCenter - config.FieldSize * 0.5f;
            var columns = Mathf.FloorToInt(config.FieldSize.x / spacing);
            var rows = Mathf.FloorToInt(config.FieldSize.y / spacing);

            var maxRadius = config.ObstacleRadius * (1f + config.ObstacleRadiusVariation);
            var jitter = Mathf.Min(config.ObstacleJitter, Mathf.Max(0f, spacing * 0.5f - maxRadius));

            for (var row = 0; row < rows; row++)
            {
                // Staggered rows give a denser, less grid-like field.
                var rowOffset = row % 2 == 0 ? 0.25f : 0.75f;

                for (var column = 0; column < columns; column++)
                {
                    var cellCenter = fieldMin + new Vector2((column + rowOffset) * spacing, (row + 0.5f) * spacing);
                    var position = cellCenter + new Vector2(NextSigned(random), NextSigned(random)) * jitter;

                    if (IsWithin(position, config.StartPoint, config.StartClearRadius)
                        || IsWithin(position, config.DoorPoint, config.DoorClearRadius))
                    {
                        continue;
                    }

                    var radius = config.ObstacleRadius * (1f + NextSigned(random) * config.ObstacleRadiusVariation);
                    obstacles.Add(new Obstacle(position, radius));
                }
            }

            return new LevelModel(config.StartPoint, config.DoorPoint, obstacles.ToArray(), neighbourGap);
        }

        private static bool IsWithin(Vector2 point, Vector2 center, float radius)
        {
            return (point - center).sqrMagnitude < radius * radius;
        }

        private static float NextSigned(Random random)
        {
            return (float)(random.NextDouble() * 2.0 - 1.0);
        }
    }
}
