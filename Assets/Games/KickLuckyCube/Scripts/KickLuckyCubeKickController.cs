using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeKickController : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private Transform cube;
        [SerializeField] private Transform landingMarker;
        [SerializeField] private Text hudText;
        [SerializeField] private TextMesh worldStatusText;
        [SerializeField] private Renderer cubeRenderer;
        [SerializeField] private KickLuckyCubeRarityZone[] zones = Array.Empty<KickLuckyCubeRarityZone>();
        [SerializeField] private bool hideCubeUntilKickInPlayMode = true;
        [SerializeField, Min(0f)] private float kickHeightOffset = 0.7f;
        [SerializeField, Min(0f)] private float baseKickDistance = 18f;
        [SerializeField, Min(0f)] private float distancePerStrength = 0.23f;
        [SerializeField, Min(1f)] private float minimumDistance = 10f;
        [SerializeField, Min(1f)] private float maximumDistance = 132f;
        [SerializeField, Min(0.05f)] private float flightSeconds = 1.85f;
        [SerializeField, Min(0f)] private float arcHeight = 11f;

        private Coroutine flightRoutine;
        private bool kickLocked;
        private Vector3 lastKickOriginPosition;
        private Quaternion lastKickOriginRotation = Quaternion.identity;

        public event Action<KickLuckyCubeKickResult> Landed;

        public bool IsKicking { get; private set; }
        public float LastDistance { get; private set; }
        public KickLuckyCubeRarity LastLandedRarity { get; private set; }
        public string LastAnimalPool { get; private set; } = string.Empty;
        public bool CanKick => !kickLocked && !IsKicking && stats != null && cube != null && zones.Length > 0;

        private void Awake()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();

            if (cube == null)
            {
                cube = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(found => found.name == "KLC_LuckyCube");
            }

            if (cubeRenderer == null && cube != null)
            {
                cubeRenderer = cube.GetComponentInChildren<Renderer>();
            }

            if (zones == null || zones.Length == 0)
            {
                zones = FindObjectsByType<KickLuckyCubeRarityZone>(FindObjectsSortMode.None);
            }

            SortZones();
            ResolveKickOrigin(null, out lastKickOriginPosition, out lastKickOriginRotation);

            if (Application.isPlaying && hideCubeUntilKickInPlayMode)
            {
                ResetCubeToOrigin();
                SetCubeVisible(false);
            }
            else if (!Application.isPlaying)
            {
                SetCubeVisible(true);
            }
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                SetCubeVisible(true);
            }
        }

        private void OnEnable()
        {
            if (stats != null)
            {
                stats.Changed += RefreshStatus;
            }

            RefreshStatus();
        }

        private void OnDisable()
        {
            if (stats != null)
            {
                stats.Changed -= RefreshStatus;
            }
        }

        public void Kick(GameObject actor)
        {
            if (!CanKick)
            {
                return;
            }

            if (flightRoutine != null)
            {
                StopCoroutine(flightRoutine);
            }

            ResolveKickOrigin(actor, out lastKickOriginPosition, out lastKickOriginRotation);
            SetCubeVisible(true);
            cube.SetPositionAndRotation(lastKickOriginPosition, lastKickOriginRotation);

            var distance = CalculateDistance(stats.Strength);
            flightRoutine = StartCoroutine(PlayFlight(distance, lastKickOriginPosition));
        }

        public void SetKickLocked(bool value)
        {
            kickLocked = value;
            RefreshStatus();
        }

        public void ResetCubeToOrigin()
        {
            if (flightRoutine != null)
            {
                StopCoroutine(flightRoutine);
                flightRoutine = null;
            }

            IsKicking = false;
            LastLandedRarity = KickLuckyCubeRarity.None;
            LastAnimalPool = string.Empty;
            LastDistance = 0f;

            if (cube != null)
            {
                cube.SetPositionAndRotation(lastKickOriginPosition, lastKickOriginRotation);
            }

            if (Application.isPlaying && hideCubeUntilKickInPlayMode)
            {
                SetCubeVisible(false);
            }
            else if (!Application.isPlaying)
            {
                SetCubeVisible(true);
            }

            if (landingMarker != null)
            {
                landingMarker.gameObject.SetActive(false);
            }

            RefreshStatus();
        }

        public float CalculateDistance(float strength)
        {
            var rawDistance = baseKickDistance + Mathf.Max(0f, strength) * distancePerStrength;
            return Mathf.Clamp(rawDistance, minimumDistance, maximumDistance);
        }

        public KickLuckyCubeRarityZone ResolveLandingZone(float distance)
        {
            SortZones();

            var landingZ = lastKickOriginPosition.z + Mathf.Max(0f, distance);

            var reachedZone = zones
                .Where(zone => zone != null && zone.HasReached(landingZ))
                .OrderByDescending(zone => zone.StartZ)
                .FirstOrDefault();

            return reachedZone != null
                ? reachedZone
                : zones.FirstOrDefault(zone => zone != null);
        }

        public void CompleteKickInstant(float strength)
        {
            if (flightRoutine != null)
            {
                StopCoroutine(flightRoutine);
                flightRoutine = null;
            }

            IsKicking = false;
            SetCubeVisible(true);
            ResolveKickOrigin(null, out lastKickOriginPosition, out lastKickOriginRotation);
            LandAtDistance(CalculateDistance(strength));
        }

        private IEnumerator PlayFlight(float distance, Vector3 start)
        {
            IsKicking = true;
            LastLandedRarity = KickLuckyCubeRarity.None;
            LastAnimalPool = string.Empty;
            RefreshStatus("Kicking...");

            var end = start + Vector3.forward * distance;
            var elapsed = 0f;

            while (elapsed < flightSeconds)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / flightSeconds);
                var eased = Mathf.SmoothStep(0f, 1f, t);
                cube.position = Vector3.Lerp(start, end, eased) + Vector3.up * (Mathf.Sin(eased * Mathf.PI) * arcHeight);
                cube.Rotate(180f * Time.deltaTime, 240f * Time.deltaTime, 90f * Time.deltaTime, Space.World);
                yield return null;
            }

            IsKicking = false;
            flightRoutine = null;
            LandAtDistance(distance);
        }

        private void LandAtDistance(float distance)
        {
            SetCubeVisible(true);
            LastDistance = distance;

            var landingZone = ResolveLandingZone(distance);
            LastLandedRarity = landingZone != null ? landingZone.Rarity : KickLuckyCubeRarity.None;
            LastAnimalPool = landingZone != null ? landingZone.AnimalPoolText : string.Empty;

            var start = lastKickOriginPosition;
            var landingPosition = start + Vector3.forward * distance;
            landingPosition.y = start.y;
            cube.position = landingPosition;

            if (landingMarker != null)
            {
                landingMarker.gameObject.SetActive(true);
                landingMarker.position = new Vector3(landingPosition.x, 0.08f, landingPosition.z);
            }

            RefreshStatus();
            Landed?.Invoke(new KickLuckyCubeKickResult(
                distance,
                start,
                landingPosition,
                landingZone,
                LastLandedRarity,
                LastAnimalPool));
        }

        private void ResolveKickOrigin(GameObject actor, out Vector3 position, out Quaternion rotation)
        {
            var actorTransform = actor != null ? actor.transform : null;
            if (actorTransform != null)
            {
                position = actorTransform.position + Vector3.up * kickHeightOffset;
                rotation = actorTransform.rotation;
                return;
            }

            if (cube != null)
            {
                position = cube.position;
                rotation = cube.rotation;
                return;
            }

            position = transform.position;
            rotation = transform.rotation;
        }

        private void RefreshStatus()
        {
            RefreshStatus(null);
        }

        private void RefreshStatus(string overrideLine)
        {
            var strengthText = stats != null
                ? stats.Strength.ToString("0", CultureInfo.InvariantCulture)
                : "n/a";
            var speedText = stats != null
                ? stats.AnimalSpeed.ToString("0.0", CultureInfo.InvariantCulture)
                : "n/a";
            var toolText = stats != null
                ? $"{stats.SelectedStrengthToolTier.ToString(CultureInfo.InvariantCulture)}/{stats.StrengthToolTier.ToString(CultureInfo.InvariantCulture)}"
                : "n/a";
            var predictedDistance = stats != null
                ? CalculateDistance(stats.Strength).ToString("0.0", CultureInfo.InvariantCulture)
                : "n/a";

            var resultLine = string.IsNullOrEmpty(overrideLine)
                ? kickLocked
                    ? "Animal run in progress."
                    : LastLandedRarity == KickLuckyCubeRarity.None
                    ? "Hold E to kick."
                    : $"Landed: {LastLandedRarity} | {LastAnimalPool}"
                : overrideLine;

            var text = $"Strength: {strengthText} | Tool {toolText}\nSpeed: {speedText} | Kick: {predictedDistance} m\n{resultLine}";

            if (hudText != null)
            {
                hudText.text = text;
            }

            if (worldStatusText != null)
            {
                worldStatusText.text = text;
            }
        }

        private void SortZones()
        {
            if (zones == null)
            {
                zones = Array.Empty<KickLuckyCubeRarityZone>();
                return;
            }

            zones = zones
                .Where(zone => zone != null)
                .OrderBy(zone => zone.StartZ)
                .ToArray();
        }

        private void SetCubeVisible(bool visible)
        {
            if (cube == null)
            {
                return;
            }

            if (cube.gameObject.activeSelf != visible)
            {
                cube.gameObject.SetActive(visible);
            }
        }
    }
}
