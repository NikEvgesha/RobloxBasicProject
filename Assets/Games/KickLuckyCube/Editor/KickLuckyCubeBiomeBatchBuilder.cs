using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace RobloxBasicProject.Games.KickLuckyCube.Editor
{
    internal static class KickLuckyCubeBiomeBatchBuilder
    {
        private const string PrefabFolder = "Assets/Games/KickLuckyCube/Prefabs/World/Biomes";
        private const string MaterialFolder = "Assets/Games/KickLuckyCube/Art/Materials/Biomes";
        private const float LocationLength = 48.4f;
        private const float FloorCenterZ = 20f;
        private const float FloorWidth = 78f;
        private const float RiverLength = 12.6f;
        private const float RiverCenterZ = 37.9f;
        private const float WallHeight = 22f;
        private const float WallThickness = 1.5f;

        private static readonly Vector3[] ObstaclePositions =
        {
            new(0f, 0f, 4.5f),
            new(-14f, 0f, 8.5f),
            new(14f, 0f, 13f),
            new(-4f, 0f, 17.5f),
            new(18f, 0f, 21.5f),
            new(-18f, 0f, 26f),
            new(6f, 0f, 29.5f),
        };

        private static readonly Vector3[] DetailPositions =
        {
            new(-33f, 0f, 3f), new(31f, 0f, 4f), new(-24f, 0f, 7f), new(25f, 0f, 9f),
            new(-31f, 0f, 13f), new(29f, 0f, 15f), new(-24f, 0f, 19f), new(26f, 0f, 21f),
            new(-31f, 0f, 24f), new(31f, 0f, 26f), new(-25f, 0f, 29f), new(25f, 0f, 30f),
            new(-8f, 0f, 11f), new(8f, 0f, 16f), new(-10f, 0f, 23f), new(11f, 0f, 28f),
        };

        private static readonly BiomeDefinition[] Definitions =
        {
            new(6, "Oasis", "oasis", "Oasis", "Chill / Wolfle / Bailey", "Palm", "Waterfall", "RockArch", "Plant", "Jar", "Rock", "Reed"),
            new(7, "Swamp", "swamp", "Swamp", "Bailey / Stripey / Gigi", "Cypress", "Shack", "RootArch", "Log", "Stump", "Plant", "Mushroom"),
            new(8, "DarkMarsh", "dark-marsh", "DarkMarsh", "Gigi / Golden Cat / Golden Dog", "DeadTree", "Ruin", "DarkBasin", "Thorn", "Lantern", "SkullStone", "Mushroom"),
            new(9, "Jungle", "jungle", "Jungle", "Golden Dog / Golden Chicken / Golden Bee", "GiantTree", "Temple", "Canopy", "Root", "Log", "CarvedStone", "Plant"),
            new(10, "BambooValley", "bamboo-valley", "Bamboo", "Golden Bee / Golden Pudding / Golden Finn", "Bamboo", "Torii", "Footbridge", "Bamboo", "Stone", "Lantern", "Plant"),
            new(11, "Snowfield", "snowfield", "Snowfield", "Golden Finn / Golden Bessie / Golden Paca", "Fir", "Cabin", "Snowbank", "Snowman", "Log", "Fence", "IceBlock"),
            new(12, "IceCliffs", "ice-cliffs", "IceCliffs", "Golden Paca / Golden Champ / Golden Svinina", "IceCliff", "IceArch", "IceSpire", "Shard", "IceBlock", "Rope", "Snowbank"),
            new(13, "CrystalCave", "crystal-cave", "CrystalCave", "Golden Svinina / Golden Chill / Golden Wolfle", "CaveRibs", "Crystal", "MineTower", "Shard", "Cart", "Rail", "Lantern"),
            new(14, "MushroomForest", "mushroom-forest", "Mushroom", "Golden Wolfle / Golden Bailey / Golden Stripey", "Mushroom", "HollowTree", "FungalArch", "Mushroom", "Log", "Plant", "Stone"),
            new(15, "AutumnGrove", "autumn-grove", "Autumn", "Golden Stripey / Golden Gigi / Diamond Cat", "Maple", "Gazebo", "CoveredBridge", "Pumpkin", "Log", "Basket", "Stone"),
            new(16, "Savanna", "savanna", "Savanna", "Diamond Cat / Diamond Dog / Diamond Chicken", "Acacia", "TermiteMound", "RockOutcrop", "Bone", "Shrub", "CrackedPlate", "Stone"),
            new(17, "Badlands", "badlands", "Badlands", "Diamond Chicken / Diamond Bee / Diamond Pudding", "Mesa", "Hoodoo", "MineTower", "Ore", "Cart", "Rail", "Fence"),
            new(18, "Volcano", "volcano", "Volcano", "Diamond Pudding / Diamond Finn / Diamond Bessie", "Volcano", "BasaltGate", "SmokeTower", "Obsidian", "LavaPlate", "Boulder", "Fence", true),
            new(19, "LavaRiver", "lava-river", "LavaRiver", "Diamond Bessie / Diamond Paca / Diamond Champ", "LavaFall", "BasaltGate", "BasaltColumn", "Obsidian", "Chain", "LavaPlate", "Post", true),
            new(20, "AshWastes", "ash-wastes", "AshWastes", "Diamond Champ / Diamond Svinina / Diamond Chill", "DeadTree", "Furnace", "CollapsedTower", "AshPile", "Log", "Chain", "Shrub"),
            new(21, "Beach", "beach", "Beach", "Diamond Chill / Diamond Wolfle / Diamond Bailey", "Palm", "Lighthouse", "RockArch", "Sandcastle", "Driftwood", "Umbrella", "Coral"),
            new(22, "PirateCove", "pirate-cove", "PirateCove", "Diamond Bailey / Diamond Stripey / Diamond Gigi", "Ship", "SkullGate", "Mast", "Chest", "Barrel", "Crate", "Cannon"),
            new(23, "CandyLand", "candy-land", "Candy", "Diamond Gigi / Fire Cat / Fire Dog", "CandyCastle", "Lollipop", "ChocolateFall", "Gumdrop", "CandyCane", "Wafer", "Sweet"),
            new(24, "ToyCity", "toy-city", "ToyCity", "Fire Dog / Fire Chicken / Fire Bee", "BlockCity", "Train", "Robot", "Block", "Domino", "Cone", "Puzzle"),
            new(25, "NeonCity", "neon-city", "NeonCity", "Fire Bee / Fire Pudding / Fire Finn", "Skyscraper", "HoloGate", "ElevatedRail", "Barrier", "Crate", "Screen", "Vent"),
            new(26, "SkyIsles", "sky-isles", "SkyIsles", "Fire Finn / Fire Bessie / Fire Paca", "FloatingIsland", "CloudArch", "Windmill", "CloudRock", "Banner", "Feather", "Stone"),
            new(27, "MagicRuins", "magic-ruins", "MagicRuins", "Fire Paca / Fire Champ / Fire Svinina", "Temple", "RunePillar", "Portal", "Column", "Book", "Crystal", "Tile"),
            new(28, "MoonCrater", "moon-crater", "Moon", "Fire Svinina / Fire Chill / Fire Wolfle", "Crater", "MoonModule", "Monolith", "MoonRock", "Flag", "Crate", "Antenna"),
            new(29, "CosmicRift", "cosmic-rift", "Cosmic", "Fire Wolfle / Fire Bailey / Fire Stripey", "RiftPortal", "AsteroidArch", "FloatingPlatform", "Shard", "Meteor", "Ring", "Cable"),
            new(30, "DivineGarden", "divine-garden", "Divine", "Fire Bailey / Fire Stripey / Fire Gigi", "CelestialTree", "MarbleTemple", "GoldenGate", "Fountain", "Hedge", "Statue", "Flower"),
        };

        [MenuItem("Tools/Kick Lucky Cube/Biomes/Build Remaining 06-30")]
        private static void BuildRemainingMenu()
        {
            Debug.Log(BuildRange(6, 30));
        }

        public static string BuildRange(int firstLocation, int lastLocation)
        {
            var corridor = UnityEngine.Object.FindFirstObjectByType<KickLuckyCubeCorridorAuthoring>();
            if (corridor == null)
            {
                throw new InvalidOperationException("KickLuckyCubeCorridorAuthoring was not found in the active scene.");
            }

            corridor.AutoBind();
            var previewRoot = corridor.PreviewRoot;
            if (previewRoot == null)
            {
                throw new InvalidOperationException("AuthoringBiomeInstances was not found.");
            }

            var definitions = Definitions
                .Where(definition => definition.Index >= firstLocation && definition.Index <= lastLocation)
                .OrderBy(definition => definition.Index)
                .ToArray();
            if (definitions.Length == 0)
            {
                return $"No biome definitions in range {firstLocation}-{lastLocation}.";
            }

            var corridorObject = new SerializedObject(corridor);
            var prefabSlots = corridorObject.FindProperty("biomePrefabs");
            if (prefabSlots.arraySize < 30)
            {
                prefabSlots.arraySize = 30;
            }

            foreach (var definition in definitions)
            {
                var prefab = BuildPrefab(definition);
                prefabSlots.GetArrayElementAtIndex(definition.Index - 1).objectReferenceValue = prefab;
                ReplacePreview(corridor, previewRoot, definition, prefab);
            }

            corridorObject.ApplyModifiedPropertiesWithoutUndo();
            if (corridor.FirstLocationAnchor != null)
            {
                var anchor = corridor.FirstLocationAnchor.localPosition;
                anchor.y = 0f;
                corridor.FirstLocationAnchor.localPosition = anchor;
                EditorUtility.SetDirty(corridor.FirstLocationAnchor);
            }

            EditorUtility.SetDirty(corridor);
            EditorSceneManager.MarkSceneDirty(corridor.gameObject.scene);
            AssetDatabase.SaveAssets();
            return $"Built {definitions.Length} biome prefabs ({definitions.First().Index:00}-{definitions.Last().Index:00}).";
        }

        private static GameObject BuildPrefab(BiomeDefinition definition)
        {
            var materials = LoadMaterialSet(definition);
            var root = CreateContractRoot(definition, materials, out var landmarks, out var details, out var vfx);
            AddBiomeWalls(root.transform, materials);
            AddLandmark(landmarks, definition.LandmarkA, new Vector3(-28f, 0f, 8f), 1.15f, materials, 0);
            AddLandmark(landmarks, definition.LandmarkB, new Vector3(28f, 0f, 17f), 1f, materials, 1);
            AddLandmark(landmarks, definition.LandmarkC, new Vector3(-29f, 0f, 28f), 0.9f, materials, 2);

            var obstacleTypes = new[] { definition.ObstacleA, definition.ObstacleB, definition.ObstacleC, definition.DetailType };
            for (var index = 0; index < ObstaclePositions.Length; index++)
            {
                AddObstacle(
                    details,
                    obstacleTypes[index % obstacleTypes.Length],
                    ObstaclePositions[index],
                    1f + (index % 3) * 0.08f,
                    materials,
                    index);
            }

            for (var index = 0; index < DetailPositions.Length; index++)
            {
                AddDetail(details, definition.DetailType, DetailPositions[index], materials, index);
            }

            AddRiver(details, materials, definition.UseLavaRiver);
            CreateAnchor(vfx, $"VFX_{definition.Suffix}_Ambient", new Vector3(0f, 2f, 18f));
            CreateAnchor(vfx, $"VFX_{definition.Suffix}_Landmark", new Vector3(-22f, 5f, 12f));
            CreateAnchor(vfx, $"VFX_{definition.Suffix}_River", new Vector3(18f, 1f, RiverCenterZ));

            var path = $"{PrefabFolder}/KLC_Biome_{definition.Index:00}_{definition.Suffix}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void ReplacePreview(
            KickLuckyCubeCorridorAuthoring corridor,
            Transform previewRoot,
            BiomeDefinition definition,
            GameObject prefab)
        {
            var previewName = $"BiomePreview_{definition.Index:00}_{definition.Suffix}";
            var oldPreview = GameObject.Find(previewName);
            if (oldPreview != null)
            {
                UnityEngine.Object.DestroyImmediate(oldPreview);
            }

            var preview = (GameObject)PrefabUtility.InstantiatePrefab(prefab, previewRoot);
            preview.name = previewName;
            var anchor = corridor.FirstLocationAnchor != null
                ? corridor.FirstLocationAnchor.localPosition
                : new Vector3(0f, 0f, 7f);
            preview.transform.localPosition = new Vector3(
                anchor.x,
                0f,
                anchor.z + (definition.Index - 1) * corridor.LocationSpacing);
            preview.transform.localRotation = Quaternion.identity;
            preview.transform.localScale = Vector3.one;
            PrefabUtility.RecordPrefabInstancePropertyModifications(preview.transform);
        }

        private static GameObject CreateContractRoot(
            BiomeDefinition definition,
            MaterialSet materials,
            out Transform landmarks,
            out Transform details,
            out Transform vfx)
        {
            var root = new GameObject($"KLC_Biome_{definition.Index:00}_{definition.Suffix}");
            var authoring = root.AddComponent<KickLuckyCubeBiomeAuthoring>();

            var floor = Box(
                root.transform,
                "GameplayFloor",
                new Vector3(0f, -0.5f, FloorCenterZ),
                new Vector3(FloorWidth, 1f, LocationLength),
                materials.Floor,
                false);
            var floorCollider = floor.gameObject.AddComponent<BoxCollider>();
            floorCollider.center = Vector3.zero;
            floorCollider.size = Vector3.one;
            floor.gameObject.AddComponent<KickLuckyCubeGroundSurface>();

            var start = CreateAnchor(root.transform, "StartAnchor", new Vector3(0f, 0f, -4.2f));
            var end = CreateAnchor(root.transform, "EndAnchor", new Vector3(0f, 0f, 44.2f));
            landmarks = CreateAnchor(root.transform, "Landmarks", Vector3.zero);
            details = CreateAnchor(root.transform, "Details", Vector3.zero);
            vfx = CreateAnchor(root.transform, "VFX", Vector3.zero);

            var rarityObject = new GameObject("RarityZone");
            rarityObject.layer = 2;
            rarityObject.transform.SetParent(root.transform, false);
            rarityObject.transform.localPosition = new Vector3(0f, 2.5f, FloorCenterZ);
            var rarityCollider = rarityObject.AddComponent<BoxCollider>();
            rarityCollider.isTrigger = true;
            rarityCollider.size = new Vector3(120f, 5f, 40f);
            var rarityZone = rarityObject.AddComponent<KickLuckyCubeRarityZone>();

            var dataZone = UnityEngine.Object.FindObjectsByType<KickLuckyCubeRarityZone>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault(zone => zone.name == $"DATA_RarityZone_{definition.Index:00}");
            var rarityIndex = dataZone != null ? (int)dataZone.Rarity : 0;

            var rarityObjectData = new SerializedObject(rarityZone);
            rarityObjectData.FindProperty("rarity").enumValueIndex = rarityIndex;
            rarityObjectData.FindProperty("zoneIndex").intValue = definition.Index;
            rarityObjectData.FindProperty("animalPoolText").stringValue = definition.AnimalPool;
            rarityObjectData.FindProperty("zoneRenderer").objectReferenceValue = null;
            rarityObjectData.ApplyModifiedPropertiesWithoutUndo();

            var authoringData = new SerializedObject(authoring);
            authoringData.FindProperty("biomeId").stringValue = definition.BiomeId;
            authoringData.FindProperty("locationIndex").intValue = definition.Index;
            authoringData.FindProperty("sceneGuideColor").colorValue = materials.GuideColor;
            authoringData.FindProperty("rarityZone").objectReferenceValue = rarityZone;
            authoringData.FindProperty("gameplayFloor").objectReferenceValue = floorCollider;
            authoringData.FindProperty("startAnchor").objectReferenceValue = start;
            authoringData.FindProperty("endAnchor").objectReferenceValue = end;
            authoringData.FindProperty("landmarksRoot").objectReferenceValue = landmarks;
            authoringData.FindProperty("detailsRoot").objectReferenceValue = details;
            authoringData.FindProperty("vfxRoot").objectReferenceValue = vfx;
            authoringData.FindProperty("decorationMayBeNonColliding").boolValue = true;
            authoringData.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static MaterialSet LoadMaterialSet(BiomeDefinition definition)
        {
            var floor = LoadMaterial($"{MaterialFolder}/KLC_Biome_{definition.MaterialStem}_Floor.mat");
            var wall = LoadMaterial($"{MaterialFolder}/KLC_Biome_{definition.MaterialStem}_Wall.mat");
            var accent = LoadMaterial($"{MaterialFolder}/KLC_Biome_{definition.MaterialStem}_Accent.mat");
            var detail = LoadMaterial($"{MaterialFolder}/KLC_Biome_{definition.MaterialStem}_Detail.mat");
            var water = definition.UseLavaRiver
                ? LoadMaterial($"{MaterialFolder}/KLC_Biome_Lava.mat")
                : LoadMaterial($"{MaterialFolder}/KLC_Biome_RiverWater.mat");
            var foam = definition.UseLavaRiver
                ? accent
                : LoadMaterial($"{MaterialFolder}/KLC_Biome_RiverFoam.mat");
            return new MaterialSet(
                floor,
                wall,
                accent,
                detail,
                LoadMaterial($"{MaterialFolder}/KLC_Biome_Stone.mat"),
                LoadMaterial("Assets/Games/KickLuckyCube/Art/Materials/KLC_PB_Wood.mat"),
                LoadMaterial("Assets/Games/KickLuckyCube/Art/Materials/KLC_PB_WoodLight.mat"),
                water,
                foam,
                floor.color);
        }

        private static Material LoadMaterial(string path)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                throw new InvalidOperationException("Missing biome material: " + path);
            }

            return material;
        }

        private static void AddBiomeWalls(Transform root, MaterialSet materials)
        {
            var walls = CreateAnchor(root, "Walls", Vector3.zero);
            var x = FloorWidth * 0.5f + WallThickness * 0.5f;
            Box(walls, "BiomeWall_Left", new Vector3(-x, WallHeight * 0.5f, FloorCenterZ), new Vector3(WallThickness, WallHeight, LocationLength), materials.Wall, true);
            Box(walls, "BiomeWall_Right", new Vector3(x, WallHeight * 0.5f, FloorCenterZ), new Vector3(WallThickness, WallHeight, LocationLength), materials.Wall, true);
            Box(walls, "BiomeWall_Left_TopCap", new Vector3(-x, WallHeight + 0.3f, FloorCenterZ), new Vector3(WallThickness + 0.5f, 0.6f, LocationLength), materials.Accent, false);
            Box(walls, "BiomeWall_Right_TopCap", new Vector3(x, WallHeight + 0.3f, FloorCenterZ), new Vector3(WallThickness + 0.5f, 0.6f, LocationLength), materials.Accent, false);
        }

        private static void AddLandmark(
            Transform parent,
            string type,
            Vector3 position,
            float scale,
            MaterialSet materials,
            int variant)
        {
            var root = CreateAnchor(parent, "Landmark_" + type, position);
            switch (type)
            {
                case "Palm":
                    AddTree(root, scale, materials, true, false);
                    break;
                case "Cypress":
                case "GiantTree":
                case "Maple":
                case "Acacia":
                case "CelestialTree":
                    AddTree(root, scale * 1.15f, materials, false, type == "CelestialTree");
                    break;
                case "Fir":
                case "Bamboo":
                case "DeadTree":
                case "Canopy":
                    AddVerticalGrove(root, scale, materials, type);
                    break;
                case "Mushroom":
                case "Lollipop":
                    AddCapLandmark(root, scale, materials, type == "Lollipop");
                    break;
                case "Crystal":
                case "IceSpire":
                case "BasaltColumn":
                    AddSpireCluster(root, scale, materials);
                    break;
                case "Mesa":
                case "IceCliff":
                case "Snowbank":
                case "RockOutcrop":
                case "FloatingIsland":
                case "Crater":
                    AddLayeredMass(root, scale, materials, type == "FloatingIsland");
                    break;
                case "Volcano":
                    AddVolcano(root, scale, materials);
                    break;
                case "Ship":
                case "Train":
                case "MoonModule":
                    AddVehicle(root, scale, materials, type);
                    break;
                case "Robot":
                    AddRobot(root, scale, materials);
                    break;
                case "Lighthouse":
                case "MineTower":
                case "SmokeTower":
                case "Monolith":
                case "Mast":
                case "Windmill":
                case "Furnace":
                case "CollapsedTower":
                case "BlockCity":
                case "Skyscraper":
                    AddTower(root, scale, materials, type, variant);
                    break;
                case "Temple":
                case "MarbleTemple":
                case "CandyCastle":
                case "Ruin":
                case "Shack":
                case "Cabin":
                case "Gazebo":
                    AddBuilding(root, scale, materials, type);
                    break;
                case "Portal":
                case "RiftPortal":
                case "HoloGate":
                case "GoldenGate":
                case "Torii":
                case "BasaltGate":
                case "SkullGate":
                    AddGate(root, scale, materials, type);
                    break;
                case "RockArch":
                case "RootArch":
                case "IceArch":
                case "FungalArch":
                case "AsteroidArch":
                case "CloudArch":
                case "CaveRibs":
                    AddArch(root, scale, materials);
                    break;
                case "Waterfall":
                case "LavaFall":
                case "ChocolateFall":
                case "DarkBasin":
                    AddFall(root, scale, materials);
                    break;
                case "Footbridge":
                case "CoveredBridge":
                case "ElevatedRail":
                case "FloatingPlatform":
                    AddElevatedStructure(root, scale, materials);
                    break;
                default:
                    AddLayeredMass(root, scale, materials, false);
                    break;
            }
        }

        private static void AddTree(Transform root, float scale, MaterialSet materials, bool palm, bool celestial)
        {
            Box(root, "Trunk", new Vector3(0f, 3.2f, 0f) * scale, new Vector3(1.8f, 6.4f, 1.8f) * scale, materials.Wood, true);
            Box(root, "Branch_Left", new Vector3(-1.5f, 5f, 0f) * scale, new Vector3(3.2f, 0.8f, 0.8f) * scale, materials.Wood, false, new Vector3(0f, 0f, 24f));
            Box(root, "Branch_Right", new Vector3(1.5f, 5.4f, 0.3f) * scale, new Vector3(3.2f, 0.8f, 0.8f) * scale, materials.Wood, false, new Vector3(0f, 15f, -22f));
            var leafMaterial = celestial ? materials.WoodLight : materials.Detail;
            var leafSize = palm ? new Vector3(4.8f, 0.8f, 1.3f) : new Vector3(3.2f, 2.5f, 3.2f);
            var offsets = palm
                ? new[] { new Vector3(-2f, 6.6f, 0f), new Vector3(2f, 6.6f, 0f), new Vector3(0f, 6.7f, 2f), new Vector3(0f, 6.7f, -2f) }
                : new[] { new Vector3(-2f, 6.8f, 0f), new Vector3(0f, 7.5f, 0f), new Vector3(2f, 6.8f, 0f), new Vector3(0f, 7f, 2f), new Vector3(0f, 7f, -2f) };
            for (var index = 0; index < offsets.Length; index++)
                Box(root, "Crown_" + index, offsets[index] * scale, leafSize * scale, index % 2 == 0 ? leafMaterial : materials.Accent, false, palm ? new Vector3(0f, index * 45f, 0f) : Vector3.zero);
        }

        private static void AddVerticalGrove(Transform root, float scale, MaterialSet materials, string type)
        {
            var count = type == "Bamboo" ? 6 : 3;
            for (var index = 0; index < count; index++)
            {
                var x = (index - (count - 1) * 0.5f) * 1.7f * scale;
                var height = (5f + index % 3 * 1.5f) * scale;
                Box(root, "Stem_" + index, new Vector3(x, height * 0.5f, (index % 2) * 0.7f), new Vector3(type == "Bamboo" ? 0.65f : 1.2f, height, type == "Bamboo" ? 0.65f : 1.2f), materials.Wood, index < 3);
                if (type != "DeadTree")
                    Box(root, "Crown_" + index, new Vector3(x, height, (index % 2) * 0.7f), new Vector3(2.6f, 1.6f, 2.6f) * scale, index % 2 == 0 ? materials.Detail : materials.Accent, false);
                else
                    Box(root, "DeadBranch_" + index, new Vector3(x + 0.8f, height * 0.75f, 0f), new Vector3(2f, 0.45f, 0.45f) * scale, materials.Wood, false, new Vector3(0f, 0f, index % 2 == 0 ? 30f : -30f));
            }
        }

        private static void AddCapLandmark(Transform root, float scale, MaterialSet materials, bool lollipop)
        {
            Box(root, "Stem", new Vector3(0f, 3f, 0f) * scale, new Vector3(1.1f, 6f, 1.1f) * scale, lollipop ? materials.WoodLight : materials.Wood, true);
            Box(root, "Cap_Center", new Vector3(0f, 6f, 0f) * scale, new Vector3(4.5f, 1.5f, 4.5f) * scale, materials.Accent, false);
            Box(root, "Cap_Left", new Vector3(-2.4f, 5.7f, 0f) * scale, new Vector3(2f, 1f, 3f) * scale, materials.Detail, false);
            Box(root, "Cap_Right", new Vector3(2.4f, 5.7f, 0f) * scale, new Vector3(2f, 1f, 3f) * scale, materials.Detail, false);
        }

        private static void AddSpireCluster(Transform root, float scale, MaterialSet materials)
        {
            for (var index = 0; index < 5; index++)
            {
                var x = (index - 2) * 1.5f * scale;
                var height = (3.5f + (index * 1.7f) % 4f) * scale;
                Box(root, "Spire_" + index, new Vector3(x, height * 0.5f, (index % 2) * 0.8f), new Vector3(1.2f, height, 1.2f) * scale, index % 2 == 0 ? materials.Accent : materials.Detail, index < 3, new Vector3(0f, 0f, index % 2 == 0 ? -8f : 8f));
            }
        }

        private static void AddLayeredMass(Transform root, float scale, MaterialSet materials, bool floating)
        {
            var y = floating ? 3f : 0f;
            Box(root, "Mass_Base", new Vector3(0f, y + 1.5f, 0f) * scale, new Vector3(8f, 3f, 6f) * scale, materials.Wall, !floating);
            Box(root, "Mass_Mid", new Vector3(0.5f, y + 4f, 0f) * scale, new Vector3(6f, 2f, 5f) * scale, materials.Detail, false);
            Box(root, "Mass_Top", new Vector3(-0.3f, y + 5.5f, 0f) * scale, new Vector3(8.5f, 1f, 6.5f) * scale, materials.Accent, false);
            if (floating)
                Box(root, "Mass_Underside", new Vector3(0f, 1.8f, 0f) * scale, new Vector3(4f, 2.2f, 3.5f) * scale, materials.Wall, false, new Vector3(0f, 0f, 45f));
        }

        private static void AddVolcano(Transform root, float scale, MaterialSet materials)
        {
            Box(root, "Volcano_Base", new Vector3(0f, 2f, 0f) * scale, new Vector3(9f, 4f, 8f) * scale, materials.Wall, true);
            Box(root, "Volcano_Mid", new Vector3(0f, 5f, 0f) * scale, new Vector3(6f, 3f, 5.5f) * scale, materials.Detail, false);
            Box(root, "Volcano_Rim", new Vector3(0f, 7f, 0f) * scale, new Vector3(4.5f, 1f, 4f) * scale, materials.Accent, false);
            Box(root, "Volcano_Glow", new Vector3(0f, 7.55f, 0f) * scale, new Vector3(2.5f, 0.2f, 2.2f) * scale, materials.River, false);
        }

        private static void AddVehicle(Transform root, float scale, MaterialSet materials, string type)
        {
            var length = type == "Ship" ? 9f : 7f;
            Box(root, "Vehicle_Body", new Vector3(0f, 1.2f, 0f) * scale, new Vector3(length, 2f, 3.5f) * scale, materials.Wood, true);
            Box(root, "Vehicle_Cabin", new Vector3(1f, 3f, 0f) * scale, new Vector3(3f, 2.2f, 2.7f) * scale, materials.Detail, false);
            Box(root, "Vehicle_Top", new Vector3(0.5f, 4.3f, 0f) * scale, new Vector3(4f, 0.5f, 3f) * scale, materials.Accent, false);
            if (type == "Ship")
                Box(root, "Vehicle_Mast", new Vector3(-1.5f, 5f, 0f) * scale, new Vector3(0.5f, 7f, 0.5f) * scale, materials.WoodLight, false);
        }

        private static void AddRobot(Transform root, float scale, MaterialSet materials)
        {
            Box(root, "Robot_Body", new Vector3(0f, 3.5f, 0f) * scale, new Vector3(4f, 4f, 3f) * scale, materials.Detail, true);
            Box(root, "Robot_Head", new Vector3(0f, 6.5f, 0f) * scale, new Vector3(3.2f, 2.2f, 2.8f) * scale, materials.Accent, false);
            Box(root, "Robot_Arm_Left", new Vector3(-3f, 3.8f, 0f) * scale, new Vector3(1.3f, 4f, 1.3f) * scale, materials.Wall, true);
            Box(root, "Robot_Arm_Right", new Vector3(3f, 3.8f, 0f) * scale, new Vector3(1.3f, 4f, 1.3f) * scale, materials.Wall, true);
        }

        private static void AddTower(Transform root, float scale, MaterialSet materials, string type, int variant)
        {
            var height = type == "Skyscraper" || type == "Lighthouse" ? 10f : 8f;
            Box(root, "Tower_Base", new Vector3(0f, height * 0.5f, 0f) * scale, new Vector3(4f + variant, height, 4f) * scale, materials.Wall, true);
            Box(root, "Tower_Mid", new Vector3(0f, height * 0.65f, -2.1f) * scale, new Vector3(2.5f, 2f, 0.35f) * scale, materials.Detail, false);
            Box(root, "Tower_Top", new Vector3(0f, height + 0.6f, 0f) * scale, new Vector3(5f + variant, 1.2f, 5f) * scale, materials.Accent, false);
            if (type == "Windmill")
            {
                Box(root, "Windmill_Blade_V", new Vector3(0f, height * 0.7f, -2.3f) * scale, new Vector3(0.5f, 7f, 0.3f) * scale, materials.WoodLight, false);
                Box(root, "Windmill_Blade_H", new Vector3(0f, height * 0.7f, -2.3f) * scale, new Vector3(7f, 0.5f, 0.3f) * scale, materials.WoodLight, false);
            }
        }

        private static void AddBuilding(Transform root, float scale, MaterialSet materials, string type)
        {
            Box(root, "Building_Base", new Vector3(0f, 2.5f, 0f) * scale, new Vector3(8f, 5f, 6f) * scale, materials.Wall, true);
            Box(root, "Building_Roof", new Vector3(0f, 5.7f, 0f) * scale, new Vector3(9f, 1.2f, 7f) * scale, materials.Accent, false);
            Box(root, "Building_Door", new Vector3(0f, 1.5f, -3.1f) * scale, new Vector3(2f, 3f, 0.3f) * scale, materials.Wood, false);
            Box(root, "Building_Trim", new Vector3(0f, 4.6f, -3.2f) * scale, new Vector3(7f, 0.5f, 0.25f) * scale, materials.Detail, false);
            if (type == "CandyCastle" || type == "MarbleTemple")
            {
                Box(root, "Building_Tower_Left", new Vector3(-3f, 6.5f, 0f) * scale, new Vector3(1.8f, 5f, 1.8f) * scale, materials.Detail, false);
                Box(root, "Building_Tower_Right", new Vector3(3f, 6.5f, 0f) * scale, new Vector3(1.8f, 5f, 1.8f) * scale, materials.Detail, false);
            }
        }

        private static void AddGate(Transform root, float scale, MaterialSet materials, string type)
        {
            Box(root, "Gate_Left", new Vector3(-3f, 3f, 0f) * scale, new Vector3(1.5f, 6f, 2f) * scale, materials.Wall, true);
            Box(root, "Gate_Right", new Vector3(3f, 3f, 0f) * scale, new Vector3(1.5f, 6f, 2f) * scale, materials.Wall, true);
            Box(root, "Gate_Top", new Vector3(0f, 6.2f, 0f) * scale, new Vector3(7.5f, 1.2f, 2.2f) * scale, materials.Accent, false);
            Box(root, "Gate_Glow", new Vector3(0f, 3.2f, 0f) * scale, new Vector3(4.5f, 4.5f, 0.3f) * scale, materials.Detail, false);
        }

        private static void AddArch(Transform root, float scale, MaterialSet materials)
        {
            Box(root, "Arch_Left", new Vector3(-2.7f, 3f, 0f) * scale, new Vector3(2f, 6f, 2.5f) * scale, materials.Wall, true);
            Box(root, "Arch_Right", new Vector3(2.7f, 3f, 0f) * scale, new Vector3(2f, 6f, 2.5f) * scale, materials.Wall, true);
            Box(root, "Arch_Top", new Vector3(0f, 6.2f, 0f) * scale, new Vector3(7f, 1.6f, 2.7f) * scale, materials.Accent, false);
        }

        private static void AddFall(Transform root, float scale, MaterialSet materials)
        {
            Box(root, "Fall_Cliff", new Vector3(0f, 4f, 1.5f) * scale, new Vector3(7f, 8f, 3f) * scale, materials.Wall, true);
            Box(root, "Fall_Stream", new Vector3(0f, 4f, -0.15f) * scale, new Vector3(3f, 7f, 0.3f) * scale, materials.River, false);
            Box(root, "Fall_Basin", new Vector3(0f, 0.2f, -2f) * scale, new Vector3(7f, 0.3f, 5f) * scale, materials.Accent, false);
        }

        private static void AddElevatedStructure(Transform root, float scale, MaterialSet materials)
        {
            Box(root, "Support_Left", new Vector3(-3f, 2f, 0f) * scale, new Vector3(1f, 4f, 1f) * scale, materials.Wood, true);
            Box(root, "Support_Right", new Vector3(3f, 2f, 0f) * scale, new Vector3(1f, 4f, 1f) * scale, materials.Wood, true);
            Box(root, "Deck", new Vector3(0f, 4f, 0f) * scale, new Vector3(9f, 0.8f, 3.5f) * scale, materials.Accent, false);
            Box(root, "Rail", new Vector3(0f, 5f, -1.6f) * scale, new Vector3(9f, 0.4f, 0.4f) * scale, materials.Detail, false);
        }

        private static void AddObstacle(
            Transform parent,
            string type,
            Vector3 position,
            float scale,
            MaterialSet materials,
            int variant)
        {
            var root = CreateAnchor(parent, $"Obstacle_{type}_{variant:00}", position);
            root.localRotation = Quaternion.Euler(0f, (variant % 2 == 0 ? 1f : -1f) * (8f + variant * 4f), 0f);
            var lowerType = type.ToLowerInvariant();

            if (lowerType.Contains("log") || lowerType.Contains("rail") || lowerType.Contains("rope") || lowerType.Contains("chain") || lowerType.Contains("cable") || lowerType.Contains("driftwood"))
            {
                Box(root, "Bar_A", new Vector3(0f, 0.9f, 0f) * scale, new Vector3(7f, 1.1f, 1.1f) * scale, materials.Wood, true);
                Box(root, "Bar_B", new Vector3(0.7f, 1.8f, 0.3f) * scale, new Vector3(5.8f, 0.7f, 0.7f) * scale, materials.WoodLight, true, new Vector3(0f, 4f, 0f));
                return;
            }

            if (lowerType.Contains("plant") || lowerType.Contains("reed") || lowerType.Contains("shrub") || lowerType.Contains("thorn") || lowerType.Contains("coral") || lowerType.Contains("flower") || lowerType.Contains("hedge") || lowerType.Contains("bamboo"))
            {
                Box(root, "Plant_Stem", new Vector3(0f, 1.5f, 0f) * scale, new Vector3(1f, 3f, 1f) * scale, materials.Detail, true);
                Box(root, "Plant_Left", new Vector3(-1.4f, 1.7f, 0f) * scale, new Vector3(2.8f, 0.8f, 1f) * scale, materials.Accent, true, new Vector3(0f, 0f, 25f));
                Box(root, "Plant_Right", new Vector3(1.4f, 1.4f, 0.3f) * scale, new Vector3(2.8f, 0.8f, 1f) * scale, materials.Accent, true, new Vector3(0f, 0f, -25f));
                return;
            }

            if (lowerType.Contains("cart") || lowerType.Contains("crate") || lowerType.Contains("chest") || lowerType.Contains("jar") || lowerType.Contains("barrel") || lowerType.Contains("basket") || lowerType.Contains("furnace") || lowerType.Contains("cannon"))
            {
                Box(root, "Container_Main", new Vector3(0f, 1f, 0f) * scale, new Vector3(4.5f, 2f, 3f) * scale, materials.Wood, true);
                Box(root, "Container_Lid", new Vector3(0f, 2.15f, 0f) * scale, new Vector3(4.8f, 0.35f, 3.3f) * scale, materials.Accent, false);
                Box(root, "Container_Trim", new Vector3(0f, 1f, -1.6f) * scale, new Vector3(3.5f, 0.4f, 0.25f) * scale, materials.Detail, false);
                return;
            }

            if (lowerType.Contains("mushroom") || lowerType.Contains("lollipop") || lowerType.Contains("umbrella") || lowerType.Contains("pumpkin") || lowerType.Contains("gumdrop") || lowerType.Contains("sweet"))
            {
                Box(root, "Stem", new Vector3(0f, 1.2f, 0f) * scale, new Vector3(0.9f, 2.4f, 0.9f) * scale, materials.WoodLight, true);
                Box(root, "Cap", new Vector3(0f, 2.7f, 0f) * scale, new Vector3(4f, 1.2f, 3.5f) * scale, materials.Accent, true);
                return;
            }

            if (lowerType.Contains("fence") || lowerType.Contains("post") || lowerType.Contains("lantern") || lowerType.Contains("flag") || lowerType.Contains("antenna") || lowerType.Contains("column"))
            {
                Box(root, "Post_Left", new Vector3(-2.5f, 1.5f, 0f) * scale, new Vector3(0.6f, 3f, 0.6f) * scale, materials.Wood, true);
                Box(root, "Post_Right", new Vector3(2.5f, 1.5f, 0f) * scale, new Vector3(0.6f, 3f, 0.6f) * scale, materials.Wood, true);
                Box(root, "Crossbar", new Vector3(0f, 1.5f, 0f) * scale, new Vector3(5f, 0.5f, 0.5f) * scale, materials.Accent, true);
                return;
            }

            AddBlockCluster(root, scale, materials, variant);
        }

        private static void AddBlockCluster(Transform root, float scale, MaterialSet materials, int variant)
        {
            Box(root, "Block_Main", new Vector3(0f, 1.1f, 0f) * scale, new Vector3(4f, 2.2f, 3.3f) * scale, materials.Wall, true, new Vector3(0f, 0f, variant % 2 == 0 ? 6f : -6f));
            Box(root, "Block_Side", new Vector3(2.4f, 0.65f, 0.4f) * scale, new Vector3(2f, 1.3f, 1.8f) * scale, materials.Detail, true, new Vector3(0f, 18f, -5f));
            Box(root, "Block_Top", new Vector3(-0.5f, 2.4f, 0f) * scale, new Vector3(2f, 0.7f, 2f) * scale, materials.Accent, false);
        }

        private static void AddDetail(Transform parent, string type, Vector3 position, MaterialSet materials, int variant)
        {
            var lowerType = type.ToLowerInvariant();
            if (lowerType.Contains("plant") || lowerType.Contains("reed") || lowerType.Contains("flower") || lowerType.Contains("feather"))
            {
                Box(parent, $"Detail_{type}_Stem_{variant:00}", position + new Vector3(0f, 0.35f, 0f), new Vector3(0.12f, 0.7f, 0.12f), materials.Detail, false, new Vector3(0f, 0f, variant % 2 == 0 ? 12f : -12f));
                Box(parent, $"Detail_{type}_Top_{variant:00}", position + new Vector3(0f, 0.8f, 0f), new Vector3(0.5f, 0.3f, 0.5f), materials.Accent, false, new Vector3(0f, 45f, 0f));
                return;
            }

            Box(parent, $"Detail_{type}_{variant:00}", position + new Vector3(0f, 0.22f, 0f), new Vector3(0.65f, 0.44f, 0.6f), variant % 2 == 0 ? materials.Detail : materials.Accent, false, new Vector3(0f, variant * 23f, variant % 2 == 0 ? 5f : -5f));
        }

        private static void AddRiver(Transform parent, MaterialSet materials, bool useLava)
        {
            var river = CreateAnchor(parent, "Transition_RiverTraversal", new Vector3(0f, 0f, RiverCenterZ));
            var trigger = river.gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0.35f, 0f);
            trigger.size = new Vector3(FloorWidth, 2.2f, RiverLength);
            var traversal = river.gameObject.AddComponent<KickLuckyCubeRiverTraversalZone>();

            Box(river, useLava ? "Transition_LavaWater" : "Transition_RiverWater", new Vector3(0f, 0.04f, 0f), new Vector3(FloorWidth, 0.08f, RiverLength), materials.River, false);
            Box(river, "Transition_RiverFoam_Start", new Vector3(0f, 0.09f, -RiverLength * 0.5f + 0.12f), new Vector3(FloorWidth, 0.05f, 0.24f), materials.Foam, false);
            Box(river, "Transition_RiverFoam_End", new Vector3(0f, 0.09f, RiverLength * 0.5f - 0.12f), new Vector3(FloorWidth, 0.05f, 0.24f), materials.Foam, false);
            AddBridge(river, "Left", -22f, materials);
            AddBridge(river, "Center", 0f, materials);
            AddBridge(river, "Right", 22f, materials);
            traversal.AutoBind();
        }

        private static void AddBridge(Transform river, string id, float x, MaterialSet materials)
        {
            var root = CreateAnchor(river, "Transition_Bridge_" + id, new Vector3(x, 0f, 0f));
            var main = Box(root, $"Bridge_{id}_MainPlank", new Vector3(0f, 0.24f, 0f), new Vector3(5.6f, 0.4f, RiverLength), materials.Wood, false);
            var collider = main.gameObject.AddComponent<BoxCollider>();
            collider.center = Vector3.zero;
            collider.size = Vector3.one;
            main.gameObject.AddComponent<KickLuckyCubeGroundSurface>();
            Box(root, $"Bridge_{id}_Rail_Left", new Vector3(-2.65f, 0.55f, 0f), new Vector3(0.22f, 0.55f, RiverLength), materials.Wood, false);
            Box(root, $"Bridge_{id}_Rail_Right", new Vector3(2.65f, 0.55f, 0f), new Vector3(0.22f, 0.55f, RiverLength), materials.Wood, false);
            for (var index = -4; index <= 4; index++)
                Box(root, $"Bridge_{id}_Slat_{index + 5}", new Vector3(0f, 0.47f, index * 1.42f), new Vector3(5.75f, 0.08f, 0.18f), materials.WoodLight, false);
            var bridge = root.gameObject.AddComponent<KickLuckyCubeRiverBridge>();
            bridge.AutoBind();
        }

        private static Transform CreateAnchor(Transform parent, string name, Vector3 localPosition)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = localPosition;
            return gameObject.transform;
        }

        private static ProBuilderMesh Box(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 size,
            Material material,
            bool collider,
            Vector3 rotation = default)
        {
            var mesh = ShapeGenerator.CreateShape(ShapeType.Cube, PivotLocation.Center);
            mesh.name = name;
            mesh.transform.SetParent(parent, false);
            mesh.transform.localPosition = position;
            mesh.transform.localRotation = Quaternion.Euler(rotation);
            mesh.transform.localScale = size;
            mesh.GetComponent<MeshRenderer>().sharedMaterial = material;
            mesh.ToMesh();
            mesh.Refresh();
            mesh.gameObject.isStatic = true;
            if (collider)
            {
                var box = mesh.gameObject.AddComponent<BoxCollider>();
                box.center = Vector3.zero;
                box.size = Vector3.one;
            }

            return mesh;
        }

        private readonly struct BiomeDefinition
        {
            public BiomeDefinition(
                int index,
                string suffix,
                string biomeId,
                string materialStem,
                string animalPool,
                string landmarkA,
                string landmarkB,
                string landmarkC,
                string obstacleA,
                string obstacleB,
                string obstacleC,
                string detailType,
                bool useLavaRiver = false)
            {
                Index = index;
                Suffix = suffix;
                BiomeId = biomeId;
                MaterialStem = materialStem;
                AnimalPool = animalPool;
                LandmarkA = landmarkA;
                LandmarkB = landmarkB;
                LandmarkC = landmarkC;
                ObstacleA = obstacleA;
                ObstacleB = obstacleB;
                ObstacleC = obstacleC;
                DetailType = detailType;
                UseLavaRiver = useLavaRiver;
            }

            public int Index { get; }
            public string Suffix { get; }
            public string BiomeId { get; }
            public string MaterialStem { get; }
            public string AnimalPool { get; }
            public string LandmarkA { get; }
            public string LandmarkB { get; }
            public string LandmarkC { get; }
            public string ObstacleA { get; }
            public string ObstacleB { get; }
            public string ObstacleC { get; }
            public string DetailType { get; }
            public bool UseLavaRiver { get; }
        }

        private readonly struct MaterialSet
        {
            public MaterialSet(
                Material floor,
                Material wall,
                Material accent,
                Material detail,
                Material stone,
                Material wood,
                Material woodLight,
                Material river,
                Material foam,
                Color guideColor)
            {
                Floor = floor;
                Wall = wall;
                Accent = accent;
                Detail = detail;
                Stone = stone;
                Wood = wood;
                WoodLight = woodLight;
                River = river;
                Foam = foam;
                GuideColor = new Color(guideColor.r, guideColor.g, guideColor.b, 0.55f);
            }

            public Material Floor { get; }
            public Material Wall { get; }
            public Material Accent { get; }
            public Material Detail { get; }
            public Material Stone { get; }
            public Material Wood { get; }
            public Material WoodLight { get; }
            public Material River { get; }
            public Material Foam { get; }
            public Color GuideColor { get; }
        }
    }
}
