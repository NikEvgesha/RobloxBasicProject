using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedPickupCube : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private MechanicsTestbedCarryController carryController;

        private void Awake()
        {
            carryController ??= FindFirstObjectByType<MechanicsTestbedCarryController>();
        }

        public bool CanInteract(GameObject actor)
        {
            return carryController != null && !carryController.HasCarriedObject;
        }

        public void Pickup()
        {
            carryController?.TryPickup(gameObject);
        }
    }
}
