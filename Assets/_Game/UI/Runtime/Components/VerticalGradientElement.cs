using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public sealed class VerticalGradientElement : VisualElement
    {
        private Color topColor = Color.clear;
        private Color middleColor = Color.clear;
        private Color bottomColor = Color.clear;
        private float middlePosition = 0.5f;
        private bool usesMiddleStop;

        public VerticalGradientElement(string className)
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(className);
            generateVisualContent += DrawGradient;
        }

        public void SetColors(Color top, Color bottom)
        {
            topColor = top;
            bottomColor = bottom;
            usesMiddleStop = false;
            MarkDirtyRepaint();
        }

        public void SetColors(Color top, Color middle, Color bottom, float middleStop)
        {
            topColor = top;
            middleColor = middle;
            bottomColor = bottom;
            middlePosition = Mathf.Clamp(middleStop, 0.05f, 0.95f);
            usesMiddleStop = true;
            MarkDirtyRepaint();
        }

        private void DrawGradient(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            if (!usesMiddleStop)
            {
                DrawTwoStopGradient(context, rect);
                return;
            }

            var middleY = Mathf.Lerp(rect.yMin, rect.yMax, middlePosition);
            var mesh = context.Allocate(6, 12);
            var vertices = new Vertex[6];
            vertices[0] = VertexAt(rect.xMin, rect.yMin, topColor);
            vertices[1] = VertexAt(rect.xMax, rect.yMin, topColor);
            vertices[2] = VertexAt(rect.xMin, middleY, middleColor);
            vertices[3] = VertexAt(rect.xMax, middleY, middleColor);
            vertices[4] = VertexAt(rect.xMin, rect.yMax, bottomColor);
            vertices[5] = VertexAt(rect.xMax, rect.yMax, bottomColor);

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(new ushort[]
            {
                0, 1, 2,
                1, 3, 2,
                2, 3, 4,
                3, 5, 4
            });
        }

        private void DrawTwoStopGradient(MeshGenerationContext context, Rect rect)
        {
            var mesh = context.Allocate(4, 6);
            var vertices = new Vertex[4];
            vertices[0] = VertexAt(rect.xMin, rect.yMin, topColor);
            vertices[1] = VertexAt(rect.xMax, rect.yMin, topColor);
            vertices[2] = VertexAt(rect.xMin, rect.yMax, bottomColor);
            vertices[3] = VertexAt(rect.xMax, rect.yMax, bottomColor);

            mesh.SetAllVertices(vertices);
            mesh.SetAllIndices(new ushort[] { 0, 1, 2, 1, 3, 2 });
        }

        private static Vertex VertexAt(float x, float y, Color color)
        {
            return new Vertex
            {
                position = new Vector3(x, y, Vertex.nearZ),
                tint = color
            };
        }
    }
}
