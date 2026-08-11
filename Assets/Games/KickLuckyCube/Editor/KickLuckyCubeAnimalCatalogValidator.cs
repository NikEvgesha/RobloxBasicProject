using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube.Editor
{
    public static class KickLuckyCubeAnimalCatalogValidator
    {
        [MenuItem("Tools/Kick Lucky Cube/Validation/Validate Animal Catalog")]
        public static void ValidateFromMenu()
        {
            var errors = Validate();
            if (errors.Count == 0)
            {
                Debug.Log($"[KLC-CATALOG] Validated {KickLuckyCubeAnimalCatalog.GetAll().Length} catalog entries: models, icons, ids and animation controllers are present.");
                return;
            }

            throw new InvalidOperationException("Kick Lucky Cube animal catalog validation failed:\n- " + string.Join("\n- ", errors));
        }

        public static IReadOnlyList<string> Validate()
        {
            var entries = KickLuckyCubeAnimalCatalog.GetAll();
            var errors = new List<string>();

            foreach (var duplicate in entries
                         .GroupBy(entry => entry.CatalogId, StringComparer.Ordinal)
                         .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() != 1))
            {
                errors.Add($"Catalog id '{duplicate.Key}' occurs {duplicate.Count()} times.");
            }

            foreach (var entry in entries)
            {
                ValidateEntry(entry, errors);
            }

            return errors;
        }

        private static void ValidateEntry(KickLuckyCubeAnimalCatalogEntry entry, ICollection<string> errors)
        {
            var context = $"{entry.CatalogId} ({entry.AnimalName})";
            if (string.IsNullOrWhiteSpace(entry.AnimalName))
            {
                errors.Add(context + ": display name is empty.");
            }

            if (entry.IncomePerSecond <= 0 || entry.SellValue <= 0)
            {
                errors.Add(context + ": income and sell value must both be positive.");
            }

            var option = entry.ToOption();
            var prefab = KickLuckyCubeAnimalCatalog.LoadVisualPrefab(option);
            if (prefab == null)
            {
                errors.Add(context + $": model is missing at Resources/{entry.VisualPrefabResourcePath}.");
            }
            else
            {
                var animator = prefab.GetComponentInChildren<Animator>(true);
                if (animator == null)
                {
                    errors.Add(context + $": prefab '{prefab.name}' has no Animator.");
                }
                else if (animator.runtimeAnimatorController == null)
                {
                    errors.Add(context + $": prefab '{prefab.name}' Animator has no controller.");
                }
            }

            var icon = KickLuckyCubeAnimalCatalog.LoadIcon(entry);
            if (icon == null)
            {
                errors.Add(context + $": icon is missing at Resources/{entry.IconResourcePath}.");
            }

            var roundTrip = KickLuckyCubeAnimalCatalog.ResolveOption(option);
            if (!string.Equals(roundTrip.CatalogId, entry.CatalogId, StringComparison.Ordinal)
                || !string.Equals(roundTrip.AnimalName, entry.AnimalName, StringComparison.Ordinal)
                || roundTrip.Rarity != entry.Rarity)
            {
                errors.Add(context + ": option round-trip resolves to different catalog data.");
            }

            foreach (var grade in KickLuckyCubeAnimalGradeUtility.ProgressionGrades)
            {
                var gradedOption = entry.ToOption(grade);
                var inventoryAnimal = entry.ToInventoryAnimal(grade, "validation_instance");
                var inventoryOption = KickLuckyCubeAnimalCatalog.CreateOption(inventoryAnimal);
                if (!string.Equals(gradedOption.VariantId, inventoryOption.VariantId, StringComparison.Ordinal)
                    || !string.Equals(gradedOption.AnimalName, inventoryOption.AnimalName, StringComparison.Ordinal)
                    || gradedOption.Rarity != inventoryOption.Rarity
                    || gradedOption.SellValue != inventoryOption.SellValue
                    || gradedOption.IncomePerSecond != inventoryOption.IncomePerSecond)
                {
                    errors.Add(context + $": {grade} inventory/model/icon round-trip changes canonical data.");
                }
            }
        }
    }
}
