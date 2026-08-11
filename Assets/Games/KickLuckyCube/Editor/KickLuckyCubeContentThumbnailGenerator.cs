using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace RobloxBasicProject.Games.KickLuckyCube.Editor
{
    public static class KickLuckyCubeContentThumbnailGenerator
    {
        private const int Resolution = 256;
        private const string PlayerModelPath = "Assets/Games/KickLuckyCube/Resources/KickLuckyCube/KLC_PlayerMannequin.fbx";
        private const string ToolModelPath = "Assets/Games/KickLuckyCube/Resources/KickLuckyCube/KLC_StrengthTools.fbx";
        private const string SkinIconFolder = "Assets/Games/KickLuckyCube/Resources/KickLuckyCube/UI/SkinItems";
        private const string ToolIconFolder = "Assets/Games/KickLuckyCube/Resources/KickLuckyCube/UI/StrengthTools";

        [MenuItem("Kick Lucky Cube/Assets/Rebuild Content Thumbnails")]
        public static void RebuildAll()
        {
            EnsureFolder(SkinIconFolder);
            EnsureFolder(ToolIconFolder);

            BuildWardrobeIcons();
            BuildStrengthToolIcons();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Kick Lucky Cube: rebuilt 23 wardrobe icons and 15 strength-tool icons from the authored models.");
        }

        public static void RebuildWardrobeIcons()
        {
            EnsureFolder(SkinIconFolder);
            BuildWardrobeIcons();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Kick Lucky Cube: rebuilt 23 wardrobe icons from the authored mannequin.");
        }

        private static void BuildWardrobeIcons()
        {
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerModelPath);
            if (playerPrefab == null)
            {
                throw new InvalidOperationException("The imported Blockbench player model is missing: " + PlayerModelPath);
            }

            foreach (var definition in KickLuckyCubeCharacterSkinCatalog.All
                         .Where(item => KickLuckyCubeCharacterSkinCatalog.RequiresWardrobeIcon(item.Slot)))
            {
                var female = definition.Id.Contains("female", StringComparison.OrdinalIgnoreCase);
                var host = new GameObject("KLC_IconCharacter_" + definition.Id)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };

                try
                {
                    var model = UnityEngine.Object.Instantiate(playerPrefab, host.transform);
                    model.name = playerPrefab.name;
                    model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, 180f, 0f));

                    var appearance = host.AddComponent<KickLuckyCubeCharacterAppearance>();
                    appearance.SetVisualRoot(model.transform);
                    var loadout = KickLuckyCubeCharacterSkinCatalog.CreateDefaultLoadout(female);
                    loadout.Set(definition.Slot, definition.Id);
                    appearance.ApplyLoadout(loadout);

                    var fullBounds = CalculateBounds(model.GetComponentsInChildren<Renderer>(true), requireVisible: true);
                    var bounds = FrameWardrobeSlot(fullBounds, definition.Slot);
                    var outputPath = SkinIconFolder + "/KLC_SkinItem_" + definition.Id + ".png";
                    RenderIcon(host.transform, bounds, outputPath, new Color(0.055f, 0.07f, 0.105f, 0f));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(host);
                }
            }
        }

        private static void BuildStrengthToolIcons()
        {
            var toolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ToolModelPath);
            if (toolPrefab == null)
            {
                throw new InvalidOperationException("The imported Blockbench strength-tool model is missing: " + ToolModelPath);
            }

            for (var tier = 1; tier <= 15; tier++)
            {
                var host = new GameObject($"KLC_IconTool_{tier:00}")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };

                try
                {
                    var model = UnityEngine.Object.Instantiate(toolPrefab, host.transform);
                    model.name = toolPrefab.name;
                    model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(12f, 205f, -5f));

                    var allTierRoots = model.GetComponentsInChildren<Transform>(true)
                        .Where(item => item.name.StartsWith("ToolTier_", StringComparison.Ordinal))
                        .ToArray();
                    var selectedRoot = allTierRoots.FirstOrDefault(
                        item => item.name.StartsWith($"ToolTier_{tier:00}_", StringComparison.Ordinal));
                    if (selectedRoot == null)
                    {
                        throw new InvalidOperationException($"Missing strength-tool tier root ToolTier_{tier:00}_*.");
                    }

                    foreach (var tierRoot in allTierRoots)
                    {
                        tierRoot.gameObject.SetActive(tierRoot == selectedRoot);
                    }

                    var bounds = CalculateBounds(selectedRoot.GetComponentsInChildren<Renderer>(true), requireVisible: false);
                    var outputPath = ToolIconFolder + $"/KLC_StrengthTool_{tier:00}.png";
                    RenderIcon(host.transform, bounds, outputPath, new Color(0.045f, 0.04f, 0.035f, 0f));
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(host);
                }
            }
        }

        private static void RenderIcon(Transform subject, Bounds bounds, string outputPath, Color background)
        {
            if (subject == null || bounds.size.sqrMagnitude <= 0.000001f)
            {
                throw new InvalidOperationException("Cannot render an icon without visible authored geometry: " + outputPath);
            }

            var cameraObject = new GameObject("KLC_IconCamera") { hideFlags = HideFlags.HideAndDontSave };
            var keyLightObject = new GameObject("KLC_IconKey") { hideFlags = HideFlags.HideAndDontSave };
            var fillLightObject = new GameObject("KLC_IconFill") { hideFlags = HideFlags.HideAndDontSave };
            var renderTexture = new RenderTexture(Resolution, Resolution, 24, RenderTextureFormat.ARGB32)
            {
                antiAliasing = 4,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var readback = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            var previousActive = RenderTexture.active;
            var previousAmbient = RenderSettings.ambientLight;
            var previousAmbientMode = RenderSettings.ambientMode;
            Camera camera = null;

            try
            {
                const int thumbnailLayer = 31;
                SetLayerRecursively(subject.gameObject, thumbnailLayer);
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.54f, 0.57f, 0.64f);

                camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = background;
                camera.orthographic = true;
                camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x) * 1.32f;
                camera.nearClipPlane = 0.01f;
                camera.farClipPlane = Mathf.Max(100f, bounds.size.magnitude * 12f);
                camera.allowHDR = false;
                camera.allowMSAA = true;
                camera.cullingMask = 1 << thumbnailLayer;
                camera.targetTexture = renderTexture;

                var viewDistance = Mathf.Max(5f, bounds.size.magnitude * 3.5f);
                camera.transform.position = bounds.center + Vector3.forward * viewDistance;
                camera.transform.LookAt(bounds.center, Vector3.up);

                ConfigureLight(keyLightObject, new Color(1f, 0.93f, 0.82f), 1.15f, new Vector3(32f, -28f, 0f), thumbnailLayer);
                ConfigureLight(fillLightObject, new Color(0.52f, 0.72f, 1f), 0.65f, new Vector3(25f, 145f, 0f), thumbnailLayer);

                camera.Render();
                RenderTexture.active = renderTexture;
                readback.ReadPixels(new Rect(0f, 0f, Resolution, Resolution), 0, 0);
                readback.Apply(false, false);
                File.WriteAllBytes(outputPath, readback.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previousActive;
                RenderSettings.ambientLight = previousAmbient;
                RenderSettings.ambientMode = previousAmbientMode;
                if (camera != null)
                {
                    camera.targetTexture = null;
                }

                UnityEngine.Object.DestroyImmediate(readback);
                UnityEngine.Object.DestroyImmediate(cameraObject);
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(keyLightObject);
                UnityEngine.Object.DestroyImmediate(fillLightObject);
            }

            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport);
            if (AssetImporter.GetAtPath(outputPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
            }
        }

        private static void ConfigureLight(GameObject lightObject, Color color, float intensity, Vector3 euler, int layer)
        {
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            light.cullingMask = 1 << layer;
            lightObject.transform.rotation = Quaternion.Euler(euler);
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private static Bounds CalculateBounds(Renderer[] renderers, bool requireVisible)
        {
            var found = false;
            var bounds = new Bounds();
            foreach (var renderer in renderers ?? Array.Empty<Renderer>())
            {
                if (renderer == null || (requireVisible && (!renderer.enabled || !renderer.gameObject.activeInHierarchy)))
                {
                    continue;
                }

                if (!found)
                {
                    bounds = renderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return found ? bounds : new Bounds();
        }

        private static Bounds FrameWardrobeSlot(Bounds fullBounds, KickLuckyCubeAppearanceSlot slot)
        {
            var heightFraction = 0.42f;
            var widthFraction = 0.9f;
            var centerFraction = 0.58f;
            switch (slot)
            {
                case KickLuckyCubeAppearanceSlot.Torso:
                    heightFraction = 0.44f;
                    widthFraction = 0.92f;
                    centerFraction = 0.64f;
                    break;
                case KickLuckyCubeAppearanceSlot.Legs:
                    heightFraction = 0.44f;
                    widthFraction = 0.76f;
                    centerFraction = 0.34f;
                    break;
                case KickLuckyCubeAppearanceSlot.Boots:
                    heightFraction = 0.27f;
                    widthFraction = 0.74f;
                    centerFraction = 0.12f;
                    break;
                case KickLuckyCubeAppearanceSlot.Gloves:
                    heightFraction = 0.48f;
                    widthFraction = 1f;
                    centerFraction = 0.57f;
                    break;
                case KickLuckyCubeAppearanceSlot.Hair:
                case KickLuckyCubeAppearanceSlot.Headwear:
                case KickLuckyCubeAppearanceSlot.Mask:
                    heightFraction = 0.32f;
                    widthFraction = 0.58f;
                    centerFraction = 0.84f;
                    break;
            }

            var framed = fullBounds;
            framed.center = new Vector3(
                fullBounds.center.x,
                fullBounds.min.y + fullBounds.size.y * centerFraction,
                fullBounds.center.z);
            framed.size = new Vector3(
                fullBounds.size.x * widthFraction,
                fullBounds.size.y * heightFraction,
                fullBounds.size.z);
            return framed;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            if (root == null || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            foreach (var item in root.GetComponentsInChildren<Transform>(true))
            {
                if (string.Equals(item.name, name, StringComparison.Ordinal))
                {
                    return item;
                }
            }

            return null;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            var name = Path.GetFileName(assetPath);
            if (string.IsNullOrWhiteSpace(parent) || string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Invalid asset folder path: " + assetPath);
            }

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
