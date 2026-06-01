using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeMobileInput : MonoBehaviour
    {
        private Vector2 moveInput;
        private bool interactHeld;
        private bool jumpQueued;

        public Vector2 MoveInput => moveInput;
        public bool InteractHeld => interactHeld;

        public void SetMoveInput(Vector2 value)
        {
            moveInput = Vector2.ClampMagnitude(value, 1f);
        }

        public void SetInteractHeld(bool value)
        {
            interactHeld = value;
        }

        public void QueueJump()
        {
            jumpQueued = true;
        }

        public bool ConsumeJumpPressed()
        {
            if (!jumpQueued)
            {
                return false;
            }

            jumpQueued = false;
            return true;
        }

        public void Clear()
        {
            moveInput = Vector2.zero;
            interactHeld = false;
            jumpQueued = false;
        }
    }
}
