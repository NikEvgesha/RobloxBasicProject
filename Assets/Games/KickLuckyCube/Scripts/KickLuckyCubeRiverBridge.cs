using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DisallowMultipleComponent]
    public sealed class KickLuckyCubeRiverBridge : MonoBehaviour
    {
        [SerializeField] private Collider bridgeBounds;
        [SerializeField, Min(0f)] private float horizontalPadding = 0.15f;

        public Collider BridgeBounds => bridgeBounds;

        public void AutoBind()
        {
            bridgeBounds ??= GetComponentInChildren<Collider>(true);
        }

        public bool ContainsHorizontal(Vector3 worldPosition)
        {
            if (!isActiveAndEnabled || bridgeBounds == null || !bridgeBounds.enabled)
            {
                return false;
            }

            var bounds = bridgeBounds.bounds;
            return worldPosition.x >= bounds.min.x - horizontalPadding
                && worldPosition.x <= bounds.max.x + horizontalPadding
                && worldPosition.z >= bounds.min.z - horizontalPadding
                && worldPosition.z <= bounds.max.z + horizontalPadding;
        }

        private void Reset()
        {
            AutoBind();
        }
    }
}
