using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [CreateAssetMenu(fileName = "KickLuckyCubeBalanceConfig", menuName = "Kick Lucky Cube/Balance Config")]
    public sealed class KickLuckyCubeBalanceConfig : ScriptableObject
    {
        public const string DefaultResourcePath = "KickLuckyCube/KickLuckyCubeBalanceConfig";

        private static KickLuckyCubeBalanceConfig cachedDefault;
        private static KickLuckyCubeBalanceConfig runtimeFallback;

        [Header("Kick Distance")]
        [SerializeField, Min(0f)] private float minimumKickDistance = 10f;
        [SerializeField, Min(1f)] private float maximumKickDistance = KickLuckyCubeCorridorLayout.MaximumKickDistance;
        [SerializeField, Min(0.01f)] private float minimumPowerMultiplier = 0.55f;
        [SerializeField, Min(0.01f)] private float maximumPowerMultiplier = 1.12f;
        [SerializeField] private AnimationCurve strengthToDistance = CreateDefaultStrengthToDistanceCurve();

        [Header("Speed")]
        [SerializeField, Min(0.01f)] private float speedGainPerLevel = 0.1f;
        [SerializeField, Min(0)] private int speedBaseCost = 35;
        [SerializeField, Min(1f)] private float speedCostMultiplier = 1.06f;

        [Header("Strength Tools")]
        [SerializeField] private string[] toolNames = CreateDefaultToolNames();
        [SerializeField] private float[] toolStrengthPerSecond = CreateDefaultToolStrengthPerSecond();
        [SerializeField] private int[] toolCosts = CreateDefaultToolCosts();

        [Header("Rebirth")]
        [SerializeField, Min(1f)] private float rebirthBaseStrengthRequirement = 1500f;
        [SerializeField, Min(1f)] private float rebirthRequirementMultiplier = 30f;

        [Header("Animal Income")]
        [SerializeField, Min(0)] private int kickAnimalIncomeStart = 30;
        [SerializeField, Min(0)] private int kickAnimalIncomeStep = 6;
        [SerializeField, Min(0)] private int kickAnimalSellSeconds = 18;

        public float SpeedGainPerLevel => Mathf.Max(0.01f, speedGainPerLevel);
        public int MaxToolTier => Mathf.Max(
            1,
            Mathf.Max(
                toolNames != null ? toolNames.Length : 0,
                Mathf.Max(
                    toolStrengthPerSecond != null ? toolStrengthPerSecond.Length : 0,
                    toolCosts != null ? toolCosts.Length : 0)));

        public static KickLuckyCubeBalanceConfig GetOrLoadDefault()
        {
            if (cachedDefault != null)
            {
                return cachedDefault;
            }

            cachedDefault = Resources.Load<KickLuckyCubeBalanceConfig>(DefaultResourcePath);
            if (cachedDefault != null)
            {
                return cachedDefault;
            }

            if (runtimeFallback == null)
            {
                runtimeFallback = CreateInstance<KickLuckyCubeBalanceConfig>();
                runtimeFallback.hideFlags = HideFlags.DontSave;
                runtimeFallback.ResetToFrontLoadedDefaults();
            }

            return runtimeFallback;
        }

        public void ResetToFrontLoadedDefaults()
        {
            minimumKickDistance = 10f;
            maximumKickDistance = KickLuckyCubeCorridorLayout.MaximumKickDistance;
            minimumPowerMultiplier = 0.55f;
            maximumPowerMultiplier = 1.12f;
            strengthToDistance = CreateDefaultStrengthToDistanceCurve();

            speedGainPerLevel = 0.1f;
            speedBaseCost = 35;
            speedCostMultiplier = 1.06f;

            toolNames = CreateDefaultToolNames();
            toolStrengthPerSecond = CreateDefaultToolStrengthPerSecond();
            toolCosts = CreateDefaultToolCosts();

            rebirthBaseStrengthRequirement = 1500f;
            rebirthRequirementMultiplier = 30f;

            kickAnimalIncomeStart = 30;
            kickAnimalIncomeStep = 6;
            kickAnimalSellSeconds = 18;
        }

        public float CalculateKickDistance(float strength)
        {
            var safeStrength = Mathf.Max(0f, strength);
            var rawDistance = strengthToDistance != null && strengthToDistance.length > 0
                ? strengthToDistance.Evaluate(safeStrength)
                : FallbackLogDistance(safeStrength);

            if (float.IsNaN(rawDistance) || float.IsInfinity(rawDistance))
            {
                rawDistance = minimumKickDistance;
            }

            return Mathf.Clamp(
                rawDistance,
                Mathf.Max(0f, minimumKickDistance),
                Mathf.Max(minimumKickDistance, maximumKickDistance));
        }

        public float ApplyKickPower(float baseDistance, float normalizedPower)
        {
            var powerMultiplier = Mathf.Lerp(
                Mathf.Max(0.01f, minimumPowerMultiplier),
                Mathf.Max(minimumPowerMultiplier, maximumPowerMultiplier),
                Mathf.Clamp01(normalizedPower));

            return Mathf.Clamp(
                baseDistance * powerMultiplier,
                Mathf.Max(0f, minimumKickDistance),
                Mathf.Max(minimumKickDistance, maximumKickDistance));
        }

        public long GetSpeedLevelCost(int level)
        {
            var safeLevel = Math.Max(1, level);
            var rawCost = speedBaseCost * Math.Pow(Math.Max(1.0, speedCostMultiplier), safeLevel - 1);
            return ClampToIntCost(rawCost);
        }

        public long GetTotalSpeedLevelCost(int currentLevel, int levels)
        {
            var total = 0L;
            var safeLevels = Math.Max(1, levels);
            for (var offset = 1; offset <= safeLevels; offset++)
            {
                total = SaturatingAdd(total, GetSpeedLevelCost(currentLevel + offset));
            }

            return total;
        }

        public string GetToolName(int tier)
        {
            var safeTier = Mathf.Max(1, tier);
            if (toolNames != null)
            {
                var index = Mathf.Clamp(safeTier, 1, Mathf.Max(1, toolNames.Length)) - 1;
                if (index >= 0 && index < toolNames.Length && !string.IsNullOrWhiteSpace(toolNames[index]))
                {
                    return toolNames[index];
                }
            }

            return "Tool " + safeTier.ToString();
        }

        public float GetToolStrengthPerSecond(int tier)
        {
            var safeTier = Mathf.Max(1, tier);
            if (toolStrengthPerSecond != null)
            {
                var index = Mathf.Clamp(safeTier, 1, Mathf.Max(1, toolStrengthPerSecond.Length)) - 1;
                if (index >= 0 && index < toolStrengthPerSecond.Length && toolStrengthPerSecond[index] > 0f)
                {
                    return toolStrengthPerSecond[index];
                }
            }

            return 2f * Mathf.Pow(2.3f, safeTier - 1);
        }

        public int GetToolCost(int tier)
        {
            var safeTier = Mathf.Max(1, tier);
            if (safeTier <= 1)
            {
                return 0;
            }

            if (toolCosts != null)
            {
                var index = Mathf.Clamp(safeTier, 1, Mathf.Max(1, toolCosts.Length)) - 1;
                if (index >= 0 && index < toolCosts.Length && toolCosts[index] >= 0)
                {
                    return toolCosts[index];
                }
            }

            return (int)ClampToIntCost(1000d * Math.Pow(2.8d, safeTier - 1));
        }

        public float GetRebirthRequirement(int rebirthCount)
        {
            var rawRequirement = rebirthBaseStrengthRequirement
                * Mathf.Pow(Mathf.Max(1f, rebirthRequirementMultiplier), Mathf.Max(0, rebirthCount));
            return float.IsNaN(rawRequirement) || float.IsInfinity(rawRequirement)
                ? float.MaxValue
                : Mathf.Max(1f, rawRequirement);
        }

        public int GetKickAnimalIncome(int progressionIndex)
        {
            var rawIncome = kickAnimalIncomeStart + Math.Max(0, progressionIndex) * (long)kickAnimalIncomeStep;
            return rawIncome > int.MaxValue ? int.MaxValue : Mathf.Max(0, (int)rawIncome);
        }

        public int GetKickAnimalSellValue(int progressionIndex, int incomePerSecond)
        {
            var scaledSell = incomePerSecond * (long)Mathf.Max(0, kickAnimalSellSeconds);
            return scaledSell > int.MaxValue ? int.MaxValue : Mathf.Max(0, (int)scaledSell);
        }

        private static long ClampToIntCost(double rawCost)
        {
            if (double.IsNaN(rawCost) || rawCost <= 0d)
            {
                return 0L;
            }

            if (double.IsInfinity(rawCost) || rawCost > int.MaxValue)
            {
                return int.MaxValue;
            }

            return Math.Max(0L, (long)Math.Ceiling(rawCost));
        }

        private static long SaturatingAdd(long left, long right)
        {
            return long.MaxValue - left <= right ? long.MaxValue : left + right;
        }

        private static float FallbackLogDistance(float strength)
        {
            const float finalStrength = 2600000000f;
            var progress = Mathf.Pow(
                Mathf.Log(1f + Mathf.Max(0f, strength) / 20f) / Mathf.Log(1f + finalStrength / 20f),
                0.8f);
            return Mathf.Lerp(
                KickLuckyCubeCorridorLayout.StretchLegacyDistance(10f),
                KickLuckyCubeCorridorLayout.MaximumKickDistance,
                Mathf.Clamp01(progress));
        }

        private static AnimationCurve CreateDefaultStrengthToDistanceCurve()
        {
            var curve = new AnimationCurve(
                new Keyframe(0f, 10f),
                new Keyframe(20f, 18f),
                new Keyframe(40f, 31f),
                new Keyframe(80f, 55.4f),
                new Keyframe(137f, 79.6f),
                new Keyframe(234f, 103.8f),
                new Keyframe(400f, 128f),
                new Keyframe(607f, 152.2f),
                new Keyframe(922f, 176.4f),
                new Keyframe(1400f, 200.6f),
                new Keyframe(2274f, 224.8f),
                new Keyframe(3694f, 249f),
                new Keyframe(6000f, 273.2f),
                new Keyframe(10801f, 297.4f),
                new Keyframe(19443f, 321.6f),
                new Keyframe(35000f, 345.8f),
                new Keyframe(64593f, 370f),
                new Keyframe(119208f, 394.2f),
                new Keyframe(220000f, 418.4f),
                new Keyframe(417169f, 442.6f),
                new Keyframe(791046f, 466.8f),
                new Keyframe(1500000f, 491f),
                new Keyframe(3000000f, 515.2f),
                new Keyframe(6000000f, 539.4f),
                new Keyframe(12000000f, 563.6f),
                new Keyframe(26552288f, 587.8f),
                new Keyframe(58751999f, 612f),
                new Keyframe(130000000f, 636.2f),
                new Keyframe(352874290f, 660.4f),
                new Keyframe(957848190f, 684.6f),
                new Keyframe(2600000000f, 708.8f),
                new Keyframe(5000000000f, 735f));

            var keys = curve.keys;
            for (var index = 0; index < keys.Length; index++)
            {
                keys[index].value = KickLuckyCubeCorridorLayout.StretchLegacyDistance(keys[index].value);
            }

            curve.keys = keys;
            return curve;
        }

        private static string[] CreateDefaultToolNames()
        {
            return new[]
            {
                "Stone Dumbbells",
                "Iron Barbell",
                "Steel Dumbbells",
                "Gold Barbell",
                "Titanium Dumbbells",
                "Obsidian Barbell",
                "Neon Alloy Dumbbells",
                "Meteorite Barbell",
                "Crystal Dumbbells",
                "Sapphire Barbell",
                "Amethyst Dumbbells",
                "Voidsteel Barbell",
                "Ruby Dumbbells",
                "Emerald Barbell",
                "Arcane Godstone Dumbbells"
            };
        }

        private static float[] CreateDefaultToolStrengthPerSecond()
        {
            return new[]
            {
                2f,
                5f,
                11f,
                24f,
                56f,
                129f,
                296f,
                681f,
                1566f,
                3602f,
                8285f,
                19056f,
                43829f,
                100807f,
                231857f
            };
        }

        private static int[] CreateDefaultToolCosts()
        {
            return new[]
            {
                0,
                100,
                330,
                1100,
                3600,
                12000,
                39000,
                130000,
                430000,
                1400000,
                4700000,
                15500000,
                51000000,
                168000000,
                555000000
            };
        }

        private void Reset()
        {
            ResetToFrontLoadedDefaults();
        }
    }
}
