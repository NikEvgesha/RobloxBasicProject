using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RobloxBasicProject.Games.KickLuckyCube;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace RobloxBasicProject.Games.KickLuckyCube.Editor
{
    public static class KickLuckyCubeWorldPolishTools
    {
        private const string ScenePath = "Assets/Games/KickLuckyCube/Scenes/KickLuckyCubeOverview.unity";
        private const string StretchMarkerName = "KLC_WorldPolish_2xDepthMarker";
        private const string DecorRootName = "KLC_ProceduralDecor_v2";
        private const string ShopPolishRootName = "KLC_VoxelPolish_v2";
        private const string MainLocationPolishRootName = "KLC_MainLocationProBuilderPolish_v1";
        private const string CorridorPolishRootName = "KLC_KickCorridorProBuilderPolish_v1";
        private const string WavePolishRootName = "KLC_WaveVoxelFoam_v2";
        private const string LuckyCubeQuestionFlipMarkerName = "KLC_LuckyCube_QuestionFlipMarker";
        private static readonly Regex BiomeIndexRegex = new(@"MOVE_Biome_(\d+)_", RegexOptions.Compiled);

        [MenuItem("Tools/Kick Lucky Cube/Apply World Polish")]
        public static void ApplyWorldPolish()
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                Debug.LogError($"Open {ScenePath} before applying Kick Lucky Cube world polish.");
                return;
            }

            StretchLocationsOnce();
            SynchronizeGameplayDistances();
            BuildLocationDecorations();
            BuildShopPolish();
            BuildMainLocationPolish();
            BuildKickCorridorPolish();
            PolishWave();
            FlipLuckyCubeQuestionOrientation();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("KLC_WORLD_POLISH_COMPLETE|locations=30|depthScale=2|decor=deterministic|shops=polished|wave=transparent");
        }

        private static void StretchLocationsOnce()
        {
            if (GameObject.Find(StretchMarkerName) != null)
            {
                return;
            }

            var moved = new HashSet<int>();
            var guide = GameObject.Find("02_ZoneAndRiverGuides");
            if (guide != null)
            {
                var renderTransforms = guide.GetComponentsInChildren<Renderer>(true)
                    .Select(renderer => renderer.transform)
                    .Distinct()
                    .Where(transform => transform.position.z >= KickLuckyCubeCorridorLayout.FirstLocationStart - 0.01f)
                    .ToArray();

                foreach (var target in renderTransforms)
                {
                    StretchPosition(target);
                    moved.Add(target.GetInstanceID());
                    if (IsLongitudinalSurface(target.name))
                    {
                        var scale = target.localScale;
                        scale.z *= 2f;
                        target.localScale = scale;
                    }
                }
            }

            foreach (var zone in UnityEngine.Object.FindObjectsByType<KickLuckyCubeRarityZone>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (!moved.Contains(zone.transform.GetInstanceID()))
                {
                    StretchPosition(zone.transform);
                }

                var scale = zone.transform.localScale;
                scale.z *= 2f;
                zone.transform.localScale = scale;
            }

            foreach (var globalName in new[]
                     {
                         "KLC_ExtendedCorridorFloor_30Zones",
                         "KLC_ExtendedLeftWall_30Zones",
                         "KLC_ExtendedRightWall_30Zones"
                     })
            {
                var target = GameObject.Find(globalName);
                if (target == null)
                {
                    continue;
                }

                StretchPosition(target.transform);
                var scale = target.transform.localScale;
                scale.z *= 2f;
                target.transform.localScale = scale;
            }

            var workspace = GameObject.Find("KLC_LayoutBlockout_Workspace");
            var marker = new GameObject(StretchMarkerName);
            if (workspace != null)
            {
                marker.transform.SetParent(workspace.transform, false);
            }

            marker.SetActive(false);
        }

        private static void SynchronizeGameplayDistances()
        {
            var kick = UnityEngine.Object.FindFirstObjectByType<KickLuckyCubeKickController>(FindObjectsInactive.Include);
            if (kick != null)
            {
                var serializedKick = new SerializedObject(kick);
                serializedKick.FindProperty("maximumDistance").floatValue = KickLuckyCubeCorridorLayout.MaximumKickDistance;
                serializedKick.ApplyModifiedPropertiesWithoutUndo();
            }

            var wave = UnityEngine.Object.FindFirstObjectByType<KickLuckyCubeWaveChaseController>(FindObjectsInactive.Include);
            if (wave != null)
            {
                var serializedWave = new SerializedObject(wave);
                serializedWave.FindProperty("firstWaveLocationStartDistance").floatValue = KickLuckyCubeCorridorLayout.FirstLocationStart;
                serializedWave.FindProperty("waveLocationLength").floatValue = KickLuckyCubeCorridorLayout.LocationSpacing;
                serializedWave.ApplyModifiedPropertiesWithoutUndo();
            }

            var config = AssetDatabase.LoadAssetAtPath<KickLuckyCubeBalanceConfig>(
                "Assets/Games/KickLuckyCube/Resources/KickLuckyCube/KickLuckyCubeBalanceConfig.asset");
            if (config == null)
            {
                return;
            }

            var serializedConfig = new SerializedObject(config);
            var maximumDistance = serializedConfig.FindProperty("maximumKickDistance");
            var curveProperty = serializedConfig.FindProperty("strengthToDistance");
            if (maximumDistance.floatValue < 1000f)
            {
                var curve = curveProperty.animationCurveValue;
                var keys = curve.keys;
                for (var index = 0; index < keys.Length; index++)
                {
                    keys[index].value = KickLuckyCubeCorridorLayout.StretchLegacyDistance(keys[index].value);
                }

                curve.keys = keys;
                curveProperty.animationCurveValue = curve;
            }

            maximumDistance.floatValue = KickLuckyCubeCorridorLayout.MaximumKickDistance;
            serializedConfig.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(config);
        }

        private static void BuildLocationDecorations()
        {
            var biomeRoot = GameObject.Find("KLC_BiomeBlockout_30Locations");
            if (biomeRoot == null)
            {
                Debug.LogWarning("KLC world polish could not find KLC_BiomeBlockout_30Locations.");
                return;
            }

            var zones = UnityEngine.Object.FindObjectsByType<KickLuckyCubeRarityZone>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .ToDictionary(zone => zone.ZoneIndex);

            foreach (Transform biome in biomeRoot.transform)
            {
                var match = BiomeIndexRegex.Match(biome.name);
                if (!match.Success || !int.TryParse(match.Groups[1].Value, out var index) || !zones.TryGetValue(index, out var zone))
                {
                    continue;
                }

                DestroyNamedChild(biome, DecorRootName);
                var decorRoot = new GameObject(DecorRootName);
                decorRoot.transform.SetParent(biome, false);

                var renderers = biome.GetComponentsInChildren<Renderer>(true)
                    .Where(renderer => !renderer.transform.IsChildOf(decorRoot.transform))
                    .ToArray();
                var floor = renderers.FirstOrDefault(renderer => renderer.name.Contains("_Floor_", StringComparison.Ordinal));
                var materials = renderers.Select(renderer => renderer.sharedMaterial)
                    .Where(material => material != null)
                    .Distinct()
                    .ToArray();
                var primary = floor != null ? floor.sharedMaterial : materials.FirstOrDefault();
                var accent = materials.FirstOrDefault(material => material != primary) ?? primary;
                var secondary = materials.LastOrDefault(material => material != primary && material != accent) ?? accent;
                var groundY = floor != null ? floor.bounds.max.y + 0.025f : -3.62f;

                var random = new System.Random(7319 + index * 97);
                for (var propIndex = 0; propIndex < 8; propIndex++)
                {
                    var side = propIndex % 2 == 0 ? -1f : 1f;
                    var x = side * Next(random, 15f, 32.5f);
                    var z = Next(random, zone.StartZ + 2.2f, zone.EndZ - 2.2f);
                    var cluster = new GameObject($"Decor_{index:00}_{propIndex:00}");
                    cluster.transform.SetParent(decorRoot.transform, false);
                    cluster.transform.position = new Vector3(x, groundY, z);
                    cluster.transform.rotation = Quaternion.Euler(0f, Next(random, 0f, 360f), 0f);
                    BuildThemedCluster(cluster.transform, biome.name, propIndex, random, primary, accent, secondary);
                }
            }
        }

        private static void BuildThemedCluster(
            Transform parent,
            string biomeName,
            int propIndex,
            System.Random random,
            Material primary,
            Material accent,
            Material secondary)
        {
            var lower = biomeName.ToLowerInvariant();
            if (ContainsAny(lower, "desert", "badlands", "ash", "moon"))
            {
                if (propIndex % 3 == 0)
                {
                    BuildCactus(parent, random, accent, secondary);
                }
                else
                {
                    BuildRockCluster(parent, random, accent, secondary);
                }
                return;
            }

            if (ContainsAny(lower, "snow", "ice", "crystal", "magic", "cosmic", "divine", "sky"))
            {
                BuildCrystalCluster(parent, random, accent, secondary);
                return;
            }

            if (ContainsAny(lower, "volcano", "lava"))
            {
                BuildBasaltCluster(parent, random, accent, secondary);
                return;
            }

            if (ContainsAny(lower, "beach", "pirate", "coral"))
            {
                if (propIndex % 2 == 0)
                {
                    BuildPalm(parent, random, accent, secondary);
                }
                else
                {
                    BuildCrates(parent, random, accent, secondary);
                }
                return;
            }

            if (ContainsAny(lower, "candy", "toy", "neon", "city"))
            {
                BuildToyBlocks(parent, random, primary, accent, secondary);
                return;
            }

            if (propIndex % 3 == 0)
            {
                BuildRockCluster(parent, random, accent, secondary);
            }
            else
            {
                BuildVoxelTree(parent, random, secondary, accent);
            }
        }

        private static void BuildVoxelTree(Transform parent, System.Random random, Material trunk, Material foliage)
        {
            var height = Next(random, 1.3f, 2.4f);
            CreateCube(parent, "Trunk", new Vector3(0f, height * 0.5f, 0f), new Vector3(0.38f, height, 0.38f), trunk);
            CreateCube(parent, "CrownA", new Vector3(0f, height + 0.45f, 0f), new Vector3(1.4f, 0.9f, 1.25f), foliage, Quaternion.Euler(0f, 20f, 0f));
            CreateCube(parent, "CrownB", new Vector3(0.25f, height + 1.05f, -0.1f), new Vector3(0.95f, 0.7f, 0.9f), foliage, Quaternion.Euler(0f, -16f, 0f));
        }

        private static void BuildCactus(Transform parent, System.Random random, Material body, Material detail)
        {
            var height = Next(random, 1.2f, 2.3f);
            CreateCube(parent, "CactusStem", new Vector3(0f, height * 0.5f, 0f), new Vector3(0.48f, height, 0.48f), body);
            CreateCube(parent, "CactusArmL", new Vector3(-0.42f, height * 0.58f, 0f), new Vector3(0.75f, 0.30f, 0.34f), detail);
            CreateCube(parent, "CactusArmR", new Vector3(0.38f, height * 0.78f, 0f), new Vector3(0.65f, 0.28f, 0.34f), detail);
        }

        private static void BuildRockCluster(Transform parent, System.Random random, Material first, Material second)
        {
            for (var index = 0; index < 3; index++)
            {
                var scale = Next(random, 0.45f, 1.1f);
                CreateCube(
                    parent,
                    "Rock_" + index,
                    new Vector3((index - 1) * 0.65f, scale * 0.32f, Next(random, -0.35f, 0.35f)),
                    new Vector3(scale, scale * 0.65f, scale * 0.8f),
                    index % 2 == 0 ? first : second,
                    Quaternion.Euler(Next(random, -12f, 12f), Next(random, 0f, 180f), Next(random, -10f, 10f)));
            }
        }

        private static void BuildCrystalCluster(Transform parent, System.Random random, Material first, Material second)
        {
            for (var index = 0; index < 3; index++)
            {
                var height = Next(random, 0.9f, 2.1f);
                CreateCube(
                    parent,
                    "Crystal_" + index,
                    new Vector3((index - 1) * 0.55f, height * 0.5f, Next(random, -0.25f, 0.25f)),
                    new Vector3(0.38f, height, 0.38f),
                    index % 2 == 0 ? first : second,
                    Quaternion.Euler(Next(random, -18f, 18f), 45f, Next(random, -12f, 12f)));
            }
        }

        private static void BuildBasaltCluster(Transform parent, System.Random random, Material rock, Material ember)
        {
            for (var index = 0; index < 3; index++)
            {
                var height = Next(random, 0.65f, 1.75f);
                CreateCube(parent, "Basalt_" + index, new Vector3((index - 1) * 0.58f, height * 0.5f, 0f), new Vector3(0.5f, height, 0.5f), rock, Quaternion.Euler(0f, 45f, 0f));
            }
            CreateCube(parent, "Ember", new Vector3(0.15f, 0.16f, -0.65f), new Vector3(0.28f, 0.28f, 0.28f), ember, Quaternion.Euler(0f, 45f, 0f));
        }

        private static void BuildPalm(Transform parent, System.Random random, Material trunk, Material leaves)
        {
            var height = Next(random, 1.5f, 2.4f);
            CreateCube(parent, "PalmTrunk", new Vector3(0f, height * 0.5f, 0f), new Vector3(0.38f, height, 0.38f), trunk, Quaternion.Euler(0f, 0f, Next(random, -6f, 6f)));
            CreateCube(parent, "PalmLeavesA", new Vector3(0f, height + 0.25f, 0f), new Vector3(2.1f, 0.25f, 0.48f), leaves, Quaternion.Euler(0f, 25f, 0f));
            CreateCube(parent, "PalmLeavesB", new Vector3(0f, height + 0.28f, 0f), new Vector3(2.1f, 0.25f, 0.48f), leaves, Quaternion.Euler(0f, -55f, 0f));
        }

        private static void BuildCrates(Transform parent, System.Random random, Material first, Material second)
        {
            CreateCube(parent, "CrateA", new Vector3(-0.35f, 0.38f, 0f), new Vector3(0.75f, 0.75f, 0.75f), first, Quaternion.Euler(0f, 8f, 0f));
            CreateCube(parent, "CrateB", new Vector3(0.42f, 0.30f, 0.18f), new Vector3(0.60f, 0.60f, 0.60f), second, Quaternion.Euler(0f, -12f, 0f));
            CreateCube(parent, "CrateTop", new Vector3(-0.28f, 1.02f, 0.02f), new Vector3(0.52f, 0.52f, 0.52f), second, Quaternion.Euler(0f, 22f, 0f));
        }

        private static void BuildToyBlocks(Transform parent, System.Random random, Material first, Material second, Material third)
        {
            var materials = new[] { first, second, third };
            for (var index = 0; index < 4; index++)
            {
                var size = Next(random, 0.45f, 0.85f);
                CreateCube(parent, "Block_" + index, new Vector3((index - 1.5f) * 0.52f, size * 0.5f + (index % 2) * 0.42f, Next(random, -0.25f, 0.25f)), new Vector3(size, size, size), materials[index % materials.Length], Quaternion.Euler(0f, index * 18f, 0f));
            }
        }

        private static void BuildShopPolish()
        {
            var immediate = GameObject.Find("KLC_ImmediateKiosks_LeftToRight");
            if (immediate != null)
            {
                foreach (Transform shop in immediate.transform)
                {
                    if (shop.name.Contains("KLC_Kiosk_", StringComparison.Ordinal))
                    {
                        BuildMainKiosk(shop);
                    }
                }
            }

            var future = GameObject.Find("KLC_FutureFeatureSpots");
            if (future != null)
            {
                foreach (Transform stand in future.transform)
                {
                    BuildFutureStand(stand);
                }
            }
        }

        private static void BuildMainKiosk(Transform shop)
        {
            DestroyNamedChild(shop, ShopPolishRootName);
            var root = new GameObject(ShopPolishRootName).transform;
            root.SetParent(shop, false);
            var primary = FindMaterial(shop, "BackPanel") ?? FindAnyMaterial(shop);
            var accent = FindMaterial(shop, "Awning") ?? primary;
            var trim = FindMaterial(shop, "Base") ?? primary;

            CreateCube(root, "FrameLeft", new Vector3(-2.02f, 1.92f, 0.42f), new Vector3(0.22f, 2.85f, 0.22f), trim);
            CreateCube(root, "FrameRight", new Vector3(2.02f, 1.92f, 0.42f), new Vector3(0.22f, 2.85f, 0.22f), trim);
            CreateCube(root, "CounterTrim", new Vector3(0f, 1.08f, -1.12f), new Vector3(4.45f, 0.18f, 0.18f), trim);
            CreateCube(root, "BackShelf", new Vector3(0f, 1.78f, 0.45f), new Vector3(3.45f, 0.14f, 0.42f), accent);
            for (var index = 0; index < 6; index++)
            {
                var material = index % 2 == 0 ? primary : accent;
                CreateCube(root, "AwningValance_" + index, new Vector3(-1.75f + index * 0.70f, 3.28f, -1.08f), new Vector3(0.62f, 0.42f, 0.20f), material);
            }
            for (var index = 0; index < 3; index++)
            {
                CreateCube(root, "Display_" + index, new Vector3(-1.15f + index * 1.15f, 2.05f, 0.20f), new Vector3(0.48f, 0.48f + index * 0.08f, 0.42f), index % 2 == 0 ? accent : primary, Quaternion.Euler(0f, index * 18f, 0f));
            }

            BuildShopIdentity(root, shop.name, primary, accent, trim);
        }

        private static void BuildFutureStand(Transform stand)
        {
            DestroyNamedChild(stand, ShopPolishRootName);
            var root = new GameObject(ShopPolishRootName).transform;
            root.SetParent(stand, false);
            var primary = FindMaterial(stand, "BackSign") ?? FindAnyMaterial(stand);
            var accent = FindMaterial(stand, "ReservedPad") ?? primary;
            CreateCube(root, "PostLeft", new Vector3(-1.75f, 1.45f, 0.45f), new Vector3(0.20f, 2.65f, 0.20f), primary);
            CreateCube(root, "PostRight", new Vector3(1.75f, 1.45f, 0.45f), new Vector3(0.20f, 2.65f, 0.20f), primary);
            CreateCube(root, "Canopy", new Vector3(0f, 2.78f, -0.15f), new Vector3(4.1f, 0.28f, 2.2f), accent);
            CreateCube(root, "FrontTrim", new Vector3(0f, 2.62f, -1.20f), new Vector3(4.25f, 0.34f, 0.18f), primary);
            BuildFutureStandIcon(root, stand.name, primary, accent);
        }

        private static void BuildShopIdentity(Transform root, string shopName, Material primary, Material accent, Material trim)
        {
            var lower = shopName.ToLowerInvariant();
            var iconRoot = new GameObject("ReadableIcon_" + SanitizeName(shopName)).transform;
            iconRoot.SetParent(root, false);
            iconRoot.localPosition = new Vector3(0f, 2.66f, -1.27f);

            if (lower.Contains("sell", StringComparison.Ordinal))
            {
                for (var index = 0; index < 4; index++)
                {
                    CreateCube(iconRoot, "Coin_" + index, new Vector3(-0.54f + index * 0.36f, index * 0.08f, 0f), new Vector3(0.28f, 0.28f, 0.10f), accent);
                }
                CreateCube(iconRoot, "CoinSpark", new Vector3(0.82f, 0.35f, 0f), new Vector3(0.18f, 0.56f, 0.10f), primary, Quaternion.Euler(0f, 0f, 45f));
                return;
            }

            if (lower.Contains("style", StringComparison.Ordinal))
            {
                CreateCube(iconRoot, "CubeBody", Vector3.zero, new Vector3(0.72f, 0.72f, 0.32f), accent, Quaternion.Euler(0f, 26f, 0f));
                CreateCube(iconRoot, "CubeEdgeX", new Vector3(0f, 0.42f, 0f), new Vector3(0.88f, 0.12f, 0.36f), trim, Quaternion.Euler(0f, 26f, 0f));
                CreateCube(iconRoot, "CubeEdgeY", new Vector3(-0.42f, 0f, 0f), new Vector3(0.12f, 0.88f, 0.36f), trim, Quaternion.Euler(0f, 26f, 0f));
                return;
            }

            if (lower.Contains("speed", StringComparison.Ordinal))
            {
                CreateCube(iconRoot, "BootToe", new Vector3(0.26f, -0.08f, 0f), new Vector3(0.82f, 0.28f, 0.16f), accent, Quaternion.Euler(0f, 0f, -10f));
                CreateCube(iconRoot, "BootLeg", new Vector3(-0.22f, 0.30f, 0f), new Vector3(0.32f, 0.72f, 0.16f), primary, Quaternion.Euler(0f, 0f, -10f));
                CreateCube(iconRoot, "SpeedSlashA", new Vector3(-0.78f, 0.25f, 0f), new Vector3(0.58f, 0.10f, 0.10f), trim, Quaternion.Euler(0f, 0f, -18f));
                CreateCube(iconRoot, "SpeedSlashB", new Vector3(-0.82f, -0.02f, 0f), new Vector3(0.42f, 0.10f, 0.10f), trim, Quaternion.Euler(0f, 0f, -18f));
                return;
            }

            if (lower.Contains("training", StringComparison.Ordinal) || lower.Contains("weight", StringComparison.Ordinal))
            {
                CreateCube(iconRoot, "Bar", Vector3.zero, new Vector3(1.35f, 0.13f, 0.13f), trim, Quaternion.Euler(0f, 0f, -12f));
                CreateCube(iconRoot, "WeightL", new Vector3(-0.78f, -0.16f, 0f), new Vector3(0.28f, 0.58f, 0.18f), accent, Quaternion.Euler(0f, 0f, -12f));
                CreateCube(iconRoot, "WeightR", new Vector3(0.78f, 0.16f, 0f), new Vector3(0.28f, 0.58f, 0.18f), accent, Quaternion.Euler(0f, 0f, -12f));
                return;
            }

            CreateCube(iconRoot, "TrophyCup", new Vector3(0f, 0.16f, 0f), new Vector3(0.78f, 0.56f, 0.18f), accent);
            CreateCube(iconRoot, "TrophyBase", new Vector3(0f, -0.34f, 0f), new Vector3(1.05f, 0.22f, 0.18f), trim);
            CreateCube(iconRoot, "TrophyHandleL", new Vector3(-0.52f, 0.17f, 0f), new Vector3(0.18f, 0.44f, 0.14f), primary, Quaternion.Euler(0f, 0f, 24f));
            CreateCube(iconRoot, "TrophyHandleR", new Vector3(0.52f, 0.17f, 0f), new Vector3(0.18f, 0.44f, 0.14f), primary, Quaternion.Euler(0f, 0f, -24f));
        }

        private static void BuildFutureStandIcon(Transform root, string standName, Material primary, Material accent)
        {
            var lower = standName.ToLowerInvariant();
            var iconRoot = new GameObject("ReadableFutureIcon_" + SanitizeName(standName)).transform;
            iconRoot.SetParent(root, false);
            iconRoot.localPosition = new Vector3(0f, 2.92f, -1.33f);

            if (lower.Contains("weather", StringComparison.Ordinal))
            {
                CreateCube(iconRoot, "CloudA", new Vector3(-0.36f, 0.03f, 0f), new Vector3(0.58f, 0.35f, 0.14f), accent);
                CreateCube(iconRoot, "CloudB", new Vector3(0.18f, 0.13f, 0f), new Vector3(0.74f, 0.45f, 0.14f), accent);
                CreateCube(iconRoot, "Rain", new Vector3(0f, -0.42f, 0f), new Vector3(0.15f, 0.55f, 0.12f), primary, Quaternion.Euler(0f, 0f, -18f));
                return;
            }

            if (lower.Contains("exchange", StringComparison.Ordinal))
            {
                CreateCube(iconRoot, "ArrowA", new Vector3(-0.34f, 0.18f, 0f), new Vector3(0.85f, 0.14f, 0.12f), primary);
                CreateCube(iconRoot, "ArrowHeadA", new Vector3(0.20f, 0.18f, 0f), new Vector3(0.28f, 0.28f, 0.12f), primary, Quaternion.Euler(0f, 0f, 45f));
                CreateCube(iconRoot, "ArrowB", new Vector3(0.34f, -0.18f, 0f), new Vector3(0.85f, 0.14f, 0.12f), accent);
                CreateCube(iconRoot, "ArrowHeadB", new Vector3(-0.20f, -0.18f, 0f), new Vector3(0.28f, 0.28f, 0.12f), accent, Quaternion.Euler(0f, 0f, 45f));
                return;
            }

            if (lower.Contains("epic", StringComparison.Ordinal))
            {
                CreateCube(iconRoot, "Gem", Vector3.zero, new Vector3(0.68f, 0.68f, 0.18f), accent, Quaternion.Euler(0f, 0f, 45f));
                CreateCube(iconRoot, "GemSpark", new Vector3(0.72f, 0.36f, 0f), new Vector3(0.16f, 0.46f, 0.12f), primary, Quaternion.Euler(0f, 0f, 45f));
                return;
            }

            CreateCube(iconRoot, "StarA", Vector3.zero, new Vector3(0.84f, 0.20f, 0.12f), accent);
            CreateCube(iconRoot, "StarB", Vector3.zero, new Vector3(0.84f, 0.20f, 0.12f), primary, Quaternion.Euler(0f, 0f, 90f));
            CreateCube(iconRoot, "StarC", Vector3.zero, new Vector3(0.72f, 0.16f, 0.12f), accent, Quaternion.Euler(0f, 0f, 45f));
            CreateCube(iconRoot, "StarD", Vector3.zero, new Vector3(0.72f, 0.16f, 0.12f), primary, Quaternion.Euler(0f, 0f, -45f));
        }

        private static void BuildMainLocationPolish()
        {
            var parent = EnsureVisualRoot();
            DestroyNamedChild(parent, MainLocationPolishRootName);
            var root = new GameObject(MainLocationPolishRootName).transform;
            root.SetParent(parent, false);

            var kickLine = GameObject.Find("MOVE_KickLine_YellowBar");
            var lineZ = kickLine != null ? kickLine.transform.position.z : 0f;
            var groundY = ResolveGroundY(lineZ);
            var grass = LoadMaterial("KLC_PB_StartBlue") ?? LoadMaterial("KLC_Grass");
            var tan = LoadMaterial("KLC_Blockout_Yellow") ?? LoadMaterial("KLC_PB_Gold");
            var trim = LoadMaterial("KLC_PB_DarkTrim") ?? LoadMaterial("KLC_DarkTrim");
            var wood = LoadMaterial("KLC_PB_Wood");
            var blue = LoadMaterial("KLC_Blockout_Blue") ?? LoadMaterial("KLC_PB_Common");
            var gold = LoadMaterial("KLC_PB_Gold") ?? LoadMaterial("KLC_KickLineYellow");

            CreateCube(root, "MainPlaza_OuterFrame_Left", new Vector3(-18.2f, groundY + 0.08f, lineZ - 7.4f), new Vector3(0.38f, 0.16f, 10.4f), trim);
            CreateCube(root, "MainPlaza_OuterFrame_Right", new Vector3(18.2f, groundY + 0.08f, lineZ - 7.4f), new Vector3(0.38f, 0.16f, 10.4f), trim);
            CreateCube(root, "MainPlaza_BackFrame", new Vector3(0f, groundY + 0.09f, lineZ - 12.55f), new Vector3(36.8f, 0.16f, 0.42f), trim);
            CreateCube(root, "MainPlaza_FrontGoldLine", new Vector3(0f, groundY + 0.12f, lineZ - 1.9f), new Vector3(36.8f, 0.18f, 0.34f), gold);
            CreateCube(root, "MainPlaza_CenterRunway", new Vector3(0f, groundY + 0.11f, lineZ - 7.3f), new Vector3(8.2f, 0.13f, 10.0f), tan);
            CreateCube(root, "MainPlaza_CenterRunway_TrimL", new Vector3(-4.32f, groundY + 0.16f, lineZ - 7.3f), new Vector3(0.22f, 0.18f, 10.0f), trim);
            CreateCube(root, "MainPlaza_CenterRunway_TrimR", new Vector3(4.32f, groundY + 0.16f, lineZ - 7.3f), new Vector3(0.22f, 0.18f, 10.0f), trim);

            for (var index = 0; index < 6; index++)
            {
                var x = -13.5f + index * 5.4f;
                CreateCube(root, "MainPlaza_Stud_" + index, new Vector3(x, groundY + 0.19f, lineZ - 5.8f), new Vector3(1.2f, 0.12f, 1.2f), grass, Quaternion.Euler(0f, 45f, 0f));
                CreateCube(root, "MainPlaza_BollardL_" + index, new Vector3(-19.2f, groundY + 0.55f, lineZ - 11.4f + index * 1.55f), new Vector3(0.38f, 0.9f, 0.38f), wood);
                CreateCube(root, "MainPlaza_BollardR_" + index, new Vector3(19.2f, groundY + 0.55f, lineZ - 11.4f + index * 1.55f), new Vector3(0.38f, 0.9f, 0.38f), wood);
            }

            CreateCube(root, "KickGate_LeftPost", new Vector3(-5.25f, groundY + 2.0f, lineZ - 0.7f), new Vector3(0.5f, 3.6f, 0.5f), trim);
            CreateCube(root, "KickGate_RightPost", new Vector3(5.25f, groundY + 2.0f, lineZ - 0.7f), new Vector3(0.5f, 3.6f, 0.5f), trim);
            CreateCube(root, "KickGate_Top", new Vector3(0f, groundY + 3.88f, lineZ - 0.7f), new Vector3(11.4f, 0.52f, 0.55f), blue);
            CreateCube(root, "KickGate_ArrowStem", new Vector3(0f, groundY + 0.27f, lineZ - 3.65f), new Vector3(1.3f, 0.15f, 3.2f), gold);
            CreateCube(root, "KickGate_ArrowHeadA", new Vector3(-0.58f, groundY + 0.28f, lineZ - 2.05f), new Vector3(1.55f, 0.16f, 0.38f), gold, Quaternion.Euler(0f, 28f, 0f));
            CreateCube(root, "KickGate_ArrowHeadB", new Vector3(0.58f, groundY + 0.28f, lineZ - 2.05f), new Vector3(1.55f, 0.16f, 0.38f), gold, Quaternion.Euler(0f, -28f, 0f));
        }

        private static void BuildKickCorridorPolish()
        {
            var parent = EnsureVisualRoot();
            DestroyNamedChild(parent, CorridorPolishRootName);
            var root = new GameObject(CorridorPolishRootName).transform;
            root.SetParent(parent, false);

            var dark = LoadMaterial("KLC_PB_DarkTrim") ?? LoadMaterial("KLC_DarkTrim");
            var stone = LoadMaterial("KLC_PB_Stone") ?? LoadMaterial("KLC_StartStone");
            var gold = LoadMaterial("KLC_PB_Gold") ?? LoadMaterial("KLC_KickLineYellow");
            var common = LoadMaterial("KLC_ZoneCommon");
            var uncommon = LoadMaterial("KLC_ZoneUncommon");
            var rare = LoadMaterial("KLC_ZoneRare");
            var epic = LoadMaterial("KLC_ZoneEpic");
            var legendary = LoadMaterial("KLC_ZoneLegendary");
            var rarityMaterials = new[] { common, uncommon, rare, epic, legendary, gold };

            for (var location = 1; location <= KickLuckyCubeCorridorLayout.LocationCount; location++)
            {
                var startZ = KickLuckyCubeCorridorLayout.FirstLocationStart + (location - 1) * KickLuckyCubeCorridorLayout.LocationSpacing;
                var centerZ = startZ + KickLuckyCubeCorridorLayout.LocationSpacing * 0.5f;
                var groundY = ResolveGroundY(centerZ);
                var color = rarityMaterials[Mathf.Min(rarityMaterials.Length - 1, location / 6)] ?? stone;

                CreateCube(root, $"Corridor_Loc_{location:00}_LeftTicker", new Vector3(-33.4f, groundY + 0.42f, centerZ), new Vector3(0.42f, 0.84f, 6.6f), color);
                CreateCube(root, $"Corridor_Loc_{location:00}_RightTicker", new Vector3(33.4f, groundY + 0.42f, centerZ), new Vector3(0.42f, 0.84f, 6.6f), color);

                if (location % 3 == 1)
                {
                    CreateCube(root, $"Corridor_Gate_{location:00}_LeftPost", new Vector3(-30.6f, groundY + 3.05f, startZ + 1.4f), new Vector3(0.68f, 5.9f, 0.68f), dark);
                    CreateCube(root, $"Corridor_Gate_{location:00}_RightPost", new Vector3(30.6f, groundY + 3.05f, startZ + 1.4f), new Vector3(0.68f, 5.9f, 0.68f), dark);
                    CreateCube(root, $"Corridor_Gate_{location:00}_TopBar", new Vector3(0f, groundY + 6.15f, startZ + 1.4f), new Vector3(62.0f, 0.52f, 0.64f), color);
                    CreateCube(root, $"Corridor_Gate_{location:00}_TopTrim", new Vector3(0f, groundY + 6.55f, startZ + 1.4f), new Vector3(56.0f, 0.24f, 0.74f), gold);
                }

                if (location % 2 == 0)
                {
                    CreateCube(root, $"Corridor_Loc_{location:00}_FloorArrowStem", new Vector3(0f, groundY + 0.16f, centerZ - 6f), new Vector3(1.6f, 0.12f, 4.8f), color);
                    CreateCube(root, $"Corridor_Loc_{location:00}_FloorArrowHeadL", new Vector3(-0.72f, groundY + 0.17f, centerZ - 3.6f), new Vector3(1.7f, 0.13f, 0.44f), color, Quaternion.Euler(0f, 28f, 0f));
                    CreateCube(root, $"Corridor_Loc_{location:00}_FloorArrowHeadR", new Vector3(0.72f, groundY + 0.17f, centerZ - 3.6f), new Vector3(1.7f, 0.13f, 0.44f), color, Quaternion.Euler(0f, -28f, 0f));
                }
            }
        }

        private static void PolishWave()
        {
            var wave = UnityEngine.Object.FindFirstObjectByType<KickLuckyCubeWaveChaseController>(FindObjectsInactive.Include);
            if (wave == null || wave.WaveVisual == null)
            {
                return;
            }

            var lower = LoadMaterial("KLC_WavePB_Lower");
            var middle = LoadMaterial("KLC_WavePB_Middle");
            var top = LoadMaterial("KLC_WavePB_Top");
            var foam = LoadMaterial("KLC_WavePB_Foam");
            ConfigureTransparent(lower, 0.34f, 0.72f);
            ConfigureTransparent(middle, 0.30f, 0.78f);
            ConfigureTransparent(top, 0.24f, 0.84f);
            ConfigureTransparent(foam, 0.46f, 0.92f);

            var shadow = LoadMaterial("KLC_WaveDangerShadow");
            var edge = LoadMaterial("KLC_WaveDangerShadow_Edge");
            SetAlpha(shadow, 0.24f);
            SetAlpha(edge, 0.38f);

            DestroyNamedChild(wave.WaveVisual, WavePolishRootName);
            var root = new GameObject(WavePolishRootName).transform;
            root.SetParent(wave.WaveVisual, false);
            var random = new System.Random(9217);
            for (var index = 0; index < 18; index++)
            {
                var x = Mathf.Lerp(-13.2f, 13.2f, index / 17f) + Next(random, -0.28f, 0.28f);
                var y = 1.45f + Mathf.Sin(index * 0.83f) * 0.55f + Next(random, -0.18f, 0.18f);
                var z = Next(random, -0.65f, 0.25f);
                var size = Next(random, 0.28f, 0.72f);
                CreateCube(root, "FoamVoxel_" + index, new Vector3(x, y, z), new Vector3(size * 1.4f, size, size), index % 4 == 0 ? top : foam, Quaternion.Euler(Next(random, -18f, 18f), Next(random, 0f, 90f), Next(random, -18f, 18f)));
            }
        }

        private static void FlipLuckyCubeQuestionOrientation()
        {
            const string prefabPath = "Assets/Games/KickLuckyCube/Prefabs/KLC_LuckyCube.prefab";
            var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (prefabRoot != null && prefabRoot.transform.Find(LuckyCubeQuestionFlipMarkerName) == null)
                {
                    FlipQuestionGroups(prefabRoot.transform);
                    var marker = new GameObject(LuckyCubeQuestionFlipMarkerName);
                    marker.transform.SetParent(prefabRoot.transform, false);
                    marker.SetActive(false);
                    PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                }
            }
            finally
            {
                if (prefabRoot != null)
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }

            var sceneRoots = UnityEngine.Object.FindObjectsByType<Transform>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Where(transform => transform.name == "KLC_LuckyCube")
                .ToArray();

            foreach (var root in sceneRoots)
            {
                if (root.Find(LuckyCubeQuestionFlipMarkerName) != null)
                {
                    continue;
                }

                FlipQuestionGroups(root);
                var marker = new GameObject(LuckyCubeQuestionFlipMarkerName);
                marker.transform.SetParent(root, false);
                marker.SetActive(false);
                EditorUtility.SetDirty(root.gameObject);
            }
        }

        private static void FlipQuestionGroups(Transform root)
        {
            foreach (var group in root.GetComponentsInChildren<Transform>(true)
                         .Where(IsLuckyCubeQuestionGroupRoot))
            {
                foreach (Transform child in group)
                {
                    if (!child.name.StartsWith(group.name + "_", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    var localPosition = child.localPosition;
                    localPosition.x = -localPosition.x;
                    child.localPosition = localPosition;
                    EditorUtility.SetDirty(child.gameObject);
                }
            }
        }

        private static bool IsLuckyCubeQuestionGroupRoot(Transform transform)
        {
            if (transform == null || !transform.name.StartsWith("KLC_LuckyCube_QuestionBlocks_", StringComparison.Ordinal))
            {
                return false;
            }

            return transform.Cast<Transform>().Any(child => child.name.StartsWith(transform.name + "_", StringComparison.Ordinal));
        }

        private static Transform EnsureVisualRoot()
        {
            var root = GameObject.Find("KLC_ProBuilderVisuals");
            if (root == null)
            {
                root = new GameObject("KLC_ProBuilderVisuals");
            }

            return root.transform;
        }

        private static float ResolveGroundY(float z)
        {
            var floor = GameObject.Find("Floor_FlatGreenGrass");
            if (floor == null)
            {
                return -3.62f;
            }

            var renderer = floor.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.max.y + 0.02f;
            }

            return floor.transform.position.y + 0.02f;
        }

        private static Material LoadMaterial(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>($"Assets/Games/KickLuckyCube/Art/Materials/{name}.mat");
        }

        private static void ConfigureTransparent(Material material, float alpha, float smoothness)
        {
            if (material == null)
            {
                return;
            }

            SetAlpha(material, alpha);
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)RenderQueue.Transparent;
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
        }

        private static void SetAlpha(Material material, float alpha)
        {
            if (material == null)
            {
                return;
            }

            foreach (var property in new[] { "_BaseColor", "_Color" })
            {
                if (!material.HasProperty(property))
                {
                    continue;
                }

                var color = material.GetColor(property);
                color.a = alpha;
                material.SetColor(property, color);
            }
            EditorUtility.SetDirty(material);
        }

        private static GameObject CreateCube(
            Transform parent,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Quaternion? localRotation = null)
        {
            var proBuilderMesh = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.Center);
            if (proBuilderMesh == null)
            {
                return null;
            }

            var cube = proBuilderMesh.gameObject;
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = localRotation ?? Quaternion.identity;
            cube.transform.localScale = Vector3.one;
            var positions = proBuilderMesh.positions.ToArray();
            for (var index = 0; index < positions.Length; index++)
            {
                positions[index] = Vector3.Scale(positions[index], localScale);
            }

            proBuilderMesh.positions = positions;
            proBuilderMesh.ToMesh();
            proBuilderMesh.Refresh();

            var collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            GameObjectUtility.SetStaticEditorFlags(cube, StaticEditorFlags.BatchingStatic);
            EditorUtility.SetDirty(proBuilderMesh);
            EditorUtility.SetDirty(cube);
            return cube;
        }

        private static Material FindMaterial(Transform root, string namePart)
        {
            var renderer = root.GetComponentsInChildren<Renderer>(true)
                .FirstOrDefault(candidate => candidate.name.Contains(namePart, StringComparison.Ordinal));
            return renderer != null ? renderer.sharedMaterial : null;
        }

        private static Material FindAnyMaterial(Transform root)
        {
            return root.GetComponentsInChildren<Renderer>(true)
                .Select(renderer => renderer.sharedMaterial)
                .FirstOrDefault(material => material != null);
        }

        private static void DestroyNamedChild(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }

        private static void StretchPosition(Transform target)
        {
            var position = target.position;
            position.z = KickLuckyCubeCorridorLayout.StretchLegacyDistance(position.z);
            target.position = position;
        }

        private static bool IsLongitudinalSurface(string name)
        {
            return ContainsAny(name, "_Floor_", "ColorWall", "ShoreWater", "LavaStream", "River_", "Bridge");
        }

        private static bool ContainsAny(string value, params string[] parts)
        {
            return parts.Any(part => value.Contains(part, StringComparison.OrdinalIgnoreCase));
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unnamed";
            }

            var chars = value.ToCharArray();
            for (var index = 0; index < chars.Length; index++)
            {
                if (!char.IsLetterOrDigit(chars[index]))
                {
                    chars[index] = '_';
                }
            }

            return new string(chars);
        }

        private static float Next(System.Random random, float minimum, float maximum)
        {
            return minimum + (float)random.NextDouble() * (maximum - minimum);
        }
    }
}
