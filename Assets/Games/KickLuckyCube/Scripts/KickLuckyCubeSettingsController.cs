using UnityEngine;
using UnityEngine.UI;
using System;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeSettingsController : MonoBehaviour
    {
        private const string MusicKey = "KickLuckyCube.Settings.Music";
        private const string SfxKey = "KickLuckyCube.Settings.Sfx";
        private const string LanguageKey = "KickLuckyCube.Settings.Language";

        [SerializeField] private Button musicButton;
        [SerializeField] private Button sfxButton;
        [SerializeField] private Button languageButton;
        [SerializeField] private Text musicStateText;
        [SerializeField] private Text sfxStateText;
        [SerializeField] private Text languageStateText;
        [SerializeField] private RectTransform musicKnob;
        [SerializeField] private RectTransform sfxKnob;
        [SerializeField] private Image musicTrack;
        [SerializeField] private Image sfxTrack;
        [SerializeField] private Image languageTrack;
        [SerializeField] private Color enabledColor = new(0.28f, 0.95f, 0.16f, 1f);
        [SerializeField] private Color disabledColor = new(0.94f, 0.12f, 0.27f, 1f);
        [SerializeField] private Color languageColor = new(0.13f, 0.66f, 0.95f, 1f);
        [SerializeField] private string[] languageCodes = { "EN", "RU" };

        private bool musicEnabled = true;
        private bool sfxEnabled = true;
        private int languageIndex;

        public event Action Changed;

        public bool MusicEnabled => musicEnabled;
        public bool SfxEnabled => sfxEnabled;
        public float MusicVolume => MusicEnabled ? 1f : 0f;
        public float SfxVolume => SfxEnabled ? 1f : 0f;
        public string LanguageCode => languageCodes.Length > 0
            ? languageCodes[Mathf.Clamp(languageIndex, 0, languageCodes.Length - 1)]
            : "EN";

        private void Awake()
        {
            LoadSettings();
            WireButtons();
            RefreshView();
        }

        private void OnDestroy()
        {
            UnwireButtons();
        }

        public void ToggleMusic()
        {
            SetMusicEnabled(!musicEnabled);
        }

        public void ToggleSfx()
        {
            SetSfxEnabled(!sfxEnabled);
        }

        public void CycleLanguage()
        {
            var count = Mathf.Max(1, languageCodes.Length);
            SetLanguageIndex((languageIndex + 1) % count);
        }

        public void SetMusicEnabled(bool isEnabled)
        {
            if (musicEnabled == isEnabled)
            {
                RefreshView();
                return;
            }

            musicEnabled = isEnabled;
            SaveSettings();
            RefreshView();
            Changed?.Invoke();
        }

        public void SetSfxEnabled(bool isEnabled)
        {
            if (sfxEnabled == isEnabled)
            {
                RefreshView();
                return;
            }

            sfxEnabled = isEnabled;
            SaveSettings();
            RefreshView();
            Changed?.Invoke();
        }

        public void SetLanguageIndex(int index)
        {
            var maxIndex = Mathf.Max(0, languageCodes.Length - 1);
            var nextIndex = Mathf.Clamp(index, 0, maxIndex);
            if (languageIndex == nextIndex)
            {
                RefreshView();
                return;
            }

            languageIndex = nextIndex;
            SaveSettings();
            RefreshView();
            Changed?.Invoke();
        }

        public void RefreshView()
        {
            ApplyToggleView(musicStateText, musicTrack, musicKnob, musicEnabled);
            ApplyToggleView(sfxStateText, sfxTrack, sfxKnob, sfxEnabled);

            if (languageStateText != null)
            {
                languageStateText.text = LanguageCode;
            }

            if (languageTrack != null)
            {
                languageTrack.color = languageColor;
            }
        }

        private void LoadSettings()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            musicEnabled = PlayerPrefs.GetInt(MusicKey, 1) == 1;
            sfxEnabled = PlayerPrefs.GetInt(SfxKey, 1) == 1;
            languageIndex = PlayerPrefs.GetInt(LanguageKey, 0);
        }

        private void SaveSettings()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            PlayerPrefs.SetInt(MusicKey, musicEnabled ? 1 : 0);
            PlayerPrefs.SetInt(SfxKey, sfxEnabled ? 1 : 0);
            PlayerPrefs.SetInt(LanguageKey, languageIndex);
        }

        private void WireButtons()
        {
            if (musicButton != null)
            {
                musicButton.onClick.RemoveListener(ToggleMusic);
                musicButton.onClick.AddListener(ToggleMusic);
            }

            if (sfxButton != null)
            {
                sfxButton.onClick.RemoveListener(ToggleSfx);
                sfxButton.onClick.AddListener(ToggleSfx);
            }

            if (languageButton != null)
            {
                languageButton.onClick.RemoveListener(CycleLanguage);
                languageButton.onClick.AddListener(CycleLanguage);
            }
        }

        private void UnwireButtons()
        {
            if (musicButton != null)
            {
                musicButton.onClick.RemoveListener(ToggleMusic);
            }

            if (sfxButton != null)
            {
                sfxButton.onClick.RemoveListener(ToggleSfx);
            }

            if (languageButton != null)
            {
                languageButton.onClick.RemoveListener(CycleLanguage);
            }
        }

        private void ApplyToggleView(Text stateText, Image trackImage, RectTransform knob, bool isEnabled)
        {
            if (stateText != null)
            {
                stateText.text = isEnabled ? "ON" : "OFF";
            }

            if (trackImage != null)
            {
                trackImage.color = isEnabled ? enabledColor : disabledColor;
            }

            if (knob != null)
            {
                var anchoredPosition = knob.anchoredPosition;
                anchoredPosition.x = isEnabled ? 26f : -26f;
                knob.anchoredPosition = anchoredPosition;
            }
        }
    }
}
