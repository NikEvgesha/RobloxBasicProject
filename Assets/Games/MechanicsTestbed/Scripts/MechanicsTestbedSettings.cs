using System;
using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedSettings : MonoBehaviour
    {
        private static readonly string[] SupportedLanguages = { "ru", "en" };

        [SerializeField] private string saveKeyPrefix = "MechanicsTestbed.Settings.";
        [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.85f;
        [SerializeField] private string defaultLanguageCode = "ru";

        private float musicVolume;
        private float sfxVolume;
        private string languageCode;

        public event Action Changed;

        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;
        public string LanguageCode => languageCode;

        private void Awake()
        {
            Load();
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            SaveAndNotify();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            SaveAndNotify();
        }

        public void SetLanguageCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                value = defaultLanguageCode;
            }

            languageCode = value.Trim().ToLowerInvariant();
            SaveAndNotify();
        }

        public void CycleLanguage()
        {
            var index = Array.IndexOf(SupportedLanguages, languageCode);
            index = index < 0 ? 0 : (index + 1) % SupportedLanguages.Length;
            SetLanguageCode(SupportedLanguages[index]);
        }

        private void Load()
        {
            musicVolume = PlayerPrefs.GetFloat(saveKeyPrefix + "music", defaultMusicVolume);
            sfxVolume = PlayerPrefs.GetFloat(saveKeyPrefix + "sfx", defaultSfxVolume);
            languageCode = PlayerPrefs.GetString(saveKeyPrefix + "language", defaultLanguageCode);
            Changed?.Invoke();
        }

        private void SaveAndNotify()
        {
            PlayerPrefs.SetFloat(saveKeyPrefix + "music", musicVolume);
            PlayerPrefs.SetFloat(saveKeyPrefix + "sfx", sfxVolume);
            PlayerPrefs.SetString(saveKeyPrefix + "language", languageCode);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
