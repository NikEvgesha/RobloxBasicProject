using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public enum KickLuckyCubeFutureFeature
    {
        WeatherMachine,
        ExchangeBooth,
        EpicMobShop,
        RatingGift
    }

    public sealed class KickLuckyCubeFutureFeatureController : MonoBehaviour
    {
        private const BindingFlags SerializedFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;
        private const string WeatherEndsAtKey = "KickLuckyCube.Future.WeatherEndsAt";
        private const string ExchangeChargesKey = "KickLuckyCube.Future.ExchangeCharges";
        private const string ExchangeLastAtKey = "KickLuckyCube.Future.ExchangeLastAt";
        private const string RatingGiftClaimedKey = "KickLuckyCube.Future.RatingGiftClaimed";
        private const string EpicMobBoughtKeyPrefix = "KickLuckyCube.Future.EpicMobBought.";

        private static readonly KickLuckyCubeInventoryAnimal[] EpicShopAnimals =
        {
            new("epic_shop_crystal_griffin", "Crystal Griffin", KickLuckyCubeRarity.Epic, new Color(0.24f, 0.86f, 1f), 3200, 115),
            new("epic_shop_neon_hydra", "Neon Hydra", KickLuckyCubeRarity.Legendary, new Color(0.72f, 0.22f, 1f), 5200, 185),
            new("epic_shop_sun_kaiju", "Sun Kaiju", KickLuckyCubeRarity.Legendary, new Color(1f, 0.66f, 0.08f), 7600, 260),
        };

        private static readonly int[] EpicShopCosts = { 1800, 4200, 8600 };

        private static KickLuckyCubeFutureFeatureController instance;

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeInventoryController inventory;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private Canvas canvas;
        [SerializeField] private string futureSpotsRootName = "KLC_FutureFeatureSpots";
        [SerializeField, Min(1)] private int maxExchangeCharges = 10;
        [SerializeField, Min(1f)] private float exchangeRechargeSeconds = 300f;
        [SerializeField, Min(30f)] private float weatherDurationSeconds = 600f;
        [SerializeField, Range(0f, 1f)] private float weatherBonusChance = 0.35f;
        [SerializeField, Min(0f)] private float statusSeconds = 3.2f;
        [SerializeField] private Vector3 interactionTriggerSize = new(3.0f, 2.6f, 2.4f);

        private RectTransform epicShopWindow;
        private RectTransform exchangeWindow;
        private Text statusText;
        private Text weatherLabel;
        private Text exchangeLabel;
        private Text exchangeStatusText;
        private Button exchangeButton;
        private Text epicShopStatusText;
        private Button[] epicButtons = Array.Empty<Button>();
        private Text[] epicButtonTexts = Array.Empty<Text>();
        private Image[] epicCardFrames = Array.Empty<Image>();
        private Font uiFont;
        private int exchangeCharges;
        private double exchangeLastAtUnix;
        private double weatherEndsAtUnix;
        private float statusHideAt;
        private float refreshTimer;

        public static bool HasActiveWeather => instance != null && instance.IsWeatherActive;
        public bool IsWeatherActive => GetUnixNow() < weatherEndsAtUnix;
        public float RemainingWeatherSeconds => Mathf.Max(0f, (float)(weatherEndsAtUnix - GetUnixNow()));
        public int ExchangeCharges => exchangeCharges;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeFutureFeatureController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_FutureFeatureController_Runtime").AddComponent<KickLuckyCubeFutureFeatureController>();
        }

        public static KickLuckyCubeKickResult TryApplyWeatherBonus(KickLuckyCubeKickResult result)
        {
            return instance != null ? instance.ApplyWeatherBonus(result) : result;
        }

        private void Awake()
        {
            instance = this;
            ResolveReferences();
            LoadState();
            RechargeExchangeCharges();
            BuildRuntimeUi();
            ConfigureFutureSpots();
            RefreshAll();
            CloseEpicShop();
            CloseExchangeWindow();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Update()
        {
            refreshTimer += Time.unscaledDeltaTime;
            if (refreshTimer >= 1f)
            {
                refreshTimer = 0f;
                RechargeExchangeCharges();
                RefreshAll();
            }

            var statusPanel = statusText != null ? statusText.transform.parent.gameObject : null;
            if (statusPanel != null && statusPanel.activeSelf && Time.unscaledTime >= statusHideAt)
            {
                statusPanel.SetActive(false);
            }
        }

        public bool CanUseFeature(KickLuckyCubeFutureFeature feature)
        {
            ResolveReferences();
            return runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal && !runPhase.IsSelectingAnimal);
        }

        public void Interact(KickLuckyCubeFutureFeature feature, GameObject actor)
        {
            switch (feature)
            {
                case KickLuckyCubeFutureFeature.WeatherMachine:
                    ActivateWeather();
                    break;
                case KickLuckyCubeFutureFeature.ExchangeBooth:
                    OpenExchangeWindow();
                    break;
                case KickLuckyCubeFutureFeature.EpicMobShop:
                    OpenEpicShop();
                    break;
                case KickLuckyCubeFutureFeature.RatingGift:
                    ClaimRatingGift();
                    break;
            }
        }

        public void CloseEpicShop()
        {
            if (epicShopWindow != null)
            {
                epicShopWindow.gameObject.SetActive(false);
            }
        }

        public void CloseExchangeWindow()
        {
            if (exchangeWindow != null)
            {
                exchangeWindow.gameObject.SetActive(false);
            }
        }

        private void ActivateWeather()
        {
            ResolveReferences();
            if (IsWeatherActive)
            {
                ShowStatus($"Weather machine already active: {FormatTime(RemainingWeatherSeconds)} left.");
                return;
            }

            if (!HasWeatherKeyMob())
            {
                ShowStatus("Weather machine needs an Epic or Legendary mob in inventory or stable.");
                return;
            }

            weatherEndsAtUnix = GetUnixNow() + weatherDurationSeconds;
            PlayerPrefs.SetString(WeatherEndsAtKey, weatherEndsAtUnix.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
            ShowStatus($"Weather machine active for {FormatTime(weatherDurationSeconds)}.\nLucky cubes can boost rarity.");
            RefreshAll();
        }

        private void OpenExchangeWindow()
        {
            ResolveReferences();
            RechargeExchangeCharges();
            if (exchangeWindow == null)
            {
                BuildRuntimeUi();
            }

            if (exchangeWindow == null)
            {
                ShowStatus("Exchange UI is not ready.");
                return;
            }

            exchangeWindow.gameObject.SetActive(true);
            exchangeWindow.SetAsLastSibling();
            RefreshExchangeWindow();
        }

        private void ExchangeSelectedAnimal()
        {
            ResolveReferences();
            RechargeExchangeCharges();

            if (inventory == null || !inventory.TryGetSelectedAnimal(out var selectedAnimal))
            {
                ShowStatus("Select a mob first, then use the exchange booth.");
                return;
            }

            if (exchangeCharges <= 0)
            {
                ShowStatus($"No exchange charges.\nNext charge in {FormatTime(GetNextExchangeChargeSeconds())}.");
                return;
            }

            if (!inventory.TryRemoveSelectedAnimal(out var removedAnimal))
            {
                ShowStatus("Could not remove selected mob for exchange.");
                return;
            }

            var replacement = CreateExchangeReplacement(removedAnimal);
            if (!inventory.TryAddAnimal(replacement, true))
            {
                inventory.TryAddAnimal(removedAnimal, true);
                ShowStatus("Inventory is full. Exchange cancelled.");
                return;
            }

            exchangeCharges = Mathf.Max(0, exchangeCharges - 1);
            exchangeLastAtUnix = GetUnixNow();
            SaveExchangeState();
            ShowStatus($"Exchanged {removedAnimal.AnimalName} -> {replacement.AnimalName}.\nCharges: {exchangeCharges}/{maxExchangeCharges}");
            RefreshAll();
            RefreshExchangeWindow();
        }

        private void OpenEpicShop()
        {
            ResolveReferences();
            if (epicShopWindow == null)
            {
                BuildRuntimeUi();
            }

            if (epicShopWindow == null)
            {
                ShowStatus("Epic mob shop UI is not ready.");
                return;
            }

            epicShopWindow.gameObject.SetActive(true);
            epicShopWindow.SetAsLastSibling();
            SetEpicShopStatus("Three exclusive mobs can only be bought here.");
            RefreshEpicShop();
        }

        private void BuyEpicMob(int index)
        {
            ResolveReferences();
            if (index < 0 || index >= EpicShopAnimals.Length)
            {
                return;
            }

            if (IsEpicMobBought(index))
            {
                SetEpicShopStatus($"{EpicShopAnimals[index].AnimalName} is already owned.");
                return;
            }

            if (wallet == null || wallet.SoftCurrency < EpicShopCosts[index])
            {
                SetEpicShopStatus($"Need {EpicShopCosts[index]} soft for {EpicShopAnimals[index].AnimalName}.");
                RefreshEpicShop();
                return;
            }

            if (inventory == null || !inventory.TryAddAnimal(EpicShopAnimals[index], true))
            {
                SetEpicShopStatus("Inventory is full. Make room before buying.");
                RefreshEpicShop();
                return;
            }

            if (!wallet.TrySpendSoft(EpicShopCosts[index]))
            {
                inventory.TryRemoveAnimalById(EpicShopAnimals[index].Id, out _);
                SetEpicShopStatus("Purchase failed. Try again.");
                RefreshEpicShop();
                return;
            }

            PlayerPrefs.SetInt(EpicMobBoughtKeyPrefix + index, 1);
            PlayerPrefs.Save();
            SetEpicShopStatus($"{EpicShopAnimals[index].AnimalName} added and selected.");
            ShowStatus($"{EpicShopAnimals[index].AnimalName} bought!");
            RefreshEpicShop();
        }

        private void ClaimRatingGift()
        {
            ResolveReferences();
            if (PlayerPrefs.GetInt(RatingGiftClaimedKey, 0) != 0)
            {
                ShowStatus("Rating gift already claimed.");
                return;
            }

            var gift = new KickLuckyCubeInventoryAnimal(
                "rating_gift_star_buddy",
                "Star Review Buddy",
                KickLuckyCubeRarity.Epic,
                new Color(1f, 0.92f, 0.26f),
                2400,
                95);

            if (inventory == null || !inventory.TryAddAnimal(gift, true))
            {
                ShowStatus("Inventory is full. Free rating gift is waiting.");
                return;
            }

            PlayerPrefs.SetInt(RatingGiftClaimedKey, 1);
            PlayerPrefs.Save();
            ShowStatus("Thanks for rating!\nStar Review Buddy added.");
            RefreshAll();
        }

        private KickLuckyCubeKickResult ApplyWeatherBonus(KickLuckyCubeKickResult result)
        {
            if (!IsWeatherActive || UnityEngine.Random.value > weatherBonusChance)
            {
                return result;
            }

            var boostedRarity = BoostRarity(result.Rarity);
            if (boostedRarity == result.Rarity)
            {
                return result;
            }

            ShowStatus($"Weather cube! {result.Rarity} -> {boostedRarity}");
            return new KickLuckyCubeKickResult(
                result.Distance,
                result.KickOriginPosition,
                result.LandingPosition,
                result.Zone,
                boostedRarity,
                string.IsNullOrWhiteSpace(result.AnimalPoolText)
                    ? "Weather boosted pool"
                    : result.AnimalPoolText + " + Weather boost");
        }

        private KickLuckyCubeInventoryAnimal CreateExchangeReplacement(KickLuckyCubeInventoryAnimal removedAnimal)
        {
            var options = KickLuckyCubeAnimalSpawner.CreateDefaultOptions()
                .Where(option => !string.Equals(option.AnimalName, removedAnimal.AnimalName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(option => Mathf.Abs(option.IncomePerSecond - removedAnimal.IncomePerSecond))
                .Take(5)
                .ToArray();

            var option = options.Length > 0
                ? options[UnityEngine.Random.Range(0, options.Length)]
                : KickLuckyCubeAnimalSpawner.CreateDefaultOptions()[0];

            var incomeDelta = Mathf.Clamp(
                Mathf.RoundToInt(removedAnimal.IncomePerSecond * UnityEngine.Random.Range(-0.2f, 0.24f)),
                -Mathf.Max(1, removedAnimal.IncomePerSecond / 2),
                Mathf.Max(1, removedAnimal.IncomePerSecond / 2));
            var income = Mathf.Max(1, option.IncomePerSecond + incomeDelta);
            var sellValue = Mathf.Max(1, option.SellValue + incomeDelta * 18);
            var color = Color.Lerp(option.BodyColor, removedAnimal.BodyColor, UnityEngine.Random.Range(0.18f, 0.42f));
            var rarity = income > removedAnimal.IncomePerSecond ? BoostRarity(removedAnimal.Rarity) : SoftenRarity(removedAnimal.Rarity);

            return new KickLuckyCubeInventoryAnimal(
                Guid.NewGuid().ToString("N"),
                option.AnimalName + " Trade",
                rarity,
                color,
                sellValue,
                income);
        }

        private bool HasWeatherKeyMob()
        {
            if (inventory != null && inventory.GetSellableAnimals().Any(slot => slot.Animal.Rarity >= KickLuckyCubeRarity.Epic))
            {
                return true;
            }

            return FindObjectsByType<KickLuckyCubeStableSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Any(slot => slot != null && slot.PlacedAnimal != null && slot.PlacedAnimal.Rarity >= KickLuckyCubeRarity.Epic);
        }

        private void RechargeExchangeCharges()
        {
            var now = GetUnixNow();
            if (exchangeCharges >= maxExchangeCharges)
            {
                exchangeCharges = maxExchangeCharges;
                exchangeLastAtUnix = now;
                SaveExchangeState();
                return;
            }

            var elapsed = Mathf.Max(0f, (float)(now - exchangeLastAtUnix));
            var recovered = Mathf.FloorToInt(elapsed / exchangeRechargeSeconds);
            if (recovered <= 0)
            {
                return;
            }

            exchangeCharges = Mathf.Min(maxExchangeCharges, exchangeCharges + recovered);
            exchangeLastAtUnix += recovered * exchangeRechargeSeconds;
            if (exchangeCharges >= maxExchangeCharges)
            {
                exchangeLastAtUnix = now;
            }

            SaveExchangeState();
        }

        private float GetNextExchangeChargeSeconds()
        {
            if (exchangeCharges >= maxExchangeCharges)
            {
                return 0f;
            }

            var elapsed = Mathf.Max(0f, (float)(GetUnixNow() - exchangeLastAtUnix));
            return Mathf.Max(0f, exchangeRechargeSeconds - elapsed);
        }

        private void LoadState()
        {
            weatherEndsAtUnix = ParseDouble(PlayerPrefs.GetString(WeatherEndsAtKey, "0"));
            exchangeCharges = PlayerPrefs.GetInt(ExchangeChargesKey, maxExchangeCharges);
            exchangeLastAtUnix = ParseDouble(PlayerPrefs.GetString(ExchangeLastAtKey, GetUnixNow().ToString(CultureInfo.InvariantCulture)));
            exchangeCharges = Mathf.Clamp(exchangeCharges, 0, maxExchangeCharges);
        }

        private void SaveExchangeState()
        {
            PlayerPrefs.SetInt(ExchangeChargesKey, exchangeCharges);
            PlayerPrefs.SetString(ExchangeLastAtKey, exchangeLastAtUnix.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            inventory ??= FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            canvas ??= FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            uiFont ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void ConfigureFutureSpots()
        {
            var root = GameObject.Find(futureSpotsRootName);
            if (root == null)
            {
                return;
            }

            ConfigureSpot(root.transform, "WeatherMachine", KickLuckyCubeFutureFeature.WeatherMachine, "Start weather", 22);
            ConfigureSpot(root.transform, "ExchangeBooth", KickLuckyCubeFutureFeature.ExchangeBooth, "Exchange selected mob", 22);
            ConfigureSpot(root.transform, "EpicMobShop", KickLuckyCubeFutureFeature.EpicMobShop, "Open epic mob shop", 22);
            ConfigureSpot(root.transform, "RatingGiftStand", KickLuckyCubeFutureFeature.RatingGift, "Claim rating gift", 22);
        }

        private void ConfigureSpot(Transform root, string namePart, KickLuckyCubeFutureFeature feature, string prompt, int priority)
        {
            var spotRoot = FindChildByNamePart(root, namePart);
            if (spotRoot == null)
            {
                return;
            }

            var pad = FindChildByNamePart(spotRoot, "ReservedPad") ?? spotRoot;
            ConfigureTriggerCollider(pad.gameObject, interactionTriggerSize);

            var target = GetOrAddComponent<GameKitInteractionTarget>(pad.gameObject);
            ConfigureInteractionTarget(target, prompt, priority);

            if (pad.GetComponent<GameKitInteractionTriggerSource>() == null)
            {
                pad.gameObject.AddComponent<GameKitInteractionTriggerSource>();
            }

            var featurePad = GetOrAddComponent<KickLuckyCubeFutureFeaturePad>(pad.gameObject);
            featurePad.Configure(this, feature);
            UpdateSpotLabel(spotRoot, feature);
        }

        private void UpdateSpotLabel(Transform spotRoot, KickLuckyCubeFutureFeature feature)
        {
            var label = spotRoot.GetComponentsInChildren<TextMesh>(true)
                .FirstOrDefault(text => text.name.IndexOf("Label", StringComparison.OrdinalIgnoreCase) >= 0)
                ?? spotRoot.GetComponentInChildren<TextMesh>(true);

            if (label == null)
            {
                return;
            }

            label.text = feature switch
            {
                KickLuckyCubeFutureFeature.WeatherMachine => "Weather Machine\nEpic+ mob required",
                KickLuckyCubeFutureFeature.ExchangeBooth => "Animal Exchange\n10 charges, +1 / 5 min",
                KickLuckyCubeFutureFeature.EpicMobShop => "Epic Mob Shop\n3 exclusives",
                KickLuckyCubeFutureFeature.RatingGift => "Rating Gift\n1 free mob",
                _ => label.text,
            };
        }

        private void BuildRuntimeUi()
        {
            ResolveReferences();
            if (canvas == null)
            {
                return;
            }

            var canvasTransform = canvas.transform;
            if (statusText == null)
            {
                var statusPanel = CreateRect("KLC_FutureFeatureStatus", canvasTransform);
                statusPanel.anchorMin = new Vector2(0.5f, 0.5f);
                statusPanel.anchorMax = new Vector2(0.5f, 0.5f);
                statusPanel.pivot = new Vector2(0.5f, 0.5f);
                statusPanel.sizeDelta = new Vector2(680f, 82f);
                statusPanel.anchoredPosition = new Vector2(0f, 225f);
                var image = AddImage(statusPanel.gameObject, new Color(0.03f, 0.035f, 0.045f, 0.72f));
                image.raycastTarget = false;
                statusText = CreateLabel(statusPanel, "Text", string.Empty, 24, TextAnchor.MiddleCenter, new Vector2(660f, 74f), Vector2.zero);
                statusPanel.SetAsLastSibling();
                statusPanel.gameObject.SetActive(false);
            }

            if (weatherLabel == null || exchangeLabel == null)
            {
                var statusStrip = CreateRect("KLC_FutureFeatureStatusStrip", canvasTransform);
                statusStrip.anchorMin = new Vector2(1f, 1f);
                statusStrip.anchorMax = new Vector2(1f, 1f);
                statusStrip.pivot = new Vector2(1f, 1f);
                statusStrip.anchoredPosition = new Vector2(-18f, -88f);
                statusStrip.sizeDelta = new Vector2(290f, 70f);

                var statusLayout = statusStrip.gameObject.AddComponent<HorizontalLayoutGroup>();
                statusLayout.childAlignment = TextAnchor.MiddleRight;
                statusLayout.childControlWidth = false;
                statusLayout.childControlHeight = false;
                statusLayout.childForceExpandWidth = false;
                statusLayout.childForceExpandHeight = false;
                statusLayout.spacing = 8f;

                weatherLabel = CreateStatusPill(statusStrip, "WeatherPill", new Color(0.10f, 0.32f, 0.62f, 0.86f));
                exchangeLabel = CreateStatusPill(statusStrip, "ExchangePill", new Color(0.42f, 0.20f, 0.66f, 0.86f));
            }

            if (exchangeWindow == null)
            {
                exchangeWindow = CreateRect("KLC_ExchangeWindow_Runtime", canvasTransform);
                exchangeWindow.anchorMin = new Vector2(0.5f, 0.5f);
                exchangeWindow.anchorMax = new Vector2(0.5f, 0.5f);
                exchangeWindow.pivot = new Vector2(0.5f, 0.5f);
                exchangeWindow.anchoredPosition = new Vector2(0f, 18f);
                exchangeWindow.sizeDelta = new Vector2(520f, 300f);
                AddImage(exchangeWindow.gameObject, new Color(0.035f, 0.032f, 0.048f, 0.94f));

                CreateLabel(exchangeWindow, "Title", "Animal Exchange", 32, TextAnchor.MiddleLeft, new Vector2(340f, 42f), new Vector2(-55f, 112f));
                exchangeStatusText = CreateLabel(exchangeWindow, "Status", string.Empty, 20, TextAnchor.MiddleCenter, new Vector2(450f, 120f), new Vector2(0f, 32f));

                exchangeButton = CreateButton(exchangeWindow, "ExchangeButton", "Exchange", new Vector2(190f, 44f));
                exchangeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(-64f, -102f);
                exchangeButton.onClick.AddListener(ExchangeSelectedAnimal);

                var closeExchangeButton = CreateButton(exchangeWindow, "Close", "X", new Vector2(44f, 36f));
                closeExchangeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(226f, 118f);
                closeExchangeButton.onClick.AddListener(CloseExchangeWindow);

                exchangeWindow.gameObject.SetActive(false);
            }

            if (epicShopWindow != null)
            {
                return;
            }

            epicShopWindow = CreateRect("KLC_EpicMobShopWindow_Runtime", canvasTransform);
            epicShopWindow.anchorMin = new Vector2(0.5f, 0.5f);
            epicShopWindow.anchorMax = new Vector2(0.5f, 0.5f);
            epicShopWindow.pivot = new Vector2(0.5f, 0.5f);
            epicShopWindow.anchoredPosition = new Vector2(0f, 26f);
            epicShopWindow.sizeDelta = new Vector2(760f, 430f);
            AddImage(epicShopWindow.gameObject, new Color(0.045f, 0.035f, 0.065f, 0.94f));

            CreateLabel(epicShopWindow, "Title", "Epic Mob Shop", 36, TextAnchor.MiddleLeft, new Vector2(420f, 50f), new Vector2(-145f, 172f));
            epicShopStatusText = CreateLabel(epicShopWindow, "Status", "Three exclusive mobs can only be bought here.", 18, TextAnchor.MiddleLeft, new Vector2(520f, 30f), new Vector2(-55f, -178f));

            var closeButton = CreateButton(epicShopWindow, "Close", "X", new Vector2(44f, 36f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(342f, 172f);
            closeButton.onClick.AddListener(CloseEpicShop);

            var grid = CreateRect("EpicCards", epicShopWindow);
            grid.anchorMin = new Vector2(0.5f, 0.5f);
            grid.anchorMax = new Vector2(0.5f, 0.5f);
            grid.pivot = new Vector2(0.5f, 0.5f);
            grid.anchoredPosition = new Vector2(0f, -12f);
            grid.sizeDelta = new Vector2(650f, 260f);

            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(196f, 246f);
            layout.spacing = new Vector2(18f, 0f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;

            epicButtons = new Button[EpicShopAnimals.Length];
            epicButtonTexts = new Text[EpicShopAnimals.Length];
            epicCardFrames = new Image[EpicShopAnimals.Length];
            for (var index = 0; index < EpicShopAnimals.Length; index++)
            {
                CreateEpicCard(grid, index);
            }
        }

        private void CreateEpicCard(RectTransform parent, int index)
        {
            var animal = EpicShopAnimals[index];
            var card = CreateRect("EpicMob_" + index, parent);
            epicCardFrames[index] = AddImage(card.gameObject, new Color(animal.BodyColor.r * 0.42f, animal.BodyColor.g * 0.42f, animal.BodyColor.b * 0.42f, 0.92f));

            var preview = CreateRect("Preview", card);
            preview.sizeDelta = new Vector2(88f, 74f);
            preview.anchoredPosition = new Vector2(0f, 70f);
            AddImage(preview.gameObject, animal.BodyColor);

            CreateLabel(card, "Name", animal.AnimalName, 20, TextAnchor.MiddleCenter, new Vector2(178f, 42f), new Vector2(0f, 18f));
            CreateLabel(card, "Detail", $"{animal.Rarity}\n+{animal.IncomePerSecond}/s\nSell {animal.SellValue}", 16, TextAnchor.MiddleCenter, new Vector2(176f, 74f), new Vector2(0f, -42f));

            var button = CreateButton(card, "Buy", string.Empty, new Vector2(154f, 40f));
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -98f);
            var capturedIndex = index;
            button.onClick.AddListener(() => BuyEpicMob(capturedIndex));
            epicButtons[index] = button;
            epicButtonTexts[index] = button.GetComponentInChildren<Text>();
        }

        private void RefreshAll()
        {
            if (weatherLabel != null)
            {
                weatherLabel.text = IsWeatherActive
                    ? $"Weather active\n{FormatTime(RemainingWeatherSeconds)}"
                    : "Weather ready";
            }

            if (exchangeLabel != null)
            {
                exchangeLabel.text = exchangeCharges > 0
                    ? $"Exchange\n{exchangeCharges}/{maxExchangeCharges}"
                    : $"Exchange\n{FormatTime(GetNextExchangeChargeSeconds())}";
            }

            RefreshExchangeWindow();
            RefreshEpicShop();
        }

        private void RefreshEpicShop()
        {
            if (epicButtonTexts == null)
            {
                return;
            }

            for (var index = 0; index < epicButtonTexts.Length; index++)
            {
                var bought = IsEpicMobBought(index);
                var canAfford = wallet != null && wallet.SoftCurrency >= EpicShopCosts[index];
                if (epicButtonTexts[index] != null)
                {
                    epicButtonTexts[index].text = bought
                        ? "Owned"
                        : canAfford
                            ? $"Buy ${EpicShopCosts[index]}"
                            : $"Need ${EpicShopCosts[index]}";
                }

                if (epicButtons != null && index < epicButtons.Length && epicButtons[index] != null)
                {
                    epicButtons[index].interactable = !bought && canAfford;
                }

                if (epicCardFrames != null && index < epicCardFrames.Length && epicCardFrames[index] != null)
                {
                    var animal = EpicShopAnimals[index];
                    epicCardFrames[index].color = bought
                        ? new Color(0.18f, 0.60f, 0.30f, 0.94f)
                        : canAfford
                            ? new Color(animal.BodyColor.r * 0.48f, animal.BodyColor.g * 0.48f, animal.BodyColor.b * 0.48f, 0.94f)
                            : new Color(0.14f, 0.14f, 0.18f, 0.88f);
                }
            }
        }

        private void RefreshExchangeWindow()
        {
            if (exchangeStatusText == null && exchangeButton == null)
            {
                return;
            }

            ResolveReferences();
            var selectedAnimal = default(KickLuckyCubeInventoryAnimal);
            var hasSelection = inventory != null && inventory.TryGetSelectedAnimal(out selectedAnimal);
            var hasCharges = exchangeCharges > 0;
            var canExchange = hasSelection && hasCharges;

            if (exchangeStatusText != null)
            {
                if (!hasSelection)
                {
                    exchangeStatusText.text = $"Select a mob from the hotbar or inventory.\nCharges: {exchangeCharges}/{maxExchangeCharges}";
                }
                else if (!hasCharges)
                {
                    exchangeStatusText.text = $"{selectedAnimal.AnimalName}\nNo exchange charges.\nNext charge in {FormatTime(GetNextExchangeChargeSeconds())}";
                }
                else
                {
                    exchangeStatusText.text = $"{selectedAnimal.AnimalName}\n{selectedAnimal.Rarity}  +{selectedAnimal.IncomePerSecond}/s\nCharges: {exchangeCharges}/{maxExchangeCharges}";
                }
            }

            if (exchangeButton != null)
            {
                exchangeButton.interactable = canExchange;
                var label = exchangeButton.GetComponentInChildren<Text>();
                if (label != null)
                {
                    label.text = canExchange ? "Exchange" : "Locked";
                }
            }
        }

        private void ShowStatus(string message)
        {
            if (statusText == null)
            {
                Debug.Log(message);
                return;
            }

            statusText.text = message;
            statusText.transform.parent.gameObject.SetActive(true);
            statusText.transform.parent.SetAsLastSibling();
            statusHideAt = Time.unscaledTime + statusSeconds;
        }

        private void SetEpicShopStatus(string message)
        {
            if (epicShopStatusText != null)
            {
                epicShopStatusText.text = message;
            }
        }

        private static KickLuckyCubeRarity BoostRarity(KickLuckyCubeRarity rarity)
        {
            return rarity switch
            {
                KickLuckyCubeRarity.None => KickLuckyCubeRarity.Common,
                KickLuckyCubeRarity.Common => KickLuckyCubeRarity.Uncommon,
                KickLuckyCubeRarity.Uncommon => KickLuckyCubeRarity.Rare,
                KickLuckyCubeRarity.Rare => KickLuckyCubeRarity.Epic,
                KickLuckyCubeRarity.Epic => KickLuckyCubeRarity.Legendary,
                _ => KickLuckyCubeRarity.Legendary,
            };
        }

        private static KickLuckyCubeRarity SoftenRarity(KickLuckyCubeRarity rarity)
        {
            return rarity switch
            {
                KickLuckyCubeRarity.Legendary => KickLuckyCubeRarity.Epic,
                KickLuckyCubeRarity.Epic => KickLuckyCubeRarity.Rare,
                KickLuckyCubeRarity.Rare => KickLuckyCubeRarity.Uncommon,
                KickLuckyCubeRarity.Uncommon => KickLuckyCubeRarity.Common,
                _ => KickLuckyCubeRarity.Common,
            };
        }

        private static void ConfigureTriggerCollider(GameObject target, Vector3 triggerSize)
        {
            var collider = GetOrAddComponent<BoxCollider>(target);
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 1.25f, 0f);
            collider.size = new Vector3(
                Mathf.Max(0.2f, triggerSize.x),
                Mathf.Max(0.2f, triggerSize.y),
                Mathf.Max(0.2f, triggerSize.z));
        }

        private static void ConfigureInteractionTarget(GameKitInteractionTarget target, string prompt, int priority)
        {
            SetPrivateField(target, "promptKey", "E");
            SetPrivateField(target, "promptText", prompt);
            SetPrivateField(target, "activationMode", GameKitInteractionActivationMode.Press);
            SetPrivateField(target, "holdSeconds", 0.05f);
            SetPrivateField(target, "priority", priority);
            SetPrivateField(target, "interactable", true);
        }

        private static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            var field = typeof(TTarget).GetField(fieldName, SerializedFieldFlags);
            field?.SetValue(target, value);
        }

        private static Transform FindChildByNamePart(Transform root, string namePart)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child => child.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static T GetOrAddComponent<T>(GameObject target)
            where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private bool IsEpicMobBought(int index)
        {
            return PlayerPrefs.GetInt(EpicMobBoughtKeyPrefix + index, 0) != 0;
        }

        private static double GetUnixNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private static double ParseDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0d;
        }

        private static string FormatTime(float seconds)
        {
            var clamped = Mathf.Max(0, Mathf.CeilToInt(seconds));
            var minutes = clamped / 60;
            var remainingSeconds = clamped % 60;
            return minutes > 0 ? $"{minutes}:{remainingSeconds:00}" : $"{remainingSeconds}s";
        }

        private Image AddImage(GameObject target, Color color)
        {
            var image = target.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private RectTransform CreateRect(string name, Transform parent)
        {
            var rectObject = new GameObject(name, typeof(RectTransform));
            var rect = rectObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            return rect;
        }

        private Text CreateStatusPill(RectTransform parent, string name, Color color)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = new Vector2(138f, 62f);
            var layout = rect.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 138f;
            layout.preferredHeight = 62f;
            AddImage(rect.gameObject, color).raycastTarget = false;
            return CreateLabel(rect, "Label", string.Empty, 16, TextAnchor.MiddleCenter, new Vector2(130f, 54f), Vector2.zero);
        }

        private Text CreateLabel(RectTransform parent, string name, string value, int fontSize, TextAnchor anchor, Vector2 size, Vector2 position)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = uiFont;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = anchor;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private Button CreateButton(RectTransform parent, string name, string value, Vector2 size)
        {
            var rect = CreateRect(name, parent);
            rect.sizeDelta = size;

            var image = AddImage(rect.gameObject, new Color(0.15f, 0.18f, 0.22f, 0.94f));
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            CreateLabel(rect, "Label", value, 16, TextAnchor.MiddleCenter, size, Vector2.zero);
            return button;
        }
    }

    [RequireComponent(typeof(GameKitInteractionTarget))]
    public sealed class KickLuckyCubeFutureFeaturePad : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeFutureFeatureController controller;
        [SerializeField] private KickLuckyCubeFutureFeature feature;

        private GameKitInteractionTarget interactionTarget;

        private void Awake()
        {
            controller ??= FindFirstObjectByType<KickLuckyCubeFutureFeatureController>(FindObjectsInactive.Include);
            interactionTarget = GetComponent<GameKitInteractionTarget>();
            interactionTarget.ActorInteracted.AddListener(Interact);
        }

        private void OnDestroy()
        {
            if (interactionTarget != null)
            {
                interactionTarget.ActorInteracted.RemoveListener(Interact);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (controller == null)
            {
                return;
            }

            var driver = FindFirstObjectByType<GameKitInteractionDriver>();
            if (driver != null && driver.IsActorCollider(other))
            {
                if (feature == KickLuckyCubeFutureFeature.EpicMobShop)
                {
                    controller.CloseEpicShop();
                }
                else if (feature == KickLuckyCubeFutureFeature.ExchangeBooth)
                {
                    controller.CloseExchangeWindow();
                }
            }
        }

        public void Configure(KickLuckyCubeFutureFeatureController owner, KickLuckyCubeFutureFeature targetFeature)
        {
            controller = owner;
            feature = targetFeature;
        }

        public bool CanInteract(GameObject actor)
        {
            controller ??= FindFirstObjectByType<KickLuckyCubeFutureFeatureController>(FindObjectsInactive.Include);
            return controller != null && controller.CanUseFeature(feature);
        }

        private void Interact(GameObject actor)
        {
            controller ??= FindFirstObjectByType<KickLuckyCubeFutureFeatureController>(FindObjectsInactive.Include);
            controller?.Interact(feature, actor);
        }
    }
}
