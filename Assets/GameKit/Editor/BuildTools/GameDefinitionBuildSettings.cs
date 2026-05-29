#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RobloxBasicProject.GameKit.Editor
{
    public static class GameDefinitionBuildSettings
    {
        private const string ApplyMenuPath = "Roblox Basic Project/Game Definition/Apply Selected To Build Settings";

        [MenuItem(ApplyMenuPath, true)]
        private static bool CanApplySelected()
        {
            return Selection.activeObject is GameDefinition;
        }

        [MenuItem(ApplyMenuPath)]
        private static void ApplySelected()
        {
            Apply((GameDefinition)Selection.activeObject);
        }

        public static void Apply(GameDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definition.BuildScenePaths.Count == 0)
            {
                throw new InvalidOperationException($"{definition.name} does not define any build scenes.");
            }

            var scenes = new List<EditorBuildSettingsScene>();

            foreach (var scenePath in definition.BuildScenePaths)
            {
                if (string.IsNullOrWhiteSpace(scenePath))
                {
                    continue;
                }

                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (sceneAsset == null)
                {
                    throw new InvalidOperationException($"Build scene does not exist: {scenePath}");
                }

                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            if (scenes.Count == 0)
            {
                throw new InvalidOperationException($"{definition.name} does not contain valid scene paths.");
            }

            if (!definition.ContainsBuildScene(definition.StartScenePath))
            {
                Debug.LogWarning($"{definition.name} start scene is not included in build scenes: {definition.StartScenePath}", definition);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log($"Applied build settings for {definition.DisplayName}. Scenes: {scenes.Count}", definition);
        }
    }
}
#endif
