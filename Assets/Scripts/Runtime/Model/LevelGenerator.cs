using System.Collections.Generic;
using BallGame.Configs;
using UnityEngine;
using Random = System.Random;

namespace BallGame.Model
{
    public static class LevelGenerator
    {
        public static LevelModel Generate(LevelConfig config, int seed)
        {
            var random = new Random(seed);
            var obstacles = new List<Obstacle>();
            var glades = PlanGlades(config, random, obstacles);

            var spacing = config.ObstacleSpacing;
            var field = config.FieldBounds;
            var fieldMin = field.min;
            var columns = Mathf.FloorToInt(field.width / spacing);
            var rows = Mathf.FloorToInt(field.height / spacing);

            var maxRadius = config.ObstacleRadius * (1f + config.ObstacleRadiusVariation);
            var jitter = Mathf.Min(config.ObstacleJitter, Mathf.Max(0f, spacing * 0.5f - maxRadius));

            for (var row = 0; row < rows; row++)
            {
                var rowOffset = row % 2 == 0 ? 0.25f : 0.75f;

                for (var column = 0; column < columns; column++)
                {
                    var cellCenter = fieldMin + new Vector2((column + rowOffset) * spacing, (row + 0.5f) * spacing);
                    var position = cellCenter + new Vector2(NextSigned(random), NextSigned(random)) * jitter;

                    if (IsWithin(position, config.StartPoint, config.StartClearRadius)
                        || IsWithin(position, config.DoorPoint, config.DoorClearRadius)
                        || IsWithin(position, config.MonumentPoint, config.MonumentClearRadius)
                        || IsInGlade(position, config, glades))
                    {
                        continue;
                    }

                    obstacles.Add(new Obstacle(position, NextRadius(config, random)));
                }
            }

            return new LevelModel(config.StartPoint, config.DoorPoint, obstacles.ToArray());
        }

        private static bool IsInGlade(Vector2 position, LevelConfig config, List<Glade> glades)
        {
            var along = (config.DoorPoint - config.StartPoint).normalized;
            var offset = position - config.StartPoint;
            var distance = Vector2.Dot(offset, along);
            var side = Vector2.Dot(offset, Perpendicular(along)) / config.GladeHalfWidth;

            foreach (var glade in glades)
            {
                var length = (distance - glade.Center) / glade.HalfLength;

                if (length * length + side * side < 1f)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsWithin(Vector2 point, Vector2 center, float radius)
        {
            return (point - center).sqrMagnitude < radius * radius;
        }

        private static float NextRadius(LevelConfig config, Random random)
        {
            return config.ObstacleRadius * (1f + NextSigned(random) * config.ObstacleRadiusVariation);
        }

        private static float NextSigned(Random random)
        {
            return (float)(random.NextDouble() * 2.0 - 1.0);
        }

        private static float NextInRange(Random random, Vector2 range)
        {
            return Mathf.Lerp(range.x, range.y, (float)random.NextDouble());
        }

        private static Vector2 Perpendicular(Vector2 direction)
        {
            return new Vector2(-direction.y, direction.x);
        }

        // Dense groups alternate with glades along the lane; each glade holds lone obstacles on the lane.
        private static List<Glade> PlanGlades(LevelConfig config, Random random, List<Obstacle> obstacles)
        {
            var glades = new List<Glade>();
            var lane = config.DoorPoint - config.StartPoint;
            var laneLength = lane.magnitude;
            var along = lane / laneLength;
            var across = Perpendicular(along);
            var distance = config.StartClearRadius + NextInRange(random, config.ClusterLength);

            while (true)
            {
                var gladeLength = NextInRange(random, config.GladeLength);

                if (distance + gladeLength > laneLength)
                {
                    return glades;
                }

                glades.Add(new Glade(distance + gladeLength * 0.5f, gladeLength * 0.5f));

                var loneCount = Mathf.Max(1, Mathf.FloorToInt(gladeLength / config.LoneSpacing));

                for (var i = 1; i <= loneCount; i++)
                {
                    var loneDistance = distance + gladeLength * i / (loneCount + 1f);
                    var position = config.StartPoint + along * loneDistance + across * (NextSigned(random) * config.LoneOffset);
                    obstacles.Add(new Obstacle(position, NextRadius(config, random)));
                }

                distance += gladeLength + NextInRange(random, config.ClusterLength);
            }
        }

        private readonly struct Glade
        {
            public Glade(float center, float halfLength)
            {
                Center = center;
                HalfLength = halfLength;
            }

            public float Center { get; }

            public float HalfLength { get; }
        }
    }
}
