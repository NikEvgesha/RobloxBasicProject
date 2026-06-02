using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeToolTrainingController : MonoBehaviour
    {
        private static readonly string[] DefaultToolNames =
        {
            "Training Dumbbell",
            "Iron Kettlebell",
            "Heavy Barbell",
            "Gold Barbell",
            "Power Trainer"
        };

        [SerializeField] private KickLuckyCubePlayerStats stats;
        [SerializeField] private KickLuckyCubeTrainingBonusPrompt trainingBonusPrompt;
        [SerializeField] private KickLuckyCubeRunPhaseController runPhase;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private Transform playerVisual;
        [SerializeField] private string carryAnchorName = "KLC_CarryAnchor";
        [SerializeField] private string[] toolNames = DefaultToolNames;
        [SerializeField] private float[] strengthPerSecondByTier = { 8f, 14f, 24f, 40f, 66f };
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

        public event Action Changed;

        public bool IsTraining { get; private set; }
        public int CurrentToolTier => stats != null ? stats.SelectedStrengthToolTier : 1;
        public string CurrentToolName => GetToolName(CurrentToolTier);

        public float CurrentStrengthPerSecond => GetStrengthPerSecond(CurrentToolTier);

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
            Changed?.Invoke();
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
            Changed?.Invoke();
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

            var selectedTier = CurrentToolTier;
            if (!forceRebuild && toolPreview != null && displayedToolTier == selectedTier)
            {
                return;
            }

            DestroyToolPreview();
            displayedToolTier = selectedTier;
            toolPreview = new GameObject("KLC_SelectedToolHandPreview_" + CurrentToolName.Replace(" ", string.Empty));
            toolPreview.transform.SetParent(carryAnchor, false);
            toolPreview.transform.localPosition = new Vector3(0.08f, -0.02f, 0.02f);
            toolPreview.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            toolPreview.transform.localScale = Vector3.one;

            var tier01 = Mathf.InverseLerp(1f, 5f, selectedTier);
            var color = Color.Lerp(new Color(0.62f, 0.66f, 0.74f), new Color(1f, 0.78f, 0.20f), tier01);
            var barLength = 0.42f + selectedTier * 0.045f;
            var barWidth = 0.045f + selectedTier * 0.006f;
            var weightSize = 0.20f + selectedTier * 0.035f;
            var weightWidth = 0.09f + selectedTier * 0.014f;
            CreateToolPart("Bar", PrimitiveType.Cylinder, new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(barWidth, barLength, barWidth), color);
            CreateToolPart("LeftWeight", PrimitiveType.Cube, new Vector3(0f, -barLength - 0.05f, 0f), Quaternion.identity, new Vector3(weightSize, weightWidth, weightSize), color * 0.85f);
            CreateToolPart("RightWeight", PrimitiveType.Cube, new Vector3(0f, barLength + 0.05f, 0f), Quaternion.identity, new Vector3(weightSize, weightWidth, weightSize), color * 0.85f);

            if (selectedTier >= 3)
            {
                CreateToolPart("LeftPlate", PrimitiveType.Cube, new Vector3(0f, -barLength - 0.18f, 0f), Quaternion.identity, new Vector3(weightSize * 0.85f, weightWidth, weightSize * 0.85f), color * 0.72f);
                CreateToolPart("RightPlate", PrimitiveType.Cube, new Vector3(0f, barLength + 0.18f, 0f), Quaternion.identity, new Vector3(weightSize * 0.85f, weightWidth, weightSize * 0.85f), color * 0.72f);
            }
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

        private float GetStrengthPerSecond(int tier)
        {
            if (strengthPerSecondByTier != null)
            {
                var index = Mathf.Clamp(tier, 1, Mathf.Max(1, strengthPerSecondByTier.Length)) - 1;
                if (index >= 0 && index < strengthPerSecondByTier.Length && strengthPerSecondByTier[index] > 0f)
                {
                    return strengthPerSecondByTier[index];
                }
            }

            return baseStrengthPerSecond * Mathf.Pow(strengthGainMultiplier, Mathf.Max(1, tier) - 1);
        }

        private string GetToolName(int tier)
        {
            if (toolNames != null)
            {
                var index = Mathf.Clamp(tier, 1, Mathf.Max(1, toolNames.Length)) - 1;
                if (index >= 0 && index < toolNames.Length && !string.IsNullOrWhiteSpace(toolNames[index]))
                {
                    return toolNames[index];
                }
            }

            var defaultIndex = Mathf.Clamp(tier, 1, DefaultToolNames.Length) - 1;
            return defaultIndex >= 0 && defaultIndex < DefaultToolNames.Length
                ? DefaultToolNames[defaultIndex]
                : $"Tool {tier}";
        }
    }
}
