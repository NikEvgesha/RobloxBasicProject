using UnityEngine;

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

            iconRoot = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_PlayerHomeIcon_Runtime", canvas.transform);
            iconRoot.anchorMin = Vector2.zero;
            iconRoot.anchorMax = Vector2.zero;
            iconRoot.pivot = new Vector2(0.5f, 0.5f);
            iconRoot.sizeDelta = iconSize;

            var graphic = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<KickLuckyCubeHomeIconGraphic>(iconRoot.gameObject);
            graphic.raycastTarget = false;

            iconRoot.gameObject.SetActive(false);
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
            if (allocator != null && allocator.PlayerPlot != null)
            {
                return allocator.PlayerPlot.gameObject;
            }

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

}
