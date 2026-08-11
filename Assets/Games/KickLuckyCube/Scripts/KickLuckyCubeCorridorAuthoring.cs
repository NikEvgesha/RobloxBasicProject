using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeCorridorAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform previewRoot;
        [SerializeField] private Transform firstLocationAnchor;
        [SerializeField] private GameObject[] biomePrefabs = Array.Empty<GameObject>();
        [SerializeField, Min(1)] private int expectedLocationCount = KickLuckyCubeCorridorLayout.LocationCount;
        [SerializeField, Min(1f)] private float locationSpacing = KickLuckyCubeCorridorLayout.LocationSpacing;
        [SerializeField] private bool inheritAnchorRotation = true;
        [SerializeField] private bool previewVisible = true;

        public Transform PreviewRoot => previewRoot;
        public Transform FirstLocationAnchor => firstLocationAnchor;
        public IReadOnlyList<GameObject> BiomePrefabs => biomePrefabs;
        public int ExpectedLocationCount => Mathf.Max(1, expectedLocationCount);
        public float LocationSpacing => Mathf.Max(1f, locationSpacing);
        public bool InheritAnchorRotation => inheritAnchorRotation;
        public bool PreviewVisible => previewVisible;

        public void AutoBind()
        {
            previewRoot ??= FindChild("AuthoringBiomeInstances");
            firstLocationAnchor ??= FindChild("FirstLocationAnchor") ?? transform;
        }

        public Vector3 GetPreviewPosition(int index)
        {
            var anchor = firstLocationAnchor != null ? firstLocationAnchor : transform;
            return anchor.position + anchor.forward * (Mathf.Max(0, index) * LocationSpacing);
        }

        public Quaternion GetPreviewRotation()
        {
            var anchor = firstLocationAnchor != null ? firstLocationAnchor : transform;
            return inheritAnchorRotation ? anchor.rotation : Quaternion.identity;
        }

        public void SetPreviewRoot(Transform value)
        {
            previewRoot = value;
            if (previewRoot != null) previewRoot.gameObject.SetActive(previewVisible);
        }

        public void CollectValidationIssues(ICollection<string> issues)
        {
            if (issues == null) return;
            if (firstLocationAnchor == null) issues.Add($"{name}: first location anchor is not assigned.");
            if (biomePrefabs == null || biomePrefabs.Length != ExpectedLocationCount)
                issues.Add($"{name}: expected {ExpectedLocationCount} biome slots, found {biomePrefabs?.Length ?? 0}.");
            if (biomePrefabs == null) return;
            for (var index = 0; index < biomePrefabs.Length; index++)
            {
                var prefab = biomePrefabs[index];
                if (prefab == null) issues.Add($"{name}: biome slot {index + 1} is empty.");
                else if (prefab.GetComponent<KickLuckyCubeBiomeAuthoring>() == null)
                    issues.Add($"{name}: {prefab.name} has no biome authoring contract.");
            }
        }

        private Transform FindChild(string partialName)
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
                if (child != transform && child.name.Contains(partialName, StringComparison.OrdinalIgnoreCase)) return child;
            return null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.42f);
            for (var index = 0; index < ExpectedLocationCount; index++)
            {
                var position = GetPreviewPosition(index);
                Gizmos.DrawWireCube(position, new Vector3(4f, 0.15f, LocationSpacing * 0.82f));
            }
        }
    }
}
