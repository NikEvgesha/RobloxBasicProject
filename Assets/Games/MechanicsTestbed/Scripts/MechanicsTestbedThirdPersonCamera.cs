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
        [SerializeField] private float minPitch = -12f;
        [SerializeField] private float maxPitch = 62f;
        [SerializeField] private float sensitivity = 0.18f;
        [SerializeField] private float followSharpness = 18f;
        [SerializeField] private float yaw;
        [SerializeField] private float pitch = 18f;
        [SerializeField] private MechanicsTestbedMobileInput mobileInput;

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
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var hasDelta = ReadOrbitDelta(out var delta);
            if (mobileInput != null)
            {
                var mobileDelta = mobileInput.ConsumeCameraDelta();
                if (mobileDelta.sqrMagnitude > 0f)
                {
                    delta += mobileDelta;
                    hasDelta = true;
                }
            }

            if (hasDelta)
            {
                yaw += delta.x * sensitivity;
                pitch = Mathf.Clamp(pitch - delta.y * sensitivity, minPitch, maxPitch);
            }

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
    }
}
