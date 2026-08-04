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

        public void Configure(int tier, int maximumTier)
        {
            tier = Mathf.Max(1, tier);
            var tier01 = Mathf.InverseLerp(1f, Mathf.Max(1f, maximumTier), tier);
            var isDumbbellTier = tier % 2 != 0;
            var color = TierColors[Mathf.Clamp(tier - 1, 0, TierColors.Length - 1)];
            var barLength = isDumbbellTier
                ? Mathf.Lerp(0.30f, 0.42f, tier01)
                : Mathf.Lerp(0.52f, 0.78f, tier01);
            var barWidth = Mathf.Lerp(0.052f, 0.105f, tier01);
            var weightSize = Mathf.Lerp(0.22f, 0.48f, tier01);
            var weightWidth = isDumbbellTier
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
    }
}