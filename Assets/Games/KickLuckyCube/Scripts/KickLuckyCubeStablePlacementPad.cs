using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStablePlacementPad : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private KickLuckyCubeInventoryController inventory;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeStableSlot stableSlot;

        private GameKitInteractionTarget interactionTarget;

        private void Awake()
        {
            ResolveReferences();
            interactionTarget = GetComponent<GameKitInteractionTarget>();
            if (interactionTarget != null)
            {
                interactionTarget.ActorInteracted.AddListener(Interact);
            }
        }

        private void OnDestroy()
        {
            if (interactionTarget != null)
            {
                interactionTarget.ActorInteracted.RemoveListener(Interact);
            }
        }

        public bool CanInteract(GameObject actor)
        {
            ResolveReferences();
            if (stableSlot == null)
            {
                return false;
            }

            if (stableSlot.IsOccupied)
            {
                return wallet != null && stableSlot.PendingSoft > 0;
            }

            return (runPhase != null && runPhase.HasCarriedAnimal)
                || (inventory != null && inventory.HasSelectedAnimal);
        }

        public void Place(GameObject actor)
        {
            Interact(actor);
        }

        private void Interact(GameObject actor)
        {
            ResolveReferences();
            if (stableSlot == null)
            {
                return;
            }

            if (stableSlot.IsOccupied)
            {
                var amount = stableSlot.Collect();
                if (amount > 0)
                {
                    wallet?.AddSoft(amount);
                }

                return;
            }

            if (runPhase != null && runPhase.TryPlaceCarriedAnimal(stableSlot))
            {
                return;
            }

            if (inventory == null || !inventory.TryRemoveSelectedAnimal(out var selectedAnimal))
            {
                return;
            }

            if (stableSlot.TryPlace(selectedAnimal))
            {
                return;
            }

            inventory.TryAddAnimal(selectedAnimal, true);
        }

        private void ResolveReferences()
        {
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            inventory ??= FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>(FindObjectsInactive.Include);
            stableSlot ??= GetComponentInParent<KickLuckyCubeStableSlot>();
        }
    }
}
