using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeToolPreviewVisual : MonoBehaviour
    {
        [SerializeField] private Transform bar;
        [SerializeField] private Transform leftWeight;
        [SerializeField] private Transform rightWeight;
        [SerializeField] private Transform leftPlate;
        [SerializeField] private Transform rightPlate;
        [SerializeField] private Renderer[] renderers = System.Array.Empty<Renderer>();
        [SerializeField] private string authoredToolsResourcePath = "KickLuckyCube/KLC_StrengthTools";
        [SerializeField] private string leftDumbbellSocketName = "SOCKET_Dumbbell_L";
        [SerializeField] private string rightDumbbellSocketName = "SOCKET_Dumbbell_R";
        [SerializeField] private string barbellSocketName = "B_Prop_Barbell";
        [SerializeField] private Vector3 dumbbellLocalEuler;
        [SerializeField] private Vector3 barbellLocalEuler;
        [SerializeField, Min(0.01f)] private float authoredLocalScale = 1f;

        private readonly List<GameObject> authoredParts = new();

        private static readonly Color[] TierColors =
        {
            new(0.42f, 0.40f, 0.36f),
            new(0.24f, 0.27f, 0.30f),
            new(0.62f, 0.68f, 0.73f),
            new(1.00f, 0.68f, 0.12f),
            new(0.62f, 0.80f, 0.88f),
            new(0.14f, 0.08f, 0.19f),
            new(0.10f, 0.94f, 0.92f),
            new(0.55f, 0.25f, 0.10f),
            new(0.60f, 0.94f, 1.00f),
            new(0.10f, 0.34f, 0.94f),
            new(0.60f, 0.20f, 0.88f),
            new(0.18f, 0.05f, 0.28f),
            new(0.92f, 0.08f, 0.18f),
            new(0.06f, 0.72f, 0.36f),
            new(0.58f, 0.96f, 0.12f)
        };

        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorProperty = Shader.PropertyToID("_Color");

        public bool UsesAuthoredModel { get; private set; }
        public bool IsDumbbellTier { get; private set; }
        public string ActiveTierRootName { get; private set; } = string.Empty;
        public int ActiveRendererCount { get; private set; }

        public void Configure(int tier, int maximumTier)
        {
            Configure(tier, maximumTier, null);
        }

        public void Configure(int tier, int maximumTier, Transform mannequinRoot)
        {
            tier = Mathf.Max(1, tier);
            IsDumbbellTier = tier % 2 != 0;
            ClearAuthoredParts();
            if (mannequinRoot != null && TryBuildAuthoredModel(tier, mannequinRoot))
            {
                SetLegacyPartsVisible(false);
                return;
            }

            UsesAuthoredModel = false;
            ActiveTierRootName = string.Empty;
            SetLegacyPartsVisible(true);
            ConfigureLegacyFallback(tier, maximumTier);
        }

        private void ConfigureLegacyFallback(int tier, int maximumTier)
        {
            var tier01 = Mathf.InverseLerp(1f, Mathf.Max(1f, maximumTier), tier);
            var color = TierColors[Mathf.Clamp(tier - 1, 0, TierColors.Length - 1)];
            var barLength = IsDumbbellTier
                ? Mathf.Lerp(0.30f, 0.42f, tier01)
                : Mathf.Lerp(0.52f, 0.78f, tier01);
            var barWidth = Mathf.Lerp(0.052f, 0.105f, tier01);
            var weightSize = Mathf.Lerp(0.22f, 0.48f, tier01);
            var weightWidth = IsDumbbellTier
                ? Mathf.Lerp(0.11f, 0.22f, tier01)
                : Mathf.Lerp(0.08f, 0.18f, tier01);
            var plateGap = Mathf.Lerp(0.09f, 0.16f, tier01);
            var showOuterPlates = tier >= 4;

            ConfigurePart(bar, Vector3.zero, new Vector3(barWidth, barLength, barWidth));
            ConfigurePart(leftWeight, new Vector3(0f, -barLength - plateGap * 0.35f, 0f), new Vector3(weightSize, weightWidth, weightSize));
            ConfigurePart(rightWeight, new Vector3(0f, barLength + plateGap * 0.35f, 0f), new Vector3(weightSize, weightWidth, weightSize));
            ConfigurePart(leftPlate, new Vector3(0f, -barLength - plateGap - weightWidth, 0f), new Vector3(weightSize * 0.82f, weightWidth * 0.82f, weightSize * 0.82f), showOuterPlates);
            ConfigurePart(rightPlate, new Vector3(0f, barLength + plateGap + weightWidth, 0f), new Vector3(weightSize * 0.82f, weightWidth * 0.82f, weightSize * 0.82f), showOuterPlates);

            var propertyBlock = new MaterialPropertyBlock();
            for (var index = 0; index < renderers.Length; index++)
            {
                var current = renderers[index];
                if (current == null)
                {
                    continue;
                }

                var partColor = index == 0 ? Shade(color, 0.62f) : index <= 2 ? color : Shade(color, 0.78f);
                current.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(BaseColorProperty, partColor);
                propertyBlock.SetColor(ColorProperty, partColor);
                current.SetPropertyBlock(propertyBlock);
            }

            ActiveRendererCount = renderers != null ? renderers.Length : 0;
        }

        private bool TryBuildAuthoredModel(int tier, Transform mannequinRoot)
        {
            var source = Resources.Load<GameObject>(authoredToolsResourcePath);
            if (source == null)
            {
                Debug.LogError(
                    $"[KLC-TRAINING] Missing authored strength tools at Resources/{authoredToolsResourcePath}.",
                    this);
                return false;
            }

            var leftSocket = FindDescendant(mannequinRoot, leftDumbbellSocketName);
            var rightSocket = FindDescendant(mannequinRoot, rightDumbbellSocketName);
            var barbellSocket = FindDescendant(mannequinRoot, barbellSocketName);
            if (IsDumbbellTier && (leftSocket == null || rightSocket == null))
            {
                Debug.LogError("[KLC-TRAINING] Dumbbell sockets are missing from the Blockbench mannequin.", this);
                return false;
            }

            if (!IsDumbbellTier && barbellSocket == null)
            {
                Debug.LogError("[KLC-TRAINING] Barbell socket is missing from the Blockbench mannequin.", this);
                return false;
            }

            var modelInstance = Instantiate(source, transform, false);
            modelInstance.name = "KLC_AuthoredStrengthToolSource";
            var tierPrefix = $"ToolTier_{Mathf.Clamp(tier, 1, 15):00}_";
            var tierRoot = FindDescendant(modelInstance.transform, tierPrefix, true);
            if (tierRoot == null)
            {
                Destroy(modelInstance);
                Debug.LogError("[KLC-TRAINING] Missing authored tier root " + tierPrefix + ".", this);
                return false;
            }

            ActiveTierRootName = tierRoot.name;
            if (IsDumbbellTier)
            {
                var left = FindDescendant(tierRoot, "Dumbbell_L", true);
                var right = FindDescendant(tierRoot, "Dumbbell_R", true);
                if (left == null || right == null)
                {
                    Destroy(modelInstance);
                    Debug.LogError("[KLC-TRAINING] Authored dumbbell tier must contain left and right models.", this);
                    return false;
                }

                AttachPart(left, leftSocket, dumbbellLocalEuler);
                AttachPart(right, rightSocket, dumbbellLocalEuler);
            }
            else
            {
                var barbell = FindDescendant(tierRoot, "Barbell", true, true);
                if (barbell == null)
                {
                    Destroy(modelInstance);
                    Debug.LogError("[KLC-TRAINING] Authored barbell tier is missing its model root.", this);
                    return false;
                }

                AttachPart(barbell, barbellSocket, barbellLocalEuler);
            }

            Destroy(modelInstance);
            UsesAuthoredModel = authoredParts.Count > 0;
            ActiveRendererCount = 0;
            foreach (var part in authoredParts)
            {
                if (part != null)
                {
                    ActiveRendererCount += part.GetComponentsInChildren<Renderer>(true).Length;
                }
            }

            return UsesAuthoredModel && ActiveRendererCount > 0;
        }

        private void AttachPart(Transform part, Transform socket, Vector3 localEuler)
        {
            part.SetParent(socket, false);
            part.localPosition = Vector3.zero;
            part.localRotation = Quaternion.Euler(localEuler);
            part.localScale = Vector3.one * Mathf.Max(0.01f, authoredLocalScale);
            part.gameObject.SetActive(true);
            authoredParts.Add(part.gameObject);
        }

        private void ClearAuthoredParts()
        {
            for (var index = authoredParts.Count - 1; index >= 0; index--)
            {
                if (authoredParts[index] != null)
                {
                    Destroy(authoredParts[index]);
                }
            }

            authoredParts.Clear();
            UsesAuthoredModel = false;
            ActiveRendererCount = 0;
        }

        private void SetLegacyPartsVisible(bool visible)
        {
            ConfigureLegacyPartVisibility(bar, visible);
            ConfigureLegacyPartVisibility(leftWeight, visible);
            ConfigureLegacyPartVisibility(rightWeight, visible);
            ConfigureLegacyPartVisibility(leftPlate, visible);
            ConfigureLegacyPartVisibility(rightPlate, visible);
        }

        private static void ConfigureLegacyPartVisibility(Transform part, bool visible)
        {
            if (part != null)
            {
                part.gameObject.SetActive(visible);
            }
        }

        private static Transform FindDescendant(
            Transform root,
            string nameOrPrefix,
            bool contains = false,
            bool skipRoot = false)
        {
            if (root == null || string.IsNullOrWhiteSpace(nameOrPrefix))
            {
                return null;
            }

            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child == null || (skipRoot && child == root))
                {
                    continue;
                }

                var matches = contains
                    ? child.name.Contains(nameOrPrefix, StringComparison.Ordinal)
                    : child.name.Equals(nameOrPrefix, StringComparison.Ordinal)
                        || child.name.StartsWith(nameOrPrefix, StringComparison.Ordinal);
                if (matches)
                {
                    return child;
                }
            }

            return null;
        }

        public void SetReferences(
            Transform barTransform,
            Transform leftWeightTransform,
            Transform rightWeightTransform,
            Transform leftPlateTransform,
            Transform rightPlateTransform,
            Renderer[] partRenderers)
        {
            bar = barTransform;
            leftWeight = leftWeightTransform;
            rightWeight = rightWeightTransform;
            leftPlate = leftPlateTransform;
            rightPlate = rightPlateTransform;
            renderers = partRenderers ?? System.Array.Empty<Renderer>();
        }

        private static Color Shade(Color source, float multiplier)
        {
            return new Color(source.r * multiplier, source.g * multiplier, source.b * multiplier, source.a);
        }

        private static void ConfigurePart(Transform part, Vector3 localPosition, Vector3 localScale, bool visible = true)
        {
            if (part == null)
            {
                return;
            }

            part.gameObject.SetActive(visible);
            part.localPosition = localPosition;
            part.localRotation = Quaternion.identity;
            part.localScale = localScale;
        }

        private void OnDestroy()
        {
            ClearAuthoredParts();
        }
    }
}
