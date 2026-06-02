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
        [SerializeField] private float followSharpness = 18f;
        [SerializeField] private float zoomSharpness = 16f;
        [SerializeField] private bool snapOnStart = true;
        [SerializeField] private float yaw;
        [SerializeField] private float pitch = 18f;

        private float targetYaw;
        private float targetPitch;
        private float targetDistance;
        private Transform activeTarget;

        public Transform ActiveTarget => activeTarget;

        private void Awake()
        {
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

            if (snapOnStart)
            {
                UpdateCamera(1f, true);
            }
        }

        private void LateUpdate()
        {
            var deltaTime = Time.unscaledDeltaTime;
            var isOrbiting = ReadOrbitDelta(out var orbitDelta);
            if (isOrbiting)
            {
                targetYaw += orbitDelta.x * sensitivity;
                targetPitch = Mathf.Clamp(targetPitch - orbitDelta.y * sensitivity, minPitch, maxPitch);
            }

            var zoomInput = ReadZoomInput();
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                targetDistance = Mathf.Clamp(targetDistance - zoomInput * zoomStep, minDistance, maxDistance);
            }

            var rotationBlend = isOrbiting ? 1f : 1f - Mathf.Exp(-rotationSharpness * deltaTime);
            var zoomBlend = 1f - Mathf.Exp(-zoomSharpness * deltaTime);
            yaw = isOrbiting ? targetYaw : Mathf.LerpAngle(yaw, targetYaw, rotationBlend);
            pitch = isOrbiting ? targetPitch : Mathf.Lerp(pitch, targetPitch, rotationBlend);
            distance = Mathf.Lerp(distance, targetDistance, zoomBlend);

            UpdateCamera(isOrbiting ? 1f : 1f - Mathf.Exp(-followSharpness * deltaTime), isOrbiting);
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

        private void UpdateCamera(float followBlend, bool force)
        {
            activeTarget = ResolveTarget();
            if (activeTarget == null)
            {
                return;
            }

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var focusPoint = activeTarget.position + targetOffset;
            var desiredPosition = focusPoint + rotation * new Vector3(0f, 0f, -distance);

            transform.SetPositionAndRotation(
                force ? desiredPosition : Vector3.Lerp(transform.position, desiredPosition, followBlend),
                rotation);
        }

        private Transform ResolveTarget()
        {
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
