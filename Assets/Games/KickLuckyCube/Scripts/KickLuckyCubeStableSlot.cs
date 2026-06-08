using RobloxBasicProject.GameKit.Interaction;
using System;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStableSlot : MonoBehaviour, GameKitInteractionCondition
    {
        private const string AnimalNameKey = "AnimalName";
        private const string AnimalJsonKey = "AnimalJson";
        private const string PendingSoftKey = "PendingSoft";
        private const string SavedAtKey = "SavedAt";
        private const string UpgradeLevelKey = "UpgradeLevel";

        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private Transform animalAnchor;
        [SerializeField] private TextMesh statusLabel;
        [SerializeField] private string stableSlotId;
        [SerializeField] private string saveKeyPrefix = "KickLuckyCube.Stable.";
        [SerializeField, Min(0f)] private float incomeUpdateSeconds = 1f;
        [SerializeField, Min(1f)] private float saveIntervalSeconds = 5f;
        [SerializeField, Min(0f)] private float offlineIncomeCapSeconds = 7200f;
        [SerializeField, Min(1)] private int upgradeLevel = 1;
        [SerializeField, Min(1)] private int maxUpgradeLevel = 10;
        [SerializeField, Min(0)] private int baseUpgradeCost = 150;
        [SerializeField, Min(1f)] private float upgradeCostMultiplier = 1.65f;
        [SerializeField, Min(0f)] private float incomeBonusPerLevel = 0.2f;
        [SerializeField] private bool saveInPlayerPrefs = true;

        private KickLuckyCubeSpawnedAnimal placedAnimal;
        private KickLuckyCubeInventoryAnimal placedInventoryAnimal;
        private float pendingSoft;
        private float incomeTimer;
        private float saveTimer;

        public KickLuckyCubeSpawnedAnimal PlacedAnimal => placedAnimal;
        public string StableSlotId => stableSlotId;
        public bool IsOccupied => placedAnimal != null;
        public int PendingSoft => Mathf.FloorToInt(pendingSoft);
        public int BaseIncomePerSecond => placedAnimal != null ? placedAnimal.IncomePerSecond : 0;
        public int IncomePerSecond => GetBoostedIncome(BaseIncomePerSecond);
        public int UpgradeLevel => upgradeLevel;
        public int MaxUpgradeLevel => maxUpgradeLevel;
        public float IncomeMultiplier => GetIncomeMultiplier(upgradeLevel);
        public bool CanUpgrade => upgradeLevel < maxUpgradeLevel;
        public int NextUpgradeCost => CanUpgrade ? Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, upgradeLevel - 1)) : 0;
        public float NextIncomeMultiplier => GetIncomeMultiplier(Mathf.Min(maxUpgradeLevel, upgradeLevel + 1));

        private void Awake()
        {
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>();

            if (animalAnchor == null)
            {
                animalAnchor = FindChildByNamePart("MobAnchor") ?? transform;
            }

            if (statusLabel == null)
            {
                var existingLabel = GetComponentInChildren<TextMesh>(true);
                statusLabel = existingLabel != null ? existingLabel : CreateStatusLabel();
            }

            if (string.IsNullOrWhiteSpace(stableSlotId))
            {
                stableSlotId = BuildStableSlotId();
            }

            LoadSlot();
            RefreshLabel();
        }

        private void Update()
        {
            if (placedAnimal == null || IncomePerSecond <= 0)
            {
                return;
            }

            incomeTimer += Time.deltaTime;
            if (incomeTimer < incomeUpdateSeconds)
            {
                return;
            }

            var ticks = Mathf.FloorToInt(incomeTimer / incomeUpdateSeconds);
            incomeTimer -= ticks * incomeUpdateSeconds;
            pendingSoft += IncomePerSecond * ticks;
            RefreshLabel();

            saveTimer += ticks * incomeUpdateSeconds;
            if (saveTimer >= saveIntervalSeconds)
            {
                saveTimer = 0f;
                SaveSlot();
            }
        }

        private void OnDestroy()
        {
            SaveSlot();
        }

        public bool CanInteract(GameObject actor)
        {
            return runPhase != null && runPhase.HasCarriedAnimal && placedAnimal == null;
        }

        public void Place(GameObject actor)
        {
            runPhase?.TryPlaceCarriedAnimal(this);
        }

        public bool TryPlace(KickLuckyCubeSpawnedAnimal animal)
        {
            if (animal == null || placedAnimal != null)
            {
                return false;
            }

            placedAnimal = animal;
            placedInventoryAnimal = KickLuckyCubeInventoryAnimal.FromSpawnedAnimal(animal);
            incomeTimer = 0f;
            placedAnimal.SetCarried(animalAnchor);

            var runner = placedAnimal.GetComponent<KickLuckyCubeAnimalRunner>();
            if (runner != null)
            {
                runner.StopRun();
                runner.enabled = false;
            }

            RefreshLabel();
            SaveSlot();
            return true;
        }

        public bool TryPlace(KickLuckyCubeInventoryAnimal animal)
        {
            if (!animal.IsValid || placedAnimal != null)
            {
                return false;
            }

            placedInventoryAnimal = animal;
            placedAnimal = CreateStableAnimal(animal);
            incomeTimer = 0f;
            RefreshLabel();
            SaveSlot();
            return true;
        }

        public int Collect()
        {
            var amount = PendingSoft;
            if (amount <= 0)
            {
                return 0;
            }

            pendingSoft -= amount;
            RefreshLabel();
            SaveSlot();
            return amount;
        }

        public bool TryUpgrade(KickLuckyCubeWallet wallet)
        {
            if (wallet == null || !CanUpgrade)
            {
                return false;
            }

            var cost = NextUpgradeCost;
            if (!wallet.TrySpendSoft(cost))
            {
                return false;
            }

            upgradeLevel = Mathf.Clamp(upgradeLevel + 1, 1, maxUpgradeLevel);
            RefreshLabel();
            SaveSlot();
            return true;
        }

        public void AddPendingForPrototype(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            pendingSoft += amount;
            RefreshLabel();
            SaveSlot();
        }

        public void ClearForPrototype()
        {
            if (placedAnimal != null)
            {
                DestroyAnimalObject(placedAnimal);
            }

            placedAnimal = null;
            placedInventoryAnimal = default;
            pendingSoft = 0f;
            incomeTimer = 0f;
            upgradeLevel = 1;
            RefreshLabel();
            SaveSlot();
        }

        private void RefreshLabel()
        {
            if (statusLabel == null)
            {
                return;
            }

            statusLabel.text = placedAnimal == null
                ? $"Empty stable\nLv {upgradeLevel} x{IncomeMultiplier:0.0}\nSelect mob + E"
                : $"{placedAnimal.AnimalName}\n+{IncomePerSecond}/s x{IncomeMultiplier:0.0}\nClaim: {PendingSoft}";
        }

        private static void DestroyAnimalObject(KickLuckyCubeSpawnedAnimal animal)
        {
            if (animal == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(animal.gameObject);
                return;
            }

            DestroyImmediate(animal.gameObject);
        }

        private void LoadSlot()
        {
            if (!Application.isPlaying || !saveInPlayerPrefs)
            {
                return;
            }

            LoadUpgradeLevel();

            var animalName = PlayerPrefs.GetString(GetKey(AnimalNameKey), string.Empty);
            var animalJson = PlayerPrefs.GetString(GetKey(AnimalJsonKey), string.Empty);
            if (!string.IsNullOrWhiteSpace(animalJson))
            {
                var savedAnimal = JsonUtility.FromJson<KickLuckyCubeInventoryAnimal>(animalJson);
                if (savedAnimal.IsValid)
                {
                    placedInventoryAnimal = savedAnimal;
                    placedAnimal = CreateStableAnimal(savedAnimal);
                    pendingSoft = Mathf.Max(0f, PlayerPrefs.GetFloat(GetKey(PendingSoftKey), 0f));
                    pendingSoft += IncomePerSecond * GetOfflineElapsedSeconds();
                    SaveSlot();
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(animalName))
            {
                pendingSoft = 0f;
                return;
            }

            var options = KickLuckyCubeAnimalSpawner.CreateDefaultOptions();
            var hasOption = options.Any(candidate => string.Equals(candidate.AnimalName, animalName, StringComparison.Ordinal));
            if (!hasOption)
            {
                ClearSavedSlot();
                return;
            }

            var option = options.First(candidate => string.Equals(candidate.AnimalName, animalName, StringComparison.Ordinal));

            placedInventoryAnimal = new KickLuckyCubeInventoryAnimal(
                "stable_" + stableSlotId,
                option.AnimalName,
                option.Rarity,
                option.BodyColor,
                option.SellValue,
                option.IncomePerSecond);
            placedAnimal = CreateStableAnimal(placedInventoryAnimal);
            pendingSoft = Mathf.Max(0f, PlayerPrefs.GetFloat(GetKey(PendingSoftKey), 0f));
            pendingSoft += IncomePerSecond * GetOfflineElapsedSeconds();
            SaveSlot();
        }

        private void SaveSlot()
        {
            if (!Application.isPlaying || !saveInPlayerPrefs)
            {
                return;
            }

            PlayerPrefs.SetInt(GetKey(UpgradeLevelKey), upgradeLevel);

            if (placedAnimal == null)
            {
                ClearSavedSlot();
                return;
            }

            if (!placedInventoryAnimal.IsValid)
            {
                placedInventoryAnimal = KickLuckyCubeInventoryAnimal.FromSpawnedAnimal(placedAnimal);
            }

            PlayerPrefs.SetString(GetKey(AnimalJsonKey), JsonUtility.ToJson(placedInventoryAnimal));
            PlayerPrefs.SetString(GetKey(AnimalNameKey), placedAnimal.AnimalName);
            PlayerPrefs.SetFloat(GetKey(PendingSoftKey), pendingSoft);
            PlayerPrefs.SetString(GetKey(SavedAtKey), DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }

        private void LoadUpgradeLevel()
        {
            upgradeLevel = Mathf.Clamp(
                PlayerPrefs.GetInt(GetKey(UpgradeLevelKey), upgradeLevel),
                1,
                maxUpgradeLevel);
        }

        private void ClearSavedSlot()
        {
            PlayerPrefs.DeleteKey(GetKey(AnimalNameKey));
            PlayerPrefs.DeleteKey(GetKey(AnimalJsonKey));
            PlayerPrefs.DeleteKey(GetKey(PendingSoftKey));
            PlayerPrefs.DeleteKey(GetKey(SavedAtKey));
        }

        private string GetKey(string suffix)
        {
            return saveKeyPrefix + stableSlotId + "." + suffix;
        }

        private int GetBoostedIncome(int baseIncome)
        {
            return baseIncome > 0 ? Mathf.Max(1, Mathf.RoundToInt(baseIncome * IncomeMultiplier)) : 0;
        }

        private float GetIncomeMultiplier(int level)
        {
            return 1f + Mathf.Max(0, level - 1) * incomeBonusPerLevel;
        }

        private KickLuckyCubeSpawnedAnimal CreateStableAnimal(KickLuckyCubeInventoryAnimal inventoryAnimal)
        {
            var option = new KickLuckyCubeAnimalOption(
                inventoryAnimal.Rarity,
                inventoryAnimal.AnimalName,
                inventoryAnimal.BodyColor,
                inventoryAnimal.SellValue,
                inventoryAnimal.IncomePerSecond,
                1f);

            return CreateStableAnimal(option);
        }

        private KickLuckyCubeSpawnedAnimal CreateStableAnimal(KickLuckyCubeAnimalOption option)
        {
            var root = new GameObject("KLC_Stable_" + Sanitize(option.AnimalName));
            root.transform.SetParent(animalAnchor, false);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one * 0.72f;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(0.72f, 0.58f, 1.05f);

            var bodyCollider = body.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                DestroyAnimalCollider(bodyCollider);
            }

            var animal = root.AddComponent<KickLuckyCubeSpawnedAnimal>();
            animal.SetBodyRenderer(body.GetComponent<Renderer>());
            animal.Configure(option, 0f);
            return animal;
        }

        private float GetOfflineElapsedSeconds()
        {
            var savedAtText = PlayerPrefs.GetString(GetKey(SavedAtKey), string.Empty);
            if (!long.TryParse(savedAtText, out var savedAtUnix))
            {
                return 0f;
            }

            return Mathf.Min(
                offlineIncomeCapSeconds,
                Mathf.Max(0f, DateTimeOffset.UtcNow.ToUnixTimeSeconds() - savedAtUnix));
        }

        private Transform FindChildByNamePart(string namePart)
        {
            var children = GetComponentsInChildren<Transform>(true);
            return children.FirstOrDefault(child =>
                child != transform
                && child.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private TextMesh CreateStatusLabel()
        {
            var labelObject = new GameObject("KLC_StableSlot_StatusLabel");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.2f, -0.9f);
            labelObject.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
            labelObject.transform.localScale = Vector3.one;

            var label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 36;
            label.characterSize = 0.045f;
            label.color = Color.white;
            return label;
        }

        private string BuildStableSlotId()
        {
            var cursor = transform.parent;
            while (cursor != null)
            {
                if (cursor.name.StartsWith("KLC_PlotInstance_", StringComparison.Ordinal)
                    || cursor.name.StartsWith("KLC_PlotTemplate_", StringComparison.Ordinal))
                {
                    return cursor.name + "." + gameObject.name;
                }

                cursor = cursor.parent;
            }

            return gameObject.name;
        }

        private static void DestroyAnimalCollider(Collider target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
                return;
            }

            DestroyImmediate(target);
        }

        private static string Sanitize(string value)
        {
            var source = string.IsNullOrWhiteSpace(value) ? "Animal" : value;
            return new string(source.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        }
    }
}
