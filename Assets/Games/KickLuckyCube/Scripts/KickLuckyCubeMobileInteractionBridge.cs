using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeMobileInteractionBridge : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeMobileInput input;
        [SerializeField] private GameKitInteractionDriver interactionDriver;

        private void Awake()
        {
            input ??= FindFirstObjectByType<KickLuckyCubeMobileInput>(FindObjectsInactive.Include);
            interactionDriver ??= FindFirstObjectByType<GameKitInteractionDriver>(FindObjectsInactive.Include);
        }

        private void Update()
        {
            if (interactionDriver != null)
            {
                interactionDriver.SetExternalHold(input != null && input.InteractHeld);
            }
        }

        private void OnDisable()
        {
            interactionDriver?.SetExternalHold(false);
        }
    }
}
