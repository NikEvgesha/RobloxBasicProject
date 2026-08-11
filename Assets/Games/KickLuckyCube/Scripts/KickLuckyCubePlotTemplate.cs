using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubePlotTemplate : MonoBehaviour
    {
        [SerializeField] private Renderer[] ownerTintRenderers = Array.Empty<Renderer>();
        [SerializeField] private TextMesh ownerLabel;
        [SerializeField] private Color playerTint = new(0.2f, 0.95f, 0.42f, 1f);
        [SerializeField] private Color botTint = new(0.35f, 0.68f, 1f, 1f);
        [SerializeField] private KickLuckyCubePlayerBaseAuthoring authoring;

        private MaterialPropertyBlock materialBlock;

        public KickLuckyCubePlayerBaseAuthoring Authoring => authoring;

        private void Reset()
        {
            authoring = GetComponent<KickLuckyCubePlayerBaseAuthoring>();
        }

        private void OnValidate()
        {
            authoring ??= GetComponent<KickLuckyCubePlayerBaseAuthoring>();
        }

        public void ConfigureOwner(string ownerName, bool isPlayerOwned)
        {
            gameObject.name = isPlayerOwned
                ? "KLC_PlayerPlot_Instance"
                : "KLC_BotPlot_" + Sanitize(ownerName);

            if (ownerLabel != null)
            {
                ownerLabel.text = isPlayerOwned ? "YOU" : ownerName;
                ownerLabel.gameObject.SetActive(false);
            }

            var tint = isPlayerOwned ? playerTint : botTint;
            ApplyTint(tint);
            ConfigureStablePersistence(ownerName, isPlayerOwned);
        }

        private void ApplyTint(Color tint)
        {
            materialBlock ??= new MaterialPropertyBlock();

            foreach (var target in ownerTintRenderers)
            {
                if (target == null)
                {
                    continue;
                }

                target.GetPropertyBlock(materialBlock);
                materialBlock.SetColor("_BaseColor", tint);
                materialBlock.SetColor("_Color", tint);
                target.SetPropertyBlock(materialBlock);
            }
        }

        private static string Sanitize(string value)
        {
            var source = string.IsNullOrWhiteSpace(value) ? "Player" : value;
            var chars = source.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (!char.IsLetterOrDigit(chars[i]))
                {
                    chars[i] = '_';
                }
            }

            return new string(chars);
        }

        private void ConfigureStablePersistence(string ownerName, bool isPlayerOwned)
        {
            var ownerId = isPlayerOwned ? "Player" : "Bot." + Sanitize(ownerName);
            var stableSlots = GetComponentsInChildren<KickLuckyCubeStableSlot>(true);
            foreach (var slot in stableSlots)
            {
                if (slot == null)
                {
                    continue;
                }

                slot.ConfigurePersistence(ownerId + "." + slot.gameObject.name, isPlayerOwned);
            }
        }
    }
}
