using System;
using System.Reflection;
using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStyleShopController : MonoBehaviour
    {
        private const BindingFlags SerializedFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private Canvas canvas;
        [SerializeField] private string styleAnchorName = "KLC_Kiosk_02_StyleShop_HoldInteractionAnchor";

        private RectTransform windowRoot;
        private Text statusText;
        private Text[] buttonTexts = Array.Empty<Text>();
        private Button[] actionButtons = Array.Empty<Button>();
        private Image[] accentImages = Array.Empty<Image>();
        private Font uiFont;
        private int selectedStyle;

        public bool IsOpen => windowRoot != null && windowRoot.gameObject.activeSelf;
        public int SelectedStyle => selectedStyle;
        public string SelectedStyleId => KickLuckyCubeKickStyleCatalog.Get(selectedStyle).Id;
        public string SelectedStyleName => KickLuckyCubeKickStyleCatalog.Get(selectedStyle).DisplayName;
        public float SelectedKickStrengthMultiplier => KickLuckyCubeKickStyleCatalog.Get(selectedStyle).StrengthMultiplier;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeStyleShopController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_KickStyleShop_Runtime").AddComponent<KickLuckyCubeStyleShopController>();
        }

        private void Awake()
        {
            ResolveReferences();
            ReloadSavedSelection();
            BuildRuntimeUi();
            ConfigureStyleAnchor();
            CloseWindow();
            Refresh();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
                wallet.Changed += OnWalletChanged;
            }
        }

        private void OnDisable()
        {
            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
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
            SetStatus($"Equipped: {SelectedStyleName}. Premium styles add +10% actual kick strength.");
            Refresh();
        }

        public void CloseWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.gameObject.SetActive(false);
            }
        }

        public bool CanOpen()
        {
            ResolveReferences();
            return runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal && !runPhase.IsSelectingAnimal);
        }

        public bool TrySelectOrBuyStyle(int index)
        {
            ResolveReferences();
            if (index < 0 || index >= KickLuckyCubeKickStyleCatalog.Count)
            {
                return false;
            }

            var style = KickLuckyCubeKickStyleCatalog.Get(index);
            if (!KickLuckyCubeKickStyleCatalog.IsOwned(index))
            {
                if (wallet == null || !wallet.TrySpendHard(style.HardCost))
                {
                    SetStatus($"Need {style.HardCost} hard currency for {style.DisplayName}.");
                    Refresh();
                    return false;
                }

                KickLuckyCubeKickStyleCatalog.Unlock(index);
            }

            selectedStyle = index;
            KickLuckyCubeKickStyleCatalog.SaveSelectedIndex(selectedStyle);
            SetStatus($"{style.DisplayName} equipped. Kick strength x{style.StrengthMultiplier:0.00}.");
            Refresh();
            return true;
        }

        public bool IsStyleOwned(int index)
        {
            return index >= 0
                && index < KickLuckyCubeKickStyleCatalog.Count
                && KickLuckyCubeKickStyleCatalog.IsOwned(index);
        }

        public void ReloadSavedSelection()
        {
            selectedStyle = KickLuckyCubeKickStyleCatalog.LoadSelectedIndex();
            Refresh();
        }

        private void ResolveReferences()
        {
            var previousWallet = wallet;
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas(canvas);
            uiFont ??= KickLuckyCubeUiTheme.Font;

            if (isActiveAndEnabled && wallet != previousWallet)
            {
                if (previousWallet != null)
                {
                    previousWallet.Changed -= OnWalletChanged;
                }

                if (wallet != null)
                {
                    wallet.Changed -= OnWalletChanged;
                    wallet.Changed += OnWalletChanged;
                }
            }
        }

        private void ConfigureStyleAnchor()
        {
            var anchor = GameObject.Find(styleAnchorName);
            if (anchor == null)
            {
                return;
            }

            var collider = GetOrAddComponent<BoxCollider>(anchor);
            collider.isTrigger = true;
            collider.center = Vector3.zero;
            collider.size = new Vector3(3.0f, 3.0f, 2.4f);

            var target = GetOrAddComponent<GameKitInteractionTarget>(anchor);
            SetPrivateField(target, "promptKey", "E");
            SetPrivateField(target, "promptText", "Open kick style shop");
            SetPrivateField(target, "activationMode", GameKitInteractionActivationMode.Press);
            SetPrivateField(target, "holdSeconds", 0.05f);
            SetPrivateField(target, "priority", 32);
            SetPrivateField(target, "interactable", true);

            if (anchor.GetComponent<GameKitInteractionTriggerSource>() == null)
            {
                anchor.AddComponent<GameKitInteractionTriggerSource>();
            }

            GetOrAddComponent<KickLuckyCubeStyleShopPad>(anchor).Configure(this);
        }

        private void BuildRuntimeUi()
        {
            ResolveReferences();
            if (canvas == null || windowRoot != null)
            {
                return;
            }

            windowRoot = CreateRect("KLC_StyleShopWindow_Runtime", canvas.transform);
            windowRoot.anchorMin = new Vector2(0.5f, 0.5f);
            windowRoot.anchorMax = new Vector2(0.5f, 0.5f);
            windowRoot.pivot = new Vector2(0.5f, 0.5f);
            windowRoot.anchoredPosition = new Vector2(0f, 12f);
            windowRoot.sizeDelta = new Vector2(760f, 500f);
            AddImage(windowRoot.gameObject, new Color(0.035f, 0.035f, 0.05f, 0.96f));

            CreateLabel(windowRoot, "Title", "Kick Styles", 34, TextAnchor.MiddleLeft, new Vector2(420f, 48f), new Vector2(-130f, 206f));
            statusText = CreateLabel(windowRoot, "Status", "Premium kick styles cost hard currency and add +10% strength.", 17, TextAnchor.MiddleLeft, new Vector2(650f, 42f), new Vector2(-5f, -218f));

            var closeButton = CreateButton(windowRoot, "Close", "X", new Vector2(44f, 36f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(344f, 206f);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseWindow);

            var grid = CreateRect("StyleCards", windowRoot);
            grid.sizeDelta = new Vector2(680f, 344f);
            grid.anchoredPosition = new Vector2(0f, -4f);

            var layout = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<GridLayoutGroup>(grid.gameObject);
            layout.cellSize = new Vector2(216f, 162f);
            layout.spacing = new Vector2(14f, 14f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;

            buttonTexts = new Text[KickLuckyCubeKickStyleCatalog.Count];
            actionButtons = new Button[KickLuckyCubeKickStyleCatalog.Count];
            accentImages = new Image[KickLuckyCubeKickStyleCatalog.Count];
            for (var index = 0; index < KickLuckyCubeKickStyleCatalog.Count; index++)
            {
                CreateStyleCard(grid, index);
            }
        }

        private void CreateStyleCard(RectTransform parent, int index)
        {
            var style = KickLuckyCubeKickStyleCatalog.Get(index);
            var card = CreateRect("Style_" + index, parent);
            AddImage(card.gameObject, new Color(0.10f, 0.12f, 0.16f, 0.96f));

            var accent = CreateRect("Swatch", card);
            accent.sizeDelta = new Vector2(176f, 30f);
            accent.anchoredPosition = new Vector2(0f, 58f);
            accentImages[index] = AddImage(accent.gameObject, style.AccentColor);
            var previewButton = GetOrAddComponent<Button>(accent.gameObject);
            previewButton.targetGraphic = accentImages[index];
            previewButton.onClick.RemoveAllListeners();
            var previewIndex = index;
            previewButton.onClick.AddListener(() => PreviewStyle(previewIndex));
            CreateLabel(accent, "PreviewLabel", "Preview", 13, TextAnchor.MiddleCenter, new Vector2(172f, 28f), Vector2.zero);

            var bonusLine = style.IsDefault ? "No strength bonus" : "+10% kick strength";
            CreateLabel(card, "Name", style.DisplayName + "\n" + bonusLine, 14, TextAnchor.MiddleCenter, new Vector2(196f, 68f), new Vector2(0f, 8f));

            var button = CreateButton(card, "Action", string.Empty, new Vector2(176f, 32f));
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -60f);
            var capturedIndex = index;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => TrySelectOrBuyStyle(capturedIndex));
            actionButtons[index] = button;
            buttonTexts[index] = button.GetComponentInChildren<Text>();
        }

        private void PreviewStyle(int index)
        {
            var kickController = FindFirstObjectByType<KickLuckyCubeKickController>(FindObjectsInactive.Include);
            var actor = GameObject.Find("KLC_PrototypePlayer");
            if (kickController == null || actor == null || !kickController.TryPreviewKickStyle(index, actor))
            {
                SetStatus("Kick preview is unavailable right now.");
                return;
            }

            CloseWindow();
        }

        private void Refresh()
        {
            for (var index = 0; index < buttonTexts.Length; index++)
            {
                if (buttonTexts[index] == null)
                {
                    continue;
                }

                var style = KickLuckyCubeKickStyleCatalog.Get(index);
                var owned = KickLuckyCubeKickStyleCatalog.IsOwned(index);
                var equipped = index == selectedStyle;
                buttonTexts[index].text = equipped
                    ? "Equipped"
                    : owned
                        ? "Equip"
                        : $"Buy {style.HardCost} hard";

                if (actionButtons[index] != null)
                {
                    actionButtons[index].interactable = !equipped;
                }

                if (accentImages[index] != null)
                {
                    accentImages[index].color = style.AccentColor;
                }
            }
        }

        private void OnWalletChanged(long soft, long hard)
        {
            Refresh();
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            var field = typeof(TTarget).GetField(fieldName, SerializedFieldFlags);
            field?.SetValue(target, value);
        }

        private static T GetOrAddComponent<T>(GameObject target)
            where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private Image AddImage(GameObject target, Color color)
        {
            return KickLuckyCubeUiTheme.AddImage(target, color);
        }

        private RectTransform CreateRect(string name, Transform parent)
        {
            return KickLuckyCubeUiPrefabFactory.CreateRect(name, parent);
        }

        private Text CreateLabel(RectTransform parent, string name, string value, int fontSize, TextAnchor anchor, Vector2 size, Vector2 position)
        {
            var label = KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(parent, name, uiFont, value, fontSize, anchor, size, position);
            DisableLegacyLocalization(label != null ? label.gameObject : null);
            if (label != null)
            {
                label.text = value;
            }

            return label;
        }

        private Button CreateButton(RectTransform parent, string name, string value, Vector2 size)
        {
            var button = KickLuckyCubeUiPrefabFactory.GetOrCreateButton(parent, name, value, uiFont, size, new Color(0.15f, 0.18f, 0.22f, 0.94f), 15);
            DisableLegacyLocalization(button != null ? button.gameObject : null);
            return button;
        }

        private static void DisableLegacyLocalization(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            foreach (var localizedText in root.GetComponentsInChildren<KickLuckyCubeLocalizedText>(true))
            {
                localizedText.enabled = false;
            }
        }
    }

    [RequireComponent(typeof(GameKitInteractionTarget))]
    public sealed class KickLuckyCubeStyleShopPad : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeStyleShopController styleShop;

        private GameKitInteractionTarget interactionTarget;

        private void Awake()
        {
            styleShop ??= FindFirstObjectByType<KickLuckyCubeStyleShopController>(FindObjectsInactive.Include);
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

        private void OnTriggerExit(Collider other)
        {
            var driver = FindFirstObjectByType<GameKitInteractionDriver>();
            if (driver != null && driver.IsActorCollider(other))
            {
                styleShop?.CloseWindow();
            }
        }

        public void Configure(KickLuckyCubeStyleShopController controller)
        {
            styleShop = controller;
        }

        public bool CanInteract(GameObject actor)
        {
            styleShop ??= FindFirstObjectByType<KickLuckyCubeStyleShopController>(FindObjectsInactive.Include);
            return styleShop != null && styleShop.CanOpen();
        }

        private void OpenShop(GameObject actor)
        {
            styleShop ??= FindFirstObjectByType<KickLuckyCubeStyleShopController>(FindObjectsInactive.Include);
            styleShop?.OpenWindow();
        }
    }
}
