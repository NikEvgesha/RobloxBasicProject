using UnityEngine;

namespace RobloxBasicProject.GameKit.Interaction
{
    public interface GameKitInteractionCondition
    {
        bool CanInteract(GameObject actor);
    }
}
