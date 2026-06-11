using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeRebirthController : MonoBehaviour
    {
        private const string RebirthCountKey = "Count";

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private GameObject windowRoot;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;
        [SerializeField] private Button rebirthButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text bodyText;
        [SerializeField] private Text requirementText;
        [SerializeField] private Text buttonText;
        [SerializeField] private Text statusText;
        [SerializeField, Min(1f)] private float baseStrengthRequirement = 1000f;
        [SerializeField, Min(1f)] private float requirementMultiplier = 10f;
        [SerializeField] private string saveKeyPrefix = "KickLuckyCube.Rebirth.";
        [SerializeField] private bool saveInPlayerPrefs = true;

        private int rebirthCount;

        public int RebirthCount => rebirthCount;
        public float MoneyMultiplier => 1f + rebirthCount;
        public float RequiredStrength => baseStrengthRequirement * Mathf.Pow(requirementMultiplier, rebirthCount);
        public bool CanRebirth => stats != null
            && stats.Strength >= RequiredStrength
            && (runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal));

        private void Awake()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>();
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>();

            Load();
            ApplyMoneyMultiplier();
            WireButtons();
            CloseWindow();
            Refresh();
        }

        private void OnEnable()
        {
            if (stats != null)
            {
                stats.Changed += Refresh;
            }

            if (wallet != null)
            {
                wallet.Changed += OnWalletChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (stats != null)
            {
                stats.Changed -= Refresh;
            }

            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
            }
        }

        private void Update()
        {
            if (windowRoot != null && windowRoot.activeSelf)
            {
                Refresh();
            }
        }

        public void OpenWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.SetActive(true);
            }

            Refresh();
        }

        public void CloseWindow()
        {
            if (windowRoot != null)
            {
                windowRoot.SetActive(false);
            }
        }

        public bool TryRebirth()
        {
            if (!CanRebirth)
            {
                Refresh("Need more strength.");
                return false;
            }

            rebirthCount++;
            stats.ResetStrengthForRebirth();
            ApplyMoneyMultiplier();
            Save();
            Refresh("Rebirth complete.");
            return true;
        }

        public void ResetRebirthForPrototype()
        {
            rebirthCount = 0;
            ApplyMoneyMultiplier();
            Save();
            Refresh();
        }

        private void WireButtons()
        {
            if (openButton != null)
            {
                openButton.onClick.RemoveListener(OpenWindow);
                openButton.onClick.AddListener(OpenWindow);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseWindow);
                closeButton.onClick.AddListener(CloseWindow);
            }

            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveListener(CloseWindow);
                backdropButton.onClick.AddListener(CloseWindow);
            }

            if (rebirthButton != null)
            {
                rebirthButton.onClick.RemoveListener(TryRebirthFromButton);
                rebirthButton.onClick.AddListener(TryRebirthFromButton);
            }
        }

        private void TryRebirthFromButton()
        {
            TryRebirth();
        }

        private void Refresh()
        {
            Refresh(null);
        }

        private void Refresh(string overrideStatus)
        {
            var strength = stats != null ? stats.Strength : 0f;
            var required = RequiredStrength;
            var nextMultiplier = MoneyMultiplier + 1f;
            var canRebirth = CanRebirth;

            if (titleText != null)
            {
                titleText.text = "Rebirth";
            }

            if (bodyText != null)
            {
                bodyText.text = $"Current multiplier: x{MoneyMultiplier.ToString("0", CultureInfo.InvariantCulture)}\nNext multiplier: x{nextMultiplier.ToString("0", CultureInfo.InvariantCulture)}\nResets strength only. Speed and tools stay owned.";
            }

            if (requirementText != null)
            {
                requirementText.text = $"Strength {strength:0} / {required:0}";
            }

            if (buttonText != null)
            {
                buttonText.text = canRebirth ? "Rebirth" : "Locked";
            }

            if (rebirthButton != null)
            {
                rebirthButton.interactable = canRebirth;
            }

            if (statusText != null)
            {
                statusText.text = !string.IsNullOrEmpty(overrideStatus)
                    ? overrideStatus
                    : $"Rebirths: {rebirthCount}\nSoft gain: x{MoneyMultiplier:0}";
            }
        }

        private void OnWalletChanged(int soft, int hard)
        {
            Refresh();
        }

        private void ApplyMoneyMultiplier()
        {
            wallet?.SetSoftGainMultiplier(MoneyMultiplier);
        }

        private void Load()
        {
            rebirthCount = Application.isPlaying && saveInPlayerPrefs
                ? PlayerPrefs.GetInt(saveKeyPrefix + RebirthCountKey, 0)
                : 0;
        }

        private void Save()
        {
            if (Application.isPlaying && saveInPlayerPrefs)
            {
                PlayerPrefs.SetInt(saveKeyPrefix + RebirthCountKey, rebirthCount);
            }
        }
    }
}
