using System;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeToolTrainingController : MonoBehaviour
    {
        private static readonly string[] DefaultToolNames =
        {
            "Stone Dumbbells",
            "Iron Barbell",
            "Steel Dumbbells",
            "Gold Barbell",
            "Titanium Dumbbells"
        };

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeTrainingBonusPrompt trainingBonusPrompt;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private KickLuckyCubeBalanceConfig balanceConfig;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private string toolPreviewResourcePath = "KickLuckyCube/World/KLC_StrengthToolHandPreview";
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform playerVisual;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private string carryAnchorName = "KLC_CarryAnchor";
        [SerializeField] private string playerRootName = "KLC_PrototypePlayer";
        [SerializeField] private string[] toolNames = DefaultToolNames;
        [SerializeField] private float[] strengthPerSecondByTier = { 8f, 14f, 24f, 40f, 66f };
        [SerializeField, Min(0.1f)] private float baseStrengthPerSecond = 8f;
        [SerializeField, Min(1f)] private float strengthGainMultiplier = 1.65f;
        [SerializeField, Min(0.5f)] private float bonusIntervalSeconds = 5f;
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
        private float strengthTickTimer;
        private float bonusTimer;
        private int displayedToolTier;
        private int displayedAnimationTier;
        private Vector3 trainingStartPosition;
        private RectTransform strengthFlyLayer;
        private bool hasPlayerVisualDefaults;
        private bool hasTrainingStartPosition;
        private bool subscribedToBonusPrompt;

        public event Action Changed;

        public bool IsTraining { get; private set; }
        public int CurrentToolTier => stats != null ? stats.SelectedStrengthToolTier : 1;
        public string CurrentToolName => GetToolName(CurrentToolTier);

        public float CurrentStrengthPerSecond => GetStrengthPerSecond(CurrentToolTier);
        public string ActiveTrainingAnimationState { get; private set; } = string.Empty;
        public bool UsesAuthoredToolVisual { get; private set; }
        public int ActiveToolRendererCount { get; private set; }

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
            PlayTrainingAnimation();
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
            PlayTrainingAnimation(true);
            Changed?.Invoke();
            return true;
        }

        public void StopTraining()
        {
            if (!IsTraining && toolPreview == null)
            {
                StopTrainingAnimation();
                RestorePlayerVisual();
                return;
            }

            IsTraining = false;
            strengthTickTimer = 0f;
            bonusTimer = 0f;
            hasTrainingStartPosition = false;
            trainingBonusPrompt?.HideImmediate();
            StopTrainingAnimation();
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
            balanceConfig ??= KickLuckyCubeBalanceConfig.GetOrLoadDefault();
            ResolveFloatingTextCanvas();
            trainingBonusPrompt ??= CreateTrainingBonusPrompt();
            SubscribeTrainingBonusPrompt();

            if (carryAnchor == null)
            {
                var anchorObject = GameObject.Find(carryAnchorName);
                carryAnchor = anchorObject != null ? anchorObject.transform : null;
            }

            var importedVisualObject = GameObject.Find("KLC_PlayerMannequin")
                ?? GameObject.Find("KLC_StealBrainrotPlayerVisual");
            if (importedVisualObject != null && playerVisual != importedVisualObject.transform)
            {
                playerVisual = importedVisualObject.transform;
                hasPlayerVisualDefaults = false;
                playerAnimator = null;
            }
            else if (playerVisual == null)
            {
                var visualObject = GameObject.Find("KLC_PlayerVisual");
                playerVisual = visualObject != null ? visualObject.transform : null;
            }

            if (playerAnimator == null && playerVisual != null)
            {
                playerAnimator = playerVisual.GetComponentInChildren<Animator>(true);
            }

            if (playerRoot == null)
            {
                var rootObject = GameObject.Find(playerRootName);
                playerRoot = rootObject != null ? rootObject.transform : null;
            }
        }

        private KickLuckyCubeTrainingBonusPrompt CreateTrainingBonusPrompt()
        {
            if (floatingTextCanvas == null || !Application.isPlaying)
            {
                return null;
            }

            var canvasRect = floatingTextCanvas.transform as RectTransform;
            if (canvasRect == null)
            {
                return null;
            }

            var promptRect = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_TrainingBonusPrompt", canvasRect);
            promptRect.anchorMin = new Vector2(0.5f, 0.5f);
            promptRect.anchorMax = new Vector2(0.5f, 0.5f);
            promptRect.pivot = new Vector2(0.5f, 0.5f);
            promptRect.anchoredPosition = new Vector2(260f, 90f);
            promptRect.sizeDelta = new Vector2(86f, 86f);

            return KickLuckyCubeUiPrefabFactory.GetOrAddComponent<KickLuckyCubeTrainingBonusPrompt>(promptRect.gameObject);
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

        private void PlayTrainingAnimation(bool force = false)
        {
            if (!IsTraining || playerAnimator == null || playerAnimator.runtimeAnimatorController == null)
            {
                return;
            }

            var tier = CurrentToolTier;
            var state = tier % 2 != 0 ? "DumbbellTraining" : "BarbellTraining";
            if (!force && displayedAnimationTier == tier && string.Equals(ActiveTrainingAnimationState, state, StringComparison.Ordinal))
            {
                return;
            }

            displayedAnimationTier = tier;
            ActiveTrainingAnimationState = state;
            playerAnimator.speed = 1f;
            playerAnimator.CrossFadeInFixedTime(state, 0.12f, 0, 0f);
        }

        private void StopTrainingAnimation()
        {
            displayedAnimationTier = 0;
            ActiveTrainingAnimationState = string.Empty;
            if (playerAnimator != null && playerAnimator.runtimeAnimatorController != null)
            {
                playerAnimator.speed = 1f;
                playerAnimator.CrossFadeInFixedTime("Idle", 0.12f, 0, 0f);
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
            if (playerVisual == null || stats == null)
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
            var previewPrefab = Resources.Load<KickLuckyCubeToolPreviewVisual>(toolPreviewResourcePath);
            if (previewPrefab == null)
            {
                Debug.LogError($"[KLC-TRAINING] Missing authored tool preview prefab at Resources/{toolPreviewResourcePath}.", this);
                return;
            }

            var previewVisual = Instantiate(previewPrefab, playerVisual, false);
            toolPreview = previewVisual.gameObject;
            toolPreview.name = "KLC_SelectedToolHandPreview_" + CurrentToolName.Replace(" ", string.Empty);
            toolPreview.transform.localPosition = Vector3.zero;
            toolPreview.transform.localRotation = Quaternion.identity;
            toolPreview.transform.localScale = Vector3.one;
            var maxTier = ResolveBalanceConfig() != null ? ResolveBalanceConfig().MaxToolTier : 5;
            previewVisual.Configure(selectedTier, maxTier, playerVisual);
            UsesAuthoredToolVisual = previewVisual.UsesAuthoredModel;
            ActiveToolRendererCount = previewVisual.ActiveRendererCount;
            PlayTrainingAnimation(true);
        }

        private void DestroyToolPreview()
        {
            displayedToolTier = 0;
            UsesAuthoredToolVisual = false;
            ActiveToolRendererCount = 0;
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
            var balance = ResolveBalanceConfig();
            if (balance != null)
            {
                return balance.GetToolStrengthPerSecond(tier);
            }

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
            var balance = ResolveBalanceConfig();
            if (balance != null)
            {
                return balance.GetToolName(tier);
            }

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

        private KickLuckyCubeBalanceConfig ResolveBalanceConfig()
        {
            balanceConfig ??= KickLuckyCubeBalanceConfig.GetOrLoadDefault();
            return balanceConfig;
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
            ResolveFloatingTextCanvas(true);
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

            strengthFlyLayer = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_StrengthFlyTextLayer", canvasRect);
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
            var rectTransform = KickLuckyCubeUiPrefabFactory.CreateRectInstance("KLC_StrengthGainFlyText", "KLC_StrengthGainFlyText", layer);
            var textObject = rectTransform.gameObject;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = burstIndex == 0 ? new Vector2(170f, 52f) : new Vector2(86f, 34f);

            var text = KickLuckyCubeUiPrefabFactory.GetRequiredComponent<Text>(textObject);
            text.text = burstIndex == 0 ? $"+{amount:0}" : "+";
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = burstIndex == 0 ? 46 : 30;
            KickLuckyCubeUiTheme.StyleFloatingText(
                text,
                burstIndex == 0 ? KickLuckyCubeUiTheme.SoftCurrency : KickLuckyCubeUiTheme.Strength);

            var group = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<CanvasGroup>(textObject);
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
            var motion = textObject.GetComponent<StrengthFlyTextMotion>()
                ?? textObject.AddComponent<StrengthFlyTextMotion>();
            motion.Initialize(rectTransform, group, start, pop, end, duration, delay, startScale);
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

        private void ResolveFloatingTextCanvas(bool preferStrengthTarget = false)
        {
            var target = ResolveStrengthFlyTarget();
            if (target != null)
            {
                var targetCanvas = target.GetComponentInParent<Canvas>();
                if (targetCanvas != null
                    && (preferStrengthTarget
                        || floatingTextCanvas == null
                        || !floatingTextCanvas.gameObject.activeInHierarchy))
                {
                    floatingTextCanvas = targetCanvas;
                    strengthFlyLayer = null;
                    return;
                }
            }

            if (floatingTextCanvas == null || !floatingTextCanvas.gameObject.activeInHierarchy)
            {
                floatingTextCanvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
                strengthFlyLayer = null;
            }
        }

        private RectTransform ResolveStrengthFlyTarget()
        {
            if (strengthFlyTarget != null)
            {
                return strengthFlyTarget;
            }

            var targetObject = GameObject.Find("KLC_BottomLeftStrengthValue")
                ?? GameObject.Find("KLC_BottomLeftStatsVisual")
                ?? GameObject.Find("KLC_KickHud");
            strengthFlyTarget = targetObject != null ? targetObject.GetComponent<RectTransform>() : null;
            return strengthFlyTarget;
        }

        private Vector2 ResolveStrengthFlyEnd(RectTransform canvasRect)
        {
            ResolveStrengthFlyTarget();

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
