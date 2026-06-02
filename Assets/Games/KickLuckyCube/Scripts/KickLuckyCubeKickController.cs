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
        [SerializeField] private CanvasGroup powerMeterGroup;
        [SerializeField] private Image powerMeterFill;
        [SerializeField] private RectTransform powerMeterMarker;
        [SerializeField] private Text powerMeterText;
        [SerializeField] private Renderer cubeRenderer;
        [SerializeField] private TrailRenderer cubeTrail;
        [SerializeField] private Transform landingSurface;
        [SerializeField] private KickLuckyCubeRarityZone[] zones = Array.Empty<KickLuckyCubeRarityZone>();
        [SerializeField] private bool hideCubeUntilKickInPlayMode = true;
        [SerializeField] private string carryAnchorName = "KLC_CarryAnchor";
        [SerializeField, Min(0f)] private float kickHeightOffset = 0.7f;
        [SerializeField, Min(0f)] private float landingMarkerSurfaceOffset = 0.08f;
        [SerializeField, Min(0f)] private float baseKickDistance = 18f;
        [SerializeField, Min(0f)] private float distancePerStrength = 0.23f;
        [SerializeField, Min(1f)] private float minimumDistance = 10f;
        [SerializeField, Min(1f)] private float maximumDistance = 132f;
        [SerializeField, Min(0.05f)] private float flightSeconds = 1.85f;
        [SerializeField, Min(0f)] private float arcHeight = 11f;
        [SerializeField, Range(0f, 1f)] private float initialPower = 0.5f;
        [SerializeField, Min(0.05f)] private float powerMeterSpeed = 1.4f;
        [SerializeField, Min(0f)] private float minimumPowerMultiplier = 0.45f;
        [SerializeField, Min(0f)] private float maximumPowerMultiplier = 1.2f;

        private Coroutine flightRoutine;
        private bool kickLocked;
        private bool isSelectingKickPower;
        private float currentPower;
        private float powerDirection = 1f;
        private Vector3 lastKickOriginPosition;
        private Quaternion lastKickOriginRotation = Quaternion.identity;
        private Vector3 pendingKickOriginPosition;
        private Quaternion pendingKickOriginRotation = Quaternion.identity;
        private Transform cubeOriginalParent;
        private Vector3 cubeOriginalLocalScale = Vector3.one;
        private bool cubeOriginalTransformStored;
        private GameObject cubePreviewActor;

        public event Action<KickLuckyCubeKickResult> Landed;

        public bool IsKicking { get; private set; }
        public bool IsSelectingKickPower => isSelectingKickPower;
        public bool IsCubeInFlight => IsKicking;
        public Transform CubeTransform => cube;
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

            if (cubeTrail == null && cube != null)
            {
                cubeTrail = cube.GetComponentInChildren<TrailRenderer>(true);
            }

            if (landingSurface == null)
            {
                landingSurface = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(found => found.name == "GUIDE_MainKickCorridor_FloorArea");
            }

            if (zones == null || zones.Length == 0)
            {
                zones = FindObjectsByType<KickLuckyCubeRarityZone>(FindObjectsSortMode.None);
            }

            CacheCubeOriginalTransform();
            ConfigureCubeTrail();
            SortZones();
            ResolveKickOrigin(null, out lastKickOriginPosition, out lastKickOriginRotation);
            HidePowerMeter();

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

        private void Update()
        {
            if (!isSelectingKickPower)
            {
                return;
            }

            currentPower += powerDirection * powerMeterSpeed * Time.unscaledDeltaTime;
            if (currentPower >= 1f)
            {
                currentPower = 1f;
                powerDirection = -1f;
            }
            else if (currentPower <= 0f)
            {
                currentPower = 0f;
                powerDirection = 1f;
            }

            RefreshPowerMeter();
            RefreshStatus("Press E again to kick.\nTop of the meter = stronger hit.");
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

            HidePowerMeter();
            SetCubeTrailEmitting(false);
        }

        public void Kick(GameObject actor)
        {
            if (isSelectingKickPower)
            {
                ConfirmKickPower();
                return;
            }

            if (!CanKick)
            {
                return;
            }

            if (flightRoutine != null)
            {
                StopCoroutine(flightRoutine);
            }

            BeginKickPowerSelection(actor);
        }

        public void ShowCubeInHands(GameObject actor)
        {
            if (actor == null || cube == null || IsKicking || kickLocked)
            {
                return;
            }

            var carryAnchor = ResolveCarryAnchor(actor.transform);
            if (carryAnchor == null)
            {
                return;
            }

            CacheCubeOriginalTransform();
            cubePreviewActor = actor;
            SetCubeTrailEmitting(false);
            SetCubeVisible(true);
            cube.SetParent(carryAnchor, false);
            cube.localPosition = Vector3.zero;
            cube.localRotation = Quaternion.identity;
            cube.localScale = cubeOriginalLocalScale;
        }

        public void HideCubeInHands(GameObject actor)
        {
            if (cube == null || IsKicking || isSelectingKickPower)
            {
                return;
            }

            if (actor != null && cubePreviewActor != null && cubePreviewActor != actor)
            {
                return;
            }

            cubePreviewActor = null;
            RestoreCubeParent();
            cube.SetPositionAndRotation(lastKickOriginPosition, lastKickOriginRotation);

            if (Application.isPlaying && hideCubeUntilKickInPlayMode)
            {
                SetCubeVisible(false);
            }
        }

        public void CancelKickPowerSelection()
        {
            if (!isSelectingKickPower)
            {
                return;
            }

            isSelectingKickPower = false;
            HidePowerMeter();
            RefreshStatus();

            if (Application.isPlaying && hideCubeUntilKickInPlayMode)
            {
                SetCubeVisible(false);
            }
        }

        private void BeginKickPowerSelection(GameObject actor)
        {
            ShowCubeInHands(actor);
            ResolveKickOrigin(actor, out pendingKickOriginPosition, out pendingKickOriginRotation);
            currentPower = initialPower;
            powerDirection = 1f;
            isSelectingKickPower = true;

            SetCubeTrailEmitting(false);
            SetCubeVisible(true);

            if (cubePreviewActor == null)
            {
                cube.SetPositionAndRotation(pendingKickOriginPosition, pendingKickOriginRotation);
            }

            if (landingMarker != null)
            {
                landingMarker.gameObject.SetActive(false);
            }

            RefreshPowerMeter();
            RefreshStatus("Choose kick power.\nPress E again to kick.");
        }

        private void ConfirmKickPower()
        {
            if (!CanKick)
            {
                CancelKickPowerSelection();
                return;
            }

            isSelectingKickPower = false;
            HidePowerMeter();

            lastKickOriginPosition = pendingKickOriginPosition;
            lastKickOriginRotation = pendingKickOriginRotation;
            DetachCubeFromHandsForFlight();
            SetCubeVisible(true);
            cube.SetPositionAndRotation(lastKickOriginPosition, lastKickOriginRotation);
            SetCubeTrailEmitting(true);

            var distance = CalculatePoweredDistance(stats.Strength, currentPower);
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

            isSelectingKickPower = false;
            HidePowerMeter();
            IsKicking = false;
            LastLandedRarity = KickLuckyCubeRarity.None;
            LastAnimalPool = string.Empty;
            LastDistance = 0f;
            cubePreviewActor = null;
            SetCubeTrailEmitting(false);

            if (cube != null)
            {
                RestoreCubeParent();
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

        public float CalculatePoweredDistance(float strength, float normalizedPower)
        {
            var baseDistance = CalculateDistance(strength);
            var powerMultiplier = Mathf.Lerp(
                minimumPowerMultiplier,
                Mathf.Max(minimumPowerMultiplier, maximumPowerMultiplier),
                Mathf.Clamp01(normalizedPower));
            return Mathf.Clamp(baseDistance * powerMultiplier, minimumDistance, maximumDistance);
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
            isSelectingKickPower = false;
            HidePowerMeter();
            DetachCubeFromHandsForFlight();
            SetCubeVisible(true);
            ResolveKickOrigin(null, out lastKickOriginPosition, out lastKickOriginRotation);
            SetCubeTrailEmitting(false);
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

            flightRoutine = null;
            LandAtDistance(distance);
            IsKicking = false;
        }

        private void LandAtDistance(float distance)
        {
            SetCubeVisible(true);
            SetCubeTrailEmitting(false);
            LastDistance = distance;

            var landingZone = ResolveLandingZone(distance);
            LastLandedRarity = landingZone != null ? landingZone.Rarity : KickLuckyCubeRarity.None;
            LastAnimalPool = landingZone != null ? landingZone.AnimalPoolText : string.Empty;

            var start = lastKickOriginPosition;
            var landingPosition = start + Vector3.forward * distance;
            var surfaceY = ResolveLandingSurfaceY(landingPosition);
            var cubeLandingPosition = new Vector3(
                landingPosition.x,
                surfaceY + ResolveCubeHalfHeight(),
                landingPosition.z);
            var resultLandingPosition = new Vector3(landingPosition.x, surfaceY, landingPosition.z);
            cube.position = cubeLandingPosition;

            if (landingMarker != null)
            {
                landingMarker.gameObject.SetActive(true);
                landingMarker.position = new Vector3(
                    resultLandingPosition.x,
                    surfaceY + landingMarkerSurfaceOffset,
                    resultLandingPosition.z);
            }

            SetCubeVisible(false);
            RefreshStatus();
            Landed?.Invoke(new KickLuckyCubeKickResult(
                distance,
                start,
                resultLandingPosition,
                landingZone,
                LastLandedRarity,
                LastAnimalPool));
        }

        private void ResolveKickOrigin(GameObject actor, out Vector3 position, out Quaternion rotation)
        {
            var actorTransform = actor != null ? actor.transform : null;
            if (actorTransform != null)
            {
                var carryAnchor = ResolveCarryAnchor(actorTransform);
                if (carryAnchor != null)
                {
                    position = carryAnchor.position;
                    rotation = carryAnchor.rotation;
                }
                else
                {
                    position = actorTransform.position + Vector3.up * kickHeightOffset;
                    rotation = actorTransform.rotation;
                }

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
                ? CalculatePoweredDistance(stats.Strength, 1f).ToString("0.0", CultureInfo.InvariantCulture)
                : "n/a";

            var resultLine = string.IsNullOrEmpty(overrideLine)
                ? kickLocked
                    ? "Animal run in progress."
                    : isSelectingKickPower
                    ? "Press E again to kick."
                    : LastLandedRarity == KickLuckyCubeRarity.None
                    ? "Press E to aim kick."
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

        private void RefreshPowerMeter()
        {
            if (powerMeterGroup != null)
            {
                powerMeterGroup.alpha = isSelectingKickPower ? 1f : 0f;
                powerMeterGroup.interactable = false;
                powerMeterGroup.blocksRaycasts = false;
            }

            if (powerMeterFill != null)
            {
                powerMeterFill.fillAmount = Mathf.Clamp01(currentPower);
            }

            if (powerMeterMarker != null)
            {
                var anchor = powerMeterMarker.anchorMin;
                anchor.y = Mathf.Clamp01(currentPower);
                powerMeterMarker.anchorMin = anchor;

                anchor = powerMeterMarker.anchorMax;
                anchor.y = Mathf.Clamp01(currentPower);
                powerMeterMarker.anchorMax = anchor;
                powerMeterMarker.anchoredPosition = Vector2.zero;
            }

            if (powerMeterText != null)
            {
                var powerPercent = Mathf.RoundToInt(Mathf.Clamp01(currentPower) * 100f);
                powerMeterText.text = $"Kick Power {powerPercent}%";
            }
        }

        private void HidePowerMeter()
        {
            if (powerMeterGroup != null)
            {
                powerMeterGroup.alpha = 0f;
                powerMeterGroup.interactable = false;
                powerMeterGroup.blocksRaycasts = false;
            }

            if (powerMeterFill != null)
            {
                powerMeterFill.fillAmount = 0f;
            }

            if (powerMeterText != null)
            {
                powerMeterText.text = string.Empty;
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

        private void CacheCubeOriginalTransform()
        {
            if (cube == null || cubeOriginalTransformStored)
            {
                return;
            }

            cubeOriginalParent = cube.parent;
            cubeOriginalLocalScale = cube.localScale;
            cubeOriginalTransformStored = true;
        }

        private void RestoreCubeParent()
        {
            if (cube == null || !cubeOriginalTransformStored)
            {
                return;
            }

            if (cube.parent != cubeOriginalParent)
            {
                cube.SetParent(cubeOriginalParent, false);
            }

            cube.localScale = cubeOriginalLocalScale;
        }

        private void DetachCubeFromHandsForFlight()
        {
            if (cube == null)
            {
                return;
            }

            var worldPosition = cube.position;
            var worldRotation = cube.rotation;
            cubePreviewActor = null;
            RestoreCubeParent();
            cube.SetPositionAndRotation(worldPosition, worldRotation);
        }

        private Transform ResolveCarryAnchor(Transform actorTransform)
        {
            if (actorTransform == null || string.IsNullOrWhiteSpace(carryAnchorName))
            {
                return null;
            }

            return actorTransform
                .GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child => child.name == carryAnchorName);
        }

        private float ResolveLandingSurfaceY(Vector3 landingPosition)
        {
            if (landingSurface != null)
            {
                var surfaceRenderer = landingSurface.GetComponentInChildren<Renderer>();
                if (surfaceRenderer != null)
                {
                    return surfaceRenderer.bounds.max.y;
                }

                return landingSurface.position.y;
            }

            var rayOrigin = new Vector3(landingPosition.x, lastKickOriginPosition.y + arcHeight + 20f, landingPosition.z);
            if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, 200f, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.point.y;
            }

            return lastKickOriginPosition.y;
        }

        private float ResolveCubeHalfHeight()
        {
            if (cubeRenderer != null)
            {
                return Mathf.Max(0.01f, cubeRenderer.bounds.extents.y);
            }

            return cube != null
                ? Mathf.Max(0.01f, cube.lossyScale.y * 0.5f)
                : 0.5f;
        }

        private void ConfigureCubeTrail()
        {
            if (cubeTrail == null)
            {
                return;
            }

            cubeTrail.emitting = false;
            cubeTrail.time = Mathf.Max(0.1f, cubeTrail.time <= 0f ? 0.45f : cubeTrail.time);
            cubeTrail.widthMultiplier = cubeTrail.widthMultiplier <= 0f ? 0.18f : cubeTrail.widthMultiplier;
            cubeTrail.minVertexDistance = Mathf.Max(0.02f, cubeTrail.minVertexDistance);
            cubeTrail.autodestruct = false;

            if (cubeTrail.widthCurve == null || cubeTrail.widthCurve.length == 0)
            {
                cubeTrail.widthCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            }
        }

        private void SetCubeTrailEmitting(bool emitting)
        {
            if (cubeTrail == null)
            {
                return;
            }

            if (emitting)
            {
                cubeTrail.Clear();
            }

            cubeTrail.emitting = emitting;
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
