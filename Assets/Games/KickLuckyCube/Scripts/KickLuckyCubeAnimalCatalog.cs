using System;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public enum KickLuckyCubeAnimalSource
    {
        Kick,
        EpicShop,
        RatingGift,
        Exchange
    }

    [Serializable]
    public readonly struct KickLuckyCubeAnimalCatalogEntry
    {
        public KickLuckyCubeAnimalCatalogEntry(
            string catalogId,
            string animalName,
            KickLuckyCubeRarity rarity,
            Color bodyColor,
            int sellValue,
            int incomePerSecond,
            float speedMultiplier,
            KickLuckyCubeAnimalSource source)
        {
            CatalogId = string.IsNullOrWhiteSpace(catalogId) ? KickLuckyCubeAnimalCatalog.MakeStableId(animalName, rarity) : catalogId;
            AnimalName = string.IsNullOrWhiteSpace(animalName) ? rarity + " Animal" : animalName;
            Rarity = rarity;
            BodyColor = bodyColor;
            SellValue = Mathf.Max(0, sellValue);
            IncomePerSecond = Mathf.Max(0, incomePerSecond);
            SpeedMultiplier = Mathf.Max(0.1f, speedMultiplier);
            Source = source;
        }

        public string CatalogId { get; }
        public string AnimalName { get; }
        public KickLuckyCubeRarity Rarity { get; }
        public Color BodyColor { get; }
        public int SellValue { get; }
        public int IncomePerSecond { get; }
        public float SpeedMultiplier { get; }
        public KickLuckyCubeAnimalSource Source { get; }
        public bool IsExclusive => Source != KickLuckyCubeAnimalSource.Kick;

        public KickLuckyCubeAnimalOption ToOption()
        {
            return new KickLuckyCubeAnimalOption(
                Rarity,
                AnimalName,
                BodyColor,
                SellValue,
                IncomePerSecond,
                SpeedMultiplier,
                CatalogId);
        }

        public KickLuckyCubeInventoryAnimal ToInventoryAnimal(string instanceId = null)
        {
            return new KickLuckyCubeInventoryAnimal(
                string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId,
                CatalogId,
                AnimalName,
                Rarity,
                BodyColor,
                SellValue,
                IncomePerSecond);
        }
    }

    public static class KickLuckyCubeAnimalCatalog
    {
        private static readonly KickLuckyCubeAnimalCatalogEntry[] Entries =
        {
            new("kick_common_cat", "Cat", KickLuckyCubeRarity.Common, new Color(0.96f, 0.78f, 0.34f), 25, 1, 1f, KickLuckyCubeAnimalSource.Kick),
            new("kick_common_dog", "Dog", KickLuckyCubeRarity.Common, new Color(0.72f, 0.52f, 0.32f), 30, 1, 1.02f, KickLuckyCubeAnimalSource.Kick),
            new("kick_uncommon_fox", "Fox", KickLuckyCubeRarity.Uncommon, new Color(1f, 0.42f, 0.18f), 70, 3, 1.08f, KickLuckyCubeAnimalSource.Kick),
            new("kick_uncommon_boar", "Boar", KickLuckyCubeRarity.Uncommon, new Color(0.45f, 0.36f, 0.28f), 82, 3, 0.96f, KickLuckyCubeAnimalSource.Kick),
            new("kick_rare_wolf", "Wolf", KickLuckyCubeRarity.Rare, new Color(0.48f, 0.58f, 0.72f), 180, 7, 1.15f, KickLuckyCubeAnimalSource.Kick),
            new("kick_rare_deer", "Deer", KickLuckyCubeRarity.Rare, new Color(0.74f, 0.47f, 0.22f), 210, 8, 1.22f, KickLuckyCubeAnimalSource.Kick),
            new("kick_epic_lion", "Lion", KickLuckyCubeRarity.Epic, new Color(1f, 0.72f, 0.18f), 520, 18, 1.28f, KickLuckyCubeAnimalSource.Kick),
            new("kick_epic_dragon_pup", "Dragon Pup", KickLuckyCubeRarity.Epic, new Color(0.62f, 0.35f, 1f), 680, 24, 1.36f, KickLuckyCubeAnimalSource.Kick),
            new("kick_legendary_phoenix", "Phoenix", KickLuckyCubeRarity.Legendary, new Color(1f, 0.24f, 0.12f), 1600, 60, 1.48f, KickLuckyCubeAnimalSource.Kick),
            new("kick_legendary_lucky_beast", "Lucky Beast", KickLuckyCubeRarity.Legendary, new Color(0.2f, 1f, 0.72f), 2200, 85, 1.55f, KickLuckyCubeAnimalSource.Kick),
            new("epic_shop_crystal_griffin", "Crystal Griffin", KickLuckyCubeRarity.Epic, new Color(0.24f, 0.86f, 1f), 3200, 115, 1.42f, KickLuckyCubeAnimalSource.EpicShop),
            new("epic_shop_neon_hydra", "Neon Hydra", KickLuckyCubeRarity.Legendary, new Color(0.72f, 0.22f, 1f), 5200, 185, 1.52f, KickLuckyCubeAnimalSource.EpicShop),
            new("epic_shop_sun_kaiju", "Sun Kaiju", KickLuckyCubeRarity.Legendary, new Color(1f, 0.66f, 0.08f), 7600, 260, 1.58f, KickLuckyCubeAnimalSource.EpicShop),
            new("rating_gift_star_buddy", "Star Review Buddy", KickLuckyCubeRarity.Epic, new Color(1f, 0.92f, 0.26f), 2400, 95, 1.34f, KickLuckyCubeAnimalSource.RatingGift),
        };

        public static KickLuckyCubeAnimalCatalogEntry[] GetAll()
        {
            return Entries.ToArray();
        }

        public static KickLuckyCubeAnimalOption[] CreateDefaultOptions()
        {
            return Entries
                .Where(entry => entry.Source == KickLuckyCubeAnimalSource.Kick)
                .Select(entry => entry.ToOption())
                .ToArray();
        }

        public static KickLuckyCubeInventoryAnimal[] CreateEpicShopAnimals()
        {
            return Entries
                .Where(entry => entry.Source == KickLuckyCubeAnimalSource.EpicShop)
                .Select(entry => entry.ToInventoryAnimal(entry.CatalogId))
                .ToArray();
        }

        public static KickLuckyCubeInventoryAnimal CreateRatingGiftAnimal()
        {
            return Entries
                .First(entry => entry.Source == KickLuckyCubeAnimalSource.RatingGift)
                .ToInventoryAnimal("rating_gift_star_buddy");
        }

        public static bool TryFindByCatalogId(string catalogId, out KickLuckyCubeAnimalCatalogEntry entry)
        {
            entry = Entries.FirstOrDefault(candidate => string.Equals(candidate.CatalogId, catalogId, StringComparison.Ordinal));
            return !string.IsNullOrWhiteSpace(entry.CatalogId);
        }

        public static bool TryFindByAnimal(KickLuckyCubeInventoryAnimal animal, out KickLuckyCubeAnimalCatalogEntry entry)
        {
            if (!string.IsNullOrWhiteSpace(animal.RawCatalogId) && TryFindByCatalogId(animal.RawCatalogId, out entry))
            {
                return true;
            }

            entry = Entries.FirstOrDefault(candidate =>
                candidate.Rarity == animal.Rarity
                && string.Equals(candidate.AnimalName, animal.AnimalName, StringComparison.OrdinalIgnoreCase));
            return !string.IsNullOrWhiteSpace(entry.CatalogId);
        }

        public static string ResolveCatalogId(KickLuckyCubeInventoryAnimal animal)
        {
            return TryFindByAnimal(animal, out var entry)
                ? entry.CatalogId
                : MakeStableId(animal.AnimalName, animal.Rarity);
        }

        public static string MakeStableId(string animalName, KickLuckyCubeRarity rarity)
        {
            var source = string.IsNullOrWhiteSpace(animalName) ? rarity.ToString() : animalName;
            var sanitized = new string(source
                .Trim()
                .ToLowerInvariant()
                .Select(ch => char.IsLetterOrDigit(ch) ? ch : '_')
                .ToArray());
            while (sanitized.IndexOf("__", StringComparison.Ordinal) >= 0)
            {
                sanitized = sanitized.Replace("__", "_");
            }

            return rarity.ToString().ToLowerInvariant() + "_" + sanitized.Trim('_');
        }
    }

    public static class KickLuckyCubeAnimalCollection
    {
        private const string DiscoveredKeyPrefix = "KickLuckyCube.AnimalCollection.Discovered.";

        public static event Action Changed;

        public static bool IsDiscovered(string catalogId)
        {
            return !string.IsNullOrWhiteSpace(catalogId)
                && PlayerPrefs.GetInt(DiscoveredKeyPrefix + catalogId, 0) != 0;
        }

        public static bool IsDiscovered(KickLuckyCubeAnimalCatalogEntry entry)
        {
            return IsDiscovered(entry.CatalogId);
        }

        public static bool MarkDiscovered(KickLuckyCubeInventoryAnimal animal)
        {
            if (!animal.IsValid)
            {
                return false;
            }

            return MarkDiscovered(KickLuckyCubeAnimalCatalog.ResolveCatalogId(animal));
        }

        public static bool MarkDiscovered(KickLuckyCubeSpawnedAnimal animal)
        {
            if (animal == null)
            {
                return false;
            }

            var catalogId = string.IsNullOrWhiteSpace(animal.CatalogId)
                ? KickLuckyCubeAnimalCatalog.MakeStableId(animal.AnimalName, animal.Rarity)
                : animal.CatalogId;
            return MarkDiscovered(catalogId);
        }

        public static bool MarkDiscovered(string catalogId)
        {
            if (string.IsNullOrWhiteSpace(catalogId) || IsDiscovered(catalogId))
            {
                return false;
            }

            PlayerPrefs.SetInt(DiscoveredKeyPrefix + catalogId, 1);
            PlayerPrefs.Save();
            Changed?.Invoke();
            return true;
        }

        public static int CountDiscovered()
        {
            return KickLuckyCubeAnimalCatalog.GetAll().Count(entry => IsDiscovered(entry));
        }

        public static void ClearForPrototype()
        {
            foreach (var entry in KickLuckyCubeAnimalCatalog.GetAll())
            {
                PlayerPrefs.DeleteKey(DiscoveredKeyPrefix + entry.CatalogId);
            }

            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
