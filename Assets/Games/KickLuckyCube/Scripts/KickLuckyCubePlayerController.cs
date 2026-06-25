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
        [SerializeField] private bool useStealBrainrotPlayerVisual = true;
        [SerializeField] private string importedPlayerVisualResourcePath = "KickLuckyCube/StealBrainrot/Models/Player/SadovnicOBJ";
        [SerializeField] private string importedPlayerVisualName = "KLC_StealBrainrotPlayerVisual";
        [SerializeField] private string generatedPlayerVisualName = "KLC_PlayerVisual";
        [SerializeField, Min(0.1f)] private float importedPlayerVisualTargetHeight = 2.05f;
        [SerializeField] private Vector3 importedPlayerVisualLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 importedPlayerVisualLocalEuler;

        private Rigidbody body;
        private float lockedY;
        private float verticalVelocity;
        private bool isGrounded = true;

        private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int ImportedSpeedHash = Animator.StringToHash("Speed");

        public Vector2 MoveInput { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsGrounded => isGrounded;

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            lockedY = position.y;
            verticalVelocity = -1f;
            isGrounded = true;

            if (body != null)
            {
                body.position = position;
                body.rotation = rotation;
            }
        }

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
            SetupImportedPlayerVisual();
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
            SetFloatIfExists(animator, MoveSpeedHash, moveSpeed, 0.08f, Time.unscaledDeltaTime);
            SetFloatIfExists(animator, ImportedSpeedHash, Mathf.Clamp01(moveSpeed / Mathf.Max(0.01f, sprintSpeed)), 0.08f, Time.unscaledDeltaTime);
            SetBoolIfExists(animator, IsMovingHash, isMoving);
            SetBoolIfExists(animator, IsSprintingHash, isMoving && IsSprinting);
            SetBoolIfExists(animator, GroundedHash, isGrounded);
        }

        private void SetupImportedPlayerVisual()
        {
            if (!useStealBrainrotPlayerVisual || string.IsNullOrWhiteSpace(importedPlayerVisualResourcePath))
            {
                return;
            }

            var existing = transform.Find(importedPlayerVisualName);
            if (existing != null)
            {
                var existingAnimator = existing.GetComponentInChildren<Animator>(true);
                if (existingAnimator != null)
                {
                    animator = existingAnimator;
                }

                return;
            }

            var prefab = Resources.Load<GameObject>(importedPlayerVisualResourcePath);
            if (prefab == null)
            {
                return;
            }

            HideGeneratedPlayerVisual();

            var visual = Instantiate(prefab, transform);
            visual.name = importedPlayerVisualName;
            visual.transform.localPosition = importedPlayerVisualLocalPosition;
            visual.transform.localRotation = Quaternion.Euler(importedPlayerVisualLocalEuler);
            visual.transform.localScale = Vector3.one;
            RemoveColliders(visual);
            NormalizeVisualToHeight(visual.transform, importedPlayerVisualTargetHeight);

            var importedAnimator = visual.GetComponentInChildren<Animator>(true);
            if (importedAnimator != null)
            {
                animator = importedAnimator;
            }
        }

        private void HideGeneratedPlayerVisual()
        {
            var generatedVisual = transform.Find(generatedPlayerVisualName);
            if (generatedVisual == null)
            {
                generatedVisual = transform.Find("AvatarRoot");
            }

            if (generatedVisual == null)
            {
                return;
            }

            foreach (var renderer in generatedVisual.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
            }
        }

        private void NormalizeVisualToHeight(Transform visualRoot, float targetHeight)
        {
            if (visualRoot == null || !TryGetRendererBounds(visualRoot, out var bounds))
            {
                return;
            }

            var currentHeight = Mathf.Max(0.001f, bounds.size.y);
            visualRoot.localScale = Vector3.one * Mathf.Clamp(Mathf.Max(0.1f, targetHeight) / currentHeight, 0.01f, 100f);

            if (!TryGetRendererBounds(visualRoot, out bounds))
            {
                return;
            }

            visualRoot.position += Vector3.up * (transform.position.y - bounds.min.y);
        }

        private static void RemoveColliders(GameObject root)
        {
            foreach (var collider in root.GetComponentsInChildren<Collider>(true))
            {
                Destroy(collider);
            }
        }

        private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
        {
            bounds = new Bounds(root != null ? root.position : Vector3.zero, Vector3.zero);
            if (root == null)
            {
                return false;
            }

            var hasBounds = false;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null || !renderer.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        private static void SetFloatIfExists(Animator targetAnimator, int parameterHash, float value, float dampTime, float deltaTime)
        {
            if (HasAnimatorParameter(targetAnimator, parameterHash))
            {
                targetAnimator.SetFloat(parameterHash, value, dampTime, deltaTime);
            }
        }

        private static void SetBoolIfExists(Animator targetAnimator, int parameterHash, bool value)
        {
            if (HasAnimatorParameter(targetAnimator, parameterHash))
            {
                targetAnimator.SetBool(parameterHash, value);
            }
        }

        private static bool HasAnimatorParameter(Animator targetAnimator, int parameterHash)
        {
            if (targetAnimator == null)
            {
                return false;
            }

            foreach (var parameter in targetAnimator.parameters)
            {
                if (parameter.nameHash == parameterHash)
                {
                    return true;
                }
            }

            return false;
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
