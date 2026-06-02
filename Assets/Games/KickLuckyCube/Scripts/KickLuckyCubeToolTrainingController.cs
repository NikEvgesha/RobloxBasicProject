using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeToolTrainingController : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeTrainingBonusPrompt trainingBonusPrompt;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private Transform playerVisual;
        [SerializeField] private string carryAnchorName = "KLC_CarryAnchor";
        [SerializeField, Min(0.1f)] private float baseStrengthPerSecond = 8f;
        [SerializeField, Min(1f)] private float strengthGainMultiplier = 1.65f;
        [SerializeField, Min(0.5f)] private float bonusIntervalSeconds = 5f;
        [SerializeField, Min(0.1f)] private float squatFrequency = 1.8f;
        [SerializeField, Min(0f)] private float squatDepth = 0.18f;
        [SerializeField, Min(0f)] private float squatYOffset = 0.12f;

        private GameObject toolPreview;
        private Vector3 playerVisualDefaultScale;
        private Vector3 playerVisualDefaultPosition;
        private float strengthTickTimer;
        private float bonusTimer;
        private int displayedToolTier;
        private bool hasPlayerVisualDefaults;

        public bool IsTraining { get; private set; }

        public float CurrentStrengthPerSecond => stats != null
            ? baseStrengthPerSecond * Mathf.Pow(strengthGainMultiplier, stats.SelectedStrengthToolTier - 1)
            : baseStrengthPerSecond;

        private void Awake()
        {
            ResolveReferences();
            CapturePlayerVisualDefaults();
        }

        private void OnDisable()
        {
            StopTraining();
        }

        private void Update()
        {
            if (!IsTraining)
            {
                return;
            }

            if (!CanTrain())
            {
                StopTraining();
                return;
            }

            var deltaTime = Time.unscaledDeltaTime;
            strengthTickTimer += deltaTime;
            bonusTimer += deltaTime;

            while (strengthTickTimer >= 1f)
            {
                strengthTickTimer -= 1f;
                stats.AddStrength(CurrentStrengthPerSecond);
            }

            if (bonusTimer >= bonusIntervalSeconds)
            {
                bonusTimer = 0f;
                if (trainingBonusPrompt == null || !trainingBonusPrompt.IsVisible)
                {
                    trainingBonusPrompt?.ShowBonus(CurrentStrengthPerSecond * 2f, stats.SelectedStrengthToolTier);
                }
            }

            RefreshToolPreview();
            AnimateSquat();
        }

        public void ToggleTraining()
        {
            if (IsTraining)
            {
                StopTraining();
            }
            else
            {
                StartTraining();
            }
        }

        public bool StartTraining()
        {
            ResolveReferences();
            if (!CanTrain())
            {
                return false;
            }

            IsTraining = true;
            strengthTickTimer = 0f;
            bonusTimer = 0f;
            CapturePlayerVisualDefaults();
            RefreshToolPreview(true);
            AnimateSquat();
            return true;
        }

        public void StopTraining()
        {
            if (!IsTraining && toolPreview == null)
            {
                RestorePlayerVisual();
                return;
            }

            IsTraining = false;
            strengthTickTimer = 0f;
            bonusTimer = 0f;
            DestroyToolPreview();
            RestorePlayerVisual();
        }

        private bool CanTrain()
        {
            ResolveReferences();
            return stats != null
                && (runPhase == null || (!runPhase.HasActiveRun && !runPhase.HasCarriedAnimal && !runPhase.IsSelectingAnimal));
        }

        private void ResolveReferences()
        {
            stats ??= FindFirstObjectByType<KickLuckyCubePlayerStats>(FindObjectsInactive.Include);
            trainingBonusPrompt ??= FindFirstObjectByType<KickLuckyCubeTrainingBonusPrompt>(FindObjectsInactive.Include);
            runPhase ??= FindFirstObjectByType<KickLuckyCubeRunPhaseController>(FindObjectsInactive.Include);

            if (carryAnchor == null)
            {
                var anchorObject = GameObject.Find(carryAnchorName);
                carryAnchor = anchorObject != null ? anchorObject.transform : null;
            }

            if (playerVisual == null)
            {
                var visualObject = GameObject.Find("KLC_PlayerVisual");
                playerVisual = visualObject != null ? visualObject.transform : null;
            }
        }

        private void CapturePlayerVisualDefaults()
        {
            if (playerVisual == null || hasPlayerVisualDefaults)
            {
                return;
            }

            playerVisualDefaultScale = playerVisual.localScale;
            playerVisualDefaultPosition = playerVisual.localPosition;
            hasPlayerVisualDefaults = true;
        }

        private void AnimateSquat()
        {
            if (playerVisual == null)
            {
                return;
            }

            CapturePlayerVisualDefaults();
            var squatAmount = (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * squatFrequency) + 1f) * 0.5f;
            var horizontalScale = 1f + squatAmount * squatDepth * 0.36f;
            var verticalScale = 1f - squatAmount * squatDepth;
            playerVisual.localScale = new Vector3(
                playerVisualDefaultScale.x * horizontalScale,
                playerVisualDefaultScale.y * verticalScale,
                playerVisualDefaultScale.z * horizontalScale);
            playerVisual.localPosition = playerVisualDefaultPosition + Vector3.down * (squatAmount * squatYOffset);
        }

        private void RestorePlayerVisual()
        {
            if (playerVisual == null || !hasPlayerVisualDefaults)
            {
                return;
            }

            playerVisual.localScale = playerVisualDefaultScale;
            playerVisual.localPosition = playerVisualDefaultPosition;
        }

        private void RefreshToolPreview(bool forceRebuild = false)
        {
            ResolveReferences();
            if (carryAnchor == null || stats == null)
            {
                return;
            }

            var selectedTier = stats.SelectedStrengthToolTier;
            if (!forceRebuild && toolPreview != null && displayedToolTier == selectedTier)
            {
                return;
            }

            DestroyToolPreview();
            displayedToolTier = selectedTier;
            toolPreview = new GameObject("KLC_SelectedToolHandPreview");
            toolPreview.transform.SetParent(carryAnchor, false);
            toolPreview.transform.localPosition = new Vector3(0.08f, -0.02f, 0.02f);
            toolPreview.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            toolPreview.transform.localScale = Vector3.one;

            var color = Color.Lerp(new Color(0.62f, 0.66f, 0.74f), new Color(1f, 0.78f, 0.20f), Mathf.InverseLerp(1f, 5f, selectedTier));
            CreateToolPart("Bar", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(0.06f, 0.46f, 0.06f), color);
            CreateToolPart("LeftWeight", PrimitiveType.Cube, new Vector3(0f, -0.5f, 0f), Quaternion.identity, new Vector3(0.28f, 0.12f, 0.28f), color * 0.85f);
            CreateToolPart("RightWeight", PrimitiveType.Cube, new Vector3(0f, 0.5f, 0f), Quaternion.identity, new Vector3(0.28f, 0.12f, 0.28f), color * 0.85f);
        }

        private void CreateToolPart(
            string partName,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Color color)
        {
            var part = GameObject.CreatePrimitive(primitiveType);
            part.name = partName;
            part.transform.SetParent(toolPreview.transform, false);
            part.transform.localPosition = localPosition;
            part.transform.localRotation = localRotation;
            part.transform.localScale = localScale;

            var collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(renderer.sharedMaterial)
                {
                    color = color
                };
                renderer.sharedMaterial = material;
            }
        }

        private void DestroyToolPreview()
        {
            displayedToolTier = 0;
            if (toolPreview == null)
            {
                return;
            }

            toolPreview.SetActive(false);
            Destroy(toolPreview);
            toolPreview = null;
        }
    }
}
