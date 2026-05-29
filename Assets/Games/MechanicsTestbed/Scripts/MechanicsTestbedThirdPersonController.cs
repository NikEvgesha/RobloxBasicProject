using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class MechanicsTestbedThirdPersonController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private MechanicsTestbedMobileInput mobileInput;
        [SerializeField] private float walkSpeed = 6f;
        [SerializeField] private float sprintSpeed = 9f;
        [SerializeField] private float acceleration = 24f;
        [SerializeField] private float rotationSharpness = 14f;
        [SerializeField] private float gravity = -30f;
        [SerializeField] private float jumpHeight = 1.35f;

        private CharacterController controller;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;

        public Vector2 MoveInput { get; private set; }
        public bool IsSprinting { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (mobileInput == null)
            {
                mobileInput = FindFirstObjectByType<MechanicsTestbedMobileInput>();
            }
        }

        private void Update()
        {
            MoveInput = GetMoveInput();
            IsSprinting = ReadSprintInput() || (mobileInput != null && mobileInput.SprintHeld);

            var targetSpeed = IsSprinting ? sprintSpeed : walkSpeed;
            var desiredVelocity = GetCameraRelativeMove(MoveInput) * targetSpeed;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, desiredVelocity, acceleration * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            var jumpPressed = ReadJumpPressed() || (mobileInput != null && mobileInput.ConsumeJumpPressed());
            if (controller.isGrounded && jumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;

            var motion = horizontalVelocity;
            motion.y = verticalVelocity;
            controller.Move(motion * Time.deltaTime);

            RotateToward(horizontalVelocity);
        }

        private Vector3 GetCameraRelativeMove(Vector2 input)
        {
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            var forward = transform.forward;
            var right = transform.right;

            if (cameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = transform.forward;
                }

                if (right.sqrMagnitude < 0.0001f)
                {
                    right = transform.right;
                }
            }

            return forward * input.y + right * input.x;
        }

        private void RotateToward(Vector3 velocity)
        {
            var flatVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
            if (flatVelocity.sqrMagnitude < 0.01f)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(flatVelocity.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }

        private Vector2 GetMoveInput()
        {
            var desktopInput = ReadMoveInput();
            if (mobileInput == null || mobileInput.MoveInput.sqrMagnitude <= desktopInput.sqrMagnitude)
            {
                return desktopInput;
            }

            return mobileInput.MoveInput;
        }

        private static Vector2 ReadMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            var input = Vector2.zero;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                input.y += 1f;
            }

            return input;
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }

        private static bool ReadSprintInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
#else
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
        }

        private static bool ReadJumpPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Space);
#endif
        }
    }
}
