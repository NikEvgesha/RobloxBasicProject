using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class KickLuckyCubeClimbableLadder : MonoBehaviour
    {
        [SerializeField] private Transform bottomExit;
        [SerializeField] private Transform topExit;
        [SerializeField, Min(0.1f)] private float climbSpeed = 4f;
        [SerializeField, Min(0.1f)] private float snapSpeed = 10f;
        [SerializeField, Min(0f)] private float exitThreshold = 0.08f;

        private KickLuckyCubePlayerController activePlayer;

        public Transform BottomExit => bottomExit;
        public Transform TopExit => topExit;

        public void Configure(Transform bottom, Transform top, float speed = 4f)
        {
            bottomExit = bottom;
            topExit = top;
            climbSpeed = Mathf.Max(0.1f, speed);

            var trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
        }

        private void OnTriggerStay(Collider other)
        {
            var player = other.GetComponentInParent<KickLuckyCubePlayerController>();
            if (player == null || bottomExit == null || topExit == null)
            {
                return;
            }

            var input = ReadVerticalInput();
            if (Mathf.Abs(input) < 0.05f)
            {
                return;
            }

            activePlayer = player;
            player.SetExternalMovementLock(true);

            var start = bottomExit.position;
            var end = topExit.position;
            var axis = end - start;
            var axisLength = Mathf.Max(0.001f, axis.magnitude);
            var direction = axis / axisLength;
            var currentDistance = Mathf.Clamp(Vector3.Dot(player.transform.position - start, direction), 0f, axisLength);
            var nextDistance = Mathf.Clamp(currentDistance + input * climbSpeed * Time.deltaTime, 0f, axisLength);
            var centerLine = start + direction * nextDistance;
            player.transform.position = Vector3.MoveTowards(
                player.transform.position,
                centerLine,
                snapSpeed * Time.deltaTime);

            if (input > 0f && nextDistance >= axisLength - exitThreshold)
            {
                ReleaseAt(player, topExit);
            }
            else if (input < 0f && nextDistance <= exitThreshold)
            {
                ReleaseAt(player, bottomExit);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponentInParent<KickLuckyCubePlayerController>();
            if (player != null && player == activePlayer)
            {
                player.SetExternalMovementLock(false);
                activePlayer = null;
            }
        }

        private void OnDisable()
        {
            if (activePlayer != null)
            {
                activePlayer.SetExternalMovementLock(false);
                activePlayer = null;
            }
        }

        private void ReleaseAt(KickLuckyCubePlayerController player, Transform exit)
        {
            player.transform.SetPositionAndRotation(exit.position, player.transform.rotation);
            player.SetExternalMovementLock(false);
            activePlayer = null;
        }

        private static float ReadVerticalInput()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return 0f;
            }

            var input = 0f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                input += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                input -= 1f;
            }

            return input;
#else
            return Input.GetAxisRaw("Vertical");
#endif
        }
    }
}
