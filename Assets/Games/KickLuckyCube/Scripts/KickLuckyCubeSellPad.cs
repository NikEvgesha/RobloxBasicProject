using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeSellPad : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeSellShopController sellShop;
        [SerializeField] private GameKitInteractionDriver driver;

        private void Awake()
        {
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>();
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>();
            sellShop ??= FindFirstObjectByType<KickLuckyCubeSellShopController>(FindObjectsInactive.Include);
            driver ??= FindFirstObjectByType<GameKitInteractionDriver>();
        }

        private void OnDisable()
        {
            CloseSellShop();
        }

        private void OnTriggerExit(Collider other)
        {
            driver ??= FindFirstObjectByType<GameKitInteractionDriver>();
            if (driver == null || !driver.IsActorCollider(other))
            {
                return;
            }

            CloseSellShop();
        }

        public bool CanInteract(GameObject actor)
        {
            return wallet != null && (sellShop != null || runPhase != null);
        }

        public void Sell(GameObject actor)
        {
            sellShop ??= FindFirstObjectByType<KickLuckyCubeSellShopController>(FindObjectsInactive.Include);
            if (sellShop != null)
            {
                sellShop.OpenWindow();
                return;
            }

            runPhase?.TrySellCarriedAnimal(wallet);
        }

        private void CloseSellShop()
        {
            sellShop ??= FindFirstObjectByType<KickLuckyCubeSellShopController>(FindObjectsInactive.Include);
            sellShop?.CloseWindow();
        }
    }
}
