using UnityEngine;

namespace Statecraft.Map.Projection
{
    public interface IWorldMapProjection
    {
        string Name { get; }
        Vector2 Project(Vector2 longitudeLatitude);
    }

    public sealed class WinkelTripelProjection : IWorldMapProjection
    {
        private const float StandardParallelCosine = 0.6366197723675814f;

        public string Name => "Winkel Tripel";

        public Vector2 Project(Vector2 longitudeLatitude)
        {
            var longitude = longitudeLatitude.x * Mathf.Deg2Rad;
            var latitude = longitudeLatitude.y * Mathf.Deg2Rad;
            var halfLongitude = longitude * 0.5f;
            var cosineLatitude = Mathf.Cos(latitude);
            var alpha = Mathf.Acos(Mathf.Clamp(cosineLatitude * Mathf.Cos(halfLongitude), -1f, 1f));
            var sincAlpha = Mathf.Abs(alpha) < 0.000001f ? 1f : Mathf.Sin(alpha) / alpha;
            var aitoffX = 2f * cosineLatitude * Mathf.Sin(halfLongitude) / sincAlpha;
            var aitoffY = Mathf.Sin(latitude) / sincAlpha;

            return new Vector2(
                (aitoffX + longitude * StandardParallelCosine) * 0.5f,
                (aitoffY + latitude) * 0.5f);
        }
    }
}
