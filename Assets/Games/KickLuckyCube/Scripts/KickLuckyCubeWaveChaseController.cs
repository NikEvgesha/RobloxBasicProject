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
        [SerializeField, Min(0f)] private float distanceSpeedMultiplier = 0.035f;
        [SerializeField, Min(0f)] private float maximumWaveSpeed = 18f;
        [SerializeField, Min(0f)] private float introRiseHeight = 4f;
        [SerializeField] private bool hideWhenIdle = true;
        [SerializeField] private Vector3 speedLabelOffset = new(0f, 3.3f, 0f);
        [SerializeField, Min(1)] private int speedLabelFontSize = 64;
        [SerializeField, Min(0.001f)] private float speedLabelCharacterSize = 0.075f;
        [SerializeField] private Color speedLabelColor = new(1f, 0.35f, 0.2f);
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

        public event Action<KickLuckyCubeAnimalRunner> AnimalCaught;

        public bool IsChasing => chasing;
        public Transform WaveVisual => waveVisual != null ? waveVisual : transform;
        public float WaveSpeed => waveSpeed;

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

            var rootObject = new GameObject("KLC_WaveDangerVignette", typeof(RectTransform), typeof(CanvasGroup));
            var root = rootObject.GetComponent<RectTransform>();
            root.SetParent(vignetteCanvas.transform, false);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            root.SetAsLastSibling();

            vignetteGroup = rootObject.GetComponent<CanvasGroup>();
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
            var edgeObject = new GameObject(edgeName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var edge = edgeObject.GetComponent<RectTransform>();
            edge.SetParent(parent, false);
            edge.anchorMin = anchorMin;
            edge.anchorMax = anchorMax;
            edge.pivot = pivot;
            edge.anchoredPosition = anchoredPosition;
            edge.sizeDelta = sizeDelta;

            var image = edgeObject.GetComponent<Image>();
            image.color = new Color(1f, 0f, 0f, 0.62f);
            image.raycastTarget = false;
        }

        private float CalculateWaveSpeed(float runnerSpeed, float kickDistance)
        {
            var baseSpeed = Mathf.Max(minimumWaveSpeed, runnerSpeed * speedMultiplier);
            var scaledSpeed = baseSpeed + Mathf.Max(0f, kickDistance) * distanceSpeedMultiplier;
            return maximumWaveSpeed > 0f
                ? Mathf.Min(scaledSpeed, maximumWaveSpeed)
                : scaledSpeed;
        }

        private Vector3 ResolveStartPosition(KickLuckyCubeAnimalRunner targetRunner)
        {
            return targetRunner.transform.position + Vector3.forward * startBehindDistance + Vector3.up * 0.2f;
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
            speedLabel.color = speedLabelColor;
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
            speedLabel.text = $"WAVE\n{waveSpeed.ToString("0.0", CultureInfo.InvariantCulture)} m/s";
            speedLabel.color = speedLabelColor;

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
