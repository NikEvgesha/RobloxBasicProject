using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeKickController : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeBalanceConfig balanceConfig;
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeInventoryController inventory;
        [SerializeField] private KickLuckyCubeKickStrengthSettingsController kickStrengthSettings;
        [SerializeField] private KickLuckyCubeStyleShopController kickStyleShop;
        [SerializeField] private KickLuckyCubeThirdPersonCamera showcaseCamera;
        [SerializeField] private Transform cube;
        [SerializeField] private Transform landingMarker;
        [SerializeField] private Text hudText;
        [SerializeField] private TextMesh worldStatusText;
        [SerializeField] private bool showHudText;
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
        [SerializeField, Min(0.01f)] private float powerSelectionMoveCancelDistance = 0.16f;
        [SerializeField, Min(0f)] private float kickHeightOffset = 0.7f;
        [SerializeField, Min(0f)] private float baseKickDistance = 18f;
        [SerializeField, Min(0f)] private float distancePerStrength = 0.23f;
        [SerializeField, Min(1f)] private float minimumDistance = 10f;
        [SerializeField, Min(1f)] private float maximumDistance = KickLuckyCubeCorridorLayout.MaximumKickDistance;
        [SerializeField, Min(0.05f)] private float flightSeconds = 1.85f;
        [SerializeField, Min(0f)] private float arcHeight = 11f;
        [SerializeField, Range(0f, 1f)] private float initialPower = 0.5f;
        [SerializeField, Min(0.05f)] private float powerMeterSpeed = 1.4f;
        [SerializeField, Min(0f)] private float minimumPowerMultiplier = 0.45f;
        [SerializeField, Min(0f)] private float maximumPowerMultiplier = 1.2f;
        [SerializeField] private Vector3 kickShowcaseFocusOffset = new(0f, 1.0f, 0f);
        [SerializeField, Min(0f)] private float kickShowcaseTransitionSeconds = 0.18f;
        [SerializeField, Min(0f)] private float kickShowcaseContactHoldSeconds = 0.12f;
        [SerializeField] private AnimationCurve kickShowcaseEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Coroutine flightRoutine;
        private Coroutine stylePreviewRoutine;
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
        private GameObject powerSelectionActor;
        private Vector3 powerSelectionActorStartPosition;
        private int powerSelectionStartedFrame = -1;
        private bool powerMeterVisualsConfigured;
        private bool showcaseCameraActive;

        public event Action<KickLuckyCubeKickResult> Landed;

        public bool IsKicking { get; private set; }
        public bool IsSelectingKickPower => isSelectingKickPower;
        public int SelectedKickStyleIndex => ResolveKickStyleShop() != null
            ? ResolveKickStyleShop().SelectedStyle
            : KickLuckyCubeKickStyleCatalog.LoadSelectedIndex();
        public string SelectedKickStyleName => KickLuckyCubeKickStyleCatalog.Get(SelectedKickStyleIndex).DisplayName;
        public float SelectedKickStyleMultiplier => KickLuckyCubeKickStyleCatalog.Get(SelectedKickStyleIndex).StrengthMultiplier;
        public bool IsCubeInFlight => IsKicking;
        public Transform CubeTransform => cube;
        public float LastDistance { get; private set; }
        public KickLuckyCubeRarity LastLandedRarity { get; private set; }
        public string LastAnimalPool { get; private set; } = string.Empty;
        public int LastPerformedKickStyleIndex { get; private set; } = -1;
        public float LastKickStylePeakRotationDegrees { get; private set; }
        public bool LastKickStyleMotionCompleted { get; private set; }
        public bool IsPreviewingStyle { get; private set; }
        public int PreviewedStyleIndex { get; private set; } = -1;
        public bool CanKick => !kickLocked && !IsKicking && !IsPreviewingStyle && stats != null && cube != null && zones.Length > 0;

        private void Awake()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            inventory ??= FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
            kickStrengthSettings ??= FindFirstObjectByType<KickLuckyCubeKickStrengthSettingsController>(FindObjectsInactive.Include);
            kickStyleShop ??= FindFirstObjectByType<KickLuckyCubeStyleShopController>(FindObjectsInactive.Include);
            showcaseCamera ??= Camera.main != null
                ? Camera.main.GetComponent<KickLuckyCubeThirdPersonCamera>()
                : FindFirstObjectByType<KickLuckyCubeThirdPersonCamera>(FindObjectsInactive.Include);

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
                    .FirstOrDefault(found => found.name == "Floor_FlatGreenGrass");
            }

            RefreshZonesFromScene();

            CacheCubeOriginalTransform();
            ConfigureCubeTrail();
            ConfigurePowerMeterVisuals();
            ApplyUiTheme();
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

            if (HasPowerSelectionActorMoved())
            {
                CancelKickPowerSelection("Kick cancelled.\nStand still while choosing power.");
                return;
            }

            if ((WasPrimaryPointerPressedOutsideUi() || WasKeyboardKickConfirmPressed()) && Time.frameCount > powerSelectionStartedFrame)
            {
                ConfirmKickPower();
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
            RefreshStatus("Click anywhere or press E to kick.\nTop of the meter = stronger hit.");
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
            CancelStylePreview();
            EndKickShowcase();
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
            CancelKickPowerSelection(null);
        }

        private void CancelKickPowerSelection(string statusOverride)
        {
            if (!isSelectingKickPower)
            {
                return;
            }

            isSelectingKickPower = false;
            powerSelectionActor = null;
            HidePowerMeter();
            RefreshStatus(statusOverride);

            if (Application.isPlaying && hideCubeUntilKickInPlayMode)
            {
                SetCubeVisible(false);
            }
        }

        private void BeginKickPowerSelection(GameObject actor)
        {
            inventory?.HideSelectedHandPreviewForAction();
            ShowCubeInHands(actor);
            ResolveKickOrigin(actor, out pendingKickOriginPosition, out pendingKickOriginRotation);
            currentPower = initialPower;
            powerDirection = 1f;
            isSelectingKickPower = true;
            powerSelectionStartedFrame = Time.frameCount;
            powerSelectionActor = actor;
            powerSelectionActorStartPosition = actor != null ? actor.transform.position : pendingKickOriginPosition;

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
            RefreshStatus("Choose kick power.\nClick anywhere or press E to kick.");
        }

        private void ConfirmKickPower()
        {
            if (!CanKick)
            {
                CancelKickPowerSelection();
                return;
            }

            var kickActor = powerSelectionActor;
            var kickStyleIndex = SelectedKickStyleIndex;
            isSelectingKickPower = false;
            powerSelectionActor = null;
            HidePowerMeter();

            lastKickOriginPosition = pendingKickOriginPosition;
            lastKickOriginRotation = pendingKickOriginRotation;
            SetCubeTrailEmitting(false);
            SetCubeVisible(true);

            var styledStrength = ApplySelectedKickStyleStrength(ResolveEffectiveStrength());
            var distance = CalculatePoweredDistance(styledStrength, currentPower);
            flightRoutine = StartCoroutine(PlayStyledKickAndFlight(kickStyleIndex, distance, kickActor));
        }

        public float ApplySelectedKickStyleStrength(float baseStrength)
        {
            return KickLuckyCubeKickStyleCatalog.ApplyStrengthMultiplier(SelectedKickStyleIndex, baseStrength);
        }

        private IEnumerator PlayStyledKickAndFlight(int styleIndex, float distance, GameObject actor)
        {
            IsKicking = true;
            LastPerformedKickStyleIndex = styleIndex;
            LastKickStylePeakRotationDegrees = 0f;
            LastKickStyleMotionCompleted = false;
            var style = KickLuckyCubeKickStyleCatalog.Get(styleIndex);
            RefreshStatus(style.DisplayName + "...");
            BeginKickShowcase(actor, style);
            if (style.ShowcaseLeadIn > 0f)
            {
                yield return new WaitForSecondsRealtime(style.ShowcaseLeadIn);
            }

            PlayKickAnimation(actor);
            yield return PlayStyleMotion(styleIndex, style, actor, true);
            LastKickStyleMotionCompleted = true;

            if (kickShowcaseContactHoldSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(kickShowcaseContactHoldSeconds);
            }

            EndKickShowcase();
            ResolveKickOrigin(actor, out lastKickOriginPosition, out lastKickOriginRotation);
            DetachCubeFromHandsForFlight();
            SetCubeVisible(true);
            cube.SetPositionAndRotation(lastKickOriginPosition, lastKickOriginRotation);
            SetCubeTrailEmitting(true);
            yield return PlayFlight(distance, lastKickOriginPosition);
        }

        private IEnumerator PlayStyleMotion(
            int styleIndex,
            KickLuckyCubeKickStyleDefinition style,
            GameObject actor,
            bool updateMetrics)
        {
            var visual = ResolveKickVisual(actor);
            if (visual != null)
            {
                var originalLocalPosition = visual.localPosition;
                var originalLocalRotation = visual.localRotation;
                var elapsed = 0f;
                try
                {
                    while (elapsed < style.MotionDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        var normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, style.MotionDuration))
                            * KickLuckyCubeKickStyleCatalog.ImpactNormalizedTime;
                        var pose = KickLuckyCubeKickStyleCatalog.EvaluatePose(styleIndex, normalized);
                        visual.localPosition = originalLocalPosition + pose.LocalPosition;
                        visual.localRotation = originalLocalRotation * Quaternion.Euler(pose.LocalEuler);
                        if (updateMetrics)
                        {
                            LastKickStylePeakRotationDegrees = Mathf.Max(
                                LastKickStylePeakRotationDegrees,
                                Quaternion.Angle(originalLocalRotation, visual.localRotation));
                        }
                        yield return null;
                    }
                }
                finally
                {
                    if (visual != null)
                    {
                        visual.localPosition = originalLocalPosition;
                        visual.localRotation = originalLocalRotation;
                    }
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(Mathf.Min(0.12f, style.MotionDuration));
            }
        }

        public bool TryPreviewKickStyle(int styleIndex, GameObject actor = null)
        {
            if (IsKicking || isSelectingKickPower || kickLocked)
            {
                return false;
            }

            actor ??= GameObject.Find("KLC_PrototypePlayer");
            if (actor == null)
            {
                return false;
            }

            CancelStylePreview();
            stylePreviewRoutine = StartCoroutine(PreviewKickStyleRoutine(
                Mathf.Clamp(styleIndex, 0, KickLuckyCubeKickStyleCatalog.Count - 1),
                actor));
            return true;
        }

        private IEnumerator PreviewKickStyleRoutine(int styleIndex, GameObject actor)
        {
            IsPreviewingStyle = true;
            PreviewedStyleIndex = styleIndex;
            var style = KickLuckyCubeKickStyleCatalog.Get(styleIndex);
            BeginKickShowcase(actor, style);
            if (style.ShowcaseLeadIn > 0f)
            {
                yield return new WaitForSecondsRealtime(style.ShowcaseLeadIn);
            }

            PlayKickAnimation(actor);
            yield return PlayStyleMotion(styleIndex, style, actor, false);
            if (kickShowcaseContactHoldSeconds > 0f)
            {
                yield return new WaitForSecondsRealtime(kickShowcaseContactHoldSeconds);
            }

            EndKickShowcase();
            IsPreviewingStyle = false;
            PreviewedStyleIndex = -1;
            stylePreviewRoutine = null;
        }

        private void BeginKickShowcase(GameObject actor, KickLuckyCubeKickStyleDefinition style)
        {
            if (actor == null)
            {
                return;
            }

            showcaseCamera ??= Camera.main != null
                ? Camera.main.GetComponent<KickLuckyCubeThirdPersonCamera>()
                : FindFirstObjectByType<KickLuckyCubeThirdPersonCamera>(FindObjectsInactive.Include);
            if (showcaseCamera == null)
            {
                return;
            }

            showcaseCamera.BeginCinematicFocus(
                actor.transform,
                style.ShowcaseDistance,
                style.ShowcasePitch,
                actor.transform.eulerAngles.y + style.ShowcaseYawOffset,
                kickShowcaseFocusOffset,
                style.ShowcaseFieldOfView,
                false,
                kickShowcaseTransitionSeconds,
                kickShowcaseEasing);
            showcaseCameraActive = true;
        }

        private void EndKickShowcase()
        {
            if (!showcaseCameraActive)
            {
                return;
            }

            showcaseCamera?.EndCinematicFocus(false);
            showcaseCameraActive = false;
        }

        private void CancelStylePreview()
        {
            if (stylePreviewRoutine != null)
            {
                StopCoroutine(stylePreviewRoutine);
                stylePreviewRoutine = null;
            }

            IsPreviewingStyle = false;
            PreviewedStyleIndex = -1;
            EndKickShowcase();
        }

        private static Transform ResolveKickVisual(GameObject actor)
        {
            if (actor == null)
            {
                return null;
            }

            var actorTransform = actor.transform;
            var visual = actorTransform.Find("KLC_PlayerMannequin")
                ?? actorTransform.Find("KLC_StealBrainrotPlayerVisual")
                ?? actorTransform.Find("KLC_PlayerVisual")
                ?? actorTransform.Find("AvatarRoot");
            if (visual != null)
            {
                return visual;
            }

            var animator = actor.GetComponentInChildren<Animator>(true);
            if (animator == null || animator.transform == actorTransform)
            {
                return null;
            }

            visual = animator.transform;
            while (visual.parent != null && visual.parent != actorTransform)
            {
                visual = visual.parent;
            }

            return visual.parent == actorTransform ? visual : null;
        }

        private static void PlayKickAnimation(GameObject actor)
        {
            if (actor == null)
            {
                return;
            }

            var mannequin = actor.transform.Find("KLC_PlayerMannequin");
            var targetAnimator = mannequin != null
                ? mannequin.GetComponentInChildren<Animator>(true)
                : actor.GetComponentInChildren<Animator>(true);
            if (targetAnimator == null || targetAnimator.runtimeAnimatorController == null)
            {
                return;
            }

            targetAnimator.CrossFadeInFixedTime("Kick", 0.05f, 0, 0f);
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

            CancelStylePreview();
            EndKickShowcase();

            isSelectingKickPower = false;
            HidePowerMeter();
            IsKicking = false;
            LastLandedRarity = KickLuckyCubeRarity.None;
            LastAnimalPool = string.Empty;
            LastDistance = 0f;
            cubePreviewActor = null;
            powerSelectionActor = null;
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
            var balance = ResolveBalanceConfig();
            if (balance != null)
            {
                return balance.CalculateKickDistance(strength);
            }

            var rawDistance = baseKickDistance + Mathf.Max(0f, strength) * distancePerStrength;
            return Mathf.Clamp(rawDistance, minimumDistance, maximumDistance);
        }

        public float CalculatePoweredDistance(float strength, float normalizedPower)
        {
            var baseDistance = CalculateDistance(strength);
            var balance = ResolveBalanceConfig();
            if (balance != null)
            {
                return balance.ApplyKickPower(baseDistance, normalizedPower);
            }

            var powerMultiplier = Mathf.Lerp(
                minimumPowerMultiplier,
                Mathf.Max(minimumPowerMultiplier, maximumPowerMultiplier),
                Mathf.Clamp01(normalizedPower));
            return Mathf.Clamp(baseDistance * powerMultiplier, minimumDistance, maximumDistance);
        }

        private KickLuckyCubeBalanceConfig ResolveBalanceConfig()
        {
            balanceConfig ??= KickLuckyCubeBalanceConfig.GetOrLoadDefault();
            return balanceConfig;
        }

        private KickLuckyCubeStyleShopController ResolveKickStyleShop()
        {
            kickStyleShop ??= FindFirstObjectByType<KickLuckyCubeStyleShopController>(FindObjectsInactive.Include);
            return kickStyleShop;
        }

        public KickLuckyCubeRarityZone ResolveLandingZone(float distance)
        {
            if (zones == null || zones.Length < KickLuckyCubeCorridorLayout.LocationCount)
            {
                RefreshZonesFromScene();
            }

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

            CancelStylePreview();
            EndKickShowcase();

            IsKicking = false;
            isSelectingKickPower = false;
            powerSelectionActor = null;
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

            var endXZ = start + Vector3.forward * distance;
            var endSurfaceY = ResolveLandingSurfaceY(endXZ);
            var end = new Vector3(endXZ.x, endSurfaceY + ResolveCubeHalfHeight(), endXZ.z);
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
                landingMarker.gameObject.SetActive(false);
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
                ? CalculatePoweredDistance(ApplySelectedKickStyleStrength(ResolveEffectiveStrength()), 1f).ToString("0.0", CultureInfo.InvariantCulture)
                : "n/a";

            var resultLine = string.IsNullOrEmpty(overrideLine)
                ? kickLocked
                    ? "Animal run in progress."
                    : isSelectingKickPower
                    ? "Click anywhere or press E to kick."
                    : LastLandedRarity == KickLuckyCubeRarity.None
                    ? "Press E to aim kick."
                    : $"Landed: {LastLandedRarity} | {LastAnimalPool}"
                : overrideLine;

            var text = $"Strength: {strengthText} | Tool {toolText}\nSpeed: {speedText} | Kick: {predictedDistance} m\nStyle: {SelectedKickStyleName} x{SelectedKickStyleMultiplier:0.00}\n{resultLine}";

            if (hudText != null)
            {
                hudText.gameObject.SetActive(showHudText);
                if (showHudText)
                {
                    hudText.text = text;
                }
            }

            if (worldStatusText != null)
            {
                worldStatusText.text = text;
            }
        }

        private void RefreshPowerMeter()
        {
            ConfigurePowerMeterVisuals();

            if (powerMeterGroup != null)
            {
                powerMeterGroup.alpha = isSelectingKickPower ? 1f : 0f;
                powerMeterGroup.interactable = false;
                powerMeterGroup.blocksRaycasts = false;
            }

            if (powerMeterFill != null)
            {
                powerMeterFill.type = Image.Type.Filled;
                powerMeterFill.fillMethod = Image.FillMethod.Vertical;
                powerMeterFill.fillOrigin = (int)Image.OriginVertical.Bottom;
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

        private void ConfigurePowerMeterVisuals()
        {
            if (powerMeterVisualsConfigured || powerMeterFill == null)
            {
                return;
            }

            var fillRect = powerMeterFill.rectTransform;
            var barParent = fillRect.parent as RectTransform;
            if (barParent == null)
            {
                powerMeterVisualsConfigured = true;
                return;
            }

            EnsurePowerBand(barParent, "KLC_PowerBand_Red", 0f, 0.34f, new Color(1f, 0.12f, 0.08f, 0.72f));
            EnsurePowerBand(barParent, "KLC_PowerBand_Yellow", 0.34f, 0.68f, new Color(1f, 0.78f, 0.08f, 0.72f));
            EnsurePowerBand(barParent, "KLC_PowerBand_Green", 0.68f, 1f, new Color(0.15f, 1f, 0.12f, 0.72f));

            powerMeterFill.type = Image.Type.Filled;
            powerMeterFill.fillMethod = Image.FillMethod.Vertical;
            powerMeterFill.fillOrigin = (int)Image.OriginVertical.Bottom;
            powerMeterFill.raycastTarget = false;
            powerMeterFill.transform.SetAsLastSibling();

            if (powerMeterMarker != null)
            {
                powerMeterMarker.SetAsLastSibling();
            }

            if (powerMeterText != null)
            {
                powerMeterText.transform.SetAsLastSibling();
            }

            ApplyUiTheme();
            powerMeterVisualsConfigured = true;
        }

        private void ApplyUiTheme()
        {
            KickLuckyCubeUiTheme.StyleHudText(hudText);

            KickLuckyCubeUiTheme.StyleWorldText(worldStatusText, Color.white, 0.009f);
        }

        private static void EnsurePowerBand(RectTransform parent, string bandName, float minY, float maxY, Color color)
        {
            if (parent != null && parent.Find(bandName) != null)
            {
                return;
            }

            CreatePowerBand(parent, bandName, minY, maxY, color);
        }

        private static void CreatePowerBand(RectTransform parent, string bandName, float minY, float maxY, Color color)
        {
            var bandTransform = KickLuckyCubeUiPrefabFactory.CreateRect(bandName, parent);
            var bandImage = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<Image>(bandTransform.gameObject);

            bandTransform.anchorMin = new Vector2(0f, minY);
            bandTransform.anchorMax = new Vector2(1f, maxY);
            bandTransform.offsetMin = Vector2.zero;
            bandTransform.offsetMax = Vector2.zero;
            bandTransform.SetAsFirstSibling();

            bandImage.color = color;
            bandImage.raycastTarget = false;
        }

        private static bool WasPrimaryPointerPressedOutsideUi()
        {
            if (IsPointerOverUi())
            {
                return false;
            }

#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private static bool WasKeyboardKickConfirmPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }

        private static bool IsPointerOverUi()
        {
            var eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject();
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

        private void RefreshZonesFromScene()
        {
            var sceneZones = FindObjectsByType<KickLuckyCubeRarityZone>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (sceneZones == null || sceneZones.Length == 0)
            {
                SortZones();
                return;
            }

            zones = (zones ?? Array.Empty<KickLuckyCubeRarityZone>())
                .Concat(sceneZones)
                .Where(zone => zone != null)
                .GroupBy(zone => zone.ZoneIndex)
                .Select(group => group
                    .OrderByDescending(zone => zone.gameObject.activeInHierarchy)
                    .First())
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
            if (KickLuckyCubeGroundResolver.TryResolveGroundY(
                    landingPosition,
                    cube,
                    120f,
                    240f,
                    2f,
                    out var physicsY))
            {
                return physicsY;
            }

            if (landingSurface != null)
            {
                var surfaceRenderer = landingSurface.GetComponentInChildren<Renderer>();
                if (surfaceRenderer != null)
                {
                    return surfaceRenderer.bounds.max.y;
                }

                return landingSurface.position.y;
            }

            return lastKickOriginPosition.y;
        }

        private bool HasPowerSelectionActorMoved()
        {
            if (powerSelectionActor == null || powerSelectionMoveCancelDistance <= 0f)
            {
                return false;
            }

            var delta = powerSelectionActor.transform.position - powerSelectionActorStartPosition;
            delta.y = 0f;
            return delta.sqrMagnitude >= powerSelectionMoveCancelDistance * powerSelectionMoveCancelDistance;
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

        private float ResolveEffectiveStrength()
        {
            kickStrengthSettings ??= FindFirstObjectByType<KickLuckyCubeKickStrengthSettingsController>(FindObjectsInactive.Include);
            kickStyleShop ??= FindFirstObjectByType<KickLuckyCubeStyleShopController>(FindObjectsInactive.Include);
            return kickStrengthSettings != null
                ? kickStrengthSettings.EffectiveStrength
                : stats != null
                    ? stats.Strength
                    : 0f;
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
