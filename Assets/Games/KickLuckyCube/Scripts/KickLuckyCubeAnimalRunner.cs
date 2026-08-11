using System;
using System.Linq;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeAnimalRunner : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeSpawnedAnimal animal;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private KickLuckyCubeMobileInput mobileInput;
        [SerializeField, Min(0f)] private float speed = 7f;
        [SerializeField, Min(0f)] private float rotationSharpness = 14f;
        [SerializeField] private bool controlEnabled;
        [SerializeField] private float returnLineZ;
        [SerializeField, Min(0f)] private float returnTolerance = 0.8f;
        [SerializeField, Min(0f)] private float groundOffset;
        [SerializeField, Min(0.1f)] private float groundRayHeight = 8f;
        [SerializeField, Min(0.1f)] private float groundRayDistance = 24f;
        [SerializeField, Min(0f)] private float groundSnapSharpness = 22f;
        [SerializeField, Min(0f)] private float upwardGroundSnapSpeed = 4.5f;
        [SerializeField, Min(0f)] private float downwardGroundSnapSpeed = 14f;
        [SerializeField, Min(0f)] private float rendererGroundBoundsPadding = 0.2f;
        [SerializeField, Min(0f)] private float jumpHeight = 1.35f;
        [SerializeField] private float gravity = -30f;
        [SerializeField, Min(0f)] private float sideBoundaryPadding = 0.75f;
        [SerializeField] private Transform fallbackGroundSurface;

        public event Action<KickLuckyCubeAnimalRunner> ReturnedToLine;

        private Bounds[] activeRendererGroundBounds = Array.Empty<Bounds>();
        private float verticalVelocity;
        private bool isGrounded = true;
        private bool hasSideBounds;
        private float sideMinX;
        private float sideMaxX;
        private KickLuckyCubeRiverTraversalZone[] riverTraversalZones = Array.Empty<KickLuckyCubeRiverTraversalZone>();
        private float traversalSpeedMultiplier = 1f;
        private float traversalSinkOffset;

        public KickLuckyCubeSpawnedAnimal Animal => animal;
        public bool ControlEnabled => controlEnabled;
        public float Speed => speed;
        public float TraversalSpeedMultiplier => traversalSpeedMultiplier;
        public float TraversalSinkOffset => traversalSinkOffset;

        private void Awake()
        {
            mobileInput ??= FindFirstObjectByType<KickLuckyCubeMobileInput>(FindObjectsInactive.Include);
            ResolveCameraTransform();

            if (fallbackGroundSurface == null)
            {
                fallbackGroundSurface = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(found => found.name == "Floor_FlatGreenGrass");
            }

            CacheActiveRendererGroundSurfaces();
            CacheSideBounds();
            CacheRiverTraversalZones();
        }

        public void Configure(KickLuckyCubeSpawnedAnimal spawnedAnimal, float runnerSpeed)
        {
            animal = spawnedAnimal;
            speed = Mathf.Max(0f, runnerSpeed);
        }

        public void BeginRun(float targetReturnLineZ)
        {
            returnLineZ = targetReturnLineZ;
            SnapToGroundImmediate();
            controlEnabled = true;
            verticalVelocity = -1f;
            isGrounded = true;
            ResolveCameraTransform();
            CacheRiverTraversalZones();
        }

        public bool SnapToGroundImmediate()
        {
            var position = transform.position;
            if (!TryResolveGroundY(position, out var groundY))
            {
                return false;
            }

            position.y = groundY + groundOffset;
            transform.position = position;
            verticalVelocity = -1f;
            isGrounded = true;
            return true;
        }

        public void StopRun()
        {
            controlEnabled = false;
            ResetTraversalModifier();
        }

        public void ForceReturnForPrototype()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, returnLineZ);
            ReturnedToLine?.Invoke(this);
        }

        private void Update()
        {
            if (!controlEnabled)
            {
                return;
            }

            var deltaTime = Time.unscaledDeltaTime;
            ResolveCameraTransform();
            UpdateTraversalModifier();

            var input = ReadMoveInput();
            if (mobileInput != null && mobileInput.MoveInput.sqrMagnitude > input.sqrMagnitude)
            {
                input = Vector2.ClampMagnitude(mobileInput.MoveInput, 1f);
            }

            var movement = GetCameraRelativeMove(input);
            if (movement.sqrMagnitude > 1f)
            {
                movement.Normalize();
            }

            var nextPosition = transform.position + movement * (speed * traversalSpeedMultiplier * deltaTime);
            ClampToSideBounds(ref nextPosition);
            ApplyVerticalMotion(ref nextPosition, deltaTime);
            ClampToSideBounds(ref nextPosition);
            transform.position = nextPosition;

            if (movement.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(movement, Vector3.up),
                    1f - Mathf.Exp(-rotationSharpness * deltaTime));
            }

            if (transform.position.z <= returnLineZ + returnTolerance)
            {
                controlEnabled = false;
                ResetTraversalModifier();
                ReturnedToLine?.Invoke(this);
            }
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

        private void ResolveCameraTransform()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
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

        private void ApplyVerticalMotion(ref Vector3 position, float deltaTime)
        {
            if (!TryResolveGroundY(position, out var groundY))
            {
                isGrounded = false;
                verticalVelocity += gravity * deltaTime;
                position.y += verticalVelocity * deltaTime;
                return;
            }

            var targetY = groundY + groundOffset + traversalSinkOffset;
            if (isGrounded && ReadJumpPressed())
            {
                isGrounded = false;
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            if (!isGrounded)
            {
                verticalVelocity += gravity * deltaTime;
                position.y += verticalVelocity * deltaTime;

                if (verticalVelocity <= 0f && position.y <= targetY + 0.03f)
                {
                    position.y = targetY;
                    verticalVelocity = -1f;
                    isGrounded = true;
                }

                return;
            }

            var snapSpeed = targetY > position.y ? upwardGroundSnapSpeed : downwardGroundSnapSpeed;
            if (snapSpeed <= 0f)
            {
                snapSpeed = groundSnapSharpness;
            }

            verticalVelocity = -1f;
            position.y = Mathf.MoveTowards(position.y, targetY, snapSpeed * Mathf.Max(0f, deltaTime));
            isGrounded = Mathf.Abs(position.y - targetY) <= 0.04f;
        }

        private bool TryResolveGroundY(Vector3 position, out float groundY)
        {
            if (KickLuckyCubeGroundResolver.TryResolveGroundY(
                    position,
                    transform,
                    groundRayHeight,
                    groundRayDistance,
                    1.5f,
                    out groundY))
            {
                return true;
            }

            if (TryResolveActiveRendererGroundY(position, out var rendererGroundY))
            {
                groundY = rendererGroundY;
                return true;
            }

            if (fallbackGroundSurface != null)
            {
                var renderer = fallbackGroundSurface.GetComponentInChildren<Renderer>();
                groundY = renderer != null ? renderer.bounds.max.y : fallbackGroundSurface.position.y;
                return true;
            }

            groundY = 0f;
            return false;
        }

        private bool ShouldIgnoreGroundCollider(Collider collider)
        {
            return collider.isTrigger
                || collider.transform.IsChildOf(transform)
                || collider.GetComponentInParent<KickLuckyCubeRarityZone>() != null
                || collider.GetComponentInParent<KickLuckyCubeWaveChaseController>() != null
                || collider.GetComponentInParent<KickLuckyCubePlayerController>() != null
                || IsBoundaryWallCollider(collider)
                || collider.name.StartsWith("KLC_Zone_", StringComparison.Ordinal)
                || IsGuideCollider(collider);
        }

        private static bool IsBoundaryWallCollider(Collider collider)
        {
            var current = collider.transform;
            while (current != null)
            {
                if (current.name == "Wall_Left_Tan" || current.name == "Wall_Right_Tan")
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private static bool IsGuideCollider(Collider collider)
        {
            var current = collider.transform;
            while (current != null)
            {
                if (current.name.StartsWith("GUIDE_", StringComparison.Ordinal))
                {
                    return true;
                }

                current = current.parent;
            }

            return false;
        }

        private void CacheActiveRendererGroundSurfaces()
        {
            activeRendererGroundBounds = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(renderer => renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                .Where(IsRendererGroundSurface)
                .Select(renderer => renderer.bounds)
                .Where(bounds => bounds.size.x > 0.4f && bounds.size.z > 0.4f && bounds.size.y <= 3f)
                .ToArray();
        }

        private void CacheSideBounds()
        {
            var activeRenderers = FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(renderer => renderer != null && renderer.enabled && renderer.gameObject.activeInHierarchy)
                .ToArray();
            var leftWall = activeRenderers.FirstOrDefault(renderer => renderer.name == "Wall_Left_Tan");
            var rightWall = activeRenderers.FirstOrDefault(renderer => renderer.name == "Wall_Right_Tan");
            if (leftWall != null && rightWall != null)
            {
                sideMinX = leftWall.bounds.max.x + sideBoundaryPadding;
                sideMaxX = rightWall.bounds.min.x - sideBoundaryPadding;
                hasSideBounds = sideMinX < sideMaxX;
                return;
            }

            var floor = activeRenderers.FirstOrDefault(renderer => renderer.name == "Floor_FlatGreenGrass");
            if (floor == null)
            {
                hasSideBounds = false;
                return;
            }

            sideMinX = floor.bounds.min.x + sideBoundaryPadding;
            sideMaxX = floor.bounds.max.x - sideBoundaryPadding;
            hasSideBounds = sideMinX < sideMaxX;
        }

        private void CacheRiverTraversalZones()
        {
            riverTraversalZones = FindObjectsByType<KickLuckyCubeRiverTraversalZone>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
        }

        private void UpdateTraversalModifier()
        {
            traversalSpeedMultiplier = 1f;
            traversalSinkOffset = 0f;

            foreach (var zone in riverTraversalZones)
            {
                if (zone == null
                    || !zone.TryGetAnimalModifier(transform.position, out var speedMultiplier, out var sinkOffset))
                {
                    continue;
                }

                traversalSpeedMultiplier = Mathf.Min(traversalSpeedMultiplier, speedMultiplier);
                traversalSinkOffset = Mathf.Min(traversalSinkOffset, sinkOffset);
            }
        }

        private void ResetTraversalModifier()
        {
            traversalSpeedMultiplier = 1f;
            traversalSinkOffset = 0f;
        }

        private void ClampToSideBounds(ref Vector3 position)
        {
            if (!hasSideBounds)
            {
                return;
            }

            position.x = Mathf.Clamp(position.x, sideMinX, sideMaxX);
        }

        private bool TryResolveActiveRendererGroundY(Vector3 position, out float groundY)
        {
            var foundGround = false;
            groundY = 0f;

            foreach (var bounds in activeRendererGroundBounds)
            {
                if (position.x < bounds.min.x - rendererGroundBoundsPadding
                    || position.x > bounds.max.x + rendererGroundBoundsPadding
                    || position.z < bounds.min.z - rendererGroundBoundsPadding
                    || position.z > bounds.max.z + rendererGroundBoundsPadding)
                {
                    continue;
                }

                var candidateY = bounds.max.y;
                if (candidateY <= position.y + 1.5f
                    && (!foundGround || Mathf.Abs(position.y - candidateY) < Mathf.Abs(position.y - groundY)))
                {
                    groundY = candidateY;
                    foundGround = true;
                }
            }

            return foundGround;
        }

        private static bool IsRendererGroundSurface(Renderer renderer)
        {
            var objectName = renderer.name;
            return renderer.GetComponentInParent<KickLuckyCubeGroundSurface>() != null
                || objectName.IndexOf("Ramp", StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Hill", StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Slope", StringComparison.OrdinalIgnoreCase) >= 0
                || (objectName.StartsWith("Bridge_", StringComparison.Ordinal)
                    && (objectName.IndexOf("_MainPlank", StringComparison.Ordinal) >= 0
                        || objectName.IndexOf("_Slat_", StringComparison.Ordinal) >= 0
                        || objectName.EndsWith("_LeftBank", StringComparison.Ordinal)
                        || objectName.EndsWith("_RightBank", StringComparison.Ordinal)));
        }
    }
}
