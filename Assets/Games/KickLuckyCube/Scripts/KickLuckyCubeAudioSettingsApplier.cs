using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeAudioSettingsApplier : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeSettingsController settings;
        [SerializeField] private AudioSource[] musicSources;
        [SerializeField] private AudioSource[] sfxSources;
        [SerializeField] private bool controlGlobalListenerWhenNoSources = true;

        public float AppliedMusicVolume { get; private set; } = 1f;
        public float AppliedSfxVolume { get; private set; } = 1f;

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
            AppliedMusicVolume = settings != null ? settings.MusicVolume : 1f;
            AppliedSfxVolume = settings != null ? settings.SfxVolume : 1f;

            ApplySources(musicSources, AppliedMusicVolume);
            ApplySources(sfxSources, AppliedSfxVolume);

            if (controlGlobalListenerWhenNoSources && !HasAnySource())
            {
                AudioListener.volume = Mathf.Min(AppliedMusicVolume, AppliedSfxVolume);
            }
        }

        private bool HasAnySource()
        {
            return HasSource(musicSources) || HasSource(sfxSources);
        }

        private static bool HasSource(AudioSource[] sources)
        {
            if (sources == null)
            {
                return false;
            }

            foreach (var source in sources)
            {
                if (source != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplySources(AudioSource[] sources, float volume)
        {
            if (sources == null)
            {
                return;
            }

            foreach (var source in sources)
            {
                if (source == null)
                {
                    continue;
                }

                source.volume = volume;
                source.mute = volume <= 0.001f;
            }
        }
    }
}
