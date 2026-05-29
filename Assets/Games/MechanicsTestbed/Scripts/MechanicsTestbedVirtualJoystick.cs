using UnityEngine;
using UnityEngine.EventSystems;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private MechanicsTestbedMobileInput input;
        [SerializeField] private RectTransform handle;
        [SerializeField] private float radius = 54f;

        private RectTransform rectTransform;
        private int activePointerId = int.MinValue;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;

            if (input == null)
            {
                input = FindFirstObjectByType<MechanicsTestbedMobileInput>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
            UpdateStick(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                UpdateStick(eventData);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            input?.SetMoveInput(Vector2.zero);

            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }

        private void UpdateStick(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint))
            {
                return;
            }

            var clamped = Vector2.ClampMagnitude(localPoint, radius);
            input?.SetMoveInput(clamped / radius);

            if (handle != null)
            {
                handle.anchoredPosition = clamped;
            }
        }
    }
}
