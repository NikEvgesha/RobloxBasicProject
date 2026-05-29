using UnityEngine;
using UnityEngine.EventSystems;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedMobileCameraArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private MechanicsTestbedMobileInput input;
        [SerializeField] private float dragMultiplier = 1f;

        private int activePointerId = int.MinValue;

        private void Awake()
        {
            if (input == null)
            {
                input = FindFirstObjectByType<MechanicsTestbedMobileInput>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            activePointerId = eventData.pointerId;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                input?.AddCameraDelta(eventData.delta * dragMultiplier);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == activePointerId)
            {
                activePointerId = int.MinValue;
            }
        }
    }
}
