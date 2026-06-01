using System.Linq;
using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStableCollectButton : MonoBehaviour, GameKitInteractionCondition
    {
        [SerializeField] private KickLuckyCubeWallet wallet;
        [SerializeField] private KickLuckyCubeStableSlot[] stableSlots;
        [SerializeField] private TextMesh statusLabel;

        public int PendingSoft => stableSlots != null ? stableSlots.Sum(slot => slot != null ? slot.PendingSoft : 0) : 0;

        private void Awake()
        {
            wallet ??= FindFirstObjectByType<KickLuckyCubeWallet>();

            if (stableSlots == null || stableSlots.Length == 0)
            {
                stableSlots = FindObjectsByType<KickLuckyCubeStableSlot>(FindObjectsSortMode.None);
            }

            RefreshLabel();
        }

        private void Update()
        {
            RefreshLabel();
        }

        public bool CanInteract(GameObject actor)
        {
            return wallet != null && PendingSoft > 0;
        }

        public void Collect(GameObject actor)
        {
            if (wallet == null || stableSlots == null)
            {
                return;
            }

            var total = 0;
            foreach (var slot in stableSlots)
            {
                if (slot == null)
                {
                    continue;
                }

                total += slot.Collect();
            }

            if (total > 0)
            {
                wallet.AddSoft(total);
            }

            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (statusLabel == null)
            {
                return;
            }

            statusLabel.text = PendingSoft > 0
                ? $"Collect {PendingSoft} soft"
                : "Stable collect";
        }
    }
}
