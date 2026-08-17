using System;
using System.Collections.Generic;
using Statecraft.Map.Data;
using Statecraft.Map.Projection;
using UnityEngine;

namespace Statecraft.Map.Rendering
{
    public sealed class WorldMapProjectedRing
    {
        public WorldMapProjectedRing(Vector2[] points, Rect bounds)
        {
            Points = points;
            Bounds = bounds;
        }

        public Vector2[] Points { get; }
        public Rect Bounds { get; }
    }

    public sealed class WorldMapProjectedPolygon
    {
        public WorldMapProjectedPolygon(WorldMapProjectedRing[] rings, Rect bounds)
        {
            Rings = rings;
            Bounds = bounds;
        }

        public WorldMapProjectedRing[] Rings { get; }
        public Rect Bounds { get; }

        public bool Contains(Vector2 point)
        {
            if (Rings.Length == 0 || !ContainsInclusive(Bounds, point) || !ContainsPoint(Rings[0].Points, point))
            {
                return false;
            }

            for (var ringIndex = 1; ringIndex < Rings.Length; ringIndex++)
            {
                var hole = Rings[ringIndex];
                if (ContainsInclusive(hole.Bounds, point) && ContainsPoint(hole.Points, point))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsPoint(IReadOnlyList<Vector2> points, Vector2 point)
        {
            var inside = false;
            for (int current = 0, previous = points.Count - 1; current < points.Count; previous = current++)
            {
                var a = points[current];
                var b = points[previous];
                var crossesLatitude = (a.y > point.y) != (b.y > point.y);
                if (!crossesLatitude)
                {
                    continue;
                }

                var edgeLongitude = (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x;
                if (point.x < edgeLongitude)
                {
                    inside = !inside;
                }
            }

            return inside;
        }

        private static bool ContainsInclusive(Rect bounds, Vector2 point)
        {
            return point.x >= bounds.xMin && point.x <= bounds.xMax &&
                   point.y >= bounds.yMin && point.y <= bounds.yMax;
        }
    }

    public sealed class WorldMapProjectedCountry
    {
        public WorldMapProjectedCountry(WorldMapCountryData source, WorldMapProjectedPolygon[] polygons, Rect bounds)
        {
            Source = source;
            Polygons = polygons;
            Bounds = bounds;
        }

        public WorldMapCountryData Source { get; }
        public WorldMapProjectedPolygon[] Polygons { get; }
        public Rect Bounds { get; }

        public bool Contains(Vector2 point)
        {
            if (point.x < Bounds.xMin || point.x > Bounds.xMax || point.y < Bounds.yMin || point.y > Bounds.yMax)
            {
                return false;
            }

            foreach (var polygon in Polygons)
            {
                if (polygon.Contains(point))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class WorldMapProjectedGeometry
    {
        private readonly Dictionary<string, WorldMapProjectedCountry> countriesById;

        public WorldMapProjectedGeometry(WorldMapData source, IWorldMapProjection projection)
        {
            ProjectionName = projection.Name;
            Countries = new WorldMapProjectedCountry[source.Countries.Count];
            countriesById = new Dictionary<string, WorldMapProjectedCountry>(Countries.Length, StringComparer.OrdinalIgnoreCase);

            var hasWorldBounds = false;
            var worldBounds = default(Rect);

            for (var countryIndex = 0; countryIndex < source.Countries.Count; countryIndex++)
            {
                var country = source.Countries[countryIndex];
                var projectedPolygons = new WorldMapProjectedPolygon[country.Polygons.Length];
                var hasCountryBounds = false;
                var countryBounds = default(Rect);

                for (var polygonIndex = 0; polygonIndex < country.Polygons.Length; polygonIndex++)
                {
                    var polygon = country.Polygons[polygonIndex];
                    var projectedRings = new WorldMapProjectedRing[polygon.Rings.Length];
                    var hasPolygonBounds = false;
                    var polygonBounds = default(Rect);

                    for (var ringIndex = 0; ringIndex < polygon.Rings.Length; ringIndex++)
                    {
                        var coordinates = polygon.Rings[ringIndex].Coordinates;
                        var points = new Vector2[coordinates.Length];
                        var ringBounds = default(Rect);

                        for (var pointIndex = 0; pointIndex < coordinates.Length; pointIndex++)
                        {
                            points[pointIndex] = projection.Project(coordinates[pointIndex]);
                            ringBounds = Encapsulate(ringBounds, points[pointIndex], pointIndex > 0);
                        }

                        projectedRings[ringIndex] = new WorldMapProjectedRing(points, ringBounds);
                        polygonBounds = Encapsulate(polygonBounds, ringBounds, hasPolygonBounds);
                        hasPolygonBounds = true;
                    }

                    projectedPolygons[polygonIndex] = new WorldMapProjectedPolygon(projectedRings, polygonBounds);
                    countryBounds = Encapsulate(countryBounds, polygonBounds, hasCountryBounds);
                    hasCountryBounds = true;
                }

                var projectedCountry = new WorldMapProjectedCountry(country, projectedPolygons, countryBounds);
                Countries[countryIndex] = projectedCountry;
                countriesById.Add(country.GeographicId, projectedCountry);
                worldBounds = Encapsulate(worldBounds, countryBounds, hasWorldBounds);
                hasWorldBounds = true;
            }

            Bounds = worldBounds;
        }

        public string ProjectionName { get; }
        public WorldMapProjectedCountry[] Countries { get; }
        public Rect Bounds { get; }

        public WorldMapProjectedCountry HitTest(Vector2 projectedPoint)
        {
            foreach (var country in Countries)
            {
                if (country.Contains(projectedPoint))
                {
                    return country;
                }
            }

            return null;
        }

        public bool TryGetCountry(string geographicId, out WorldMapProjectedCountry country)
        {
            return countriesById.TryGetValue(geographicId, out country);
        }

        private static Rect Encapsulate(Rect bounds, Vector2 point, bool initialized)
        {
            return initialized
                ? Rect.MinMaxRect(
                    Mathf.Min(bounds.xMin, point.x),
                    Mathf.Min(bounds.yMin, point.y),
                    Mathf.Max(bounds.xMax, point.x),
                    Mathf.Max(bounds.yMax, point.y))
                : Rect.MinMaxRect(point.x, point.y, point.x, point.y);
        }

        private static Rect Encapsulate(Rect bounds, Rect addition, bool initialized)
        {
            return initialized
                ? Rect.MinMaxRect(
                    Mathf.Min(bounds.xMin, addition.xMin),
                    Mathf.Min(bounds.yMin, addition.yMin),
                    Mathf.Max(bounds.xMax, addition.xMax),
                    Mathf.Max(bounds.yMax, addition.yMax))
                : addition;
        }
    }
}
