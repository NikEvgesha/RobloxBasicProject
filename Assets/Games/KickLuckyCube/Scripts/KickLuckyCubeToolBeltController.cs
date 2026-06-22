using System;
using UnityEngine;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeToolBeltController : MonoBehaviour
    {
        [Serializable]
        public sealed class ToolSlot
        {
            [SerializeField, Min(1)] private int tier = 1;
            [SerializeField] private Button button;
            [SerializeField] private Image frameImage;
            [SerializeField] private Image iconImage;
            [SerializeField] private Text labelText;

            public int Tier => Mathf.Max(1, tier);
            public Button Button => button;

            public void ApplyTheme()
            {
                if (frameImage != null)
                {
                    KickLuckyCubeUiTheme.AddImage(frameImage.gameObject, frameImage.color);
                }

                if (iconImage != null)
                {
                    KickLuckyCubeUiTheme.AddImage(iconImage.gameObject, iconImage.color);
                }

                KickLuckyCubeUiTheme.StyleButton(button, button != null ? button.gameObject.name : string.Empty);
                KickLuckyCubeUiTheme.StyleText(labelText, labelText != null ? labelText.gameObject.name : string.Empty);
            }

            public void Refresh(KickLuckyCubePlayerStats stats, string displayName, Color unlocked, Color selected, Color locked)
            {
                var owned = stats != null && Tier <= stats.StrengthToolTier;
                var isSelected = owned && stats.SelectedStrengthToolTier == Tier;

                if (button != null)
                {
                    button.interactable = owned;
                }

                if (frameImage != null)
                {
                    frameImage.color = !owned ? locked : isSelected ? selected : unlocked;
                }

                if (iconImage != null)
                {
                    iconImage.color = owned ? Color.white : new Color(1f, 1f, 1f, 0.35f);
                }

                if (labelText != null)
                {
                    labelText.text = owned ? displayName : "Locked";
                }
            }
        }

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private ToolSlot[] slots = Array.Empty<ToolSlot>();
        [SerializeField] private string[] toolNames = { "Kick", "Boots", "Hammer", "Rocket" };
        [SerializeField] private Color unlockedColor = KickLuckyCubeUiTheme.Secondary;
        [SerializeField] private Color selectedColor = KickLuckyCubeUiTheme.Warning;
        [SerializeField] private Color lockedColor = KickLuckyCubeUiTheme.CardDark;

        private void Awake()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            ApplyTheme();
            WireButtons();
            Refresh();
        }

        private void OnEnable()
        {
            if (stats != null)
            {
                stats.Changed += Refresh;
            }
        }

        private void OnDisable()
        {
            if (stats != null)
            {
                stats.Changed -= Refresh;
            }
        }

        private void OnDestroy()
        {
            UnwireButtons();
        }

        private void Update()
        {
            var keyboardTier = ReadKeyboardTier();
            if (keyboardTier > 0)
            {
                SelectTool(keyboardTier);
            }
        }

        public void SelectTool(int tier)
        {
            stats?.SelectStrengthToolTier(tier);
            Refresh();
        }

        public void Refresh()
        {
            if (slots == null)
            {
                return;
            }

            foreach (var slot in slots)
            {
                if (slot == null)
                {
                    continue;
                }

                slot.Refresh(stats, GetToolName(slot.Tier), unlockedColor, selectedColor, lockedColor);
            }
        }

        private void WireButtons()
        {
            if (slots == null)
            {
                return;
            }

            foreach (var slot in slots)
            {
                if (slot?.Button == null)
                {
                    continue;
                }

                var tier = slot.Tier;
                slot.Button.onClick.RemoveAllListeners();
                slot.Button.onClick.AddListener(() => SelectTool(tier));
            }
        }

        private void ApplyTheme()
        {
            if (slots == null)
            {
                return;
            }

            foreach (var slot in slots)
            {
                slot?.ApplyTheme();
            }
        }

        private void UnwireButtons()
        {
            if (slots == null)
            {
                return;
            }

            foreach (var slot in slots)
            {
                slot?.Button?.onClick.RemoveAllListeners();
            }
        }

        private string GetToolName(int tier)
        {
            var index = tier - 1;
            return toolNames != null && index >= 0 && index < toolNames.Length && !string.IsNullOrWhiteSpace(toolNames[index])
                ? toolNames[index]
                : $"Tool {tier}";
        }

        private static int ReadKeyboardTier()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return 0;
            }

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) return 1;
            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) return 2;
            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame) return 3;
            if (keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame) return 4;
            return 0;
#else
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 2;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 3;
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return 4;
            return 0;
#endif
        }
    }
}
