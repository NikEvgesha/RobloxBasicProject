using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeAudioChannel : MonoBehaviour
    {
        public enum ChannelType
        {
            Music,
            Sfx
        }

        [SerializeField] private ChannelType channel = ChannelType.Sfx;
        [SerializeField] private AudioSource source;
        [SerializeField, Min(0f)] private float baseVolume = 1f;
        [SerializeField] private bool captureInitialVolumeOnAwake = true;

        public ChannelType Channel => channel;
        public AudioSource Source => source;

        private void Reset()
        {
            ResolveSource();
            CaptureCurrentVolume();
        }

        private void Awake()
        {
            ResolveSource();
            if (captureInitialVolumeOnAwake)
            {
                CaptureCurrentVolume();
            }
        }

        public void ApplyVolume(float settingsVolume)
        {
            ResolveSource();
            if (source == null)
            {
                return;
            }

            var resolvedVolume = Mathf.Max(0f, baseVolume) * Mathf.Clamp01(settingsVolume);
            source.volume = resolvedVolume;
            source.mute = resolvedVolume <= 0.001f;
        }

        private void ResolveSource()
        {
            source ??= GetComponent<AudioSource>();
        }

        private void CaptureCurrentVolume()
        {
            if (source != null)
            {
                baseVolume = Mathf.Max(0f, source.volume);
            }
        }
    }
}
