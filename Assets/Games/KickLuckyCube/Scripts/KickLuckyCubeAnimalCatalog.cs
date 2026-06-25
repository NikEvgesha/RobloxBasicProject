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
            KickLuckyCubeAnimalSource source,
            string visualPrefabResourcePath = "",
            string iconResourcePath = "")
        {
            CatalogId = string.IsNullOrWhiteSpace(catalogId) ? KickLuckyCubeAnimalCatalog.MakeStableId(animalName, rarity) : catalogId;
            AnimalName = string.IsNullOrWhiteSpace(animalName) ? rarity + " Animal" : animalName;
            Rarity = rarity;
            BodyColor = bodyColor;
            SellValue = Mathf.Max(0, sellValue);
            IncomePerSecond = Mathf.Max(0, incomePerSecond);
            SpeedMultiplier = Mathf.Max(0.1f, speedMultiplier);
            Source = source;
            VisualPrefabResourcePath = visualPrefabResourcePath;
            IconResourcePath = iconResourcePath;
        }

        public string CatalogId { get; }
        public string AnimalName { get; }
        public KickLuckyCubeRarity Rarity { get; }
        public Color BodyColor { get; }
        public int SellValue { get; }
        public int IncomePerSecond { get; }
        public float SpeedMultiplier { get; }
        public KickLuckyCubeAnimalSource Source { get; }
        public string VisualPrefabResourcePath { get; }
        public string IconResourcePath { get; }
        public bool IsExclusive => Source != KickLuckyCubeAnimalSource.Kick;

        public KickLuckyCubeAnimalOption ToOption()
        {
            return ToOption(KickLuckyCubeAnimalGrade.Normal);
        }

        public KickLuckyCubeAnimalOption ToOption(KickLuckyCubeAnimalGrade grade)
        {
            return new KickLuckyCubeAnimalOption(
                Rarity,
                AnimalName,
                KickLuckyCubeAnimalGradeUtility.BlendBodyColor(BodyColor, grade),
                SellValue,
                KickLuckyCubeAnimalGradeUtility.ApplyIncomeMultiplier(IncomePerSecond, grade),
                SpeedMultiplier,
                CatalogId,
                VisualPrefabResourcePath,
                IconResourcePath,
                grade);
        }

        public KickLuckyCubeInventoryAnimal ToInventoryAnimal(string instanceId = null)
        {
            return ToInventoryAnimal(KickLuckyCubeAnimalGrade.Normal, instanceId);
        }

        public KickLuckyCubeInventoryAnimal ToInventoryAnimal(KickLuckyCubeAnimalGrade grade, string instanceId = null)
        {
            return new KickLuckyCubeInventoryAnimal(
                string.IsNullOrWhiteSpace(instanceId) ? Guid.NewGuid().ToString("N") : instanceId,
                CatalogId,
                AnimalName,
                Rarity,
                KickLuckyCubeAnimalGradeUtility.BlendBodyColor(BodyColor, grade),
                SellValue,
                KickLuckyCubeAnimalGradeUtility.ApplyIncomeMultiplier(IncomePerSecond, grade),
                grade);
        }
    }

    public static class KickLuckyCubeAnimalCatalog
    {
        private static readonly KickLuckyCubeAnimalCatalogEntry[] Entries =
        {
            new("kick_common_cat", "Cat", KickLuckyCubeRarity.Common, new Color(0.96f, 0.78f, 0.34f), 400, 20, 1f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("cat"), PetIcon("cat")),
            new("kick_common_dog", "Dog", KickLuckyCubeRarity.Common, new Color(0.72f, 0.52f, 0.32f), 420, 25, 1.02f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("dog"), PetIcon("dog")),
            new("kick_common_chicken", "Chicken", KickLuckyCubeRarity.Common, new Color(1f, 0.88f, 0.36f), 440, 30, 1.04f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("chicken"), PetIcon("chick")),
            new("kick_common_bee", "Bee", KickLuckyCubeRarity.Common, new Color(1f, 0.72f, 0.1f), 460, 35, 1.1f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("bee"), PetIcon("bee")),
            new("kick_uncommon_boar", "Pudding", KickLuckyCubeRarity.Uncommon, new Color(0.86f, 0.50f, 0.54f), 480, 40, 0.96f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Pudding"), PetIcon("pig")),
            new("kick_uncommon_fox", "Finn", KickLuckyCubeRarity.Uncommon, new Color(1f, 0.42f, 0.18f), 500, 45, 1.08f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Finn"), PetIcon("fox")),
            new("kick_uncommon_cow", "Bessie", KickLuckyCubeRarity.Uncommon, new Color(0.92f, 0.86f, 0.74f), 520, 50, 0.98f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Bessie"), PetIcon("cow")),
            new("kick_uncommon_paca", "Paca", KickLuckyCubeRarity.Uncommon, new Color(0.64f, 0.48f, 0.32f), 540, 55, 1.06f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Paca"), PetIcon("capy")),
            new("kick_rare_horse", "Champ", KickLuckyCubeRarity.Rare, new Color(0.54f, 0.32f, 0.16f), 560, 60, 1.18f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Champ"), PetIcon("horse")),
            new("kick_rare_svinina", "Svinina", KickLuckyCubeRarity.Rare, new Color(0.62f, 0.42f, 0.38f), 580, 65, 1.1f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Svinina"), PetIcon("hippo")),
            new("kick_rare_chill", "Chill", KickLuckyCubeRarity.Rare, new Color(0.58f, 0.84f, 1f), 600, 70, 1.16f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Chill"), PetIcon("sheep")),
            new("kick_rare_wolf", "Wolfle", KickLuckyCubeRarity.Rare, new Color(0.48f, 0.58f, 0.72f), 620, 75, 1.24f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Wolfle"), PetIcon("wolf")),
            new("kick_epic_deer", "Bailey", KickLuckyCubeRarity.Epic, new Color(0.74f, 0.47f, 0.22f), 640, 80, 1.28f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Bailey"), PetIcon("alpaca")),
            new("kick_epic_lion", "Stripey", KickLuckyCubeRarity.Epic, new Color(1f, 0.72f, 0.18f), 680, 85, 1.34f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Stripey"), PetIcon("tiger")),
            new("kick_epic_dragon_pup", "Gigi", KickLuckyCubeRarity.Epic, new Color(0.62f, 0.35f, 1f), 700, 90, 1.4f, KickLuckyCubeAnimalSource.Kick, ZooPrefab("Gigi"), PetIcon("giraffe")),
            new("epic_shop_crystal_griffin", "Balerina Capuchina", KickLuckyCubeRarity.Epic, new Color(0.24f, 0.86f, 1f), 3200, 115, 1.42f, KickLuckyCubeAnimalSource.EpicShop, BrainrotPrefab("balerina", "Balerina_Rig"), PetIcon("capy")),
            new("epic_shop_neon_hydra", "Tung Tung Sahur", KickLuckyCubeRarity.Legendary, new Color(0.72f, 0.22f, 1f), 5200, 185, 1.52f, KickLuckyCubeAnimalSource.EpicShop, BrainrotPrefab("TungTungSahur", "TungTungSahur_Rig"), PetIcon("horse")),
            new("epic_shop_sun_kaiju", "Glorbo Fruttodrillo", KickLuckyCubeRarity.Legendary, new Color(1f, 0.66f, 0.08f), 7600, 260, 1.58f, KickLuckyCubeAnimalSource.EpicShop, BrainrotPrefab("GlorboFruttodrillo", "GlorboFruttodrillo_Rig"), PetIcon("hippo")),
            new("elite_shop_prism_thumper", "Elite Prism Thumper", KickLuckyCubeRarity.Legendary, new Color(1f, 0.18f, 0.9f), 24000, 900, 1.64f, KickLuckyCubeAnimalSource.EpicShop, ZooPrefab("Thumper"), PetIcon("rabbit")),
            new("elite_shop_aurora_penguin", "Elite Aurora Penguin", KickLuckyCubeRarity.Legendary, new Color(0.1f, 0.92f, 1f), 42000, 1400, 1.72f, KickLuckyCubeAnimalSource.EpicShop, ZooPrefab("penguin"), PetIcon("penguin")),
            new("elite_shop_inferno_jet", "Elite Inferno Jet", KickLuckyCubeRarity.Legendary, new Color(1f, 0.1f, 0.18f), 68000, 2200, 1.86f, KickLuckyCubeAnimalSource.EpicShop, ZooPrefab("Jet"), PetIcon("chetaah")),
            new("rating_gift_star_buddy", "Star Thumper", KickLuckyCubeRarity.Epic, new Color(1f, 0.92f, 0.26f), 2400, 95, 1.34f, KickLuckyCubeAnimalSource.RatingGift, ZooPrefab("Thumper"), PetIcon("rabbit")),
        };

        private const string ResourceRoot = "KickLuckyCube/StealBrainrot/";

        public static KickLuckyCubeAnimalCatalogEntry[] GetAll()
        {
            return Entries.ToArray();
        }

        public static KickLuckyCubeAnimalOption[] CreateDefaultOptions()
        {
            return CreateKickProgressionOptions();
        }

        public static KickLuckyCubeAnimalOption[] CreateKickProgressionOptions()
        {
            var kickEntries = Entries
                .Where(entry => entry.Source == KickLuckyCubeAnimalSource.Kick)
                .ToArray();

            return KickLuckyCubeAnimalGradeUtility.ProgressionGrades
                .SelectMany((grade, gradeIndex) => kickEntries.Select((entry, entryIndex) =>
                    CreateLinearProgressionOption(entry, grade, gradeIndex * kickEntries.Length + entryIndex)))
                .ToArray();
        }

        public static KickLuckyCubeAnimalOption[] CreateLocationOptions(int locationIndex)
        {
            var progressionOptions = CreateKickProgressionOptions();
            if (progressionOptions.Length <= 3)
            {
                return progressionOptions;
            }

            var safeLocationIndex = Mathf.Max(1, locationIndex);
            var startIndex = (safeLocationIndex - 1) * 2;
            if (startIndex >= progressionOptions.Length)
            {
                return progressionOptions
                    .Skip(Mathf.Max(0, progressionOptions.Length - 3))
                    .Take(3)
                    .ToArray();
            }

            var options = progressionOptions
                .Skip(startIndex)
                .Take(3)
                .ToList();
            for (var index = progressionOptions.Length - 1; options.Count < 3 && index >= 0; index--)
            {
                var fallback = progressionOptions[index];
                if (options.Any(option => string.Equals(option.VariantId, fallback.VariantId, StringComparison.Ordinal)))
                {
                    continue;
                }

                options.Insert(0, fallback);
            }

            return options
                .OrderBy(option => option.Grade)
                .ThenBy(option => option.IncomePerSecond)
                .ThenBy(option => option.SellValue)
                .ThenBy(option => option.AnimalName, StringComparer.Ordinal)
                .ToArray();
        }

        private static KickLuckyCubeAnimalOption CreateLinearProgressionOption(
            KickLuckyCubeAnimalCatalogEntry entry,
            KickLuckyCubeAnimalGrade grade,
            int progressionIndex)
        {
            var balance = KickLuckyCubeBalanceConfig.GetOrLoadDefault();
            var income = balance.GetKickAnimalIncome(progressionIndex);
            return new KickLuckyCubeAnimalOption(
                entry.Rarity,
                entry.AnimalName,
                KickLuckyCubeAnimalGradeUtility.BlendBodyColor(entry.BodyColor, grade),
                balance.GetKickAnimalSellValue(progressionIndex, income),
                income,
                entry.SpeedMultiplier,
                entry.CatalogId,
                entry.VisualPrefabResourcePath,
                entry.IconResourcePath,
                grade);
        }

        private static KickLuckyCubeAnimalOption CreateResolvedOption(
            KickLuckyCubeAnimalCatalogEntry entry,
            KickLuckyCubeAnimalOption option)
        {
            if (entry.Source == KickLuckyCubeAnimalSource.Kick)
            {
                return CreateLinearProgressionOption(entry, option.Grade, GetLinearProgressionIndex(entry, option.Grade));
            }

            return entry.ToOption(option.Grade);
        }

        private static int GetLinearProgressionIndex(KickLuckyCubeAnimalCatalogEntry entry, KickLuckyCubeAnimalGrade grade)
        {
            var kickEntries = Entries
                .Where(candidate => candidate.Source == KickLuckyCubeAnimalSource.Kick)
                .ToArray();
            var entryIndex = Array.FindIndex(kickEntries, candidate => string.Equals(candidate.CatalogId, entry.CatalogId, StringComparison.Ordinal));
            var gradeIndex = Array.IndexOf(KickLuckyCubeAnimalGradeUtility.ProgressionGrades, grade);
            return Mathf.Max(0, gradeIndex) * kickEntries.Length + Mathf.Max(0, entryIndex);
        }

        private static KickLuckyCubeAnimalGrade GetEpicShopGrade(string catalogId)
        {
            return catalogId switch
            {
                "elite_shop_prism_thumper" => KickLuckyCubeAnimalGrade.Golden,
                "elite_shop_aurora_penguin" => KickLuckyCubeAnimalGrade.Diamond,
                "elite_shop_inferno_jet" => KickLuckyCubeAnimalGrade.Fire,
                _ => KickLuckyCubeAnimalGrade.Normal,
            };
        }

        public static KickLuckyCubeInventoryAnimal[] CreateEpicShopAnimals()
        {
            return Entries
                .Where(entry => entry.Source == KickLuckyCubeAnimalSource.EpicShop)
                .Select(entry => entry.ToInventoryAnimal(GetEpicShopGrade(entry.CatalogId), entry.CatalogId))
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
            if (!string.IsNullOrWhiteSpace(entry.CatalogId))
            {
                return true;
            }

            if (TryResolveLegacyCatalogId(catalogId, out var currentCatalogId))
            {
                entry = Entries.FirstOrDefault(candidate => string.Equals(candidate.CatalogId, currentCatalogId, StringComparison.Ordinal));
                return !string.IsNullOrWhiteSpace(entry.CatalogId);
            }

            return false;
        }

        public static KickLuckyCubeAnimalOption ResolveOption(KickLuckyCubeAnimalOption option)
        {
            return TryFindByOption(option, out var entry)
                ? CreateResolvedOption(entry, option)
                : option;
        }

        public static KickLuckyCubeAnimalOption CreateOption(KickLuckyCubeInventoryAnimal animal, float speedMultiplier = 1f)
        {
            var visualPath = string.Empty;
            var iconPath = string.Empty;
            if (TryFindByAnimal(animal, out var entry))
            {
                visualPath = entry.VisualPrefabResourcePath;
                iconPath = entry.IconResourcePath;
            }

            return new KickLuckyCubeAnimalOption(
                animal.Rarity,
                animal.AnimalName,
                animal.BodyColor,
                animal.SellValue,
                animal.IncomePerSecond,
                speedMultiplier,
                animal.CatalogId,
                visualPath,
                iconPath,
                animal.Grade);
        }

        public static GameObject LoadVisualPrefab(KickLuckyCubeAnimalOption option)
        {
            var path = TryFindByOption(option, out var entry)
                ? entry.VisualPrefabResourcePath
                : ResolveVisualPrefabPath(option.CatalogId, option.VisualPrefabResourcePath);
            return string.IsNullOrWhiteSpace(path) ? null : Resources.Load<GameObject>(path);
        }

        public static GameObject LoadVisualPrefab(KickLuckyCubeInventoryAnimal animal)
        {
            var path = TryFindByAnimal(animal, out var entry)
                ? entry.VisualPrefabResourcePath
                : ResolveVisualPrefabPath(animal.CatalogId, string.Empty);
            return string.IsNullOrWhiteSpace(path) ? null : Resources.Load<GameObject>(path);
        }

        public static Sprite LoadIcon(KickLuckyCubeAnimalCatalogEntry entry)
        {
            return string.IsNullOrWhiteSpace(entry.IconResourcePath) ? null : Resources.Load<Sprite>(entry.IconResourcePath);
        }

        public static Sprite LoadIcon(KickLuckyCubeAnimalOption option)
        {
            var path = TryFindByOption(option, out var entry)
                ? entry.IconResourcePath
                : ResolveIconPath(option.CatalogId, option.IconResourcePath);
            return string.IsNullOrWhiteSpace(path) ? null : Resources.Load<Sprite>(path);
        }

        public static Sprite LoadIcon(KickLuckyCubeInventoryAnimal animal)
        {
            var path = TryFindByAnimal(animal, out var entry)
                ? entry.IconResourcePath
                : ResolveIconPath(animal.CatalogId, string.Empty);
            return string.IsNullOrWhiteSpace(path) ? null : Resources.Load<Sprite>(path);
        }

        public static bool TryFindByAnimal(KickLuckyCubeInventoryAnimal animal, out KickLuckyCubeAnimalCatalogEntry entry)
        {
            if (!string.IsNullOrWhiteSpace(animal.RawCatalogId) && TryFindByCatalogId(animal.RawCatalogId, out entry))
            {
                return true;
            }

            return TryFindByNameAndRarity(animal.AnimalName, animal.Rarity, out entry);
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

        private static string ResolveVisualPrefabPath(string catalogId, string fallbackPath)
        {
            if (!string.IsNullOrWhiteSpace(fallbackPath))
            {
                return fallbackPath;
            }

            return TryFindByCatalogId(catalogId, out var entry)
                ? entry.VisualPrefabResourcePath
                : string.Empty;
        }

        private static bool TryFindByOption(KickLuckyCubeAnimalOption option, out KickLuckyCubeAnimalCatalogEntry entry)
        {
            if (TryFindByCatalogId(option.CatalogId, out entry))
            {
                return true;
            }

            return TryFindByNameAndRarity(option.AnimalName, option.Rarity, out entry);
        }

        private static bool TryFindByNameAndRarity(string animalName, KickLuckyCubeRarity rarity, out KickLuckyCubeAnimalCatalogEntry entry)
        {
            entry = Entries.FirstOrDefault(candidate =>
                candidate.Rarity == rarity
                && string.Equals(candidate.AnimalName, animalName, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(entry.CatalogId))
            {
                return true;
            }

            var legacyStableId = MakeStableId(animalName, rarity);
            if (!TryResolveLegacyCatalogId(legacyStableId, out var currentCatalogId))
            {
                return false;
            }

            entry = Entries.FirstOrDefault(candidate => string.Equals(candidate.CatalogId, currentCatalogId, StringComparison.Ordinal));
            return !string.IsNullOrWhiteSpace(entry.CatalogId);
        }

        private static bool TryResolveLegacyCatalogId(string catalogId, out string currentCatalogId)
        {
            currentCatalogId = catalogId switch
            {
                "common_cat" => "kick_common_cat",
                "common_dog" => "kick_common_dog",
                "uncommon_fox" => "kick_uncommon_fox",
                "uncommon_boar" => "kick_uncommon_boar",
                "rare_wolf" => "kick_rare_wolf",
                "rare_deer" => "kick_epic_deer",
                "kick_rare_deer" => "kick_epic_deer",
                "epic_lion" => "kick_epic_lion",
                "epic_dragon_pup" => "kick_epic_dragon_pup",
                "legendary_thumper" => "elite_shop_prism_thumper",
                "kick_legendary_thumper" => "elite_shop_prism_thumper",
                "legendary_penguin" => "elite_shop_aurora_penguin",
                "legendary_phoenix" => "elite_shop_aurora_penguin",
                "kick_legendary_phoenix" => "elite_shop_aurora_penguin",
                "legendary_jet" => "elite_shop_inferno_jet",
                "legendary_lucky_beast" => "elite_shop_inferno_jet",
                "kick_legendary_lucky_beast" => "elite_shop_inferno_jet",
                "epic_crystal_griffin" => "epic_shop_crystal_griffin",
                "legendary_neon_hydra" => "epic_shop_neon_hydra",
                "legendary_sun_kaiju" => "epic_shop_sun_kaiju",
                "epic_star_review_buddy" => "rating_gift_star_buddy",
                _ => string.Empty,
            };

            return !string.IsNullOrWhiteSpace(currentCatalogId);
        }

        private static string ResolveIconPath(string catalogId, string fallbackPath)
        {
            if (!string.IsNullOrWhiteSpace(fallbackPath))
            {
                return fallbackPath;
            }

            return TryFindByCatalogId(catalogId, out var entry)
                ? entry.IconResourcePath
                : string.Empty;
        }

        private static string ZooPrefab(string assetName)
        {
            return ResourceRoot + "Models/ZOO/" + assetName;
        }

        private static string BrainrotPrefab(string folderName, string assetName)
        {
            return ResourceRoot + "Models/Brainrot/" + folderName + "/" + assetName;
        }

        private static string PetIcon(string assetName)
        {
            return ResourceRoot + "Sprites/Pets/" + assetName;
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
