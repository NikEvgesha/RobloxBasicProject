using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeHomeIconMarker : MonoBehaviour
    {
        private const string RuntimeCanvasName = "KLC_HomeIconCanvas_Runtime";

        [SerializeField] private Transform target;
        [SerializeField] private Canvas canvas;
        [SerializeField] private string targetName = "KLC_PlotInstance_MOVE_PlotSlot_01";
        [SerializeField] private Vector3 worldOffset = new(0f, 1.35f, 0f);
        [SerializeField] private Vector2 iconSize = new(76f, 76f);
        [SerializeField] private bool clampToScreenEdge = true;
        [SerializeField, Min(0f)] private float screenPadding = 48f;
        [SerializeField] private bool useRendererBounds = true;

        private RectTransform iconRoot;
        private Transform cachedTarget;
        private Renderer[] targetRenderers = System.Array.Empty<Renderer>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeHomeIconMarker>(FindObjectsInactive.Include) != null
                || FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include) == null)
            {
                return;
            }

            new GameObject("KLC_HomeIconMarker_Runtime").AddComponent<KickLuckyCubeHomeIconMarker>();
        }

        private void LateUpdate()
        {
            ResolveReferences();
            EnsureIcon();

            if (target == null || canvas == null || iconRoot == null)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                iconRoot.gameObject.SetActive(false);
                return;
            }

            var screenPosition = GetClampedScreenPosition(camera.WorldToScreenPoint(GetTargetWorldPosition()), out var visible);
            iconRoot.gameObject.SetActive(visible);
            if (!visible)
            {
                return;
            }

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                iconRoot.position = screenPosition;
            }
            else
            {
                var canvasRect = canvas.transform as RectTransform;
                var canvasCamera = canvas.worldCamera != null ? canvas.worldCamera : camera;
                if (canvasRect != null
                    && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvasCamera, out var localPosition))
                {
                    iconRoot.anchoredPosition = localPosition;
                }
            }

            iconRoot.SetAsLastSibling();
        }

        private Vector3 GetClampedScreenPosition(Vector3 screenPosition, out bool visible)
        {
            visible = screenPosition.z > 0f || clampToScreenEdge;
            if (!visible)
            {
                return screenPosition;
            }

            if (!clampToScreenEdge)
            {
                return screenPosition;
            }

            var screenPoint = new Vector2(screenPosition.x, screenPosition.y);
            var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (screenPosition.z <= 0f)
            {
                screenPoint = screenCenter - (screenPoint - screenCenter);
            }

            screenPoint.x = Mathf.Clamp(screenPoint.x, screenPadding, Screen.width - screenPadding);
            screenPoint.y = Mathf.Clamp(screenPoint.y, screenPadding, Screen.height - screenPadding);
            return new Vector3(screenPoint.x, screenPoint.y, Mathf.Abs(screenPosition.z));
        }

        private void ResolveReferences()
        {
            if (target == null)
            {
                var targetObject = ResolvePlayerPlotObject();
                if (targetObject == null)
                {
                    targetObject = GameObject.Find(targetName);
                    if (targetObject == null)
                    {
                        targetObject = FindObjectByPartialName(targetName);
                    }
                }

                if (targetObject != null)
                {
                    target = targetObject.transform;
                }
            }

            if (canvas != null)
            {
                return;
            }

            var existingCanvasObject = GameObject.Find(RuntimeCanvasName);
            if (existingCanvasObject != null)
            {
                canvas = existingCanvasObject.GetComponent<Canvas>();
            }

            if (canvas == null)
            {
                var canvasObject = new GameObject(RuntimeCanvasName, typeof(RectTransform), typeof(Canvas));
                canvas = canvasObject.GetComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 5000;
        }

        private void EnsureIcon()
        {
            if (iconRoot != null || canvas == null)
            {
                return;
            }

            var iconObject = new GameObject("KLC_PlayerHomeIcon_Runtime", typeof(RectTransform));
            iconRoot = iconObject.GetComponent<RectTransform>();
            iconRoot.SetParent(canvas.transform, false);
            iconRoot.anchorMin = Vector2.zero;
            iconRoot.anchorMax = Vector2.zero;
            iconRoot.pivot = new Vector2(0.5f, 0.5f);
            iconRoot.sizeDelta = iconSize;

            var graphic = iconObject.AddComponent<KickLuckyCubeHomeIconGraphic>();
            graphic.raycastTarget = false;

            iconObject.SetActive(false);
        }

        private Vector3 GetTargetWorldPosition()
        {
            if (!useRendererBounds || target == null)
            {
                return target != null ? target.position + worldOffset : worldOffset;
            }

            if (cachedTarget != target)
            {
                cachedTarget = target;
                targetRenderers = target.GetComponentsInChildren<Renderer>(true);
            }

            var hasBounds = false;
            var bounds = new Bounds(target.position, Vector3.zero);
            for (var index = 0; index < targetRenderers.Length; index++)
            {
                var current = targetRenderers[index];
                if (current == null || !current.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = current.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(current.bounds);
                }
            }

            if (!hasBounds)
            {
                return target.position + worldOffset;
            }

            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z) + worldOffset;
        }

        private static GameObject ResolvePlayerPlotObject()
        {
            var playerPlot = GameObject.Find("KLC_PlayerPlot_Instance");
            if (playerPlot != null)
            {
                return playerPlot;
            }

            var allocator = FindFirstObjectByType<KickLuckyCubePlotAllocator>(FindObjectsInactive.Include);
            if (allocator != null && allocator.PlayerSlot != null)
            {
                return allocator.PlayerSlot.gameObject;
            }

            return null;
        }

        private static GameObject FindObjectByPartialName(string partialName)
        {
            var transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var index = 0; index < transforms.Length; index++)
            {
                var current = transforms[index];
                if (current != null && current.name.Contains(partialName))
                {
                    return current.gameObject;
                }
            }

            return null;
        }
    }

    public sealed class KickLuckyCubeHomeIconGraphic : MaskableGraphic
    {
        [SerializeField] private Color outlineColor = Color.black;
        [SerializeField] private Color roofColor = new(1f, 0.88f, 0.08f, 1f);
        [SerializeField] private Color bodyColor = new(0.12f, 0.78f, 1f, 1f);
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
