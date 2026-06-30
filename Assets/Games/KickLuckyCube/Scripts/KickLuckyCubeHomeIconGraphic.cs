using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeHomeIconGraphic : MaskableGraphic
    {
        [SerializeField] private Color outlineColor = KickLuckyCubeUiTheme.Outline;
        [SerializeField] private Color roofColor = KickLuckyCubeUiTheme.Warning;
        [SerializeField] private Color bodyColor = KickLuckyCubeUiTheme.Secondary;
        [SerializeField] private Color doorColor = new(1f, 1f, 1f, 1f);
        [SerializeField, Min(0f)] private float outlinePixels = 5f;

        protected override void OnPopulateMesh(VertexHelper vertexHelper)
        {
            vertexHelper.Clear();

            var rect = GetPixelAdjustedRect();
            var center = rect.center;
            var size = new Vector2(rect.width, rect.height);
            AddHouse(vertexHelper, center + new Vector2(-outlinePixels, 0f), size, outlineColor, outlineColor, outlineColor);
            AddHouse(vertexHelper, center + new Vector2(outlinePixels, 0f), size, outlineColor, outlineColor, outlineColor);
            AddHouse(vertexHelper, center + new Vector2(0f, -outlinePixels), size, outlineColor, outlineColor, outlineColor);
            AddHouse(vertexHelper, center + new Vector2(0f, outlinePixels), size, outlineColor, outlineColor, outlineColor);
            AddHouse(vertexHelper, center + new Vector2(outlinePixels * 0.55f, -outlinePixels * 0.55f), size, outlineColor, outlineColor, outlineColor);
            AddHouse(vertexHelper, center, size, roofColor, bodyColor, doorColor);
        }

        private static void AddHouse(VertexHelper vertexHelper, Vector2 center, Vector2 size, Color roof, Color body, Color door)
        {
            var bodyMin = center + new Vector2(size.x * -0.28f, size.y * -0.33f);
            var bodyMax = center + new Vector2(size.x * 0.28f, size.y * 0.12f);
            var roofLeft = center + new Vector2(size.x * -0.42f, size.y * 0.04f);
            var roofPeak = center + new Vector2(0f, size.y * 0.42f);
            var roofRight = center + new Vector2(size.x * 0.42f, size.y * 0.04f);
            var doorMin = center + new Vector2(size.x * -0.08f, size.y * -0.33f);
            var doorMax = center + new Vector2(size.x * 0.08f, size.y * -0.08f);

            AddTriangle(vertexHelper, roofLeft, roofPeak, roofRight, roof);
            AddQuad(vertexHelper, bodyMin, bodyMax, body);
            AddQuad(vertexHelper, doorMin, doorMax, door);
        }

        private static void AddQuad(VertexHelper vertexHelper, Vector2 min, Vector2 max, Color color)
        {
            var startIndex = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, new Vector2(min.x, min.y), color);
            AddVertex(vertexHelper, new Vector2(min.x, max.y), color);
            AddVertex(vertexHelper, new Vector2(max.x, max.y), color);
            AddVertex(vertexHelper, new Vector2(max.x, min.y), color);
            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
        }

        private static void AddTriangle(VertexHelper vertexHelper, Vector2 a, Vector2 b, Vector2 c, Color color)
        {
            var startIndex = vertexHelper.currentVertCount;
            AddVertex(vertexHelper, a, color);
            AddVertex(vertexHelper, b, color);
            AddVertex(vertexHelper, c, color);
            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        }

        private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Color color)
        {
            vertexHelper.AddVert(new Vector3(position.x, position.y, 0f), color, Vector2.zero);
        }
    }
}
