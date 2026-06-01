using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    public sealed class KickLuckyCubePlotSlot : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private bool canHostPlayer = true;
        [SerializeField] private bool canHostBot = true;

        public string SlotId => string.IsNullOrWhiteSpace(slotId) ? gameObject.name : slotId;
        public bool CanHostPlayer => canHostPlayer;
        public bool CanHostBot => canHostBot;

        private void Reset()
        {
            slotId = gameObject.name;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(slotId))
            {
                slotId = gameObject.name;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = canHostPlayer
                ? new Color(0.2f, 0.9f, 0.45f, 0.45f)
                : new Color(0.25f, 0.55f, 1f, 0.35f);
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(18f, 0.25f, 16f));
        }
    }
}
