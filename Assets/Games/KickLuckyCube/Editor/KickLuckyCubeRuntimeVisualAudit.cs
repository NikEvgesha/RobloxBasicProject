using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube.Editor
{
    public static class KickLuckyCubeRuntimeVisualAudit
    {
        private const string ScriptsFolder = "Assets/Games/KickLuckyCube/Scripts";

        private static readonly Regex ConstructionPattern = new(
            @"new\s+GameObject\s*\(|GameObject\.CreatePrimitive\s*\(|AddComponent\s*<\s*(?:Image|Text|TextMeshProUGUI|TextMesh|ParticleSystem|Light|LineRenderer|TrailRenderer|MeshRenderer|SpriteRenderer)",
            RegexOptions.Compiled);

        private static readonly string[] ManagerMarkers =
        {
            "Controller_Runtime",
            "KLC_Leaderboard_Runtime",
            "KLC_AnimalAlbum_Runtime",
            "KLC_SpeedShop_Runtime",
            "KLC_StrengthToolShop_Runtime",
            "KLC_StyleShop_Runtime",
            "KLC_StablePlotRuntimeBinder",
            "KLC_RuntimePlotInstances",
            "KLC_HomeIconMarker_Runtime",
            "new GameObject(RuntimeInstancesRootName)",
            "new GameObject(RootName).transform",
        };

        private static readonly string[] PrefabDataInstanceMarkers =
        {
            "KLC_Runner_",
            "KLC_SelectedMobHandPreview",
            "KLC_BotStableVisual_",
            "RouletteVisual_",
            "KLC_Stable_",
        };

        private static readonly HashSet<string> KnownMigrationFiles = new(StringComparer.Ordinal)
        {
            "KickLuckyCubeAnimalVisualFactory.cs",
            "KickLuckyCubeCurrencyFxController.cs",
            "KickLuckyCubeHomeIconMarker.cs",
            "KickLuckyCubeKickController.cs",
            "KickLuckyCubePlotAllocator.cs",
            "KickLuckyCubeRunPhaseController.cs",
            "KickLuckyCubeStablePlacementPad.cs",
            "KickLuckyCubeStableSlot.cs",
            "KickLuckyCubeStableUpgradeBoard.cs",
            "KickLuckyCubeToolTrainingController.cs",
            "KickLuckyCubeUiPrefabFactory.cs",
            "KickLuckyCubeUiTheme.cs",
            "KickLuckyCubeWaveChaseController.cs",
            "KickLuckyCubeWorldTextOutline.cs",
        };

        public enum Ownership
        {
            ManagerOnly,
            DynamicDataInstanceFromPrefab,
            MustMigrate,
            Unclassified,
        }

        public readonly struct Item
        {
            public Item(string path, int line, string source, Ownership ownership)
            {
                Path = path;
                Line = line;
                Source = source;
                Ownership = ownership;
            }

            public string Path { get; }
            public int Line { get; }
            public string Source { get; }
            public Ownership Ownership { get; }
        }

        [MenuItem("Tools/Kick Lucky Cube/Validation/Audit Runtime-Created Visuals")]
        public static void AuditFromMenu()
        {
            var items = ScanSource();
            var groups = items.GroupBy(item => item.Ownership).ToDictionary(group => group.Key, group => group.Count());
            Debug.Log(
                $"[KLC-PREFAB-AUDIT] scanned={items.Count}, "
                + $"manager={GetCount(groups, Ownership.ManagerOnly)}, "
                + $"prefab-data={GetCount(groups, Ownership.DynamicDataInstanceFromPrefab)}, "
                + $"must-migrate={GetCount(groups, Ownership.MustMigrate)}, "
                + $"unclassified={GetCount(groups, Ownership.Unclassified)}. "
                + "See Docs/Games/KickLuckyCubeRuntimeVisualAudit.md for ownership and migration order.");

            foreach (var item in items.Where(item => item.Ownership is Ownership.MustMigrate or Ownership.Unclassified))
            {
                Debug.LogWarning($"[KLC-PREFAB-AUDIT:{item.Ownership}] {item.Path}:{item.Line} {item.Source}");
            }

            var unknown = items.Where(item => item.Ownership == Ownership.Unclassified).ToArray();
            if (unknown.Length > 0)
            {
                throw new InvalidOperationException(
                    "Unclassified runtime visual construction was added. Classify or migrate it before commit:\n"
                    + string.Join("\n", unknown.Select(item => $"{item.Path}:{item.Line}")));
            }
        }

        [MenuItem("Tools/Kick Lucky Cube/Validation/Scan Play Mode Prefab Ownership")]
        public static void ScanPlayModeOwnership()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException("Enter Play Mode before scanning runtime prefab ownership.");
            }

            // PrefabUtility does not reliably retain source links for every object cloned
            // after entering Play Mode. Source-policy validation is deterministic and also
            // catches fallback visual construction before that code path is exercised.
            var items = ScanSource();
            var offenders = items
                .Where(item => item.Ownership is Ownership.MustMigrate or Ownership.Unclassified)
                .ToArray();
            Debug.Log(
                $"[KLC-PREFAB-AUDIT] Play Mode source-policy check scanned {items.Count} construction sites; "
                + $"visual construction offenders={offenders.Length}.");

            if (offenders.Length > 0)
            {
                throw new InvalidOperationException(
                    "Runtime visual construction must be migrated to authored prefabs:\n"
                    + string.Join("\n", offenders.Select(item => $"{item.Path}:{item.Line}")));
            }
        }

        public static IReadOnlyList<Item> ScanSource()
        {
            var absoluteFolder = Path.GetFullPath(ScriptsFolder);
            var items = new List<Item>();
            foreach (var absolutePath in Directory.EnumerateFiles(absoluteFolder, "*.cs", SearchOption.AllDirectories))
            {
                var relativePath = absolutePath.Replace('\\', '/');
                var assetsIndex = relativePath.IndexOf("Assets/", StringComparison.Ordinal);
                if (assetsIndex >= 0)
                {
                    relativePath = relativePath.Substring(assetsIndex);
                }

                var lines = File.ReadAllLines(absolutePath);
                for (var index = 0; index < lines.Length; index++)
                {
                    var source = lines[index].Trim();
                    if (!ConstructionPattern.IsMatch(source))
                    {
                        continue;
                    }

                    items.Add(new Item(relativePath, index + 1, source, Classify(absolutePath, source)));
                }
            }

            return items;
        }

        private static Ownership Classify(string absolutePath, string source)
        {
            if (ManagerMarkers.Any(source.Contains))
            {
                return Ownership.ManagerOnly;
            }

            if (PrefabDataInstanceMarkers.Any(source.Contains))
            {
                return Ownership.DynamicDataInstanceFromPrefab;
            }

            return KnownMigrationFiles.Contains(Path.GetFileName(absolutePath))
                ? Ownership.MustMigrate
                : Ownership.Unclassified;
        }

        private static int GetCount(IReadOnlyDictionary<Ownership, int> groups, Ownership ownership)
        {
            return groups.TryGetValue(ownership, out var count) ? count : 0;
        }

    }
}
