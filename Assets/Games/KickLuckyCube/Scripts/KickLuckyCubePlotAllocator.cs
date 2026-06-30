using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubePlotAllocator : MonoBehaviour
    {
        private const string TemplateName = "KLC_PlotTemplate_EditSource";
        private const string StaticInstancesRootName = "KLC_PlotSlot_StaticInstances";
        private const string RuntimeInstancesRootName = "KLC_RuntimePlotInstances";
        private const string PlayerPlotName = "KLC_PlayerPlot_Instance";
        private const string BotPlotPrefix = "KLC_BotPlot_";

        [SerializeField] private KickLuckyCubePlotTemplate plotTemplate;
        [SerializeField] private KickLuckyCubePlotSlot[] plotSlots = Array.Empty<KickLuckyCubePlotSlot>();
        [SerializeField] private Transform runtimeInstancesRoot;
        [SerializeField] private bool allocateOnStart = true;
        [SerializeField] private bool allocateOnRuntimeLoad = true;
        [SerializeField] private bool hideTemplateAtRuntime = true;
        [SerializeField] private bool hideStaticPreviewInstancesAtRuntime = true;
        [SerializeField] private bool populateBotStableVisuals = true;
        [SerializeField, Min(0)] private int maximumBotPlots = 4;
        [SerializeField, Min(0)] private int botStableVisualsPerPlot = 3;
        [SerializeField, Min(0.1f)] private float botStableAnimalTargetHeight = 1.05f;
        [SerializeField] private string[] botNames =
        {
            "Astrozto",
            "MiraKit",
            "NoobPro77",
            "LuckyMax",
            "CubeFan"
        };

        private readonly List<KickLuckyCubePlotTemplate> spawnedPlots = new();
        private bool allocationCompleted;

        public KickLuckyCubePlotSlot PlayerSlot { get; private set; }
        public KickLuckyCubePlotTemplate PlayerPlot { get; private set; }
        public IReadOnlyList<KickLuckyCubePlotTemplate> SpawnedPlots => spawnedPlots;
        public event Action<KickLuckyCubePlotSlot> PlayerSlotAssigned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var allocator = FindObjectsByType<KickLuckyCubePlotAllocator>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault(candidate => candidate != null && candidate.enabled);

            if (allocator == null)
            {
                return;
            }

            if (allocator.allocateOnRuntimeLoad)
            {
                allocator.AllocatePlots();
            }
        }

        private void Awake()
        {
            ResolveRuntimeInstancesRoot();
        }

        private void Start()
        {
            if (allocateOnStart)
            {
                AllocatePlots();
            }
        }

        public void AllocatePlots()
        {
            AllocatePlots(false);
        }

        public void ReallocatePlots()
        {
            AllocatePlots(true);
        }

        private void AllocatePlots(bool force)
        {
            if (!Application.isPlaying || plotTemplate == null)
            {
                plotTemplate = ResolvePlotTemplate();
            }

            if (!Application.isPlaying || plotTemplate == null)
            {
                return;
            }

            if (allocationCompleted && !force)
            {
                return;
            }

            ResolveRuntimeInstancesRoot();
            ClearSpawnedPlots();

            var availableSlots = ResolveSlots();
            if (availableSlots.Count == 0)
            {
                return;
            }

            var playerCandidates = availableSlots.Where(slot => slot.CanHostPlayer).ToList();
            PlayerSlot = playerCandidates.Count > 0
                ? playerCandidates[UnityEngine.Random.Range(0, playerCandidates.Count)]
                : availableSlots[UnityEngine.Random.Range(0, availableSlots.Count)];

            PlayerPlot = SpawnPlot(PlayerSlot, "Player", true, 0);
            PlayerSlotAssigned?.Invoke(PlayerSlot);

            var botSlots = availableSlots
                .Where(slot => slot != PlayerSlot && slot.CanHostBot)
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(Mathf.Max(0, maximumBotPlots))
                .ToArray();

            for (var i = 0; i < botSlots.Length; i++)
            {
                var botName = botNames != null && botNames.Length > 0
                    ? botNames[i % botNames.Length]
                    : "Bot";
                SpawnPlot(botSlots[i], botName, false, i);
            }

            HideStaticPreviewInstances();

            if (hideTemplateAtRuntime)
            {
                plotTemplate.gameObject.SetActive(false);
            }

            allocationCompleted = true;
        }

        private List<KickLuckyCubePlotSlot> ResolveSlots()
        {
            if (plotSlots == null || plotSlots.Length == 0)
            {
                plotSlots = FindObjectsByType<KickLuckyCubePlotSlot>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);
            }

            return plotSlots
                .Where(slot => slot != null && slot.isActiveAndEnabled)
                .Distinct()
                .OrderBy(slot => slot.SlotId, StringComparer.Ordinal)
                .ToList();
        }

        private KickLuckyCubePlotTemplate SpawnPlot(KickLuckyCubePlotSlot slot, string ownerName, bool isPlayerOwned, int botIndex)
        {
            var rotation = slot.transform.rotation * plotTemplate.transform.localRotation;
            var instance = Instantiate(
                plotTemplate,
                slot.transform.position,
                rotation,
                runtimeInstancesRoot);

            instance.transform.localScale = slot.transform.lossyScale;
            instance.gameObject.SetActive(true);
            instance.ConfigureOwner(ownerName, isPlayerOwned);
            if (!isPlayerOwned)
            {
                PopulateBotStableVisuals(instance.transform, botIndex);
            }

            spawnedPlots.Add(instance);
            return instance;
        }

        private void PopulateBotStableVisuals(Transform plotRoot, int botIndex)
        {
            if (!populateBotStableVisuals || plotRoot == null || botStableVisualsPerPlot <= 0)
            {
                return;
            }

            var slotRoots = plotRoot.GetComponentsInChildren<Transform>(true)
                .Where(child => child.parent == plotRoot && child.name.StartsWith("Template_StableSlot_", StringComparison.Ordinal))
                .OrderBy(child => child.name, StringComparer.Ordinal)
                .ToArray();
            if (slotRoots.Length == 0)
            {
                return;
            }

            var visualCount = Mathf.Min(botStableVisualsPerPlot, slotRoots.Length);
            for (var index = 0; index < visualCount; index++)
            {
                var slotIndex = Mathf.Clamp(Mathf.RoundToInt(index * (slotRoots.Length - 1f) / Mathf.Max(1, visualCount - 1)), 0, slotRoots.Length - 1);
                var slotRoot = slotRoots[slotIndex];
                var anchor = FindChildByNamePart(slotRoot, "MobAnchor") ?? slotRoot;
                var locationIndex = Mathf.Max(1, botIndex * visualCount + index + 1);
                var options = KickLuckyCubeAnimalCatalog.CreateLocationOptions(locationIndex);
                if (options.Length == 0)
                {
                    continue;
                }

                var option = options[Mathf.Abs((botIndex * 17 + index * 7) % options.Length)];
                var visualRoot = new GameObject("KLC_BotStableVisual_" + option.AnimalName);
                visualRoot.transform.SetParent(plotRoot, true);
                visualRoot.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
                visualRoot.transform.localScale = Vector3.one;
                KickLuckyCubeAnimalVisualFactory.CreateVisual(
                    visualRoot.transform,
                    option,
                    botStableAnimalTargetHeight,
                    out _,
                    includeGradeEffects: true);
                FaceVisualTowardPlotCenter(visualRoot.transform, plotRoot);
                CreateBotStableLabel(visualRoot.transform, option);
            }
        }

        private static void FaceVisualTowardPlotCenter(Transform visualRoot, Transform plotRoot)
        {
            if (visualRoot == null || plotRoot == null)
            {
                return;
            }

            var direction = plotRoot.position - visualRoot.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
            {
                return;
            }

            visualRoot.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private static void CreateBotStableLabel(Transform visualRoot, KickLuckyCubeAnimalOption option)
        {
            if (visualRoot == null)
            {
                return;
            }

            var labelObject = new GameObject("KLC_BotStableLabel");
            labelObject.transform.SetParent(visualRoot, false);
            labelObject.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(60f, 0f, 0f);
            labelObject.transform.localScale = Vector3.one * 0.22f;

            var label = labelObject.AddComponent<TextMesh>();
            label.text = $"{option.AnimalName}\n+{option.IncomePerSecond}/s";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.16f;
            KickLuckyCubeUiTheme.StyleWorldText(label, Color.white, 0.012f);
        }

        private void ClearSpawnedPlots()
        {
            PlayerSlot = null;
            PlayerPlot = null;

            for (var i = spawnedPlots.Count - 1; i >= 0; i--)
            {
                var plot = spawnedPlots[i];
                if (plot == null)
                {
                    continue;
                }

                Destroy(plot.gameObject);
            }

            spawnedPlots.Clear();

            if (runtimeInstancesRoot == null)
            {
                return;
            }

            for (var i = runtimeInstancesRoot.childCount - 1; i >= 0; i--)
            {
                var child = runtimeInstancesRoot.GetChild(i);
                if (child == null
                    || (!string.Equals(child.name, PlayerPlotName, StringComparison.Ordinal)
                        && !child.name.StartsWith(BotPlotPrefix, StringComparison.Ordinal)))
                {
                    continue;
                }

                Destroy(child.gameObject);
            }
        }

        private KickLuckyCubePlotTemplate ResolvePlotTemplate()
        {
            if (plotTemplate != null)
            {
                return plotTemplate;
            }

            var templates = FindObjectsByType<KickLuckyCubePlotTemplate>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            return templates
                .Where(template => template != null
                    && !string.Equals(template.gameObject.name, PlayerPlotName, StringComparison.Ordinal)
                    && !template.gameObject.name.StartsWith(BotPlotPrefix, StringComparison.Ordinal))
                .OrderByDescending(template => string.Equals(template.gameObject.name, TemplateName, StringComparison.Ordinal))
                .FirstOrDefault();
        }

        private void ResolveRuntimeInstancesRoot()
        {
            if (runtimeInstancesRoot != null)
            {
                return;
            }

            runtimeInstancesRoot = FindChildByName(transform, RuntimeInstancesRootName);
            if (runtimeInstancesRoot != null)
            {
                return;
            }

            var root = new GameObject(RuntimeInstancesRootName);
            runtimeInstancesRoot = root.transform;
            runtimeInstancesRoot.SetParent(transform, false);
            runtimeInstancesRoot.localPosition = Vector3.zero;
            runtimeInstancesRoot.localRotation = Quaternion.identity;
            runtimeInstancesRoot.localScale = Vector3.one;
        }

        private void HideStaticPreviewInstances()
        {
            if (!hideStaticPreviewInstancesAtRuntime)
            {
                return;
            }

            var staticRoot = FindChildByName(transform, StaticInstancesRootName)
                ?? FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(candidate => string.Equals(candidate.name, StaticInstancesRootName, StringComparison.Ordinal));

            if (staticRoot != null)
            {
                staticRoot.gameObject.SetActive(false);
            }
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if (string.Equals(child.name, childName, StringComparison.Ordinal))
                {
                    return child;
                }
            }

            return null;
        }

        private static Transform FindChildByNamePart(Transform root, string namePart)
        {
            if (root == null || string.IsNullOrWhiteSpace(namePart))
            {
                return null;
            }

            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child =>
                    child != root
                    && child.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
