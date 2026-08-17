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

    internal sealed class WorldMapBackdropElement : VisualElement
    {
        private static readonly Color32 OceanEdge = new(2, 7, 11, 255);
        private static readonly Color32 OceanMid = new(4, 12, 18, 255);
        private static readonly Color32 OceanCenter = new(9, 22, 30, 255);
        private static readonly ushort[] GradientIndices =
        {
            0, 3, 1, 1, 3, 4,
            1, 4, 2, 2, 4, 5,
            3, 6, 4, 4, 6, 7,
            4, 7, 5, 5, 7, 8
        };

        public WorldMapBackdropElement()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList("world-map-backdrop");
            generateVisualContent += DrawBackdrop;
        }

        private void DrawBackdrop(MeshGenerationContext context)
        {
            var width = contentRect.width;
            var height = contentRect.height;
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            var vertices = new Vertex[9];
            var horizontal = new[] { 0f, width * 0.5f, width };
            var vertical = new[] { 0f, height * 0.5f, height };
            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 3; column++)
                {
                    var isCenter = row == 1 && column == 1;
                    var isCorner = row != 1 && column != 1;
                    vertices[row * 3 + column] = new Vertex
                    {
                        position = new Vector3(horizontal[column], vertical[row], Vertex.nearZ),
                        tint = isCenter ? OceanCenter : isCorner ? OceanEdge : OceanMid,
                        uv = Vector2.zero
                    };
                }
            }

            var mesh = context.Allocate(vertices.Length, GradientIndices.Length);
            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(GradientIndices);
        }
    }

    internal sealed class WorldMapGraticuleElement : VisualElement
    {
        private static readonly Color GraticuleColor = new Color32(149, 165, 169, 15);
        private readonly IWorldMapProjection projection;
        private readonly WorldMapProjectedGeometry geometry;
        private Vector2[][] lines = Array.Empty<Vector2[]>();

        public WorldMapGraticuleElement(IWorldMapProjection projection, WorldMapProjectedGeometry geometry)
        {
            this.projection = projection;
            this.geometry = geometry;
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.left = 0f;
            style.right = 0f;
            style.top = 0f;
            style.bottom = 0f;
            generateVisualContent += DrawGraticule;
        }

        public void SetMapRect(Rect mapRect)
        {
            var graticules = new List<Vector2[]>();
            for (var longitude = -150; longitude <= 150; longitude += 30)
            {
                var points = new Vector2[42];
                for (var index = 0; index < points.Length; index++)
                {
                    var latitude = -82f + index * 4f;
                    points[index] = ProjectToMap(new Vector2(longitude, latitude), mapRect);
                }

                graticules.Add(points);
            }

            for (var latitude = -60; latitude <= 60; latitude += 30)
            {
                var points = new Vector2[90];
                for (var index = 0; index < points.Length; index++)
                {
                    var longitude = -178f + index * 4f;
                    points[index] = ProjectToMap(new Vector2(longitude, latitude), mapRect);
                }

                graticules.Add(points);
            }

            lines = graticules.ToArray();
            MarkDirtyRepaint();
        }

        private Vector2 ProjectToMap(Vector2 longitudeLatitude, Rect mapRect)
        {
            var point = projection.Project(longitudeLatitude);
            var fitScale = Mathf.Min(mapRect.width / geometry.Bounds.width, mapRect.height / geometry.Bounds.height);
            var renderedSize = geometry.Bounds.size * fitScale;
            var origin = mapRect.center - renderedSize * 0.5f;
            return new Vector2(
                origin.x + (point.x - geometry.Bounds.xMin) * fitScale,
                origin.y + (geometry.Bounds.yMax - point.y) * fitScale);
        }

        private void DrawGraticule(MeshGenerationContext context)
        {
            var painter = context.painter2D;
            painter.strokeColor = GraticuleColor;
            painter.lineWidth = 0.38f;
            foreach (var line in lines)
            {
                if (line.Length < 2)
                {
                    continue;
                }

                painter.BeginPath();
                painter.MoveTo(line[0]);
                for (var index = 1; index < line.Length; index++)
                {
                    painter.LineTo(line[index]);
                }

                painter.Stroke();
            }
        }
    }

    internal sealed class WorldMapGeometryElement : VisualElement
    {
        private static readonly Color UnavailableFill = new Color32(21, 33, 41, 255);
        private static readonly Color AvailableFill = new Color32(29, 45, 54, 255);
        private static readonly Color UnavailableHoverFill = new Color32(42, 52, 56, 255);
        private static readonly Color AvailableHoverFill = new Color32(67, 67, 58, 255);
        private static readonly Color CountryBorder = new Color32(218, 211, 191, 46);
        private static readonly Color AvailableBorder = new Color32(220, 203, 161, 92);
        private static readonly Color HoverBorder = new Color32(238, 220, 180, 180);

        private readonly WorldMapProjectedGeometry geometry;
        private readonly IReadOnlyList<WorldMapProjectedCountry> sourceCountries;
        private readonly HashSet<string> geographicIds;
        private readonly IReadOnlyDictionary<string, WorldMapCountryPresentation> presentations;
        private RenderedCountry[] renderedCountries = Array.Empty<RenderedCountry>();
        private string hoveredCountryId;
        private string previousHoveredCountryId;
        private string selectedCountryId;
        private string previousSelectedCountryId;
        private float hoverTransition = 1f;
        private float selectionTransition = 1f;

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

        public bool SetHoveredCountry(string geographicId)
        {
            if (string.Equals(hoveredCountryId, geographicId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            previousHoveredCountryId = hoveredCountryId;
            hoveredCountryId = geographicId;
            hoverTransition = 0f;
            if (ContainsCountry(previousHoveredCountryId) || ContainsCountry(hoveredCountryId))
            {
                MarkDirtyRepaint();
            }

            return true;
        }

        public bool SetSelectedCountry(string geographicId)
        {
            if (string.Equals(selectedCountryId, geographicId, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            previousSelectedCountryId = selectedCountryId;
            selectedCountryId = geographicId;
            selectionTransition = 0f;
            MarkDirtyRepaint();
            return true;
        }

        public void SetHoverTransition(float value)
        {
            if (Mathf.Approximately(hoverTransition, value))
            {
                return;
            }

            hoverTransition = value;
            if (ContainsCountry(previousHoveredCountryId) || ContainsCountry(hoveredCountryId))
            {
                MarkDirtyRepaint();
            }

            if (hoverTransition >= 1f)
            {
                previousHoveredCountryId = null;
            }
        }

        public void SetSelectionTransition(float value)
        {
            if (Mathf.Approximately(selectionTransition, value))
            {
                return;
            }

            selectionTransition = value;
            if (ContainsCountry(previousSelectedCountryId) || ContainsCountry(selectedCountryId))
            {
                MarkDirtyRepaint();
            }

            if (selectionTransition >= 1f)
            {
                previousSelectedCountryId = null;
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
            var isAvailable = presentations.TryGetValue(geographicId, out var presentation) && presentation.IsAvailable;
            var hoverWeight = ResolveTransitionWeight(
                geographicId,
                hoveredCountryId,
                previousHoveredCountryId,
                hoverTransition);
            var selectionWeight = ResolveTransitionWeight(
                geographicId,
                selectedCountryId,
                previousSelectedCountryId,
                selectionTransition);
            var hasSelection = !string.IsNullOrEmpty(selectedCountryId) ||
                               !string.IsNullOrEmpty(previousSelectedCountryId);
            ResolvePaint(
                isAvailable,
                hasSelection,
                hoverWeight,
                selectionWeight,
                presentation.AccentColor,
                out var fill,
                out var border,
                out var lineWidth);
            painter.fillColor = fill;
            painter.strokeColor = border;
            painter.lineWidth = lineWidth;

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

        private static float ResolveTransitionWeight(
            string geographicId,
            string currentId,
            string previousId,
            float transition)
        {
            if (string.Equals(currentId, geographicId, StringComparison.OrdinalIgnoreCase))
            {
                return transition;
            }

            if (string.Equals(previousId, geographicId, StringComparison.OrdinalIgnoreCase))
            {
                return 1f - transition;
            }

            return 0f;
        }

        private static void ResolvePaint(
            bool isAvailable,
            bool hasSelection,
            float hoverWeight,
            float selectionWeight,
            Color accentColor,
            out Color fill,
            out Color border,
            out float lineWidth)
        {
            var baseFill = isAvailable ? AvailableFill : UnavailableFill;
            var baseBorder = isAvailable ? AvailableBorder : CountryBorder;
            if (hasSelection && selectionWeight <= 0f)
            {
                baseFill = Color.Lerp(baseFill, Color.black, 0.18f);
                baseBorder = Color.Lerp(baseBorder, Color.clear, 0.2f);
            }

            var hoverFill = isAvailable ? AvailableHoverFill : UnavailableHoverFill;
            fill = Color.Lerp(baseFill, hoverFill, hoverWeight);
            border = Color.Lerp(baseBorder, HoverBorder, hoverWeight);
            lineWidth = Mathf.Lerp(isAvailable ? 0.62f : 0.42f, 0.88f, hoverWeight);

            if (selectionWeight <= 0f)
            {
                return;
            }

            var selectedFill = Color.Lerp(baseFill, accentColor, 0.66f);
            var selectedBorder = Color.Lerp(accentColor, Color.white, 0.24f);
            selectedBorder.a = 0.94f;
            fill = Color.Lerp(fill, selectedFill, selectionWeight);
            border = Color.Lerp(border, selectedBorder, selectionWeight);
            lineWidth = Mathf.Lerp(lineWidth, 1.36f, selectionWeight);
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
        private const float MapPadding = 8f;
        private const float MinimumVisibleMargin = 54f;
        private const float ClickDragThreshold = 6f;
        private const int MaximumPointsPerGeometryChunk = 12000;
        private const float HoverTransitionDuration = 0.15f;
        private const float SelectionTransitionDuration = 0.18f;

        private readonly WorldMapData sourceData;
        private readonly IWorldMapProjection projection;
        private readonly WorldMapProjectedGeometry projectedGeometry;
        private readonly WorldMapBackdropElement backdropElement;
        private readonly VisualElement geometryLayer;
        private readonly WorldMapGraticuleElement graticuleElement;
        private readonly WorldMapGeometryElement[] geometryChunks;
        private readonly Label debugLabel;
        private readonly IVisualElementScheduledItem viewAnimation;
        private readonly IVisualElementScheduledItem interactionAnimation;
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
        private bool hoverTransitionActive;
        private bool selectionTransitionActive;
        private float hoverTransitionStartedAt;
        private float selectionTransitionStartedAt;

        public WorldMapView(
            WorldMapData data,
            IReadOnlyDictionary<string, WorldMapCountryPresentation> presentations)
        {
            sourceData = data ?? throw new ArgumentNullException(nameof(data));
            projection = new WinkelTripelProjection();
            projectedGeometry = new WorldMapProjectedGeometry(data, projection);
            backdropElement = new WorldMapBackdropElement();
            geometryLayer = new VisualElement { pickingMode = PickingMode.Ignore };
            geometryLayer.AddToClassList("world-map-geometry");
            graticuleElement = new WorldMapGraticuleElement(projection, projectedGeometry);
            geometryLayer.Add(graticuleElement);
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
            Add(backdropElement);
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
            interactionAnimation = schedule.Execute(UpdateInteractionAnimation).Every(16);
            interactionAnimation.Pause();
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
            var changed = false;
            foreach (var chunk in geometryChunks)
            {
                changed |= chunk.SetSelectedCountry(country?.GeographicId);
            }

            if (changed)
            {
                selectionTransitionActive = true;
                selectionTransitionStartedAt = Time.realtimeSinceStartup;
                interactionAnimation.Resume();
            }

            UpdateDebugLabel();
        }

        public void FocusCountry(WorldMapCountryData country)
        {
            if (country == null || mapRect.width <= 0f ||
                !projectedGeometry.TryGetCountry(country.GeographicId, out var projectedCountry))
            {
                return;
            }

            var fitScale = Mathf.Min(
                mapRect.width / projectedGeometry.Bounds.width,
                mapRect.height / projectedGeometry.Bounds.height);
            var renderedSize = projectedGeometry.Bounds.size * fitScale;
            var renderedOrigin = mapRect.center - renderedSize * 0.5f;
            var projectedCenter = projectedCountry.Bounds.center;
            var baseCenter = new Vector2(
                renderedOrigin.x + (projectedCenter.x - projectedGeometry.Bounds.xMin) * fitScale,
                renderedOrigin.y + (projectedGeometry.Bounds.yMax - projectedCenter.y) * fitScale);

            targetZoom = Mathf.Max(targetZoom, 1.34f);
            var viewportCenter = new Vector2(resolvedStyle.width * 0.5f, resolvedStyle.height * 0.5f);
            targetPan = viewportCenter - baseCenter * targetZoom;
            ClampPan(ref targetPan, targetZoom);
            viewAnimation.Resume();
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
            graticuleElement.SetMapRect(mapRect);
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
                    SetSelectedCountry(releasedCountry.Source);
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

        private void UpdateInteractionAnimation()
        {
            var now = Time.realtimeSinceStartup;
            if (hoverTransitionActive)
            {
                var progress = Mathf.Clamp01((now - hoverTransitionStartedAt) / HoverTransitionDuration);
                var eased = Mathf.SmoothStep(0f, 1f, progress);
                foreach (var chunk in geometryChunks)
                {
                    chunk.SetHoverTransition(eased);
                }

                hoverTransitionActive = progress < 1f;
            }

            if (selectionTransitionActive)
            {
                var progress = Mathf.Clamp01((now - selectionTransitionStartedAt) / SelectionTransitionDuration);
                var eased = Mathf.SmoothStep(0f, 1f, progress);
                foreach (var chunk in geometryChunks)
                {
                    chunk.SetSelectionTransition(eased);
                }

                selectionTransitionActive = progress < 1f;
            }

            if (!hoverTransitionActive && !selectionTransitionActive)
            {
                interactionAnimation.Pause();
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
            var changed = false;
            foreach (var chunk in geometryChunks)
            {
                changed |= chunk.SetHoveredCountry(country?.Source.GeographicId);
            }

            if (changed)
            {
                hoverTransitionActive = true;
                hoverTransitionStartedAt = Time.realtimeSinceStartup;
                interactionAnimation.Resume();
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
