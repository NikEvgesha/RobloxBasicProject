using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeMobileActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private enum ActionKind
        {
            InteractHold,
            Jump
        }

        [SerializeField] private KickLuckyCubeMobileInput input;
        [SerializeField] private ActionKind action;

        private void Awake()
        {
            input ??= FindFirstObjectByType<KickLuckyCubeMobileInput>(FindObjectsInactive.Include);
            ApplyTheme();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (input == null)
            {
                return;
            }

            if (action == ActionKind.Jump)
            {
                input.QueueJump();
                return;
            }

            input.SetInteractHeld(true);
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
            if (action == ActionKind.InteractHold)
            {
                input?.SetInteractHeld(false);
            }
        }

        private void ApplyTheme()
        {
            KickLuckyCubeUiTheme.StyleTree(gameObject);

            var button = GetComponent<Button>();
            if (button != null)
            {
                KickLuckyCubeUiTheme.StyleButton(button, action == ActionKind.InteractHold ? "ActionButton" : "SideButton");
            }
        }
    }
}
