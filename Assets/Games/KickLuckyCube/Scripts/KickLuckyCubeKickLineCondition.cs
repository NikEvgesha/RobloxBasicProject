using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeKickLineCondition : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeKickController kickController;

        private void Awake()
        {
            kickController ??= FindFirstObjectByType<KickLuckyCubeKickController>();
        }

        public bool CanInteract(GameObject actor)
        {
            return kickController != null && kickController.CanKick;
        }
    }
}
