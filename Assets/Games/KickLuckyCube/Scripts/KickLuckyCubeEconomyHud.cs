using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeEconomyHud : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeStableCollectButton collectButton;
        [SerializeField] private Text hudText;
        [SerializeField] private TextMesh worldText;

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>();
            collectButton ??= FindFirstObjectByType<KickLuckyCubeStableCollectButton>();
            ApplyTheme();
        }

        private void OnEnable()
        {
            if (wallet != null)
            {
                wallet.Changed += OnWalletChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (wallet != null)
            {
                wallet.Changed -= OnWalletChanged;
            }
        }

        private void Update()
        {
            Refresh();
        }

        private void OnWalletChanged(long soft, long hard)
        {
            Refresh();
        }

        private void Refresh()
        {
            var soft = wallet != null ? wallet.SoftCurrency : 0;
            var hard = wallet != null ? wallet.HardCurrency : 0;
            var multiplier = wallet != null ? wallet.SoftGainMultiplier : 1f;
            var pending = collectButton != null ? collectButton.PendingSoft : 0;
            var text = $"Soft: {soft} (x{multiplier:0})\nHard: {hard}\nStable: {pending}";

            if (hudText != null)
            {
                hudText.text = text;
            }

            if (worldText != null)
            {
                worldText.text = text;
            }
        }

        private void ApplyTheme()
        {
            KickLuckyCubeUiTheme.StyleHudText(hudText);
            KickLuckyCubeUiTheme.StyleWorldText(worldText, Color.white, 0.009f);
        }
    }
}
