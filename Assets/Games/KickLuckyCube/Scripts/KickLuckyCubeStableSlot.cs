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
        [SerializeField] private string statusLabelResourcePath = "KickLuckyCube/World/KLC_StableSlot_StatusLabel";
        [SerializeField] private string stableSlotId;
        [SerializeField] private string saveKeyPrefix = "KickLuckyCube.Stable.";
        [SerializeField, Min(1f)] private float saveIntervalSeconds = 5f;
        [SerializeField, Min(0f)] private float offlineIncomeCapSeconds;
        [SerializeField, Min(1)] private int upgradeLevel = 1;
        [SerializeField, Min(0)] private int baseUpgradeCost = 150;
        [SerializeField, Min(1f)] private float upgradeCostMultiplier = 1.65f;
        [SerializeField, Min(0f)] private float incomeBonusPerLevel = 0.2f;
        [SerializeField] private Vector3 stableAnimalLocalPosition = Vector3.zero;
        [SerializeField, Min(0.1f)] private float stableAnimalTargetHeight = 1.05f;
        [SerializeField, Min(0f)] private float stableAnimalBottomOffset = 0.04f;
        [SerializeField] private bool faceAnimalTowardPlotCenter = true;
        [SerializeField] private float stableAnimalFacingFallbackCenterX;
        [SerializeField, Min(0f)] private float stableAnimalFacingDeadZone = 0.25f;
        [SerializeField] private bool hideMobAnchorVisuals = true;
        [SerializeField, Min(1f)] private float labelVisibleDistance = 22f;
        [SerializeField] private Vector3 labelWorldOffset = new(0f, 0.62f, 0f);
        [SerializeField] private bool saveInPlayerPrefs = true;

        private KickLuckyCubeSpawnedAnimal placedAnimal;
        private KickLuckyCubeInventoryAnimal placedInventoryAnimal;
        private float pendingSoft;
        private float saveTimer;
        private long lastIncomeUnixSeconds;
        private bool hasResolvedFacingCenterX;
        private float resolvedFacingCenterX;

        public KickLuckyCubeSpawnedAnimal PlacedAnimal => placedAnimal;
        public string StableSlotId => stableSlotId;
        public bool IsOccupied => placedAnimal != null;
        public int PendingSoft => Mathf.FloorToInt(pendingSoft);
        public int BaseIncomePerSecond => placedAnimal != null ? placedAnimal.IncomePerSecond : 0;
        public int IncomePerSecond => GetBoostedIncome(BaseIncomePerSecond);
        public int UpgradeLevel => upgradeLevel;
        public int MaxUpgradeLevel => int.MaxValue;
        public float IncomeMultiplier => GetIncomeMultiplier(upgradeLevel);
        public bool CanUpgrade => true;
        public int NextUpgradeCost => CalculateUpgradeCost(upgradeLevel);
        public float NextIncomeMultiplier => GetIncomeMultiplier(upgradeLevel + 1);
        public bool IsPlayerPersistentSlot => saveInPlayerPrefs
            && !string.IsNullOrWhiteSpace(stableSlotId)
            && stableSlotId.StartsWith("Player.", StringComparison.Ordinal);

        private void Awake()
        {
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>();

            if (animalAnchor == null)
            {
                animalAnchor = FindChildByNamePart("MobAnchor") ?? transform;
            }

            if (statusLabel == null)
            {
                statusLabel = ResolveStatusLabel() ?? CreateStatusLabel();
            }

            ApplyStatusLabelStyle(statusLabel);
            HideMobAnchorPlaceholders();

            if (string.IsNullOrWhiteSpace(stableSlotId))
            {
                stableSlotId = BuildStableSlotId(out var shouldSaveByDefault);
                saveInPlayerPrefs = saveInPlayerPrefs && shouldSaveByDefault;
            }

            LoadSlot();
            RefreshLabel();
            UpdateLabelView();
        }

        private void Update()
        {
            ApplyStableAnimalPose();
            UpdateLabelView();

            if (placedAnimal == null || IncomePerSecond <= 0)
            {
                return;
            }

            var changed = AccrueRealtimeIncome(false);
            if (changed)
            {
                RefreshLabel();
            }

            saveTimer += Time.unscaledDeltaTime;
            if (saveTimer >= saveIntervalSeconds)
            {
                saveTimer = 0f;
                SaveSlot();
            }
        }

        private void OnDestroy()
        {
            SaveSlot();

            if (Application.isPlaying
                && statusLabel != null
                && statusLabel.transform.parent != null
                && string.Equals(statusLabel.transform.parent.name, "KLC_RuntimeWorldLabels", StringComparison.Ordinal))
            {
                Destroy(statusLabel.gameObject);
            }
        }

        public bool CanInteract(GameObject actor)
        {
            return placedAnimal != null || (runPhase != null && runPhase.HasCarriedAnimal && placedAnimal == null);
        }

        public void ConfigurePersistence(string slotId, bool saveInPrefs)
        {
            var normalizedSlotId = NormalizeStableSlotId(slotId);
            var changed = !string.Equals(stableSlotId, normalizedSlotId, StringComparison.Ordinal)
                || saveInPlayerPrefs != saveInPrefs;

            stableSlotId = normalizedSlotId;
            saveInPlayerPrefs = saveInPrefs;

            if (!Application.isPlaying || !changed)
            {
                return;
            }

            ClearRuntimeState(true);
            if (saveInPlayerPrefs)
            {
                LoadSlot();
            }

            RefreshLabel();
        }

        public void Place(GameObject actor)
        {
            runPhase?.TryPlaceCarriedAnimal(this);
        }

        public void ConfigureStableVisuals(float animalTargetHeight, Vector3 animalLocalPosition, float animalBottomOffset, Vector3 labelOffset)
        {
            stableAnimalTargetHeight = Mathf.Max(0.1f, animalTargetHeight);
            stableAnimalLocalPosition = animalLocalPosition;
            stableAnimalBottomOffset = Mathf.Max(0f, animalBottomOffset);
            labelWorldOffset = labelOffset;
            hasResolvedFacingCenterX = false;
            ApplyStableAnimalPose();
            RefreshLabel();
        }

        public bool TryPlace(KickLuckyCubeSpawnedAnimal animal)
        {
            if (animal == null || placedAnimal != null)
            {
                return false;
            }

            placedAnimal = animal;
            placedInventoryAnimal = KickLuckyCubeInventoryAnimal.FromSpawnedAnimal(animal);
            KickLuckyCubeAnimalCollection.MarkDiscovered(placedInventoryAnimal);
            lastIncomeUnixSeconds = GetCurrentUnixSeconds();
            placedAnimal.SetCarried(animalAnchor);
            ApplyStableAnimalPose();
            placedAnimal.EnsureBlackOutline();
            ApplyStableAnimalPose();

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

        public bool TryTakeToInventory(KickLuckyCubeInventoryController inventory, out int collectedSoft)
        {
            collectedSoft = 0;
            if (inventory == null || placedAnimal == null)
            {
                return false;
            }

            if (!placedInventoryAnimal.IsValid)
            {
                placedInventoryAnimal = KickLuckyCubeInventoryAnimal.FromSpawnedAnimal(placedAnimal);
            }

            if (!inventory.TryAddAnimal(placedInventoryAnimal, true))
            {
                return false;
            }

            AccrueRealtimeIncome(false);
            collectedSoft = PendingSoft;
            if (placedAnimal != null)
            {
                DestroyAnimalObject(placedAnimal);
            }

            placedAnimal = null;
            placedInventoryAnimal = default;
            pendingSoft = 0f;
            lastIncomeUnixSeconds = GetCurrentUnixSeconds();
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
            KickLuckyCubeAnimalCollection.MarkDiscovered(placedInventoryAnimal);
            placedAnimal = CreateStableAnimal(animal);
            lastIncomeUnixSeconds = GetCurrentUnixSeconds();
            RefreshLabel();
            SaveSlot();
            return true;
        }

        public int Collect()
        {
            AccrueRealtimeIncome(false);
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
            if (wallet == null)
            {
                return false;
            }

            var cost = NextUpgradeCost;
            if (!wallet.TrySpendSoft(cost))
            {
                return false;
            }

            upgradeLevel = Mathf.Max(1, upgradeLevel + 1);
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
            ClearRuntimeState(true);
            RefreshLabel();
            SaveSlot();
        }

        private void RefreshLabel()
        {
            if (statusLabel == null)
            {
                return;
            }

            if (placedAnimal == null)
            {
                statusLabel.text = string.Empty;
                statusLabel.gameObject.SetActive(false);
                return;
            }

            statusLabel.text = $"{placedAnimal.DisplayName}\nLv {upgradeLevel}  +{IncomePerSecond}/s";
            UpdateLabelView();
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
            if (!Application.isPlaying || !saveInPlayerPrefs || string.IsNullOrWhiteSpace(stableSlotId))
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
                    lastIncomeUnixSeconds = GetSavedIncomeUnixSeconds();
                    AccrueRealtimeIncome(true);
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
                option.CatalogId,
                option.AnimalName,
                option.Rarity,
                option.BodyColor,
                option.SellValue,
                option.IncomePerSecond,
                option.Grade);
            placedAnimal = CreateStableAnimal(placedInventoryAnimal);
            pendingSoft = Mathf.Max(0f, PlayerPrefs.GetFloat(GetKey(PendingSoftKey), 0f));
            lastIncomeUnixSeconds = GetSavedIncomeUnixSeconds();
            AccrueRealtimeIncome(true);
            SaveSlot();
        }

        private void SaveSlot()
        {
            if (!Application.isPlaying || !saveInPlayerPrefs || string.IsNullOrWhiteSpace(stableSlotId))
            {
                return;
            }

            PlayerPrefs.SetInt(GetKey(UpgradeLevelKey), upgradeLevel);

            if (placedAnimal == null && !placedInventoryAnimal.IsValid)
            {
                ClearSavedSlot();
                PlayerPrefs.Save();
                return;
            }

            if (placedAnimal != null)
            {
                AccrueRealtimeIncome(false);
            }
            else if (lastIncomeUnixSeconds <= 0L)
            {
                lastIncomeUnixSeconds = GetCurrentUnixSeconds();
            }

            if (!placedInventoryAnimal.IsValid)
            {
                placedInventoryAnimal = KickLuckyCubeInventoryAnimal.FromSpawnedAnimal(placedAnimal);
            }

            PlayerPrefs.SetString(GetKey(AnimalJsonKey), JsonUtility.ToJson(placedInventoryAnimal));
            PlayerPrefs.SetString(GetKey(AnimalNameKey), placedInventoryAnimal.AnimalName);
            PlayerPrefs.SetFloat(GetKey(PendingSoftKey), pendingSoft);
            PlayerPrefs.SetString(GetKey(SavedAtKey), lastIncomeUnixSeconds.ToString());
            PlayerPrefs.Save();
        }

        private void LoadUpgradeLevel()
        {
            upgradeLevel = Mathf.Max(1, PlayerPrefs.GetInt(GetKey(UpgradeLevelKey), upgradeLevel));
        }

        private void ClearSavedSlot()
        {
            PlayerPrefs.DeleteKey(GetKey(AnimalNameKey));
            PlayerPrefs.DeleteKey(GetKey(AnimalJsonKey));
            PlayerPrefs.DeleteKey(GetKey(PendingSoftKey));
            PlayerPrefs.DeleteKey(GetKey(SavedAtKey));
        }

        private void ClearRuntimeState(bool resetUpgrade)
        {
            if (placedAnimal != null)
            {
                DestroyAnimalObject(placedAnimal);
            }

            placedAnimal = null;
            placedInventoryAnimal = default;
            pendingSoft = 0f;
            lastIncomeUnixSeconds = GetCurrentUnixSeconds();
            if (resetUpgrade)
            {
                upgradeLevel = 1;
            }
        }

        private string GetKey(string suffix)
        {
            return saveKeyPrefix + stableSlotId + "." + suffix;
        }

        private int GetBoostedIncome(int baseIncome)
        {
            return baseIncome > 0 ? Mathf.Max(1, Mathf.RoundToInt(baseIncome * IncomeMultiplier)) : 0;
        }

        private int CalculateUpgradeCost(int level)
        {
            var exponent = Math.Max(0, level - 1);
            var cost = baseUpgradeCost * Math.Pow(upgradeCostMultiplier, exponent);
            if (double.IsNaN(cost) || cost <= 0d)
            {
                return 0;
            }

            return cost >= int.MaxValue ? int.MaxValue : Mathf.RoundToInt((float)cost);
        }

        private float GetIncomeMultiplier(int level)
        {
            return 1f + Mathf.Max(0, level - 1) * incomeBonusPerLevel;
        }

        private KickLuckyCubeSpawnedAnimal CreateStableAnimal(KickLuckyCubeInventoryAnimal inventoryAnimal)
        {
            return CreateStableAnimal(KickLuckyCubeAnimalCatalog.CreateOption(inventoryAnimal));
        }

        private KickLuckyCubeSpawnedAnimal CreateStableAnimal(KickLuckyCubeAnimalOption option)
        {
            var root = new GameObject("KLC_Stable_" + Sanitize(option.DisplayName));
            root.transform.SetParent(animalAnchor, false);
            root.transform.localPosition = stableAnimalLocalPosition;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            var bodyRenderer = KickLuckyCubeAnimalVisualFactory.CreateVisual(
                root.transform,
                option,
                stableAnimalTargetHeight,
                out var usedImportedVisual);

            var animal = root.AddComponent<KickLuckyCubeSpawnedAnimal>();
            animal.SetBodyRenderer(bodyRenderer);
            animal.Configure(option, 0f, !usedImportedVisual);
            ApplyStableAnimalPose(animal);
            if (!usedImportedVisual)
            {
                animal.EnsureBlackOutline();
            }

            ApplyStableAnimalPose(animal);
            return animal;
        }

        private void ApplyStableAnimalPose()
        {
            ApplyStableAnimalPose(placedAnimal);
        }

        private void ApplyStableAnimalPose(KickLuckyCubeSpawnedAnimal animal)
        {
            if (animal == null || animalAnchor == null)
            {
                return;
            }

            var targetParent = Application.isPlaying ? ResolveRuntimeAnimalRoot() : animalAnchor;
            if (animal.transform.parent != targetParent)
            {
                animal.transform.SetParent(targetParent, true);
            }

            animal.transform.SetPositionAndRotation(
                animalAnchor.TransformPoint(stableAnimalLocalPosition),
                ResolveStableAnimalRotation());
            animal.transform.localScale = Vector3.one;

            if (!TryGetRendererBounds(animal.transform, out var bounds))
            {
                return;
            }

            var targetHeight = Mathf.Max(0.1f, stableAnimalTargetHeight);
            var currentHeight = Mathf.Max(0.001f, bounds.size.y);
            var uniformScale = Mathf.Clamp(targetHeight / currentHeight, 0.01f, 100f);
            animal.transform.localScale = Vector3.one * uniformScale;

            if (!TryGetRendererBounds(animal.transform, out bounds))
            {
                return;
            }

            var targetBottomY = animalAnchor.position.y + stableAnimalBottomOffset;
            animal.transform.position += Vector3.up * (targetBottomY - bounds.min.y);
        }

        private Quaternion ResolveStableAnimalRotation()
        {
            if (!faceAnimalTowardPlotCenter || animalAnchor == null)
            {
                return animalAnchor != null ? animalAnchor.rotation : transform.rotation;
            }

            var xOffset = animalAnchor.position.x - ResolveStableAnimalFacingCenterX();
            if (Mathf.Abs(xOffset) <= stableAnimalFacingDeadZone)
            {
                return animalAnchor.rotation;
            }

            var facingDirection = xOffset > 0f ? Vector3.left : Vector3.right;
            return Quaternion.LookRotation(facingDirection, Vector3.up);
        }

        private float ResolveStableAnimalFacingCenterX()
        {
            if (hasResolvedFacingCenterX)
            {
                return resolvedFacingCenterX;
            }

            resolvedFacingCenterX = stableAnimalFacingFallbackCenterX;
            var plotRoot = transform.parent;
            if (plotRoot == null)
            {
                hasResolvedFacingCenterX = true;
                return resolvedFacingCenterX;
            }

            var siblingSlotAnchors = Enumerable.Range(0, plotRoot.childCount)
                .Select(index => plotRoot.GetChild(index))
                .Where(IsStableSlotSibling)
                .Select(ResolveStableSlotAnchorX)
                .ToArray();
            if (siblingSlotAnchors.Length <= 1)
            {
                hasResolvedFacingCenterX = true;
                return resolvedFacingCenterX;
            }

            resolvedFacingCenterX = siblingSlotAnchors.Average();
            hasResolvedFacingCenterX = true;
            return resolvedFacingCenterX;
        }

        private bool IsStableSlotSibling(Transform sibling)
        {
            return sibling != null
                && (sibling == transform
                    || sibling.name.StartsWith("Template_StableSlot_", StringComparison.Ordinal)
                    || sibling.GetComponent<KickLuckyCubeStableSlot>() != null);
        }

        private static float ResolveStableSlotAnchorX(Transform slotRoot)
        {
            if (slotRoot == null)
            {
                return 0f;
            }

            var anchor = slotRoot
                .GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child =>
                    child != slotRoot
                    && child.name.IndexOf("MobAnchor", StringComparison.OrdinalIgnoreCase) >= 0);
            return anchor != null ? anchor.position.x : slotRoot.position.x;
        }

        private bool AccrueRealtimeIncome(bool applyOfflineCap)
        {
            if (placedAnimal == null || IncomePerSecond <= 0)
            {
                lastIncomeUnixSeconds = GetCurrentUnixSeconds();
                return false;
            }

            var now = GetCurrentUnixSeconds();
            if (lastIncomeUnixSeconds <= 0L)
            {
                lastIncomeUnixSeconds = now;
                return false;
            }

            var elapsedSeconds = Math.Max(0L, now - lastIncomeUnixSeconds);
            if (elapsedSeconds <= 0L)
            {
                return false;
            }

            if (applyOfflineCap && offlineIncomeCapSeconds > 0f)
            {
                elapsedSeconds = Math.Min(elapsedSeconds, Mathf.FloorToInt(offlineIncomeCapSeconds));
            }

            pendingSoft += IncomePerSecond * elapsedSeconds;
            lastIncomeUnixSeconds = now;
            return true;
        }

        private long GetSavedIncomeUnixSeconds()
        {
            var savedAtText = PlayerPrefs.GetString(GetKey(SavedAtKey), string.Empty);
            return long.TryParse(savedAtText, out var savedAtUnix)
                ? savedAtUnix
                : GetCurrentUnixSeconds();
        }

        private static long GetCurrentUnixSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
            var labelPrefab = Resources.Load<TextMesh>(statusLabelResourcePath);
            if (labelPrefab == null)
            {
                Debug.LogError($"[KLC-STABLE] Missing authored status label at Resources/{statusLabelResourcePath}.", this);
                return null;
            }

            var label = Instantiate(labelPrefab, transform, false);
            label.name = "KLC_StableSlot_StatusLabel";
            ApplyStatusLabelStyle(label);
            return label;
        }

        private TextMesh ResolveStatusLabel()
        {
            return GetComponentsInChildren<TextMesh>(true)
                .FirstOrDefault(label => string.Equals(label.name, "KLC_StableSlot_StatusLabel", StringComparison.Ordinal));
        }

        private void UpdateLabelView()
        {
            if (statusLabel == null)
            {
                return;
            }

            if (placedAnimal == null)
            {
                statusLabel.gameObject.SetActive(false);
                return;
            }

            var camera = Camera.main;
            var viewer = camera != null ? camera.transform : ResolveViewer();
            if (viewer != null && (viewer.position - placedAnimal.transform.position).sqrMagnitude > labelVisibleDistance * labelVisibleDistance)
            {
                statusLabel.gameObject.SetActive(false);
                return;
            }

            statusLabel.gameObject.SetActive(true);
            statusLabel.transform.position = ResolveLabelWorldPosition();
            if (viewer == null)
            {
                return;
            }

            statusLabel.transform.localScale = Vector3.one;

            var direction = statusLabel.transform.position - viewer.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                statusLabel.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        private Vector3 ResolveLabelWorldPosition()
        {
            if (placedAnimal == null)
            {
                return transform.position + Vector3.up * 1.4f;
            }

            if (!TryGetRendererBounds(placedAnimal.transform, out var bounds))
            {
                return placedAnimal.transform.position + Vector3.up * 1.35f;
            }

            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z) + labelWorldOffset;
        }

        private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
        {
            bounds = new Bounds(root != null ? root.position : Vector3.zero, Vector3.zero);
            if (root == null)
            {
                return false;
            }

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            var hasBounds = false;
            foreach (var current in renderers)
            {
                if (current == null || !current.gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = current.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(current.bounds);
                }
            }

            return hasBounds;
        }

        private static Transform ResolveRuntimeAnimalRoot()
        {
            const string RootName = "KLC_RuntimeStableAnimals";
            var existing = GameObject.Find(RootName);
            var root = existing != null ? existing.transform : new GameObject(RootName).transform;
            root.position = Vector3.zero;
            root.rotation = Quaternion.identity;
            root.localScale = Vector3.one;
            return root;
        }

        private static Transform ResolveViewer()
        {
            var player = FindFirstObjectByType<KickLuckyCubePlayerController>(FindObjectsInactive.Include);
            if (player != null)
            {
                return player.transform;
            }

            var runner = FindFirstObjectByType<KickLuckyCubeAnimalRunner>(FindObjectsInactive.Include);
            return runner != null ? runner.transform : null;
        }

        private void HideMobAnchorPlaceholders()
        {
            if (!hideMobAnchorVisuals || animalAnchor == null)
            {
                return;
            }

            var renderers = animalAnchor.GetComponentsInChildren<Renderer>(true);
            foreach (var current in renderers)
            {
                if (current != null)
                {
                    current.enabled = false;
                }
            }
        }

        private string BuildStableSlotId(out bool shouldSaveByDefault)
        {
            shouldSaveByDefault = false;
            var cursor = transform.parent;
            while (cursor != null)
            {
                if (string.Equals(cursor.name, "KLC_PlayerPlot_Instance", StringComparison.Ordinal))
                {
                    shouldSaveByDefault = true;
                    return "Player." + gameObject.name;
                }

                if (cursor.name.StartsWith("KLC_BotPlot_", StringComparison.Ordinal)
                    || cursor.name.StartsWith("KLC_PlotTemplate_", StringComparison.Ordinal))
                {
                    return cursor.name + "." + gameObject.name;
                }

                if (cursor.name.StartsWith("KLC_PlotInstance_", StringComparison.Ordinal))
                {
                    shouldSaveByDefault = true;
                    return cursor.name + "." + gameObject.name;
                }

                cursor = cursor.parent;
            }

            return gameObject.name;
        }

        private static string NormalizeStableSlotId(string value)
        {
            var source = string.IsNullOrWhiteSpace(value) ? "Player.StableSlot" : value.Trim();
            return source.Replace(' ', '_');
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

        private static void ApplyStatusLabelStyle(TextMesh label)
        {
            if (label == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                label.transform.SetParent(KickLuckyCubeWorldTextOutline.ResolveRuntimeLabelRoot(), false);
            }

            label.transform.localScale = Vector3.one;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontStyle = FontStyle.Bold;
            label.fontSize = 72;
            label.characterSize = 0.062f;
            label.lineSpacing = 0.9f;
            KickLuckyCubeUiTheme.StyleWorldText(label, Color.white, 0.011f);
        }

    }
}
