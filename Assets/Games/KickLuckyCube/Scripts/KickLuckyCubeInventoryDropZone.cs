using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeInventoryDropZone : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeInventoryController inventory;

        public void Configure(KickLuckyCubeInventoryController controller)
        {
            inventory = controller;
        }

        public bool TryAppendFrom(KickLuckyCubeInventorySlotView source)
        {
            return inventory != null && inventory.TryMoveAnimalToInventory(source);
        }
    }
}
