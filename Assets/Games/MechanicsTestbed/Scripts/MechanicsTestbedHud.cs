using System.Globalization;
using UnityEngine;
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
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
        [SerializeField] private Text musicValueText;
        [SerializeField] private Text sfxValueText;
        [SerializeField] private Text languageValueText;
        [SerializeField] private Text wheelStatusText;

        [Header("Windows")]
        [SerializeField] private GameObject modalBackdrop;
        [SerializeField] private GameObject settingsWindow;
        [SerializeField] private GameObject shopWindow;
        [SerializeField] private GameObject wheelWindow;
        [SerializeField] private GameObject rewardsWindow;
        [SerializeField] private GameObject cheatWindow;

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
            CloseActiveWindow();
            SetWindowVisible(cheatWindow, false);
        }

        private void Update()
        {
#if UNITY_EDITOR && ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
            {
                ToggleCheatWindow();
            }
#endif

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
            OpenWindow(settingsWindow);
            RefreshPanel();
        }

        public void ShowShopPanel()
        {
            activePanel = PanelKind.Shop;
            OpenWindow(shopWindow);
            RefreshPanel();
        }

        public void ShowWheelPanel()
        {
            activePanel = PanelKind.Wheel;
            OpenWindow(wheelWindow);
            RefreshPanel();
        }

        public void ShowRewardsPanel()
        {
            activePanel = PanelKind.Rewards;
            OpenWindow(rewardsWindow);
            RefreshPanel();
        }

        public void CloseActiveWindow()
        {
            SetWindowVisible(settingsWindow, false);
            SetWindowVisible(shopWindow, false);
            SetWindowVisible(wheelWindow, false);
            SetWindowVisible(rewardsWindow, false);
            SetWindowVisible(modalBackdrop, false);
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
            OpenWindow(wheelWindow);

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
            OpenWindow(rewardsWindow);
            RefreshPanel(timedRewards != null && timedRewards.Claim() ? "Reward claimed" : "Reward not ready");
        }

        public void ToggleCheatWindow()
        {
#if UNITY_EDITOR
            if (cheatWindow == null)
            {
                return;
            }

            SetWindowVisible(cheatWindow, !IsWindowVisible(cheatWindow));
#endif
        }

        public void CloseCheatWindow()
        {
            SetWindowVisible(cheatWindow, false);
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
                softCurrencyText.text = wallet.SoftCurrency.ToString(CultureInfo.InvariantCulture);
            }

            if (hardCurrencyText != null)
            {
                hardCurrencyText.text = wallet.HardCurrency.ToString(CultureInfo.InvariantCulture);
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

            if (musicValueText != null)
            {
                musicValueText.text = settings == null ? "--" : ToPercent(settings.MusicVolume);
            }

            if (sfxValueText != null)
            {
                sfxValueText.text = settings == null ? "--" : ToPercent(settings.SfxVolume);
            }

            if (languageValueText != null)
            {
                languageValueText.text = GetLanguageCode();
            }

            if (wheelStatusText != null)
            {
                wheelStatusText.text = GetPanelBody(null);
            }
        }

        private void OpenWindow(GameObject window)
        {
            CloseActiveWindow();
            SetWindowVisible(modalBackdrop, window != null);
            SetWindowVisible(window, true);
        }

        private static void SetWindowVisible(GameObject window, bool visible)
        {
            if (window == null)
            {
                return;
            }

            if (!window.activeSelf)
            {
                window.SetActive(true);
            }

            var canvasGroup = window.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = window.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private static bool IsWindowVisible(GameObject window)
        {
            if (window == null || !window.activeSelf)
            {
                return false;
            }

            var canvasGroup = window.GetComponent<CanvasGroup>();
            return canvasGroup == null || canvasGroup.alpha > 0.01f;
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
                        : "Music " + ToPercent(settings.MusicVolume) + "\nSFX " + ToPercent(settings.SfxVolume) + "\nLanguage " + GetLanguageCode();
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

        private string GetLanguageCode()
        {
            return settings == null || string.IsNullOrWhiteSpace(settings.LanguageCode)
                ? "--"
                : settings.LanguageCode.ToUpperInvariant();
        }
    }
}
