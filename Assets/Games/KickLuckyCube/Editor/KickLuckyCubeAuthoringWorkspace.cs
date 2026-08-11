using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RobloxBasicProject.Games.KickLuckyCube;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace RobloxBasicProject.Games.KickLuckyCube.Editor
{
    public sealed class KickLuckyCubeAuthoringWorkspace : EditorWindow
    {
        private const string GameRoot = "Assets/Games/KickLuckyCube";
        private const string ScenePath = GameRoot + "/Scenes/KickLuckyCubeOverview.unity";
        private const string CameraConfigPath = GameRoot + "/Resources/KickLuckyCube/KickLuckyCubeCameraChoreographyConfig.asset";
        private const string LuckyCubePath = GameRoot + "/Prefabs/KLC_LuckyCube.prefab";
        private const string CorridorDataRootName = "KLC_CorridorGameplayData_DoNotDelete";
        private const string LegacyCorridorRootName = "KLC_BiomeBlockout_30Locations";
        private const string CorridorZonesRootName = "GameplayZones";
        private const string CorridorPreviewRootName = "AuthoringBiomeInstances";
        private const string FirstLocationAnchorName = "FirstLocationAnchor";
        private const string KickLineName = "MOVE_KickLine_YellowBar";
        private const string ExtendedCorridorFloorName = "KLC_ExtendedCorridorFloor_30Zones";
        private const string BaseFloorName = "Floor_FlatGreenGrass";
        private const float ExtendedCorridorFloorStartZ = 0f;
        private const float ExtendedCorridorFloorWidth = 78f;
        private const float ExtendedCorridorFloorThickness = 0.5f;
        private const string BiomeFolder = GameRoot + "/Prefabs/World/Biomes";
        private const string ShopFolder = GameRoot + "/Prefabs/World/Shops";
        private const string BaseFolder = GameRoot + "/Prefabs/World/PlayerBase";
        private const string TrainingFolder = GameRoot + "/Prefabs/Training";
        private const string UiFolder = GameRoot + "/Resources/KickLuckyCube/UI/Elements";

        private static readonly (string Label, string Path)[] UiWindows =
        {
            ("Speed Shop", UiFolder + "/KLC_SpeedShopWindow_Runtime.prefab"),
            ("Training Tool Shop", UiFolder + "/KLC_StrengthToolShopWindow_Runtime.prefab"),
            ("Exchange", UiFolder + "/KLC_ExchangeWindow_Runtime.prefab"),
            ("Elite Mob Shop", UiFolder + "/KLC_EpicMobShopWindow_Runtime.prefab"),
            ("Weather", UiFolder + "/KLC_WeatherMachineWindow_Runtime.prefab"),
            ("Offline Reward", UiFolder + "/KLC_OfflineRewardWindow_Runtime.prefab"),
            ("Sell Shop", UiFolder + "/KLC_SellShopWindow_Runtime.prefab"),
            ("Style Shop", UiFolder + "/KLC_StyleShopWindow_Runtime.prefab"),
        };

        private Vector2 scroll;
        private bool showCamera = true;
        private bool showWorld = true;
        private bool showUi = true;
        private bool showAnimation = true;

        [MenuItem("Tools/Kick Lucky Cube/Authoring Workspace", priority = 1)]
        public static void Open()
        {
            var window = GetWindow<KickLuckyCubeAuthoringWorkspace>("KLC Authoring");
            window.minSize = new Vector2(470f, 620f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Kick Lucky Cube Authoring Workspace", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Edit one prefab at a time. The workspace creates contracts and previews; it does not rebuild the whole scene.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Install / Repair Tools", GUILayout.Height(28f))) InstallCoreAuthoringContracts();
                if (GUILayout.Button("Validate All", GUILayout.Height(28f))) ValidateAll(true);
                if (GUILayout.Button("Open Overview", GUILayout.Height(28f))) OpenOverviewScene();
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            DrawCameraSection();
            DrawWorldSection();
            DrawUiSection();
            DrawAnimationSection();
            EditorGUILayout.EndScrollView();
        }

        private void DrawCameraSection()
        {
            showCamera = EditorGUILayout.BeginFoldoutHeaderGroup(showCamera, "Camera choreography (KLC-PROD-003)");
            if (showCamera)
            {
                DrawAssetRow("Choreography config", CameraConfigPath);
                EditorGUILayout.LabelField("Select the wave or runner target in the Hierarchy, then preview a shot:");
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUI.enabled = Selection.activeTransform != null;
                    if (GUILayout.Button("Preview Wave Shot")) PreviewCameraShot(true);
                    if (GUILayout.Button("Preview Runner Shot")) PreviewCameraShot(false);
                    GUI.enabled = true;
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawWorldSection()
        {
            showWorld = EditorGUILayout.BeginFoldoutHeaderGroup(showWorld, "World prefabs and contracts");
            if (showWorld)
            {
                DrawPrefabCreationRow("Biome", typeof(KickLuckyCubeBiomeAuthoring), BiomeFolder);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Corridor sequence", GUILayout.Width(175f));
                    GUI.enabled = Selection.activeGameObject != null;
                    if (GUILayout.Button("Add / Auto-bind")) AddContractToSelection(typeof(KickLuckyCubeCorridorAuthoring));
                    if (GUILayout.Button("Build Non-destructive Preview")) RebuildCorridorPreview();
                    GUI.enabled = true;
                }
                DrawPrefabCreationRow("World shop", typeof(KickLuckyCubeWorldShopAuthoring), ShopFolder);
                DrawPrefabCreationRow("Player base", typeof(KickLuckyCubePlayerBaseAuthoring), BaseFolder);
                DrawAssetRow("Lucky Cube", LuckyCubePath);
                DrawAssetRow("Home marker", UiFolder + "/KLC_PlayerHomeIcon_Runtime.prefab");
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select Player Base")) SelectSceneObject<KickLuckyCubePlotTemplate>();
                    if (GUILayout.Button("Select Home Marker")) SelectSceneObject<KickLuckyCubeHomeIconMarker>();
                    if (GUILayout.Button("Select Wave")) SelectSceneObject<KickLuckyCubeWaveChaseController>();
                }
                EditorGUILayout.HelpBox(
                    "To convert scene art: select exactly one complete root, add/auto-bind its contract, then create a connected prefab. Undo is supported.",
                    MessageType.None);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawUiSection()
        {
            showUi = EditorGUILayout.BeginFoldoutHeaderGroup(showUi, "UI windows and preview states");
            if (showUi)
            {
                foreach (var item in UiWindows) DrawAssetRow(item.Label, item.Path);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add Preview To Selected Root")) AddContractToSelection(typeof(KickLuckyCubeUiAuthoringPreview));
                    if (GUILayout.Button("Apply Selected Preview"))
                    {
                        var preview = Selection.activeGameObject != null
                            ? Selection.activeGameObject.GetComponent<KickLuckyCubeUiAuthoringPreview>()
                            : null;
                        preview?.ApplyPreview();
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("Fill Sample Values"))
                    {
                        var preview = Selection.activeGameObject != null
                            ? Selection.activeGameObject.GetComponent<KickLuckyCubeUiAuthoringPreview>()
                            : null;
                        preview?.ApplyHeuristicSampleText();
                        if (preview != null) EditorUtility.SetDirty(preview.gameObject);
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawAnimationSection()
        {
            showAnimation = EditorGUILayout.BeginFoldoutHeaderGroup(showAnimation, "Training animation authoring (KLC-PROD-022)");
            if (showAnimation)
            {
                DrawAssetRow("Tool hand preview", GameRoot + "/Resources/KickLuckyCube/World/KLC_StrengthToolHandPreview.prefab");
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add Rig Contract To Selection")) AddContractToSelection(typeof(KickLuckyCubeTrainingAnimationAuthoring));
                    if (GUILayout.Button("Sample Selected Clip")) SampleSelectedTrainingClip();
                    if (GUILayout.Button("Stop Animation Preview")) AnimationMode.StopAnimationMode();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private static void DrawAssetRow(string label, string path)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(175f));
                EditorGUILayout.LabelField(asset != null ? "Ready" : "Missing", GUILayout.Width(65f));
                GUI.enabled = asset != null;
                if (GUILayout.Button("Open", GUILayout.Width(62f))) AssetDatabase.OpenAsset(asset);
                if (GUILayout.Button("Ping", GUILayout.Width(62f))) EditorGUIUtility.PingObject(asset);
                GUI.enabled = true;
            }
        }

        private static void DrawPrefabCreationRow(string label, Type contractType, string folder)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(175f));
                GUI.enabled = Selection.activeGameObject != null;
                if (GUILayout.Button("Add / Auto-bind")) AddContractToSelection(contractType);
                if (GUILayout.Button("Create Connected Prefab")) CreatePrefabFromSelection(contractType, folder);
                GUI.enabled = true;
            }
        }

        [MenuItem("Tools/Kick Lucky Cube/Authoring/Install Or Repair Core Contracts")]
        public static void InstallCoreAuthoringContracts()
        {
            EnsureFolders();
            EnsureCameraConfig();
            InstallLuckyCubeContract();
            InstallUiPreviewContracts();
            BindCameraConfigInOpenScene();
            InstallOpenSceneAuthoringContracts();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[KLC-AUTHORING] Core authoring tools installed or repaired.");
        }

        [MenuItem("Tools/Kick Lucky Cube/Validation/Validate Authoring Contracts")]
        public static void ValidateAllFromMenu()
        {
            ValidateAll(true);
        }

        public static int ValidateAll(bool logResults)
        {
            var issues = new List<string>();
            if (AssetDatabase.LoadAssetAtPath<KickLuckyCubeCameraChoreographyConfig>(CameraConfigPath) == null)
                issues.Add("Camera choreography config is missing.");

            ValidatePrefabContracts<KickLuckyCubeBiomeAuthoring>(issues, BiomeFolder, c => c.CollectValidationIssues(issues));
            ValidatePrefabContracts<KickLuckyCubeWorldShopAuthoring>(issues, ShopFolder, c => c.CollectValidationIssues(issues));
            ValidatePrefabContracts<KickLuckyCubePlayerBaseAuthoring>(issues, BaseFolder, c => c.CollectValidationIssues(issues));

            var cube = AssetDatabase.LoadAssetAtPath<GameObject>(LuckyCubePath);
            var cubeContract = cube != null ? cube.GetComponent<KickLuckyCubeLuckyCubeAuthoring>() : null;
            if (cubeContract == null) issues.Add("Lucky Cube authoring contract is missing.");
            else cubeContract.CollectValidationIssues(issues);

            foreach (var item in UiWindows)
            {
                var window = AssetDatabase.LoadAssetAtPath<GameObject>(item.Path);
                if (window == null) issues.Add($"UI prefab is missing: {item.Path}");
                else if (window.GetComponent<KickLuckyCubeUiAuthoringPreview>() == null)
                    issues.Add($"{item.Path}: UI authoring preview component is missing.");
            }

            var corridors = FindObjectsByType<KickLuckyCubeCorridorAuthoring>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            if (corridors.Length == 0)
                issues.Add($"Required corridor data root '{CorridorDataRootName}' is missing.");
            foreach (var corridor in corridors)
                corridor.CollectValidationIssues(issues);

            var gameplayZones = FindObjectsByType<KickLuckyCubeRarityZone>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(zone => zone != null
                    && zone.name.StartsWith("DATA_RarityZone_", StringComparison.Ordinal))
                .ToArray();
            if (gameplayZones.Length != KickLuckyCubeCorridorLayout.LocationCount)
                issues.Add(
                    $"Corridor gameplay zones: expected {KickLuckyCubeCorridorLayout.LocationCount}, "
                    + $"found {gameplayZones.Length}.");

            if (FindFirstObjectByType<KickLuckyCubePlayerKickBoundary>(FindObjectsInactive.Include) == null)
                issues.Add($"Player kick boundary is missing from '{KickLineName}'.");

            var extendedFloor = FindSceneObject(ExtendedCorridorFloorName);
            var extendedFloorCollider = extendedFloor != null
                ? extendedFloor.GetComponentInChildren<Collider>(true)
                : null;
            if (extendedFloor == null)
                issues.Add($"Required corridor gameplay floor '{ExtendedCorridorFloorName}' is missing.");
            else if (extendedFloorCollider == null || !extendedFloorCollider.enabled || extendedFloorCollider.isTrigger)
                issues.Add($"{ExtendedCorridorFloorName}: an enabled non-trigger collider is required.");

            foreach (var shop in FindObjectsByType<KickLuckyCubeWorldShopAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                shop.CollectValidationIssues(issues);
            foreach (var playerBase in FindObjectsByType<KickLuckyCubePlayerBaseAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                playerBase.CollectValidationIssues(issues);
            foreach (var training in FindObjectsByType<KickLuckyCubeTrainingAnimationAuthoring>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                training.CollectValidationIssues(issues);

            if (logResults)
            {
                if (issues.Count == 0) Debug.Log("[KLC-AUTHORING] Validation passed with 0 contract issues.");
                else foreach (var issue in issues) Debug.LogWarning("[KLC-AUTHORING] " + issue);
                Debug.Log($"[KLC-AUTHORING] Validation complete. Issues={issues.Count}.");
            }
            return issues.Count;
        }

        private static void ValidatePrefabContracts<T>(List<string> issues, string folder, Action<T> validate)
            where T : Component
        {
            if (!AssetDatabase.IsValidFolder(folder)) return;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { folder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var contract = prefab != null ? prefab.GetComponent<T>() : null;
                if (contract == null) issues.Add($"{path}: missing {typeof(T).Name}.");
                else validate(contract);
            }
        }

        private static void EnsureFolders()
        {
            EnsureFolder(GameRoot + "/Prefabs", "World");
            EnsureFolder(GameRoot + "/Prefabs/World", "Biomes");
            EnsureFolder(GameRoot + "/Prefabs/World", "Shops");
            EnsureFolder(GameRoot + "/Prefabs/World", "PlayerBase");
            EnsureFolder(GameRoot + "/Prefabs", "Training");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }

        private static void EnsureCameraConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<KickLuckyCubeCameraChoreographyConfig>(CameraConfigPath) != null) return;
            var config = CreateInstance<KickLuckyCubeCameraChoreographyConfig>();
            var serialized = new SerializedObject(config);
            ConfigureShot(serialized.FindProperty("waveReveal"), "Wave Reveal", 8.5f, 9f, 0, 0f, 1.15f, 0.45f, 54f);
            ConfigureShot(serialized.FindProperty("runnerReady"), "Runner Ready", 6.2f, 14f, 2, 0f, 0.9f, 0.3f, 60f);
            serialized.FindProperty("revealToWaveDelay").floatValue = 0.12f;
            serialized.FindProperty("runnerGroundingPause").floatValue = 0.08f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(config, CameraConfigPath);
        }

        private static void ConfigureShot(
            SerializedProperty shot,
            string displayName,
            float distance,
            float pitch,
            int yawBasis,
            float yawOffset,
            float transition,
            float hold,
            float fov)
        {
            shot.FindPropertyRelative("displayName").stringValue = displayName;
            shot.FindPropertyRelative("distance").floatValue = distance;
            shot.FindPropertyRelative("pitch").floatValue = pitch;
            shot.FindPropertyRelative("yawBasis").enumValueIndex = yawBasis;
            shot.FindPropertyRelative("yawOffset").floatValue = yawOffset;
            shot.FindPropertyRelative("transitionSeconds").floatValue = transition;
            shot.FindPropertyRelative("holdSeconds").floatValue = hold;
            shot.FindPropertyRelative("fieldOfView").floatValue = fov;
        }

        private static void InstallLuckyCubeContract()
        {
            var root = PrefabUtility.LoadPrefabContents(LuckyCubePath);
            try
            {
                var contract = root.GetComponent<KickLuckyCubeLuckyCubeAuthoring>()
                    ?? root.AddComponent<KickLuckyCubeLuckyCubeAuthoring>();
                EnsureAnchor(root.transform, "CarryAnchor", new Vector3(0f, 0.65f, 0f));
                EnsureAnchor(root.transform, "TrailAnchor", new Vector3(0f, 0f, -0.55f));
                EnsureAnchor(root.transform, "ImpactVfxAnchor", new Vector3(0f, -0.52f, 0f));
                contract.AutoBind();
                EditorUtility.SetDirty(contract);
                PrefabUtility.SaveAsPrefabAsset(root, LuckyCubePath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static Transform EnsureAnchor(Transform parent, string name, Vector3 localPosition)
        {
            var child = parent.Find(name);
            if (child != null) return child;
            var anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = localPosition;
            return anchor;
        }

        private static void InstallUiPreviewContracts()
        {
            foreach (var item in UiWindows)
            {
                if (!File.Exists(item.Path)) continue;
                var root = PrefabUtility.LoadPrefabContents(item.Path);
                try
                {
                    if (root.GetComponent<KickLuckyCubeUiAuthoringPreview>() == null)
                        root.AddComponent<KickLuckyCubeUiAuthoringPreview>();
                    PrefabUtility.SaveAsPrefabAsset(root, item.Path);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }

        private static void BindCameraConfigInOpenScene()
        {
            var config = AssetDatabase.LoadAssetAtPath<KickLuckyCubeCameraChoreographyConfig>(CameraConfigPath);
            var runPhase = FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            if (config == null || runPhase == null) return;
            var serialized = new SerializedObject(runPhase);
            serialized.FindProperty("cameraChoreography").objectReferenceValue = config;
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(runPhase.gameObject.scene);
        }

        private static void InstallOpenSceneAuthoringContracts()
        {
            InstallCorridorGameplayData();
            InstallExtendedCorridorFloor();
            InstallPlayerKickBoundary();
            RepairLegacyEconomyHudReference();

            var plotTemplate = FindFirstObjectByType<KickLuckyCubePlotTemplate>(FindObjectsInactive.Include);
            if (plotTemplate != null)
            {
                EnsureAnchor(plotTemplate.transform, "HomeMarkerAnchor", new Vector3(0f, 5f, 0f));
                EnsureAnchor(plotTemplate.transform, "OwnerLabelAnchor", new Vector3(0f, 3.2f, 0f));
                EnsureAnchor(plotTemplate.transform, "InteractionRoot", Vector3.zero);
                EnsureAnchor(plotTemplate.transform, "DecorationRoot", Vector3.zero);
                var playerBase = plotTemplate.GetComponent<KickLuckyCubePlayerBaseAuthoring>()
                    ?? plotTemplate.gameObject.AddComponent<KickLuckyCubePlayerBaseAuthoring>();
                playerBase.AutoBind();
                EditorUtility.SetDirty(playerBase);
            }

            var player = FindSceneObject("KLC_PrototypePlayer");
            if (player != null)
            {
                var training = player.GetComponent<KickLuckyCubeTrainingAnimationAuthoring>()
                    ?? player.AddComponent<KickLuckyCubeTrainingAnimationAuthoring>();
                training.AutoBind();
                EditorUtility.SetDirty(training);
            }

            InstallWorldShopContract("KLC_Kiosk_01_SellAnimals", KickLuckyCubeWorldShopAuthoring.ShopKind.SellAnimals, UiFolder + "/KLC_SellShopWindow_Runtime.prefab");
            InstallWorldShopContract("KLC_Kiosk_02_StyleShop", KickLuckyCubeWorldShopAuthoring.ShopKind.Styles, UiFolder + "/KLC_StyleShopWindow_Runtime.prefab");
            InstallWorldShopContract("KLC_Kiosk_03_SpeedUpgrade", KickLuckyCubeWorldShopAuthoring.ShopKind.Speed, UiFolder + "/KLC_SpeedShopWindow_Runtime.prefab");
            InstallWorldShopContract("KLC_Kiosk_04_WeightsTraining", KickLuckyCubeWorldShopAuthoring.ShopKind.TrainingTools, UiFolder + "/KLC_StrengthToolShopWindow_Runtime.prefab");
            InstallWorldShopContract("KLC_Future_ExchangeBooth", KickLuckyCubeWorldShopAuthoring.ShopKind.Exchange, UiFolder + "/KLC_ExchangeWindow_Runtime.prefab");
            InstallWorldShopContract("KLC_Future_EpicMobShop", KickLuckyCubeWorldShopAuthoring.ShopKind.EliteMobs, UiFolder + "/KLC_EpicMobShopWindow_Runtime.prefab");
            InstallWorldShopContract("KLC_Future_WeatherMachine", KickLuckyCubeWorldShopAuthoring.ShopKind.Weather, UiFolder + "/KLC_WeatherMachineWindow_Runtime.prefab");
            InstallWorldShopContract("KLC_Future_RatingGiftStand", KickLuckyCubeWorldShopAuthoring.ShopKind.Rewards, UiFolder + "/KLC_OfflineRewardWindow_Runtime.prefab");
        }

        private static void InstallCorridorGameplayData()
        {
            var root = FindSceneObject(CorridorDataRootName) ?? FindSceneObject(LegacyCorridorRootName);
            if (root == null)
            {
                root = new GameObject(CorridorDataRootName);
                var workspace = FindSceneObject("KLC_LayoutBlockout_Workspace");
                if (workspace != null) root.transform.SetParent(workspace.transform, false);
                root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                root.transform.localScale = Vector3.one;
            }
            else if (root.name == LegacyCorridorRootName)
            {
                root.name = CorridorDataRootName;
            }

            var firstLocationAnchor = EnsureAnchor(
                root.transform,
                FirstLocationAnchorName,
                new Vector3(0f, 0f, KickLuckyCubeCorridorLayout.FirstLocationStart));
            var previewRoot = EnsureAnchor(root.transform, CorridorPreviewRootName, Vector3.zero);
            var zonesRoot = EnsureAnchor(root.transform, CorridorZonesRootName, Vector3.zero);
            zonesRoot.gameObject.layer = 2;

            var zones = new KickLuckyCubeRarityZone[KickLuckyCubeCorridorLayout.LocationCount];
            for (var index = 0; index < zones.Length; index++)
            {
                var zoneName = $"DATA_RarityZone_{index + 1:00}";
                var zoneTransform = zonesRoot.Find(zoneName);
                if (zoneTransform == null)
                {
                    zoneTransform = new GameObject(zoneName).transform;
                    zoneTransform.SetParent(zonesRoot, false);
                }

                zoneTransform.gameObject.layer = 2;
                zoneTransform.localPosition = new Vector3(
                    0f,
                    2.5f,
                    KickLuckyCubeCorridorLayout.FirstLocationStart
                    + index * KickLuckyCubeCorridorLayout.LocationSpacing
                    + KickLuckyCubeCorridorLayout.LocationPlayableLength * 0.5f);
                zoneTransform.localRotation = Quaternion.identity;
                zoneTransform.localScale = Vector3.one;

                var trigger = zoneTransform.GetComponent<BoxCollider>();
                if (trigger == null) trigger = zoneTransform.gameObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = Vector3.zero;
                trigger.size = new Vector3(120f, 5f, KickLuckyCubeCorridorLayout.LocationPlayableLength);

                var zone = zoneTransform.GetComponent<KickLuckyCubeRarityZone>();
                if (zone == null) zone = zoneTransform.gameObject.AddComponent<KickLuckyCubeRarityZone>();
                ConfigureGameplayZone(zone, index + 1);
                zones[index] = zone;
            }

            var corridor = root.GetComponent<KickLuckyCubeCorridorAuthoring>()
                ?? root.AddComponent<KickLuckyCubeCorridorAuthoring>();
            var corridorSerialized = new SerializedObject(corridor);
            corridorSerialized.FindProperty("firstLocationAnchor").objectReferenceValue = firstLocationAnchor;
            corridorSerialized.FindProperty("previewRoot").objectReferenceValue = previewRoot;
            corridorSerialized.ApplyModifiedPropertiesWithoutUndo();
            corridor.AutoBind();
            EditorUtility.SetDirty(corridor);

            var kickController = FindFirstObjectByType<KickLuckyCubeKickController>(FindObjectsInactive.Include);
            if (kickController != null)
            {
                var kickSerialized = new SerializedObject(kickController);
                var zoneArray = kickSerialized.FindProperty("zones");
                zoneArray.arraySize = zones.Length;
                for (var index = 0; index < zones.Length; index++)
                    zoneArray.GetArrayElementAtIndex(index).objectReferenceValue = zones[index];
                kickSerialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(kickController);
            }

            EditorSceneManager.MarkSceneDirty(root.scene);
        }

        private static void InstallExtendedCorridorFloor()
        {
            var floor = FindSceneObject(ExtendedCorridorFloorName);
            if (floor == null)
            {
                var baseFloor = FindSceneObject(BaseFloorName);
                var parent = baseFloor != null
                    ? baseFloor.transform.parent
                    : FindSceneObject("KLC_LayoutBlockout_Workspace")?.transform;
                var topY = ResolveCorridorFloorTopY();
                var endZ = KickLuckyCubeCorridorLayout.MaximumKickDistance + 24f;
                var depth = endZ - ExtendedCorridorFloorStartZ;
                var centerZ = ExtendedCorridorFloorStartZ + depth * 0.5f;

                var proBuilderMesh = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.Center);
                if (proBuilderMesh == null) return;

                floor = proBuilderMesh.gameObject;
                floor.name = ExtendedCorridorFloorName;
                Undo.RegisterCreatedObjectUndo(floor, "Create KLC extended corridor floor");
                if (parent != null) floor.transform.SetParent(parent, true);
                floor.transform.SetPositionAndRotation(
                    new Vector3(0f, topY - ExtendedCorridorFloorThickness * 0.5f, centerZ),
                    Quaternion.identity);
                floor.transform.localScale = Vector3.one;

                var positions = proBuilderMesh.positions.ToArray();
                var size = new Vector3(
                    ExtendedCorridorFloorWidth,
                    ExtendedCorridorFloorThickness,
                    depth);
                for (var index = 0; index < positions.Length; index++)
                    positions[index] = Vector3.Scale(positions[index], size);
                proBuilderMesh.positions = positions;
                proBuilderMesh.ToMesh();
                proBuilderMesh.Refresh();

                var baseRenderer = baseFloor != null ? baseFloor.GetComponentInChildren<Renderer>() : null;
                var renderer = floor.GetComponent<Renderer>();
                if (renderer != null && baseRenderer != null)
                    renderer.sharedMaterial = baseRenderer.sharedMaterial;

                EditorUtility.SetDirty(proBuilderMesh);
            }

            foreach (var collider in floor.GetComponents<Collider>())
            {
                if (collider is MeshCollider) continue;
                UnityEngine.Object.DestroyImmediate(collider);
            }

            var meshCollider = floor.GetComponent<MeshCollider>();
            if (meshCollider == null) meshCollider = floor.AddComponent<MeshCollider>();
            var meshFilter = floor.GetComponent<MeshFilter>();
            if (meshFilter != null) meshCollider.sharedMesh = meshFilter.sharedMesh;
            meshCollider.enabled = true;
            meshCollider.isTrigger = false;

            if (floor.GetComponent<KickLuckyCubeGroundSurface>() == null)
                floor.AddComponent<KickLuckyCubeGroundSurface>();

            GameObjectUtility.SetStaticEditorFlags(
                floor,
                StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
            EditorUtility.SetDirty(floor);
            EditorUtility.SetDirty(meshCollider);
            EditorSceneManager.MarkSceneDirty(floor.scene);
        }

        private static float ResolveCorridorFloorTopY()
        {
            Physics.SyncTransforms();
            var sampleZ = KickLuckyCubeCorridorLayout.FirstLocationStart
                + KickLuckyCubeCorridorLayout.LocationPlayableLength * 0.5f;
            var hits = Physics.RaycastAll(
                    new Vector3(0f, 120f, sampleZ),
                    Vector3.down,
                    240f,
                    ~0,
                    QueryTriggerInteraction.Ignore)
                .Where(hit => hit.collider != null && hit.collider.name.Contains("Floor", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(hit => hit.point.y)
                .ToArray();
            return hits.Length > 0 ? hits[0].point.y : -3.65f;
        }

        private static void ConfigureGameplayZone(KickLuckyCubeRarityZone zone, int locationIndex)
        {
            var options = KickLuckyCubeAnimalCatalog.CreateLocationOptions(locationIndex);
            var rarity = options.Length > 0
                ? options.Max(option => option.Rarity)
                : KickLuckyCubeRarity.Common;
            var animalPool = options.Length > 0
                ? string.Join(" / ", options.Select(option => option.AnimalName).Distinct())
                : "No animals configured";

            var serialized = new SerializedObject(zone);
            serialized.FindProperty("rarity").enumValueIndex = (int)rarity;
            serialized.FindProperty("zoneIndex").intValue = locationIndex;
            serialized.FindProperty("animalPoolText").stringValue = animalPool;
            serialized.FindProperty("zoneRenderer").objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(zone);
        }

        private static void InstallPlayerKickBoundary()
        {
            var kickLine = FindSceneObject(KickLineName);
            if (kickLine == null) return;

            var boundary = kickLine.GetComponent<KickLuckyCubePlayerKickBoundary>()
                ?? kickLine.AddComponent<KickLuckyCubePlayerKickBoundary>();
            var serialized = new SerializedObject(boundary);
            serialized.FindProperty("player").objectReferenceValue = FindSceneObject("KLC_PrototypePlayer");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(boundary);
            EditorSceneManager.MarkSceneDirty(kickLine.scene);
        }

        private static void RepairLegacyEconomyHudReference()
        {
            var collectButton = FindFirstObjectByType<KickLuckyCubeStableCollectButton>(FindObjectsInactive.Include);
            foreach (var hud in FindObjectsByType<KickLuckyCubeEconomyHud>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                var serialized = new SerializedObject(hud);
                serialized.FindProperty("collectButton").objectReferenceValue = collectButton;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(hud);
            }
        }

        private static void InstallWorldShopContract(
            string objectName,
            KickLuckyCubeWorldShopAuthoring.ShopKind kind,
            string windowPath)
        {
            var root = FindSceneObject(objectName);
            if (root == null) return;
            var contract = root.GetComponent<KickLuckyCubeWorldShopAuthoring>()
                ?? root.AddComponent<KickLuckyCubeWorldShopAuthoring>();
            var serialized = new SerializedObject(contract);
            serialized.FindProperty("shopKind").enumValueIndex = (int)kind;
            serialized.FindProperty("visualRoot").objectReferenceValue = root.transform;
            serialized.FindProperty("interactionAnchor").objectReferenceValue = FindChildByTerms(root.transform, "StandPad", "HoldInteraction", "ReservedPad");
            serialized.FindProperty("signAnchor").objectReferenceValue = FindChildByTerms(root.transform, "_Sign", "BackSign", "_Label");
            serialized.FindProperty("windowPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(windowPath);
            serialized.FindProperty("interactionBinding").objectReferenceValue = ResolveShopBinding(kind) ?? contract;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            contract.AutoBind();
            EditorUtility.SetDirty(contract);
            EditorSceneManager.MarkSceneDirty(root.scene);
        }

        private static MonoBehaviour ResolveShopBinding(KickLuckyCubeWorldShopAuthoring.ShopKind kind)
        {
            return kind switch
            {
                KickLuckyCubeWorldShopAuthoring.ShopKind.Speed
                    => FindFirstObjectByType<KickLuckyCubeSpeedShopController>(FindObjectsInactive.Include),
                KickLuckyCubeWorldShopAuthoring.ShopKind.TrainingTools
                    => FindFirstObjectByType<KickLuckyCubeStrengthToolShopController>(FindObjectsInactive.Include),
                KickLuckyCubeWorldShopAuthoring.ShopKind.SellAnimals
                    => FindFirstObjectByType<KickLuckyCubeSellShopController>(FindObjectsInactive.Include),
                KickLuckyCubeWorldShopAuthoring.ShopKind.Styles
                    => FindFirstObjectByType<KickLuckyCubeStyleShopController>(FindObjectsInactive.Include),
                _ => FindFirstObjectByType<KickLuckyCubeFutureFeatureController>(FindObjectsInactive.Include),
            };
        }

        private static GameObject FindSceneObject(string exactName)
        {
            return Resources.FindObjectsOfTypeAll<Transform>()
                .Where(item => item != null
                    && item.gameObject.scene.IsValid()
                    && string.Equals(item.name, exactName, StringComparison.Ordinal))
                .Select(item => item.gameObject)
                .FirstOrDefault();
        }

        private static Transform FindChildByTerms(Transform root, params string[] terms)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child => child != root
                    && terms.Any(term => child.name.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        private static void AddContractToSelection(Type type)
        {
            var selected = Selection.activeGameObject;
            if (selected == null) return;
            var component = selected.GetComponent(type) ?? Undo.AddComponent(selected, type);
            AutoBind(component);
            EditorUtility.SetDirty(component);
            Selection.activeObject = component;
        }

        private static void AutoBind(Component component)
        {
            switch (component)
            {
                case KickLuckyCubeBiomeAuthoring biome: biome.AutoBind(); break;
                case KickLuckyCubeCorridorAuthoring corridor: corridor.AutoBind(); break;
                case KickLuckyCubeWorldShopAuthoring shop: shop.AutoBind(); break;
                case KickLuckyCubePlayerBaseAuthoring playerBase: playerBase.AutoBind(); break;
                case KickLuckyCubeLuckyCubeAuthoring cube: cube.AutoBind(); break;
                case KickLuckyCubeTrainingAnimationAuthoring training: training.AutoBind(); break;
            }
        }

        private static void CreatePrefabFromSelection(Type contractType, string folder)
        {
            var selected = Selection.activeGameObject;
            if (selected == null) return;
            if (!EditorUtility.DisplayDialog(
                    "Create connected prefab",
                    $"Create a prefab from '{selected.name}' and connect the scene object to it?",
                    "Create",
                    "Cancel")) return;

            AddContractToSelection(contractType);
            var path = EditorUtility.SaveFilePanelInProject(
                "Save authoring prefab",
                selected.name,
                "prefab",
                "Choose a project path for the prefab.",
                folder);
            if (string.IsNullOrWhiteSpace(path)) return;

            var prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(selected, path, InteractionMode.UserAction);
            if (prefab != null)
            {
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log($"[KLC-AUTHORING] Created and connected {path}.");
            }
        }

        internal static void RebuildCorridorPreview()
        {
            var selected = Selection.activeGameObject;
            var corridor = selected != null ? selected.GetComponent<KickLuckyCubeCorridorAuthoring>() : null;
            if (corridor == null) return;
            if (!EditorUtility.DisplayDialog(
                    "Build biome preview",
                    "Replace only the KLC_AuthoringBiomeInstances preview root? Existing authored corridor objects are not touched.",
                    "Build Preview",
                    "Cancel")) return;

            if (corridor.PreviewRoot != null)
                Undo.DestroyObjectImmediate(corridor.PreviewRoot.gameObject);

            var previewRoot = new GameObject("KLC_AuthoringBiomeInstances");
            Undo.RegisterCreatedObjectUndo(previewRoot, "Create corridor preview");
            previewRoot.transform.SetParent(corridor.transform, true);
            corridor.SetPreviewRoot(previewRoot.transform);

            for (var index = 0; index < corridor.BiomePrefabs.Count; index++)
            {
                var prefab = corridor.BiomePrefabs[index];
                if (prefab == null) continue;
                var instance = PrefabUtility.InstantiatePrefab(prefab, corridor.gameObject.scene) as GameObject;
                if (instance == null) continue;
                Undo.RegisterCreatedObjectUndo(instance, "Instantiate biome preview");
                instance.name = $"BiomePreview_{index + 1:00}_{prefab.name}";
                instance.transform.SetParent(previewRoot.transform, true);
                instance.transform.SetPositionAndRotation(corridor.GetPreviewPosition(index), corridor.GetPreviewRotation());
            }

            EditorUtility.SetDirty(corridor);
            EditorSceneManager.MarkSceneDirty(corridor.gameObject.scene);
            Selection.activeObject = previewRoot;
        }

        internal static void PreviewCameraShot(bool wave)
        {
            var target = Selection.activeTransform;
            var config = AssetDatabase.LoadAssetAtPath<KickLuckyCubeCameraChoreographyConfig>(CameraConfigPath);
            var sceneView = SceneView.lastActiveSceneView;
            if (target == null || config == null || sceneView == null) return;
            var shot = wave ? config.WaveReveal : config.RunnerReady;
            var home = FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);
            var homePosition = home != null ? home.transform.position : target.position - Vector3.forward;
            var yaw = shot.ResolveYaw(target, homePosition);
            sceneView.LookAtDirect(target.position + shot.FocusOffset, Quaternion.Euler(shot.Pitch, yaw, 0f), shot.Distance);
            sceneView.cameraSettings.fieldOfView = shot.FieldOfView;
            sceneView.Repaint();
        }

        internal static void SampleSelectedTrainingClip()
        {
            var selected = Selection.activeGameObject;
            var authoring = selected != null ? selected.GetComponentInParent<KickLuckyCubeTrainingAnimationAuthoring>() : null;
            if (authoring == null || authoring.PreviewClip == null) return;
            if (!AnimationMode.InAnimationMode()) AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(
                authoring.gameObject,
                authoring.PreviewClip,
                authoring.PreviewClip.length * authoring.PreviewNormalizedTime);
            AnimationMode.EndSampling();
            SceneView.RepaintAll();
        }

        private static void SelectSceneObject<T>() where T : Component
        {
            var target = FindFirstObjectByType<T>(FindObjectsInactive.Include);
            if (target == null) return;
            Selection.activeObject = target.gameObject;
            EditorGUIUtility.PingObject(target.gameObject);
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private static void OpenOverviewScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }

    internal abstract class KickLuckyCubeContractEditor<T> : UnityEditor.Editor where T : Component
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Auto-bind obvious references"))
            {
                Undo.RecordObject(target, "Auto-bind KLC authoring contract");
                AutoBind((T)target);
                EditorUtility.SetDirty(target);
            }
        }

        protected abstract void AutoBind(T component);
    }

    [CustomEditor(typeof(KickLuckyCubeBiomeAuthoring))]
    internal sealed class KickLuckyCubeBiomeAuthoringEditor : KickLuckyCubeContractEditor<KickLuckyCubeBiomeAuthoring>
    { protected override void AutoBind(KickLuckyCubeBiomeAuthoring component) => component.AutoBind(); }

    [CustomEditor(typeof(KickLuckyCubeCorridorAuthoring))]
    internal sealed class KickLuckyCubeCorridorAuthoringEditor : KickLuckyCubeContractEditor<KickLuckyCubeCorridorAuthoring>
    {
        protected override void AutoBind(KickLuckyCubeCorridorAuthoring component) => component.AutoBind();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Build non-destructive corridor preview"))
            {
                Selection.activeObject = target;
                KickLuckyCubeAuthoringWorkspace.RebuildCorridorPreview();
            }
        }
    }

    [CustomEditor(typeof(KickLuckyCubeWorldShopAuthoring))]
    internal sealed class KickLuckyCubeWorldShopAuthoringEditor : KickLuckyCubeContractEditor<KickLuckyCubeWorldShopAuthoring>
    {
        protected override void AutoBind(KickLuckyCubeWorldShopAuthoring component) => component.AutoBind();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("Preview state", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (KickLuckyCubeWorldShopAuthoring.PreviewState state in Enum.GetValues(typeof(KickLuckyCubeWorldShopAuthoring.PreviewState)))
                {
                    if (!GUILayout.Button(state.ToString())) continue;
                    Undo.RecordObject(target, "Change shop preview state");
                    ((KickLuckyCubeWorldShopAuthoring)target).ApplyPreviewState(state);
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }

    [CustomEditor(typeof(KickLuckyCubePlayerBaseAuthoring))]
    internal sealed class KickLuckyCubePlayerBaseAuthoringEditor : KickLuckyCubeContractEditor<KickLuckyCubePlayerBaseAuthoring>
    { protected override void AutoBind(KickLuckyCubePlayerBaseAuthoring component) => component.AutoBind(); }

    [CustomEditor(typeof(KickLuckyCubeLuckyCubeAuthoring))]
    internal sealed class KickLuckyCubeLuckyCubeAuthoringEditor : KickLuckyCubeContractEditor<KickLuckyCubeLuckyCubeAuthoring>
    { protected override void AutoBind(KickLuckyCubeLuckyCubeAuthoring component) => component.AutoBind(); }

    [CustomEditor(typeof(KickLuckyCubeTrainingAnimationAuthoring))]
    internal sealed class KickLuckyCubeTrainingAnimationAuthoringEditor : KickLuckyCubeContractEditor<KickLuckyCubeTrainingAnimationAuthoring>
    {
        protected override void AutoBind(KickLuckyCubeTrainingAnimationAuthoring component) => component.AutoBind();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var authoring = (KickLuckyCubeTrainingAnimationAuthoring)target;
            EditorGUILayout.LabelField(
                "Grip error",
                $"L {authoring.LeftGripError:0.000} m / R {authoring.RightGripError:0.000} m");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Sample clip"))
                {
                    Selection.activeObject = target;
                    KickLuckyCubeAuthoringWorkspace.SampleSelectedTrainingClip();
                }
                GUI.enabled = authoring.ToolRoot != null;
                if (GUILayout.Button("Align tool to hands"))
                {
                    Undo.RecordObject(authoring.ToolRoot, "Align training tool to hands");
                    authoring.AlignToolToHands();
                    EditorUtility.SetDirty(authoring.ToolRoot);
                }
                GUI.enabled = true;
                if (GUILayout.Button("Stop preview")) AnimationMode.StopAnimationMode();
            }
        }
    }

    [CustomEditor(typeof(KickLuckyCubeUiAuthoringPreview))]
    internal sealed class KickLuckyCubeUiAuthoringPreviewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var preview = (KickLuckyCubeUiAuthoringPreview)target;
            if (GUILayout.Button("Apply preview state")) preview.ApplyPreview();
            if (GUILayout.Button("Fill common sample values")) preview.ApplyHeuristicSampleText();
        }
    }

    [CustomEditor(typeof(KickLuckyCubeCameraChoreographyConfig))]
    internal sealed class KickLuckyCubeCameraChoreographyConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Select a target in the scene before previewing a shot.", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = Selection.activeTransform != null;
                if (GUILayout.Button("Preview Wave")) KickLuckyCubeAuthoringWorkspace.PreviewCameraShot(true);
                if (GUILayout.Button("Preview Runner")) KickLuckyCubeAuthoringWorkspace.PreviewCameraShot(false);
                GUI.enabled = true;
            }
        }
    }
}
