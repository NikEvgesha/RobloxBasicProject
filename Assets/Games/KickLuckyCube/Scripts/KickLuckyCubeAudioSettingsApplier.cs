using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeAudioSettingsApplier : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeSettingsController settings;
        [SerializeField] private AudioSource[] musicSources;
        [SerializeField] private AudioSource[] sfxSources;
        [SerializeField] private bool autoDiscoverSceneChannels = true;
        [SerializeField] private bool controlGlobalListenerWhenNoSources = true;

        public float AppliedMusicVolume { get; private set; } = 1f;
        public float AppliedSfxVolume { get; private set; } = 1f;

        private KickLuckyCubeAudioChannel[] sceneChannels = System.Array.Empty<KickLuckyCubeAudioChannel>();

        private void Awake()
        {
            settings ??= FindFirstObjectByType<KickLuckyCubeSettingsController>(FindObjectsInactive.Include);
            RefreshSceneChannels();
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

            RefreshSceneChannels();
            ApplySources(musicSources, AppliedMusicVolume);
            ApplySources(sfxSources, AppliedSfxVolume);
            ApplyChannels(sceneChannels, AppliedMusicVolume, AppliedSfxVolume);

            if (controlGlobalListenerWhenNoSources && !HasAnySource())
            {
                AudioListener.volume = AppliedMusicVolume > 0.001f || AppliedSfxVolume > 0.001f ? 1f : 0f;
            }
        }

        private bool HasAnySource()
        {
            return HasSource(musicSources) || HasSource(sfxSources) || HasChannelSource(sceneChannels);
        }

        private void RefreshSceneChannels()
        {
            sceneChannels = autoDiscoverSceneChannels
                ? FindObjectsByType<KickLuckyCubeAudioChannel>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                : System.Array.Empty<KickLuckyCubeAudioChannel>();
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

        private static bool HasChannelSource(KickLuckyCubeAudioChannel[] channels)
        {
            if (channels == null)
            {
                return false;
            }

            foreach (var channel in channels)
            {
                if (channel != null && channel.Source != null)
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

        private static void ApplyChannels(KickLuckyCubeAudioChannel[] channels, float musicVolume, float sfxVolume)
        {
            if (channels == null)
            {
                return;
            }

            foreach (var channel in channels)
            {
                if (channel == null)
                {
                    continue;
                }

                var volume = channel.Channel == KickLuckyCubeAudioChannel.ChannelType.Music
                    ? musicVolume
                    : sfxVolume;
                channel.ApplyVolume(volume);
            }
        }
    }
}
