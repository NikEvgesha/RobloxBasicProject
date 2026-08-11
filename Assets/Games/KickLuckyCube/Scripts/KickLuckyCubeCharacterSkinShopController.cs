using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeCharacterSkinShopController : MonoBehaviour
    {
        private sealed class ItemCardUi
        {
            public KickLuckyCubeAppearanceItemDefinition Definition;
            public Text ActionText;
            public Text PriceText;
            public Button ActionButton;
            public Image CardImage;
        }

        [SerializeField] private KickLuckyCubeCharacterCustomization customization;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private Canvas canvas;

        private readonly List<ItemCardUi> itemCards = new();
        private readonly Dictionary<KickLuckyCubeAppearanceSlot, Button> tabButtons = new();
        private RectTransform windowRoot;
        private RectTransform itemGrid;
        private Text statusText;
        private Text balanceText;
        private Text categoryTitle;
        private Button openButton;
        private Font uiFont;
        private KickLuckyCubeAppearanceSlot activeSlot = KickLuckyCubeAppearanceSlot.Body;

        public bool IsOpen => windowRoot != null && windowRoot.gameObject.activeSelf;
        public KickLuckyCubeAppearanceSlot ActiveSlot => activeSlot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying
                || FindFirstObjectByType<KickLuckyCubeCharacterSkinShopController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_CharacterSkinShop_Runtime")
                .AddComponent<KickLuckyCubeCharacterSkinShopController>();
        }

        private void Awake()
        {
            ResolveReferences();
            Subscribe();
            BuildRuntimeUi();
            CloseWindow();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Update()
        {
            var togglePressed = false;
#if ENABLE_INPUT_SYSTEM
            togglePressed = Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame;
#else
            togglePressed = Input.GetKeyDown(KeyCode.K);
#endif
            if (togglePressed)
            {
                ToggleWindow();
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
            BuildCategory(activeSlot);
            Refresh();
        }

        public void CloseWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.gameObject.SetActive(false);
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

        private void ResolveReferences()
        {
            customization ??= KickLuckyCubeCharacterCustomization.Instance
                ?? FindFirstObjectByType<KickLuckyCubeCharacterCustomization>(FindObjectsInactive.Include);
            wallet ??= customization != null
                ? customization.Wallet
                : FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas(canvas);
            uiFont ??= KickLuckyCubeUiTheme.Font;
        }

        private void Subscribe()
        {
            ResolveReferences();
            if (customization != null)
            {
                customization.Changed -= Refresh;
                customization.Changed += Refresh;
            }

            if (wallet != null)
            {
                wallet.Changed -= HandleWalletChanged;
                wallet.Changed += HandleWalletChanged;
            }
        }

        private void Unsubscribe()
        {
            if (customization != null)
            {
                customization.Changed -= Refresh;
            }

            if (wallet != null)
            {
                wallet.Changed -= HandleWalletChanged;
            }
        }

        private void BuildRuntimeUi()
        {
            ResolveReferences();
            Subscribe();
            if (canvas == null || windowRoot != null)
            {
                return;
            }

            openButton = CreateButton(canvas.transform as RectTransform, "KLC_SkinOpenButton_Runtime", "SKINS", new Vector2(132f, 54f));
            var openRect = openButton.GetComponent<RectTransform>();
            openRect.anchorMin = new Vector2(1f, 0.5f);
            openRect.anchorMax = new Vector2(1f, 0.5f);
            openRect.pivot = new Vector2(1f, 0.5f);
            openRect.anchoredPosition = new Vector2(-22f, -120f);
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(ToggleWindow);

            windowRoot = CreateRect("KLC_SkinShopWindow_Runtime", canvas.transform);
            windowRoot.anchorMin = new Vector2(0.5f, 0.5f);
            windowRoot.anchorMax = new Vector2(0.5f, 0.5f);
            windowRoot.pivot = new Vector2(0.5f, 0.5f);
            windowRoot.anchoredPosition = new Vector2(0f, 8f);
            windowRoot.sizeDelta = new Vector2(980f, 630f);
            AddImage(windowRoot.gameObject, new Color(0.035f, 0.055f, 0.075f, 0.97f));

            CreateLabel(windowRoot, "Title", "Character Wardrobe", 34, TextAnchor.MiddleLeft, new Vector2(450f, 46f), new Vector2(-235f, 278f));
            balanceText = CreateLabel(windowRoot, "HardValue", string.Empty, 22, TextAnchor.MiddleRight, new Vector2(240f, 38f), new Vector2(270f, 278f));

            var closeButton = CreateButton(windowRoot, "Close", "X", new Vector2(46f, 38f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(452f, 278f);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseWindow);

            var tabGrid = KickLuckyCubeUiPrefabFactory.CreateRectInstance("KLC_SkinGrid", "KLC_SkinTabGrid", windowRoot);
            tabGrid.sizeDelta = new Vector2(900f, 76f);
            tabGrid.anchoredPosition = new Vector2(0f, 218f);
            var tabLayout = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<GridLayoutGroup>(tabGrid.gameObject);
            tabLayout.cellSize = new Vector2(94f, 68f);
            tabLayout.spacing = new Vector2(7f, 0f);
            tabLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            tabLayout.constraintCount = 1;

            foreach (var slot in KickLuckyCubeCharacterSkinCatalog.SlotOrder)
            {
                CreateTab(tabGrid, slot);
            }

            categoryTitle = CreateLabel(windowRoot, "CategoryTitle", string.Empty, 25, TextAnchor.MiddleLeft, new Vector2(500f, 38f), new Vector2(-190f, 160f));
            CreateLabel(windowRoot, "MixHint", "Mix any owned pieces. Free defaults switch with body type.", 16, TextAnchor.MiddleRight, new Vector2(520f, 32f), new Vector2(185f, 160f));

            itemGrid = KickLuckyCubeUiPrefabFactory.CreateRectInstance("KLC_SkinGrid", "KLC_SkinItemGrid", windowRoot);
            itemGrid.sizeDelta = new Vector2(900f, 350f);
            itemGrid.anchoredPosition = new Vector2(0f, -34f);
            var itemLayout = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<GridLayoutGroup>(itemGrid.gameObject);
            itemLayout.cellSize = new Vector2(288f, 164f);
            itemLayout.spacing = new Vector2(16f, 12f);
            itemLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            itemLayout.constraintCount = 3;

            statusText = CreateLabel(windowRoot, "Status", "Choose a category and equip a piece.", 17, TextAnchor.MiddleLeft, new Vector2(760f, 34f), new Vector2(-55f, -286f));
            BuildCategory(activeSlot, true);
            Refresh();
        }

        private void CreateTab(RectTransform parent, KickLuckyCubeAppearanceSlot slot)
        {
            var button = CreateButton(parent, "KLC_SkinTabButton_" + slot, KickLuckyCubeCharacterSkinCatalog.GetSlotDisplayName(slot), new Vector2(94f, 68f));
            var capturedSlot = slot;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => BuildCategory(capturedSlot));
            tabButtons[slot] = button;

            var iconRect = CreateRect("Icon", button.transform);
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(42f, 42f);
            iconRect.anchoredPosition = new Vector2(0f, 8f);
            var icon = iconRect.GetComponent<Image>();
            if (icon != null)
            {
                icon.sprite = Resources.Load<Sprite>(KickLuckyCubeCharacterSkinCatalog.GetTabIconResourcePath(slot));
                icon.color = Color.white;
                icon.preserveAspect = true;
                icon.raycastTarget = false;
            }

            var label = button.GetComponentInChildren<Text>(true);
            if (label != null)
            {
                var labelRect = label.rectTransform;
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 0f);
                labelRect.pivot = new Vector2(0.5f, 0f);
                labelRect.anchoredPosition = new Vector2(0f, 2f);
                labelRect.sizeDelta = new Vector2(0f, 20f);
                label.fontSize = 12;
                label.alignment = TextAnchor.MiddleCenter;
            }
        }

        private void BuildCategory(KickLuckyCubeAppearanceSlot slot, bool force = false)
        {
            if (!force && slot == activeSlot && itemCards.Count > 0)
            {
                return;
            }

            activeSlot = slot;
            if (itemGrid == null)
            {
                return;
            }

            for (var index = itemGrid.childCount - 1; index >= 0; index--)
            {
                Destroy(itemGrid.GetChild(index).gameObject);
            }

            itemCards.Clear();
            var definitions = KickLuckyCubeCharacterSkinCatalog.GetForSlot(slot);
            for (var index = 0; index < definitions.Count; index++)
            {
                CreateItemCard(itemGrid, definitions[index]);
            }

            if (categoryTitle != null)
            {
                categoryTitle.text = KickLuckyCubeCharacterSkinCatalog.GetSlotDisplayName(slot);
            }

            Refresh();
        }

        private void CreateItemCard(RectTransform parent, KickLuckyCubeAppearanceItemDefinition definition)
        {
            var card = CreateRect("KLC_SkinCard_" + definition.Id, parent);
            var cardImage = AddImage(card.gameObject, new Color(0.08f, 0.13f, 0.18f, 0.96f));

            var swatch = CreateRect("Swatch", card);
            swatch.sizeDelta = new Vector2(76f, 76f);
            swatch.anchoredPosition = new Vector2(-104f, 42f);
            var swatchImage = AddImage(swatch.gameObject, definition.PreviewColor);
            if (swatchImage != null)
            {
                var itemIcon = KickLuckyCubeCharacterSkinCatalog.RequiresWardrobeIcon(definition.Slot)
                    ? Resources.Load<Sprite>(KickLuckyCubeCharacterSkinCatalog.GetItemIconResourcePath(definition.Id))
                    : null;
                if (itemIcon != null)
                {
                    swatchImage.sprite = itemIcon;
                    swatchImage.color = Color.white;
                    swatchImage.preserveAspect = true;
                }
                else
                {
                    swatchImage.color = definition.PreviewColor;
                }
            }

            CreateLabel(card, "Name", definition.DisplayName, 19, TextAnchor.MiddleLeft, new Vector2(190f, 30f), new Vector2(34f, 58f));
            CreateLabel(card, "Detail", definition.Description, 14, TextAnchor.UpperLeft, new Vector2(190f, 48f), new Vector2(34f, 18f));
            var price = CreateLabel(card, "Price", string.Empty, 16, TextAnchor.MiddleLeft, new Vector2(116f, 30f), new Vector2(-68f, -54f));
            var action = CreateButton(card, "Action", string.Empty, new Vector2(118f, 36f));
            action.GetComponent<RectTransform>().anchoredPosition = new Vector2(76f, -54f);
            var capturedId = definition.Id;
            action.onClick.RemoveAllListeners();
            action.onClick.AddListener(() => SelectItem(capturedId));

            itemCards.Add(new ItemCardUi
            {
                Definition = definition,
                ActionText = action.GetComponentInChildren<Text>(true),
                PriceText = price,
                ActionButton = action,
                CardImage = cardImage,
            });
        }

        private void SelectItem(string itemId)
        {
            ResolveReferences();
            if (customization == null)
            {
                SetStatus("Customization system is not ready yet.");
                return;
            }

            customization.TryPurchaseAndEquip(itemId, out var status);
            SetStatus(status);
            Refresh();
        }

        private void Refresh()
        {
            ResolveReferences();
            if (balanceText != null)
            {
                balanceText.text = wallet != null ? $"◆ {wallet.HardCurrency:N0}" : "◆ --";
            }

            foreach (var pair in tabButtons)
            {
                if (pair.Value == null)
                {
                    continue;
                }

                var colors = pair.Value.colors;
                colors.normalColor = pair.Key == activeSlot ? new Color(0.70f, 1f, 0.78f, 1f) : Color.white;
                pair.Value.colors = colors;
            }

            for (var index = 0; index < itemCards.Count; index++)
            {
                var card = itemCards[index];
                if (card?.Definition == null)
                {
                    continue;
                }

                var selected = customization != null
                    && string.Equals(customization.GetSelectedId(card.Definition.Slot), card.Definition.Id, StringComparison.Ordinal);
                var owned = customization != null && customization.IsOwned(card.Definition.Id);
                if (card.ActionText != null)
                {
                    card.ActionText.text = selected ? "Equipped" : owned ? "Equip" : "Buy";
                }

                if (card.PriceText != null)
                {
                    card.PriceText.text = card.Definition.IsFree ? "FREE" : $"◆ {card.Definition.HardCost}";
                    card.PriceText.color = card.Definition.IsFree ? KickLuckyCubeUiTheme.Primary : KickLuckyCubeUiTheme.HardCurrency;
                }

                if (card.ActionButton != null)
                {
                    card.ActionButton.interactable = !selected;
                }

                if (card.CardImage != null)
                {
                    card.CardImage.color = selected
                        ? new Color(0.12f, 0.55f, 0.34f, 0.98f)
                        : new Color(0.08f, 0.13f, 0.18f, 0.96f);
                }
            }
        }

        private void HandleWalletChanged(long soft, long hard)
        {
            Refresh();
        }

        private void SetStatus(string value)
        {
            if (statusText != null)
            {
                statusText.text = value;
            }
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
            var existing = KickLuckyCubeUiPrefabFactory.FindDirectChild(parent, name);
            var rect = existing ?? KickLuckyCubeUiPrefabFactory.CreateRectInstance(
                name.IndexOf("Close", StringComparison.OrdinalIgnoreCase) >= 0 ? "Close" : "Action",
                name,
                parent);
            rect.sizeDelta = size;

            var button = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<Button>(rect.gameObject);
            var image = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<Image>(rect.gameObject);
            button.targetGraphic = image;
            KickLuckyCubeUiTheme.StyleButton(button, name);

            var label = button.GetComponentInChildren<Text>(true);
            if (label == null)
            {
                var labelRect = KickLuckyCubeUiPrefabFactory.CreateRectInstance("Label", "Label", rect);
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;
                label = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<Text>(labelRect.gameObject);
            }

            if (label != null)
            {
                label.text = value;
                label.font = uiFont;
                label.fontSize = 15;
                label.alignment = TextAnchor.MiddleCenter;
                KickLuckyCubeUiTheme.StyleText(label, "Label");
            }

            return button;
        }
    }
}
