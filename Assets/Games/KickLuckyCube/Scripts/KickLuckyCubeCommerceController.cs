using System;
using System.Globalization;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DefaultExecutionOrder(-120)]
    public sealed class KickLuckyCubeCommerceController : MonoBehaviour
    {
        private const string PrivilegeExpiresAtKey = "KickLuckyCube.Commerce.PrivilegeExpiresAt";

        private static KickLuckyCubeCommerceController instance;

        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField, Min(1)] private int privilegeDurationDays = 30;
        [SerializeField, Min(1f)] private float privilegeSoftMultiplier = 5f;
        [SerializeField, Min(0)] private int privilegeHardReward = 300;
        [SerializeField, Min(0)] private int privilegeFallbackHardCost = 1200;
        [SerializeField] private bool simulateInAppPurchasesInEditor;
        [SerializeField] private bool simulateRewardedAdsInEditor = true;
        [SerializeField] private bool disableInAppPurchasesOnWebGl = true;
        [SerializeField] private bool disableRewardedAdsOnWebGl = true;

        private double privilegeExpiresAtUnix;
        private float refreshTimer;

        public static KickLuckyCubeCommerceController Instance => instance;
        public event Action Changed;

        public bool IsPrivilegeActive => GetUnixNow() < privilegeExpiresAtUnix;
        public bool AdsDisabled => IsPrivilegeActive;
        public float PrivilegeSoftMultiplier => privilegeSoftMultiplier;
        public int PrivilegeHardReward => privilegeHardReward;
        public int PrivilegeFallbackHardCost => privilegeFallbackHardCost;
        public float RemainingPrivilegeSeconds => Mathf.Max(0f, (float)(privilegeExpiresAtUnix - GetUnixNow()));
        public bool SupportsInAppPurchases => ResolveSupportsInAppPurchases();
        public bool SupportsRewardedAds => ResolveSupportsRewardedAds();
        public bool CanUseRewardedMultiplier => AdsDisabled || SupportsRewardedAds;
        public string PrivilegePurchasePriceText => SupportsInAppPurchases
            ? "IAP"
            : $"{privilegeFallbackHardCost} hard";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeCommerceController>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_CommerceController_Runtime").AddComponent<KickLuckyCubeCommerceController>();
        }

        private void Awake()
        {
            instance = this;
            ResolveReferences();
            LoadState();
            ApplyWalletBonus();
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
            if (refreshTimer < 1f)
            {
                return;
            }

            refreshTimer = 0f;
            ApplyWalletBonus();
            Changed?.Invoke();
        }

        public bool TryPurchasePrivilege(out string status)
        {
            ResolveReferences();
            if (SupportsInAppPurchases)
            {
                GrantPrivilege("Prototype IAP");
                status = $"VIP active for {privilegeDurationDays} days. +{privilegeHardReward} hard.";
                return true;
            }

            if (wallet == null)
            {
                status = "Wallet is not ready.";
                return false;
            }

            if (!wallet.TrySpendHard(privilegeFallbackHardCost))
            {
                status = $"Need {privilegeFallbackHardCost} hard for VIP.";
                return false;
            }

            GrantPrivilege("Hard fallback");
            status = $"VIP active for {privilegeDurationDays} days. +{privilegeHardReward} hard.";
            return true;
        }

        public bool TryUseRewardedAd(out string status)
        {
            if (AdsDisabled)
            {
                status = "VIP skips ads.";
                return true;
            }

            if (!SupportsRewardedAds)
            {
                status = "Rewarded ads are unavailable on this platform.";
                return false;
            }

            status = "Rewarded ad completed.";
            return true;
        }

        public string FormatPrivilegeRemaining()
        {
            return IsPrivilegeActive
                ? FormatDuration(RemainingPrivilegeSeconds)
                : "inactive";
        }

        private void GrantPrivilege(string source)
        {
            var now = GetUnixNow();
            var start = Math.Max(now, privilegeExpiresAtUnix);
            privilegeExpiresAtUnix = start + Math.Max(1, privilegeDurationDays) * 86400d;
            SaveState();

            if (wallet != null && privilegeHardReward > 0)
            {
                wallet.AddHard(privilegeHardReward);
            }

            ApplyWalletBonus();
            Debug.Log($"Kick Lucky Cube VIP pass granted ({source}) until {privilegeExpiresAtUnix.ToString(CultureInfo.InvariantCulture)}.");
            Changed?.Invoke();
        }

        private void ApplyWalletBonus()
        {
            ResolveReferences();
            if (wallet != null)
            {
                wallet.SetSoftGainBonusMultiplier(IsPrivilegeActive ? privilegeSoftMultiplier : 1f);
            }
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
        }

        private void LoadState()
        {
            privilegeExpiresAtUnix = ParseDouble(PlayerPrefs.GetString(PrivilegeExpiresAtKey, "0"));
        }

        private void SaveState()
        {
            PlayerPrefs.SetString(PrivilegeExpiresAtKey, privilegeExpiresAtUnix.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        private bool ResolveSupportsInAppPurchases()
        {
#if UNITY_EDITOR
            return simulateInAppPurchasesInEditor;
#else
            if (disableInAppPurchasesOnWebGl && Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return false;
            }

            return Application.platform == RuntimePlatform.IPhonePlayer
                || Application.platform == RuntimePlatform.Android;
#endif
        }

        private bool ResolveSupportsRewardedAds()
        {
#if UNITY_EDITOR
            return simulateRewardedAdsInEditor;
#else
            if (disableRewardedAdsOnWebGl && Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return false;
            }

            return Application.platform == RuntimePlatform.IPhonePlayer
                || Application.platform == RuntimePlatform.Android;
#endif
        }

        private static double GetUnixNow()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private static double ParseDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0d;
        }

        private static string FormatDuration(float seconds)
        {
            var totalSeconds = Mathf.Max(0, Mathf.CeilToInt(seconds));
            var days = totalSeconds / 86400;
            var hours = totalSeconds % 86400 / 3600;
            var minutes = totalSeconds % 3600 / 60;
            if (days > 0)
            {
                return $"{days}d {hours}h";
            }

            if (hours > 0)
            {
                return $"{hours}h {minutes}m";
            }

            return $"{minutes}m";
        }
    }
}
