using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    /// <summary>
    /// Shared dimensions for the Kick Lucky Cube progression corridor.
    /// Keeping these values in one place prevents gameplay, wave tiers and scene art from drifting apart.
    /// </summary>
    public static class KickLuckyCubeCorridorLayout
    {
        public const int LocationCount = 30;
        public const float FirstLocationStart = 7f;
        public const float LocationSpacing = 48.4f;
        public const float LocationPlayableLength = 40f;
        public const float MaximumKickDistance = 1463f;

        private const float LegacyDepthScale = 2f;

        public static float StretchLegacyDistance(float legacyDistance)
        {
            return FirstLocationStart + (legacyDistance - FirstLocationStart) * LegacyDepthScale;
        }

        public static int ResolveLocationIndex(float distance)
        {
            var distancePastStart = Mathf.Max(0f, distance - FirstLocationStart);
            return Mathf.Clamp(
                Mathf.FloorToInt(distancePastStart / LocationSpacing) + 1,
                1,
                LocationCount);
        }
    }
}
