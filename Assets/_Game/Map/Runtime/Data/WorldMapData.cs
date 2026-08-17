using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Statecraft.Map.Data
{
    public sealed class WorldMapRing
    {
        public WorldMapRing(Vector2[] coordinates)
        {
            Coordinates = coordinates ?? Array.Empty<Vector2>();
        }

        public Vector2[] Coordinates { get; }
    }

    public sealed class WorldMapPolygon
    {
        public WorldMapPolygon(WorldMapRing[] rings)
        {
            Rings = rings ?? Array.Empty<WorldMapRing>();
        }

        public WorldMapRing[] Rings { get; }
    }

    public sealed class WorldMapCountryData
    {
        public WorldMapCountryData(string geographicId, string displayName, WorldMapPolygon[] polygons)
        {
            GeographicId = geographicId;
            DisplayName = displayName;
            Polygons = polygons ?? Array.Empty<WorldMapPolygon>();
        }

        public string GeographicId { get; }
        public string DisplayName { get; }
        public WorldMapPolygon[] Polygons { get; }
    }

    public sealed class WorldMapData
    {
        public const int BinaryMagic = 0x5343574D;
        public const int BinaryVersion = 1;
        public const string ResourcePath = "Map/WorldMapData";

        private readonly Dictionary<string, WorldMapCountryData> countriesById;

        private WorldMapData(
            string sourceName,
            int featureCount,
            int polygonCount,
            int pointCount,
            WorldMapCountryData[] countries)
        {
            SourceName = sourceName;
            FeatureCount = featureCount;
            PolygonCount = polygonCount;
            PointCount = pointCount;
            Countries = countries;
            countriesById = new Dictionary<string, WorldMapCountryData>(countries.Length, StringComparer.OrdinalIgnoreCase);

            foreach (var country in countries)
            {
                countriesById.Add(country.GeographicId, country);
            }
        }

        public string SourceName { get; }
        public int FeatureCount { get; }
        public int PolygonCount { get; }
        public int PointCount { get; }
        public IReadOnlyList<WorldMapCountryData> Countries { get; }

        public bool TryGetCountry(string geographicId, out WorldMapCountryData country)
        {
            return countriesById.TryGetValue(geographicId, out country);
        }

        public static WorldMapData LoadFromResources()
        {
            var binaryAsset = Resources.Load<TextAsset>(ResourcePath);
            if (binaryAsset == null)
            {
                throw new InvalidOperationException($"Missing world map data at Resources/{ResourcePath}.bytes.");
            }

            return Read(binaryAsset.bytes);
        }

        public static WorldMapData Read(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes, false);
            using var reader = new BinaryReader(stream);

            var magic = reader.ReadInt32();
            if (magic != BinaryMagic)
            {
                throw new InvalidDataException("Invalid Statecraft world map binary header.");
            }

            var version = reader.ReadInt32();
            if (version != BinaryVersion)
            {
                throw new InvalidDataException($"Unsupported world map binary version {version}.");
            }

            var sourceName = reader.ReadString();
            var featureCount = reader.ReadInt32();
            var polygonCount = reader.ReadInt32();
            var pointCount = reader.ReadInt32();
            var countryCount = reader.ReadInt32();
            var countries = new WorldMapCountryData[countryCount];

            for (var countryIndex = 0; countryIndex < countryCount; countryIndex++)
            {
                var geographicId = reader.ReadString();
                var displayName = reader.ReadString();
                var countryPolygonCount = reader.ReadInt32();
                var polygons = new WorldMapPolygon[countryPolygonCount];

                for (var polygonIndex = 0; polygonIndex < countryPolygonCount; polygonIndex++)
                {
                    var ringCount = reader.ReadInt32();
                    var rings = new WorldMapRing[ringCount];

                    for (var ringIndex = 0; ringIndex < ringCount; ringIndex++)
                    {
                        var coordinateCount = reader.ReadInt32();
                        var coordinates = new Vector2[coordinateCount];
                        for (var coordinateIndex = 0; coordinateIndex < coordinateCount; coordinateIndex++)
                        {
                            coordinates[coordinateIndex] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                        }

                        rings[ringIndex] = new WorldMapRing(coordinates);
                    }

                    polygons[polygonIndex] = new WorldMapPolygon(rings);
                }

                countries[countryIndex] = new WorldMapCountryData(geographicId, displayName, polygons);
            }

            if (stream.Position != stream.Length)
            {
                throw new InvalidDataException("Unexpected trailing bytes in world map data.");
            }

            return new WorldMapData(sourceName, featureCount, polygonCount, pointCount, countries);
        }
    }
}
