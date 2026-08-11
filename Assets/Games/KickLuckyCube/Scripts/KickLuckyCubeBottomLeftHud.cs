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
        [SerializeField] private TMP_Text speedValueText;
        [SerializeField] private TMP_Text speedInfoButtonText;
        [SerializeField] private TMP_Text strengthValueText;
        [SerializeField] private TMP_Text strengthSuffixText;
        [SerializeField] private TMP_Text softValueText;
        [SerializeField] private TMP_Text hardValueText;
        [SerializeField] private Button speedTileButton;
        [SerializeField] private Button rebirthTileButton;
        [SerializeField] private Button strengthTileButton;
        [SerializeField] private Button softTileButton;
        [SerializeField] private Button hardTileButton;
        [SerializeField] private Button kickPowerSettingsButton;
        [SerializeField] private Button speedInfoButton;
        [SerializeField] private Button masteryInfoButton;
        [SerializeField] private Button masteryInfoCloseButton;
        [SerializeField] private GameObject masteryInfoWindow;
        [SerializeField] private bool useEditModePreviewValues = true;
        [SerializeField, Min(0)] private int previewRebirthCount = 5;
        [SerializeField, Min(0f)] private float previewSpeed = 7f;
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
            UnwireButton(speedTileButton, OpenSpeedShop);
            UnwireButton(rebirthTileButton, OpenKickShop);
            UnwireButton(strengthTileButton, ShowMasteryInfo);
            UnwireButton(softTileButton, OpenSoftShop);
            UnwireButton(hardTileButton, OpenHardShop);
            UnwireButton(kickPowerSettingsButton, OpenKickStrengthSettings);
            UnwireButton(speedInfoButton, OpenSpeedShop);

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

        private void OnWalletChanged(long soft, long hard)
        {
            Refresh();
        }

        private void ResolveReferences()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            rebirthController ??= FindFirstObjectByType<KickLuckyCubeRebirthController>(FindObjectsInactive.Include);
            rebirthValueText ??= FindText("KLC_BottomLeftRebirthValue");
            speedValueText ??= FindText("KLC_SpeedValue");
            speedInfoButtonText ??= FindText("KLC_SpeedInfoButton_Label");
            strengthValueText ??= FindText("KLC_BottomLeftStrengthValue");
            strengthSuffixText ??= FindText("KLC_BottomLeftStrengthMax");
            softValueText ??= FindText("KLC_BottomLeftSoftValue");
            hardValueText ??= FindText("KLC_BottomLeftHardValue");
            speedTileButton ??= FindButton("Speed");
            rebirthTileButton ??= FindButton("Rebirth");
            strengthTileButton ??= FindButton("Strength");
            softTileButton ??= FindButton("Soft");
            hardTileButton ??= FindButton("Hard");
            kickPowerSettingsButton ??= FindButton("KLC_BottomLeftKickPowerSettingsButton");
            speedInfoButton ??= FindButton("KLC_SpeedInfoButton");
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
            var speed = usePreview ? previewSpeed : (stats != null ? stats.AnimalSpeed : 0f);
            var strength = usePreview ? previewStrength : (stats != null ? stats.Strength : 0f);
            var soft = usePreview ? previewSoft : (wallet != null ? wallet.SoftCurrency : 0);
            var hard = usePreview ? previewHard : (wallet != null ? wallet.HardCurrency : 0);

            if (rebirthValueText != null)
            {
                rebirthValueText.text = rebirthCount.ToString(CultureInfo.InvariantCulture);
            }

            if (speedValueText != null)
            {
                speedValueText.text = speed.ToString("0.0", CultureInfo.InvariantCulture);
            }

            if (speedInfoButtonText != null)
            {
                speedInfoButtonText.text = "+";
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
            StyleOutlinedText(speedValueText, new Color(0.35f, 1f, 1f, 1f), 54, TextAlignmentOptions.Left);
            StyleOutlinedText(speedInfoButtonText, Color.white, 38, TextAlignmentOptions.Center);
            StyleOutlinedText(strengthValueText, new Color(1f, 0.66f, 0.02f, 1f), 36, TextAlignmentOptions.Left);
            StyleOutlinedText(strengthSuffixText, new Color(1f, 0.91f, 0.12f, 1f), 15, TextAlignmentOptions.Left);
            StyleOutlinedText(softValueText, new Color(0.42f, 1f, 0.03f, 1f), 58, TextAlignmentOptions.Left);
            StyleOutlinedText(hardValueText, new Color(1f, 0.54f, 1f, 1f), 54, TextAlignmentOptions.Left);
        }

        private void WireButtons()
        {
            WireButton(speedTileButton, OpenSpeedShop);
            WireButton(rebirthTileButton, OpenKickShop);
            WireButton(strengthTileButton, ShowMasteryInfo);
            WireButton(softTileButton, OpenSoftShop);
            WireButton(hardTileButton, OpenHardShop);
            WireButton(kickPowerSettingsButton, OpenKickStrengthSettings);
            WireButton(speedInfoButton, OpenSpeedShop);

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

        private void OpenSpeedShop()
        {
            var speedShop = FindFirstObjectByType<KickLuckyCubeSpeedShopController>(FindObjectsInactive.Include);
            if (speedShop != null)
            {
                speedShop.OpenWindow();
            }
        }

        private void OpenSoftShop()
        {
            OpenWindowController("KLC_ShopWindowController");
        }

        private void OpenHardShop()
        {
            var futureFeatures = FindFirstObjectByType<KickLuckyCubeFutureFeatureController>(FindObjectsInactive.Include);
            if (futureFeatures != null)
            {
                futureFeatures.Interact(KickLuckyCubeFutureFeature.EpicMobShop, null);
                return;
            }

            OpenWindowController("KLC_ShopWindowController");
        }

        private void OpenKickShop()
        {
            OpenWindowController("KLC_ShopWindowController");
        }

        private void OpenKickStrengthSettings()
        {
            var settings = FindFirstObjectByType<KickLuckyCubeKickStrengthSettingsController>(FindObjectsInactive.Include);
            settings?.OpenWindow();
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

        private static void WireButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private static void UnwireButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(action);
        }

        private static void OpenWindowController(string controllerName)
        {
            var controllerObject = GameObject.Find(controllerName);
            var windowController = controllerObject != null ? controllerObject.GetComponent<KickLuckyCubeUiWindowController>() : null;
            windowController?.OpenWindow();
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
            ApplyUiOutline(text);
        }

        private static void ApplyUiOutline(TMP_Text text)
        {
            if (text == null)
            {
                return;
            }

            var outline = text.GetComponent<Outline>();
            if (outline == null)
            {
                outline = text.gameObject.AddComponent<Outline>();
            }

            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2.5f, -2.5f);
            outline.useGraphicAlpha = true;
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
            return KickLuckyCubeNumberFormatter.FormatCompact((long)Mathf.Round(Mathf.Max(0f, value)));
        }

        private static string FormatCompact(long value)
        {
            return KickLuckyCubeNumberFormatter.FormatCompact(value);
        }
    }
}
