using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeWorldShopAuthoring : MonoBehaviour
    {
        public enum ShopKind
        {
            Speed,
            TrainingTools,
            SellAnimals,
            Styles,
            Exchange,
            EliteMobs,
            Weather,
            Rewards,
        }

        public enum PreviewState
        {
            Idle,
            Ready,
            Active,
            Cooldown,
            Claimed,
        }

        [SerializeField] private ShopKind shopKind;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Transform interactionAnchor;
        [SerializeField] private Transform signAnchor;
        [SerializeField] private GameObject windowPrefab;
        [SerializeField] private MonoBehaviour interactionBinding;
        [SerializeField] private GameObject[] readyStateObjects = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] activeStateObjects = System.Array.Empty<GameObject>();
        [SerializeField] private GameObject[] claimedStateObjects = System.Array.Empty<GameObject>();
        [SerializeField] private PreviewState editorPreviewState;

        public ShopKind Kind => shopKind;
        public Transform VisualRoot => visualRoot;
        public Transform InteractionAnchor => interactionAnchor;
        public Transform SignAnchor => signAnchor;
        public GameObject WindowPrefab => windowPrefab;
        public MonoBehaviour InteractionBinding => interactionBinding;
        public PreviewState EditorPreviewState => editorPreviewState;

        public void AutoBind()
        {
            visualRoot ??= FindChild("Visual");
            interactionAnchor ??= FindChild("Interact");
            signAnchor ??= FindChild("Sign");
            if (interactionBinding == null)
            {
                foreach (var candidate in GetComponentsInChildren<MonoBehaviour>(true))
                {
                    var typeName = candidate.GetType().Name;
                    if (typeName.Contains("Interaction", System.StringComparison.OrdinalIgnoreCase)
                        || typeName.EndsWith("Pad", System.StringComparison.OrdinalIgnoreCase)
                        || typeName.EndsWith("Spot", System.StringComparison.OrdinalIgnoreCase))
                    {
                        interactionBinding = candidate;
                        break;
                    }
                }
            }
        }

        public void ApplyPreviewState(PreviewState state)
        {
            editorPreviewState = state;
            SetActive(readyStateObjects, state == PreviewState.Ready);
            SetActive(activeStateObjects, state == PreviewState.Active || state == PreviewState.Cooldown);
            SetActive(claimedStateObjects, state == PreviewState.Claimed);
        }

        public void CollectValidationIssues(ICollection<string> issues)
        {
            if (issues == null) return;
            if (visualRoot == null) issues.Add($"{name}: visual root is not assigned.");
            if (interactionAnchor == null) issues.Add($"{name}: interaction anchor is not assigned.");
            if (signAnchor == null) issues.Add($"{name}: sign anchor is not assigned.");
            if (windowPrefab == null) issues.Add($"{name}: UI window prefab is not assigned.");
            if (interactionBinding == null) issues.Add($"{name}: interaction binding is not assigned.");
        }

        private Transform FindChild(string partialName)
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                if (child != transform && child.name.Contains(partialName, System.StringComparison.OrdinalIgnoreCase))
                    return child;
            }
            return null;
        }

        private static void SetActive(GameObject[] targets, bool active)
        {
            if (targets == null) return;
            foreach (var target in targets)
                if (target != null) target.SetActive(active);
        }

        private void OnDrawGizmosSelected()
        {
            if (interactionAnchor == null) return;
            Gizmos.color = new Color(0.15f, 0.8f, 1f, 0.75f);
            Gizmos.DrawWireSphere(interactionAnchor.position, 0.65f);
            Gizmos.DrawLine(transform.position, interactionAnchor.position);
        }
    }
}
