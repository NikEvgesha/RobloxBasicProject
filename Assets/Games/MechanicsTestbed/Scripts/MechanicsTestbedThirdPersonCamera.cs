using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.45f, 0f);
        [SerializeField] private float distance = 6f;
        [SerializeField] private float minDistance = 3.5f;
        [SerializeField] private float maxDistance = 11f;
        [SerializeField] private float zoomStep = 1.15f;
        [SerializeField] private float minPitch = -12f;
        [SerializeField] private float maxPitch = 62f;
        [SerializeField] private float sensitivity = 0.18f;
        [SerializeField] private float mobileSensitivityMultiplier = 0.65f;
        [SerializeField] private float rotationSharpness = 22f;
        [SerializeField] private float followSharpness = 18f;
        [SerializeField] private float zoomSharpness = 16f;
        [SerializeField] private float yaw;
        [SerializeField] private float pitch = 18f;
        [SerializeField] private MechanicsTestbedMobileInput mobileInput;

        private float targetYaw;
        private float targetPitch;
        private float targetDistance;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void Awake()
        {
            if (mobileInput == null)
            {
                mobileInput = FindFirstObjectByType<MechanicsTestbedMobileInput>();
            }
        }

        private void Start()
        {
            if (Mathf.Approximately(yaw, 0f))
            {
                yaw = transform.eulerAngles.y;
            }

            distance = Mathf.Clamp(distance, minDistance, maxDistance);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            targetYaw = yaw;
            targetPitch = pitch;
            targetDistance = distance;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            if (ReadOrbitDelta(out var desktopDelta))
            {
                targetYaw += desktopDelta.x * sensitivity;
                targetPitch = Mathf.Clamp(targetPitch - desktopDelta.y * sensitivity, minPitch, maxPitch);
            }

            if (mobileInput != null)
            {
                var mobileDelta = mobileInput.ConsumeCameraDelta();
                if (mobileDelta.sqrMagnitude > 0f)
                {
                    var mobileSensitivity = sensitivity * mobileSensitivityMultiplier;
                    targetYaw += mobileDelta.x * mobileSensitivity;
                    targetPitch = Mathf.Clamp(targetPitch - mobileDelta.y * mobileSensitivity, minPitch, maxPitch);
                }
            }

            var zoomInput = ReadZoomInput();
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                targetDistance = Mathf.Clamp(targetDistance - zoomInput * zoomStep, minDistance, maxDistance);
            }

            var rotationBlend = 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime);
            var zoomBlend = 1f - Mathf.Exp(-zoomSharpness * Time.deltaTime);
            yaw = Mathf.LerpAngle(yaw, targetYaw, rotationBlend);
            pitch = Mathf.Lerp(pitch, targetPitch, rotationBlend);
            distance = Mathf.Lerp(distance, targetDistance, zoomBlend);

            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var focusPoint = target.position + targetOffset;
            var desiredPosition = focusPoint + rotation * new Vector3(0f, 0f, -distance);
            var blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);

            transform.SetPositionAndRotation(Vector3.Lerp(transform.position, desiredPosition, blend), rotation);
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
