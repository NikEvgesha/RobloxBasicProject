using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DefaultExecutionOrder(100)]
    public sealed class KickLuckyCubePlayerKickBoundary : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField, Min(0f)] private float padding = 0.05f;

        private void Awake()
        {
            ResolvePlayer();
        }

        private void Update()
        {
            ResolvePlayer();

            if (player == null || !player.activeInHierarchy)
            {
                return;
            }

            var position = player.transform.position;
            var maximumZ = transform.position.z - padding;
            if (position.z <= maximumZ)
            {
                return;
            }

            position.z = maximumZ;
            player.transform.position = position;
        }

        private void ResolvePlayer()
        {
            if (player != null)
            {
                return;
            }

            player = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(found => found.name == "KLC_PrototypePlayer");
        }
    }
}
