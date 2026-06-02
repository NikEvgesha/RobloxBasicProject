using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeInventoryController : MonoBehaviour
    {
        private const int HotbarAnimalSlotCount = 4;

        public readonly struct AnimalSlot
        {
            public AnimalSlot(bool isHotbar, int index, KickLuckyCubeInventoryAnimal animal)
            {
                IsHotbar = isHotbar;
                Index = index;
                Animal = animal;
            }

            public bool IsHotbar { get; }
            public int Index { get; }
            public KickLuckyCubeInventoryAnimal Animal { get; }
            public bool IsValid => Index >= 0 && Animal.IsValid;
        }

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeToolTrainingController toolTraining;
        [SerializeField] private Canvas canvas;
        [SerializeField, Min(HotbarAnimalSlotCount)] private int inventorySlotCount = 18;
        [SerializeField] private bool buildRuntimeUi = true;
        [SerializeField] private bool hideLegacyToolBelt = true;
        [SerializeField] private string carryAnchorName = "KLC_CarryAnchor";
        [SerializeField] private Vector3 handPreviewScale = new(0.48f, 0.38f, 0.62f);
        [SerializeField] private Color toolFrameColor = new Color(0.20f, 0.26f, 0.32f, 0.86f);
        [SerializeField] private Color animalFrameColor = new Color(0.18f, 0.38f, 0.12f, 0.84f);
        [SerializeField] private Color selectedFrameColor = new Color(1f, 0.82f, 0.18f, 0.94f);
        [SerializeField] private Color emptyFrameColor = new Color(0.06f, 0.08f, 0.08f, 0.58f);

        private readonly KickLuckyCubeInventoryAnimal[] hotbarAnimals = new KickLuckyCubeInventoryAnimal[HotbarAnimalSlotCount];
        private KickLuckyCubeInventoryAnimal[] inventoryAnimals;
        private KickLuckyCubeInventorySlotView toolSlot;
        private KickLuckyCubeInventorySlotView[] hotbarSlots = Array.Empty<KickLuckyCubeInventorySlotView>();
        private KickLuckyCubeInventorySlotView[] inventorySlots = Array.Empty<KickLuckyCubeInventorySlotView>();
        private RectTransform canvasRect;
        private RectTransform hotbarRoot;
        private RectTransform inventoryWindowRoot;
        private KickLuckyCubeInventoryDropZone inventoryDropZone;
        private Transform carryAnchor;
        private GameObject selectedAnimalHandPreview;
        private Renderer selectedAnimalHandRenderer;
        private Text statusText;
        private Font uiFont;
        private GameObject dragGhost;
        private RectTransform dragGhostRect;
        private Text dragGhostText;
        private Image dragGhostImage;
        private int selectedHotbarIndex = -1;
        private int selectedInventoryIndex = -1;
        private bool subscribedToToolTraining;

        public bool IsWindowOpen => inventoryWindowRoot != null && inventoryWindowRoot.gameObject.activeSelf;
        public int HotbarAnimalCount => CountValid(hotbarAnimals);
        public int StoredAnimalCount => CountValid(inventoryAnimals);

        private void Awake()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            toolTraining ??= FindFirstObjectByType<KickLuckyCubeToolTrainingController>(FindObjectsInactive.Include);
            canvas ??= FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            inventoryAnimals = new KickLuckyCubeInventoryAnimal[Mathf.Max(HotbarAnimalSlotCount, inventorySlotCount)];
            ResolveUiFont();
            ResolveCarryAnchor();

            if (canvas != null)
            {
                canvasRect = canvas.transform as RectTransform;
            }

            if (hideLegacyToolBelt)
            {
                HideLegacyToolBelt();
            }

            if (buildRuntimeUi)
            {
                BuildRuntimeUi();
            }

            Refresh();
        }

        private void OnEnable()
        {
            if (stats != null)
            {
                stats.Changed += Refresh;
            }

            ResolveToolTraining();
            SubscribeToolTraining();
        }

        private void OnDisable()
        {
            if (stats != null)
            {
                stats.Changed -= Refresh;
            }

            UnsubscribeToolTraining();
            ClearHandPreview();
            toolTraining?.StopTraining();
        }

        private void Update()
        {
            if (ReadInventoryTogglePressed())
            {
                ToggleWindow();
            }

            var selectedSlot = ReadHotbarNumber();
            if (selectedSlot == 1)
            {
                ToggleToolTraining();
            }
            else if (selectedSlot >= 2 && selectedSlot <= 5)
            {
                var hotbarIndex = selectedSlot - 2;
                if (selectedHotbarIndex == hotbarIndex && selectedInventoryIndex < 0)
                {
                    ClearSelectedAnimal();
                }
                else
                {
                    SelectHotbarAnimal(hotbarIndex);
                }
            }
        }

        public bool TryAddAnimal(KickLuckyCubeSpawnedAnimal animal)
        {
            return TryAddAnimal(KickLuckyCubeInventoryAnimal.FromSpawnedAnimal(animal));
        }

        public bool TryAddAnimal(KickLuckyCubeInventoryAnimal animal)
        {
            if (!animal.IsValid)
            {
                return false;
            }

            for (var index = 0; index < hotbarAnimals.Length; index++)
            {
                if (hotbarAnimals[index].IsValid)
                {
                    continue;
                }

                hotbarAnimals[index] = animal;
                SelectHotbarAnimal(index);
                SetStatus($"{animal.AnimalName} added to hotbar.");
                Refresh();
                return true;
            }

            for (var index = 0; index < inventoryAnimals.Length; index++)
            {
                if (inventoryAnimals[index].IsValid)
                {
                    continue;
                }

                inventoryAnimals[index] = animal;
                SelectInventoryAnimal(index);
                SetStatus($"{animal.AnimalName} added to inventory.");
                Refresh();
                return true;
            }

            SetStatus("Inventory is full.");
            return false;
        }

        public bool TryRemoveSelectedAnimal(out KickLuckyCubeInventoryAnimal animal)
        {
            if (selectedHotbarIndex >= 0 && selectedHotbarIndex < hotbarAnimals.Length)
            {
                animal = hotbarAnimals[selectedHotbarIndex];
                if (!animal.IsValid)
                {
                    return false;
                }

                hotbarAnimals[selectedHotbarIndex] = default;
                selectedHotbarIndex = -1;
                ClearHandPreview();
                Refresh();
                return true;
            }

            if (selectedInventoryIndex >= 0 && selectedInventoryIndex < inventoryAnimals.Length)
            {
                animal = inventoryAnimals[selectedInventoryIndex];
                if (!animal.IsValid)
                {
                    return false;
                }

                inventoryAnimals[selectedInventoryIndex] = default;
                selectedInventoryIndex = -1;
                ClearHandPreview();
                Refresh();
                return true;
            }

            animal = default;
            return false;
        }

        public bool TrySellSelectedAnimal(KickLuckyCubeWallet wallet, out KickLuckyCubeInventoryAnimal soldAnimal)
        {
            soldAnimal = default;
            if (wallet == null || !TryRemoveSelectedAnimal(out soldAnimal))
            {
                return false;
            }

            wallet.AddSoft(soldAnimal.SellValue);
            SetStatus($"Sold {soldAnimal.AnimalName} for ${soldAnimal.SellValue}.");
            return true;
        }

        public AnimalSlot[] GetSellableAnimals()
        {
            var count = HotbarAnimalCount + StoredAnimalCount;
            var result = new AnimalSlot[count];
            var writeIndex = 0;

            for (var index = 0; index < hotbarAnimals.Length; index++)
            {
                if (!hotbarAnimals[index].IsValid)
                {
                    continue;
                }

                result[writeIndex++] = new AnimalSlot(true, index, hotbarAnimals[index]);
            }

            for (var index = 0; index < inventoryAnimals.Length; index++)
            {
                if (!inventoryAnimals[index].IsValid)
                {
                    continue;
                }

                result[writeIndex++] = new AnimalSlot(false, index, inventoryAnimals[index]);
            }

            return result;
        }

        public bool TrySellAnimal(AnimalSlot slot, KickLuckyCubeWallet wallet, out KickLuckyCubeInventoryAnimal soldAnimal)
        {
            soldAnimal = default;
            if (wallet == null || !TryRemoveAnimal(slot, out soldAnimal))
            {
                return false;
            }

            wallet.AddSoft(soldAnimal.SellValue);
            SetStatus($"Sold {soldAnimal.AnimalName} for ${soldAnimal.SellValue}.");
            return true;
        }

        public bool CanDragFrom(KickLuckyCubeInventorySlotView slot)
        {
            return slot != null
                && slot.Kind != KickLuckyCubeInventorySlotView.SlotKind.Tool
                && GetAnimal(slot).IsValid;
        }

        private bool TryRemoveAnimal(AnimalSlot slot, out KickLuckyCubeInventoryAnimal animal)
        {
            animal = default;
            if (!slot.IsValid)
            {
                return false;
            }

            var sourceArray = slot.IsHotbar ? hotbarAnimals : inventoryAnimals;
            if (slot.Index < 0 || slot.Index >= sourceArray.Length)
            {
                return false;
            }

            animal = sourceArray[slot.Index];
            if (!animal.IsValid || animal.Id != slot.Animal.Id)
            {
                return false;
            }

            sourceArray[slot.Index] = default;
            if (slot.IsHotbar && selectedHotbarIndex == slot.Index)
            {
                selectedHotbarIndex = -1;
                ClearHandPreview();
            }
            else if (!slot.IsHotbar && selectedInventoryIndex == slot.Index)
            {
                selectedInventoryIndex = -1;
                ClearHandPreview();
            }

            Refresh();
            return true;
        }

        public void HandleSlotClicked(KickLuckyCubeInventorySlotView slot)
        {
            if (slot == null)
            {
                return;
            }

            switch (slot.Kind)
            {
                case KickLuckyCubeInventorySlotView.SlotKind.Tool:
                    ToggleToolTraining();
                    break;
                case KickLuckyCubeInventorySlotView.SlotKind.HotbarAnimal:
                    if (selectedHotbarIndex == slot.Index && selectedInventoryIndex < 0)
                    {
                        ClearSelectedAnimal();
                        break;
                    }

                    SelectHotbarAnimal(slot.Index);
                    break;
                case KickLuckyCubeInventorySlotView.SlotKind.InventoryAnimal:
                    if (selectedInventoryIndex == slot.Index && selectedHotbarIndex < 0)
                    {
                        ClearSelectedAnimal();
                        break;
                    }

                    SelectInventoryAnimal(slot.Index);
                    break;
            }
        }

        public void BeginDrag(KickLuckyCubeInventorySlotView source, PointerEventData eventData)
        {
            if (!CanDragFrom(source))
            {
                return;
            }

            CreateDragGhost(GetAnimal(source));
            UpdateDrag(eventData);
        }

        public void UpdateDrag(PointerEventData eventData)
        {
            if (dragGhostRect == null || eventData == null || canvasRect == null)
            {
                return;
            }

            var cameraForCanvas = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    eventData.position,
                    cameraForCanvas,
                    out var localPosition))
            {
                dragGhostRect.anchoredPosition = localPosition;
            }
        }

        public void EndDrag(KickLuckyCubeInventorySlotView source, PointerEventData eventData)
        {
            var destination = eventData?.pointerEnter != null
                ? eventData.pointerEnter.GetComponentInParent<KickLuckyCubeInventorySlotView>()
                : null;

            if (destination != null)
            {
                TryMoveAnimal(source, destination);
            }
            else
            {
                var dropZone = eventData?.pointerEnter != null
                    ? eventData.pointerEnter.GetComponentInParent<KickLuckyCubeInventoryDropZone>()
                    : null;
                dropZone?.TryAppendFrom(source);
            }

            DestroyDragGhost();
        }

        public void ToggleWindow()
        {
            SetWindowOpen(!IsWindowOpen);
        }

        public void SetWindowOpen(bool open)
        {
            if (inventoryWindowRoot == null)
            {
                return;
            }

            inventoryWindowRoot.gameObject.SetActive(open);
            if (open)
            {
                inventoryWindowRoot.SetAsLastSibling();
            }

            Refresh();
        }

        private void BuildRuntimeUi()
        {
            if (canvas == null || canvasRect == null)
            {
                return;
            }

            if (hotbarRoot == null)
            {
                hotbarRoot = CreateHotbar();
            }

            if (inventoryWindowRoot == null)
            {
                inventoryWindowRoot = CreateInventoryWindow();
                inventoryWindowRoot.gameObject.SetActive(false);
            }
        }

        private RectTransform CreateHotbar()
        {
            var root = CreateRect("KLC_InventoryHotbar_Runtime", canvas.transform);
            root.anchorMin = new Vector2(0.5f, 0f);
            root.anchorMax = new Vector2(0.5f, 0f);
            root.pivot = new Vector2(0.5f, 0f);
            root.anchoredPosition = new Vector2(0f, 18f);
            root.sizeDelta = new Vector2(500f, 82f);

            var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = 8f;

            toolSlot = CreateSlot(
                root,
                KickLuckyCubeInventorySlotView.SlotKind.Tool,
                -1,
                new Vector2(76f, 76f),
                true);

            hotbarSlots = new KickLuckyCubeInventorySlotView[HotbarAnimalSlotCount];
            for (var index = 0; index < hotbarSlots.Length; index++)
            {
                hotbarSlots[index] = CreateSlot(
                    root,
                    KickLuckyCubeInventorySlotView.SlotKind.HotbarAnimal,
                    index,
                    new Vector2(76f, 76f),
                    true);
            }

            var inventoryButton = CreateButton(root, "KLC_InventoryToggleButton", "Bag\nI", new Vector2(58f, 76f));
            inventoryButton.onClick.AddListener(ToggleWindow);
            return root;
        }

        private RectTransform CreateInventoryWindow()
        {
            var root = CreateRect("KLC_InventoryWindow_Runtime", canvas.transform);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.anchoredPosition = new Vector2(0f, 8f);
            root.sizeDelta = new Vector2(620f, 350f);
            AddImage(root.gameObject, new Color(0.04f, 0.03f, 0.025f, 0.76f));

            CreateLabel(root, "KLC_InventoryTitle", "Все предметы", 28, TextAnchor.MiddleLeft, new Vector2(190f, 36f), new Vector2(-205f, 148f));
            CreateLabel(root, "KLC_InventorySearch", "Поиск...", 26, TextAnchor.MiddleRight, new Vector2(170f, 36f), new Vector2(210f, 148f));
            statusText = CreateLabel(root, "KLC_InventoryStatus", "Drag mobs between slots.", 15, TextAnchor.MiddleLeft, new Vector2(350f, 24f), new Vector2(-112f, -148f));

            var closeButton = CreateButton(root, "KLC_InventoryCloseButton", "X", new Vector2(42f, 32f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(286f, 148f);
            closeButton.onClick.AddListener(() => SetWindowOpen(false));

            CreateCategoryButton(root, "Все\nпредметы", new Vector2(-270f, 105f));
            CreateCategoryButton(root, "Мобы", new Vector2(-270f, 52f));
            CreateCategoryButton(root, "Блоки", new Vector2(-270f, -1f));

            var grid = CreateRect("KLC_InventoryGrid", root);
            grid.anchorMin = new Vector2(0f, 0f);
            grid.anchorMax = new Vector2(1f, 1f);
            grid.offsetMin = new Vector2(92f, 44f);
            grid.offsetMax = new Vector2(-24f, -58f);
            var gridImage = AddImage(grid.gameObject, new Color(0f, 0f, 0f, 0f));
            gridImage.raycastTarget = true;
            inventoryDropZone = grid.gameObject.AddComponent<KickLuckyCubeInventoryDropZone>();
            inventoryDropZone.Configure(this);

            var gridLayout = grid.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(76f, 76f);
            gridLayout.spacing = new Vector2(8f, 8f);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 6;

            inventorySlots = new KickLuckyCubeInventorySlotView[inventoryAnimals.Length];
            for (var index = 0; index < inventorySlots.Length; index++)
            {
                inventorySlots[index] = CreateSlot(
                    grid,
                    KickLuckyCubeInventorySlotView.SlotKind.InventoryAnimal,
                    index,
                    new Vector2(76f, 76f),
                    false);
            }

            return root;
        }

        private KickLuckyCubeInventorySlotView CreateSlot(
            RectTransform parent,
            KickLuckyCubeInventorySlotView.SlotKind kind,
            int index,
            Vector2 size,
            bool compact)
        {
            var slotRoot = CreateRect($"KLC_InventorySlot_{kind}_{index}", parent);
            slotRoot.sizeDelta = size;
            var layoutElement = slotRoot.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = size.x;
            layoutElement.preferredHeight = size.y;

            var frameImage = AddImage(slotRoot.gameObject, emptyFrameColor);
            frameImage.raycastTarget = true;

            var icon = CreateRect("Icon", slotRoot);
            icon.anchorMin = new Vector2(0.5f, 0.5f);
            icon.anchorMax = new Vector2(0.5f, 0.5f);
            icon.pivot = new Vector2(0.5f, 0.5f);
            icon.anchoredPosition = new Vector2(0f, compact ? 7f : 8f);
            icon.sizeDelta = compact ? new Vector2(46f, 38f) : new Vector2(48f, 38f);
            var iconImage = AddImage(icon.gameObject, Color.white);
            iconImage.raycastTarget = false;

            var title = CreateLabel(slotRoot, "Title", string.Empty, compact ? 12 : 11, TextAnchor.LowerCenter, new Vector2(size.x - 6f, 24f), new Vector2(0f, -26f));
            var detail = CreateLabel(slotRoot, "Detail", string.Empty, 9, TextAnchor.UpperCenter, new Vector2(size.x - 8f, 26f), new Vector2(0f, -6f));
            var badge = CreateLabel(slotRoot, "Badge", string.Empty, 9, TextAnchor.MiddleCenter, new Vector2(size.x - 6f, 18f), new Vector2(0f, size.y * 0.5f - 12f));

            var view = slotRoot.gameObject.AddComponent<KickLuckyCubeInventorySlotView>();
            view.Configure(this, kind, index, frameImage, iconImage, title, detail, badge);
            return view;
        }

        private void CreateCategoryButton(RectTransform parent, string label, Vector2 position)
        {
            var button = CreateButton(parent, "KLC_InventoryCategory_" + label.Replace("\n", "_"), label, new Vector2(70f, 44f));
            button.GetComponent<RectTransform>().anchoredPosition = position;
        }

        private Button CreateButton(RectTransform parent, string name, string label, Vector2 size)
        {
            var rect = CreateRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            var image = AddImage(rect.gameObject, new Color(0.08f, 0.08f, 0.08f, 0.86f));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            CreateLabel(rect, "Label", label, 14, TextAnchor.MiddleCenter, size, Vector2.zero);
            return button;
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
            var rect = CreateRect(name, parent);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            var label = rect.gameObject.AddComponent<Text>();
            label.font = uiFont;
            label.fontSize = fontSize;
            label.alignment = anchor;
            label.color = Color.white;
            label.text = text;
            label.raycastTarget = false;
            return label;
        }

        private void Refresh()
        {
            if (toolSlot != null)
            {
                var selectedTier = stats != null ? stats.SelectedStrengthToolTier : 1;
                var ownedTier = stats != null ? stats.StrengthToolTier : 1;
                ResolveToolTraining();
                var isTraining = toolTraining != null && toolTraining.IsTraining;
                var detail = isTraining && toolTraining != null
                    ? $"+{toolTraining.CurrentStrengthPerSecond:0}/s"
                    : $"Owned {ownedTier}";
                var title = toolTraining != null ? toolTraining.CurrentToolName : $"Tool {selectedTier}";
                toolSlot.SetTool(title, detail, isTraining ? "Training" : "Tool", isTraining ? selectedFrameColor : toolFrameColor, new Color(0.72f, 0.72f, 0.9f));
            }

            for (var index = 0; index < hotbarSlots.Length; index++)
            {
                if (hotbarSlots[index] == null)
                {
                    continue;
                }

                hotbarSlots[index].gameObject.SetActive(IsWindowOpen || hotbarAnimals[index].IsValid);
                hotbarSlots[index].SetAnimal(
                    hotbarAnimals[index],
                    selectedHotbarIndex == index,
                    selectedHotbarIndex == index ? selectedFrameColor : animalFrameColor,
                    emptyFrameColor);
            }

            for (var index = 0; index < inventorySlots.Length; index++)
            {
                if (inventorySlots[index] == null)
                {
                    continue;
                }

                inventorySlots[index].gameObject.SetActive(inventoryAnimals[index].IsValid);
                inventorySlots[index].SetAnimal(
                    inventoryAnimals[index],
                    selectedInventoryIndex == index,
                    selectedInventoryIndex == index ? selectedFrameColor : animalFrameColor,
                    emptyFrameColor);
            }
        }

        private bool TryMoveAnimal(KickLuckyCubeInventorySlotView source, KickLuckyCubeInventorySlotView destination)
        {
            if (source == null
                || destination == null
                || source == destination
                || source.Kind == KickLuckyCubeInventorySlotView.SlotKind.Tool
                || destination.Kind == KickLuckyCubeInventorySlotView.SlotKind.Tool)
            {
                return false;
            }

            if (!TryGetLocation(source, out var sourceArray, out var sourceIndex)
                || !TryGetLocation(destination, out var destinationArray, out var destinationIndex)
                || !sourceArray[sourceIndex].IsValid)
            {
                return false;
            }

            (sourceArray[sourceIndex], destinationArray[destinationIndex]) =
                (destinationArray[destinationIndex], sourceArray[sourceIndex]);

            if (destination.Kind == KickLuckyCubeInventorySlotView.SlotKind.HotbarAnimal)
            {
                SelectHotbarAnimal(destination.Index);
            }
            else
            {
                SelectInventoryAnimal(destination.Index);
            }

            SetStatus("Moved item.");
            Refresh();
            return true;
        }

        public bool TryMoveAnimalToInventory(KickLuckyCubeInventorySlotView source)
        {
            if (source == null
                || source.Kind == KickLuckyCubeInventorySlotView.SlotKind.Tool
                || source.Kind == KickLuckyCubeInventorySlotView.SlotKind.InventoryAnimal
                || !TryGetLocation(source, out var sourceArray, out var sourceIndex)
                || !sourceArray[sourceIndex].IsValid)
            {
                return false;
            }

            for (var index = 0; index < inventoryAnimals.Length; index++)
            {
                if (inventoryAnimals[index].IsValid)
                {
                    continue;
                }

                inventoryAnimals[index] = sourceArray[sourceIndex];
                sourceArray[sourceIndex] = default;
                SelectInventoryAnimal(index);
                SetStatus("Moved item to inventory.");
                Refresh();
                return true;
            }

            SetStatus("Inventory is full.");
            return false;
        }

        private KickLuckyCubeInventoryAnimal GetAnimal(KickLuckyCubeInventorySlotView slot)
        {
            return TryGetLocation(slot, out var sourceArray, out var sourceIndex)
                ? sourceArray[sourceIndex]
                : default;
        }

        private bool TryGetLocation(
            KickLuckyCubeInventorySlotView slot,
            out KickLuckyCubeInventoryAnimal[] sourceArray,
            out int sourceIndex)
        {
            sourceArray = null;
            sourceIndex = -1;

            if (slot == null)
            {
                return false;
            }

            switch (slot.Kind)
            {
                case KickLuckyCubeInventorySlotView.SlotKind.HotbarAnimal:
                    sourceArray = hotbarAnimals;
                    sourceIndex = slot.Index;
                    break;
                case KickLuckyCubeInventorySlotView.SlotKind.InventoryAnimal:
                    sourceArray = inventoryAnimals;
                    sourceIndex = slot.Index;
                    break;
            }

            return sourceArray != null && sourceIndex >= 0 && sourceIndex < sourceArray.Length;
        }

        private void SelectHotbarAnimal(int index)
        {
            if (index < 0 || index >= hotbarAnimals.Length || !hotbarAnimals[index].IsValid)
            {
                return;
            }

            selectedHotbarIndex = index;
            selectedInventoryIndex = -1;
            SetStatus($"Selected {hotbarAnimals[index].AnimalName}.");
            ShowHandPreview(hotbarAnimals[index]);
            Refresh();
        }

        private void SelectInventoryAnimal(int index)
        {
            if (index < 0 || index >= inventoryAnimals.Length || !inventoryAnimals[index].IsValid)
            {
                return;
            }

            selectedInventoryIndex = index;
            selectedHotbarIndex = -1;
            SetStatus($"Selected {inventoryAnimals[index].AnimalName}.");
            ShowHandPreview(inventoryAnimals[index]);
            Refresh();
        }

        private void ClearSelectedAnimal()
        {
            selectedHotbarIndex = -1;
            selectedInventoryIndex = -1;
            ClearHandPreview();
            SetStatus("Mob selection cleared.");
            Refresh();
        }

        private void ToggleToolTraining()
        {
            ResolveToolTraining();
            if (toolTraining == null)
            {
                return;
            }

            if (!toolTraining.IsTraining)
            {
                selectedHotbarIndex = -1;
                selectedInventoryIndex = -1;
                ClearHandPreview();
            }

            toolTraining.ToggleTraining();
            SetStatus(toolTraining.IsTraining
                ? $"Training with {toolTraining.CurrentToolName}: +{toolTraining.CurrentStrengthPerSecond:0}/s."
                : "Training stopped.");
            Refresh();
        }

        private void ResolveToolTraining()
        {
            toolTraining ??= FindFirstObjectByType<KickLuckyCubeToolTrainingController>(FindObjectsInactive.Include);
            SubscribeToolTraining();
        }

        private void SubscribeToolTraining()
        {
            if (toolTraining == null || subscribedToToolTraining)
            {
                return;
            }

            toolTraining.Changed += Refresh;
            subscribedToToolTraining = true;
        }

        private void UnsubscribeToolTraining()
        {
            if (toolTraining == null || !subscribedToToolTraining)
            {
                return;
            }

            toolTraining.Changed -= Refresh;
            subscribedToToolTraining = false;
        }

        private void ShowHandPreview(KickLuckyCubeInventoryAnimal animal)
        {
            if (!animal.IsValid)
            {
                ClearHandPreview();
                return;
            }

            ResolveCarryAnchor();
            if (carryAnchor == null)
            {
                return;
            }

            if (selectedAnimalHandPreview == null)
            {
                selectedAnimalHandPreview = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                selectedAnimalHandPreview.name = "KLC_SelectedMobHandPreview";
                selectedAnimalHandPreview.transform.SetParent(carryAnchor, false);
                selectedAnimalHandPreview.transform.localPosition = Vector3.zero;
                selectedAnimalHandPreview.transform.localRotation = Quaternion.identity;
                selectedAnimalHandPreview.transform.localScale = handPreviewScale;
                selectedAnimalHandRenderer = selectedAnimalHandPreview.GetComponent<Renderer>();

                var previewCollider = selectedAnimalHandPreview.GetComponent<Collider>();
                if (previewCollider != null)
                {
                    Destroy(previewCollider);
                }
            }

            selectedAnimalHandPreview.SetActive(true);
            selectedAnimalHandPreview.transform.SetParent(carryAnchor, false);
            selectedAnimalHandPreview.transform.localPosition = Vector3.zero;
            selectedAnimalHandPreview.transform.localRotation = Quaternion.identity;
            selectedAnimalHandPreview.transform.localScale = handPreviewScale;

            if (selectedAnimalHandRenderer == null)
            {
                selectedAnimalHandRenderer = selectedAnimalHandPreview.GetComponent<Renderer>();
            }

            if (selectedAnimalHandRenderer != null)
            {
                var material = new Material(selectedAnimalHandRenderer.sharedMaterial);
                material.color = animal.BodyColor;
                selectedAnimalHandRenderer.sharedMaterial = material;
            }
        }

        private void ClearHandPreview()
        {
            selectedAnimalHandRenderer = null;
            if (selectedAnimalHandPreview == null)
            {
                return;
            }

            selectedAnimalHandPreview.SetActive(false);
            Destroy(selectedAnimalHandPreview);
            selectedAnimalHandPreview = null;
        }

        private void CreateDragGhost(KickLuckyCubeInventoryAnimal animal)
        {
            DestroyDragGhost();

            dragGhostRect = CreateRect("KLC_InventoryDragGhost", canvas.transform);
            dragGhostRect.sizeDelta = new Vector2(78f, 78f);
            dragGhostRect.SetAsLastSibling();
            dragGhost = dragGhostRect.gameObject;
            var group = dragGhost.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.alpha = 0.82f;

            dragGhostImage = AddImage(dragGhost, animal.BodyColor);
            dragGhostText = CreateLabel(dragGhostRect, "Label", animal.AnimalName, 11, TextAnchor.LowerCenter, new Vector2(76f, 24f), new Vector2(0f, -25f));
        }

        private void DestroyDragGhost()
        {
            dragGhostImage = null;
            dragGhostText = null;
            dragGhostRect = null;

            if (dragGhost == null)
            {
                return;
            }

            Destroy(dragGhost);
            dragGhost = null;
        }

        private void SetStatus(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }

        private void HideLegacyToolBelt()
        {
            var legacyBelts = FindObjectsByType<KickLuckyCubeToolBeltController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var legacyBelt in legacyBelts)
            {
                if (legacyBelt != null)
                {
                    legacyBelt.gameObject.SetActive(false);
                }
            }
        }

        private void ResolveCarryAnchor()
        {
            if (carryAnchor != null || string.IsNullOrWhiteSpace(carryAnchorName))
            {
                return;
            }

            var transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var foundTransform in transforms)
            {
                if (foundTransform != null && foundTransform.name == carryAnchorName)
                {
                    carryAnchor = foundTransform;
                    return;
                }
            }
        }

        private void ResolveUiFont()
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (uiFont == null)
            {
                uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static Image AddImage(GameObject target, Color color)
        {
            var image = target.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static int CountValid(KickLuckyCubeInventoryAnimal[] animals)
        {
            if (animals == null)
            {
                return 0;
            }

            var count = 0;
            foreach (var animal in animals)
            {
                if (animal.IsValid)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ReadInventoryTogglePressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.iKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.I);
#endif
        }

        private static int ReadHotbarNumber()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return 0;
            }

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) return 1;
            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) return 2;
            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) return 3;
            if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame) return 4;
            if (keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame) return 5;
            return 0;
#else
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 2;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 3;
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return 4;
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) return 5;
            return 0;
#endif
        }
    }
}
