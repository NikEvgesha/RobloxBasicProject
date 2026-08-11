using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubePlayerBaseAuthoring : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeStableSlot[] stableSlots = Array.Empty<KickLuckyCubeStableSlot>();
        [SerializeField] private Transform homeMarkerAnchor;
        [SerializeField] private Transform ownerLabelAnchor;
        [SerializeField] private Transform interactionRoot;
        [SerializeField] private Transform decorationRoot;
        [SerializeField] private Transform botTrainingAnchor;
        [SerializeField] private Transform botKickAnchor;

        public IReadOnlyList<KickLuckyCubeStableSlot> StableSlots => stableSlots;
        public Transform HomeMarkerAnchor => homeMarkerAnchor;
        public Transform OwnerLabelAnchor => ownerLabelAnchor;
        public Transform InteractionRoot => interactionRoot;
        public Transform DecorationRoot => decorationRoot;
        public Transform BotTrainingAnchor => botTrainingAnchor;
        public Transform BotKickAnchor => botKickAnchor;

        public void AutoBind()
        {
            stableSlots = GetComponentsInChildren<KickLuckyCubeStableSlot>(true);
            homeMarkerAnchor ??= FindChild("Home");
            ownerLabelAnchor ??= FindChild("Owner");
            interactionRoot ??= FindChild("Interaction");
            decorationRoot ??= FindChild("Decoration");
            var botAnchors = GetComponentInChildren<KickLuckyCubeBotActivityAnchors>(true);
            if (botAnchors != null)
            {
                botTrainingAnchor ??= botAnchors.TrainingPoint;
                botKickAnchor ??= botAnchors.KickPoint;
            }
        }

        public void CollectValidationIssues(ICollection<string> issues)
        {
            if (issues == null) return;
            if (stableSlots == null || stableSlots.Length == 0) issues.Add($"{name}: no stable slots are assigned.");
            if (homeMarkerAnchor == null) issues.Add($"{name}: home marker anchor is not assigned.");
            if (ownerLabelAnchor == null) issues.Add($"{name}: owner label anchor is not assigned.");
            if (interactionRoot == null) issues.Add($"{name}: interaction root is not assigned.");
            if (decorationRoot == null) issues.Add($"{name}: decoration root is not assigned.");
        }

        private Transform FindChild(string partialName)
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child != transform && child.name.Contains(partialName, StringComparison.OrdinalIgnoreCase))
                    return child;
            }
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.45f, 0.75f);
            if (homeMarkerAnchor != null) Gizmos.DrawWireSphere(homeMarkerAnchor.position, 0.8f);
            if (ownerLabelAnchor != null) Gizmos.DrawLine(transform.position, ownerLabelAnchor.position);
        }
    }
}
