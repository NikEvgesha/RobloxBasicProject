using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStablePlacementPad : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private KickLuckyCubeStableSlot stableSlot;

        private void Awake()
        {
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>();
        }

        public bool CanInteract(GameObject actor)
        {
            return runPhase != null
                && stableSlot != null
                && runPhase.HasCarriedAnimal
                && !stableSlot.IsOccupied;
        }

        public void Place(GameObject actor)
        {
            runPhase?.TryPlaceCarriedAnimal(stableSlot);
        }
    }
}
