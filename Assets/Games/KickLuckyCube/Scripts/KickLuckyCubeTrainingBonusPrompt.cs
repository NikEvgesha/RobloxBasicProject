using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeTrainingBonusPrompt : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button claimButton;
        [SerializeField] private Text headlineText;
        [SerializeField] private Text amountText;
        [SerializeField, Min(0.2f)] private float expiresSeconds = 2.4f;

        private float pendingStrength;
        private float expiresAt;
        private bool visible;

        public bool IsVisible => visible;
        public float PendingStrength => pendingStrength;

        private void Awake()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            canvasGroup ??= GetComponent<CanvasGroup>();
            claimButton ??= GetComponentInChildren<Button>(true);

            if (claimButton != null)
            {
                claimButton.onClick.AddListener(ClaimBonusFromButton);
            }

            HideImmediate();
        }

        private void OnDestroy()
        {
            if (claimButton != null)
            {
                claimButton.onClick.RemoveListener(ClaimBonusFromButton);
            }
        }

        private void Update()
        {
            if (!visible)
            {
                return;
            }

            if (ReadClaimPressed())
            {
                ClaimBonus();
                return;
            }

            if (Time.unscaledTime >= expiresAt)
            {
                HideImmediate();
            }
        }

        public void ShowBonus(float strengthAmount, int toolTier)
        {
            if (strengthAmount <= 0f)
            {
                return;
            }

            pendingStrength = strengthAmount;
            expiresAt = Time.unscaledTime + expiresSeconds;
            visible = true;

            if (headlineText != null)
            {
                headlineText.text = "x2";
            }

            if (amountText != null)
            {
                amountText.text = $"+{pendingStrength:0} strength\nPress X";
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        public bool ClaimBonus()
        {
            if (!visible || pendingStrength <= 0f)
            {
                return false;
            }

            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            stats?.AddStrength(pendingStrength);
            HideImmediate();
            return true;
        }

        private void ClaimBonusFromButton()
        {
            ClaimBonus();
        }

        public void HideImmediate()
        {
            pendingStrength = 0f;
            visible = false;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        private static bool ReadClaimPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.xKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.X);
#endif
        }
    }
}
