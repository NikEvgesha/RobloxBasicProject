using System;
using System.Linq;
using System.Reflection;
using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStrengthToolShopController : MonoBehaviour
    {
        private const BindingFlags SerializedFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly string[] DefaultToolNames =
        {
            "Training Dumbbell",
            "Iron Kettlebell",
            "Heavy Barbell",
            "Gold Barbell",
            "Power Trainer"
        };

        private static readonly float[] DefaultStrengthPerSecond = { 8f, 14f, 24f, 40f, 66f };

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeToolTrainingController toolTraining;
        [SerializeField] private Canvas canvas;
        [SerializeField] private string[] toolNames = DefaultToolNames;
        [SerializeField] private float[] strengthPerSecondByTier = DefaultStrengthPerSecond;
        [SerializeField, Min(0)] private int baseToolCost = 90;
        [SerializeField, Min(1f)] private float toolCostMultiplier = 2f;
        [SerializeField, Min(1)] private int maxToolTier = 5;
        [SerializeField] private Color selectedColor = KickLuckyCubeUiTheme.Warning;
        [SerializeField] private Color ownedColor = KickLuckyCubeUiTheme.Secondary;
        [SerializeField] private Color buyColor = KickLuckyCubeUiTheme.Primary;
        [SerializeField] private Color lockedColor = KickLuckyCubeUiTheme.CardDark;
        [SerializeField] private string worldPadName = "KLC_Kiosk_04_WeightsTraining_StandPad";
        [SerializeField] private Vector3 worldPadTriggerSize = new(2.4f, 2.4f, 2.0f);

        private RectTransform windowRoot;
        private Text statusText;
        private Font uiFont;
        private Button[] tierButtons = Array.Empty<Button>();
        private Text[] tierNameTexts = Array.Empty<Text>();
        private Text[] tierDetailTexts = Array.Empty<Text>();
        private Text[] tierButtonTexts = Array.Empty<Text>();
        private Image[] tierFrames = Array.Empty<Image>();

        public bool IsOpen => windowRoot != null && windowRoot.gameObject.activeSelf;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeStrengthToolShopController>(FindObjectsInactive.Include) != null
                || FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include) == null)
            {
                return;
            }

            new GameObject("KLC_StrengthToolShop_Runtime").AddComponent<KickLuckyCubeStrengthToolShopController>();
        }

        private void Awake()
        {
            ResolveReferences();
            BuildRuntimeUi();
            ConfigureWorldPad();
            Subscribe();
            CloseWindow();
            Refresh();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (ReadTogglePressed())
            {
                ToggleWindow();
            }

            if (IsOpen && ReadClosePressed())
            {
                CloseWindow();
            }
        }

        public void ToggleWindow()
        {
            if (IsOpen)
            {
                CloseWindow();
            }
            else
            {
                OpenWindow();
            }
        }

        public void OpenWindow()
        {
            if (windowRoot == null)
            {
                BuildRuntimeUi();
            }

            if (windowRoot == null)
            {
                return;
            }

            windowRoot.gameObject.SetActive(true);
            windowRoot.SetAsLastSibling();
            Refresh();
        }

        public void CloseWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.gameObject.SetActive(false);
            }
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            toolTraining ??= FindFirstObjectByType<KickLuckyCubeToolTrainingController>(FindObjectsInactive.Include);
            canvas ??= FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            uiFont = KickLuckyCubeUiTheme.Font;
        }

        private void ConfigureWorldPad()
        {
            if (!Application.isPlaying || string.IsNullOrWhiteSpace(worldPadName))
            {
                return;
            }

            var padObject = GameObject.Find(worldPadName);
            if (padObject == null)
            {
                padObject = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(candidate => string.Equals(candidate.name, worldPadName, StringComparison.Ordinal))
                    ?.gameObject;
            }

            if (padObject == null)
            {
                return;
            }

            var collider = padObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = padObject.AddComponent<BoxCollider>();
            }

            collider.isTrigger = true;
            collider.center = new Vector3(0f, worldPadTriggerSize.y * 0.5f, 0f);
            collider.size = new Vector3(
                Mathf.Max(0.2f, worldPadTriggerSize.x),
                Mathf.Max(0.2f, worldPadTriggerSize.y),
                Mathf.Max(0.2f, worldPadTriggerSize.z));

            var target = padObject.GetComponent<GameKitInteractionTarget>();
            if (target == null)
            {
                target = padObject.AddComponent<GameKitInteractionTarget>();
            }

            target.SetPrompt("E", "Open tools shop");
            SetPrivateField(target, "activationMode", GameKitInteractionActivationMode.Press);
            SetPrivateField(target, "holdSeconds", 0.05f);
            SetPrivateField(target, "priority", 23);
            target.SetInteractable(true);

            if (padObject.GetComponent<GameKitInteractionTriggerSource>() == null)
            {
                padObject.AddComponent<GameKitInteractionTriggerSource>();
            }

            var pad = padObject.GetComponent<KickLuckyCubeStrengthToolShopPad>();
            if (pad == null)
            {
                pad = padObject.AddComponent<KickLuckyCubeStrengthToolShopPad>();
            }

            pad.Configure(this);
        }

        private void Subscribe()
        {
            if (wallet != null)
            {
                wallet.Changed += OnWalletChanged;
            }

            if (stats != null)
            {
                stats.Changed += Refresh;
            }
        }

        private void Unsubscribe()
        {
            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
            }

            if (stats != null)
            {
                stats.Changed -= Refresh;
            }
        }

        private void BuildRuntimeUi()
        {
            if (canvas == null || windowRoot != null)
            {
                return;
            }

            var canvasTransform = canvas.transform;
            CreateOpenButton(canvasTransform);
            windowRoot = CreateRect("KLC_StrengthToolShopWindow_Runtime", canvasTransform);
            windowRoot.anchorMin = new Vector2(0.5f, 0.5f);
            windowRoot.anchorMax = new Vector2(0.5f, 0.5f);
            windowRoot.pivot = new Vector2(0.5f, 0.5f);
            windowRoot.anchoredPosition = new Vector2(0f, 18f);
            windowRoot.sizeDelta = new Vector2(760f, 410f);
            AddImage(windowRoot.gameObject, new Color(0.04f, 0.035f, 0.025f, 0.90f));

            CreateLabel(windowRoot, "Title", "Training Equipment", 34, TextAnchor.MiddleLeft, new Vector2(430f, 46f), new Vector2(-150f, 172f));
            statusText = CreateLabel(windowRoot, "Status", "Buy or equip training tools.", 18, TextAnchor.MiddleLeft, new Vector2(500f, 28f), new Vector2(-110f, -174f));

            var closeButton = CreateButton(windowRoot, "Close", "X", new Vector2(44f, 36f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(342f, 172f);
            closeButton.onClick.AddListener(CloseWindow);

            var grid = CreateRect("ToolCards", windowRoot);
            grid.anchorMin = new Vector2(0.5f, 0.5f);
            grid.anchorMax = new Vector2(0.5f, 0.5f);
            grid.pivot = new Vector2(0.5f, 0.5f);
            grid.anchoredPosition = new Vector2(0f, -10f);
            grid.sizeDelta = new Vector2(690f, 270f);

            var layout = grid.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 12f;

            var tierCount = Mathf.Max(1, maxToolTier);
            tierButtons = new Button[tierCount];
            tierNameTexts = new Text[tierCount];
            tierDetailTexts = new Text[tierCount];
            tierButtonTexts = new Text[tierCount];
            tierFrames = new Image[tierCount];

            for (var tier = 1; tier <= tierCount; tier++)
            {
                CreateToolCard(grid, tier);
            }
        }

        private void CreateOpenButton(Transform parent)
        {
            var button = CreateButton(parent as RectTransform, "KLC_StrengthToolShopOpenButton", "Tools\nT", new Vector2(72f, 76f));
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(350f, 18f);
            button.onClick.AddListener(ToggleWindow);
        }

        private void CreateToolCard(RectTransform parent, int tier)
        {
            var card = CreateRect("ToolTier_" + tier, parent);
            card.sizeDelta = new Vector2(126f, 246f);
            var layoutElement = card.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 126f;
            layoutElement.preferredHeight = 246f;
            tierFrames[tier - 1] = AddImage(card.gameObject, lockedColor);

            var icon = CreateRect("Icon", card);
            icon.anchoredPosition = new Vector2(0f, 58f);
            icon.sizeDelta = new Vector2(72f, 58f);
            AddImage(icon.gameObject, ColorForTier(tier));

            tierNameTexts[tier - 1] = CreateLabel(card, "Name", GetToolName(tier), 14, TextAnchor.MiddleCenter, new Vector2(116f, 42f), new Vector2(0f, 8f));
            tierDetailTexts[tier - 1] = CreateLabel(card, "Detail", string.Empty, 13, TextAnchor.MiddleCenter, new Vector2(116f, 54f), new Vector2(0f, -42f));
            var actionButton = CreateButton(card, "Action", string.Empty, new Vector2(104f, 36f));
            actionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -94f);
            var capturedTier = tier;
            actionButton.onClick.AddListener(() => HandleTierPressed(capturedTier));
            tierButtons[tier - 1] = actionButton;
            tierButtonTexts[tier - 1] = actionButton.GetComponentInChildren<Text>();
        }

        private void HandleTierPressed(int tier)
        {
            ResolveReferences();
            if (stats == null)
            {
                SetStatus("Stats are not ready.");
                return;
            }

            var ownedTier = stats.StrengthToolTier;
            if (tier <= ownedTier)
            {
                stats.SelectStrengthToolTier(tier);
                SetStatus($"Equipped {GetToolName(tier)}.");
                Refresh();
                return;
            }

            if (tier != ownedTier + 1)
            {
                SetStatus("Unlock previous tool first.");
                return;
            }

            var cost = GetToolCost(tier);
            if (wallet == null || !wallet.TrySpendSoft(cost))
            {
                SetStatus($"Need {cost} soft for {GetToolName(tier)}.");
                Refresh();
                return;
            }

            stats.SetStrengthToolTier(tier);
            stats.SelectStrengthToolTier(tier);
            SetStatus($"Unlocked {GetToolName(tier)}.");
            Refresh();
        }

        private void Refresh()
        {
            if (stats == null || tierButtons == null)
            {
                return;
            }

            var ownedTier = stats.StrengthToolTier;
            var selectedTier = stats.SelectedStrengthToolTier;
            for (var tier = 1; tier <= tierButtons.Length; tier++)
            {
                var button = tierButtons[tier - 1];
                var detailText = tierDetailTexts[tier - 1];
                var actionText = tierButtonTexts[tier - 1];
                var frame = tierFrames[tier - 1];
                var owned = tier <= ownedTier;
                var selected = tier == selectedTier;
                var nextToBuy = tier == ownedTier + 1;
                var cost = GetToolCost(tier);
                var canBuy = wallet != null && wallet.SoftCurrency >= cost && nextToBuy;

                if (detailText != null)
                {
                    detailText.text = $"+{GetStrengthPerSecond(tier):0}/s\nTier {tier}";
                }

                if (actionText != null)
                {
                    actionText.text = selected ? "Selected" : owned ? "Equip" : nextToBuy ? $"Buy ${cost}" : "Locked";
                }

                if (button != null)
                {
                    button.interactable = !selected && (owned || nextToBuy);
                }

                if (frame != null)
                {
                    frame.color = selected ? selectedColor : owned ? ownedColor : canBuy ? buyColor : lockedColor;
                }
            }
        }

        private int GetToolCost(int tier)
        {
            return tier <= 1 ? 0 : Mathf.RoundToInt(baseToolCost * Mathf.Pow(toolCostMultiplier, tier - 2));
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

            return DefaultStrengthPerSecond[Mathf.Clamp(tier, 1, DefaultStrengthPerSecond.Length) - 1];
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

            return DefaultToolNames[Mathf.Clamp(tier, 1, DefaultToolNames.Length) - 1];
        }

        private Color ColorForTier(int tier)
        {
            return Color.Lerp(new Color(0.58f, 0.64f, 0.78f, 1f), new Color(1f, 0.75f, 0.15f, 1f), Mathf.InverseLerp(1f, Mathf.Max(1f, maxToolTier), tier));
        }

        private void OnWalletChanged(int soft, int hard)
        {
            Refresh();
        }

        private void SetStatus(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }

        private RectTransform CreateRect(string name, Transform parent)
        {
            var rectObject = new GameObject(name, typeof(RectTransform));
            var rect = rectObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        private Button CreateButton(RectTransform parent, string name, string label, Vector2 size)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = size;
            var image = AddImage(rect.gameObject, new Color(0.08f, 0.08f, 0.08f, 0.88f));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            KickLuckyCubeUiTheme.StyleButton(button, name);
            CreateLabel(rect, "Label", label, 14, TextAnchor.MiddleCenter, size, Vector2.zero);
            return button;
        }

        private Text CreateLabel(RectTransform parent, string name, string text, int fontSize, TextAnchor anchor, Vector2 size, Vector2 position)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var label = rect.gameObject.AddComponent<Text>();
            label.font = uiFont;
            label.fontSize = fontSize;
            label.fontStyle = FontStyle.Bold;
            label.alignment = anchor;
            label.color = Color.white;
            label.text = text;
            label.raycastTarget = false;
            KickLuckyCubeUiTheme.StyleText(label, name);
            return label;
        }

        private static Image AddImage(GameObject target, Color color)
        {
            return KickLuckyCubeUiTheme.AddImage(target, color);
        }

        private static bool ReadTogglePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.tKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.T);
