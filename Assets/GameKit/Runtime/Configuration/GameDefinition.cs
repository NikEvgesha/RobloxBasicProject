using System;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.GameKit
{
    [CreateAssetMenu(fileName = "GameDefinition", menuName = "Roblox Basic Project/Game Definition")]
    public sealed class GameDefinition : ScriptableObject
    {
        [SerializeField] private string gameId = "game-id";
        [SerializeField] private string displayName = "Game";
        [SerializeField] private string version = "0.1.0";
        [SerializeField] private string startScenePath = string.Empty;
        [SerializeField] private List<string> buildScenePaths = new List<string>();
        [SerializeField] private string inputProfileId = "default";

#if ENABLE_INPUT_SYSTEM
        [SerializeField] private InputActionAsset inputActions;
#endif

        [SerializeField] private string cameraProfileId = "default";
        [SerializeField] private string uiThemeId = "default";
        [SerializeField] private List<string> enabledMechanics = new List<string>();
        [SerializeField] private WebGLBuildProfile webGL = new WebGLBuildProfile();
        [SerializeField] private MobileBuildProfile mobile = new MobileBuildProfile();

        public string GameId => gameId;
        public string DisplayName => displayName;
        public string Version => version;
        public string StartScenePath => startScenePath;
        public IReadOnlyList<string> BuildScenePaths => buildScenePaths;
        public string InputProfileId => inputProfileId;

#if ENABLE_INPUT_SYSTEM
        public InputActionAsset InputActions => inputActions;
#endif

        public string CameraProfileId => cameraProfileId;
        public string UiThemeId => uiThemeId;
        public IReadOnlyList<string> EnabledMechanics => enabledMechanics;
        public WebGLBuildProfile WebGL => webGL;
        public MobileBuildProfile Mobile => mobile;

        public bool ContainsBuildScene(string scenePath)
        {
            return !string.IsNullOrWhiteSpace(scenePath) && buildScenePaths.Contains(scenePath);
        }
    }

    [Serializable]
    public sealed class WebGLBuildProfile
    {
        [SerializeField] private string buildDirectory = "Builds/WebGL";
        [SerializeField] private string template = "APPLICATION:Default";
        [SerializeField] private string compressionFormat = "Disabled";
        [SerializeField] private bool dataCaching = true;
        [SerializeField] private bool developmentBuild;
        [SerializeField] private int targetWidth = 960;
        [SerializeField] private int targetHeight = 600;
        [SerializeField] private string memoryGrowthMode = "Geometric";

        public string BuildDirectory => buildDirectory;
        public string Template => template;
        public string CompressionFormat => compressionFormat;
        public bool DataCaching => dataCaching;
        public bool DevelopmentBuild => developmentBuild;
        public int TargetWidth => targetWidth;
        public int TargetHeight => targetHeight;
        public string MemoryGrowthMode => memoryGrowthMode;
    }

    [Serializable]
    public sealed class MobileBuildProfile
    {
        [SerializeField] private bool supportsTouch = true;
        [SerializeField] private string defaultOrientation = "Landscape";
        [SerializeField] private bool requiresVirtualJoystick = true;
        [SerializeField] private string notes = "Prototype mobile support through the shared input profile.";

        public bool SupportsTouch => supportsTouch;
        public string DefaultOrientation => defaultOrientation;
        public bool RequiresVirtualJoystick => requiresVirtualJoystick;
        public string Notes => notes;
    }
}
