using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.MechanicsTestbed
{
    public sealed class MechanicsTestbedPickupCube : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private MechanicsTestbedCarryController carryController;
        [SerializeField] private MechanicsTestbedCurrencyVfx currencyVfx;

        private void Awake()
        {
            carryController ??= FindFirstObjectByType<MechanicsTestbedCarryController>();
            currencyVfx ??= FindFirstObjectByType<MechanicsTestbedCurrencyVfx>();
        }

        public bool CanInteract(GameObject actor)
        {
            return carryController != null && !carryController.HasCarriedObject;
        }

        public void Pickup()
        {
            if (carryController == null)
            {
                return;
            }

            var effectOrigin = carryController.transform.position + Vector3.up * 1.25f;
            if (carryController.TryPickup(gameObject))
            {
                currencyVfx?.PlayPickupBurst(effectOrigin);
            }
        }
    }
}
