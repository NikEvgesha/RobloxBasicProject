using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [Serializable]
    public sealed class KickLuckyCubeSkinMaterialBinding
    {
        [SerializeField] private string skinToneId;
        [SerializeField] private Material material;

        public string SkinToneId => skinToneId;
        public Material Material => material;
    }

    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeCharacterAppearance : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Transform visualRoot;
        [SerializeField] private Renderer[] explicitSkinRenderers = Array.Empty<Renderer>();
        [SerializeField] private KickLuckyCubeSkinMaterialBinding[] skinMaterials = Array.Empty<KickLuckyCubeSkinMaterialBinding>();
        [SerializeField] private KickLuckyCubeAppearanceLoadout activeLoadout = new();

        private readonly Dictionary<string, Transform> rootBindings = new(StringComparer.Ordinal);
        private MaterialPropertyBlock propertyBlock;
        private bool isApplying;

        public event Action<KickLuckyCubeAppearanceLoadout> AppearanceApplied;

        public KickLuckyCubeAppearanceLoadout ActiveLoadout => activeLoadout?.Clone();
        public Transform VisualRoot => visualRoot;

        private void Awake()
        {
            ResolveVisualRoot();
            if (!HasCompleteLoadout(activeLoadout))
            {
                activeLoadout = KickLuckyCubeCharacterSkinCatalog.CreateDefaultLoadout();
            }

            ApplyLoadout(activeLoadout);
        }

        private void OnTransformChildrenChanged()
        {
            if (isApplying)
            {
                return;
            }

            rootBindings.Clear();
            ResolveVisualRoot();
            ApplyLoadout(activeLoadout);
        }

        public void SetVisualRoot(Transform value)
        {
            if (visualRoot == value)
            {
                return;
            }

            visualRoot = value;
            rootBindings.Clear();
            ApplyLoadout(activeLoadout);
        }

        public bool ApplyLoadout(KickLuckyCubeAppearanceLoadout loadout)
        {
            activeLoadout = Sanitize(loadout);
            ResolveVisualRoot();
            CacheBindings();

            var visibleRoots = new HashSet<string>(StringComparer.Ordinal);
            foreach (var slot in KickLuckyCubeCharacterSkinCatalog.SlotOrder)
            {
                var definition = KickLuckyCubeCharacterSkinCatalog.Get(activeLoadout.Get(slot));
                if (definition != null && !string.IsNullOrWhiteSpace(definition.VisualRootName))
                {
                    visibleRoots.Add(definition.VisualRootName);
                }
            }

            var foundAnyVariant = false;
            isApplying = true;
            try
            {
                foreach (var rootName in KickLuckyCubeCharacterSkinCatalog.AllVisualRootNames)
                {
                    if (!rootBindings.TryGetValue(rootName, out var variantRoot) || variantRoot == null)
                    {
                        continue;
                    }

                    foundAnyVariant = true;
                    var shouldBeVisible = visibleRoots.Contains(rootName);
                    if (variantRoot.gameObject.activeSelf != shouldBeVisible)
                    {
                        variantRoot.gameObject.SetActive(shouldBeVisible);
                    }
                }

                ApplyExportedRendererVisibility(visibleRoots);

                ApplySkinTone();
            }
            finally
            {
                isApplying = false;
            }

            AppearanceApplied?.Invoke(activeLoadout.Clone());
            return foundAnyVariant;
        }

        private void ApplyExportedRendererVisibility(HashSet<string> visibleRoots)
        {
            if (visualRoot == null)
            {
                return;
            }

            foreach (var renderer in visualRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null)
                {
                    continue;
                }

                var rendererName = renderer.gameObject.name;
                if (rendererName.StartsWith("preview_", StringComparison.OrdinalIgnoreCase))
                {
                    renderer.gameObject.SetActive(false);
                    continue;
                }

                var variantRootName = ResolveExportedRendererVariant(rendererName);
                if (!string.IsNullOrWhiteSpace(variantRootName))
                {
                    renderer.gameObject.SetActive(visibleRoots.Contains(variantRootName));
                }
            }
        }

        private static string ResolveExportedRendererVariant(string rendererName)
        {
            if (StartsWithAny(rendererName, "female_base_", "female_eye_", "female_eyebrow_", "female_lash_", "female_lower_lip", "female_nose", "female_pupil_", "female_smile_", "female_underwear_", "female_knuckle_"))
            {
                return "Mannequin_Female";
            }

            if (StartsWithAny(rendererName, "base_", "eye_white_", "eyebrow_", "lower_lip", "nose", "pupil_", "smile_", "underwear_"))
            {
                return "Mannequin_Male";
            }

            if (StartsWithAny(rendererName, "female_jacket_"))
            {
                return "Torso_Female_CroppedJacket";
            }

            if (StartsWithAny(rendererName, "unisex_varsity_"))
            {
                return "Torso_Unisex_Varsity";
            }

            if (StartsWithAny(rendererName, "hoodie_", "hood_", "drawstring_", "lucky_badge"))
            {
                return "Torso_Current_LuckyHoodie";
            }

            if (StartsWithAny(rendererName, "female_leggings_", "female_skort"))
            {
                return "Legs_Female_SkortLeggings";
            }

            if (StartsWithAny(rendererName, "unisex_cargo_"))
            {
                return "Legs_Unisex_Cargo";
            }

            if (StartsWithAny(rendererName, "jogger_", "pants_cuff_", "knee_panel_", "pants_waist_cover"))
            {
                return "Legs_Current_Joggers";
            }

            if (StartsWithAny(rendererName, "female_boot_"))
            {
                return "Boots_Female_AnkleBoots";
            }

            if (StartsWithAny(rendererName, "unisex_runner_", "unisex_lace_"))
            {
                return "Boots_Unisex_Runners";
            }

            if (StartsWithAny(rendererName, "sneaker_", "shoe_lace_", "sole_", "heel_"))
            {
                return "Boots_Current_Sneakers";
            }

            if (StartsWithAny(rendererName, "female_wrist_glove_", "female_bracelet_"))
            {
                return "Gloves_Female_Wrist";
            }

            if (StartsWithAny(rendererName, "unisex_sport_"))
            {
                return "Gloves_Unisex_Sport";
            }

            if (StartsWithAny(rendererName, "fingerless_glove_", "glove_back_", "glove_band_"))
            {
                return "Gloves_Current_Fingerless";
            }

            if (StartsWithAny(rendererName, "female_hair_", "female_bang_", "female_ponytail_"))
            {
                return "Hair_Female_Ponytail";
            }

            if (StartsWithAny(rendererName, "unisex_curl_"))
            {
                return "Hair_Unisex_Curly";
            }

            if (StartsWithAny(rendererName, "hair_"))
            {
                return "Hair_Current_Short";
            }

            if (StartsWithAny(rendererName, "beanie_"))
            {
                return "Hat_Beanie";
            }

            if (StartsWithAny(rendererName, "cap_"))
            {
                return "Hat_Cap";
            }

            if (StartsWithAny(rendererName, "bucket_"))
            {
                return "Hat_Bucket";
            }

            if (StartsWithAny(rendererName, "bandana_"))
            {
                return "Mask_Bandana";
            }

            if (StartsWithAny(rendererName, "visor_"))
            {
                return "Mask_Visor";
            }

            if (StartsWithAny(rendererName, "respirator_"))
            {
                return "Mask_Respirator";
            }

            return string.Empty;
        }

        private static bool StartsWithAny(string value, params string[] prefixes)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            for (var index = 0; index < prefixes.Length; index++)
            {
                if (value.StartsWith(prefixes[index], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public KickLuckyCubeAppearanceLoadout ApplyRandomLoadout(int seed, bool includePremium = true)
        {
            var loadout = KickLuckyCubeCharacterSkinCatalog.CreateRandomLoadout(seed, includePremium);
            ApplyLoadout(loadout);
            return loadout.Clone();
        }

        public void RefreshBindings()
        {
            rootBindings.Clear();
            ResolveVisualRoot();
            ApplyLoadout(activeLoadout);
        }

        private void ApplySkinTone()
        {
            propertyBlock ??= new MaterialPropertyBlock();
            var skinDefinition = KickLuckyCubeCharacterSkinCatalog.Get(activeLoadout.Get(KickLuckyCubeAppearanceSlot.SkinTone));
            if (skinDefinition == null)
            {
                return;
            }

            var replacement = FindSkinMaterial(skinDefinition.Id);
            var renderers = ResolveSkinRenderers();
            for (var rendererIndex = 0; rendererIndex < renderers.Count; rendererIndex++)
            {
                var renderer = renderers[rendererIndex];
                if (renderer == null)
                {
                    continue;
                }

                var materials = renderer.sharedMaterials;
                var materialsChanged = false;
                for (var materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    var sourceMaterial = materials[materialIndex];
                    if (!IsSkinMaterial(renderer, sourceMaterial))
                    {
                        continue;
                    }

                    if (replacement != null)
                    {
                        materials[materialIndex] = replacement;
                        materialsChanged = true;
                        renderer.SetPropertyBlock(null, materialIndex);
                        continue;
                    }

                    renderer.GetPropertyBlock(propertyBlock, materialIndex);
                    if (sourceMaterial != null && sourceMaterial.HasProperty(BaseColorId))
                    {
                        propertyBlock.SetColor(BaseColorId, skinDefinition.PreviewColor);
                    }

                    if (sourceMaterial != null && sourceMaterial.HasProperty(ColorId))
                    {
                        propertyBlock.SetColor(ColorId, skinDefinition.PreviewColor);
                    }

                    renderer.SetPropertyBlock(propertyBlock, materialIndex);
                    propertyBlock.Clear();
                }

                if (materialsChanged)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }

        private Material FindSkinMaterial(string skinToneId)
        {
            if (skinMaterials == null)
            {
                return null;
            }

            for (var index = 0; index < skinMaterials.Length; index++)
            {
                var binding = skinMaterials[index];
                if (binding != null && string.Equals(binding.SkinToneId, skinToneId, StringComparison.Ordinal))
                {
                    return binding.Material;
                }
            }

            return null;
        }

        private List<Renderer> ResolveSkinRenderers()
        {
            var renderers = new List<Renderer>();
            if (explicitSkinRenderers != null && explicitSkinRenderers.Length > 0)
            {
                for (var index = 0; index < explicitSkinRenderers.Length; index++)
                {
                    if (explicitSkinRenderers[index] != null)
                    {
                        renderers.Add(explicitSkinRenderers[index]);
                    }
                }

                return renderers;
            }

            if (visualRoot != null)
            {
                renderers.AddRange(visualRoot.GetComponentsInChildren<Renderer>(true));
            }

            return renderers;
        }

        private static bool IsSkinMaterial(Renderer renderer, Material material)
        {
            if (material != null && material.name.IndexOf("skin", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return renderer != null
                && renderer.gameObject.name.IndexOf("skin", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ResolveVisualRoot()
        {
            if (visualRoot != null)
            {
                return;
            }

            visualRoot = FindDescendant(transform, "KLC_PlayerMannequin")
                ?? FindDescendant(transform, "KLC_BlockbenchPlayerVisual")
                ?? FindDescendant(transform, "KLC_StealBrainrotPlayerVisual")
                ?? transform;
        }

        private void CacheBindings()
        {
            if (visualRoot == null)
            {
                return;
            }

            foreach (var rootName in KickLuckyCubeCharacterSkinCatalog.AllVisualRootNames)
            {
                if (rootBindings.ContainsKey(rootName))
                {
                    continue;
                }

                var match = FindDescendant(visualRoot, rootName);
                if (match != null)
                {
                    rootBindings.Add(rootName, match);
                }
            }
        }

        private static KickLuckyCubeAppearanceLoadout Sanitize(KickLuckyCubeAppearanceLoadout source)
        {
            var bodyId = source?.Get(KickLuckyCubeAppearanceSlot.Body);
            var female = KickLuckyCubeCharacterSkinCatalog.IsFemaleBody(bodyId);
            var result = new KickLuckyCubeAppearanceLoadout();
            foreach (var slot in KickLuckyCubeCharacterSkinCatalog.SlotOrder)
            {
                var itemId = source?.Get(slot);
                var definition = KickLuckyCubeCharacterSkinCatalog.Get(itemId);
                result.Set(slot, definition != null && definition.Slot == slot
                    ? definition.Id
                    : KickLuckyCubeCharacterSkinCatalog.GetDefaultId(slot, female));
            }

            return result;
        }

        private static bool HasCompleteLoadout(KickLuckyCubeAppearanceLoadout loadout)
        {
            if (loadout == null)
            {
                return false;
            }

            foreach (var slot in KickLuckyCubeCharacterSkinCatalog.SlotOrder)
            {
                var definition = KickLuckyCubeCharacterSkinCatalog.Get(loadout.Get(slot));
                if (definition == null || definition.Slot != slot)
                {
                    return false;
                }
            }

            return true;
        }

        private static Transform FindDescendant(Transform root, string exactName)
        {
            if (root == null || string.IsNullOrWhiteSpace(exactName))
            {
                return null;
            }

            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(child.name, exactName, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }
    }
}
