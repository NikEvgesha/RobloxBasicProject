using System;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeRiverTraversalZone : MonoBehaviour
    {
        [SerializeField] private BoxCollider waterBounds;
        [SerializeField] private KickLuckyCubeRiverBridge[] bridges = Array.Empty<KickLuckyCubeRiverBridge>();
        [SerializeField, Range(0.1f, 1f)] private float animalSpeedMultiplier = 0.9f;
        [SerializeField, Range(0f, 1f)] private float animalSinkDepth = 0.22f;
        [SerializeField, Min(1)] private int activeBridgeCount = 2;
        [SerializeField] private bool randomizeBridgesOnPlay = true;

        public float AnimalSpeedMultiplier => Mathf.Clamp(animalSpeedMultiplier, 0.1f, 1f);
        public float AnimalSinkDepth => Mathf.Max(0f, animalSinkDepth);
        public int ActiveBridgeCount
        {
            get
            {
                var count = 0;
                foreach (var bridge in bridges)
                {
                    if (bridge != null && bridge.gameObject.activeSelf)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private void Awake()
        {
            AutoBind();
            if (Application.isPlaying)
            {
                ConfigureRuntimeBridges();
            }
        }

        public void AutoBind()
        {
            waterBounds ??= GetComponent<BoxCollider>();
            if (bridges == null || bridges.Length == 0)
            {
                bridges = GetComponentsInChildren<KickLuckyCubeRiverBridge>(true);
            }

            foreach (var bridge in bridges)
            {
                bridge?.AutoBind();
            }
        }

        public bool TryGetAnimalModifier(Vector3 worldPosition, out float speedMultiplier, out float sinkOffset)
        {
            speedMultiplier = 1f;
            sinkOffset = 0f;
            if (!isActiveAndEnabled || waterBounds == null || !ContainsHorizontal(waterBounds.bounds, worldPosition))
            {
                return false;
            }

            foreach (var bridge in bridges)
            {
                if (bridge != null && bridge.ContainsHorizontal(worldPosition))
                {
                    return false;
                }
            }

            speedMultiplier = AnimalSpeedMultiplier;
            sinkOffset = -AnimalSinkDepth;
            return true;
        }

        private void ConfigureRuntimeBridges()
        {
            if (bridges == null || bridges.Length == 0)
            {
                return;
            }

            var order = new int[bridges.Length];
            for (var index = 0; index < order.Length; index++)
            {
                order[index] = index;
            }

            if (randomizeBridgesOnPlay)
            {
                for (var index = order.Length - 1; index > 0; index--)
                {
                    var swapIndex = UnityEngine.Random.Range(0, index + 1);
                    (order[index], order[swapIndex]) = (order[swapIndex], order[index]);
                }
            }

            var enabledCount = Mathf.Clamp(activeBridgeCount, 1, bridges.Length);
            for (var orderIndex = 0; orderIndex < order.Length; orderIndex++)
            {
                var bridge = bridges[order[orderIndex]];
                if (bridge != null)
                {
                    bridge.gameObject.SetActive(orderIndex < enabledCount);
                }
            }
        }

        private static bool ContainsHorizontal(Bounds bounds, Vector3 worldPosition)
        {
            return worldPosition.x >= bounds.min.x
                && worldPosition.x <= bounds.max.x
                && worldPosition.z >= bounds.min.z
                && worldPosition.z <= bounds.max.z;
        }

        private void Reset()
        {
            AutoBind();
        }

        private void OnValidate()
        {
            activeBridgeCount = Mathf.Max(1, activeBridgeCount);
        }
    }
}
