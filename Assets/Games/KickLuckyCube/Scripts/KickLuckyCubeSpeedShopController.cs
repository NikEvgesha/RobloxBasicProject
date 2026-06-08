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
        [SerializeField, Min(0f)] private float baselineAnimalSpeed = 7f;
        [SerializeField, Min(1)] private int maxSpeedLevel = 8;
        [SerializeField] private Color ownedColor = new(0.20f, 0.62f, 0.92f, 0.92f);
        [SerializeField] private Color nextColor = new(0.18f, 0.78f, 0.26f, 0.94f);
        [SerializeField] private Color lockedColor = new(0.18f, 0.18f, 0.20f, 0.86f);
        [SerializeField] private Color maxedColor = new(1f, 0.82f, 0.18f, 0.96f);

        private RectTransform windowRoot;
        private Text statusText;
        private Text summaryText;
        private Font uiFont;
        private Button[] levelButtons = Array.Empty<Button>();
        private Text[] levelTitleTexts = Array.Empty<Text>();
        private Text[] levelDetailTexts = Array.Empty<Text>();
        private Text[] levelButtonTexts = Array.Empty<Text>();
        private Image[] levelFrames = Array.Empty<Image>();

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
            ResolveReferences();
            if (wallet == null || stats == null)
            {
                SetStatus("Speed shop is not ready.");
                return false;
            }

            if (stats.SpeedUpgradeLevel >= maxSpeedLevel)
            {
                SetStatus("Speed is already maxed.");
                Refresh();
                return false;
            }

            var nextLevel = stats.SpeedUpgradeLevel + 1;
            var cost = GetLevelCost(nextLevel);
            if (!wallet.TrySpendSoft(cost))
            {
                SetStatus($"Need {cost} soft for speed Lv {nextLevel}.");
                Refresh();
                return false;
            }

            stats.AddAnimalSpeed(speedGain);
            SetStatus($"Speed upgraded to Lv {stats.SpeedUpgradeLevel}. Run speed {stats.AnimalSpeed:0.0}.");
            Refresh();
            return true;
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            canvas ??= FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
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
            windowRoot.sizeDelta = new Vector2(780f, 460f);
            AddImage(windowRoot.gameObject, new Color(0.035f, 0.04f, 0.055f, 0.92f));

            CreateLabel(windowRoot, "Title", "Speed Upgrades", 34, TextAnchor.MiddleLeft, new Vector2(430f, 46f), new Vector2(-160f, 196f));
            summaryText = CreateLabel(windowRoot, "Summary", string.Empty, 18, TextAnchor.MiddleRight, new Vector2(300f, 44f), new Vector2(170f, 196f));
            statusText = CreateLabel(windowRoot, "Status", "Buy speed levels for your returned mobs.", 18, TextAnchor.MiddleLeft, new Vector2(590f, 28f), new Vector2(-55f, -202f));

            var closeButton = CreateButton(windowRoot, "Close", "X", new Vector2(44f, 36f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(352f, 196f);
            closeButton.onClick.AddListener(CloseWindow);

            var grid = CreateRect("SpeedCards", windowRoot);
            grid.anchorMin = new Vector2(0.5f, 0.5f);
            grid.anchorMax = new Vector2(0.5f, 0.5f);
            grid.pivot = new Vector2(0.5f, 0.5f);
            grid.anchoredPosition = new Vector2(0f, -18f);
            grid.sizeDelta = new Vector2(700f, 330f);

            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(162f, 148f);
            layout.spacing = new Vector2(12f, 12f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;
            layout.childAlignment = TextAnchor.MiddleCenter;

            levelButtons = new Button[maxSpeedLevel];
            levelTitleTexts = new Text[maxSpeedLevel];
            levelDetailTexts = new Text[maxSpeedLevel];
            levelButtonTexts = new Text[maxSpeedLevel];
            levelFrames = new Image[maxSpeedLevel];

            for (var level = 1; level <= maxSpeedLevel; level++)
            {
                CreateSpeedCard(grid, level);
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

        private void CreateSpeedCard(RectTransform parent, int level)
        {
            var card = CreateRect("SpeedLevel_" + level, parent);
            card.sizeDelta = new Vector2(162f, 148f);
            levelFrames[level - 1] = AddImage(card.gameObject, lockedColor);

            levelTitleTexts[level - 1] = CreateLabel(card, "Title", $"Lv {level}", 21, TextAnchor.MiddleCenter, new Vector2(148f, 32f), new Vector2(0f, 48f));
            levelDetailTexts[level - 1] = CreateLabel(card, "Detail", string.Empty, 15, TextAnchor.MiddleCenter, new Vector2(150f, 48f), new Vector2(0f, 4f));
            var actionButton = CreateButton(card, "Action", string.Empty, new Vector2(128f, 34f));
            actionButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -48f);
            actionButton.onClick.AddListener(() => BuyNextLevel());
            levelButtons[level - 1] = actionButton;
            levelButtonTexts[level - 1] = actionButton.GetComponentInChildren<Text>();
        }

        private void Refresh()
        {
            if (stats == null || levelButtons == null)
            {
                return;
            }

            if (summaryText != null)
            {
                summaryText.text = $"Current: {stats.AnimalSpeed:0.0}\nLv {stats.SpeedUpgradeLevel}/{maxSpeedLevel}";
            }

            var ownedLevel = stats.SpeedUpgradeLevel;
            var nextLevel = ownedLevel + 1;
            for (var level = 1; level <= levelButtons.Length; level++)
            {
                var owned = level <= ownedLevel;
                var nextToBuy = level == nextLevel && ownedLevel < maxSpeedLevel;
                var maxed = ownedLevel >= maxSpeedLevel && level == maxSpeedLevel;
                var cost = GetLevelCost(level);
                var canAfford = wallet != null && wallet.SoftCurrency >= cost;

                if (levelDetailTexts[level - 1] != null)
                {
                    levelDetailTexts[level - 1].text = $"Run {GetSpeedAfterLevel(level):0.0}\n+{speedGain:0.0} speed";
                }

                if (levelButtonTexts[level - 1] != null)
                {
                    levelButtonTexts[level - 1].text = maxed ? "MAX" : owned ? "Owned" : nextToBuy ? $"Buy ${cost}" : "Locked";
                }

                if (levelButtons[level - 1] != null)
                {
                    levelButtons[level - 1].interactable = nextToBuy;
                }

                if (levelFrames[level - 1] != null)
                {
                    levelFrames[level - 1].color = maxed
                        ? maxedColor
                        : owned
                            ? ownedColor
                            : nextToBuy && canAfford
                                ? nextColor
                                : lockedColor;
                }
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

        private int GetLevelCost(int level)
        {
            return Mathf.RoundToInt(baseSpeedCost + Mathf.Max(0, level - 1) * speedCostStep);
        }

        private float GetSpeedAfterLevel(int level)
        {
            if (stats == null)
            {
                return baselineAnimalSpeed + level * speedGain;
            }

            var inferredBaseSpeed = Mathf.Max(0f, stats.AnimalSpeed - stats.SpeedUpgradeLevel * speedGain);
            return inferredBaseSpeed + level * speedGain;
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
            return label;
        }

        private static Image AddImage(GameObject target, Color color)
        {
            var image = target.AddComponent<Image>();
            image.color = color;
            return image;
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
