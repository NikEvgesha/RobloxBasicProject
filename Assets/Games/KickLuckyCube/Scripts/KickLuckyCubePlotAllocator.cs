using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubePlotAllocator : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubePlotTemplate plotTemplate;
        [SerializeField] private KickLuckyCubePlotSlot[] plotSlots = Array.Empty<KickLuckyCubePlotSlot>();
        [SerializeField] private Transform runtimeInstancesRoot;
        [SerializeField] private bool allocateOnStart = true;
        [SerializeField] private bool hideTemplateAtRuntime = true;
        [SerializeField, Min(0)] private int maximumBotPlots = 4;
        [SerializeField] private string[] botNames =
        {
            "Astrozto",
            "MiraKit",
            "NoobPro77",
            "LuckyMax",
            "CubeFan"
        };

        private readonly List<KickLuckyCubePlotTemplate> spawnedPlots = new();

        public KickLuckyCubePlotSlot PlayerSlot { get; private set; }
        public event Action<KickLuckyCubePlotSlot> PlayerSlotAssigned;

        private void Awake()
        {
            if (runtimeInstancesRoot == null)
            {
                runtimeInstancesRoot = transform;
            }
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
            if (!Application.isPlaying || plotTemplate == null)
            {
                return;
            }

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

            SpawnPlot(PlayerSlot, "Player", true);
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
                SpawnPlot(botSlots[i], botName, false);
            }

            if (hideTemplateAtRuntime)
            {
                plotTemplate.gameObject.SetActive(false);
            }
        }

        private List<KickLuckyCubePlotSlot> ResolveSlots()
        {
            if (plotSlots == null || plotSlots.Length == 0)
            {
                plotSlots = FindObjectsByType<KickLuckyCubePlotSlot>(FindObjectsSortMode.None);
            }

            return plotSlots
                .Where(slot => slot != null && slot.isActiveAndEnabled)
                .Distinct()
                .ToList();
        }

        private void SpawnPlot(KickLuckyCubePlotSlot slot, string ownerName, bool isPlayerOwned)
        {
            var instance = Instantiate(
                plotTemplate,
                slot.transform.position,
                slot.transform.rotation,
                runtimeInstancesRoot);

            instance.transform.localScale = slot.transform.lossyScale;
            instance.gameObject.SetActive(true);
            instance.ConfigureOwner(ownerName, isPlayerOwned);
            spawnedPlots.Add(instance);
        }

        private void ClearSpawnedPlots()
        {
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
        }
    }
}
