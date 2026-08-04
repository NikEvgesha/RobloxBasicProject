using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube.Editor
{
    public static class KickLuckyCubeBlockbenchPlayerImporter
    {
        private const string SourcePath = "Assets/Games/KickLuckyCube/Art/KLC_PlayerMannequin.bbmodel";
        private const string ModelPath = "Assets/Games/KickLuckyCube/Resources/KickLuckyCube/KLC_PlayerMannequin.fbx";
        private const string ControllerPath = "Assets/Games/KickLuckyCube/Resources/KickLuckyCube/KLC_PlayerMannequinAnimator.controller";
        private const string GeneratedRoot = "Assets/Games/KickLuckyCube/Art/BlockbenchPlayer";
        private const string TextureFolder = GeneratedRoot + "/Textures";
        private const string MaterialFolder = GeneratedRoot + "/Materials";

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

        [MenuItem("Kick Lucky Cube/Assets/Rebuild Blockbench Player")]
        public static void Rebuild()
        {
            EnsureFolder(GeneratedRoot);
            EnsureFolder(TextureFolder);
            EnsureFolder(MaterialFolder);

            var project = ReadSourceProject();
            var materialNames = ExtractTexturesAndBuildMaterials(project);
            ConfigureModelImporter(materialNames);
            BuildAnimatorController();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Kick Lucky Cube: rebuilt Blockbench mannequin, materials and animator controller.");
        }

        private static BlockbenchProject ReadSourceProject()
        {
            if (!File.Exists(SourcePath))
            {
                throw new FileNotFoundException("Blockbench player source was not found.", SourcePath);
            }

            var project = JsonUtility.FromJson<BlockbenchProject>(File.ReadAllText(SourcePath));
            if (project == null || project.textures == null || project.textures.Length == 0)
            {
                throw new InvalidOperationException("Blockbench player source contains no embedded textures.");
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
                throw new InvalidOperationException("The exported Blockbench FBX could not be imported as a model.");
            }

            importer.importAnimation = true;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importCameras = false;
            importer.importLights = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;

            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.loopTime = clip.name.Contains("Idle", StringComparison.OrdinalIgnoreCase)
                    || clip.name.Contains("Walk", StringComparison.OrdinalIgnoreCase)
                    || clip.name.Contains("Squat", StringComparison.OrdinalIgnoreCase)
                    || clip.name.Contains("Lunge", StringComparison.OrdinalIgnoreCase);
                clip.loopPose = clip.loopTime;
            }

            importer.clipAnimations = clips;
            foreach (var materialName in materialNames)
            {
                var materialPath = MaterialFolder + "/" + SanitizeFileName(materialName) + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    continue;
                }

                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), materialName), material);
            }

            importer.SaveAndReimport();
        }

        private static void BuildAnimatorController()
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(ModelPath)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                .ToArray();
            var idleClip = FindClip(clips, "KLC_Idle");
            var walkClip = FindClip(clips, "KLC_Walk");
            var kickClip = FindClip(clips, "KLC_Kick_Classic");

            if (idleClip == null || walkClip == null || kickClip == null)
            {
                throw new InvalidOperationException("The Blockbench FBX must contain Idle, Walk and Classic Kick clips.");
            }

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            {
                AssetDatabase.DeleteAsset(ControllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var stateMachine = controller.layers[0].stateMachine;

            var idle = stateMachine.AddState("Idle");
            idle.motion = idleClip;
            stateMachine.defaultState = idle;

            var walk = stateMachine.AddState("Walk");
            walk.motion = walkClip;

            var kick = stateMachine.AddState("Kick");
            kick.motion = kickClip;

            var toWalk = idle.AddTransition(walk);
            toWalk.hasExitTime = false;
            toWalk.duration = 0.12f;
            toWalk.AddCondition(AnimatorConditionMode.Greater, 0.08f, "Speed");

            var toIdle = walk.AddTransition(idle);
            toIdle.hasExitTime = false;
            toIdle.duration = 0.12f;
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.08f, "Speed");

            var kickToIdle = kick.AddTransition(idle);
            kickToIdle.hasExitTime = true;
            kickToIdle.exitTime = 0.98f;
            kickToIdle.duration = 0.08f;

            EditorUtility.SetDirty(controller);
        }

        private static AnimationClip FindClip(AnimationClip[] clips, string suffix)
        {
            return clips.FirstOrDefault(clip => clip.name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
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
