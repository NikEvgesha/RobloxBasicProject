using System;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    internal static class KickLuckyCubeGroundResolver
    {
        private const float MinimumWalkableNormalY = 0.52f;

        private static readonly string[] PreferredGroundNames =
        {
            "Floor",
            "Ground",
            "Road",
            "Path",
            "Bridge",
            "Ramp",
            "Slope",
            "Terrain",
            "Platform",
            "Plot",
            "Base",
        };

        private static readonly string[] RejectedGroundNames =
        {
            "Wall",
            "Gate",
            "Sign",
            "Label",
            "Board",
            "Kiosk",
            "Shop",
            "Tree",
            "Rock",
            "Mountain",
            "Cactus",
            "Fence",
            "Decor",
            "Trigger",
            "Wave",
            "LuckyCube",
        };

        public static bool TryResolveGroundY(
            Vector3 position,
            Transform ignoredRoot,
            float probeHeight,
            float probeDistance,
            float maximumStepUp,
            out float groundY)
        {
            probeHeight = Mathf.Max(0.1f, probeHeight);
            probeDistance = Mathf.Max(0.1f, probeDistance);
            maximumStepUp = Mathf.Max(0f, maximumStepUp);

            var rayOrigin = position + Vector3.up * probeHeight;
            var hits = Physics.RaycastAll(
                rayOrigin,
                Vector3.down,
                probeHeight + probeDistance,
                ~0,
                QueryTriggerInteraction.Ignore);

            var candidates = hits
                .Where(hit => IsWalkableCandidate(hit, position, ignoredRoot, maximumStepUp))
                .Select(hit => new GroundCandidate(hit, ResolvePriority(hit.collider)))
                .OrderBy(candidate => candidate.Priority)
                .ThenByDescending(candidate => candidate.Hit.point.y)
                .ThenBy(candidate => candidate.Hit.distance)
                .ToArray();

            if (candidates.Length == 0)
            {
                groundY = 0f;
                return false;
            }

            groundY = candidates[0].Hit.point.y;
            return true;
        }

        private static bool IsWalkableCandidate(
            RaycastHit hit,
            Vector3 position,
            Transform ignoredRoot,
            float maximumStepUp)
        {
            var collider = hit.collider;
            if (collider == null
                || collider.isTrigger
                || hit.normal.y < MinimumWalkableNormalY
                || hit.point.y > position.y + maximumStepUp)
            {
                return false;
            }

            if (ignoredRoot != null && collider.transform.IsChildOf(ignoredRoot))
            {
                return false;
            }

            if (collider.GetComponentInParent<KickLuckyCubeRarityZone>() != null
                || collider.GetComponentInParent<KickLuckyCubePlayerController>() != null
                || collider.GetComponentInParent<KickLuckyCubeAnimalRunner>() != null
                || collider.GetComponentInParent<KickLuckyCubeWaveChaseController>() != null)
            {
                return false;
            }

            var cursor = collider.transform;
            while (cursor != null)
            {
                var objectName = cursor.name;
                if (objectName.StartsWith("GUIDE_", StringComparison.Ordinal)
                    || objectName.StartsWith("KLC_Zone_", StringComparison.Ordinal)
                    || ContainsAny(objectName, RejectedGroundNames))
                {
                    return false;
                }

                cursor = cursor.parent;
            }

            return true;
        }

        private static int ResolvePriority(Collider collider)
        {
            if (collider.GetComponentInParent<KickLuckyCubeGroundSurface>() != null)
            {
                return 0;
            }

            var cursor = collider.transform;
            while (cursor != null)
            {
                if (ContainsAny(cursor.name, PreferredGroundNames))
                {
                    return 1;
                }

                cursor = cursor.parent;
            }

            return 2;
        }

        private static bool ContainsAny(string value, string[] fragments)
        {
            if (string.IsNullOrWhiteSpace(value) || fragments == null)
            {
                return false;
            }

            foreach (var fragment in fragments)
            {
                if (value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private readonly struct GroundCandidate
        {
            public GroundCandidate(RaycastHit hit, int priority)
            {
                Hit = hit;
                Priority = priority;
            }

            public RaycastHit Hit { get; }
            public int Priority { get; }
        }
    }
}
