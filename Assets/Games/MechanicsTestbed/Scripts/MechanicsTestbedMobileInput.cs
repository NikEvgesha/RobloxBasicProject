using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedMobileInput : MonoBehaviour
    {
        private Vector2 moveInput;
        private Vector2 cameraDelta;
        private bool sprintHeld;
        private bool jumpQueued;

        public Vector2 MoveInput => moveInput;
        public bool SprintHeld => sprintHeld;

        public void SetMoveInput(Vector2 value)
        {
            moveInput = Vector2.ClampMagnitude(value, 1f);
        }

        public void SetSprintHeld(bool value)
        {
            sprintHeld = value;
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

        public void AddCameraDelta(Vector2 delta)
        {
            cameraDelta += delta;
        }

        public Vector2 ConsumeCameraDelta()
        {
            var value = cameraDelta;
            cameraDelta = Vector2.zero;
            return value;
        }
    }
}
