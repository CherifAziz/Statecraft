using System;
using System.Collections.Generic;
using System.IO;
using Statecraft.Map.Data;
using UnityEditor;
using UnityEngine;

namespace Statecraft.Editor.Map
{
    public static class WorldMapDataImporter
    {
        private const string SourcePath = "Assets/_Game/Map/Data/world_admin0_50m.json";
        private const string OutputDirectory = "Assets/_Game/Resources/Map";
        private const string OutputPath = OutputDirectory + "/WorldMapData.bytes";

        [MenuItem("Statecraft/Map/Rebuild World Map Data")]
        public static void Rebuild()
        {
            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(SourcePath);
            if (source == null)
            {
                throw new FileNotFoundException($"Natural Earth GeoJSON is missing at {SourcePath}.");
            }

            var document = new NaturalEarthGeoJsonParser(source.text).Parse();
            Validate(document);

            Directory.CreateDirectory(OutputDirectory);
            using (var stream = File.Open(OutputPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(WorldMapData.BinaryMagic);
                writer.Write(WorldMapData.BinaryVersion);
                writer.Write(Path.GetFileName(SourcePath));
                writer.Write(document.Countries.Count);
                writer.Write(document.PolygonCount);
                writer.Write(document.PointCount);
                writer.Write(document.Countries.Count);

                foreach (var country in document.Countries)
                {
                    writer.Write(country.GeographicId);
                    writer.Write(country.DisplayName);
                    writer.Write(country.Polygons.Count);

                    foreach (var polygon in country.Polygons)
                    {
                        writer.Write(polygon.Count);
                        foreach (var ring in polygon)
                        {
                            writer.Write(ring.Count);
                            foreach (var coordinate in ring)
                            {
                                writer.Write(coordinate.x);
                                writer.Write(coordinate.y);
                            }
                        }
                    }
                }
            }

            AssetDatabase.ImportAsset(OutputPath, ImportAssetOptions.ForceUpdate);
            Debug.Log(
                $"Statecraft world map rebuilt: {document.Countries.Count} features, " +
                $"{document.PolygonCount} polygons, {document.RingCount} rings, {document.PointCount} points.");
        }

        public static void RebuildFromCommandLine()
        {
            Rebuild();
        }

        private static void Validate(NaturalEarthDocument document)
        {
            if (!string.Equals(document.Type, "FeatureCollection", StringComparison.Ordinal))
            {
                throw new InvalidDataException("Natural Earth source is not a GeoJSON FeatureCollection.");
            }

            var identifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var country in document.Countries)
            {
                if (!identifiers.Add(country.GeographicId))
                {
                    throw new InvalidDataException($"Duplicate ADM0_A3 identifier '{country.GeographicId}'.");
                }
            }

            RequireCountry(identifiers, "FRA");
            RequireCountry(identifiers, "TUN");
        }

        private static void RequireCountry(ISet<string> identifiers, string geographicId)
        {
            if (!identifiers.Contains(geographicId))
            {
                throw new InvalidDataException($"Required country '{geographicId}' is missing from the map source.");
            }
        }
    }
}
