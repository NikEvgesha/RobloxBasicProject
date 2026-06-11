using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [RequireComponent(typeof(TextMesh))]
    public sealed class KickLuckyCubeWorldTextOutline : MonoBehaviour
    {
        private static readonly Vector3[] OffsetDirections =
        {
            Vector3.right,
            Vector3.left,
            Vector3.up,
            Vector3.down
        };

        [SerializeField] private Color outlineColor = Color.black;
        [SerializeField, Min(0f)] private float outlineOffset = 0.018f;

        private TextMesh source;
        private readonly TextMesh[] outlines = new TextMesh[4];

        private void Awake()
        {
            source = GetComponent<TextMesh>();
            EnsureOutlines();
            SyncOutlines();
        }

        private void LateUpdate()
        {
            EnsureOutlines();
            SyncOutlines();
        }

        private void OnEnable()
        {
            SetOutlinesActive(true);
        }

        private void OnDisable()
        {
            SetOutlinesActive(false);
        }

        public void Configure(Color color, float offset)
        {
            if (source == null)
            {
                source = GetComponent<TextMesh>();
            }

            outlineColor = color;
            outlineOffset = Mathf.Max(0f, offset);
            ApplyTextColor(source, Color.white);
            SyncOutlines();
        }

        private void EnsureOutlines()
        {
            if (source == null)
            {
                source = GetComponent<TextMesh>();
            }

            for (var index = 0; index < outlines.Length; index++)
            {
                if (outlines[index] != null)
                {
                    continue;
                }

                var outlineObject = new GameObject("Outline_" + index);
                outlineObject.transform.SetParent(transform, false);
                outlines[index] = outlineObject.AddComponent<TextMesh>();
            }
        }

        private void SyncOutlines()
        {
            if (source == null)
            {
                return;
            }

            var sourceRenderer = source.GetComponent<Renderer>();
            if (sourceRenderer != null)
            {
                sourceRenderer.sortingOrder = 20;
            }

            for (var index = 0; index < outlines.Length; index++)
            {
                var outline = outlines[index];
                if (outline == null)
                {
                    continue;
                }

                outline.transform.localPosition = OffsetDirections[index] * outlineOffset + Vector3.back * 0.01f;
                outline.transform.localRotation = Quaternion.identity;
                outline.transform.localScale = Vector3.one;
                outline.text = source.text;
                outline.font = source.font;
                outline.fontSize = source.fontSize;
                outline.fontStyle = source.fontStyle;
                outline.characterSize = source.characterSize;
                outline.anchor = source.anchor;
                outline.alignment = source.alignment;
                outline.richText = source.richText;
                outline.tabSize = source.tabSize;
                outline.lineSpacing = source.lineSpacing;
                outline.offsetZ = source.offsetZ + 0.01f;
                ApplyTextColor(outline, outlineColor);

                var outlineRenderer = outline.GetComponent<Renderer>();
                if (outlineRenderer != null)
                {
                    outlineRenderer.sortingOrder = 19;
                }
            }
        }

        public static void ApplyTextColor(TextMesh text, Color color)
        {
            if (text == null)
            {
                return;
            }

            text.color = color;

            var renderer = text.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            var material = renderer.material;
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        public static Transform ResolveRuntimeLabelRoot()
        {
            const string RootName = "KLC_RuntimeWorldLabels";
            var existing = GameObject.Find(RootName);
            var root = existing != null ? existing.transform : new GameObject(RootName).transform;
            root.position = Vector3.zero;
            root.rotation = Quaternion.identity;
            root.localScale = Vector3.one;
            return root;
        }

        private void SetOutlinesActive(bool isActive)
        {
            for (var index = 0; index < outlines.Length; index++)
            {
                if (outlines[index] != null)
                {
                    outlines[index].gameObject.SetActive(isActive);
                }
            }
        }
    }
}
