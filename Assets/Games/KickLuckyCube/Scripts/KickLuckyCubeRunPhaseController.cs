using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeRunPhaseController : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeKickController kickController;
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeAnimalSpawner animalSpawner;
        [SerializeField] private KickLuckyCubeWaveChaseController waveChase;
        [SerializeField] private GameObject prototypePlayer;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private Transform returnLine;
        [SerializeField] private Text hudText;
        [SerializeField] private TextMesh worldStatusText;

        private KickLuckyCubeAnimalRunner currentRunner;
        private KickLuckyCubeSpawnedAnimal carriedAnimal;

        public bool HasActiveRun => currentRunner != null && currentRunner.ControlEnabled;
        public bool HasCarriedAnimal => carriedAnimal != null;
        public KickLuckyCubeSpawnedAnimal CarriedAnimal => carriedAnimal;

        private void Awake()
        {
            kickController ??= FindFirstObjectByType<KickLuckyCubeKickController>();
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            animalSpawner ??= FindFirstObjectByType<KickLuckyCubeAnimalSpawner>();
            waveChase ??= FindFirstObjectByType<KickLuckyCubeWaveChaseController>();

            if (prototypePlayer == null)
            {
                var player = GameObject.Find("KLC_PrototypePlayer");
                prototypePlayer = player;
            }
        }

        private void OnEnable()
        {
            if (kickController != null)
            {
                kickController.Landed += OnCubeLanded;
            }

            if (waveChase != null)
            {
                waveChase.AnimalCaught += OnAnimalCaught;
            }
        }

        private void OnDisable()
        {
            if (kickController != null)
            {
                kickController.Landed -= OnCubeLanded;
            }

            if (waveChase != null)
            {
                waveChase.AnimalCaught -= OnAnimalCaught;
            }

            if (currentRunner != null)
            {
                currentRunner.ReturnedToLine -= OnAnimalReturned;
            }
        }

        public void BeginRunFromResult(KickLuckyCubeKickResult result)
        {
            if (animalSpawner == null || waveChase == null || stats == null)
            {
                return;
            }

            ClearRunnerSubscription();
            carriedAnimal = null;
            kickController?.SetKickLocked(true);

            var spawnedAnimal = animalSpawner.Spawn(result, stats.AnimalSpeed);
            currentRunner = spawnedAnimal.GetComponent<KickLuckyCubeAnimalRunner>();
            if (currentRunner == null)
            {
                return;
            }

            currentRunner.ReturnedToLine += OnAnimalReturned;
            currentRunner.BeginRun(GetReturnLineZ());

            if (prototypePlayer != null)
            {
                prototypePlayer.SetActive(false);
            }

            waveChase.BeginChase(currentRunner);
            SetStatus($"Run back as {spawnedAnimal.AnimalName}!\nRarity: {spawnedAnimal.Rarity}\nWave is chasing you.");
        }

        public void ResetToKickLine()
        {
            ClearRunnerSubscription();
            carriedAnimal = null;
            animalSpawner?.ClearCurrentAnimal();
            waveChase?.StopChase();
            kickController?.SetKickLocked(false);
            kickController?.ResetCubeToOrigin();

            if (prototypePlayer != null)
            {
                prototypePlayer.SetActive(true);
            }

            SetStatus("Ready for the next kick.");
        }

        public void ForceReturnForPrototype()
        {
            currentRunner?.ForceReturnForPrototype();
        }

        public void ForceCatchForPrototype()
        {
            waveChase?.ForceCatchForPrototype();
        }

        public bool TrySellCarriedAnimal(KickLuckyCubeWallet wallet)
        {
            if (carriedAnimal == null || wallet == null)
            {
                return false;
            }

            var soldAnimal = carriedAnimal;
            var sellValue = soldAnimal.SellValue;
            var animalName = soldAnimal.AnimalName;
            animalSpawner?.ReleaseCurrentAnimal(soldAnimal);
            carriedAnimal = null;

            DestroyAnimalObject(soldAnimal);
            wallet.AddSoft(sellValue);
            FinishCarriedAnimalFlow($"Sold {animalName}\n+{sellValue} soft\nReady for next kick.");
            return true;
        }

        public bool TryPlaceCarriedAnimal(KickLuckyCubeStableSlot stableSlot)
        {
            if (carriedAnimal == null || stableSlot == null)
            {
                return false;
            }

            var animal = carriedAnimal;
            if (!stableSlot.TryPlace(animal))
            {
                return false;
            }

            animalSpawner?.ReleaseCurrentAnimal(animal);
            carriedAnimal = null;
            FinishCarriedAnimalFlow($"{animal.AnimalName} placed in stable.\nIt now earns {animal.IncomePerSecond} soft/s.");
            return true;
        }

        private void OnCubeLanded(KickLuckyCubeKickResult result)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            BeginRunFromResult(result);
        }

        private void OnAnimalReturned(KickLuckyCubeAnimalRunner runner)
        {
            if (currentRunner != runner || runner == null)
            {
                return;
            }

            waveChase?.StopChase();
            ClearRunnerSubscription();

            carriedAnimal = runner.Animal;
            if (prototypePlayer != null)
            {
                prototypePlayer.SetActive(true);
                prototypePlayer.transform.position = new Vector3(0f, prototypePlayer.transform.position.y, GetReturnLineZ() - 1.4f);
            }

            if (carriedAnimal != null)
            {
                carriedAnimal.SetCarried(carryAnchor);
            }

            SetStatus(carriedAnimal != null
                ? $"{carriedAnimal.AnimalName} returned!\nCarry it to sell or stable next."
                : "Animal returned.\nSell/stable flow is next.");
        }

        private void OnAnimalCaught(KickLuckyCubeAnimalRunner runner)
        {
            ClearRunnerSubscription();
            animalSpawner?.ClearCurrentAnimal();
            carriedAnimal = null;
            kickController?.SetKickLocked(false);
            kickController?.ResetCubeToOrigin();

            if (prototypePlayer != null)
            {
                prototypePlayer.SetActive(true);
            }

            SetStatus("Wave caught the animal.\nTry another kick.");
        }

        private void FinishCarriedAnimalFlow(string status)
        {
            kickController?.SetKickLocked(false);
            kickController?.ResetCubeToOrigin();

            if (prototypePlayer != null)
            {
                prototypePlayer.SetActive(true);
            }

            SetStatus(status);
        }

        private void ClearRunnerSubscription()
        {
            if (currentRunner != null)
            {
                currentRunner.ReturnedToLine -= OnAnimalReturned;
                currentRunner.StopRun();
            }

            currentRunner = null;
        }

        private float GetReturnLineZ()
        {
            return returnLine != null ? returnLine.position.z : -2f;
        }

        private void SetStatus(string text)
        {
            if (hudText != null)
            {
                hudText.text = text;
            }

            if (worldStatusText != null)
            {
                worldStatusText.text = text;
            }
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
    }
}
