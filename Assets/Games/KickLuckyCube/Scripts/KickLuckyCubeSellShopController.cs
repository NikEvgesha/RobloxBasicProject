using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeSellShopController : MonoBehaviour
    {
        private const float CardSize = 176f;
        private const float CardSpacing = 14f;
        private const int CardsPerRow = 3;

        [SerializeField] private KickLuckyCubeInventoryController inventory;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private Canvas canvas;
        [SerializeField] private bool buildRuntimeUi = true;

        private readonly List<GameObject> rowObjects = new();
        private RectTransform windowRoot;
        private RectTransform listRoot;
        private ScrollRect scrollRect;
        private Text statusText;
        private Font uiFont;

        public bool IsOpen => windowRoot != null && windowRoot.gameObject.activeSelf;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying
                || FindFirstObjectByType<KickLuckyCubeSellShopController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_SellShopController_Runtime").AddComponent<KickLuckyCubeSellShopController>();
        }

        private void Awake()
        {
            inventory ??= FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas(canvas);
            ResolveUiFont();

            if (buildRuntimeUi)
            {
                BuildRuntimeUi();
            }
        }

        private void Update()
        {
            if (IsOpen && ReadEscapePressed())
            {
                CloseWindow();
            }
        }

        public void OpenWindow()
        {
            EnsureReferences();
            BuildRuntimeUi();

            if (windowRoot == null)
            {
                return;
            }

            windowRoot.gameObject.SetActive(true);
            windowRoot.SetAsLastSibling();
            RefreshRows();
        }

        public void CloseWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.gameObject.SetActive(false);
            }
        }

        public void RefreshRows()
        {
            ClearRows();

            if (inventory == null)
            {
                SetStatus("Inventory is not ready.");
                return;
            }

            var sellableAnimals = inventory.GetSellableAnimals();
            if (sellableAnimals.Length == 0)
            {
                SetStatus("No mobs to sell yet.");
                return;
            }

            SetStatus($"Mobs for sale: {sellableAnimals.Length}");
            foreach (var slot in sellableAnimals)
            {
                CreateAnimalCard(slot);
            }

            if (listRoot != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(listRoot);
            }

            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void CreateAnimalCard(KickLuckyCubeInventoryController.AnimalSlot slot)
        {
            var card = CreateRect("KLC_SellShopCard_" + slot.Animal.DisplayName, listRoot);
            card.sizeDelta = new Vector2(CardSize, CardSize);
            var layoutElement = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<LayoutElement>(card.gameObject);
            layoutElement.preferredHeight = CardSize;
            layoutElement.preferredWidth = CardSize;
            AddImage(card.gameObject, new Color(0.08f, 0.06f, 0.04f, 0.86f));
            rowObjects.Add(card.gameObject);

            var icon = CreateRect("Icon", card);
            icon.anchorMin = new Vector2(0.5f, 1f);
            icon.anchorMax = new Vector2(0.5f, 1f);
            icon.pivot = new Vector2(0.5f, 1f);
            icon.anchoredPosition = new Vector2(0f, -14f);
            icon.sizeDelta = new Vector2(86f, 66f);
            var iconImage = AddImage(icon.gameObject, slot.Animal.BodyColor);
            var iconSprite = KickLuckyCubeAnimalCatalog.LoadIcon(slot.Animal);
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.preserveAspect = true;
                iconImage.color = Color.white;
            }

            CreateLabel(
                card,
                "Name",
                slot.Animal.DisplayName,
                16,
                TextAnchor.MiddleCenter,
                new Vector2(158f, 26f),
                new Vector2(0f, 8f));
            CreateLabel(
                card,
                "Rarity",
                slot.Animal.Grade == KickLuckyCubeAnimalGrade.Normal
                    ? slot.Animal.Rarity.ToString()
                    : $"{slot.Animal.Rarity} / {KickLuckyCubeAnimalGradeUtility.GetShortName(slot.Animal.Grade)}",
                14,
                TextAnchor.MiddleCenter,
                new Vector2(158f, 22f),
                new Vector2(0f, -17f));
            CreateLabel(
                card,
                "Income",
                $"+{slot.Animal.IncomePerSecond}/s income",
                13,
                TextAnchor.MiddleCenter,
                new Vector2(158f, 20f),
                new Vector2(0f, -40f));
            CreateLabel(
                card,
                "SellValue",
                $"Sell: ${slot.Animal.SellValue}",
                14,
                TextAnchor.MiddleCenter,
                new Vector2(158f, 22f),
                new Vector2(0f, -62f));

            var sellButton = CreateButton(card, "SellButton", "Sell", new Vector2(118f, 30f));
            sellButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -82f);
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => SellSlot(slot));
        }

        private void SellSlot(KickLuckyCubeInventoryController.AnimalSlot slot)
        {
            EnsureReferences();
            if (inventory == null || wallet == null)
            {
                SetStatus("Sell shop is not ready.");
                return;
            }

            if (!inventory.TrySellAnimal(slot, wallet, out var soldAnimal))
            {
                SetStatus("This mob is no longer available.");
                RefreshRows();
                return;
            }

            SetStatus($"Sold {soldAnimal.DisplayName} for ${soldAnimal.SellValue}.");
            RefreshRows();
        }

        private void BuildRuntimeUi()
        {
            if (windowRoot != null)
            {
                return;
            }

            if (canvas == null)
            {
                canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas(canvas);
            }

            if (canvas == null)
            {
                return;
            }

            windowRoot = CreateRect("KLC_SellShopWindow_Runtime", canvas.transform);
            windowRoot.anchorMin = new Vector2(0.5f, 0.5f);
            windowRoot.anchorMax = new Vector2(0.5f, 0.5f);
            windowRoot.pivot = new Vector2(0.5f, 0.5f);
            windowRoot.anchoredPosition = new Vector2(0f, 10f);
            windowRoot.sizeDelta = new Vector2(760f, 560f);
            AddImage(windowRoot.gameObject, new Color(0.05f, 0.035f, 0.025f, 0.88f));

            CreateLabel(windowRoot, "Title", "Sell Mobs", 34, TextAnchor.MiddleLeft, new Vector2(260f, 44f), new Vector2(-248f, 228f));
            CreateLabel(windowRoot, "Hint", "Choose a mob and press Sell", 18, TextAnchor.MiddleRight, new Vector2(330f, 32f), new Vector2(82f, 228f));

            var closeButton = CreateButton(windowRoot, "CloseButton", "X", new Vector2(42f, 32f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(344f, 228f);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseWindow);

            var viewport = CreateRect("KLC_SellShopViewport", windowRoot);
            viewport.anchorMin = new Vector2(0.5f, 0.5f);
            viewport.anchorMax = new Vector2(0.5f, 0.5f);
            viewport.pivot = new Vector2(0.5f, 0.5f);
            viewport.anchoredPosition = new Vector2(0f, -14f);
            viewport.sizeDelta = new Vector2(620f, 412f);
            AddImage(viewport.gameObject, new Color(0.02f, 0.018f, 0.014f, 0.34f));
            KickLuckyCubeUiPrefabFactory.GetOrAddComponent<RectMask2D>(viewport.gameObject);

            listRoot = CreateRect("KLC_SellShopGrid", viewport);
            listRoot.anchorMin = new Vector2(0.5f, 1f);
            listRoot.anchorMax = new Vector2(0.5f, 1f);
            listRoot.pivot = new Vector2(0.5f, 1f);
            listRoot.anchoredPosition = new Vector2(0f, -8f);
            listRoot.sizeDelta = new Vector2(CardsPerRow * CardSize + (CardsPerRow - 1) * CardSpacing, 0f);
            var gridLayout = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<GridLayoutGroup>(listRoot.gameObject);
            gridLayout.cellSize = new Vector2(CardSize, CardSize);
            gridLayout.spacing = new Vector2(CardSpacing, CardSpacing);
            gridLayout.childAlignment = TextAnchor.UpperCenter;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = CardsPerRow;
            gridLayout.padding = new RectOffset(0, 0, 0, 48);
            var contentSizeFitter = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<ContentSizeFitter>(listRoot.gameObject);
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<ScrollRect>(windowRoot.gameObject);
            scrollRect.viewport = viewport;
            scrollRect.content = listRoot;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.18f;
            scrollRect.inertia = true;
            scrollRect.scrollSensitivity = 28f;

            statusText = CreateLabel(windowRoot, "Status", string.Empty, 18, TextAnchor.MiddleLeft, new Vector2(650f, 30f), new Vector2(0f, -244f));
            windowRoot.gameObject.SetActive(false);
        }

        private void ClearRows()
        {
            foreach (var rowObject in rowObjects)
            {
                if (rowObject != null)
                {
                    Destroy(rowObject);
                }
            }

            rowObjects.Clear();
        }

        private void EnsureReferences()
        {
            inventory ??= FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
        }

        private void SetStatus(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }

        private Button CreateButton(RectTransform parent, string name, string label, Vector2 size)
        {
            return KickLuckyCubeUiPrefabFactory.GetOrCreateButton(parent, name, label, uiFont, size, new Color(0.18f, 0.44f, 0.12f, 0.92f), 16);
        }

        private Text CreateLabel(
            RectTransform parent,
            string name,
            string text,
            int fontSize,
            TextAnchor anchor,
            Vector2 size,
            Vector2 anchoredPosition)
        {
            return KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(parent, name, uiFont, text, fontSize, anchor, size, anchoredPosition);
        }

        private void ResolveUiFont()
        {
            uiFont = KickLuckyCubeUiTheme.Font;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            return KickLuckyCubeUiPrefabFactory.CreateRect(name, parent);
        }

        private static Image AddImage(GameObject target, Color color)
        {
            return KickLuckyCubeUiTheme.AddImage(target, color);
        }

        private static bool ReadEscapePressed()
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
