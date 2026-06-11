using System;
using System.Linq;
using System.Reflection;
using RobloxBasicProject.GameKit.Interaction;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubeStablePlotRuntimeBinder : MonoBehaviour
    {
        private const BindingFlags SerializedFieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        [SerializeField] private string playerPlotInstanceName = "KLC_PlotInstance_MOVE_PlotSlot_01";
        [SerializeField] private string stableSlotPrefix = "Template_StableSlot_";
        [SerializeField] private string collectSpotNamePart = "CollectSpot";
        [SerializeField] private string upgradeBoardNamePart = "UpgradeBoard";
        [SerializeField] private string collectPromptText = "Поставить";
        [SerializeField] private Vector3 collectTriggerSize = new(1.8f, 2.4f, 1.4f);
        [SerializeField] private Vector3 upgradeTriggerSize = new(1.8f, 2.2f, 1.2f);
        [SerializeField, Min(0.1f)] private float stableAnimalTargetHeight = 1.05f;
        [SerializeField] private Vector3 stableAnimalLocalPosition = Vector3.zero;
        [SerializeField, Min(0f)] private float stableAnimalBottomOffset = 0.04f;
        [SerializeField] private Vector3 stableLabelWorldOffset = new(0f, 0.62f, 0f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FindFirstObjectByType<KickLuckyCubeStablePlotRuntimeBinder>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("KLC_StablePlotRuntimeBinder").AddComponent<KickLuckyCubeStablePlotRuntimeBinder>();
        }

        private void Awake()
        {
            ConfigurePlayerPlot();
        }

        private void Update()
        {
            ConfigurePlayerPlot();
        }

        public void ConfigurePlayerPlot()
        {
            var plot = ResolvePlayerPlot();
            if (plot == null)
            {
                return;
            }

            var slotRoots = plot.GetComponentsInChildren<Transform>(true)
                .Where(child => child.parent == plot.transform && child.name.StartsWith(stableSlotPrefix, StringComparison.Ordinal))
                .OrderBy(child => child.name, StringComparer.Ordinal)
                .ToArray();

            foreach (var slotRoot in slotRoots)
            {
                ConfigureStableSlot(slotRoot);
            }
        }

        private void ConfigureStableSlot(Transform slotRoot)
        {
            var stableSlot = GetOrAddComponent<KickLuckyCubeStableSlot>(slotRoot.gameObject);
            stableSlot.ConfigureStableVisuals(
                stableAnimalTargetHeight,
                stableAnimalLocalPosition,
                stableAnimalBottomOffset,
                stableLabelWorldOffset);

            var collectSpot = FindChildByNamePart(slotRoot, collectSpotNamePart) ?? slotRoot;
            ConfigureCollectSpot(collectSpot.gameObject, stableSlot);

            var upgradeBoard = FindChildByNamePart(slotRoot, upgradeBoardNamePart);
            if (upgradeBoard != null)
            {
                ConfigureUpgradeBoard(upgradeBoard.gameObject, stableSlot);
            }
        }

        private void ConfigureCollectSpot(GameObject collectSpot, KickLuckyCubeStableSlot stableSlot)
        {
            ConfigureTriggerCollider(collectSpot, collectTriggerSize);

            var target = GetOrAddComponent<GameKitInteractionTarget>(collectSpot);
            ConfigureInteractionTarget(target, collectPromptText, 25);

            if (collectSpot.GetComponent<GameKitInteractionTriggerSource>() == null)
            {
                collectSpot.AddComponent<GameKitInteractionTriggerSource>();
            }

            var placementPad = GetOrAddComponent<KickLuckyCubeStablePlacementPad>(collectSpot);
            SetPrivateField(placementPad, "stableSlot", stableSlot);
        }

        private void ConfigureUpgradeBoard(GameObject upgradeBoard, KickLuckyCubeStableSlot stableSlot)
        {
            ConfigureTriggerCollider(upgradeBoard, upgradeTriggerSize);

            var target = upgradeBoard.GetComponent<GameKitInteractionTarget>();
            if (target != null)
            {
                target.SetInteractable(false);
            }

            var triggerSource = upgradeBoard.GetComponent<GameKitInteractionTriggerSource>();
            if (triggerSource != null)
            {
                triggerSource.enabled = false;
            }

            var board = GetOrAddComponent<KickLuckyCubeStableUpgradeBoard>(upgradeBoard);
            board.Configure(stableSlot);
        }

        private GameObject ResolvePlayerPlot()
        {
            var playerPlot = GameObject.Find("KLC_PlayerPlot_Instance");
            if (playerPlot != null)
            {
                return playerPlot;
            }

            var configuredPlotObject = GameObject.Find(playerPlotInstanceName);
            if (configuredPlotObject != null)
            {
                return configuredPlotObject;
            }

            var allocator = FindFirstObjectByType<KickLuckyCubePlotAllocator>(FindObjectsInactive.Include);
            if (allocator != null && allocator.PlayerSlot != null)
            {
                var slotId = allocator.PlayerSlot.SlotId;
                if (!string.IsNullOrWhiteSpace(slotId))
                {
                    var plotBySlot = GameObject.Find("KLC_PlotInstance_" + slotId);
                    if (plotBySlot != null)
                    {
                        return plotBySlot;
                    }
                }
            }

            return null;
        }

        private static void ConfigureTriggerCollider(GameObject target, Vector3 triggerSize)
        {
            var collider = GetOrAddComponent<BoxCollider>(target);
            collider.isTrigger = true;

            collider.center = new Vector3(0f, triggerSize.y * 0.5f, 0f);
            collider.size = triggerSize;
        }

        private void ConfigureInteractionTarget(GameKitInteractionTarget target, string prompt, int priority)
        {
            SetPrivateField(target, "promptKey", "E");
            SetPrivateField(target, "promptText", prompt);
            SetPrivateField(target, "activationMode", GameKitInteractionActivationMode.Press);
            SetPrivateField(target, "holdSeconds", 0.05f);
            SetPrivateField(target, "priority", priority);
            SetPrivateField(target, "interactable", true);
        }

        private static Transform FindChildByNamePart(Transform root, string namePart)
        {
            return root.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(child =>
                    child != root
                    && child.name.IndexOf(namePart, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static T GetOrAddComponent<T>(GameObject target)
            where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
        {
            var field = typeof(TTarget).GetField(fieldName, SerializedFieldFlags);
            field?.SetValue(target, value);
        }
    }
}
