using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubePlotMobAnchor : MonoBehaviour
    {
        [SerializeField, Min(1)] private int variantNumber = 1;
        [SerializeField, Range(1, 3)] private int floorNumber = 1;
        [SerializeField, Min(1)] private int positionNumber = 1;

        public int VariantNumber => variantNumber;
        public int FloorNumber => floorNumber;
        public int PositionNumber => positionNumber;

        public void Configure(int variant, int floor, int position)
        {
            variantNumber = Mathf.Max(1, variant);
            floorNumber = Mathf.Clamp(floor, 1, 3);
            positionNumber = Mathf.Max(1, position);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.72f, 0.15f, 0.9f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(Vector3.zero, 0.7f);
            Gizmos.DrawLine(Vector3.zero, Vector3.forward);
        }
    }
}
