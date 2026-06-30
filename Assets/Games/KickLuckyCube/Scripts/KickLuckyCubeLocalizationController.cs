using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeLocalizationController : MonoBehaviour
    {
        [Serializable]
        public sealed class LocalizedTextEntry
        {
            [SerializeField] private string key;
            [SerializeField] private Text uiText;
            [SerializeField] private TMP_Text tmpText;
            [SerializeField] private TextMesh worldText;
            [SerializeField] private string english;
            [SerializeField] private string russian;

            public string Key => key;

            public void Apply(string languageCode)
            {
                var value = string.Equals(languageCode, "RU", StringComparison.OrdinalIgnoreCase)
                    ? russian
                    : english;

                if (string.IsNullOrWhiteSpace(value))
                {
                    value = english;
                }

                if (uiText != null)
                {
                    uiText.text = value;
                }

                if (tmpText != null)
                {
                    tmpText.text = value;
                }

                if (worldText != null)
                {
                    worldText.text = value;
                }
            }
        }

        [SerializeField] private KickLuckyCubeSettingsController settings;
        [SerializeField] private LocalizedTextEntry[] entries = Array.Empty<LocalizedTextEntry>();

        public string LanguageCode => settings != null ? settings.LanguageCode : "EN";

        private void Awake()
        {
            settings ??= FindFirstObjectByType<KickLuckyCubeSettingsController>(FindObjectsInactive.Include);
            Apply();
        }

        private void OnEnable()
        {
            if (settings != null)
            {
                settings.Changed += Apply;
            }

            Apply();
        }

        private void OnDisable()
        {
            if (settings != null)
            {
                settings.Changed -= Apply;
            }
        }

        public void Apply()
        {
            var languageCode = LanguageCode;
            if (entries == null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                entry?.Apply(languageCode);
            }
        }
    }
}
