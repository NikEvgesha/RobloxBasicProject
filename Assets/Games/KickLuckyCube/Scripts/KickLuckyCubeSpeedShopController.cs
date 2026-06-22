using System;
using System.Reflection;
using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeSpeedShopController : MonoBehaviour
    {
        private const BindingFlags SerializedFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly int[] PurchaseLevelCounts = { 1, 5, 10 };

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private Canvas canvas;
        [SerializeField] private string kioskStandPadName = "KLC_Kiosk_03_SpeedUpgrade_StandPad";
        [SerializeField] private bool autoAttachKioskPad = true;
        [SerializeField] private bool createRuntimeOpenButton = true;
        [SerializeField, Min(0)] private int baseSpeedCost = 60;
        [SerializeField, Min(0)] private int speedCostStep = 55;
        [SerializeField, Min(0.1f)] private float speedGain = 0.8f;
        [SerializeField] private Color nextColor = KickLuckyCubeUiTheme.Primary;
        [SerializeField] private Color lockedColor = KickLuckyCubeUiTheme.CardDark;

        private RectTransform windowRoot;
        private Text statusText;
        private Text summaryText;
        private Font uiFont;
        private RectTransform[] purchaseCards = Array.Empty<RectTransform>();
        private Button[] purchaseButtons = Array.Empty<Button>();
        private Text[] purchaseTitleTexts = Array.Empty<Text>();
        private Text[] purchaseDetailTexts = Array.Empty<Text>();
        private Text[] purchaseButtonTexts = Array.Empty<Text>();
        private Image[] purchaseFrames = Array.Empty<Image>();

        public bool IsOpen => windowRoot != null && windowRoot.gameObject.activeSelf;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeSpeedShopController>(FindObjectsInactive.Include) != null
                || FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include) == null)
            {
                return;
            }

            new GameObject("KLC_SpeedShop_Runtime").AddComponent<KickLuckyCubeSpeedShopController>();
        }

        private void Awake()
        {
            ResolveReferences();
            BuildRuntimeUi();
            Subscribe();
            ConfigureRuntimeKioskPad();
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
            ResolveReferences();
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

        public bool BuyNextLevel()
        {
            return BuyLevels(1);
        }

        public bool BuyLevels(int levels)
        {
            ResolveReferences();
            if (wallet == null || stats == null)
            {
                SetStatus("Speed shop is not ready.");
                return false;
            }

            var normalizedLevels = Mathf.Max(1, levels);
            var totalCost = GetTotalCostForNextLevels(normalizedLevels);
            if (totalCost > int.MaxValue || wallet.SoftCurrency < totalCost)
            {
                SetStatus($"No money for +{normalizedLevels} speed levels.");
                Refresh();
                return false;
            }

            if (!wallet.TrySpendSoft((int)totalCost))
            {
                SetStatus($"No money for +{normalizedLevels} speed levels.");
                Refresh();
                return false;
            }

            stats.AddAnimalSpeedLevels(normalizedLevels, speedGain);
            SetStatus($"Speed +{normalizedLevels} levels. Lv {stats.SpeedUpgradeLevel}, speed {stats.AnimalSpeed:0.0}.");
            Refresh();
            return true;
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            canvas ??= FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            uiFont = KickLuckyCubeUiTheme.Font;
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
            if (createRuntimeOpenButton)
            {
                CreateOpenButton(canvasTransform);
            }

            windowRoot = CreateRect("KLC_SpeedShopWindow_Runtime", canvasTransform);
            windowRoot.anchorMin = new Vector2(0.5f, 0.5f);
            windowRoot.anchorMax = new Vector2(0.5f, 0.5f);
            windowRoot.pivot = new Vector2(0.5f, 0.5f);
            windowRoot.anchoredPosition = new Vector2(0f, 16f);
            windowRoot.sizeDelta = new Vector2(780f, 360f);
            AddImage(windowRoot.gameObject, new Color(0.035f, 0.04f, 0.055f, 0.92f));

            CreateLabel(windowRoot, "Title", "Speed Upgrades", 34, TextAnchor.MiddleLeft, new Vector2(430f, 46f), new Vector2(-160f, 146f));
            summaryText = CreateLabel(windowRoot, "Summary", string.Empty, 18, TextAnchor.MiddleRight, new Vector2(300f, 44f), new Vector2(170f, 146f));
            statusText = CreateLabel(windowRoot, "Status", "Buy speed levels for your returned mobs.", 18, TextAnchor.MiddleLeft, new Vector2(590f, 28f), new Vector2(-55f, -152f));

            var closeButton = CreateButton(windowRoot, "Close", "X", new Vector2(44f, 36f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(352f, 146f);
            closeButton.onClick.AddListener(CloseWindow);

            var grid = CreateRect("SpeedPurchaseOptions", windowRoot);
            grid.anchorMin = new Vector2(0.5f, 0.5f);
            grid.anchorMax = new Vector2(0.5f, 0.5f);
            grid.pivot = new Vector2(0.5f, 0.5f);
            grid.anchoredPosition = new Vector2(0f, -12f);
            grid.sizeDelta = new Vector2(700f, 190f);

            var layout = grid.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            purchaseCards = new RectTransform[PurchaseLevelCounts.Length];
            purchaseButtons = new Button[PurchaseLevelCounts.Length];
            purchaseTitleTexts = new Text[PurchaseLevelCounts.Length];
            purchaseDetailTexts = new Text[PurchaseLevelCounts.Length];
            purchaseButtonTexts = new Text[PurchaseLevelCounts.Length];
            purchaseFrames = new Image[PurchaseLevelCounts.Length];

            for (var index = 0; index < PurchaseLevelCounts.Length; index++)
            {
                CreatePurchaseCard(grid, index, PurchaseLevelCounts[index]);
            }
        }

        private void CreateOpenButton(Transform parent)
        {
            var button = CreateButton(parent as RectTransform, "KLC_SpeedShopOpenButton", "Speed\nY", new Vector2(72f, 76f));
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(438f, 18f);
            button.onClick.AddListener(ToggleWindow);
        }

        private void CreatePurchaseCard(RectTransform parent, int index, int levels)
        {
            var card = CreateRect("SpeedUpgradePlus_" + levels, parent);
            card.sizeDelta = new Vector2(208f, 178f);
            purchaseCards[index] = card;
            purchaseFrames[index] = AddImage(card.gameObject, lockedColor);
            var cardButton = card.gameObject.AddComponent<Button>();
            cardButton.targetGraphic = purchaseFrames[index];
            KickLuckyCubeUiTheme.StyleButton(cardButton, "SpeedUpgradeButton");
            cardButton.onClick.AddListener(() => BuyLevels(levels));
            purchaseButtons[index] = cardButton;

            purchaseTitleTexts[index] = CreateLabel(card, "Title", string.Empty, 23, TextAnchor.MiddleCenter, new Vector2(190f, 34f), new Vector2(0f, 56f));
            purchaseDetailTexts[index] = CreateLabel(card, "Detail", string.Empty, 16, TextAnchor.MiddleCenter, new Vector2(190f, 66f), new Vector2(0f, 10f));
            var actionPlate = CreateRect("ActionLabel", card);
            actionPlate.sizeDelta = new Vector2(164f, 40f);
            actionPlate.anchoredPosition = new Vector2(0f, -58f);
            AddImage(actionPlate.gameObject, new Color(0.08f, 0.08f, 0.08f, 0.88f)).raycastTarget = false;
            purchaseButtonTexts[index] = CreateLabel(actionPlate, "Label", string.Empty, 14, TextAnchor.MiddleCenter, actionPlate.sizeDelta, Vector2.zero);
        }

        private void Refresh()
        {
            if (stats == null || purchaseCards == null)
            {
                return;
            }

            if (summaryText != null)
            {
                summaryText.text = $"Current: {stats.AnimalSpeed:0.0}\nLv {stats.SpeedUpgradeLevel}";
            }

            var visibleOptions = 0;
            for (var index = 0; index < PurchaseLevelCounts.Length; index++)
            {
                var levels = PurchaseLevelCounts[index];
                var totalCost = GetTotalCostForNextLevels(levels);
                var canAfford = wallet != null && totalCost <= wallet.SoftCurrency;
                var isRequiredBaseOption = index == 0;
                if (purchaseCards[index] != null)
                {
                    purchaseCards[index].gameObject.SetActive(isRequiredBaseOption || canAfford);
                }

                if (!canAfford && !isRequiredBaseOption)
                {
                    if (purchaseButtons[index] != null)
                    {
                        purchaseButtons[index].interactable = false;
                    }

                    continue;
                }

                visibleOptions++;
                if (purchaseTitleTexts[index] != null)
                {
                    purchaseTitleTexts[index].text = "+" + levels + (levels == 1 ? " Level" : " Levels");
                }

                if (purchaseDetailTexts[index] != null)
                {
                    purchaseDetailTexts[index].text = $"+{speedGain * levels:0.0} speed";
                }

                if (purchaseButtonTexts[index] != null)
                {
                    purchaseButtonTexts[index].text = canAfford
                        ? $"Buy {FormatCost(totalCost)}"
                        : "No money";
                }

                if (purchaseButtons[index] != null)
                {
                    purchaseButtons[index].interactable = canAfford;
                }

                if (purchaseFrames[index] != null)
                {
                    purchaseFrames[index].color = canAfford ? nextColor : lockedColor;
                }
            }

            if (visibleOptions == 0)
            {
                SetStatus("No money for the next speed level.");
            }
        }

        private void ConfigureRuntimeKioskPad()
        {
            if (!autoAttachKioskPad || string.IsNullOrWhiteSpace(kioskStandPadName))
            {
                return;
            }

            var padObject = GameObject.Find(kioskStandPadName);
            if (padObject == null)
            {
                return;
            }

            ConfigureTriggerCollider(padObject);

            var target = GetOrAddComponent<GameKitInteractionTarget>(padObject);
            ConfigureInteractionTarget(target);

            if (padObject.GetComponent<GameKitInteractionTriggerSource>() == null)
            {
                padObject.AddComponent<GameKitInteractionTriggerSource>();
            }

            var pad = GetOrAddComponent<KickLuckyCubeSpeedShopPad>(padObject);
            pad.Configure(this);
        }

        private static void ConfigureTriggerCollider(GameObject padObject)
        {
            var collider = GetOrAddComponent<BoxCollider>(padObject);
            collider.isTrigger = true;

            var bounds = new Bounds(Vector3.zero, new Vector3(2.6f, 0.25f, 2.0f));
            var meshFilter = padObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                bounds = meshFilter.sharedMesh.bounds;
            }

            var size = bounds.size;
            size.x = Mathf.Max(size.x, 2.4f);
            size.y = 2.8f;
            size.z = Mathf.Max(size.z, 1.8f);
            collider.center = new Vector3(bounds.center.x, bounds.max.y + size.y * 0.5f, bounds.center.z);
            collider.size = size;
        }

        private static void ConfigureInteractionTarget(GameKitInteractionTarget target)
        {
            SetPrivateField(target, "promptKey", "E");
            SetPrivateField(target, "promptText", "Open speed shop");
            SetPrivateField(target, "activationMode", GameKitInteractionActivationMode.Press);
            SetPrivateField(target, "holdSeconds", 0.05f);
            SetPrivateField(target, "priority", 30);
            SetPrivateField(target, "interactable", true);
        }

        private static void SetPrivateField<T>(GameKitInteractionTarget target, string fieldName, T value)
        {
            var field = typeof(GameKitInteractionTarget).GetField(fieldName, SerializedFieldFlags);
            field?.SetValue(target, value);
        }

        private static T GetOrAddComponent<T>(GameObject target)
            where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private long GetLevelCost(int level)
        {
            return Math.Max(0L, baseSpeedCost + Math.Max(0, level - 1) * (long)speedCostStep);
        }

        private long GetTotalCostForNextLevels(int levels)
        {
            var total = 0L;
            var currentLevel = stats != null ? stats.SpeedUpgradeLevel : 0;
            for (var offset = 1; offset <= Mathf.Max(1, levels); offset++)
            {
                total += GetLevelCost(currentLevel + offset);
            }

            return total;
        }

        private static string FormatCost(long cost)
        {
            if (cost >= 1000000L)
            {
                return (cost / 1000000f).ToString("0.0M");
            }

            if (cost >= 1000L)
            {
                return (cost / 1000f).ToString("0.0K");
            }

            return cost.ToString();
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
            return keyboard != null && keyboard.yKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Y);
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
    }
}
