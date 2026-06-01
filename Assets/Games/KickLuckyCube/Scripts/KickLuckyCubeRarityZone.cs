using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeRarityZone : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeRarity rarity = KickLuckyCubeRarity.Common;
        [SerializeField, Min(1)] private int zoneIndex = 1;
        [SerializeField] private string animalPoolText = "cat / dog";
        [SerializeField] private Renderer zoneRenderer;

        public KickLuckyCubeRarity Rarity => rarity;
        public int ZoneIndex => zoneIndex;
        public string AnimalPoolText => animalPoolText;

        public float StartZ => ResolveBounds().min.z;
        public float EndZ => ResolveBounds().max.z;

        private void Reset()
        {
            zoneRenderer = GetComponentInChildren<Renderer>();
        }

        private void Awake()
        {
            if (zoneRenderer == null)
            {
                zoneRenderer = GetComponentInChildren<Renderer>();
            }
        }

        public bool HasReached(float worldZ)
        {
            return worldZ >= StartZ;
        }

        public bool ContainsZ(float worldZ)
        {
            return worldZ >= StartZ && worldZ <= EndZ;
        }

        private Bounds ResolveBounds()
        {
            if (zoneRenderer != null)
            {
                return zoneRenderer.bounds;
            }

            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds;
            }

            var collider = GetComponentInChildren<Collider>();
            if (collider != null)
            {
                return collider.bounds;
            }

            return new Bounds(transform.position, Vector3.one);
        }
    }
}
