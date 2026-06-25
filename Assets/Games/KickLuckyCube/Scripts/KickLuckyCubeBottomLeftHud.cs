using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [ExecuteAlways]
    public sealed class KickLuckyCubeBottomLeftHud : MonoBehaviour
    {
        private const string KickPowerLabel = "\u0421\u0438\u043b\u0430 \u0443\u0434\u0430\u0440\u0430";
        private const string MaximumLabel = "(\u041c\u0410\u041a\u0421\u0418\u041c\u0423\u041c)";

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeRebirthController rebirthController;
        [SerializeField] private TMP_Text rebirthValueText;
        [SerializeField] private TMP_Text strengthValueText;
        [SerializeField] private TMP_Text strengthSuffixText;
        [SerializeField] private TMP_Text softValueText;
        [SerializeField] private TMP_Text hardValueText;
        [SerializeField] private Button masteryInfoButton;
        [SerializeField] private Button masteryInfoCloseButton;
        [SerializeField] private GameObject masteryInfoWindow;
        [SerializeField] private bool useEditModePreviewValues = true;
        [SerializeField, Min(0)] private int previewRebirthCount = 5;
        [SerializeField, Min(0f)] private float previewStrength = 7100f;
        [SerializeField, Min(0)] private int previewSoft = 1900000000;
        [SerializeField, Min(0)] private int previewHard = 100;

        private void Awake()
        {
            ResolveReferences();
            ApplyTextStyle();
            WireButtons();
            Refresh();
        }

        private void OnEnable()
        {
            ResolveReferences();
            WireButtons();

            if (stats != null)
            {
                stats.Changed += Refresh;
            }

            if (wallet != null)
            {
                wallet.Changed += OnWalletChanged;
            }

            ApplyTextStyle();
            Refresh();
        }

        private void OnDisable()
        {
            if (masteryInfoButton != null)
            {
                masteryInfoButton.onClick.RemoveListener(ShowMasteryInfo);
            }

            if (masteryInfoCloseButton != null)
            {
                masteryInfoCloseButton.onClick.RemoveListener(HideMasteryInfo);
            }

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
            if (!Application.isPlaying)
            {
                ResolveReferences();
            }

            Refresh();
        }

        private void OnValidate()
        {
            ResolveReferences();
            ApplyTextStyle();
            WireButtons();
            Refresh();
        }

        private void OnWalletChanged(int soft, int hard)
        {
            Refresh();
        }

        private void ResolveReferences()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            rebirthController ??= FindFirstObjectByType<KickLuckyCubeRebirthController>(FindObjectsInactive.Include);
            rebirthValueText ??= FindText("KLC_BottomLeftRebirthValue");
            strengthValueText ??= FindText("KLC_BottomLeftStrengthValue");
            strengthSuffixText ??= FindText("KLC_BottomLeftStrengthMax");
            softValueText ??= FindText("KLC_BottomLeftSoftValue");
            hardValueText ??= FindText("KLC_BottomLeftHardValue");
            masteryInfoButton ??= FindButton("KLC_BottomLeftMasteryInfoButton");
            masteryInfoCloseButton ??= FindButton("KLC_KickMasteryInfoCloseButton");
            masteryInfoWindow ??= GameObject.Find("KLC_KickMasteryInfoWindow");
        }

        private void Refresh()
        {
            var usePreview = !Application.isPlaying && useEditModePreviewValues;
            var rebirthCount = usePreview
                ? previewRebirthCount
                : Mathf.Max(0, rebirthController != null ? rebirthController.RebirthCount : 0);
            var strength = usePreview ? previewStrength : (stats != null ? stats.Strength : 0f);
            var soft = usePreview ? previewSoft : (wallet != null ? wallet.SoftCurrency : 0);
            var hard = usePreview ? previewHard : (wallet != null ? wallet.HardCurrency : 0);

            if (rebirthValueText != null)
            {
                rebirthValueText.text = rebirthCount.ToString(CultureInfo.InvariantCulture);
            }

            if (strengthValueText != null)
            {
                strengthValueText.text = $"{FormatCompact(strength)} {KickPowerLabel}";
            }

            if (strengthSuffixText != null)
            {
                strengthSuffixText.text = MaximumLabel;
            }

            if (softValueText != null)
            {
                softValueText.text = $"${FormatCompact(soft)}";
            }

            if (hardValueText != null)
            {
                hardValueText.text = FormatCompact(hard);
            }
        }

        private void ApplyTextStyle()
        {
            StyleOutlinedText(rebirthValueText, new Color(1f, 0.86f, 0.05f, 1f), 56, TextAlignmentOptions.Left);
            StyleOutlinedText(strengthValueText, new Color(1f, 0.66f, 0.02f, 1f), 36, TextAlignmentOptions.Left);
            StyleOutlinedText(strengthSuffixText, new Color(1f, 0.91f, 0.12f, 1f), 15, TextAlignmentOptions.Left);
            StyleOutlinedText(softValueText, new Color(0.42f, 1f, 0.03f, 1f), 58, TextAlignmentOptions.Left);
            StyleOutlinedText(hardValueText, new Color(1f, 0.54f, 1f, 1f), 54, TextAlignmentOptions.Left);
        }

        private void WireButtons()
        {
            if (masteryInfoButton != null)
            {
                masteryInfoButton.onClick.RemoveListener(ShowMasteryInfo);
                masteryInfoButton.onClick.AddListener(ShowMasteryInfo);
            }

            if (masteryInfoCloseButton != null)
            {
                masteryInfoCloseButton.onClick.RemoveListener(HideMasteryInfo);
                masteryInfoCloseButton.onClick.AddListener(HideMasteryInfo);
            }
        }

        private void ShowMasteryInfo()
        {
            if (masteryInfoWindow != null)
            {
                masteryInfoWindow.SetActive(true);
            }
        }

        private void HideMasteryInfo()
        {
            if (masteryInfoWindow != null)
            {
                masteryInfoWindow.SetActive(false);
            }
        }

        private static void StyleOutlinedText(TMP_Text text, Color color, int fontSize, TextAlignmentOptions alignment)
        {
            if (text == null)
            {
                return;
            }

            text.font = KickLuckyCubeUiTheme.TmpFont;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = alignment;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.color = color;
            text.raycastTarget = false;
            text.outlineColor = Color.black;
            text.outlineWidth = 0.16f;
        }

        private static TMP_Text FindText(string objectName)
        {
            var target = GameObject.Find(objectName);
            return target != null ? target.GetComponent<TMP_Text>() : null;
        }

        private static Button FindButton(string objectName)
        {
            var target = GameObject.Find(objectName);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private static string FormatCompact(float value)
        {
            return FormatCompact(Mathf.RoundToInt(Mathf.Max(0f, value)));
        }

        private static string FormatCompact(int value)
        {
            var abs = Mathf.Abs(value);
            if (abs >= 1000000000)
            {
                return FormatCompactScaled(value, 1000000000f, "B");
            }

            if (abs >= 1000000)
            {
                return FormatCompactScaled(value, 1000000f, "M");
            }

            if (abs >= 1000)
            {
                return FormatCompactScaled(value, 1000f, "K");
            }

            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatCompactScaled(int value, float scale, string suffix)
        {
            var scaled = value / scale;
            var format = scaled >= 100f ? "0" : "0.#";
            return scaled.ToString(format, CultureInfo.InvariantCulture) + suffix;
        }
    }
}
