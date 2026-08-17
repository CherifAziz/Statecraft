using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Statecraft.Editor.Map
{
    internal sealed class NaturalEarthCountry
    {
        public string GeographicId { get; set; }
        public string DisplayName { get; set; }
        public List<List<List<Vector2>>> Polygons { get; } = new();
    }

    internal sealed class NaturalEarthDocument
    {
        public string Type { get; set; }
        public List<NaturalEarthCountry> Countries { get; } = new();
        public int PolygonCount { get; set; }
        public int RingCount { get; set; }
        public int PointCount { get; set; }
    }

    internal sealed class NaturalEarthGeoJsonParser
    {
        private readonly string json;
        private int index;

        public NaturalEarthGeoJsonParser(string json)
        {
            this.json = json ?? throw new ArgumentNullException(nameof(json));
        }

        public NaturalEarthDocument Parse()
        {
            var document = new NaturalEarthDocument();
            Expect('{');
            while (!TryConsume('}'))
            {
                var propertyName = ReadString();
                Expect(':');
                switch (propertyName)
                {
                    case "type":
                        document.Type = ReadString();
                        break;
                    case "features":
                        ParseFeatures(document);
                        break;
                    default:
                        SkipValue();
                        break;
                }

                ConsumePropertySeparator('}');
            }

            SkipWhitespace();
            if (index != json.Length)
            {
                Fail("Unexpected content after the root object.");
            }

            if (!string.Equals(document.Type, "FeatureCollection", StringComparison.Ordinal))
            {
                throw new FormatException($"Expected a GeoJSON FeatureCollection but found '{document.Type}'.");
            }

            return document;
        }

        private void ParseFeatures(NaturalEarthDocument document)
        {
            Expect('[');
            while (!TryConsume(']'))
            {
                document.Countries.Add(ParseFeature(document));
                ConsumeArraySeparator(']');
            }
        }

        private NaturalEarthCountry ParseFeature(NaturalEarthDocument document)
        {
            string featureType = null;
            string geographicId = null;
            string frenchName = null;
            string englishName = null;
            string fallbackName = null;
            List<List<List<Vector2>>> polygons = null;

            Expect('{');
            while (!TryConsume('}'))
            {
                var propertyName = ReadString();
                Expect(':');
                switch (propertyName)
                {
                    case "type":
                        featureType = ReadString();
                        break;
                    case "geometry":
                        polygons = ParseGeometry(document);
                        break;
                    case "properties":
                        ParseCountryProperties(ref geographicId, ref frenchName, ref englishName, ref fallbackName);
                        break;
                    default:
                        SkipValue();
                        break;
                }

                ConsumePropertySeparator('}');
            }

            if (!string.Equals(featureType, "Feature", StringComparison.Ordinal))
            {
                throw new FormatException($"Expected GeoJSON Feature but found '{featureType}'.");
            }

            if (string.IsNullOrWhiteSpace(geographicId) || geographicId == "-99")
            {
                throw new FormatException("Every map feature must provide a stable ADM0_A3 identifier.");
            }

            if (polygons == null || polygons.Count == 0)
            {
                throw new FormatException($"Country {geographicId} has no supported geometry.");
            }

            var country = new NaturalEarthCountry
            {
                GeographicId = geographicId,
                DisplayName = FirstUsable(frenchName, englishName, fallbackName, geographicId)
            };
            country.Polygons.AddRange(polygons);
            return country;
        }

        private List<List<List<Vector2>>> ParseGeometry(NaturalEarthDocument document)
        {
            string geometryType = null;
            List<List<List<Vector2>>> polygons = null;
            var deferredCoordinatesIndex = -1;

            Expect('{');
            while (!TryConsume('}'))
            {
                var propertyName = ReadString();
                Expect(':');
                switch (propertyName)
                {
                    case "type":
                        geometryType = ReadString();
                        break;
                    case "coordinates":
                        if (string.IsNullOrEmpty(geometryType))
                        {
                            deferredCoordinatesIndex = index;
                            SkipValue();
                            break;
                        }

                        polygons = ParseCoordinates(geometryType, document);
                        break;
                    default:
                        SkipValue();
                        break;
                }

                ConsumePropertySeparator('}');
            }

            if (polygons == null && deferredCoordinatesIndex >= 0)
            {
                var geometryEndIndex = index;
                index = deferredCoordinatesIndex;
                polygons = ParseCoordinates(geometryType, document);
                index = geometryEndIndex;
            }

            return polygons;
        }

        private List<List<List<Vector2>>> ParseCoordinates(string geometryType, NaturalEarthDocument document)
        {
            return geometryType switch
            {
                "Polygon" => new List<List<List<Vector2>>> { ParsePolygon(document) },
                "MultiPolygon" => ParseMultiPolygon(document),
                _ => throw new FormatException($"Unsupported GeoJSON geometry type '{geometryType}'.")
            };
        }

        private List<List<List<Vector2>>> ParseMultiPolygon(NaturalEarthDocument document)
        {
            var polygons = new List<List<List<Vector2>>>();
            Expect('[');
            while (!TryConsume(']'))
            {
                polygons.Add(ParsePolygon(document));
                ConsumeArraySeparator(']');
            }

            return polygons;
        }

        private List<List<Vector2>> ParsePolygon(NaturalEarthDocument document)
        {
            var rings = new List<List<Vector2>>();
            Expect('[');
            while (!TryConsume(']'))
            {
                rings.Add(ParseRing(document));
                ConsumeArraySeparator(']');
            }

            if (rings.Count == 0)
            {
                Fail("Polygon contains no rings.");
            }

            document.PolygonCount++;
            document.RingCount += rings.Count;
            return rings;
        }

        private List<Vector2> ParseRing(NaturalEarthDocument document)
        {
            var points = new List<Vector2>();
            Expect('[');
            while (!TryConsume(']'))
            {
                points.Add(ParsePosition());
                ConsumeArraySeparator(']');
            }

            if (points.Count > 1 && Approximately(points[0], points[points.Count - 1]))
            {
                points.RemoveAt(points.Count - 1);
            }

            if (points.Count < 3)
            {
                Fail("A polygon ring must contain at least three distinct points.");
            }

            document.PointCount += points.Count;
            return points;
        }

        private Vector2 ParsePosition()
        {
            Expect('[');
            var longitude = (float)ReadNumber();
            Expect(',');
            var latitude = (float)ReadNumber();

            while (TryConsume(','))
            {
                SkipValue();
            }

            Expect(']');
            return new Vector2(longitude, latitude);
        }

        private void ParseCountryProperties(
            ref string geographicId,
            ref string frenchName,
            ref string englishName,
            ref string fallbackName)
        {
            Expect('{');
            while (!TryConsume('}'))
            {
                var propertyName = ReadString();
                Expect(':');
                switch (propertyName)
                {
                    case "ADM0_A3":
                        geographicId = ReadString();
                        break;
                    case "NAME_FR":
                        frenchName = ReadString();
                        break;
                    case "NAME_EN":
                        englishName = ReadString();
                        break;
                    case "NAME":
                        fallbackName = ReadString();
                        break;
                    default:
                        SkipValue();
                        break;
                }

                ConsumePropertySeparator('}');
            }
        }

        private string ReadString()
        {
            SkipWhitespace();
            if (index >= json.Length || json[index] != '"')
            {
                Fail("Expected a JSON string.");
            }

            index++;
            var start = index;
            StringBuilder builder = null;

            while (index < json.Length)
            {
                var character = json[index++];
                if (character == '"')
                {
                    if (builder == null)
                    {
                        return json.Substring(start, index - start - 1);
                    }

                    builder.Append(json, start, index - start - 1);
                    return builder.ToString();
                }

                if (character != '\\')
                {
                    continue;
                }

                builder ??= new StringBuilder();
                builder.Append(json, start, index - start - 1);
                if (index >= json.Length)
                {
                    Fail("Unterminated JSON escape sequence.");
                }

                var escaped = json[index++];
                switch (escaped)
                {
                    case '"': builder.Append('"'); break;
                    case '\\': builder.Append('\\'); break;
                    case '/': builder.Append('/'); break;
                    case 'b': builder.Append('\b'); break;
                    case 'f': builder.Append('\f'); break;
                    case 'n': builder.Append('\n'); break;
                    case 'r': builder.Append('\r'); break;
                    case 't': builder.Append('\t'); break;
                    case 'u': builder.Append(ReadUnicodeEscape()); break;
                    default: Fail($"Unsupported JSON escape '\\{escaped}'."); break;
                }

                start = index;
            }

            Fail("Unterminated JSON string.");
            return null;
        }

        private char ReadUnicodeEscape()
        {
            if (index + 4 > json.Length)
            {
                Fail("Incomplete JSON unicode escape.");
            }

            var value = int.Parse(json.Substring(index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            index += 4;
            return (char)value;
        }

        private double ReadNumber()
        {
            SkipWhitespace();
            var start = index;
            while (index < json.Length)
            {
                var character = json[index];
                if ((character >= '0' && character <= '9') || character == '-' || character == '+' ||
                    character == '.' || character == 'e' || character == 'E')
                {
                    index++;
                    continue;
                }

                break;
            }

            if (start == index)
            {
                Fail("Expected a JSON number.");
            }

            return double.Parse(json.Substring(start, index - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private void SkipValue()
        {
            SkipWhitespace();
            if (index >= json.Length)
            {
                Fail("Unexpected end of JSON while skipping a value.");
            }

            switch (json[index])
            {
                case '"':
                    SkipString();
                    return;
                case '{':
                    SkipObject();
                    return;
                case '[':
                    SkipArray();
                    return;
                case 't':
                    ExpectLiteral("true");
                    return;
                case 'f':
                    ExpectLiteral("false");
                    return;
                case 'n':
                    ExpectLiteral("null");
                    return;
                default:
                    ReadNumber();
                    return;
            }
        }

        private void SkipString()
        {
            Expect('"');
            while (index < json.Length)
            {
                var character = json[index++];
                if (character == '"')
                {
                    return;
                }

                if (character == '\\' && index < json.Length)
                {
                    index++;
                }
            }

            Fail("Unterminated JSON string.");
        }

        private void SkipObject()
        {
            Expect('{');
            while (!TryConsume('}'))
            {
                SkipString();
                Expect(':');
                SkipValue();
                ConsumePropertySeparator('}');
            }
        }

        private void SkipArray()
        {
            Expect('[');
            while (!TryConsume(']'))
            {
                SkipValue();
                ConsumeArraySeparator(']');
            }
        }

        private void ExpectLiteral(string literal)
        {
            SkipWhitespace();
            if (index + literal.Length > json.Length ||
                !string.Equals(json.Substring(index, literal.Length), literal, StringComparison.Ordinal))
            {
                Fail($"Expected JSON literal '{literal}'.");
            }

            index += literal.Length;
        }

        private void ConsumePropertySeparator(char closingCharacter)
        {
            SkipWhitespace();
            if (index < json.Length && json[index] == closingCharacter)
            {
                return;
            }

            Expect(',');
        }

        private void ConsumeArraySeparator(char closingCharacter)
        {
            SkipWhitespace();
            if (index < json.Length && json[index] == closingCharacter)
            {
                return;
            }

            Expect(',');
        }

        private bool TryConsume(char character)
        {
            SkipWhitespace();
            if (index >= json.Length || json[index] != character)
            {
                return false;
            }

            index++;
            return true;
        }

        private void Expect(char character)
        {
            SkipWhitespace();
            if (index >= json.Length || json[index] != character)
            {
                Fail($"Expected '{character}'.");
            }

            index++;
        }

        private void SkipWhitespace()
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }
        }

        private void Fail(string message)
        {
            throw new FormatException($"{message} JSON offset: {index}.");
        }

        private static string FirstUsable(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value) && value != "-99")
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static bool Approximately(Vector2 a, Vector2 b)
        {
            return Mathf.Abs(a.x - b.x) < 0.000001f && Mathf.Abs(a.y - b.y) < 0.000001f;
        }
    }
}
