using System;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubePlotVariantCollection : MonoBehaviour
    {
        [SerializeField] private GameObject[] variants = Array.Empty<GameObject>();
        [SerializeField, Range(1, 4)] private int activeVariant = 1;

        public int ActiveVariant => activeVariant;
        public int VariantCount => variants?.Length ?? 0;

        private void Awake()
        {
            if (variants == null || variants.Length == 0 || Array.Exists(variants, variant => variant == null))
            {
                RepairReferencesFromChildren();
                return;
            }

            ApplyVariant();
        }

        private void OnValidate()
        {
            ApplyVariant();
        }

        public void Configure(GameObject[] variantObjects, int selectedVariant)
        {
            variants = variantObjects ?? Array.Empty<GameObject>();
            activeVariant = Mathf.Clamp(selectedVariant, 1, Mathf.Max(1, variants.Length));
            ApplyVariant();
        }

        public void SetVariant(int variantNumber)
        {
            activeVariant = Mathf.Clamp(variantNumber, 1, Mathf.Max(1, VariantCount));
            ApplyVariant();
        }

        private void ApplyVariant()
        {
            variants ??= Array.Empty<GameObject>();
            for (var index = 0; index < variants.Length; index++)
            {
                var variant = variants[index];
                if (variant == null)
                {
                    continue;
                }

                var shouldBeActive = index == activeVariant - 1;
                if (variant.activeSelf != shouldBeActive)
                {
                    variant.SetActive(shouldBeActive);
                }
            }

            var active = activeVariant > 0 && activeVariant <= variants.Length
                ? variants[activeVariant - 1]
                : null;
            active?.GetComponent<KickLuckyCubePlotVariantController>()?.BindToPlot();
        }

        [ContextMenu("Repair Variant References From Children")]
        public void RepairReferencesFromChildren()
        {
            variants = GetComponentsInChildren<KickLuckyCubePlotVariantController>(true)
                .OrderBy(controller => controller.VariantNumber)
                .Select(controller => controller.gameObject)
                .ToArray();
            activeVariant = Mathf.Clamp(activeVariant, 1, Mathf.Max(1, variants.Length));
            ApplyVariant();
        }
    }
}
