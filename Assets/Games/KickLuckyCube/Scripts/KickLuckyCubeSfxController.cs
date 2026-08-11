using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public enum KickLuckyCubeSfxType
    {
        Money,
        Purchase,
        Place,
        Take,
        Upgrade,
        Deny
    }

    [DefaultExecutionOrder(-200)]
    public sealed class KickLuckyCubeSfxController : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeSettingsController settings;
        [SerializeField] private AudioSource audioSource;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.62f;

        private static KickLuckyCubeSfxController instance;
        private readonly Dictionary<KickLuckyCubeSfxType, AudioClip[]> clips = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeSfxController>(FindObjectsInactive.Include) != null
                || FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include) == null)
            {
                return;
            }

            new GameObject("KLC_SfxController_Runtime").AddComponent<KickLuckyCubeSfxController>();
        }

        public static void Play(KickLuckyCubeSfxType type)
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<KickLuckyCubeSfxController>(FindObjectsInactive.Include);
            }

            instance?.PlayLocal(type);
        }

        private void Awake()
        {
            instance = this;
            ResolveReferences();
            EnsureClips();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (wallet != null)
            {
                wallet.CurrencyGained += OnCurrencyGained;
                wallet.CurrencySpent += OnCurrencySpent;
            }
        }

        private void OnDisable()
        {
            if (wallet != null)
            {
                wallet.CurrencyGained -= OnCurrencyGained;
                wallet.CurrencySpent -= OnCurrencySpent;
            }
        }

        private void ResolveReferences()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            settings ??= FindFirstObjectByType<KickLuckyCubeSettingsController>(FindObjectsInactive.Include);
            if (audioSource == null)
            {
                var existingSource = GetComponent<AudioSource>();
                audioSource = existingSource != null ? existingSource : gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }

        private void OnCurrencyGained(long soft, long hard)
        {
            if (soft > 0 || hard > 0)
            {
                PlayLocal(KickLuckyCubeSfxType.Money);
            }
        }

        private void OnCurrencySpent(long soft, long hard)
        {
            if (soft > 0 || hard > 0)
            {
                PlayLocal(KickLuckyCubeSfxType.Purchase);
            }
        }

        private void PlayLocal(KickLuckyCubeSfxType type)
        {
            ResolveReferences();
            EnsureClips();
            if (audioSource == null || settings != null && settings.SfxVolume <= 0.001f)
            {
                return;
            }

            if (!clips.TryGetValue(type, out var sequence) || sequence.Length == 0)
            {
                return;
            }

            StopCoroutine(nameof(PlaySequence));
            StartCoroutine(PlaySequence(sequence));
        }

        private IEnumerator PlaySequence(AudioClip[] sequence)
        {
            var volume = masterVolume * (settings != null ? settings.SfxVolume : 1f);
            for (var index = 0; index < sequence.Length; index++)
            {
                var clip = sequence[index];
                if (clip != null)
                {
                    audioSource.PlayOneShot(clip, volume);
                }

                if (index < sequence.Length - 1)
                {
                    yield return new WaitForSecondsRealtime(0.045f);
                }
            }
        }

        private void EnsureClips()
        {
            if (clips.Count > 0)
            {
                return;
            }

            clips[KickLuckyCubeSfxType.Money] = new[]
            {
                CreateTone("KLC_Sfx_Money_1", 980f, 0.08f, WaveShape.Sine, 0.72f),
                CreateTone("KLC_Sfx_Money_2", 1320f, 0.08f, WaveShape.Sine, 0.64f),
                CreateTone("KLC_Sfx_Money_3", 1760f, 0.09f, WaveShape.Sine, 0.54f)
            };
            clips[KickLuckyCubeSfxType.Purchase] = new[]
            {
                CreateTone("KLC_Sfx_Purchase_1", 660f, 0.07f, WaveShape.Triangle, 0.62f),
                CreateTone("KLC_Sfx_Purchase_2", 880f, 0.08f, WaveShape.Triangle, 0.58f)
            };
            clips[KickLuckyCubeSfxType.Place] = new[]
            {
                CreateTone("KLC_Sfx_Place_Low", 220f, 0.07f, WaveShape.Sine, 0.66f),
                CreateTone("KLC_Sfx_Place_High", 440f, 0.08f, WaveShape.Triangle, 0.52f)
            };
            clips[KickLuckyCubeSfxType.Take] = new[]
            {
                CreateTone("KLC_Sfx_Take_1", 520f, 0.06f, WaveShape.Triangle, 0.55f),
                CreateTone("KLC_Sfx_Take_2", 760f, 0.07f, WaveShape.Triangle, 0.55f)
            };
            clips[KickLuckyCubeSfxType.Upgrade] = new[]
            {
                CreateTone("KLC_Sfx_Upgrade_1", 740f, 0.07f, WaveShape.Sine, 0.62f),
                CreateTone("KLC_Sfx_Upgrade_2", 1110f, 0.08f, WaveShape.Sine, 0.62f),
                CreateTone("KLC_Sfx_Upgrade_3", 1480f, 0.10f, WaveShape.Sine, 0.58f)
            };
            clips[KickLuckyCubeSfxType.Deny] = new[]
            {
                CreateTone("KLC_Sfx_Deny", 160f, 0.14f, WaveShape.Square, 0.38f)
            };
        }

        private static AudioClip CreateTone(string clipName, float frequency, float seconds, WaveShape shape, float amplitude)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * seconds));
            var data = new float[sampleCount];
            for (var index = 0; index < sampleCount; index++)
            {
                var t = index / (float)sampleRate;
                var normalized = index / (float)(sampleCount - 1);
                var envelope = Mathf.Sin(Mathf.PI * normalized);
                data[index] = ResolveWave(shape, frequency, t) * amplitude * envelope;
            }

            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float ResolveWave(WaveShape shape, float frequency, float time)
        {
            var phase = frequency * time;
            return shape switch
            {
                WaveShape.Square => Mathf.Repeat(phase, 1f) < 0.5f ? 1f : -1f,
                WaveShape.Triangle => 1f - 4f * Mathf.Abs(Mathf.Round(phase - 0.25f) - (phase - 0.25f)),
                _ => Mathf.Sin(Mathf.PI * 2f * phase)
            };
        }

        private enum WaveShape
        {
            Sine,
            Triangle,
            Square
        }
    }
}
