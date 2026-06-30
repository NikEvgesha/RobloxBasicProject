using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeLocalizedText : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeSettingsController settings;
        [SerializeField] private string key;
        [SerializeField] private Text uiText;
        [SerializeField] private TMP_Text tmpText;
        [SerializeField] private TextMesh worldText;
        [SerializeField, TextArea] private string english;
        [SerializeField, TextArea] private string russian;
        [SerializeField] private bool captureCurrentTextAsEnglish = true;

        public string Key => key;

        private void Reset()
        {
            ResolveTargets();
            CaptureEnglishIfEmpty();
        }

        private void OnValidate()
        {
            ResolveTargets();
            CaptureEnglishIfEmpty();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (settings != null)
            {
                settings.Changed += ApplyFromSettings;
            }

            ApplyFromSettings();
        }

        private void OnDisable()
        {
            if (settings != null)
            {
                settings.Changed -= ApplyFromSettings;
            }
        }

        public void Apply(string languageCode)
        {
            var value = ResolveValue(languageCode);
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

        private void ApplyFromSettings()
        {
            Apply(settings != null ? settings.LanguageCode : "EN");
        }

        private string ResolveValue(string languageCode)
        {
            var useRussian = string.Equals(languageCode, "RU", StringComparison.OrdinalIgnoreCase);
            var value = useRussian ? russian : english;
            if (string.IsNullOrWhiteSpace(value))
            {
                value = english;
            }

            return value;
        }

        private void ResolveReferences()
        {
            settings ??= FindFirstObjectByType<KickLuckyCubeSettingsController>(FindObjectsInactive.Include);
            ResolveTargets();
            CaptureEnglishIfEmpty();
        }

        private void ResolveTargets()
        {
            uiText ??= GetComponent<Text>();
            tmpText ??= GetComponent<TMP_Text>();
            worldText ??= GetComponent<TextMesh>();
        }

        private void CaptureEnglishIfEmpty()
        {
            if (!captureCurrentTextAsEnglish || !string.IsNullOrWhiteSpace(english))
            {
                return;
            }

            english = uiText != null ? uiText.text : tmpText != null ? tmpText.text : worldText != null ? worldText.text : english;
        }
    }
}
