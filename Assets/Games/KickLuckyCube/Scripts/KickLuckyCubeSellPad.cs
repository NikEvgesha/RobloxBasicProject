using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeSellPad : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private KickLuckyCubeWallet wallet;

        private void Awake()
        {
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>();
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>();
        }

        public bool CanInteract(GameObject actor)
        {
            return runPhase != null && wallet != null && runPhase.HasCarriedAnimal;
        }

        public void Sell(GameObject actor)
        {
            runPhase?.TrySellCarriedAnimal(wallet);
        }
    }
}
