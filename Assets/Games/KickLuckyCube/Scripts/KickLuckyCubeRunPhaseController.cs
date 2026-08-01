using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeRunPhaseController : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeKickController kickController;
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeAnimalSpawner animalSpawner;
        [SerializeField] private KickLuckyCubeInventoryController inventory;
        [SerializeField] private KickLuckyCubeWaveChaseController waveChase;
        [SerializeField] private KickLuckyCubeThirdPersonCamera thirdPersonCamera;
        [SerializeField] private GameObject prototypePlayer;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private Transform returnLine;
        [SerializeField] private Text hudText;
        [SerializeField] private TextMesh worldStatusText;
        [SerializeField, Min(0f)] private float animalRouletteSeconds = 2.4f;
        [SerializeField, Min(0.02f)] private float animalRouletteMinimumStepSeconds = 0.07f;
        [SerializeField, Min(0.02f)] private float animalRouletteMaximumStepSeconds = 0.34f;
        [SerializeField] private Vector3 animalRoulettePreviewOffset = new(0f, 0.58f, 0f);
        [SerializeField] private Vector3 animalRoulettePreviewScale = new(0.72f, 0.58f, 1.05f);
        [SerializeField] private Color animalRouletteShadowColor = new(0.06f, 0.06f, 0.08f, 0.86f);
        [SerializeField, Min(0f)] private float returnedPlayerGroundOffset = 0.58f;
        [SerializeField, Min(0f)] private float waveIntroSeconds = 2.2f;
        [SerializeField, Min(0f)] private float waveIntroHoldSeconds = 0.35f;
        [SerializeField, Min(0f)] private float waveIntroCameraDistance = 4.8f;
        [SerializeField] private float waveIntroCameraPitch = 13f;
        [SerializeField] private float waveIntroCameraYawOffset;

        private KickLuckyCubeAnimalRunner currentRunner;
        private KickLuckyCubeSpawnedAnimal carriedAnimal;
        private Coroutine runStartRoutine;
        private GameObject roulettePreview;
        private Transform roulettePreviewBody;
        private Renderer[] roulettePreviewRenderers;
        private TextMesh roulettePreviewLabel;
        private string roulettePreviewVariantId;
        private bool roulettePreviewShowsSelected;
        private Vector3 currentReturnPosition;
        private bool hasCurrentReturnPosition;
        private string carriedInventoryAnimalId;

        public bool HasActiveRun => currentRunner != null && currentRunner.ControlEnabled;
        public bool HasCarriedAnimal => carriedAnimal != null;
        public KickLuckyCubeSpawnedAnimal CarriedAnimal => carriedAnimal;
        public bool IsSelectingAnimal => runStartRoutine != null && roulettePreview != null;
        public Transform RoulettePreviewTransform => roulettePreview != null ? roulettePreview.transform : null;

        private void Awake()
        {
            EnsureRouletteDefaults();
            kickController ??= FindFirstObjectByType<KickLuckyCubeKickController>();
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>();
            animalSpawner ??= FindFirstObjectByType<KickLuckyCubeAnimalSpawner>();
            inventory ??= FindFirstObjectByType<KickLuckyCubeInventoryController>(FindObjectsInactive.Include);
            waveChase ??= FindFirstObjectByType<KickLuckyCubeWaveChaseController>();
            thirdPersonCamera ??= FindFirstObjectByType<KickLuckyCubeThirdPersonCamera>(FindObjectsInactive.Include);

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

            if (inventory != null)
            {
                inventory.AnimalSold += OnInventoryAnimalSold;
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

            if (inventory != null)
            {
                inventory.AnimalSold -= OnInventoryAnimalSold;
            }

            if (currentRunner != null)
            {
                currentRunner.ReturnedToLine -= OnAnimalReturned;
            }

            StopRunStartRoutine();
        }

        public void BeginRunFromResult(KickLuckyCubeKickResult result)
        {
            if (!Application.isPlaying)
            {
                if (PrepareRun(result))
                {
                    StartRunWithAnimal(result, animalSpawner.PickRandomAnimal(result));
                }

                return;
            }

            StopRunStartRoutine();
            runStartRoutine = StartCoroutine(BeginRunAfterRoulette(result));
        }

        public void ResetToKickLine()
        {
            StopRunStartRoutine();
            ClearRunnerSubscription();
            carriedAnimal = null;
            carriedInventoryAnimalId = null;
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
            var animalName = soldAnimal.DisplayName;
            animalSpawner?.ReleaseCurrentAnimal(soldAnimal);
            carriedAnimal = null;
            RemoveCarriedInventoryItem();

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
            RemoveCarriedInventoryItem();
            FinishCarriedAnimalFlow($"{animal.DisplayName} placed in stable.\nIt now earns {animal.IncomePerSecond} soft/s.");
            return true;
        }

        private IEnumerator BeginRunAfterRoulette(KickLuckyCubeKickResult result)
        {
            if (!PrepareRun(result))
            {
                runStartRoutine = null;
                yield break;
            }

            var selectedOption = animalSpawner.PickRandomAnimal(result);
            yield return PlayAnimalRoulette(result, selectedOption);
            yield return StartRunWithAnimalAfterIntro(result, selectedOption);
            runStartRoutine = null;
        }

        private bool PrepareRun(KickLuckyCubeKickResult result)
        {
            if (animalSpawner == null || waveChase == null || stats == null)
            {
                return false;
            }

            ClearRunnerSubscription();
            DestroyRoulettePreview();
            animalSpawner.ClearCurrentAnimal();
            waveChase.StopChase();
            carriedAnimal = null;
            currentReturnPosition = result.KickOriginPosition;
            hasCurrentReturnPosition = true;
            kickController?.SetKickLocked(true);
            SetStatus($"Cube landed in {result.Rarity}.\nChoosing animal...");
            return true;
        }

        private void StartRunWithAnimal(KickLuckyCubeKickResult result, KickLuckyCubeAnimalOption selectedOption)
        {
            if (!TrySpawnRunner(result, selectedOption, out var spawnedAnimal))
            {
                return;
            }

            currentRunner.BeginRun(GetReturnLineZ());
            waveChase.BeginChase(currentRunner, result.Distance);
            SetRunStartedStatus(spawnedAnimal);
        }

        private IEnumerator StartRunWithAnimalAfterIntro(KickLuckyCubeKickResult result, KickLuckyCubeAnimalOption selectedOption)
        {
            if (!TrySpawnRunner(result, selectedOption, out var spawnedAnimal))
            {
                yield break;
            }

            waveChase.PrepareChase(currentRunner, result.Distance);

            var introFocusTarget = currentRunner != null
                ? currentRunner.transform
                : waveChase.WaveVisual;
            if (thirdPersonCamera != null && introFocusTarget != null && waveIntroSeconds > 0f)
            {
                thirdPersonCamera.BeginCinematicFocus(
                    introFocusTarget,
                    waveIntroCameraDistance,
                    waveIntroCameraPitch,
                    waveIntroCameraYawOffset);
                SetStatus($"Wave is rising...\n{waveChase.WaveSpeedTierName} | Location {waveChase.WaveLocationIndex}\nSpeed: {waveChase.WaveSpeed:0.0} m/s\nGet ready!");
                yield return waveChase.PlayPreparedRise(waveIntroSeconds);

                if (waveIntroHoldSeconds > 0f)
                {
                    yield return new WaitForSeconds(waveIntroHoldSeconds);
                }
            }

            currentRunner.BeginRun(GetReturnLineZ());
            thirdPersonCamera?.EndCinematicFocus(true);
            waveChase.StartPreparedChase();
            SetRunStartedStatus(spawnedAnimal);
        }

        private bool TrySpawnRunner(
            KickLuckyCubeKickResult result,
            KickLuckyCubeAnimalOption selectedOption,
            out KickLuckyCubeSpawnedAnimal spawnedAnimal)
        {
            spawnedAnimal = animalSpawner.Spawn(result, stats.AnimalSpeed, selectedOption);
            currentRunner = spawnedAnimal != null ? spawnedAnimal.GetComponent<KickLuckyCubeAnimalRunner>() : null;
            if (currentRunner == null)
            {
                return false;
            }

            currentRunner.ReturnedToLine += OnAnimalReturned;

            if (prototypePlayer != null)
            {
                prototypePlayer.SetActive(false);
            }

            return true;
        }

        private void SetRunStartedStatus(KickLuckyCubeSpawnedAnimal spawnedAnimal)
        {
            SetStatus($"Run back as {spawnedAnimal.DisplayName}!\n{spawnedAnimal.Rarity} / {spawnedAnimal.Grade}\n{waveChase.WaveSpeedTierName}: {waveChase.WaveSpeed:0.0} m/s");
        }

        private IEnumerator PlayAnimalRoulette(KickLuckyCubeKickResult result, KickLuckyCubeAnimalOption selectedOption)
        {
            var candidates = animalSpawner.GetCandidateOptions(result);
            if (candidates.Length == 0 || animalRouletteSeconds <= 0f)
            {
                yield break;
            }

            CreateRoulettePreview(result.LandingPosition);

            var elapsed = 0f;
            var index = 0;
            while (elapsed < animalRouletteSeconds)
            {
                var progress = Mathf.Clamp01(elapsed / animalRouletteSeconds);
                var option = candidates[index % candidates.Length];
                ApplyRoulettePreview(option, false);
                SetStatus($"Choosing animal...\n???\nLocation {ResolveLocationIndex(result)} pool");

                var stepSeconds = Mathf.Lerp(
                    animalRouletteMinimumStepSeconds,
                    animalRouletteMaximumStepSeconds,
                    Mathf.SmoothStep(0f, 1f, progress));
                yield return new WaitForSeconds(stepSeconds);

                elapsed += stepSeconds;
                index++;
            }

            ApplyRoulettePreview(selectedOption, true);
            SetStatus($"Selected: {selectedOption.DisplayName}!\n+{selectedOption.IncomePerSecond}/s");
            yield return new WaitForSeconds(0.45f);

            DestroyRoulettePreview();
        }

        private void CreateRoulettePreview(Vector3 landingPosition)
        {
            DestroyRoulettePreview();

            roulettePreview = new GameObject("KLC_AnimalRoulettePreview");
            roulettePreview.transform.SetPositionAndRotation(
                landingPosition,
                Quaternion.LookRotation(Vector3.back, Vector3.up));
            roulettePreviewVariantId = string.Empty;
            roulettePreviewShowsSelected = false;
            roulettePreviewRenderers = null;

            var labelObject = new GameObject("RouletteLabel");
            labelObject.transform.SetParent(roulettePreview.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.75f, 0f);
            roulettePreviewLabel = labelObject.AddComponent<TextMesh>();
            roulettePreviewLabel.anchor = TextAnchor.MiddleCenter;
            roulettePreviewLabel.alignment = TextAlignment.Center;
            roulettePreviewLabel.fontSize = 64;
            roulettePreviewLabel.characterSize = 0.055f;
            roulettePreviewLabel.color = Color.white;
        }

        private void ApplyRoulettePreview(KickLuckyCubeAnimalOption option, bool selected)
        {
            if (roulettePreview == null)
            {
                return;
            }

            option = KickLuckyCubeAnimalCatalog.ResolveOption(option);
            if (roulettePreviewBody == null
                || !string.Equals(roulettePreviewVariantId, option.VariantId, System.StringComparison.Ordinal)
                || roulettePreviewShowsSelected != selected)
            {
                RebuildRoulettePreviewVisual(option, selected);
            }

            if (roulettePreviewBody != null)
            {
                roulettePreviewBody.localPosition = new Vector3(
                    animalRoulettePreviewOffset.x,
                    0f,
                    animalRoulettePreviewOffset.z);
            }

            if (roulettePreviewLabel != null)
            {
                roulettePreviewLabel.text = selected
                    ? option.DisplayName
                    : "???";
                roulettePreviewLabel.color = selected ? new Color(1f, 0.88f, 0.22f) : Color.white;

                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    roulettePreviewLabel.transform.rotation = Quaternion.LookRotation(
                        roulettePreviewLabel.transform.position - mainCamera.transform.position,
                        Vector3.up);
                }
            }
        }

        private void RebuildRoulettePreviewVisual(KickLuckyCubeAnimalOption option, bool selected)
        {
            if (roulettePreview == null)
            {
                return;
            }

            if (roulettePreviewBody != null)
            {
                DestroyUnityObject(roulettePreviewBody.gameObject);
                roulettePreviewBody = null;
            }

            var visualRoot = new GameObject("RouletteVisual_" + Sanitize(option.AnimalName));
            visualRoot.transform.SetParent(roulettePreview.transform, false);
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one;
            roulettePreviewBody = visualRoot.transform;
            roulettePreviewVariantId = option.VariantId;
            roulettePreviewShowsSelected = selected;

            var targetHeight = selected ? 1.48f : 1.28f;
            var bodyRenderer = KickLuckyCubeAnimalVisualFactory.CreateVisual(
                visualRoot.transform,
                option,
                targetHeight,
                out var usedImportedVisual,
                selected);
            roulettePreviewRenderers = visualRoot.GetComponentsInChildren<Renderer>(true);

            if (!selected)
            {
                ApplyRouletteSilhouette();
                return;
            }

            if (!usedImportedVisual && bodyRenderer != null)
            {
                bodyRenderer.material.color = option.BodyColor;
            }
        }

        private void ApplyRouletteSilhouette()
        {
            if (roulettePreviewRenderers == null)
            {
                return;
            }

            foreach (var targetRenderer in roulettePreviewRenderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                var materials = targetRenderer.materials;
                for (var index = 0; index < materials.Length; index++)
                {
                    materials[index] = CreateRouletteSilhouetteMaterial(materials[index]);
                }

                targetRenderer.materials = materials;
            }
        }

        private Material CreateRouletteSilhouetteMaterial(Material source)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null && source != null)
            {
                shader = source.shader;
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            var material = shader != null
                ? new Material(shader)
                : new Material(source);
            material.name = "KLC_RouletteBlackSilhouette_Runtime";

            var color = animalRouletteShadowColor;
            color.a = 1f;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", Texture2D.blackTexture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", Texture2D.blackTexture);
            }

            return material;
        }

        private static string Sanitize(string value)
        {
            var source = string.IsNullOrWhiteSpace(value) ? "Animal" : value;
            var chars = source.ToCharArray();
            for (var index = 0; index < chars.Length; index++)
            {
                if (!char.IsLetterOrDigit(chars[index]))
                {
                    chars[index] = '_';
                }
            }

            return new string(chars);
        }

        private void StopRunStartRoutine()
        {
            if (runStartRoutine != null)
            {
                StopCoroutine(runStartRoutine);
                runStartRoutine = null;
            }

            DestroyRoulettePreview();
        }

        private void DestroyRoulettePreview()
        {
            roulettePreviewBody = null;
            roulettePreviewRenderers = null;
            roulettePreviewLabel = null;
            roulettePreviewVariantId = string.Empty;
            roulettePreviewShowsSelected = false;

            if (roulettePreview == null)
            {
                return;
            }

            DestroyUnityObject(roulettePreview);
            roulettePreview = null;
        }

        private void EnsureRouletteDefaults()
        {
            if (animalRouletteSeconds <= 0f)
            {
                animalRouletteSeconds = 2.4f;
            }

            if (animalRouletteMinimumStepSeconds <= 0f)
            {
                animalRouletteMinimumStepSeconds = 0.07f;
            }

            if (animalRouletteMaximumStepSeconds < animalRouletteMinimumStepSeconds)
            {
                animalRouletteMaximumStepSeconds = Mathf.Max(0.34f, animalRouletteMinimumStepSeconds);
            }

            if (animalRoulettePreviewOffset.sqrMagnitude <= 0.0001f)
            {
                animalRoulettePreviewOffset = new Vector3(0f, 0.58f, 0f);
            }

            if (animalRoulettePreviewScale.sqrMagnitude <= 0.0001f)
            {
                animalRoulettePreviewScale = new Vector3(0.72f, 0.58f, 1.05f);
            }

            if (animalRouletteShadowColor.a <= 0f)
            {
                animalRouletteShadowColor = new Color(0.06f, 0.06f, 0.08f, 0.86f);
            }
        }

        private void OnCubeLanded(KickLuckyCubeKickResult result)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            result = KickLuckyCubeFutureFeatureController.TryApplyWeatherBonus(result);
            BeginRunFromResult(result);
        }

        private void OnAnimalReturned(KickLuckyCubeAnimalRunner runner)
        {
            if (currentRunner != runner || runner == null)
            {
                return;
            }

            var returnedPosition = runner.transform.position;
            var returnedRotation = runner.transform.rotation;
            var returnedAnimal = runner.Animal;

            waveChase?.StopChase();
            ClearRunnerSubscription();

            if (prototypePlayer != null)
            {
                var playerPosition = new Vector3(
                    returnedPosition.x,
                    returnedPosition.y - returnedPlayerGroundOffset,
                    returnedPosition.z);
                prototypePlayer.SetActive(true);
                if (prototypePlayer.TryGetComponent<KickLuckyCubePlayerController>(out var playerController))
                {
                    playerController.TeleportTo(playerPosition, returnedRotation);
                }
                else
                {
                    prototypePlayer.transform.SetPositionAndRotation(playerPosition, returnedRotation);
                }
            }

            var hasReturnedAnimal = returnedAnimal != null;
            var returnedAnimalName = hasReturnedAnimal ? returnedAnimal.DisplayName : string.Empty;
            var addedToInventory = false;
            carriedAnimal = null;
            carriedInventoryAnimalId = null;

            if (hasReturnedAnimal)
            {
                KickLuckyCubeAnimalCollection.MarkDiscovered(returnedAnimal);

                if (inventory != null && inventory.TryAddAnimal(returnedAnimal, true, out _))
                {
                    addedToInventory = true;
                    animalSpawner?.ReleaseCurrentAnimal(returnedAnimal);
                    DestroyAnimalObject(returnedAnimal);
                }
                else
                {
                    carriedAnimal = returnedAnimal;
                    carriedAnimal.SetCarried(carryAnchor);
                    animalSpawner?.ReleaseCurrentAnimal(returnedAnimal);
                }
            }

            kickController?.SetKickLocked(false);
            kickController?.ResetCubeToOrigin();
            SetStatus(hasReturnedAnimal
                ? addedToInventory
                    ? $"{returnedAnimalName} returned!\nAdded to inventory and selected."
                    : $"{returnedAnimalName} returned!\nInventory is full."
                : "Animal returned.\nSell/stable flow is next.");
        }

        private void OnAnimalCaught(KickLuckyCubeAnimalRunner runner)
        {
            StopRunStartRoutine();
            ClearRunnerSubscription();
            animalSpawner?.ClearCurrentAnimal();
            carriedAnimal = null;
            carriedInventoryAnimalId = null;
            kickController?.SetKickLocked(false);
            kickController?.ResetCubeToOrigin();

            if (prototypePlayer != null)
            {
                prototypePlayer.SetActive(true);
            }

            SetStatus("Wave caught the animal.\nTry another kick.");
        }

        private void OnInventoryAnimalSold(KickLuckyCubeInventoryAnimal soldAnimal)
        {
            if (!soldAnimal.IsValid
                || string.IsNullOrWhiteSpace(carriedInventoryAnimalId)
                || !string.Equals(soldAnimal.Id, carriedInventoryAnimalId, System.StringComparison.Ordinal))
            {
                return;
            }

            var animalName = soldAnimal.DisplayName;
            DestroyAnimalObject(carriedAnimal);
            carriedAnimal = null;
            carriedInventoryAnimalId = null;
            SetStatus($"{animalName} sold.\nReady for next kick.");
        }

        private void FinishCarriedAnimalFlow(string status)
        {
            StopRunStartRoutine();
            kickController?.SetKickLocked(false);
            kickController?.ResetCubeToOrigin();

            if (prototypePlayer != null)
            {
                prototypePlayer.SetActive(true);
            }

            SetStatus(status);
        }

        private void RemoveCarriedInventoryItem()
        {
            if (inventory == null || string.IsNullOrWhiteSpace(carriedInventoryAnimalId))
            {
                carriedInventoryAnimalId = null;
                return;
            }

            inventory.TryRemoveAnimalById(carriedInventoryAnimalId, out _);
            carriedInventoryAnimalId = null;
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
            return hasCurrentReturnPosition
                ? currentReturnPosition.z
                : returnLine != null
                    ? returnLine.position.z
                    : -2f;
        }

        private static int ResolveLocationIndex(KickLuckyCubeKickResult result)
        {
            var rarityLocationIndex = result.Rarity switch
            {
                KickLuckyCubeRarity.Uncommon => 2,
                KickLuckyCubeRarity.Rare => 3,
                KickLuckyCubeRarity.Epic => 4,
                KickLuckyCubeRarity.Legendary => 5,
                _ => 1,
            };

            if (result.Zone != null)
            {
                return Mathf.Max(1, Mathf.Max(result.Zone.ZoneIndex, rarityLocationIndex));
            }

            return rarityLocationIndex;
        }

        private Vector3 GetReturnPosition()
        {
            return hasCurrentReturnPosition
                ? currentReturnPosition
                : returnLine != null
                    ? returnLine.position
                    : Vector3.zero;
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

            DestroyUnityObject(animal.gameObject);
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
