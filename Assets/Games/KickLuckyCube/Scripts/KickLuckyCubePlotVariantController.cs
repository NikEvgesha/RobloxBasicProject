using System;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubePlotVariantController : MonoBehaviour
    {
        [SerializeField, Min(1)] private int variantNumber = 1;
        [SerializeField] private bool bindStableSlots = true;

        public int VariantNumber => variantNumber;

        public void Configure(int variant, bool bindSlots)
        {
            variantNumber = Mathf.Max(1, variant);
            bindStableSlots = bindSlots;
            BindToPlot();
        }

        private void Awake()
        {
            BindToPlot();
        }

        [ContextMenu("Bind Stable Slots To Variant")]
        public void BindToPlot()
        {
            if (!bindStableSlots || transform.parent == null)
            {
                return;
            }

            var plotRoot = transform;
            while (plotRoot.parent != null
                   && plotRoot.GetComponent<KickLuckyCubePlotTemplate>() == null)
            {
                plotRoot = plotRoot.parent;
            }

            if (plotRoot.GetComponent<KickLuckyCubePlotTemplate>() == null)
            {
                return;
            }
            var anchors = GetComponentsInChildren<KickLuckyCubePlotMobAnchor>(true)
                .Where(anchor => anchor != null && anchor.VariantNumber == variantNumber)
                .OrderBy(anchor => anchor.FloorNumber)
                .ThenBy(anchor => anchor.PositionNumber)
                .ToArray();
            var stableSlots = plotRoot.Cast<Transform>()
                .Where(child => child.name.StartsWith("Template_StableSlot_", StringComparison.Ordinal))
                .OrderBy(child => child.name, StringComparer.Ordinal)
                .ToArray();

            if (anchors.Length == 0 || stableSlots.Length == 0)
            {
                Debug.LogWarning(
                    $"Plot variant {variantNumber} has {anchors.Length} mob anchors for {stableSlots.Length} stable slots.",
                    this);
                return;
            }

            for (var index = 0; index < stableSlots.Length; index++)
            {
                var slot = stableSlots[index];
                var anchor = anchors[index % anchors.Length];
                var mobAnchor = slot.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(child => child != slot
                                             && child.name.IndexOf("MobAnchor", StringComparison.OrdinalIgnoreCase) >= 0);
                var mobAnchorOffset = mobAnchor != null ? mobAnchor.localPosition : Vector3.zero;
                slot.localPosition = plotRoot.InverseTransformPoint(anchor.transform.position) - mobAnchorOffset;
            }
        }
    }
}
