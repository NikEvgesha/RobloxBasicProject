using System;
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
        [SerializeField] private bool expiresAutomatically;

        private float pendingStrength;
        private float expiresAt;
        private bool visible;

        public event Action<float> Claimed;

        public bool IsVisible => visible;
        public float PendingStrength => pendingStrength;

        private void Awake()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            canvasGroup ??= GetComponent<CanvasGroup>();
            EnsureRuntimeButton();

            if (claimButton != null)
            {
                claimButton.onClick.RemoveListener(ClaimBonusFromButton);
                claimButton.onClick.AddListener(ClaimBonusFromButton);
            }

            ConfigureCircleOnlyPresentation();
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

            if (expiresAutomatically && Time.unscaledTime >= expiresAt)
            {
                HideImmediate();
            }
        }

        public void ShowBonus(float strengthAmount, int toolTier)
        {
            if (visible || strengthAmount <= 0f)
            {
                return;
            }

            pendingStrength = strengthAmount;
            expiresAt = Time.unscaledTime + expiresSeconds;
            visible = true;
            ConfigureCircleOnlyPresentation();

            if (headlineText != null)
            {
                headlineText.text = "x2";
            }

            if (amountText != null)
            {
                amountText.text = string.Empty;
                amountText.gameObject.SetActive(false);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        private void ConfigureCircleOnlyPresentation()
        {
            var rootRect = transform as RectTransform;
            if (rootRect != null)
            {
                rootRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 86f);
                rootRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 86f);
            }

            var rootImage = GetComponent<Image>();
            if (rootImage == null)
            {
                rootImage = KickLuckyCubeUiTheme.AddImage(gameObject, KickLuckyCubeUiTheme.ActionPlate);
            }

            if (rootImage != null)
            {
                KickLuckyCubeUiTheme.AddImage(rootImage.gameObject, KickLuckyCubeUiTheme.ActionPlate);
                rootImage.color = KickLuckyCubeUiTheme.ActionPlate;
                rootImage.raycastTarget = true;
            }

            var iconFrame = transform.Find("IconFrame") as RectTransform;
            Image iconFrameImage = null;
            if (iconFrame != null)
            {
                iconFrame.anchorMin = new Vector2(0.5f, 0.5f);
                iconFrame.anchorMax = new Vector2(0.5f, 0.5f);
                iconFrame.pivot = new Vector2(0.5f, 0.5f);
                iconFrame.anchoredPosition = Vector2.zero;
                iconFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 82f);
                iconFrame.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 82f);
                iconFrameImage = iconFrame.GetComponent<Image>();
                if (iconFrameImage != null)
                {
                    KickLuckyCubeUiTheme.AddImage(iconFrameImage.gameObject, KickLuckyCubeUiTheme.Primary);
                    iconFrameImage.color = KickLuckyCubeUiTheme.Primary;
                    iconFrameImage.raycastTarget = true;
                }
            }

            var icon = transform.Find("IconFrame/Icon");
            if (icon != null)
            {
                icon.gameObject.SetActive(false);
            }

            if (headlineText != null)
            {
                var headlineRect = headlineText.transform as RectTransform;
                if (headlineRect != null)
                {
                    headlineRect.anchorMin = new Vector2(0.5f, 0.5f);
                    headlineRect.anchorMax = new Vector2(0.5f, 0.5f);
                    headlineRect.pivot = new Vector2(0.5f, 0.5f);
                    headlineRect.anchoredPosition = Vector2.zero;
                    headlineRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 82f);
                    headlineRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 82f);
                }

                headlineText.text = "x2";
                headlineText.alignment = TextAnchor.MiddleCenter;
                headlineText.fontSize = 38;
                headlineText.raycastTarget = false;
                KickLuckyCubeUiTheme.StyleText(headlineText, "Title");
            }

            if (amountText != null)
            {
                amountText.text = string.Empty;
                amountText.gameObject.SetActive(false);
            }

            if (claimButton != null)
            {
                claimButton.targetGraphic = iconFrameImage != null ? iconFrameImage : rootImage;
                KickLuckyCubeUiTheme.StyleButton(claimButton, "ActionButton");
                claimButton.interactable = visible;
            }
        }

        private void EnsureRuntimeButton()
        {
            canvasGroup ??= gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            claimButton = gameObject.GetComponent<Button>() ?? gameObject.AddComponent<Button>();
        }

        public bool ClaimBonus()
        {
            if (!visible || pendingStrength <= 0f)
            {
                return false;
            }

            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            var claimedStrength = pendingStrength;
            stats?.AddStrength(claimedStrength);
            HideImmediate();
            Claimed?.Invoke(claimedStrength);
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

            if (claimButton != null)
            {
                claimButton.interactable = false;
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
