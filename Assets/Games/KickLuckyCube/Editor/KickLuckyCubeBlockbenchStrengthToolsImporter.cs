using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube.Editor
{
    public static class KickLuckyCubeBlockbenchStrengthToolsImporter
    {
        private const string SourcePath = "Assets/Games/KickLuckyCube/Art/KLC_StrengthTools.bbmodel";
        private const string ModelPath = "Assets/Games/KickLuckyCube/Resources/KickLuckyCube/KLC_StrengthTools.fbx";
        private const string GeneratedRoot = "Assets/Games/KickLuckyCube/Art/BlockbenchStrengthTools";
        private const string TextureFolder = GeneratedRoot + "/Textures";
        private const string MaterialFolder = GeneratedRoot + "/Materials";
        private const int ExpectedTierCount = 15;

        [Serializable]
        private sealed class BlockbenchProject
        {
            public BlockbenchTexture[] textures = Array.Empty<BlockbenchTexture>();
        }

        [Serializable]
        private sealed class BlockbenchTexture
        {
            public string name;
            public string source;
        }

        [MenuItem("Kick Lucky Cube/Assets/Rebuild Blockbench Strength Tools")]
        public static void Rebuild()
        {
            EnsureFolder(GeneratedRoot);
            EnsureFolder(TextureFolder);
            EnsureFolder(MaterialFolder);

            var project = ReadSourceProject();
            var materialNames = ExtractTexturesAndBuildMaterials(project);
            ConfigureModelImporter(materialNames);
            ValidateTierRoots();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Kick Lucky Cube: rebuilt all 15 Blockbench strength-tool tiers and materials.");
        }

        private static BlockbenchProject ReadSourceProject()
        {
            if (!File.Exists(SourcePath))
            {
                throw new FileNotFoundException("Blockbench strength-tool source was not found.", SourcePath);
            }

            var project = JsonUtility.FromJson<BlockbenchProject>(File.ReadAllText(SourcePath));
            if (project == null || project.textures == null || project.textures.Length < ExpectedTierCount + 1)
            {
                throw new InvalidOperationException(
                    "The strength-tool source must contain the shared grip texture and 15 tier textures.");
            }

            return project;
        }

        private static string[] ExtractTexturesAndBuildMaterials(BlockbenchProject project)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null)
            {
                throw new InvalidOperationException("No compatible lit shader is available.");
            }

            foreach (var entry in project.textures)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.name) || string.IsNullOrWhiteSpace(entry.source))
                {
                    continue;
                }

                var marker = entry.source.IndexOf("base64,", StringComparison.Ordinal);
                if (marker < 0)
                {
                    continue;
                }

                var safeName = SanitizeFileName(entry.name);
                var texturePath = TextureFolder + "/" + safeName + ".png";
                File.WriteAllBytes(texturePath, Convert.FromBase64String(entry.source[(marker + 7)..]));
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceSynchronousImport);

                if (AssetImporter.GetAtPath(texturePath) is TextureImporter textureImporter)
                {
                    textureImporter.textureType = TextureImporterType.Default;
                    textureImporter.alphaIsTransparency = true;
                    textureImporter.mipmapEnabled = false;
                    textureImporter.filterMode = FilterMode.Point;
                    textureImporter.wrapMode = TextureWrapMode.Clamp;
                    textureImporter.SaveAndReimport();
                }

                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                var materialPath = MaterialFolder + "/" + safeName + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(shader) { name = entry.name };
                    AssetDatabase.CreateAsset(material, materialPath);
                }
                else
                {
                    material.shader = shader;
                }

                material.mainTexture = texture;
                material.color = Color.white;
                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }

                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", Color.white);
                }

                if (entry.name.Contains("Neon", StringComparison.OrdinalIgnoreCase)
                    || entry.name.Contains("Arcane", StringComparison.OrdinalIgnoreCase))
                {
                    material.EnableKeyword("_EMISSION");
                    var emission = texture != null ? Color.white * 0.45f : Color.black;
                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.SetColor("_EmissionColor", emission);
                    }
                }

                EditorUtility.SetDirty(material);
            }

            return project.textures
                .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.name))
                .Select(entry => entry.name)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        private static void ConfigureModelImporter(string[] materialNames)
        {
            AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceSynchronousImport);
            if (AssetImporter.GetAtPath(ModelPath) is not ModelImporter importer)
            {
                throw new InvalidOperationException("The exported Blockbench strength tools could not be imported as a model.");
            }

            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;

            foreach (var materialName in materialNames)
            {
                var materialPath = MaterialFolder + "/" + SanitizeFileName(materialName) + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material != null)
                {
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), materialName), material);
                }
            }

            importer.SaveAndReimport();
        }

        private static void ValidateTierRoots()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (prefab == null)
            {
                throw new InvalidOperationException("The strength-tool FBX prefab is missing after import.");
            }

            var names = prefab.GetComponentsInChildren<Transform>(true)
                .Select(item => item.name)
                .ToArray();
            for (var tier = 1; tier <= ExpectedTierCount; tier++)
            {
                var prefix = $"ToolTier_{tier:00}_";
                if (!names.Any(name => name.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    throw new InvalidOperationException("Missing authored strength-tool tier root: " + prefix);
                }
            }
        }

        private static string SanitizeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return new string((value ?? string.Empty)
                .Select(character => invalid.Contains(character) ? '_' : character)
                .ToArray());
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
