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
        private const string SelectedStyleKey = "KickLuckyCube.Style.Selected";
        private const string OwnedStyleKeyPrefix = "KickLuckyCube.Style.Owned.";

        private static readonly string[] StyleNames = { "Classic", "Candy Cube", "Neon Cube", "Gold Cube" };
        private static readonly int[] StyleCosts = { 0, 450, 1250, 3200 };
        private static readonly Color[] StyleColors =
        {
            new(1f, 0.86f, 0.24f),
            new(1f, 0.34f, 0.78f),
            new(0.12f, 0.9f, 1f),
            new(1f, 0.72f, 0.08f),
        };

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private Canvas canvas;
        [SerializeField] private string styleAnchorName = "KLC_Kiosk_02_StyleShop_HoldInteractionAnchor";
        [SerializeField] private string luckyCubeName = "KLC_LuckyCube";

        private RectTransform windowRoot;
        private Text statusText;
        private Text[] buttonTexts = Array.Empty<Text>();
        private Image[] swatches = Array.Empty<Image>();
        private Font uiFont;
        private int selectedStyle;

        public bool IsOpen => windowRoot != null && windowRoot.gameObject.activeSelf;

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

            new GameObject("KLC_StyleShop_Runtime").AddComponent<KickLuckyCubeStyleShopController>();
        }

        private void Awake()
        {
            ResolveReferences();
            LoadStyle();
            BuildRuntimeUi();
            ConfigureStyleAnchor();
            ApplyStyle();
            CloseWindow();
            Refresh();
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

        public bool CanOpen()
        {
            ResolveReferences();
            return runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal && !runPhase.IsSelectingAnimal);
        }

        private void SelectOrBuyStyle(int index)
        {
            ResolveReferences();
            if (index < 0 || index >= StyleNames.Length)
            {
                return;
            }

            if (!IsOwned(index))
            {
                var cost = StyleCosts[index];
                if (wallet == null || !wallet.TrySpendSoft(cost))
                {
                    SetStatus($"Need {cost} soft for {StyleNames[index]}.");
                    Refresh();
                    return;
                }

                PlayerPrefs.SetInt(OwnedStyleKeyPrefix + index, 1);
            }

            selectedStyle = index;
            PlayerPrefs.SetInt(SelectedStyleKey, selectedStyle);
            PlayerPrefs.Save();
            ApplyStyle();
            SetStatus($"{StyleNames[index]} equipped.");
            Refresh();
        }

        private void LoadStyle()
        {
            PlayerPrefs.SetInt(OwnedStyleKeyPrefix + 0, 1);
            selectedStyle = Mathf.Clamp(PlayerPrefs.GetInt(SelectedStyleKey, 0), 0, StyleNames.Length - 1);
            if (!IsOwned(selectedStyle))
            {
                selectedStyle = 0;
            }
        }

        private void ApplyStyle()
        {
            var cube = GameObject.Find(luckyCubeName);
            if (cube == null)
            {
                return;
            }

            var styleColor = StyleColors[Mathf.Clamp(selectedStyle, 0, StyleColors.Length - 1)];
            foreach (var renderer in cube.GetComponentsInChildren<Renderer>(true))
            {
                if (!ShouldTintCubeRenderer(cube, renderer))
                {
                    continue;
                }

                var material = renderer.material;
                if (material != null && material.HasProperty("_Color"))
                {
                    material.color = styleColor;
                }
            }
        }

        private static bool ShouldTintCubeRenderer(GameObject cube, Renderer renderer)
        {
            if (cube == null || renderer == null || renderer is TrailRenderer)
            {
                return false;
            }

            var objectName = renderer.gameObject.name;
            if (objectName.IndexOf("Edge", StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Question", StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Corner", StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Outline", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            if (renderer.transform == cube.transform
                || objectName.IndexOf("Face", StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Body", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return renderer.sharedMaterials != null
                && Array.Exists(
                    renderer.sharedMaterials,
                    material => material != null
                        && material.name.IndexOf("KLC_LuckyCube", StringComparison.OrdinalIgnoreCase) >= 0
                        && material.name.IndexOf("Black", StringComparison.OrdinalIgnoreCase) < 0
                        && material.name.IndexOf("Question", StringComparison.OrdinalIgnoreCase) < 0
                        && material.name.IndexOf("Edge", StringComparison.OrdinalIgnoreCase) < 0);
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            canvas ??= FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            uiFont ??= KickLuckyCubeUiTheme.Font;
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
            collider.center = new Vector3(0f, 1.2f, 0f);
            collider.size = new Vector3(2.6f, 2.4f, 2.0f);

            var target = GetOrAddComponent<GameKitInteractionTarget>(anchor);
            SetPrivateField(target, "promptKey", "E");
            SetPrivateField(target, "promptText", "Hold style shop");
            SetPrivateField(target, "activationMode", GameKitInteractionActivationMode.Hold);
            SetPrivateField(target, "holdSeconds", 0.65f);
            SetPrivateField(target, "priority", 24);
            SetPrivateField(target, "interactable", true);

            if (anchor.GetComponent<GameKitInteractionTriggerSource>() == null)
            {
                anchor.AddComponent<GameKitInteractionTriggerSource>();
            }

            var pad = GetOrAddComponent<KickLuckyCubeStyleShopPad>(anchor);
            pad.Configure(this);
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
            windowRoot.sizeDelta = new Vector2(720f, 360f);
            AddImage(windowRoot.gameObject, new Color(0.035f, 0.035f, 0.05f, 0.94f));

            CreateLabel(windowRoot, "Title", "Cube Styles", 34, TextAnchor.MiddleLeft, new Vector2(360f, 48f), new Vector2(-145f, 136f));
            statusText = CreateLabel(windowRoot, "Status", "Buy and equip lucky cube skins.", 18, TextAnchor.MiddleLeft, new Vector2(500f, 30f), new Vector2(-40f, -144f));

            var closeButton = CreateButton(windowRoot, "Close", "X", new Vector2(44f, 36f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(324f, 136f);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseWindow);

            var grid = CreateRect("StyleCards", windowRoot);
            grid.sizeDelta = new Vector2(630f, 210f);
            grid.anchoredPosition = new Vector2(0f, -10f);

            var layout = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<GridLayoutGroup>(grid.gameObject);
            layout.cellSize = new Vector2(146f, 192f);
            layout.spacing = new Vector2(12f, 0f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;

            buttonTexts = new Text[StyleNames.Length];
            swatches = new Image[StyleNames.Length];
            for (var index = 0; index < StyleNames.Length; index++)
            {
                CreateStyleCard(grid, index);
            }
        }

        private void CreateStyleCard(RectTransform parent, int index)
        {
            var card = CreateRect("Style_" + index, parent);
            AddImage(card.gameObject, new Color(0.10f, 0.12f, 0.16f, 0.94f));

            var swatch = CreateRect("Swatch", card);
            swatch.sizeDelta = new Vector2(92f, 74f);
            swatch.anchoredPosition = new Vector2(0f, 46f);
            swatches[index] = AddImage(swatch.gameObject, StyleColors[index]);

            CreateLabel(card, "Name", StyleNames[index], 17, TextAnchor.MiddleCenter, new Vector2(132f, 40f), new Vector2(0f, -10f));
            var button = CreateButton(card, "Action", string.Empty, new Vector2(120f, 34f));
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -66f);
            var capturedIndex = index;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => SelectOrBuyStyle(capturedIndex));
            buttonTexts[index] = button.GetComponentInChildren<Text>();
        }

        private void Refresh()
        {
            for (var index = 0; index < buttonTexts.Length; index++)
            {
                if (buttonTexts[index] == null)
                {
                    continue;
                }

                buttonTexts[index].text = index == selectedStyle
                    ? "Equipped"
                    : IsOwned(index)
                        ? "Equip"
                        : $"Buy ${StyleCosts[index]}";
            }
        }

        private bool IsOwned(int index)
        {
            return index == 0 || PlayerPrefs.GetInt(OwnedStyleKeyPrefix + index, 0) != 0;
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
            return KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(parent, name, uiFont, value, fontSize, anchor, size, position);
        }

        private Button CreateButton(RectTransform parent, string name, string value, Vector2 size)
        {
            return KickLuckyCubeUiPrefabFactory.GetOrCreateButton(parent, name, value, uiFont, size, new Color(0.15f, 0.18f, 0.22f, 0.94f), 15);
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
