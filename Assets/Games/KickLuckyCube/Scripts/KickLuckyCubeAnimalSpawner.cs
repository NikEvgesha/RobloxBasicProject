using System;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeAnimalSpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private KickLuckyCubeAnimalOption[] animalOptions = Array.Empty<KickLuckyCubeAnimalOption>();
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.58f, 0f);

        private KickLuckyCubeSpawnedAnimal currentAnimal;

        public KickLuckyCubeSpawnedAnimal CurrentAnimal => currentAnimal;

        private void Awake()
        {
            if (spawnRoot == null)
            {
                spawnRoot = transform;
            }

            if (animalOptions == null || animalOptions.Length == 0)
            {
                animalOptions = CreateDefaultOptions();
            }
        }

        public KickLuckyCubeSpawnedAnimal Spawn(KickLuckyCubeKickResult result, float baseRunnerSpeed)
        {
            EnsureReady();
            ClearCurrentAnimal();

            var option = PickAnimal(result);
            return SpawnConfiguredAnimal(result, baseRunnerSpeed, option);
        }

        public KickLuckyCubeSpawnedAnimal Spawn(
            KickLuckyCubeKickResult result,
            float baseRunnerSpeed,
            KickLuckyCubeAnimalOption option)
        {
            EnsureReady();
            ClearCurrentAnimal();
            return SpawnConfiguredAnimal(result, baseRunnerSpeed, option);
        }

        public KickLuckyCubeAnimalOption[] GetCandidateOptions(KickLuckyCubeRarity rarity)
        {
            EnsureReady();

            var candidates = rarity == KickLuckyCubeRarity.None
                ? Array.Empty<KickLuckyCubeAnimalOption>()
                : KickLuckyCubeAnimalCatalog.CreateLocationOptions(RarityToLocationIndex(rarity));

            return candidates.Length > 0
                ? candidates
                : animalOptions
                    .Select(KickLuckyCubeAnimalCatalog.ResolveOption)
                    .ToArray();
        }

        public KickLuckyCubeAnimalOption[] GetCandidateOptions(KickLuckyCubeKickResult result)
        {
            EnsureReady();

            var candidates = KickLuckyCubeAnimalCatalog.CreateLocationOptions(ResolveLocationIndex(result));
            return candidates.Length > 0
                ? candidates
                : GetCandidateOptions(result.Rarity);
        }

        public KickLuckyCubeAnimalOption PickRandomAnimal(KickLuckyCubeRarity rarity)
        {
            var candidates = GetCandidateOptions(rarity);
            return candidates.Length > 0
                ? candidates[UnityEngine.Random.Range(0, candidates.Length)]
                : CreateFallbackOption();
        }

        public KickLuckyCubeAnimalOption PickRandomAnimal(KickLuckyCubeKickResult result)
        {
            var candidates = GetCandidateOptions(result);
            return candidates.Length > 0
                ? candidates[UnityEngine.Random.Range(0, candidates.Length)]
                : CreateFallbackOption();
        }

        private KickLuckyCubeSpawnedAnimal SpawnConfiguredAnimal(
            KickLuckyCubeKickResult result,
            float baseRunnerSpeed,
            KickLuckyCubeAnimalOption option)
        {
            option = KickLuckyCubeAnimalCatalog.ResolveOption(option);

            var root = new GameObject("KLC_Runner_" + Sanitize(option.DisplayName));
            root.transform.SetParent(spawnRoot, true);
            root.transform.SetPositionAndRotation(
                result.LandingPosition,
                Quaternion.LookRotation(Vector3.back, Vector3.up));

            var bodyRenderer = KickLuckyCubeAnimalVisualFactory.CreateVisual(
                root.transform,
                option,
                1.15f,
                out var usedImportedVisual);
            if (!usedImportedVisual && bodyRenderer != null)
            {
                bodyRenderer.transform.localPosition = spawnOffset;
            }

            var animal = root.AddComponent<KickLuckyCubeSpawnedAnimal>();
            animal.SetBodyRenderer(bodyRenderer);
            animal.Configure(option, baseRunnerSpeed, !usedImportedVisual);
            if (!usedImportedVisual)
            {
                animal.EnsureBlackOutline();
            }

            var runner = root.AddComponent<KickLuckyCubeAnimalRunner>();
            runner.Configure(animal, animal.RunnerSpeed);

            currentAnimal = animal;
            return currentAnimal;
        }

        public void ClearCurrentAnimal()
        {
            if (currentAnimal == null)
            {
                return;
            }

            DestroyUnityObject(currentAnimal.gameObject);
            currentAnimal = null;
        }

        public void ReleaseCurrentAnimal(KickLuckyCubeSpawnedAnimal animal)
        {
            if (animal != null && currentAnimal != animal)
            {
                return;
            }

            currentAnimal = null;
        }

        private void EnsureReady()
        {
            if (spawnRoot == null)
            {
                spawnRoot = transform;
            }

            if (animalOptions == null || animalOptions.Length == 0)
            {
                animalOptions = CreateDefaultOptions();
            }
        }

        private KickLuckyCubeAnimalOption PickAnimal(KickLuckyCubeKickResult result)
        {
            var candidates = GetCandidateOptions(result);
            if (candidates.Length == 0)
            {
                return CreateFallbackOption();
            }

            var index = Mathf.Abs(Mathf.FloorToInt(result.Distance)) % candidates.Length;
            return candidates[index];
        }

        private static KickLuckyCubeAnimalOption CreateFallbackOption()
        {
            return new KickLuckyCubeAnimalOption(
                KickLuckyCubeRarity.Common,
                "Fallback Cat",
                new Color(0.96f, 0.78f, 0.34f),
                10,
                1,
                1f);
        }

        public static KickLuckyCubeAnimalOption[] CreateDefaultOptions()
        {
            return KickLuckyCubeAnimalCatalog.CreateDefaultOptions();
        }

        private static int ResolveLocationIndex(KickLuckyCubeKickResult result)
        {
            var rarityLocationIndex = RarityToLocationIndex(result.Rarity);
            if (result.Zone != null)
            {
                return Mathf.Max(1, Mathf.Max(result.Zone.ZoneIndex, rarityLocationIndex));
            }

            return rarityLocationIndex;
        }

        private static int RarityToLocationIndex(KickLuckyCubeRarity rarity)
        {
            return rarity switch
            {
                KickLuckyCubeRarity.Uncommon => 2,
                KickLuckyCubeRarity.Rare => 3,
                KickLuckyCubeRarity.Epic => 4,
                KickLuckyCubeRarity.Legendary => 5,
                _ => 1,
            };
        }

        private static string Sanitize(string value)
        {
            var source = string.IsNullOrWhiteSpace(value) ? "Animal" : value;
            return new string(source.Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray());
        }

        private static void DestroyUnityObject(UnityEngine.Object target)
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
    }
}
