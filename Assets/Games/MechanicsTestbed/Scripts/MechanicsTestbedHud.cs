using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedHud : MonoBehaviour
    {
        private enum PanelKind
        {
            Settings,
            Shop,
            Wheel,
            Rewards
        }

        [Header("Services")]
        [SerializeField] private MechanicsTestbedWallet wallet;
        [SerializeField] private MechanicsTestbedSettings settings;
        [SerializeField] private MechanicsTestbedTimedRewards timedRewards;

        [Header("Texts")]
        [SerializeField] private Text softCurrencyText;
        [SerializeField] private Text hardCurrencyText;
        [SerializeField] private Text panelTitleText;
        [SerializeField] private Text panelBodyText;
        [SerializeField] private Text rewardStatusText;

        private PanelKind activePanel = PanelKind.Settings;
        private float nextRefreshTime;
        private float nextWheelSpinTime;

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<MechanicsTestbedWallet>();
            settings ??= FindFirstObjectByType<MechanicsTestbedSettings>();
            timedRewards ??= FindFirstObjectByType<MechanicsTestbedTimedRewards>();
        }

        private void OnEnable()
        {
            if (wallet != null)
            {
                wallet.Changed += OnWalletChanged;
            }

            if (settings != null)
            {
                settings.Changed += RefreshPanel;
            }

            if (timedRewards != null)
            {
                timedRewards.Changed += RefreshPanel;
            }
        }

        private void Start()
        {
            RefreshCurrency();
            RefreshPanel();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + 0.25f;
            RefreshPanel();
        }

        private void OnDisable()
        {
            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
            }

            if (settings != null)
            {
                settings.Changed -= RefreshPanel;
            }

            if (timedRewards != null)
            {
                timedRewards.Changed -= RefreshPanel;
            }
        }

        public void ShowSettingsPanel()
        {
            activePanel = PanelKind.Settings;
            RefreshPanel();
        }

        public void ShowShopPanel()
        {
            activePanel = PanelKind.Shop;
            RefreshPanel();
        }

        public void ShowWheelPanel()
        {
            activePanel = PanelKind.Wheel;
            RefreshPanel();
        }

        public void ShowRewardsPanel()
        {
            activePanel = PanelKind.Rewards;
            RefreshPanel();
        }

        public void AddSoftPrototypeGrant()
        {
            wallet?.AddSoft(250);
        }

        public void AddHardPrototypeGrant()
        {
            wallet?.AddHard(5);
        }

        public void DecreaseMusic()
        {
            if (settings != null)
            {
                settings.SetMusicVolume(settings.MusicVolume - 0.1f);
            }
        }

        public void IncreaseMusic()
        {
            if (settings != null)
            {
                settings.SetMusicVolume(settings.MusicVolume + 0.1f);
            }
        }

        public void DecreaseSfx()
        {
            if (settings != null)
            {
                settings.SetSfxVolume(settings.SfxVolume - 0.1f);
            }
        }

        public void IncreaseSfx()
        {
            if (settings != null)
            {
                settings.SetSfxVolume(settings.SfxVolume + 0.1f);
            }
        }

        public void CycleLanguage()
        {
            settings?.CycleLanguage();
        }

        public void SpinWheel()
        {
            activePanel = PanelKind.Wheel;

            if (Time.unscaledTime < nextWheelSpinTime)
            {
                RefreshPanel("Wheel cooldown");
                return;
            }

            nextWheelSpinTime = Time.unscaledTime + 20f;
            var roll = Random.Range(0, 100);

            if (roll >= 80)
            {
                var hardReward = Random.Range(1, 4);
                wallet?.AddHard(hardReward);
                RefreshPanel("Wheel: +" + hardReward.ToString(CultureInfo.InvariantCulture) + " hard");
                return;
            }

            var softReward = Random.Range(50, 201);
            wallet?.AddSoft(softReward);
            RefreshPanel("Wheel: +" + softReward.ToString(CultureInfo.InvariantCulture) + " soft");
        }

        public void ClaimTimedReward()
        {
            activePanel = PanelKind.Rewards;
            RefreshPanel(timedRewards != null && timedRewards.Claim() ? "Reward claimed" : "Reward not ready");
        }

        private void OnWalletChanged(int soft, int hard)
        {
            RefreshCurrency();
            RefreshPanel();
        }

        private void RefreshCurrency()
        {
            if (wallet == null)
            {
                return;
            }

            if (softCurrencyText != null)
            {
                softCurrencyText.text = "Soft " + wallet.SoftCurrency.ToString(CultureInfo.InvariantCulture);
            }

            if (hardCurrencyText != null)
            {
                hardCurrencyText.text = "Hard " + wallet.HardCurrency.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void RefreshPanel()
        {
            RefreshPanel(null);
        }

        private void RefreshPanel(string statusOverride)
        {
            if (panelTitleText != null)
            {
                panelTitleText.text = activePanel.ToString();
            }

            if (panelBodyText != null)
            {
                panelBodyText.text = GetPanelBody(statusOverride);
            }

            if (rewardStatusText != null)
            {
                rewardStatusText.text = GetRewardStatus();
            }
        }

        private string GetPanelBody(string statusOverride)
        {
            if (!string.IsNullOrEmpty(statusOverride))
            {
                return statusOverride;
            }

            switch (activePanel)
            {
                case PanelKind.Settings:
                    return settings == null
                        ? "Settings unavailable"
                        : "Music " + ToPercent(settings.MusicVolume) + "\nSFX " + ToPercent(settings.SfxVolume) + "\nLanguage " + settings.LanguageCode.ToUpperInvariant();
                case PanelKind.Shop:
                    return "Prototype grants\n+250 soft\n+5 hard";
                case PanelKind.Wheel:
                    return Time.unscaledTime < nextWheelSpinTime
                        ? "Next spin in " + Mathf.CeilToInt(nextWheelSpinTime - Time.unscaledTime).ToString(CultureInfo.InvariantCulture) + "s"
                        : "Spin available";
                case PanelKind.Rewards:
                    return GetRewardStatus();
                default:
                    return string.Empty;
            }
        }

        private string GetRewardStatus()
        {
            if (timedRewards == null)
            {
                return "Timed reward unavailable";
            }

            return timedRewards.RewardReady
                ? "+" + timedRewards.HardCurrencyReward.ToString(CultureInfo.InvariantCulture) + " hard ready"
                : "Timed reward in " + Mathf.CeilToInt(timedRewards.RemainingSeconds).ToString(CultureInfo.InvariantCulture) + "s";
        }

        private static string ToPercent(float value)
        {
            return Mathf.RoundToInt(value * 100f).ToString(CultureInfo.InvariantCulture) + "%";
        }
    }
}
