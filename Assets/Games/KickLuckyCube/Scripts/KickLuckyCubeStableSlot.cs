using RobloxBasicProject.GameKit.Interaction;
using System;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStableSlot : MonoBehaviour, GameKitInteractionCondition
    {
        private const string AnimalNameKey = "AnimalName";
        private const string PendingSoftKey = "PendingSoft";
        private const string SavedAtKey = "SavedAt";

        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private Transform animalAnchor;
        [SerializeField] private TextMesh statusLabel;
        [SerializeField] private string stableSlotId;
        [SerializeField] private string saveKeyPrefix = "KickLuckyCube.Stable.";
        [SerializeField, Min(0f)] private float incomeUpdateSeconds = 1f;
        [SerializeField, Min(1f)] private float saveIntervalSeconds = 5f;
        [SerializeField, Min(0f)] private float offlineIncomeCapSeconds = 7200f;
        [SerializeField] private bool saveInPlayerPrefs = true;

        private KickLuckyCubeSpawnedAnimal placedAnimal;
        private float pendingSoft;
        private float incomeTimer;
        private float saveTimer;

        public KickLuckyCubeSpawnedAnimal PlacedAnimal => placedAnimal;
        public bool IsOccupied => placedAnimal != null;
        public int PendingSoft => Mathf.FloorToInt(pendingSoft);
        public int IncomePerSecond => placedAnimal != null ? placedAnimal.IncomePerSecond : 0;

        private void Awake()
        {
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>();

            if (animalAnchor == null)
            {
                animalAnchor = transform;
            }

            if (string.IsNullOrWhiteSpace(stableSlotId))
            {
                stableSlotId = gameObject.name;
            }

            LoadSlot();
            RefreshLabel();
        }

        private void Update()
        {
            if (placedAnimal == null || placedAnimal.IncomePerSecond <= 0)
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
            pendingSoft += placedAnimal.IncomePerSecond * ticks;
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
            pendingSoft = 0f;
            incomeTimer = 0f;
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
                ? "Empty stable"
                : $"{placedAnimal.AnimalName}\n+{placedAnimal.IncomePerSecond}/s\nClaim: {PendingSoft}";
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

            var animalName = PlayerPrefs.GetString(GetKey(AnimalNameKey), string.Empty);
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

            pendingSoft = Mathf.Max(0f, PlayerPrefs.GetFloat(GetKey(PendingSoftKey), 0f));

            var savedAtText = PlayerPrefs.GetString(GetKey(SavedAtKey), string.Empty);
            if (long.TryParse(savedAtText, out var savedAtUnix))
            {
                var elapsed = Mathf.Min(
                    offlineIncomeCapSeconds,
                    Mathf.Max(0f, DateTimeOffset.UtcNow.ToUnixTimeSeconds() - savedAtUnix));
                pendingSoft += option.IncomePerSecond * elapsed;
            }

            placedAnimal = CreateStableAnimal(option);
            SaveSlot();
        }

        private void SaveSlot()
        {
            if (!Application.isPlaying || !saveInPlayerPrefs)
            {
                return;
            }

            if (placedAnimal == null)
            {
                ClearSavedSlot();
                return;
            }

            PlayerPrefs.SetString(GetKey(AnimalNameKey), placedAnimal.AnimalName);
            PlayerPrefs.SetFloat(GetKey(PendingSoftKey), pendingSoft);
            PlayerPrefs.SetString(GetKey(SavedAtKey), DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }

        private void ClearSavedSlot()
        {
            PlayerPrefs.DeleteKey(GetKey(AnimalNameKey));
            PlayerPrefs.DeleteKey(GetKey(PendingSoftKey));
            PlayerPrefs.DeleteKey(GetKey(SavedAtKey));
        }

        private string GetKey(string suffix)
        {
            return saveKeyPrefix + stableSlotId + "." + suffix;
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
