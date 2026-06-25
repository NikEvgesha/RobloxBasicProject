using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeWaveChaseController : MonoBehaviour
    {
        [SerializeField] private Transform waveVisual;
        [SerializeField, Min(0f)] private float startBehindDistance = 14f;
        [SerializeField, Min(0f)] private float catchDistance = 1.25f;
        [SerializeField, Min(0f)] private float speedMultiplier = 0.72f;
        [SerializeField, Min(0f)] private float minimumWaveSpeed = 4.5f;
        [SerializeField, Min(0f)] private float distanceSpeedMultiplier = 0.01f;
        [SerializeField, Min(0f)] private float maximumWaveSpeed = 34f;
        [SerializeField, Min(0f)] private float firstWaveLocationStartDistance = 7f;
        [SerializeField, Min(0.1f)] private float waveLocationLength = 24.2f;
        [SerializeField, Min(1)] private int locationsPerSpeedTier = 3;
        [SerializeField, Min(0f)] private float speedTierBonus = 1.15f;
        [SerializeField, Min(0f)] private float introRiseHeight = 4f;
        [SerializeField, Min(0f)] private float groundProbeHeight = 6f;
        [SerializeField, Min(0f)] private float groundProbeDistance = 18f;
        [SerializeField, Min(0f)] private float groundSinkOffset = 0.18f;
        [SerializeField] private bool hideWhenIdle = true;
        [SerializeField] private Vector3 speedLabelOffset = new(0f, 12.7f, -1.75f);
        [SerializeField, Min(1)] private int speedLabelFontSize = 96;
        [SerializeField, Min(0.001f)] private float speedLabelCharacterSize = 0.13f;
        [SerializeField] private Color speedLabelColor = Color.white;
        [SerializeField, Min(0.1f)] private float vignetteStartDistance = 10f;
        [SerializeField, Range(0f, 1f)] private float vignetteMaximumAlpha = 0.72f;
        [SerializeField] private Canvas vignetteCanvas;
        [SerializeField] private CanvasGroup vignetteGroup;

        private KickLuckyCubeAnimalRunner runner;
        private float waveSpeed;
        private bool chasing;
        private bool hasPreparedChase;
        private Vector3 preparedStartPosition;
        private TextMesh speedLabel;
        private int waveLocationIndex = 1;
        private int waveSpeedTierIndex;

        public event Action<KickLuckyCubeAnimalRunner> AnimalCaught;

        public bool IsChasing => chasing;
        public Transform WaveVisual => waveVisual != null ? waveVisual : transform;
        public float WaveSpeed => waveSpeed;
        public int WaveLocationIndex => waveLocationIndex;
        public int WaveSpeedTierIndex => waveSpeedTierIndex;
        public string WaveSpeedTierName => ResolveWaveSpeedTierName(waveSpeedTierIndex);

        private void Awake()
        {
            if (waveVisual == null)
            {
                waveVisual = transform;
            }

            if (hideWhenIdle && waveVisual != null)
            {
                waveVisual.gameObject.SetActive(false);
            }

            EnsureVignette();
            SetVignetteAlpha(0f);
        }

        private void Update()
        {
            if (waveVisual != null && waveVisual.gameObject.activeInHierarchy)
            {
                RefreshSpeedLabel();
            }

            if (!chasing || runner == null || waveVisual == null)
            {
                return;
            }

            waveVisual.position += Vector3.back * (waveSpeed * Time.deltaTime);
            RefreshVignette();

            if (waveVisual.position.z <= runner.transform.position.z + catchDistance)
            {
                var caughtRunner = runner;
                StopChase();
                AnimalCaught?.Invoke(caughtRunner);
            }
        }

        public void BeginChase(KickLuckyCubeAnimalRunner targetRunner)
        {
            BeginChase(targetRunner, 0f);
        }

        public void BeginChase(KickLuckyCubeAnimalRunner targetRunner, float kickDistance)
        {
            PrepareChase(targetRunner, kickDistance);
            StartPreparedChase();
        }

        public void PrepareChase(KickLuckyCubeAnimalRunner targetRunner, float kickDistance)
        {
            runner = targetRunner;
            if (runner == null || waveVisual == null)
            {
                chasing = false;
                hasPreparedChase = false;
                return;
            }

            waveLocationIndex = ResolveWaveLocationIndex(kickDistance);
            waveSpeedTierIndex = ResolveWaveSpeedTierIndex(waveLocationIndex);
            waveSpeed = CalculateWaveSpeed(runner.Speed, kickDistance);
            preparedStartPosition = ResolveStartPosition(runner);
            waveVisual.gameObject.SetActive(true);
            waveVisual.position = preparedStartPosition + Vector3.down * introRiseHeight;
            chasing = false;
            hasPreparedChase = true;
            EnsureSpeedLabel();
            RefreshSpeedLabel();
            EnsureVignette();
            SetVignetteAlpha(0f);
        }

        public IEnumerator PlayPreparedRise(float duration)
        {
            if (!hasPreparedChase || waveVisual == null)
            {
                yield break;
            }

            var startPosition = preparedStartPosition + Vector3.down * introRiseHeight;
            duration = Mathf.Max(0f, duration);
            if (duration <= 0f)
            {
                waveVisual.position = preparedStartPosition;
                RefreshSpeedLabel();
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                var progress = Mathf.Clamp01(elapsed / duration);
                var easedProgress = Mathf.SmoothStep(0f, 1f, progress);
                waveVisual.position = Vector3.LerpUnclamped(startPosition, preparedStartPosition, easedProgress);
                RefreshSpeedLabel();
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            waveVisual.position = preparedStartPosition;
            RefreshSpeedLabel();
        }

        public void StartPreparedChase()
        {
            if (!hasPreparedChase || runner == null || waveVisual == null)
            {
                chasing = false;
                return;
            }

            waveVisual.gameObject.SetActive(true);
            waveVisual.position = preparedStartPosition;
            chasing = true;
            hasPreparedChase = false;
            EnsureVignette();
            RefreshVignette();
            RefreshSpeedLabel();
        }

        public void StopChase()
        {
            chasing = false;
            hasPreparedChase = false;
            runner = null;

            if (hideWhenIdle && waveVisual != null)
            {
                waveVisual.gameObject.SetActive(false);
            }

            SetVignetteAlpha(0f);
        }

        public void ForceCatchForPrototype()
        {
            if (runner == null)
            {
                return;
            }

            var caughtRunner = runner;
            StopChase();
            AnimalCaught?.Invoke(caughtRunner);
        }

        private void RefreshVignette()
        {
            if (!chasing || runner == null || waveVisual == null)
            {
                SetVignetteAlpha(0f);
                return;
            }

            EnsureVignette();
            var distanceToRunner = Mathf.Max(0f, waveVisual.position.z - runner.transform.position.z);
            var danger01 = Mathf.InverseLerp(vignetteStartDistance, catchDistance, distanceToRunner);
            SetVignetteAlpha(Mathf.Clamp01(danger01) * vignetteMaximumAlpha);
        }

        private void SetVignetteAlpha(float alpha)
        {
            if (vignetteGroup == null)
            {
                return;
            }

            vignetteGroup.alpha = Mathf.Clamp01(alpha);
            vignetteGroup.interactable = false;
            vignetteGroup.blocksRaycasts = false;
        }

        private void EnsureVignette()
        {
            if (vignetteGroup != null)
            {
                return;
            }

            vignetteCanvas ??= FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (vignetteCanvas == null)
            {
                return;
            }

            var root = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_WaveDangerVignette", vignetteCanvas.transform);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            root.SetAsLastSibling();

            vignetteGroup = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<CanvasGroup>(root.gameObject);
            CreateVignetteEdge(root, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(0f, 160f));
            CreateVignetteEdge(root, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(0f, 160f));
            CreateVignetteEdge(root, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(160f, 0f));
            CreateVignetteEdge(root, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(160f, 0f));
        }

        private static void CreateVignetteEdge(
            RectTransform parent,
            string edgeName,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            var edge = KickLuckyCubeUiPrefabFactory.CreateRect("KLC_WaveDangerVignette_" + edgeName, parent);
            edge.anchorMin = anchorMin;
            edge.anchorMax = anchorMax;
            edge.pivot = pivot;
            edge.anchoredPosition = anchoredPosition;
            edge.sizeDelta = sizeDelta;

            var image = KickLuckyCubeUiPrefabFactory.GetOrAddComponent<Image>(edge.gameObject);
            image.color = new Color(1f, 0f, 0f, 0.62f);
            image.raycastTarget = false;
        }

        private float CalculateWaveSpeed(float runnerSpeed, float kickDistance)
        {
            var baseSpeed = Mathf.Max(minimumWaveSpeed, runnerSpeed * speedMultiplier);
            var distanceBonus = Mathf.Max(0f, kickDistance) * distanceSpeedMultiplier;
            var tierBonus = waveSpeedTierIndex * speedTierBonus;
            var scaledSpeed = baseSpeed + distanceBonus + tierBonus;
            return maximumWaveSpeed > 0f
                ? Mathf.Min(scaledSpeed, maximumWaveSpeed)
                : scaledSpeed;
        }

        private int ResolveWaveLocationIndex(float kickDistance)
        {
            var distancePastFirstZoneStart = Mathf.Max(0f, kickDistance - firstWaveLocationStartDistance);
            return Mathf.Max(1, Mathf.FloorToInt(distancePastFirstZoneStart / Mathf.Max(0.1f, waveLocationLength)) + 1);
        }

        private int ResolveWaveSpeedTierIndex(int locationIndex)
        {
            return Mathf.Max(0, (Mathf.Max(1, locationIndex) - 1) / Mathf.Max(1, locationsPerSpeedTier));
        }

        private static string ResolveWaveSpeedTierName(int tierIndex)
        {
            return tierIndex switch
            {
                0 => "Slow Wave",
                1 => "Steady Wave",
                2 => "Fast Wave",
                3 => "Very Fast Wave",
                4 => "Danger Wave",
                5 => "Wild Wave",
                6 => "Extreme Wave",
                7 => "Insane Wave",
                8 => "Mythic Wave",
                _ => "Impossible Wave",
            };
        }

        private Vector3 ResolveStartPosition(KickLuckyCubeAnimalRunner targetRunner)
        {
            var position = targetRunner.transform.position + Vector3.forward * startBehindDistance;
            position.y = ResolveGroundY(position, targetRunner.transform.position, targetRunner.transform) - groundSinkOffset;
            return position;
        }

        private float ResolveGroundY(Vector3 probePosition, Vector3 fallbackProbePosition, Transform ignoredRoot)
        {
            if (TryResolveGroundY(probePosition, ignoredRoot, out var groundY)
                || TryResolveGroundY(fallbackProbePosition, ignoredRoot, out groundY))
            {
                return groundY;
            }

            return fallbackProbePosition.y;
        }

        private bool TryResolveGroundY(Vector3 probePosition, Transform ignoredRoot, out float groundY)
        {
            var origin = probePosition + Vector3.up * groundProbeHeight;
            var maxDistance = groundProbeHeight + groundProbeDistance;
            var hits = Physics.RaycastAll(origin, Vector3.down, maxDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, static (left, right) => left.distance.CompareTo(right.distance));

            foreach (var hit in hits)
            {
                if (ignoredRoot != null && hit.transform != null && hit.transform.IsChildOf(ignoredRoot))
                {
                    continue;
                }

                groundY = hit.point.y;
                return true;
            }

            groundY = 0f;
            return false;
        }

        private void EnsureSpeedLabel()
        {
            if (speedLabel != null || waveVisual == null)
            {
                return;
            }

            var labelObject = new GameObject("KLC_WaveSpeedLabel");
            labelObject.transform.SetParent(waveVisual, false);
            labelObject.transform.localPosition = speedLabelOffset;

            speedLabel = labelObject.AddComponent<TextMesh>();
            speedLabel.anchor = TextAnchor.MiddleCenter;
            speedLabel.alignment = TextAlignment.Center;
            speedLabel.fontSize = speedLabelFontSize;
            speedLabel.characterSize = speedLabelCharacterSize;
            KickLuckyCubeUiTheme.StyleWorldText(speedLabel, speedLabelColor, 0.025f);
        }

        private void RefreshSpeedLabel()
        {
            if (waveVisual == null)
            {
                return;
            }

            EnsureSpeedLabel();
            if (speedLabel == null)
            {
                return;
            }

            speedLabel.transform.localPosition = speedLabelOffset;
            speedLabel.fontSize = speedLabelFontSize;
            speedLabel.characterSize = speedLabelCharacterSize;
            speedLabel.text = $"WAVE\n{WaveSpeedTierName}\nLoc {waveLocationIndex}\n{waveSpeed.ToString("0.0", CultureInfo.InvariantCulture)} m/s";
            KickLuckyCubeUiTheme.StyleWorldText(speedLabel, speedLabelColor, 0.025f);

            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                speedLabel.transform.rotation = Quaternion.LookRotation(
                    speedLabel.transform.position - mainCamera.transform.position,
                    Vector3.up);
            }
        }
    }
}
