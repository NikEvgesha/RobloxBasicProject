using UnityEngine;
using UnityEngine.EventSystems;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedMobileActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private enum ActionKind
        {
            Sprint,
            Jump
        }

        [SerializeField] private MechanicsTestbedMobileInput input;
        [SerializeField] private ActionKind action;

        private void Awake()
        {
            if (input == null)
            {
                input = FindFirstObjectByType<MechanicsTestbedMobileInput>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (input == null)
            {
                return;
            }

            if (action == ActionKind.Sprint)
            {
                input.SetSprintHeld(true);
                return;
            }

            input.QueueJump();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Release();
        }

        private void Release()
        {
            if (action == ActionKind.Sprint)
            {
                input?.SetSprintHeld(false);
            }
        }
    }
}
