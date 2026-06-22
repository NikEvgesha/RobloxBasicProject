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

            var option = PickAnimal(result.Rarity, result.Distance);
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

            var candidates = animalOptions
                .Where(option => option.Rarity == rarity)
                .ToArray();

            return candidates.Length > 0
                ? candidates
                : animalOptions.ToArray();
        }

        public KickLuckyCubeAnimalOption PickRandomAnimal(KickLuckyCubeRarity rarity)
        {
            var candidates = GetCandidateOptions(rarity);
            return candidates.Length > 0
                ? candidates[UnityEngine.Random.Range(0, candidates.Length)]
                : CreateFallbackOption();
        }

        private KickLuckyCubeSpawnedAnimal SpawnConfiguredAnimal(
            KickLuckyCubeKickResult result,
            float baseRunnerSpeed,
            KickLuckyCubeAnimalOption option)
        {
            var root = new GameObject("KLC_Runner_" + Sanitize(option.AnimalName));
            root.transform.SetParent(spawnRoot, true);
            root.transform.SetPositionAndRotation(
                result.LandingPosition + spawnOffset,
                Quaternion.LookRotation(Vector3.back, Vector3.up));

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = new Vector3(0.72f, 0.58f, 1.05f);

            var bodyCollider = body.GetComponent<Collider>();
            if (bodyCollider != null)
            {
                DestroyUnityObject(bodyCollider);
            }

            var animal = root.AddComponent<KickLuckyCubeSpawnedAnimal>();
            animal.SetBodyRenderer(body.GetComponent<Renderer>());
            animal.Configure(option, baseRunnerSpeed);
            animal.EnsureBlackOutline();

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

        private KickLuckyCubeAnimalOption PickAnimal(KickLuckyCubeRarity rarity, float distance)
        {
            var candidates = GetCandidateOptions(rarity);
            if (candidates.Length == 0)
            {
                return CreateFallbackOption();
            }

            var index = Mathf.Abs(Mathf.FloorToInt(distance)) % candidates.Length;
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
