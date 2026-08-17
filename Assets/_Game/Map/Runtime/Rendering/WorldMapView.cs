using System;
using System.Collections.Generic;
using Statecraft.Map.Data;
using Statecraft.Map.Projection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.Map.Rendering
{
    public readonly struct WorldMapCountryPresentation
    {
        public WorldMapCountryPresentation(bool isAvailable, Color accentColor)
        {
            IsAvailable = isAvailable;
            AccentColor = accentColor;
        }

        public bool IsAvailable { get; }
        public Color AccentColor { get; }
    }

    internal sealed class WorldMapGeometryElement : VisualElement
    {
        private static readonly Color UnavailableFill = new Color32(24, 37, 46, 255);
        private static readonly Color AvailableFill = new Color32(43, 61, 71, 255);
        private static readonly Color HoverFill = new Color32(104, 91, 65, 255);
        private static readonly Color UnavailableSelectedFill = new Color32(116, 97, 62, 255);
        private static readonly Color CountryBorder = new Color32(211, 205, 188, 82);
        private static readonly Color ActiveBorder = new Color32(240, 226, 193, 188);

        private readonly WorldMapProjectedGeometry geometry;
        private readonly IReadOnlyList<WorldMapProjectedCountry> sourceCountries;
        private readonly HashSet<string> geographicIds;
        private readonly IReadOnlyDictionary<string, WorldMapCountryPresentation> presentations;
        private RenderedCountry[] renderedCountries = Array.Empty<RenderedCountry>();
        private string hoveredCountryId;
        private string selectedCountryId;

        public WorldMapGeometryElement(
            WorldMapProjectedGeometry geometry,
            IReadOnlyList<WorldMapProjectedCountry> sourceCountries,
            IReadOnlyDictionary<string, WorldMapCountryPresentation> presentations)
        {
            this.geometry = geometry;
            this.sourceCountries = sourceCountries;
            this.presentations = presentations;
            geographicIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var country in sourceCountries)
            {
                geographicIds.Add(country.Source.GeographicId);
            }

            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0f;
            style.right = 0f;
            style.top = 0f;
            style.bottom = 0f;
            generateVisualContent += DrawMap;
        }

        public void SetMapRect(Rect mapRect)
        {
            if (mapRect.width <= 0f || mapRect.height <= 0f || geometry.Bounds.width <= 0f || geometry.Bounds.height <= 0f)
            {
                renderedCountries = Array.Empty<RenderedCountry>();
                return;
            }

            var fitScale = Mathf.Min(mapRect.width / geometry.Bounds.width, mapRect.height / geometry.Bounds.height);
            var renderedWidth = geometry.Bounds.width * fitScale;
            var renderedHeight = geometry.Bounds.height * fitScale;
            var origin = new Vector2(
                mapRect.center.x - renderedWidth * 0.5f,
                mapRect.center.y - renderedHeight * 0.5f);
            var countries = new RenderedCountry[sourceCountries.Count];

            for (var countryIndex = 0; countryIndex < sourceCountries.Count; countryIndex++)
            {
                var projectedCountry = sourceCountries[countryIndex];
                var polygons = new Vector2[projectedCountry.Polygons.Length][][];

                for (var polygonIndex = 0; polygonIndex < projectedCountry.Polygons.Length; polygonIndex++)
                {
                    var projectedPolygon = projectedCountry.Polygons[polygonIndex];
                    var rings = new Vector2[projectedPolygon.Rings.Length][];
                    for (var ringIndex = 0; ringIndex < projectedPolygon.Rings.Length; ringIndex++)
                    {
                        var projectedPoints = projectedPolygon.Rings[ringIndex].Points;
                        var renderedPoints = new Vector2[projectedPoints.Length];
                        for (var pointIndex = 0; pointIndex < projectedPoints.Length; pointIndex++)
                        {
                            var point = projectedPoints[pointIndex];
                            renderedPoints[pointIndex] = new Vector2(
                                origin.x + (point.x - geometry.Bounds.xMin) * fitScale,
                                origin.y + (geometry.Bounds.yMax - point.y) * fitScale);
                        }

                        rings[ringIndex] = renderedPoints;
                    }

                    polygons[polygonIndex] = rings;
                }

                countries[countryIndex] = new RenderedCountry(projectedCountry, polygons);
            }

            renderedCountries = countries;
            MarkDirtyRepaint();
        }

        public void SetHoveredCountry(string geographicId)
        {
            if (string.Equals(hoveredCountryId, geographicId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var mustRepaint = ContainsCountry(hoveredCountryId) || ContainsCountry(geographicId);
            hoveredCountryId = geographicId;
            if (mustRepaint)
            {
                MarkDirtyRepaint();
            }
        }

        public void SetSelectedCountry(string geographicId)
        {
            if (string.Equals(selectedCountryId, geographicId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var mustRepaint = ContainsCountry(selectedCountryId) || ContainsCountry(geographicId);
            selectedCountryId = geographicId;
            if (mustRepaint)
            {
                MarkDirtyRepaint();
            }
        }

        private bool ContainsCountry(string geographicId)
        {
            return !string.IsNullOrEmpty(geographicId) && geographicIds.Contains(geographicId);
        }

        private void DrawMap(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            for (var priority = 0; priority <= 3; priority++)
            {
                foreach (var country in renderedCountries)
                {
                    if (GetDrawPriority(country.Source.Source.GeographicId) == priority)
                    {
                        DrawCountry(painter, country);
                    }
                }
            }
        }

        private void DrawCountry(Painter2D painter, RenderedCountry country)
        {
            var geographicId = country.Source.Source.GeographicId;
            var isHovered = string.Equals(hoveredCountryId, geographicId, StringComparison.OrdinalIgnoreCase);
            var isSelected = string.Equals(selectedCountryId, geographicId, StringComparison.OrdinalIgnoreCase);
            var isAvailable = presentations.TryGetValue(geographicId, out var presentation) && presentation.IsAvailable;
            painter.fillColor = ResolveFill(isAvailable, isHovered, isSelected, presentation.AccentColor);
            painter.strokeColor = isHovered || isSelected ? ActiveBorder : CountryBorder;
            painter.lineWidth = isHovered || isSelected ? 1.15f : 0.55f;

            foreach (var polygon in country.Polygons)
            {
                painter.BeginPath();
                foreach (var ring in polygon)
                {
                    if (ring.Length < 3)
                    {
                        continue;
                    }

                    painter.MoveTo(ring[0]);
                    for (var pointIndex = 1; pointIndex < ring.Length; pointIndex++)
                    {
                        painter.LineTo(ring[pointIndex]);
                    }

                    painter.ClosePath();
                }

                painter.Fill(FillRule.OddEven);
                painter.Stroke();
            }
        }

        private int GetDrawPriority(string geographicId)
        {
            if (string.Equals(selectedCountryId, geographicId, StringComparison.OrdinalIgnoreCase))
            {
                return 3;
            }

            if (string.Equals(hoveredCountryId, geographicId, StringComparison.OrdinalIgnoreCase))
            {
                return 2;
            }

            return presentations.TryGetValue(geographicId, out var presentation) && presentation.IsAvailable ? 1 : 0;
        }

        private static Color ResolveFill(bool isAvailable, bool isHovered, bool isSelected, Color accentColor)
        {
            if (isSelected)
            {
                return isAvailable ? accentColor : UnavailableSelectedFill;
            }

            if (isHovered)
            {
                return HoverFill;
            }

            return isAvailable ? AvailableFill : UnavailableFill;
        }

        private sealed class RenderedCountry
        {
            public RenderedCountry(WorldMapProjectedCountry source, Vector2[][][] polygons)
            {
                Source = source;
                Polygons = polygons;
            }

            public WorldMapProjectedCountry Source { get; }
            public Vector2[][][] Polygons { get; }
        }
    }

    public sealed class WorldMapView : VisualElement
    {
        private const float MinimumZoom = 1f;
        private const float MaximumZoom = 5f;
        private const float MapPadding = 26f;
        private const float MinimumVisibleMargin = 54f;
        private const float ClickDragThreshold = 6f;
        private const int MaximumPointsPerGeometryChunk = 12000;

        private readonly WorldMapData sourceData;
        private readonly IWorldMapProjection projection;
        private readonly WorldMapProjectedGeometry projectedGeometry;
        private readonly VisualElement geometryLayer;
        private readonly WorldMapGeometryElement[] geometryChunks;
        private readonly Label debugLabel;
        private readonly IVisualElementScheduledItem viewAnimation;
        private Rect mapRect;
        private float currentZoom = MinimumZoom;
        private float targetZoom = MinimumZoom;
        private Vector2 currentPan;
        private Vector2 targetPan;
        private Vector2 pointerDownPosition;
        private Vector2 lastPointerPosition;
        private WorldMapProjectedCountry pointerDownCountry;
        private WorldMapProjectedCountry hoveredCountry;
        private int capturedPointerId = -1;
        private bool isPanning;
        private bool pointerMoved;
        private bool debugOverlayEnabled;

        public WorldMapView(
            WorldMapData data,
            IReadOnlyDictionary<string, WorldMapCountryPresentation> presentations)
        {
            sourceData = data ?? throw new ArgumentNullException(nameof(data));
            projection = new WinkelTripelProjection();
            projectedGeometry = new WorldMapProjectedGeometry(data, projection);
            geometryLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            geometryLayer.AddToClassList("world-map-geometry");
            geometryChunks = CreateGeometryChunks(projectedGeometry, presentations);
            foreach (var chunk in geometryChunks)
            {
                geometryLayer.Add(chunk);
            }

            debugLabel = new Label();
            debugLabel.AddToClassList("world-map-debug");
            debugLabel.pickingMode = PickingMode.Ignore;
            debugLabel.style.display = DisplayStyle.None;

            name = "world-map-view";
            AddToClassList("world-map-view");
            Add(geometryLayer);
            Add(debugLabel);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            RegisterCallback<WheelEvent>(OnWheel);

            viewAnimation = schedule.Execute(UpdateViewAnimation).Every(16);
            viewAnimation.Pause();
        }

        public event Action<WorldMapCountryData> HoveredCountryChanged;
        public event Action<WorldMapCountryData> CountryClicked;

        public string ProjectionName => projectedGeometry.ProjectionName;
        public float Zoom => currentZoom;
        public Vector2 Pan => currentPan;
        public WorldMapCountryData HoveredCountry => hoveredCountry?.Source;

        public bool DebugOverlayEnabled
        {
            get => debugOverlayEnabled;
            set
            {
                debugOverlayEnabled = value;
                debugLabel.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                UpdateDebugLabel();
            }
        }

        public void SetSelectedCountry(WorldMapCountryData country)
        {
            foreach (var chunk in geometryChunks)
            {
                chunk.SetSelectedCountry(country?.GeographicId);
            }

            UpdateDebugLabel();
        }

        public void ResetView()
        {
            currentZoom = targetZoom = MinimumZoom;
            currentPan = targetPan = Vector2.zero;
            ApplyViewTransform();
        }

        public WorldMapCountryData HitTestCountry(Vector2 localPosition)
        {
            return HitTestProjected(localPosition)?.Source;
        }

        public Vector2 GeographicToLocal(Vector2 longitudeLatitude)
        {
            if (mapRect.width <= 0f)
            {
                return Vector2.zero;
            }

            var projectedPoint = projection.Project(longitudeLatitude);
            var fitScale = Mathf.Min(
                mapRect.width / projectedGeometry.Bounds.width,
                mapRect.height / projectedGeometry.Bounds.height);
            var renderedSize = projectedGeometry.Bounds.size * fitScale;
            var renderedOrigin = mapRect.center - renderedSize * 0.5f;
            var basePoint = new Vector2(
                renderedOrigin.x + (projectedPoint.x - projectedGeometry.Bounds.xMin) * fitScale,
                renderedOrigin.y + (projectedGeometry.Bounds.yMax - projectedPoint.y) * fitScale);
            return basePoint * currentZoom + currentPan;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (evt.newRect.width <= 0f || evt.newRect.height <= 0f)
            {
                return;
            }

            mapRect = new Rect(
                MapPadding,
                MapPadding,
                Mathf.Max(1f, evt.newRect.width - MapPadding * 2f),
                Mathf.Max(1f, evt.newRect.height - MapPadding * 2f));
            foreach (var chunk in geometryChunks)
            {
                chunk.SetMapRect(mapRect);
            }
            ClampPan(ref currentPan, currentZoom);
            ClampPan(ref targetPan, targetZoom);
            ApplyViewTransform();
            UpdateDebugLabel();
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            var localPosition = ToVector2(evt.localPosition);
            var hitCountry = HitTestProjected(localPosition);
            var beginsPan = evt.button == 2 || evt.button == 0 && hitCountry == null;
            if (!beginsPan && (evt.button != 0 || hitCountry == null))
            {
                return;
            }

            capturedPointerId = evt.pointerId;
            pointerDownPosition = localPosition;
            lastPointerPosition = localPosition;
            pointerDownCountry = hitCountry;
            isPanning = beginsPan;
            pointerMoved = false;
            PointerCaptureHelper.CapturePointer(this, evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            var localPosition = ToVector2(evt.localPosition);
            if (capturedPointerId == evt.pointerId && PointerCaptureHelper.HasPointerCapture(this, evt.pointerId))
            {
                var distance = Vector2.Distance(pointerDownPosition, localPosition);
                pointerMoved |= distance > ClickDragThreshold;

                if (isPanning)
                {
                    var delta = localPosition - lastPointerPosition;
                    currentPan += delta;
                    ClampPan(ref currentPan, currentZoom);
                    targetPan = currentPan;
                    targetZoom = currentZoom;
                    ApplyViewTransform();
                }

                lastPointerPosition = localPosition;
                return;
            }

            SetHoveredCountry(HitTestProjected(localPosition));
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (capturedPointerId != evt.pointerId)
            {
                return;
            }

            if (!isPanning && !pointerMoved)
            {
                var releasedCountry = HitTestProjected(ToVector2(evt.localPosition));
                if (releasedCountry == pointerDownCountry && releasedCountry != null)
                {
                    foreach (var chunk in geometryChunks)
                    {
                        chunk.SetSelectedCountry(releasedCountry.Source.GeographicId);
                    }

                    CountryClicked?.Invoke(releasedCountry.Source);
                }
            }

            FinishPointerInteraction(evt.pointerId);
            SetHoveredCountry(HitTestProjected(ToVector2(evt.localPosition)));
            evt.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (capturedPointerId == evt.pointerId)
            {
                FinishPointerInteraction(evt.pointerId);
            }
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (capturedPointerId < 0)
            {
                SetHoveredCountry(null);
            }
        }

        private void OnWheel(WheelEvent evt)
        {
            if (mapRect.width <= 0f)
            {
                return;
            }

            var zoomFactor = Mathf.Exp(-evt.delta.y * 0.08f);
            var newTargetZoom = Mathf.Clamp(targetZoom * zoomFactor, MinimumZoom, MaximumZoom);
            if (Mathf.Abs(newTargetZoom - targetZoom) < 0.0001f)
            {
                return;
            }

            var cursor = evt.localMousePosition;
            var mapPointUnderCursor = (cursor - targetPan) / targetZoom;
            targetPan = cursor - mapPointUnderCursor * newTargetZoom;
            targetZoom = newTargetZoom;
            ClampPan(ref targetPan, targetZoom);
            viewAnimation.Resume();
            evt.StopPropagation();
        }

        private void UpdateViewAnimation()
        {
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, 0.24f);
            currentPan = Vector2.Lerp(currentPan, targetPan, 0.24f);
            ClampPan(ref currentPan, currentZoom);
            ApplyViewTransform();

            if (Mathf.Abs(currentZoom - targetZoom) < 0.001f && Vector2.SqrMagnitude(currentPan - targetPan) < 0.01f)
            {
                currentZoom = targetZoom;
                currentPan = targetPan;
                ApplyViewTransform();
                viewAnimation.Pause();
            }
        }

        private void ApplyViewTransform()
        {
            geometryLayer.style.scale = new Scale(new Vector3(currentZoom, currentZoom, 1f));
            geometryLayer.style.translate = new Translate(currentPan.x, currentPan.y);
            UpdateDebugLabel();
        }

        private WorldMapProjectedCountry HitTestProjected(Vector2 localPosition)
        {
            if (mapRect.width <= 0f || currentZoom <= 0f)
            {
                return null;
            }

            var basePoint = (localPosition - currentPan) / currentZoom;
            var fitScale = Mathf.Min(
                mapRect.width / projectedGeometry.Bounds.width,
                mapRect.height / projectedGeometry.Bounds.height);
            var renderedSize = projectedGeometry.Bounds.size * fitScale;
            var renderedOrigin = mapRect.center - renderedSize * 0.5f;
            var renderedRect = new Rect(renderedOrigin, renderedSize);
            if (!renderedRect.Contains(basePoint))
            {
                return null;
            }

            var projectedPoint = new Vector2(
                projectedGeometry.Bounds.xMin + (basePoint.x - renderedOrigin.x) / fitScale,
                projectedGeometry.Bounds.yMax - (basePoint.y - renderedOrigin.y) / fitScale);
            return projectedGeometry.HitTest(projectedPoint);
        }

        private void SetHoveredCountry(WorldMapProjectedCountry country)
        {
            if (hoveredCountry == country)
            {
                return;
            }

            hoveredCountry = country;
            foreach (var chunk in geometryChunks)
            {
                chunk.SetHoveredCountry(country?.Source.GeographicId);
            }

            HoveredCountryChanged?.Invoke(country?.Source);
            UpdateDebugLabel();
        }

        private void FinishPointerInteraction(int pointerId)
        {
            if (PointerCaptureHelper.HasPointerCapture(this, pointerId))
            {
                PointerCaptureHelper.ReleasePointer(this, pointerId);
            }

            capturedPointerId = -1;
            pointerDownCountry = null;
            isPanning = false;
            pointerMoved = false;
        }

        private void ClampPan(ref Vector2 pan, float zoom)
        {
            if (mapRect.width <= 0f)
            {
                pan = Vector2.zero;
                return;
            }

            var minimumX = resolvedStyle.width - MinimumVisibleMargin - mapRect.xMax * zoom;
            var maximumX = MinimumVisibleMargin - mapRect.xMin * zoom;
            var minimumY = resolvedStyle.height - MinimumVisibleMargin - mapRect.yMax * zoom;
            var maximumY = MinimumVisibleMargin - mapRect.yMin * zoom;
            pan.x = ClampAxis(pan.x, minimumX, maximumX);
            pan.y = ClampAxis(pan.y, minimumY, maximumY);
        }

        private static float ClampAxis(float value, float minimum, float maximum)
        {
            return minimum <= maximum ? Mathf.Clamp(value, minimum, maximum) : (minimum + maximum) * 0.5f;
        }

        private static Vector2 ToVector2(Vector3 value)
        {
            return new Vector2(value.x, value.y);
        }

        private static WorldMapGeometryElement[] CreateGeometryChunks(
            WorldMapProjectedGeometry geometry,
            IReadOnlyDictionary<string, WorldMapCountryPresentation> presentations)
        {
            var chunks = new List<WorldMapGeometryElement>();
            var countries = new List<WorldMapProjectedCountry>();
            var pointCount = 0;

            foreach (var country in geometry.Countries)
            {
                var countryPointCount = CountPoints(country);
                if (countries.Count > 0 && pointCount + countryPointCount > MaximumPointsPerGeometryChunk)
                {
                    chunks.Add(new WorldMapGeometryElement(geometry, countries.ToArray(), presentations));
                    countries.Clear();
                    pointCount = 0;
                }

                countries.Add(country);
                pointCount += countryPointCount;
            }

            if (countries.Count > 0)
            {
                chunks.Add(new WorldMapGeometryElement(geometry, countries.ToArray(), presentations));
            }

            return chunks.ToArray();
        }

        private static int CountPoints(WorldMapProjectedCountry country)
        {
            var pointCount = 0;
            foreach (var polygon in country.Polygons)
            {
                foreach (var ring in polygon.Rings)
                {
                    pointCount += ring.Points.Length;
                }
            }

            return pointCount;
        }

        private void UpdateDebugLabel()
        {
            if (!debugOverlayEnabled)
            {
                return;
            }

            var hoveredId = hoveredCountry?.Source.GeographicId ?? "—";
            debugLabel.text =
                $"ID: {hoveredId}\n" +
                $"Features: {sourceData.FeatureCount}  Polygons: {sourceData.PolygonCount}\n" +
                $"Projection: {projectedGeometry.ProjectionName}\n" +
                $"Bounds: {projectedGeometry.Bounds.xMin:F2}, {projectedGeometry.Bounds.yMin:F2} → " +
                $"{projectedGeometry.Bounds.xMax:F2}, {projectedGeometry.Bounds.yMax:F2}\n" +
                $"Zoom: {currentZoom:F2}  Pan: {currentPan.x:F0}, {currentPan.y:F0}";
        }
    }
}