#endif
        }

        private static bool ReadClosePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        private static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            var field = typeof(TTarget).GetField(fieldName, SerializedFieldFlags);
            field?.SetValue(target, value);
        }
    }

    [RequireComponent(typeof(GameKitInteractionTarget))]
    public sealed class KickLuckyCubeStrengthToolShopPad : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeStrengthToolShopController toolShop;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private GameKitInteractionDriver driver;

        private GameKitInteractionTarget interactionTarget;

        private void Awake()
        {
            ResolveReferences();
            interactionTarget = GetComponent<GameKitInteractionTarget>();
            interactionTarget.ActorInteracted.AddListener(OpenShop);
        }

        private void OnDestroy()
        {
            if (interactionTarget != null)
            {
                interactionTarget.ActorInteracted.RemoveListener(OpenShop);
            }
        }

        private void OnDisable()
        {
            CloseShop();
        }

        private void OnTriggerExit(Collider other)
        {
            driver ??= FindFirstObjectByType<GameKitInteractionDriver>();
            if (driver == null || !driver.IsActorCollider(other))
            {
                return;
            }

            CloseShop();
        }

        public void Configure(KickLuckyCubeStrengthToolShopController controller)
        {
            toolShop = controller;
            ResolveReferences();
        }

        public bool CanInteract(GameObject actor)
        {
            ResolveReferences();
            return toolShop != null
                && (runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal && !runPhase.IsSelectingAnimal));
        }

        private void OpenShop(GameObject actor)
        {
            ResolveReferences();
            toolShop?.OpenWindow();
        }

        private void CloseShop()
        {
            toolShop ??= FindFirstObjectByType<KickLuckyCubeStrengthToolShopController>(FindObjectsInactive.Include);
            toolShop?.CloseWindow();
        }

        private void ResolveReferences()
        {
            toolShop ??= FindFirstObjectByType<KickLuckyCubeStrengthToolShopController>(FindObjectsInactive.Include);
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            driver ??= FindFirstObjectByType<GameKitInteractionDriver>();
        }
    }
}
