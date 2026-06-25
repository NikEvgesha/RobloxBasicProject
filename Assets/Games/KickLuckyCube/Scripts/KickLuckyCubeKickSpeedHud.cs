using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [ExecuteAlways]
    public sealed class KickLuckyCubeKickSpeedHud : MonoBehaviour
    {
        private const string SpeedLabel = "\u0421\u043a\u043e\u0440\u043e\u0441\u0442\u044c";
        private const string UpgradeLabel = "\u0423\u043b\u0443\u0447\u0448.";

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeSpeedShopController speedShop;
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text upgradeButtonText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private bool useEditModePreviewValues = true;
        [SerializeField, Min(0f)] private float previewSpeed = 7f;
        [SerializeField, Min(0)] private int previewSpeedLevel;

        private void Awake()
        {
            ResolveReferences();
            ApplyTextStyle();
            WireButton();
            Refresh();
        }

        private void OnEnable()
        {
            ResolveReferences();
            WireButton();

            if (stats != null)
            {
                stats.Changed += Refresh;
            }

            ApplyTextStyle();
            Refresh();
        }

        private void OnDisable()
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveListener(OpenSpeedShop);
            }

            if (stats != null)
            {
                stats.Changed -= Refresh;
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
            WireButton();
            Refresh();
        }

        private void ResolveReferences()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            speedShop ??= FindFirstObjectByType<KickLuckyCubeSpeedShopController>(FindObjectsInactive.Include);
            speedText ??= FindText("KLC_KickSpeedHudValue");
            levelText ??= FindText("KLC_KickSpeedHudLevel");
            upgradeButtonText ??= FindText("KLC_KickSpeedHudUpgradeLabel");
            upgradeButton ??= FindButton("KLC_KickSpeedHudUpgradeButton");
        }

        private void Refresh()
        {
            var usePreview = !Application.isPlaying && useEditModePreviewValues;
            var speed = usePreview ? previewSpeed : (stats != null ? stats.AnimalSpeed : 0f);
            var level = usePreview ? previewSpeedLevel : (stats != null ? stats.SpeedUpgradeLevel : 0);

            if (speedText != null)
            {
                speedText.text = $"{SpeedLabel} {speed.ToString("0.0", CultureInfo.InvariantCulture)}";
            }

            if (levelText != null)
            {
                levelText.text = $"Lv {level.ToString(CultureInfo.InvariantCulture)}";
            }

            if (upgradeButtonText != null)
            {
                upgradeButtonText.text = UpgradeLabel;
            }
        }

        private void ApplyTextStyle()
        {
            StyleOutlinedText(speedText, new Color(0.35f, 1f, 1f, 1f), 27, TextAlignmentOptions.Left);
            StyleOutlinedText(levelText, Color.white, 17, TextAlignmentOptions.Left);
            StyleOutlinedText(upgradeButtonText, Color.white, 18, TextAlignmentOptions.Center);
        }

        private void WireButton()
        {
            if (upgradeButton == null)
            {
                return;
            }

            upgradeButton.onClick.RemoveListener(OpenSpeedShop);
            upgradeButton.onClick.AddListener(OpenSpeedShop);
        }

        private void OpenSpeedShop()
        {
            ResolveReferences();
            speedShop?.OpenWindow();
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
            text.outlineWidth = 0.14f;
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
    }
}
