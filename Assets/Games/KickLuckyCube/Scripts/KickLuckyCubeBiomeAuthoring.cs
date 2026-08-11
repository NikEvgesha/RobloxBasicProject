using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeBiomeAuthoring : MonoBehaviour
    {
        [SerializeField] private string biomeId = "biome";
        [SerializeField, Min(1)] private int locationIndex = 1;
        [SerializeField] private Color sceneGuideColor = new(0.25f, 0.9f, 0.45f, 0.55f);
        [SerializeField] private KickLuckyCubeRarityZone rarityZone;
        [SerializeField] private Collider gameplayFloor;
        [SerializeField] private Transform startAnchor;
        [SerializeField] private Transform endAnchor;
        [SerializeField] private Transform landmarksRoot;
        [SerializeField] private Transform detailsRoot;
        [SerializeField] private Transform vfxRoot;
        [SerializeField] private bool decorationMayBeNonColliding = true;

        public string BiomeId => string.IsNullOrWhiteSpace(biomeId) ? gameObject.name : biomeId;
        public int LocationIndex => Mathf.Max(1, locationIndex);
        public KickLuckyCubeRarityZone RarityZone => rarityZone;
        public Collider GameplayFloor => gameplayFloor;
        public Transform StartAnchor => startAnchor;
        public Transform EndAnchor => endAnchor;
        public Transform LandmarksRoot => landmarksRoot;
        public Transform DetailsRoot => detailsRoot;
        public Transform VfxRoot => vfxRoot;
        public bool DecorationMayBeNonColliding => decorationMayBeNonColliding;

        public void AutoBind()
        {
            rarityZone ??= GetComponentInChildren<KickLuckyCubeRarityZone>(true);
            gameplayFloor ??= FindFloorCollider();
            startAnchor ??= FindChild("StartAnchor");
            endAnchor ??= FindChild("EndAnchor");
            landmarksRoot ??= FindChild("Landmarks");
            detailsRoot ??= FindChild("Details");
            vfxRoot ??= FindChild("VFX");
        }

        public void CollectValidationIssues(ICollection<string> issues)
        {
            if (issues == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(biomeId)) issues.Add($"{name}: biome id is empty.");
            if (rarityZone == null) issues.Add($"{name}: rarity zone is not assigned.");
            if (gameplayFloor == null) issues.Add($"{name}: gameplay floor collider is not assigned.");
            if (startAnchor == null) issues.Add($"{name}: start anchor is not assigned.");
            if (endAnchor == null) issues.Add($"{name}: end anchor is not assigned.");
            if (landmarksRoot == null) issues.Add($"{name}: landmarks root is not assigned.");
            if (detailsRoot == null) issues.Add($"{name}: details root is not assigned.");
            if (startAnchor != null && endAnchor != null && endAnchor.position.z <= startAnchor.position.z)
            {
                issues.Add($"{name}: end anchor must be farther down the corridor than start anchor.");
            }
        }

        private Collider FindFloorCollider()
        {
            var colliders = GetComponentsInChildren<Collider>(true);
            foreach (var candidate in colliders)
            {
                if (candidate != null && candidate.name.Contains("Floor", StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return colliders.Length > 0 ? colliders[0] : null;
        }

        private Transform FindChild(string partialName)
        {
            var children = GetComponentsInChildren<Transform>(true);
            foreach (var child in children)
            {
                if (child != transform && child.name.Contains(partialName, StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }
            }

            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = sceneGuideColor;
            if (startAnchor != null) Gizmos.DrawWireSphere(startAnchor.position, 0.7f);
            if (endAnchor != null) Gizmos.DrawWireSphere(endAnchor.position, 0.7f);
            if (startAnchor != null && endAnchor != null) Gizmos.DrawLine(startAnchor.position, endAnchor.position);
            if (gameplayFloor != null) Gizmos.DrawWireCube(gameplayFloor.bounds.center, gameplayFloor.bounds.size);
        }
    }
}
