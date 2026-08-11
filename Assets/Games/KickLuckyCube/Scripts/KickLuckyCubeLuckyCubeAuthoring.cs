using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeLuckyCubeAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform geometryRoot;
        [SerializeField] private Transform edgesRoot;
        [SerializeField] private Transform[] questionMarks = Array.Empty<Transform>();
        [SerializeField] private Collider gameplayCollider;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private Transform trailAnchor;
        [SerializeField] private Transform impactVfxAnchor;
        [SerializeField] private Renderer[] styleRenderers = Array.Empty<Renderer>();

        public Transform GeometryRoot => geometryRoot;
        public Transform EdgesRoot => edgesRoot;
        public IReadOnlyList<Transform> QuestionMarks => questionMarks;
        public Collider GameplayCollider => gameplayCollider;
        public Transform CarryAnchor => carryAnchor;
        public Transform TrailAnchor => trailAnchor;
        public Transform ImpactVfxAnchor => impactVfxAnchor;
        public IReadOnlyList<Renderer> StyleRenderers => styleRenderers;

        public void AutoBind()
        {
            geometryRoot ??= FindChild("Geometry") ?? transform;
            edgesRoot ??= FindChild("Edge") != null ? transform : null;
            gameplayCollider ??= GetComponentInChildren<Collider>(true);
            carryAnchor ??= FindChild("CarryAnchor");
            trailAnchor ??= FindChild("TrailAnchor");
            impactVfxAnchor ??= FindChild("Impact");
            styleRenderers = GetComponentsInChildren<Renderer>(true);

            var marks = new List<Transform>();
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child == transform) continue;
                var childName = child.name;
                if (childName.EndsWith("QuestionBlocks_Front", StringComparison.OrdinalIgnoreCase)
                    || childName.EndsWith("QuestionBlocks_Back", StringComparison.OrdinalIgnoreCase)
                    || childName.EndsWith("QuestionBlocks_Left", StringComparison.OrdinalIgnoreCase)
                    || childName.EndsWith("QuestionBlocks_Right", StringComparison.OrdinalIgnoreCase)
                    || childName.EndsWith("QuestionBlocks_Top", StringComparison.OrdinalIgnoreCase)
                    || childName.EndsWith("QuestionBlocks_Bottom", StringComparison.OrdinalIgnoreCase))
                {
                    marks.Add(child);
                }
            }
            questionMarks = marks.ToArray();
        }

        public void CollectValidationIssues(ICollection<string> issues)
        {
            if (issues == null) return;
            if (geometryRoot == null) issues.Add($"{name}: geometry root is not assigned.");
            if (edgesRoot == null) issues.Add($"{name}: edges root is not assigned.");
            if (gameplayCollider == null) issues.Add($"{name}: gameplay collider is not assigned.");
            if (carryAnchor == null) issues.Add($"{name}: carry anchor is not assigned.");
            if (trailAnchor == null) issues.Add($"{name}: trail anchor is not assigned.");
            if (impactVfxAnchor == null) issues.Add($"{name}: impact VFX anchor is not assigned.");
            if (questionMarks == null || questionMarks.Length != 6)
                issues.Add($"{name}: expected 6 question-mark faces, found {questionMarks?.Length ?? 0}.");
        }

        private Transform FindChild(string partialName)
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
                if (child != transform && child.name.Contains(partialName, StringComparison.OrdinalIgnoreCase)) return child;
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.8f);
            if (carryAnchor != null) Gizmos.DrawWireSphere(carryAnchor.position, 0.12f);
            if (trailAnchor != null) Gizmos.DrawWireSphere(trailAnchor.position, 0.12f);
            if (impactVfxAnchor != null) Gizmos.DrawWireSphere(impactVfxAnchor.position, 0.12f);
        }
    }
}
