using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform defaultTarget;
        [SerializeField] private KickLuckyCubeKickController kickController;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private KickLuckyCubeAnimalSpawner animalSpawner;
        [SerializeField] private Vector3 targetOffset = new(0f, 1.45f, 0f);
        [SerializeField] private float distance = 7f;
        [SerializeField] private float minDistance = 3.8f;
        [SerializeField] private float maxDistance = 12f;
        [SerializeField] private float zoomStep = 1.15f;
        [SerializeField] private float minPitch = -8f;
        [SerializeField] private float maxPitch = 58f;
        [SerializeField] private float sensitivity = 0.18f;
        [SerializeField] private float rotationSharpness = 22f;
        [SerializeField] private float orbitRotationSharpness = 30f;
        [SerializeField] private float followSharpness = 18f;
        [SerializeField] private float targetSwitchSharpness = 4.5f;
        [SerializeField] private float zoomSharpness = 16f;
        [SerializeField] private bool snapOnStart = true;
        [SerializeField] private float yaw;
        [SerializeField] private float pitch = 18f;
        [SerializeField] private LayerMask cameraCollisionMask = Physics.DefaultRaycastLayers;
        [SerializeField, Min(0.01f)] private float cameraCollisionRadius = 0.24f;
        [SerializeField, Min(0f)] private float cameraCollisionPadding = 0.18f;
        [SerializeField, Min(0.5f)] private float cameraCollisionMinimumDistance = 1.15f;
        [SerializeField, Min(0.1f)] private float collisionRetractSharpness = 45f;
        [SerializeField, Min(0.1f)] private float collisionRestoreSharpness = 8f;

        private Transform cinematicTarget;
        private float targetYaw;
        private float targetPitch;
        private float targetDistance;
        private Transform activeTarget;
        private Vector3 smoothedFocusPoint;
        private bool hasSmoothedFocusPoint;
        private bool hasSavedCinematicSettings;
        private float savedCinematicTargetYaw;
        private float savedCinematicTargetPitch;
        private float savedCinematicTargetDistance;
        private float savedCinematicFieldOfView;
        private Vector3 cinematicFocusOffset;
        private float targetFieldOfView;
        private Camera cameraComponent;
        private float cinematicTransitionDuration;
        private float cinematicTransitionElapsed;
        private float cinematicStartYaw;
        private float cinematicStartPitch;
        private float cinematicStartDistance;
        private float cinematicStartFieldOfView;
        private AnimationCurve cinematicEasing;
        private float smoothedCameraDistance;
        private bool hasSmoothedCameraDistance;

        public Transform ActiveTarget => activeTarget;

        private void Awake()
        {
            cameraComponent = GetComponent<Camera>();
            if (defaultTarget == null)
            {
                var player = GameObject.Find("KLC_PrototypePlayer");
                defaultTarget = player != null ? player.transform : null;
            }

            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            animalSpawner ??= FindFirstObjectByType<KickLuckyCubeAnimalSpawner>(FindObjectsInactive.Include);
            kickController ??= FindFirstObjectByType<KickLuckyCubeKickController>(FindObjectsInactive.Include);
        }

        private void Start()
        {
            var target = ResolveTarget();
            var targetYawSource = target != null ? target.eulerAngles.y : transform.eulerAngles.y;

            yaw = Mathf.Approximately(yaw, 0f) ? targetYawSource : yaw;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            targetYaw = yaw;
            targetPitch = pitch;
            targetDistance = distance;
            targetFieldOfView = cameraComponent != null ? cameraComponent.fieldOfView : 60f;

            if (snapOnStart)
            {
                UpdateCamera(1f, true);
            }
        }

        private void LateUpdate()
        {
            var deltaTime = Time.unscaledDeltaTime;
            var isCinematic = cinematicTarget != null;
            var orbitDelta = Vector2.zero;
            var isOrbiting = !isCinematic && ReadOrbitDelta(out orbitDelta);
            if (isOrbiting)
            {
                targetYaw += orbitDelta.x * sensitivity;
                targetPitch = Mathf.Clamp(targetPitch - orbitDelta.y * sensitivity, minPitch, maxPitch);
            }

            var zoomInput = isCinematic ? 0f : ReadZoomInput();
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                targetDistance = Mathf.Clamp(targetDistance - zoomInput * zoomStep, minDistance, maxDistance);
            }

            var activeRotationSharpness = isOrbiting ? orbitRotationSharpness : rotationSharpness;
            var rotationBlend = 1f - Mathf.Exp(-activeRotationSharpness * deltaTime);
            var zoomBlend = 1f - Mathf.Exp(-zoomSharpness * deltaTime);
            if (isCinematic && cinematicTransitionDuration > 0f
                && cinematicTransitionElapsed < cinematicTransitionDuration)
            {
                cinematicTransitionElapsed = Mathf.Min(
                    cinematicTransitionDuration,
                    cinematicTransitionElapsed + deltaTime);
                var normalized = cinematicTransitionElapsed / cinematicTransitionDuration;
                var blend = cinematicEasing != null ? cinematicEasing.Evaluate(normalized) : normalized;
                yaw = Mathf.LerpAngle(cinematicStartYaw, targetYaw, blend);
                pitch = Mathf.Lerp(cinematicStartPitch, targetPitch, blend);
                distance = Mathf.Lerp(cinematicStartDistance, targetDistance, blend);
                if (cameraComponent != null)
                {
                    cameraComponent.fieldOfView = Mathf.Lerp(cinematicStartFieldOfView, targetFieldOfView, blend);
                }
            }
            else
            {
                yaw = Mathf.LerpAngle(yaw, targetYaw, rotationBlend);
                pitch = Mathf.Lerp(pitch, targetPitch, rotationBlend);
                distance = Mathf.Lerp(distance, targetDistance, zoomBlend);
                if (cameraComponent != null)
                {
                    cameraComponent.fieldOfView = Mathf.Lerp(cameraComponent.fieldOfView, targetFieldOfView, zoomBlend);
                }
            }

            UpdateCamera(deltaTime, false);
        }

        public void SnapToTarget()
        {
            var target = ResolveTarget();
            if (target != null)
            {
                targetYaw = target.eulerAngles.y;
                yaw = targetYaw;
            }

            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            pitch = targetPitch;
            distance = targetDistance;
            UpdateCamera(1f, true);
        }

        public void BeginCinematicFocus(Transform target, float focusDistance, float focusPitch, float worldYaw)
        {
            BeginCinematicFocus(
                target,
                focusDistance,
                focusPitch,
                worldYaw,
                targetOffset,
                cameraComponent != null ? cameraComponent.fieldOfView : 60f,
                false,
                0f,
                null);
        }

        public void BeginCinematicFocus(
            Transform target,
            float focusDistance,
            float focusPitch,
            float worldYaw,
            Vector3 focusOffset,
            float fieldOfView,
            bool snapOnEnter,
            float transitionSeconds,
            AnimationCurve easing)
        {
            if (target == null)
            {
                return;
            }

            if (!hasSavedCinematicSettings)
            {
                savedCinematicTargetYaw = targetYaw;
                savedCinematicTargetPitch = targetPitch;
                savedCinematicTargetDistance = targetDistance;
                savedCinematicFieldOfView = cameraComponent != null ? cameraComponent.fieldOfView : 60f;
                hasSavedCinematicSettings = true;
            }

            cinematicTarget = target;
            cinematicFocusOffset = focusOffset;
            targetDistance = Mathf.Clamp(focusDistance, minDistance, maxDistance);
            targetPitch = Mathf.Clamp(focusPitch, minPitch, maxPitch);
            targetYaw = worldYaw;
            targetFieldOfView = Mathf.Clamp(fieldOfView, 20f, 100f);
            cinematicTransitionDuration = Mathf.Max(0f, transitionSeconds);
            cinematicTransitionElapsed = 0f;
            cinematicStartYaw = yaw;
            cinematicStartPitch = pitch;
            cinematicStartDistance = distance;
            cinematicStartFieldOfView = cameraComponent != null ? cameraComponent.fieldOfView : targetFieldOfView;
            cinematicEasing = easing;

            if (snapOnEnter)
            {
                yaw = targetYaw;
                pitch = targetPitch;
                distance = targetDistance;
                if (cameraComponent != null)
                {
                    cameraComponent.fieldOfView = targetFieldOfView;
                }

                UpdateCamera(1f, true);
            }
        }

        public void EndCinematicFocus(bool alignYawToNextTarget)
        {
            cinematicTarget = null;
            cinematicTransitionDuration = 0f;
            cinematicTransitionElapsed = 0f;

            if (hasSavedCinematicSettings)
            {
                targetYaw = savedCinematicTargetYaw;
                targetPitch = Mathf.Clamp(savedCinematicTargetPitch, minPitch, maxPitch);
                targetDistance = Mathf.Clamp(savedCinematicTargetDistance, minDistance, maxDistance);
                targetFieldOfView = Mathf.Clamp(savedCinematicFieldOfView, 20f, 100f);
                hasSavedCinematicSettings = false;
            }

            if (!alignYawToNextTarget)
            {
                return;
            }

            var target = ResolveTarget();
            if (target != null)
            {
                targetYaw = target.eulerAngles.y;
            }
        }

        private void UpdateCamera(float deltaTime, bool force)
        {
            var resolvedTarget = ResolveTarget();
            if (resolvedTarget == null)
            {
                return;
            }

            var targetChanged = resolvedTarget != activeTarget;
            activeTarget = resolvedTarget;
            if (targetChanged && !force && cinematicTarget == null)
            {
                targetYaw = resolvedTarget.eulerAngles.y;
            }

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var desiredFocusPoint = activeTarget.position
                + (cinematicTarget != null ? cinematicFocusOffset : targetOffset);
            var followBlend = force
                ? 1f
                : 1f - Mathf.Exp(-(targetChanged ? targetSwitchSharpness : followSharpness) * Mathf.Max(0f, deltaTime));

            if (force || !hasSmoothedFocusPoint)
            {
                smoothedFocusPoint = desiredFocusPoint;
                hasSmoothedFocusPoint = true;
            }
            else
            {
                smoothedFocusPoint = Vector3.Lerp(smoothedFocusPoint, desiredFocusPoint, followBlend);
            }

            var desiredPosition = smoothedFocusPoint + rotation * new Vector3(0f, 0f, -distance);
            var resolvedPosition = ResolveCameraCollision(smoothedFocusPoint, desiredPosition);
            var resolvedDistance = Vector3.Distance(smoothedFocusPoint, resolvedPosition);
            if (force || !hasSmoothedCameraDistance)
            {
                smoothedCameraDistance = resolvedDistance;
                hasSmoothedCameraDistance = true;
            }
            else
            {
                var collisionSharpness = resolvedDistance < smoothedCameraDistance
                    ? collisionRetractSharpness
                    : collisionRestoreSharpness;
                var collisionBlend = 1f - Mathf.Exp(-collisionSharpness * Mathf.Max(0f, deltaTime));
                smoothedCameraDistance = Mathf.Lerp(smoothedCameraDistance, resolvedDistance, collisionBlend);
            }

            var cameraPosition = smoothedFocusPoint
                + rotation * new Vector3(0f, 0f, -smoothedCameraDistance);
            transform.SetPositionAndRotation(cameraPosition, rotation);
        }

        private Vector3 ResolveCameraCollision(Vector3 focusPoint, Vector3 desiredPosition)
        {
            var offset = desiredPosition - focusPoint;
            var desiredDistance = offset.magnitude;
            if (desiredDistance <= 0.001f || cameraCollisionRadius <= 0f)
            {
                return desiredPosition;
            }

            var direction = offset / desiredDistance;
            var hits = Physics.SphereCastAll(
                focusPoint,
                cameraCollisionRadius,
                direction,
                desiredDistance,
                cameraCollisionMask,
                QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
            {
                return desiredPosition;
            }

            Array.Sort(hits, static (left, right) => left.distance.CompareTo(right.distance));
            foreach (var hit in hits)
            {
                if (ShouldIgnoreCameraHit(hit))
                {
                    continue;
                }

                var safeDistance = Mathf.Clamp(
                    hit.distance - cameraCollisionPadding,
                    cameraCollisionMinimumDistance,
                    desiredDistance);
                return focusPoint + direction * safeDistance;
            }

            return desiredPosition;
        }

        private bool ShouldIgnoreCameraHit(RaycastHit hit)
        {
            if (hit.collider == null)
            {
                return true;
            }

            var hitTransform = hit.collider.transform;
            if (activeTarget != null
                && (hitTransform == activeTarget
                    || hitTransform.IsChildOf(activeTarget)
                    || activeTarget.IsChildOf(hitTransform)))
            {
                return true;
            }

            return hit.collider.isTrigger
                && hit.collider.GetComponentInParent<KickLuckyCubeWaveChaseController>() == null;
        }

        private Transform ResolveTarget()
        {
            if (cinematicTarget != null)
            {
                return cinematicTarget;
            }

            if (kickController != null && kickController.IsCubeInFlight && kickController.CubeTransform != null)
            {
                return kickController.CubeTransform;
            }

            if (runPhase != null && runPhase.IsSelectingAnimal && runPhase.RoulettePreviewTransform != null)
            {
                return runPhase.RoulettePreviewTransform;
            }

            if (runPhase != null && runPhase.HasActiveRun && animalSpawner != null && animalSpawner.CurrentAnimal != null)
            {
                return animalSpawner.CurrentAnimal.transform;
            }

            return defaultTarget;
        }

        private static bool ReadOrbitDelta(out Vector2 delta)
        {
            delta = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null || !mouse.rightButton.isPressed)
            {
                return false;
            }

            delta = mouse.delta.ReadValue();
            return delta.sqrMagnitude > 0f;
#else
            if (!Input.GetMouseButton(1))
            {
                return false;
            }

            delta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 18f;
            return delta.sqrMagnitude > 0f;
#endif
        }

        private static float ReadZoomInput()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return 0f;
            }

            return Mathf.Clamp(mouse.scroll.ReadValue().y / 120f, -1f, 1f);
#else
            return Input.mouseScrollDelta.y;
#endif
        }
    }
}
