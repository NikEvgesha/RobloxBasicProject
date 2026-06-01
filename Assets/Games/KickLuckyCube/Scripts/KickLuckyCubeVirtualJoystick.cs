using UnityEngine;
using UnityEngine.EventSystems;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private KickLuckyCubeMobileInput input;
        [SerializeField] private RectTransform handle;
        [SerializeField, Min(8f)] private float radius = 58f;

        private RectTransform rectTransform;
        private int activePointerId = int.MinValue;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;
            input ??= FindFirstObjectByType<KickLuckyCubeMobileInput>(FindObjectsInactive.Include);
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
