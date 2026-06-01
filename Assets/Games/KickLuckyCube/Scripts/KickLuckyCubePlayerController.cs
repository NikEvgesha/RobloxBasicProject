using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class KickLuckyCubePlayerController : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private KickLuckyCubeMobileInput mobileInput;
        [SerializeField] private Animator animator;
        [SerializeField, Min(0f)] private float walkSpeed = 6f;
        [SerializeField, Min(0f)] private float sprintSpeed = 8.5f;
        [SerializeField, Min(0f)] private float rotationSharpness = 14f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.35f;
        [SerializeField] private float gravity = -30f;
        [SerializeField] private bool lockVerticalPosition;

        private Rigidbody body;
        private float lockedY;
        private float verticalVelocity;
        private bool isGrounded = true;

        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");

        public Vector2 MoveInput { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsGrounded => isGrounded;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.freezeRotation = true;

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            mobileInput ??= FindFirstObjectByType<KickLuckyCubeMobileInput>(FindObjectsInactive.Include);
            animator ??= GetComponentInChildren<Animator>(true);
            lockedY = transform.position.y;
        }

        private void Update()
        {
            var deltaTime = Time.unscaledDeltaTime;
            MoveInput = GetMoveInput();
            IsSprinting = ReadSprintInput();

            var movement = GetCameraRelativeMove(MoveInput);
            if (movement.sqrMagnitude > 1f)
            {
                movement.Normalize();
            }

            var speed = IsSprinting ? sprintSpeed : walkSpeed;
            var nextPosition = transform.position + movement * (speed * deltaTime);
            ApplyVerticalMotion(ref nextPosition, deltaTime);

            transform.position = nextPosition;

            if (movement.sqrMagnitude > 0.0001f)
            {
                var targetRotation = Quaternion.LookRotation(movement.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    1f - Mathf.Exp(-rotationSharpness * deltaTime));
            }

            UpdateAnimator(movement.magnitude * speed);
        }

        private void ApplyVerticalMotion(ref Vector3 nextPosition, float deltaTime)
        {
            if (lockVerticalPosition)
            {
                isGrounded = true;
                verticalVelocity = 0f;
                nextPosition.y = lockedY;
                return;
            }

            if (transform.position.y <= lockedY + 0.02f && verticalVelocity <= 0f)
            {
                isGrounded = true;
                verticalVelocity = -1f;
                nextPosition.y = lockedY;
            }
            else
            {
                isGrounded = false;
            }

            if (isGrounded && ReadJumpPressed())
            {
                isGrounded = false;
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * deltaTime;
            nextPosition.y += verticalVelocity * deltaTime;

            if (nextPosition.y <= lockedY)
            {
                nextPosition.y = lockedY;
                verticalVelocity = -1f;
                isGrounded = true;
            }
        }

        private void UpdateAnimator(float moveSpeed)
        {
            if (animator == null)
            {
                return;
            }

            var isMoving = moveSpeed > 0.12f;
            animator.SetFloat(MoveSpeedHash, moveSpeed, 0.08f, Time.unscaledDeltaTime);
            animator.SetBool(IsMovingHash, isMoving);
            animator.SetBool(IsSprintingHash, isMoving && IsSprinting);
            animator.SetBool(GroundedHash, isGrounded);
        }

        private Vector3 GetCameraRelativeMove(Vector2 input)
        {
            if (input.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
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

            return Vector2.ClampMagnitude(input, 1f);
#else
            return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
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

        private bool ReadJumpPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                || (mobileInput != null && mobileInput.ConsumeJumpPressed());
#else
            return Input.GetKeyDown(KeyCode.Space)
                || (mobileInput != null && mobileInput.ConsumeJumpPressed());
#endif
        }
    }
}
