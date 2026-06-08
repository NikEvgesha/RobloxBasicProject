using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [RequireComponent(typeof(GameKitInteractionTarget))]
    public sealed class KickLuckyCubeSpeedShopPad : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeSpeedShopController speedShop;
        [SerializeField] private GameKitInteractionDriver driver;

        private GameKitInteractionTarget interactionTarget;

        private void Awake()
        {
            ResolveReferences();
            interactionTarget = GetComponent<GameKitInteractionTarget>();
            interactionTarget.ActorInteracted.AddListener(OpenShop);
        }

        private void OnDestroy()
        {
            if (interactionTarget != null)
            {
                interactionTarget.ActorInteracted.RemoveListener(OpenShop);
            }
        }

        private void OnDisable()
        {
            CloseShop();
        }

        private void OnTriggerExit(Collider other)
        {
            driver ??= FindFirstObjectByType<GameKitInteractionDriver>();
            if (driver == null || !driver.IsActorCollider(other))
            {
                return;
            }

            CloseShop();
        }

        public void Configure(KickLuckyCubeSpeedShopController controller)
        {
            speedShop = controller;
            ResolveReferences();
        }

        public bool CanInteract(GameObject actor)
        {
            ResolveReferences();
            return stats != null
                && speedShop != null
                && (runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal && !runPhase.IsSelectingAnimal));
        }

        private void OpenShop(GameObject actor)
        {
            ResolveReferences();
            speedShop?.OpenWindow();
        }

        private void CloseShop()
        {
            speedShop ??= FindFirstObjectByType<KickLuckyCubeSpeedShopController>(FindObjectsInactive.Include);
            speedShop?.CloseWindow();
        }

        private void ResolveReferences()
        {
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            speedShop ??= FindFirstObjectByType<KickLuckyCubeSpeedShopController>(FindObjectsInactive.Include);
            driver ??= FindFirstObjectByType<GameKitInteractionDriver>();
        }
    }
}
