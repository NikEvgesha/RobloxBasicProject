using System;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeToolTrainingController : MonoBehaviour
    {
        private static readonly string[] DefaultToolNames =
        {
            "Training Dumbbell",
            "Iron Kettlebell",
            "Heavy Barbell",
            "Gold Barbell",
            "Power Trainer"
        };

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeTrainingBonusPrompt trainingBonusPrompt;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform playerVisual;
        [SerializeField] private string carryAnchorName = "KLC_CarryAnchor";
        [SerializeField] private string playerRootName = "KLC_PrototypePlayer";
        [SerializeField] private string[] toolNames = DefaultToolNames;
        [SerializeField] private float[] strengthPerSecondByTier = { 8f, 14f, 24f, 40f, 66f };
        [SerializeField, Min(0.1f)] private float baseStrengthPerSecond = 8f;
        [SerializeField, Min(1f)] private float strengthGainMultiplier = 1.65f;
        [SerializeField, Min(0.5f)] private float bonusIntervalSeconds = 5f;
        [SerializeField, Min(0.1f)] private float squatFrequency = 1.8f;
        [SerializeField, Min(0f)] private float squatDepth = 0.18f;
        [SerializeField, Min(0f)] private float squatYOffset = 0.12f;
        [SerializeField, Min(0f)] private float toolSquatYOffset = 0.16f;
        [SerializeField, Min(0f)] private float toolSwingDegrees = 12f;
        [SerializeField, Min(0.1f)] private float strengthFlySeconds = 0.72f;
        [SerializeField, Min(0f)] private float movementCancelDistance = 0.08f;
        [SerializeField] private Vector3 strengthFlyWorldOffset = new(0f, 2.15f, 0f);
        [SerializeField, Min(0f)] private float strengthFlyWorldScatterRadius = 0.85f;
        [SerializeField, Min(0f)] private float strengthFlyScreenScatter = 28f;
        [SerializeField] private Vector2 fallbackStrengthFlyTarget = new(275f, -62f);
        [SerializeField] private Canvas floatingTextCanvas;
        [SerializeField] private RectTransform strengthFlyTarget;

        private GameObject toolPreview;
        private Vector3 playerVisualDefaultScale;
        private Vector3 playerVisualDefaultPosition;
        private Vector3 toolPreviewDefaultPosition;
        private Quaternion toolPreviewDefaultRotation = Quaternion.identity;
        private float strengthTickTimer;
        private float bonusTimer;
        private int displayedToolTier;
        private Vector3 trainingStartPosition;
        private RectTransform strengthFlyLayer;
        private bool hasPlayerVisualDefaults;
        private bool hasToolPreviewDefaults;
        private bool hasTrainingStartPosition;
        private bool subscribedToBonusPrompt;

        public event Action Changed;

        public bool IsTraining { get; private set; }
        public int CurrentToolTier => stats != null ? stats.SelectedStrengthToolTier : 1;
        public string CurrentToolName => GetToolName(CurrentToolTier);

        public float CurrentStrengthPerSecond => GetStrengthPerSecond(CurrentToolTier);

        private void Awake()
        {
            ResolveReferences();
            SubscribeTrainingBonusPrompt();
            CapturePlayerVisualDefaults();
        }

        private void OnDisable()
        {
            StopTraining();
            UnsubscribeTrainingBonusPrompt();
        }

        private void Update()
        {
            if (!IsTraining)
            {
                return;
            }

            if (!CanTrain())
            {
                StopTraining();
                return;
            }

            if (HasMovedSinceTrainingStarted())
            {
                StopTraining();
                return;
            }

            var deltaTime = Time.unscaledDeltaTime;
            strengthTickTimer += deltaTime;
            bonusTimer += deltaTime;

            while (strengthTickTimer >= 1f)
            {
                strengthTickTimer -= 1f;
                var gain = CurrentStrengthPerSecond;
                GrantStrength(gain);
            }

            if (bonusTimer >= bonusIntervalSeconds)
            {
                bonusTimer = 0f;
                if (trainingBonusPrompt == null || !trainingBonusPrompt.IsVisible)
                {
                    trainingBonusPrompt?.ShowBonus(CurrentStrengthPerSecond * 2f, stats.SelectedStrengthToolTier);
                }
            }

            RefreshToolPreview();
            AnimateSquat();
        }

        public void ToggleTraining()
        {
            if (IsTraining)
            {
                StopTraining();
            }
            else
            {
                StartTraining();
            }
        }

        public bool StartTraining()
        {
            ResolveReferences();
            if (!CanTrain())
            {
                return false;
            }

            IsTraining = true;
            strengthTickTimer = 0f;
            bonusTimer = 0f;
            CaptureTrainingStartPosition();
            CapturePlayerVisualDefaults();
            RefreshToolPreview(true);
            AnimateSquat();
            Changed?.Invoke();
            return true;
        }

        public void StopTraining()
        {
            if (!IsTraining && toolPreview == null)
            {
                RestorePlayerVisual();
                return;
            }

            IsTraining = false;
            strengthTickTimer = 0f;
            bonusTimer = 0f;
            hasTrainingStartPosition = false;
            trainingBonusPrompt?.HideImmediate();
            DestroyToolPreview();
            RestorePlayerVisual();
            Changed?.Invoke();
        }

        private bool CanTrain()
        {
            ResolveReferences();
            return stats != null
                && (runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal && !runPhase.IsSelectingAnimal));
        }

        private void ResolveReferences()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            trainingBonusPrompt ??= FindFirstObjectByType<KickLuckyCubeTrainingBonusPrompt>(FindObjectsInactive.Include);
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            floatingTextCanvas ??= FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            SubscribeTrainingBonusPrompt();

            if (carryAnchor == null)
            {
                var anchorObject = GameObject.Find(carryAnchorName);
                carryAnchor = anchorObject != null ? anchorObject.transform : null;
            }

            if (playerVisual == null)
            {
                var visualObject = GameObject.Find("KLC_PlayerVisual");
                playerVisual = visualObject != null ? visualObject.transform : null;
            }

            if (playerRoot == null)
            {
                var rootObject = GameObject.Find(playerRootName);
                playerRoot = rootObject != null ? rootObject.transform : null;
            }
        }

        private void SubscribeTrainingBonusPrompt()
        {
            if (trainingBonusPrompt == null || subscribedToBonusPrompt)
            {
                return;
            }

            trainingBonusPrompt.Claimed += OnTrainingBonusClaimed;
            subscribedToBonusPrompt = true;
        }

        private void UnsubscribeTrainingBonusPrompt()
        {
            if (trainingBonusPrompt == null || !subscribedToBonusPrompt)
            {
                return;
            }

            trainingBonusPrompt.Claimed -= OnTrainingBonusClaimed;
            subscribedToBonusPrompt = false;
        }

        private void CaptureTrainingStartPosition()
        {
            ResolveReferences();
            if (playerRoot == null)
            {
                hasTrainingStartPosition = false;
                return;
            }

            trainingStartPosition = playerRoot.position;
            hasTrainingStartPosition = true;
        }

        private bool HasMovedSinceTrainingStarted()
        {
            if (!hasTrainingStartPosition || movementCancelDistance <= 0f || playerRoot == null)
            {
                return false;
            }

            var delta = playerRoot.position - trainingStartPosition;
            delta.y = 0f;
            return delta.sqrMagnitude >= movementCancelDistance * movementCancelDistance;
        }

        private void CapturePlayerVisualDefaults()
        {
            if (playerVisual == null || hasPlayerVisualDefaults)
            {
                return;
            }

            playerVisualDefaultScale = playerVisual.localScale;
            playerVisualDefaultPosition = playerVisual.localPosition;
            hasPlayerVisualDefaults = true;
        }

        private void AnimateSquat()
        {
            if (playerVisual == null)
            {
                return;
            }

            CapturePlayerVisualDefaults();
            var squatAmount = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * squatFrequency) + 1f) * 0.5f;
            var horizontalScale = 1f + squatAmount * squatDepth * 0.36f;
            var verticalScale = 1f - squatAmount * squatDepth;
            playerVisual.localScale = new Vector3(
                playerVisualDefaultScale.x * horizontalScale,
                playerVisualDefaultScale.y * verticalScale,
                playerVisualDefaultScale.z * horizontalScale);
            playerVisual.localPosition = playerVisualDefaultPosition + Vector3.down * (squatAmount * squatYOffset);

            if (toolPreview != null && hasToolPreviewDefaults)
            {
                toolPreview.transform.localPosition = toolPreviewDefaultPosition + Vector3.down * (squatAmount * toolSquatYOffset);
                toolPreview.transform.localRotation = toolPreviewDefaultRotation * Quaternion.Euler(squatAmount * toolSwingDegrees, 0f, 0f);
            }
        }

        private void RestorePlayerVisual()
        {
            if (playerVisual == null || !hasPlayerVisualDefaults)
            {
                return;
            }

            playerVisual.localScale = playerVisualDefaultScale;
            playerVisual.localPosition = playerVisualDefaultPosition;
        }

        private void RefreshToolPreview(bool forceRebuild = false)
        {
            ResolveReferences();
            if (carryAnchor == null || stats == null)
            {
                return;
            }

            var selectedTier = CurrentToolTier;
            if (!forceRebuild && toolPreview != null && displayedToolTier == selectedTier)
            {
                return;
            }

            DestroyToolPreview();
            displayedToolTier = selectedTier;
            toolPreview = new GameObject("KLC_SelectedToolHandPreview_" + CurrentToolName.Replace(" ", string.Empty));
            toolPreview.transform.SetParent(carryAnchor, false);
            toolPreview.transform.localPosition = new Vector3(0.08f, -0.02f, 0.02f);
            toolPreview.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            toolPreview.transform.localScale = Vector3.one;
            toolPreviewDefaultPosition = toolPreview.transform.localPosition;
            toolPreviewDefaultRotation = toolPreview.transform.localRotation;
            hasToolPreviewDefaults = true;

            var tier01 = Mathf.InverseLerp(1f, 5f, selectedTier);
            var color = Color.Lerp(new Color(0.62f, 0.66f, 0.74f), new Color(1f, 0.78f, 0.20f), tier01);
            var barLength = 0.42f + selectedTier * 0.045f;
            var barWidth = 0.045f + selectedTier * 0.006f;
            var weightSize = 0.20f + selectedTier * 0.035f;
            var weightWidth = 0.09f + selectedTier * 0.014f;
            CreateToolPart("Bar", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(barWidth, barLength, barWidth), color);
            CreateToolPart("LeftWeight", PrimitiveType.Cube, new Vector3(0f, -barLength - 0.05f, 0f), Quaternion.identity, new Vector3(weightSize, weightWidth, weightSize), color * 0.85f);
            CreateToolPart("RightWeight", PrimitiveType.Cube, new Vector3(0f, barLength + 0.05f, 0f), Quaternion.identity, new Vector3(weightSize, weightWidth, weightSize), color * 0.85f);

            if (selectedTier >= 3)
            {
                CreateToolPart("LeftPlate", PrimitiveType.Cube, new Vector3(0f, -barLength - 0.18f, 0f), Quaternion.identity, new Vector3(weightSize * 0.85f, weightWidth, weightSize * 0.85f), color * 0.72f);
                CreateToolPart("RightPlate", PrimitiveType.Cube, new Vector3(0f, barLength + 0.18f, 0f), Quaternion.identity, new Vector3(weightSize * 0.85f, weightWidth, weightSize * 0.85f), color * 0.72f);
            }
        }

        private void CreateToolPart(
            string partName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Color color)
        {
            var part = GameObject.CreatePrimitive(primitiveType);
            part.name = partName;
            part.transform.SetParent(toolPreview.transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(renderer.sharedMaterial)
                {
                    color = color
                };
                renderer.sharedMaterial = material;
            }
        }

        private void DestroyToolPreview()
        {
            displayedToolTier = 0;
            hasToolPreviewDefaults = false;
            if (toolPreview == null)
            {
                return;
            }

            toolPreview.SetActive(false);
            Destroy(toolPreview);
            toolPreview = null;
        }

        private float GetStrengthPerSecond(int tier)
        {
            if (strengthPerSecondByTier != null)
            {
                var index = Mathf.Clamp(tier, 1, Mathf.Max(1, strengthPerSecondByTier.Length)) - 1;
                if (index >= 0 && index < strengthPerSecondByTier.Length && strengthPerSecondByTier[index] > 0f)
                {
                    return strengthPerSecondByTier[index];
                }
            }

            return baseStrengthPerSecond * Mathf.Pow(strengthGainMultiplier, Mathf.Max(1, tier) - 1);
        }

        private string GetToolName(int tier)
        {
            if (toolNames != null)
            {
                var index = Mathf.Clamp(tier, 1, Mathf.Max(1, toolNames.Length)) - 1;
                if (index >= 0 && index < toolNames.Length && !string.IsNullOrWhiteSpace(toolNames[index]))
                {
                    return toolNames[index];
                }
            }

            var defaultIndex = Mathf.Clamp(tier, 1, DefaultToolNames.Length) - 1;
            return defaultIndex >= 0 && defaultIndex < DefaultToolNames.Length
                ? DefaultToolNames[defaultIndex]
                : $"Tool {tier}";
        }

        private void GrantStrength(float amount)
        {
            if (amount <= 0f || stats == null)
            {
                return;
            }

            stats.AddStrength(amount);
            SpawnStrengthGainFx(amount);
        }

        private void OnTrainingBonusClaimed(float amount)
        {
            SpawnStrengthGainFx(amount);
        }

        public void SpawnStrengthGainFx(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            ResolveReferences();
            if (floatingTextCanvas == null)
            {
                return;
            }

            var canvasRect = floatingTextCanvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return;
            }

            var layer = GetStrengthFlyLayer(canvasRect);
            CreateStrengthFlyText(layer, canvasRect, amount, 0);
            CreateStrengthFlyText(layer, canvasRect, amount, 1);
            CreateStrengthFlyText(layer, canvasRect, amount, 2);
        }

        private RectTransform GetStrengthFlyLayer(RectTransform canvasRect)
        {
            if (strengthFlyLayer != null)
            {
                ClearBrokenFlyTexts(strengthFlyLayer);
                strengthFlyLayer.SetAsLastSibling();
                return strengthFlyLayer;
            }

            ClearLegacyFlyTexts(canvasRect);

            var existingLayer = canvasRect.Find("KLC_StrengthFlyTextLayer") as RectTransform;
            if (existingLayer != null)
            {
                strengthFlyLayer = existingLayer;
                ClearBrokenFlyTexts(strengthFlyLayer);
                strengthFlyLayer.SetAsLastSibling();
                return strengthFlyLayer;
            }

            var layerObject = new GameObject("KLC_StrengthFlyTextLayer", typeof(RectTransform));
            strengthFlyLayer = layerObject.GetComponent<RectTransform>();
            strengthFlyLayer.SetParent(canvasRect, false);
            strengthFlyLayer.anchorMin = Vector2.zero;
            strengthFlyLayer.anchorMax = Vector2.one;
            strengthFlyLayer.offsetMin = Vector2.zero;
            strengthFlyLayer.offsetMax = Vector2.zero;
            strengthFlyLayer.pivot = new Vector2(0.5f, 0.5f);
            strengthFlyLayer.localScale = Vector3.one;
            strengthFlyLayer.SetAsLastSibling();
            return strengthFlyLayer;
        }

        private static void ClearLegacyFlyTexts(RectTransform canvasRect)
        {
            for (var index = canvasRect.childCount - 1; index >= 0; index--)
            {
                var child = canvasRect.GetChild(index);
                if (child != null && child.name.StartsWith("KLC_StrengthGainFlyText", StringComparison.Ordinal))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private static void ClearBrokenFlyTexts(RectTransform parent)
        {
            for (var index = parent.childCount - 1; index >= 0; index--)
            {
                var child = parent.GetChild(index);
                var text = child != null ? child.GetComponent<Text>() : null;
                if (child != null
                    && child.name.StartsWith("KLC_StrengthGainFlyText", StringComparison.Ordinal)
                    && (child.GetComponent<StrengthFlyTextMotion>() == null
                        || (text != null && (text.raycastTarget || text.fontSize <= 14))))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void CreateStrengthFlyText(RectTransform layer, RectTransform canvasRect, float amount, int burstIndex)
        {
            var textObject = new GameObject("KLC_StrengthGainFlyText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(CanvasGroup), typeof(Shadow), typeof(Outline));
            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(layer, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = burstIndex == 0 ? new Vector2(170f, 52f) : new Vector2(86f, 34f);

            var text = textObject.GetComponent<Text>();
            text.text = burstIndex == 0 ? $"+{amount:0}" : "+";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = burstIndex == 0 ? 46 : 30;
            KickLuckyCubeUiTheme.StyleFloatingText(
                text,
                burstIndex == 0 ? KickLuckyCubeUiTheme.SoftCurrency : KickLuckyCubeUiTheme.Strength);

            var group = textObject.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;

            var start = ResolveStrengthFlyStart(canvasRect, burstIndex);
            var end = ResolveStrengthFlyEnd(canvasRect);
            var pop = start + new Vector2(
                (burstIndex - 1) * 46f + UnityEngine.Random.Range(-12f, 12f),
                36f + burstIndex * 10f + UnityEngine.Random.Range(-8f, 10f));
            rectTransform.anchoredPosition = start;

            var delay = burstIndex * 0.055f;
            var duration = burstIndex == 0 ? strengthFlySeconds : strengthFlySeconds * 0.82f;
            var startScale = burstIndex == 0 ? 1.38f : 1.05f;
            textObject.AddComponent<StrengthFlyTextMotion>().Initialize(rectTransform, group, start, pop, end, duration, delay, startScale);
        }

        private Vector2 ResolveStrengthFlyStart(RectTransform canvasRect, int burstIndex)
        {
            var source = playerVisual != null
                ? playerVisual.position
                : carryAnchor != null
                    ? carryAnchor.position
                    : transform.position;
            var worldCamera = Camera.main;
            var worldPosition = source + strengthFlyWorldOffset + ResolveStrengthFlyWorldScatter(worldCamera);
            var cameraForCanvas = floatingTextCanvas != null && floatingTextCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? floatingTextCanvas.worldCamera
                : null;
            var screenPosition = worldCamera != null
                ? RectTransformUtility.WorldToScreenPoint(worldCamera, worldPosition)
                : new Vector2(Screen.width * 0.5f, Screen.height * 0.58f);
            var screenScatter = ResolveStrengthFlyScreenScatter();

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                cameraForCanvas,
                out var localPoint)
                ? localPoint + new Vector2((burstIndex - 1) * 18f, burstIndex * 8f) + screenScatter
                : Vector2.zero;
        }

        private Vector3 ResolveStrengthFlyWorldScatter(Camera worldCamera)
        {
            if (strengthFlyWorldScatterRadius <= 0f)
            {
                return Vector3.zero;
            }

            var right = worldCamera != null ? worldCamera.transform.right : Vector3.right;
            var up = Vector3.up;
            var angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            var radius = UnityEngine.Random.Range(strengthFlyWorldScatterRadius * 0.35f, strengthFlyWorldScatterRadius);
            var horizontal = Mathf.Cos(angle) * radius;
            var vertical = Mathf.Sin(angle) * radius * 0.7f;
            return right * horizontal + up * vertical;
        }

        private Vector2 ResolveStrengthFlyScreenScatter()
        {
            if (strengthFlyScreenScatter <= 0f)
            {
                return Vector2.zero;
            }

            return new Vector2(
                UnityEngine.Random.Range(-strengthFlyScreenScatter, strengthFlyScreenScatter),
                UnityEngine.Random.Range(-strengthFlyScreenScatter * 0.65f, strengthFlyScreenScatter));
        }

        private Vector2 ResolveStrengthFlyEnd(RectTransform canvasRect)
        {
            if (strengthFlyTarget == null)
            {
                var targetObject = GameObject.Find("KLC_KickHud");
                strengthFlyTarget = targetObject != null ? targetObject.GetComponent<RectTransform>() : null;
            }

            if (strengthFlyTarget != null)
            {
                var cameraForCanvas = floatingTextCanvas != null && floatingTextCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? floatingTextCanvas.worldCamera
                    : null;
                var screenPosition = RectTransformUtility.WorldToScreenPoint(cameraForCanvas, strengthFlyTarget.position);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, cameraForCanvas, out var localPoint))
                {
                    return localPoint;
                }
            }

            return fallbackStrengthFlyTarget;
        }

        private sealed class StrengthFlyTextMotion : MonoBehaviour
        {
            private RectTransform rectTransform;
            private CanvasGroup canvasGroup;
            private Vector2 start;
            private Vector2 pop;
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
                Vector2 popPosition,
                Vector2 endPosition,
                float animationDuration,
                float startDelay,
                float initialScale)
            {
                rectTransform = targetRect;
                canvasGroup = targetGroup;
                start = startPosition;
                pop = popPosition;
                end = endPosition;
                duration = Mathf.Max(0.1f, animationDuration);
                delay = Mathf.Max(0f, startDelay);
                startScale = Mathf.Max(0.1f, initialScale);
                started = delay <= 0f;
                Apply(0f);

                if (!started && canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                }
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
                rectTransform.anchoredPosition = t < 0.22f
                    ? Vector2.Lerp(start, pop, Mathf.SmoothStep(0f, 1f, t / 0.22f))
                    : Vector2.Lerp(pop, end, Mathf.SmoothStep(0f, 1f, (t - 0.22f) / 0.78f));
                rectTransform.localScale = Vector3.one * Mathf.Lerp(startScale, 0.58f, eased);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01((t - 0.68f) / 0.32f));
                }
            }
        }
    }
}
