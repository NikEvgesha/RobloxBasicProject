using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    /// <summary>
    /// Keeps the authored plot floors independent so progression can install or remove
    /// any floor module without rebuilding the complete plot visual.
    /// </summary>
    public sealed class KickLuckyCubePlotFloorStack : MonoBehaviour
    {
        [Serializable]
        private sealed class FloorBinding
        {
            [SerializeField] private GameObject module;
            [SerializeField] private GameObject[] gameplayObjects = Array.Empty<GameObject>();
            [SerializeField] private bool installed;

            public GameObject Module => module;
            public GameObject[] GameplayObjects => gameplayObjects;
            public bool Installed { get => installed; set => installed = value; }

            public FloorBinding(GameObject module, GameObject[] gameplayObjects, bool installed)
            {
                this.module = module;
                this.gameplayObjects = gameplayObjects ?? Array.Empty<GameObject>();
                this.installed = installed;
            }
        }

        [SerializeField] private FloorBinding[] floors = Array.Empty<FloorBinding>();

        public int FloorCount => floors?.Length ?? 0;

        private void Awake()
        {
            ApplyInstalledState();
        }

        private void OnValidate()
        {
            ApplyInstalledState();
        }

        public bool IsFloorInstalled(int floorNumber)
        {
            var index = floorNumber - 1;
            return floors != null
                   && index >= 0
                   && index < floors.Length
                   && floors[index] != null
                   && floors[index].Installed;
        }

        public void SetFloorInstalled(int floorNumber, bool installed)
        {
            var index = floorNumber - 1;
            if (floors == null || index < 0 || index >= floors.Length || floors[index] == null)
            {
                Debug.LogWarning($"Floor {floorNumber} is outside the configured plot stack.", this);
                return;
            }

            floors[index].Installed = installed;
            ApplyFloorState(index);
        }

        public void Configure(GameObject[] modules, GameObject[][] gameplayObjects, bool installAllForPreview)
        {
            modules ??= Array.Empty<GameObject>();
            floors = new FloorBinding[modules.Length];

            for (var i = 0; i < floors.Length; i++)
            {
                var attachedGameplay = gameplayObjects != null && i < gameplayObjects.Length
                    ? gameplayObjects[i]
                    : Array.Empty<GameObject>();
                floors[i] = new FloorBinding(modules[i], attachedGameplay, installAllForPreview || i == 0);
            }

            ApplyInstalledState();
        }

        public void SetInstalledFloorCount(int installedFloorCount)
        {
            floors ??= Array.Empty<FloorBinding>();
            installedFloorCount = Mathf.Clamp(installedFloorCount, 0, floors.Length);
            for (var index = 0; index < floors.Length; index++)
            {
                if (floors[index] != null)
                {
                    floors[index].Installed = index < installedFloorCount;
                }
            }

            ApplyInstalledState();
        }

        private void ApplyInstalledState()
        {
            floors ??= Array.Empty<FloorBinding>();
            for (var i = 0; i < floors.Length; i++)
            {
                ApplyFloorState(i);
            }
        }

        private void ApplyFloorState(int index)
        {
            var floor = floors[index];
            if (floor == null)
            {
                return;
            }

            SetActiveIfNeeded(floor.Module, floor.Installed);
            foreach (var gameplayObject in floor.GameplayObjects)
            {
                SetActiveIfNeeded(gameplayObject, floor.Installed);
            }
        }

        private static void SetActiveIfNeeded(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }
    }
}
