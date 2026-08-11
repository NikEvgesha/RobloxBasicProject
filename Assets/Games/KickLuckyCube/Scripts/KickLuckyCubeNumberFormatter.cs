using System;
using System.Globalization;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    internal static class KickLuckyCubeNumberFormatter
    {
        private static readonly (long Threshold, string Suffix)[] CompactUnits =
        {
            (1_000_000_000_000_000_000L, "Qi"),
            (1_000_000_000_000_000L, "Qa"),
            (1_000_000_000_000L, "T"),
            (1_000_000_000L, "B"),
            (1_000_000L, "M"),
            (1_000L, "K"),
        };

        public static string FormatCompact(long value)
        {
            var magnitude = value == long.MinValue ? long.MaxValue : Math.Abs(value);
            foreach (var unit in CompactUnits)
            {
                if (magnitude < unit.Threshold)
                {
                    continue;
                }

                var scaled = value / (double)unit.Threshold;
                var format = Math.Abs(scaled) >= 100d ? "0" : "0.#";
                return scaled.ToString(format, CultureInfo.InvariantCulture) + unit.Suffix;
            }

            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
