using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public readonly struct KickLuckyCubeKickStyleDefinition
    {
        public KickLuckyCubeKickStyleDefinition(
            string id,
            string displayName,
            string description,
            int hardCost,
            float strengthMultiplier,
            float motionDuration,
            Color accentColor,
            float showcaseYawOffset,
            float showcaseDistance,
            float showcasePitch,
            float showcaseFieldOfView,
            float showcaseLeadIn)
        {
            Id = id;
            DisplayName = displayName;
            Description = description;
            HardCost = hardCost;
            StrengthMultiplier = strengthMultiplier;
            MotionDuration = motionDuration;
            AccentColor = accentColor;
            ShowcaseYawOffset = showcaseYawOffset;
            ShowcaseDistance = showcaseDistance;
            ShowcasePitch = showcasePitch;
            ShowcaseFieldOfView = showcaseFieldOfView;
            ShowcaseLeadIn = showcaseLeadIn;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int HardCost { get; }
        public float StrengthMultiplier { get; }
        public float MotionDuration { get; }
        public Color AccentColor { get; }
        public float ShowcaseYawOffset { get; }
        public float ShowcaseDistance { get; }
        public float ShowcasePitch { get; }
        public float ShowcaseFieldOfView { get; }
        public float ShowcaseLeadIn { get; }
        public bool IsDefault => HardCost <= 0;
    }

    public readonly struct KickLuckyCubeKickStylePose
    {
        public KickLuckyCubeKickStylePose(Vector3 localPosition, Vector3 localEuler)
        {
            LocalPosition = localPosition;
            LocalEuler = localEuler;
        }

        public Vector3 LocalPosition { get; }
        public Vector3 LocalEuler { get; }
    }

    public static class KickLuckyCubeKickStyleCatalog
    {
        public const string SelectedStyleKey = "KickLuckyCube.KickStyle.SelectedId";
        public const string OwnedStyleKeyPrefix = "KickLuckyCube.KickStyle.Owned.";
        public const float PremiumStrengthMultiplier = 1.10f;
        public const float ImpactNormalizedTime = 0.78f;

        private static readonly KickLuckyCubeKickStyleDefinition[] Styles =
        {
            new("classic", "Classic Kick", "A clean straight kick. No bonus.", 0, 1f, 0.82f, new Color(1f, 0.82f, 0.20f), 150f, 4.1f, 10f, 52f, 0.12f),
            new("roundhouse", "Roundhouse", "A wide turning strike. +10% strength.", 80, PremiumStrengthMultiplier, 0.90f, new Color(1f, 0.38f, 0.16f), 210f, 4.4f, 9f, 50f, 0.24f),
            new("tornado", "Tornado Kick", "An airborne spinning strike. +10% strength.", 180, PremiumStrengthMultiplier, 1.00f, new Color(0.18f, 0.88f, 1f), 155f, 4.8f, 13f, 54f, 0.26f),
            new("bicycle", "Bicycle Kick", "A jumping overhead strike. +10% strength.", 400, PremiumStrengthMultiplier, 1.05f, new Color(0.44f, 0.60f, 1f), 200f, 5.0f, 16f, 55f, 0.28f),
            new("scorpion", "Scorpion Kick", "A forward dive and heel strike. +10% strength.", 850, PremiumStrengthMultiplier, 1.05f, new Color(0.80f, 0.24f, 1f), 165f, 4.6f, 8f, 49f, 0.26f),
            new("lightning_spiral", "Lightning Spiral", "A triple magical spin. +10% strength.", 1600, PremiumStrengthMultiplier, 1.10f, new Color(0.58f, 1f, 0.16f), 220f, 5.2f, 14f, 56f, 0.30f)
        };

        public static int Count => Styles.Length;
        public static int DefaultIndex => 0;

        public static KickLuckyCubeKickStyleDefinition Get(int index)
        {
            return Styles[Mathf.Clamp(index, 0, Styles.Length - 1)];
        }

        public static int FindIndex(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return DefaultIndex;
            }

            for (var index = 0; index < Styles.Length; index++)
            {
                if (string.Equals(Styles[index].Id, id, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return DefaultIndex;
        }

        public static bool IsOwned(int index)
        {
            var style = Get(index);
            return style.IsDefault || PlayerPrefs.GetInt(OwnedStyleKeyPrefix + style.Id, 0) != 0;
        }

        public static void Unlock(int index)
        {
            var style = Get(index);
            if (!style.IsDefault)
            {
                PlayerPrefs.SetInt(OwnedStyleKeyPrefix + style.Id, 1);
            }
        }

        public static int LoadSelectedIndex()
        {
            var selectedIndex = FindIndex(PlayerPrefs.GetString(SelectedStyleKey, Styles[DefaultIndex].Id));
            return IsOwned(selectedIndex) ? selectedIndex : DefaultIndex;
        }

        public static void SaveSelectedIndex(int index)
        {
            var safeIndex = Mathf.Clamp(index, 0, Styles.Length - 1);
            if (!IsOwned(safeIndex))
            {
                safeIndex = DefaultIndex;
            }

            PlayerPrefs.SetString(SelectedStyleKey, Styles[safeIndex].Id);
            PlayerPrefs.Save();
        }

        public static float ApplyStrengthMultiplier(int index, float strength)
        {
            return Mathf.Max(0f, strength) * Get(index).StrengthMultiplier;
        }

        public static KickLuckyCubeKickStylePose EvaluatePose(int index, float normalizedTime)
        {
            var t = Mathf.Clamp01(normalizedTime);
            var eased = Mathf.SmoothStep(0f, 1f, t);
            var arc = Mathf.Sin(t * Mathf.PI);

            return Mathf.Clamp(index, 0, Styles.Length - 1) switch
            {
                0 => new KickLuckyCubeKickStylePose(
                    new Vector3(0f, arc * 0.025f, 0f),
                    new Vector3(Mathf.Lerp(5f, -14f, eased), -8f * arc, Mathf.Lerp(-8f, 24f, eased))),
                1 => new KickLuckyCubeKickStylePose(
                    new Vector3(0f, arc * 0.10f, 0f),
                    new Vector3(-10f * arc, 340f * eased, -18f * arc)),
                2 => new KickLuckyCubeKickStylePose(
                    new Vector3(0f, arc * 0.32f, 0f),
                    new Vector3(-16f * arc, 700f * eased, 20f * arc)),
                3 => new KickLuckyCubeKickStylePose(
                    new Vector3(0f, arc * 0.42f, -0.08f * arc),
                    new Vector3(-76f * arc, 22f * arc, 12f * Mathf.Sin(t * Mathf.PI * 2f))),
                4 => new KickLuckyCubeKickStylePose(
                    new Vector3(0f, arc * 0.24f, 0.10f * arc),
                    new Vector3(82f * arc, -24f * arc, 18f * arc)),
                _ => new KickLuckyCubeKickStylePose(
                    new Vector3(0f, arc * 0.36f, 0f),
                    new Vector3(-20f * arc, 1080f * eased, 24f * Mathf.Sin(t * Mathf.PI * 3f)))
            };
        }
    }
}
