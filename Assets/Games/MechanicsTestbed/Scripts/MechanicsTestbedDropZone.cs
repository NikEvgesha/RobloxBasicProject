using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedDropZone : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private MechanicsTestbedCarryController carryController;
        [SerializeField] private Transform dropAnchor;

        private void Awake()
        {
            carryController ??= FindFirstObjectByType<MechanicsTestbedCarryController>();

            if (dropAnchor == null)
            {
                dropAnchor = transform;
            }
        }

        public bool CanInteract(GameObject actor)
        {
            return carryController != null && carryController.HasCarriedObject;
        }

        public void Drop()
        {
            carryController?.TryDrop(dropAnchor);
        }
    }
}
