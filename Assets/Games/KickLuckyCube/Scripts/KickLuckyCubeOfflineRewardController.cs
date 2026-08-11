using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeOfflineRewardController : MonoBehaviour
    {
        private const string LastSeenAtKey = "KickLuckyCube.OfflineReward.LastSeenAt";

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeCommerceController commerce;
        [SerializeField] private Canvas canvas;
        [SerializeField, Min(60f)] private float minimumOfflineSeconds = 1800f;
        [SerializeField, Min(1f)] private float lastSeenSaveIntervalSeconds = 15f;

        private RectTransform backdropRoot;
        private RectTransform windowRoot;
        private Text titleText;
        private Text subtitleText;
        private Text amountText;
        private Text detailText;
        private Text statusText;
        private Button closeButton;
        private Button claimButton;
        private Text claimButtonText;
        private Button adClaimButton;
        private Text adClaimButtonText;
        private RectTransform privilegeCard;
        private Text privilegeTitleText;
        private Text privilegeDetailText;
        private Text privilegeStatusText;
        private Button privilegeButton;
        private Text privilegeButtonText;
        private Font uiFont;
        private float saveTimer;
        private bool hasStartupCheckCompleted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeOfflineRewardController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_OfflineRewardController_Runtime").AddComponent<KickLuckyCubeOfflineRewardController>();
        }

        private IEnumerator Start()
        {
            ResolveReferences();
            Subscribe();
            BuildRuntimeUi();
            CloseWindow();

            yield return null;
            yield return null;

            CheckStartupReward();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            saveTimer += Time.unscaledDeltaTime;
            if (saveTimer >= lastSeenSaveIntervalSeconds)
            {
                saveTimer = 0f;
                SaveLastSeenNow();
            }

            if (windowRoot != null && windowRoot.gameObject.activeSelf)
            {
                RefreshWindow();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveLastSeenNow();
                return;
            }

            StartCoroutine(CheckAfterResume());
        }

        private void OnApplicationQuit()
        {
            SaveLastSeenNow();
        }

        private IEnumerator CheckAfterResume()
        {
            yield return null;
            CheckStartupReward();
        }

        private void CheckStartupReward()
        {
            ResolveReferences();
            BuildRuntimeUi();

            var lastSeenAt = ParseDouble(PlayerPrefs.GetString(LastSeenAtKey, "0"));
            var now = GetUnixNow();
            var offlineSeconds = lastSeenAt > 0d ? now - lastSeenAt : 0d;

            if (offlineSeconds >= minimumOfflineSeconds && GetTotalPendingSoft() > 0)
            {
                OpenWindow(offlineSeconds);
            }

            if (!hasStartupCheckCompleted)
            {
                hasStartupCheckCompleted = true;
                SaveLastSeenNow();
            }
        }

        private void OpenWindow(double offlineSeconds)
        {
            if (backdropRoot != null)
            {
                backdropRoot.gameObject.SetActive(true);
                backdropRoot.SetAsLastSibling();
            }

            if (windowRoot != null)
            {
                windowRoot.gameObject.SetActive(true);
                windowRoot.SetAsLastSibling();
            }

            if (subtitleText != null)
            {
                subtitleText.text = $"You were away for {FormatDuration((float)offlineSeconds)}.";
            }

            RefreshWindow();
        }

        private void CloseWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.gameObject.SetActive(false);
            }

            if (backdropRoot != null)
            {
                backdropRoot.gameObject.SetActive(false);
            }
        }

        private void ClaimNormal()
        {
            ClaimReward(1, "Offline reward claimed.");
        }

        private void ClaimWithAd()
        {
            ResolveReferences();
            var status = "Rewarded ads are not ready.";
            if (commerce == null || !commerce.TryUseRewardedAd(out status))
            {
                SetStatus(status);
                RefreshWindow();
                return;
            }

            ClaimReward(2, status + " x2 reward claimed.");
        }

        private void ClaimReward(int multiplier, string status)
        {
            ResolveReferences();
            if (wallet == null)
            {
                SetStatus("Wallet is not ready.");
                return;
            }

            var baseAmount = CollectAllPendingSoft();
            if (baseAmount <= 0)
            {
                SetStatus("No offline income to claim.");
                CloseWindow();
                return;
            }

            var multiplied = baseAmount > long.MaxValue / Mathf.Max(1, multiplier)
                ? long.MaxValue
                : baseAmount * Mathf.Max(1, multiplier);
            wallet.AddSoft(multiplied);
            SetStatus(status);
            SaveLastSeenNow();
            CloseWindow();
        }

        private void BuyPrivilegePass()
        {
            ResolveReferences();
            if (commerce == null)
            {
                SetStatus("VIP pass is not ready.");
                return;
            }

            commerce.TryPurchasePrivilege(out var status);
            SetStatus(status);
            RefreshWindow();
        }

        private long GetTotalPendingSoft()
        {
            return ResolvePlayerStableSlots().Aggregate(0L, (total, slot) => total + (slot != null ? slot.PendingSoft : 0));
        }

        private long CollectAllPendingSoft()
        {
            var total = 0L;
            var slots = ResolvePlayerStableSlots();
            for (var index = 0; index < slots.Length; index++)
            {
                var slot = slots[index];
                if (slot != null)
                {
                    total += slot.Collect();
                }
            }

            return total;
        }

        private KickLuckyCubeStableSlot[] ResolvePlayerStableSlots()
        {
            return FindObjectsByType<KickLuckyCubeStableSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(slot => slot != null && slot.IsPlayerPersistentSlot)
                .ToArray();
        }

        private void RefreshWindow()
        {
            ResolveReferences();
            var pending = GetTotalPendingSoft();
            var normalReward = wallet != null ? wallet.PreviewSoftGain(pending) : pending;
            var doubledBase = pending > long.MaxValue / 2L ? long.MaxValue : pending * 2L;
            var doubledReward = wallet != null ? wallet.PreviewSoftGain(doubledBase) : doubledBase;

            if (titleText != null)
            {
                titleText.text = "Offline Earnings";
            }

            if (amountText != null)
            {
                amountText.text = pending > 0
                    ? $"${FormatCompact(normalReward)}"
                    : "$0";
            }

            if (detailText != null)
            {
                var multiplierText = wallet != null && wallet.SoftGainMultiplier > 1f
                    ? $" Wallet multiplier x{wallet.SoftGainMultiplier:0.#} is included."
                    : string.Empty;
                detailText.text = $"Collected by your stable mobs while you were away.{multiplierText}";
            }

            if (claimButtonText != null)
            {
                claimButtonText.text = pending > 0 ? "Claim" : "Claimed";
            }

            if (claimButton != null)
            {
                claimButton.interactable = pending > 0;
            }

            var canUseAdMultiplier = commerce != null && commerce.CanUseRewardedMultiplier;
            if (adClaimButtonText != null)
            {
                if (commerce != null && commerce.AdsDisabled)
                {
                    adClaimButtonText.text = $"VIP x2: ${FormatCompact(doubledReward)}";
                }
                else if (commerce != null && commerce.SupportsRewardedAds)
                {
                    adClaimButtonText.text = $"Ad x2: ${FormatCompact(doubledReward)}";
                }
                else
                {
                    adClaimButtonText.text = "Ad x2 unavailable";
                }
            }

            if (adClaimButton != null)
            {
                adClaimButton.interactable = pending > 0 && canUseAdMultiplier;
            }

            RefreshPrivilegeCard();
        }

        private void RefreshPrivilegeCard()
        {
            if (commerce == null)
            {
                return;
            }

            if (privilegeTitleText != null)
            {
                privilegeTitleText.text = "VIP 30 Days";
            }

            if (privilegeDetailText != null)
            {
                privilegeDetailText.text = $"x{commerce.PrivilegeSoftMultiplier:0.#} soft income, +{commerce.PrivilegeHardReward} hard, no ads.";
            }

            if (privilegeStatusText != null)
            {
                privilegeStatusText.text = commerce.IsPrivilegeActive
                    ? $"Active: {commerce.FormatPrivilegeRemaining()}"
                    : commerce.SupportsInAppPurchases
                        ? "IAP available"
                        : "IAP unavailable: buy with hard";
            }

            if (privilegeButtonText != null)
            {
                privilegeButtonText.text = commerce.IsPrivilegeActive
                    ? "Extend"
                    : $"Buy {commerce.PrivilegePurchasePriceText}";
            }

            if (privilegeButton != null)
            {
                privilegeButton.interactable = commerce.SupportsInAppPurchases
                    || (wallet != null && wallet.HardCurrency >= commerce.PrivilegeFallbackHardCost);
            }
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            commerce ??= FindFirstObjectByType<KickLuckyCubeCommerceController>(FindObjectsInactive.Include);
            canvas = KickLuckyCubeUiPrefabFactory.ResolveMainCanvas(canvas);
            uiFont ??= KickLuckyCubeUiTheme.Font;
        }

        private void Subscribe()
        {
            if (commerce == null)
            {
                return;
            }

            commerce.Changed -= RefreshWindow;
            commerce.Changed += RefreshWindow;
        }

        private void Unsubscribe()
        {
            if (commerce != null)
            {
                commerce.Changed -= RefreshWindow;
            }
        }

        private void BuildRuntimeUi()
        {
            if (canvas == null || windowRoot != null)
            {
                return;
            }

            var canvasTransform = canvas.transform;
            backdropRoot = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_OfflineRewardBackdrop_Runtime", canvasTransform);
            StretchToCanvas(backdropRoot);
            var backdropImage = KickLuckyCubeUiTheme.AddImage(backdropRoot.gameObject, new Color(0f, 0f, 0f, 0.56f));
            backdropImage.raycastTarget = true;

            windowRoot = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_OfflineRewardWindow_Runtime", canvasTransform);
            windowRoot.anchorMin = new Vector2(0.5f, 0.5f);
            windowRoot.anchorMax = new Vector2(0.5f, 0.5f);
            windowRoot.pivot = new Vector2(0.5f, 0.5f);
            if (windowRoot.sizeDelta.sqrMagnitude <= 1f)
            {
                windowRoot.sizeDelta = new Vector2(780f, 500f);
            }

            windowRoot.anchoredPosition = new Vector2(0f, 24f);
            KickLuckyCubeUiTheme.AddImage(windowRoot.gameObject, new Color(0.04f, 0.08f, 0.11f, 0.93f));
            KickLuckyCubeUiTheme.StyleTree(windowRoot.gameObject);

            titleText = CreateLabel(windowRoot, "Title", "Offline Earnings", 36, TextAnchor.MiddleCenter, new Vector2(620f, 54f), new Vector2(0f, 190f));
            subtitleText = CreateLabel(windowRoot, "Subtitle", string.Empty, 20, TextAnchor.MiddleCenter, new Vector2(620f, 32f), new Vector2(0f, 150f));
            amountText = CreateLabel(windowRoot, "AmountText", "$0", 58, TextAnchor.MiddleCenter, new Vector2(420f, 80f), new Vector2(0f, 48f));
            detailText = CreateLabel(windowRoot, "DetailText", string.Empty, 18, TextAnchor.MiddleCenter, new Vector2(630f, 54f), new Vector2(0f, -24f));
            statusText = CreateLabel(windowRoot, "StatusText", string.Empty, 16, TextAnchor.MiddleCenter, new Vector2(620f, 28f), new Vector2(0f, -214f));

            claimButton = CreateButton(windowRoot, "ClaimButton", "Claim", new Vector2(190f, 58f), new Vector2(116f, -116f), KickLuckyCubeUiTheme.Primary);
            claimButtonText = claimButton.GetComponentInChildren<Text>();
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(ClaimNormal);

            adClaimButton = CreateButton(windowRoot, "AdClaimButton", "Ad x2", new Vector2(250f, 58f), new Vector2(-126f, -116f), new Color(0.70f, 0.22f, 0.92f, 0.96f));
            adClaimButtonText = adClaimButton.GetComponentInChildren<Text>();
            adClaimButton.onClick.RemoveAllListeners();
            adClaimButton.onClick.AddListener(ClaimWithAd);

            closeButton = CreateButton(windowRoot, "CloseButton", "X", new Vector2(52f, 46f), new Vector2(346f, 214f), KickLuckyCubeUiTheme.Close);
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseWindow);

            privilegeCard = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_PrivilegePassCard", windowRoot);
            ConfigureCentered(privilegeCard, new Vector2(650f, 102f), new Vector2(0f, -178f));
            KickLuckyCubeUiTheme.AddImage(privilegeCard.gameObject, new Color(0.10f, 0.48f, 0.96f, 0.92f));
            privilegeTitleText = CreateLabel(privilegeCard, "Title", "VIP 30 Days", 22, TextAnchor.MiddleLeft, new Vector2(190f, 32f), new Vector2(-210f, 24f));
            privilegeDetailText = CreateLabel(privilegeCard, "Detail", string.Empty, 16, TextAnchor.MiddleLeft, new Vector2(370f, 42f), new Vector2(-120f, -14f));
            privilegeStatusText = CreateLabel(privilegeCard, "Status", string.Empty, 14, TextAnchor.MiddleLeft, new Vector2(300f, 24f), new Vector2(4f, 30f));
            privilegeButton = CreateButton(privilegeCard, "BuyButton", "Buy", new Vector2(156f, 42f), new Vector2(224f, -18f), KickLuckyCubeUiTheme.Warning);
            privilegeButtonText = privilegeButton.GetComponentInChildren<Text>();
            privilegeButton.onClick.RemoveAllListeners();
            privilegeButton.onClick.AddListener(BuyPrivilegePass);
        }

        private Text CreateLabel(RectTransform parent, string name, string value, int fontSize, TextAnchor anchor, Vector2 size, Vector2 position)
        {
            return KickLuckyCubeUiPrefabFactory.GetOrCreateLabel(parent, name, uiFont, value, fontSize, anchor, size, position);
        }

        private Button CreateButton(RectTransform parent, string name, string value, Vector2 size, Vector2 position, Color color)
        {
            var button = KickLuckyCubeUiPrefabFactory.GetOrCreateButton(parent, name, value, uiFont, size, color, 18);
            var rect = button.transform as RectTransform;
            if (rect != null)
            {
                ConfigureCentered(rect, size, position);
            }

            return button;
        }

        private static void StretchToCanvas(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ConfigureCentered(RectTransform rect, Vector2 size, Vector2 position)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private void SetStatus(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }

        private static void SaveLastSeenNow()
        {
            PlayerPrefs.SetString(LastSeenAtKey, GetUnixNow().ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        private static double GetUnixNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private static double ParseDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0d;
        }

        private static int ClampToInt(long value)
        {
            if (value <= 0L)
            {
                return 0;
            }

            return value >= int.MaxValue ? int.MaxValue : (int)value;
        }

        private static string FormatCompact(long value)
        {
            return KickLuckyCubeNumberFormatter.FormatCompact(value);
        }

        private static string FormatDuration(float seconds)
        {
            var totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
            var hours = totalSeconds / 3600;
            var minutes = totalSeconds % 3600 / 60;
            if (hours > 0)
            {
                return $"{hours}h {minutes}m";
            }

            return $"{minutes}m";
        }
    }
}
