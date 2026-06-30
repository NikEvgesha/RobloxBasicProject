using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeAnimalAlbumController : MonoBehaviour
    {
        private const float CardWidth = 154f;
        private const float CardHeight = 172f;
        private const float CardSpacing = 12f;
        private const int CardsPerRow = 4;

        [SerializeField] private Canvas canvas;
        [SerializeField] private bool buildRuntimeUi = true;
        [SerializeField] private Vector2 openButtonPosition = new(26f, -410f);

        private readonly List<GameObject> cardObjects = new();
        private RectTransform backdropRoot;
        private RectTransform windowRoot;
        private RectTransform listRoot;
        private RectTransform openButtonRoot;
        private ScrollRect scrollRect;
        private Text titleText;
        private Text statusText;
        private Font uiFont;

        public bool IsOpen => windowRoot != null && windowRoot.gameObject.activeSelf;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeAnimalAlbumController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_AnimalAlbum_Runtime").AddComponent<KickLuckyCubeAnimalAlbumController>();
        }

        private void Awake()
        {
            canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas(canvas);
            ResolveUiFont();

            if (buildRuntimeUi)
            {
                BuildRuntimeUi();
            }
        }

        private void OnEnable()
        {
            KickLuckyCubeAnimalCollection.Changed += RefreshCards;
        }

        private void OnDisable()
        {
            KickLuckyCubeAnimalCollection.Changed -= RefreshCards;
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
            BuildRuntimeUi();
            if (windowRoot == null)
            {
                return;
            }

            RefreshCards();
            if (backdropRoot != null)
            {
                backdropRoot.gameObject.SetActive(true);
                backdropRoot.SetAsLastSibling();
            }

            windowRoot.gameObject.SetActive(true);
            windowRoot.SetAsLastSibling();
        }

        public void CloseWindow()
        {
            if (backdropRoot != null)
            {
                backdropRoot.gameObject.SetActive(false);
            }

            if (windowRoot != null)
            {
                windowRoot.gameObject.SetActive(false);
            }
        }

        private void BuildRuntimeUi()
        {
            if (canvas == null)
            {
                canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas(canvas);
            }

            if (canvas == null)
            {
                return;
            }

            if (openButtonRoot == null)
            {
                var button = CreateButton(canvas.transform as RectTransform, "KLC_AnimalAlbumOpenButton", "Album", new Vector2(118f, 118f), new Color(0.13f, 0.40f, 0.62f, 0.94f));
                openButtonRoot = button.GetComponent<RectTransform>();
                openButtonRoot.anchorMin = new Vector2(0f, 1f);
                openButtonRoot.anchorMax = new Vector2(0f, 1f);
                openButtonRoot.pivot = new Vector2(0f, 1f);
                openButtonRoot.anchoredPosition = openButtonPosition;
                button.onClick.RemoveListener(OpenWindow);
                button.onClick.AddListener(OpenWindow);
            }

            if (windowRoot != null)
            {
                return;
            }

            backdropRoot = CreateRect("KLC_AnimalAlbumBackdrop_Runtime", canvas.transform);
            backdropRoot.anchorMin = Vector2.zero;
            backdropRoot.anchorMax = Vector2.one;
            backdropRoot.pivot = new Vector2(0.5f, 0.5f);
            backdropRoot.offsetMin = Vector2.zero;
            backdropRoot.offsetMax = Vector2.zero;
            var backdropImage = AddImage(backdropRoot.gameObject, new Color(0f, 0f, 0f, 0.42f));
            var backdropButton = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<Button>(backdropRoot.gameObject);
            backdropButton.targetGraphic = backdropImage;
            backdropButton.onClick.RemoveAllListeners();
            backdropButton.onClick.AddListener(CloseWindow);

            windowRoot = CreateRect("KLC_AnimalAlbumWindow_Runtime", canvas.transform);
            windowRoot.anchorMin = new Vector2(0.5f, 0.5f);
            windowRoot.anchorMax = new Vector2(0.5f, 0.5f);
            windowRoot.pivot = new Vector2(0.5f, 0.5f);
            windowRoot.anchoredPosition = new Vector2(0f, 14f);
            windowRoot.sizeDelta = new Vector2(760f, 560f);
            AddImage(windowRoot.gameObject, new Color(0.045f, 0.04f, 0.055f, 0.95f));

            titleText = CreateLabel(windowRoot, "Title", "Mob Album", 36, TextAnchor.MiddleLeft, new Vector2(360f, 48f), new Vector2(-160f, 232f));
            statusText = CreateLabel(windowRoot, "Status", string.Empty, 18, TextAnchor.MiddleLeft, new Vector2(560f, 30f), new Vector2(-42f, -246f));

            var closeButton = CreateButton(windowRoot, "CloseButton", "X", new Vector2(44f, 36f), new Color(0.88f, 0.08f, 0.15f, 0.96f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(344f, 232f);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseWindow);

            var viewport = CreateRect("KLC_AnimalAlbumViewport", windowRoot);
            viewport.anchorMin = new Vector2(0.5f, 0.5f);
            viewport.anchorMax = new Vector2(0.5f, 0.5f);
            viewport.pivot = new Vector2(0.5f, 0.5f);
            viewport.anchoredPosition = new Vector2(0f, -8f);
            viewport.sizeDelta = new Vector2(672f, 420f);
            AddImage(viewport.gameObject, new Color(0.015f, 0.016f, 0.022f, 0.42f));
            KickLuckyCubeUiPrefabFactory.GetOrAddComponent<RectMask2D>(viewport.gameObject);

            listRoot = CreateRect("KLC_AnimalAlbumGrid", viewport);
            listRoot.anchorMin = new Vector2(0.5f, 1f);
            listRoot.anchorMax = new Vector2(0.5f, 1f);
            listRoot.pivot = new Vector2(0.5f, 1f);
            listRoot.anchoredPosition = new Vector2(0f, -10f);
            listRoot.sizeDelta = new Vector2(CardsPerRow * CardWidth + (CardsPerRow - 1) * CardSpacing, 0f);

            var grid = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<GridLayoutGroup>(listRoot.gameObject);
            grid.cellSize = new Vector2(CardWidth, CardHeight);
            grid.spacing = new Vector2(CardSpacing, CardSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = CardsPerRow;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(0, 0, 0, 44);

            var fitter = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<ContentSizeFitter>(listRoot.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<ScrollRect>(windowRoot.gameObject);
            scrollRect.viewport = viewport;
            scrollRect.content = listRoot;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 32f;

            backdropRoot.gameObject.SetActive(false);
            windowRoot.gameObject.SetActive(false);
            RefreshCards();
        }

        private void RefreshCards()
        {
            if (listRoot == null)
            {
                return;
            }

            ClearCards();
            var entries = KickLuckyCubeAnimalCatalog.GetAll();
            Array.Sort(entries, CompareEntries);

            foreach (var entry in entries)
            {
                CreateCard(entry, KickLuckyCubeAnimalCollection.IsDiscovered(entry));
            }

            var discovered = KickLuckyCubeAnimalCollection.CountDiscovered();
            if (titleText != null)
            {
                titleText.text = $"Mob Album  {discovered}/{entries.Length}";
            }

            if (statusText != null)
            {
                statusText.text = "Kick cubes, trade, buy exclusives, and claim gifts to reveal silhouettes.";
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(listRoot);
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        private void CreateCard(KickLuckyCubeAnimalCatalogEntry entry, bool discovered)
        {
            var card = CreateRect("KLC_AlbumCard_" + entry.CatalogId, listRoot);
            card.sizeDelta = new Vector2(CardWidth, CardHeight);
            var layout = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<LayoutElement>(card.gameObject);
            layout.preferredWidth = CardWidth;
            layout.preferredHeight = CardHeight;

            var frameColor = KickLuckyCubeUiTheme.CardColorForRarity(entry.Rarity, discovered);
            AddImage(card.gameObject, frameColor);
            cardObjects.Add(card.gameObject);

            var icon = CreateRect("Icon", card);
            icon.anchorMin = new Vector2(0.5f, 1f);
            icon.anchorMax = new Vector2(0.5f, 1f);
            icon.pivot = new Vector2(0.5f, 1f);
            icon.anchoredPosition = new Vector2(0f, -14f);
            icon.sizeDelta = new Vector2(88f, 72f);
            var iconImage = AddImage(icon.gameObject, discovered ? entry.BodyColor : new Color(0.025f, 0.025f, 0.03f, 1f));
            var iconSprite = KickLuckyCubeAnimalCatalog.LoadIcon(entry);
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
                iconImage.preserveAspect = true;
                iconImage.color = discovered ? Color.white : new Color(0f, 0f, 0f, 0.9f);
            }

            if (iconSprite == null)
            {
                var silhouette = CreateLabel(icon, "Shape", discovered ? "O" : "?", 52, TextAnchor.MiddleCenter, icon.sizeDelta, Vector2.zero);
                silhouette.color = discovered ? new Color(1f, 1f, 1f, 0.22f) : new Color(0f, 0f, 0f, 0.92f);
            }

            CreateLabel(card, "Name", discovered ? entry.AnimalName : "???", 17, TextAnchor.MiddleCenter, new Vector2(136f, 30f), new Vector2(0f, 18f));
            CreateLabel(card, "Rarity", entry.Rarity.ToString(), 14, TextAnchor.MiddleCenter, new Vector2(136f, 24f), new Vector2(0f, -10f));
            CreateLabel(
                card,
                "Stats",
                discovered ? $"+{entry.IncomePerSecond}/s  Sell {entry.SellValue}" : "Not found",
                13,
                TextAnchor.MiddleCenter,
                new Vector2(136f, 24f),
                new Vector2(0f, -38f));
            CreateLabel(card, "Source", FormatSource(entry.Source), 12, TextAnchor.MiddleCenter, new Vector2(136f, 24f), new Vector2(0f, -65f));
        }

        private void ClearCards()
        {
            foreach (var cardObject in cardObjects)
            {
                if (cardObject != null)
                {
                    Destroy(cardObject);
                }
            }

            cardObjects.Clear();
        }

        private Button CreateButton(RectTransform parent, string name, string label, Vector2 size, Color color)
        {
            return KickLuckyCubeUiPrefabFactory.GetOrCreateButton(parent, name, label, uiFont, size, color, 15);
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

        private static int CompareEntries(KickLuckyCubeAnimalCatalogEntry left, KickLuckyCubeAnimalCatalogEntry right)
        {
            var rarityCompare = left.Rarity.CompareTo(right.Rarity);
            return rarityCompare != 0
                ? rarityCompare
                : string.Compare(left.AnimalName, right.AnimalName, StringComparison.Ordinal);
        }

        private static string FormatSource(KickLuckyCubeAnimalSource source)
        {
            return source switch
            {
                KickLuckyCubeAnimalSource.Kick => "Lucky Cube",
                KickLuckyCubeAnimalSource.EpicShop => "Elite Shop",
                KickLuckyCubeAnimalSource.RatingGift => "Rating Gift",
                KickLuckyCubeAnimalSource.Exchange => "Exchange",
                _ => source.ToString(),
            };
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
