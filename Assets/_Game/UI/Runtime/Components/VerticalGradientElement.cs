using UnityEngine;
using UnityEngine.UIElements;

namespace Statecraft.UI.Components
{
    public sealed class VerticalGradientElement : VisualElement
    {
        private Color topColor = Color.clear;
        private Color bottomColor = Color.clear;

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
            MarkDirtyRepaint();
        }

        private void DrawGradient(MeshGenerationContext context)
        {
            var rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

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
