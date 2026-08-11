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
        [SerializeField, Min(0.1f)] private float groundProbeHeight = 4f;
        [SerializeField, Min(0.1f)] private float groundProbeDistance = 16f;
        [SerializeField, Min(0f)] private float maximumGroundStepUp = 1.4f;
        [SerializeField, Min(0f)] private float upwardGroundSnapSpeed = 7f;
        [SerializeField, Min(0f)] private float downwardGroundSnapSpeed = 20f;
        [SerializeField, Min(0f)] private float groundedTolerance = 0.06f;
        private const string LegacyPlayerVisualResourcePath = "KickLuckyCube/StealBrainrot/Models/Player/SadovnicOBJ";
        private const string LegacyPlayerVisualName = "KLC_StealBrainrotPlayerVisual";
        private const string BlockbenchPlayerVisualResourcePath = "KickLuckyCube/KLC_PlayerMannequin";
        private const string BlockbenchPlayerVisualName = "KLC_PlayerMannequin";

        [SerializeField] private bool useStealBrainrotPlayerVisual = true;
        [SerializeField] private string importedPlayerVisualResourcePath = BlockbenchPlayerVisualResourcePath;
        [SerializeField] private string importedPlayerVisualName = BlockbenchPlayerVisualName;
        [SerializeField] private string importedAnimatorControllerResourcePath = "KickLuckyCube/KLC_PlayerMannequinAnimator";
        [SerializeField] private string generatedPlayerVisualName = "KLC_PlayerVisual";
        [SerializeField, Min(0.1f)] private float importedPlayerVisualTargetHeight = 2.05f;
        [SerializeField] private Vector3 importedPlayerVisualLocalPosition = Vector3.zero;
        [SerializeField] private Vector3 importedPlayerVisualLocalEuler;
        [SerializeField] private Vector3 blockbenchForwardCorrectionEuler = new(0f, 180f, 0f);

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
            if (TryResolveGroundY(position, out var groundY))
            {
                position.y = groundY;
            }

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
            SnapToGroundImmediate();
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

            var hasGround = TryResolveGroundY(nextPosition, out var groundY);
            if (hasGround
                && transform.position.y <= groundY + groundedTolerance
                && verticalVelocity <= 0f)
            {
                isGrounded = true;
                verticalVelocity = -1f;
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

            if (!hasGround)
            {
                return;
            }

            if (verticalVelocity <= 0f && nextPosition.y <= groundY + groundedTolerance)
            {
                nextPosition.y = groundY;
                verticalVelocity = -1f;
                isGrounded = true;
                lockedY = groundY;
                return;
            }

            if (isGrounded)
            {
                var snapSpeed = groundY > nextPosition.y
                    ? upwardGroundSnapSpeed
                    : downwardGroundSnapSpeed;
                nextPosition.y = Mathf.MoveTowards(nextPosition.y, groundY, snapSpeed * deltaTime);
                lockedY = groundY;
            }
        }

        private void SnapToGroundImmediate()
        {
            var position = transform.position;
            if (TryResolveGroundY(position, out var groundY))
            {
                position.y = groundY;
                transform.position = position;
                if (body != null)
                {
                    body.position = position;
                }
            }

            lockedY = transform.position.y;
            verticalVelocity = -1f;
            isGrounded = true;
        }

        private bool TryResolveGroundY(Vector3 position, out float groundY)
        {
            return KickLuckyCubeGroundResolver.TryResolveGroundY(
                position,
                transform,
                groundProbeHeight,
                groundProbeDistance,
                maximumGroundStepUp,
                out groundY);
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

            MigrateLegacyPlayerVisualSettings();

            var existing = transform.Find(importedPlayerVisualName);
            if (existing != null)
            {
                HideGeneratedPlayerVisual();
                ApplyImportedVisualOrientation(existing);
                ConfigureImportedAnimator(existing.gameObject);
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
            ApplyImportedVisualOrientation(visual.transform);
            visual.transform.localScale = Vector3.one;
            RemoveColliders(visual);
            NormalizeVisualToHeight(visual.transform, importedPlayerVisualTargetHeight);
            ConfigureImportedAnimator(visual);
        }

        private void ApplyImportedVisualOrientation(Transform visual)
        {
            if (visual == null)
            {
                return;
            }

            var correction = string.Equals(
                importedPlayerVisualResourcePath,
                BlockbenchPlayerVisualResourcePath,
                System.StringComparison.Ordinal)
                ? blockbenchForwardCorrectionEuler
                : Vector3.zero;
            visual.localRotation = Quaternion.Euler(importedPlayerVisualLocalEuler + correction);
        }

        private void MigrateLegacyPlayerVisualSettings()
        {
            if (string.Equals(importedPlayerVisualResourcePath, LegacyPlayerVisualResourcePath, System.StringComparison.Ordinal))
            {
                importedPlayerVisualResourcePath = BlockbenchPlayerVisualResourcePath;
            }

            if (string.Equals(importedPlayerVisualName, LegacyPlayerVisualName, System.StringComparison.Ordinal))
            {
                importedPlayerVisualName = BlockbenchPlayerVisualName;
            }
        }

        private void ConfigureImportedAnimator(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            var importedAnimator = visual.GetComponentInChildren<Animator>(true)
                ?? visual.AddComponent<Animator>();

            if (!string.IsNullOrWhiteSpace(importedAnimatorControllerResourcePath))
            {
                var controller = Resources.Load<RuntimeAnimatorController>(importedAnimatorControllerResourcePath);
                if (controller != null)
                {
                    importedAnimator.runtimeAnimatorController = controller;
                }
            }

            importedAnimator.applyRootMotion = false;
            importedAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator = importedAnimator;
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
