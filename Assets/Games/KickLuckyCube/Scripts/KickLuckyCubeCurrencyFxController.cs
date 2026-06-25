using System;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DefaultExecutionOrder(-150)]
    public sealed class KickLuckyCubeCurrencyFxController : MonoBehaviour
    {
        private const string RuntimeCanvasName = "KLC_CurrencyFxCanvas_Runtime";

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform softTarget;
        [SerializeField] private RectTransform hardTarget;
        [SerializeField] private Transform sourceTransform;
        [SerializeField] private Vector3 sourceWorldOffset = new(0f, 2.05f, 0f);
        [SerializeField] private Vector2 fallbackSoftTarget = new(720f, 304f);
        [SerializeField] private Vector2 fallbackHardTarget = new(720f, 246f);
        [SerializeField, Min(1)] private int burstCount = 7;
        [SerializeField, Min(0.1f)] private float flySeconds = 0.78f;

        private RectTransform canvasRect;
        private RectTransform layer;
        private Font uiFont;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeCurrencyFxController>(FindObjectsInactive.Include) != null
                || FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include) == null)
            {
                return;
            }

            new GameObject("KLC_CurrencyFxController_Runtime").AddComponent<KickLuckyCubeCurrencyFxController>();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (wallet != null)
            {
                wallet.CurrencyGained += OnCurrencyGained;
            }
        }

        private void OnDisable()
        {
            if (wallet != null)
            {
                wallet.CurrencyGained -= OnCurrencyGained;
            }
        }

        private void OnCurrencyGained(int soft, int hard)
        {
            if (soft > 0)
            {
                SpawnCurrencyFx(soft, "soft", ResolveSoftColor(), ResolveTarget(softTarget, fallbackSoftTarget));
            }

            if (hard > 0)
            {
                SpawnCurrencyFx(hard, "hard", ResolveHardColor(), ResolveTarget(hardTarget, fallbackHardTarget));
            }
        }

        private void SpawnCurrencyFx(int amount, string label, Color color, Vector2 target)
        {
            ResolveReferences();
            if (amount <= 0 || canvasRect == null)
            {
                return;
            }

            var fxLayer = GetLayer();
            var start = ResolveStartPosition();
            for (var index = 0; index < burstCount; index++)
            {
                CreateCurrencyText(fxLayer, amount, label, color, start, target, index);
            }
        }

        private void CreateCurrencyText(
            RectTransform parent,
            int amount,
            string label,
            Color color,
            Vector2 start,
            Vector2 target,
            int index)
        {
            var isMain = index == 0;
            var rect = KickLuckyCubeUiPrefabFactory.CreateRectInstance("KLC_CurrencyGainFlyText", "KLC_CurrencyGainFlyText", parent);
            var textObject = rect.gameObject;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = isMain ? new Vector2(190f, 56f) : new Vector2(104f, 36f);

            var text = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<Text>(textObject);
            text.font = uiFont;
            text.text = isMain ? $"+{amount:0} {label}" : $"+{Mathf.Max(1, amount / burstCount):0}";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = isMain ? 38 : 24;
            KickLuckyCubeUiTheme.StyleFloatingText(text, color);

            var group = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<CanvasGroup>(textObject);
            group.blocksRaycasts = false;
            group.interactable = false;

            var angle = index / (float)Mathf.Max(1, burstCount) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.25f, 0.25f);
            var radius = isMain ? 44f : UnityEngine.Random.Range(42f, 104f);
            var burst = start + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius + new Vector2(0f, UnityEngine.Random.Range(20f, 58f));
            var delay = isMain ? 0f : UnityEngine.Random.Range(0.025f, 0.16f);
            var duration = flySeconds + UnityEngine.Random.Range(-0.06f, 0.1f);
            rect.anchoredPosition = start;
            KickLuckyCubeUiPrefabFactory.GetOrAddComponent<CurrencyFlyTextMotion>(textObject)
                .Initialize(rect, group, start, burst, target, duration, delay, isMain ? 1.3f : 0.95f);
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            uiFont ??= KickLuckyCubeUiTheme.Font;
            ResolveCanvas();
            ResolveSource();
            ResolveTargets();
        }

        private void ResolveCanvas()
        {
            if (canvas != null && canvasRect != null)
            {
                return;
            }

            var existingCanvas = GameObject.Find(RuntimeCanvasName);
            if (existingCanvas != null)
            {
                canvas = existingCanvas.GetComponent<Canvas>();
            }

            if (canvas == null)
            {
                var canvasObject = new GameObject(RuntimeCanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1600f, 900f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 5200;
            canvasRect = canvas.transform as RectTransform;
        }

        private void ResolveSource()
        {
            if (sourceTransform != null)
            {
                return;
            }

            var player = FindFirstObjectByType<KickLuckyCubePlayerController>(FindObjectsInactive.Include);
            if (player != null)
            {
                sourceTransform = player.transform;
                return;
            }

            var runner = FindFirstObjectByType<KickLuckyCubeAnimalRunner>(FindObjectsInactive.Include);
            sourceTransform = runner != null ? runner.transform : null;
        }

        private void ResolveTargets()
        {
            if (softTarget != null && hardTarget != null)
            {
                return;
            }

            softTarget ??= FindUiTarget("KLC_BottomLeftSoftValue");
            hardTarget ??= FindUiTarget("KLC_BottomLeftHardValue");
            if (softTarget != null && hardTarget != null)
            {
                return;
            }

            var texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var index = 0; index < texts.Length; index++)
            {
                var current = texts[index];
                if (current == null)
                {
                    continue;
                }

                var text = current.text ?? string.Empty;
                if (softTarget == null && text.IndexOf("Soft", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    softTarget = current.rectTransform;
                }

                if (hardTarget == null && text.IndexOf("Hard", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    hardTarget = current.rectTransform;
                }
            }
        }

        private static RectTransform FindUiTarget(string objectName)
        {
            var target = GameObject.Find(objectName);
            return target != null ? target.GetComponent<RectTransform>() : null;
        }

        private RectTransform GetLayer()
        {
            if (layer != null)
            {
                layer.SetAsLastSibling();
                return layer;
            }

            layer = canvasRect.Find("KLC_CurrencyFlyTextLayer") as RectTransform;
            if (layer == null)
            {
                layer = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_CurrencyFlyTextLayer", canvasRect);
                layer.anchorMin = Vector2.zero;
                layer.anchorMax = Vector2.one;
                layer.offsetMin = Vector2.zero;
                layer.offsetMax = Vector2.zero;
                layer.pivot = new Vector2(0.5f, 0.5f);
            }

            layer.SetAsLastSibling();
            return layer;
        }

        private Vector2 ResolveStartPosition()
        {
            var camera = Camera.main;
            var screenPosition = new Vector2(Screen.width * 0.5f, Screen.height * 0.56f);
            if (sourceTransform != null && camera != null)
            {
                screenPosition = RectTransformUtility.WorldToScreenPoint(camera, sourceTransform.position + sourceWorldOffset);
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out var localPoint)
                ? localPoint
                : Vector2.zero;
        }

        private Vector2 ResolveTarget(RectTransform targetRect, Vector2 fallback)
        {
            if (targetRect != null && canvasRect != null)
            {
                var screenPosition = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, null, out var localPoint))
                {
                    return localPoint;
                }
            }

            return fallback;
        }

        private static Color ResolveSoftColor()
        {
            return KickLuckyCubeUiTheme.SoftCurrency;
        }

        private static Color ResolveHardColor()
        {
            return KickLuckyCubeUiTheme.HardCurrency;
        }

        private sealed class CurrencyFlyTextMotion : MonoBehaviour
        {
            private RectTransform rectTransform;
            private CanvasGroup canvasGroup;
            private Vector2 start;
            private Vector2 burst;
            private Vector2 end;
            private float duration;
            private float delay;
            private float elapsed;
            private float startScale;
            private bool started;

            public void Initialize(
                RectTransform targetRect,
                CanvasGroup targetGroup,
                Vector2 startPosition,
                Vector2 burstPosition,
                Vector2 endPosition,
                float animationDuration,
                float startDelay,
                float initialScale)
            {
                rectTransform = targetRect;
                canvasGroup = targetGroup;
                start = startPosition;
                burst = burstPosition;
                end = endPosition;
                duration = Mathf.Max(0.12f, animationDuration);
                delay = Mathf.Max(0f, startDelay);
                startScale = Mathf.Max(0.1f, initialScale);
                started = delay <= 0f;
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = started ? 1f : 0f;
                }

                Apply(0f);
            }

            private void Update()
            {
                if (rectTransform == null)
                {
                    Destroy(gameObject);
                    return;
                }

                var deltaTime = Time.unscaledDeltaTime;
                if (!started)
                {
                    delay -= deltaTime;
                    if (delay > 0f)
                    {
                        return;
                    }

                    started = true;
                    if (canvasGroup != null)
                    {
                        canvasGroup.alpha = 1f;
                    }
                }

                elapsed += deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                Apply(t);
                if (t >= 1f)
                {
                    Destroy(gameObject);
                }
            }

            private void Apply(float t)
            {
                var eased = Mathf.SmoothStep(0f, 1f, t);
                rectTransform.anchoredPosition = t < 0.26f
                    ? Vector2.Lerp(start, burst, Mathf.SmoothStep(0f, 1f, t / 0.26f))
                    : Vector2.Lerp(burst, end, Mathf.SmoothStep(0f, 1f, (t - 0.26f) / 0.74f));
                rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 0.54f, eased);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01((t - 0.72f) / 0.28f));
                }
            }
        }
    }
}
