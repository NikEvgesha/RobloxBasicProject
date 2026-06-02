using System.Linq;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [DefaultExecutionOrder(-11000)]
    public sealed class KickLuckyCubePlayerSpawnController : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private bool snapOnAwake = true;
        [SerializeField] private bool useSpawnRendererTop = true;

        private void Awake()
        {
            if (!snapOnAwake)
            {
                return;
            }

            SnapPlayerToSpawn();
        }

        public void SnapPlayerToSpawn()
        {
            ResolveReferences();

            if (player == null || spawnPoint == null)
            {
                return;
            }

            var targetPosition = spawnPoint.position;
            if (useSpawnRendererTop && spawnPoint.TryGetComponent<Renderer>(out var renderer))
            {
                targetPosition.y = renderer.bounds.max.y;
            }

            player.transform.SetPositionAndRotation(targetPosition, spawnPoint.rotation);
        }

        private void ResolveReferences()
        {
            if (player == null)
            {
                player = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(found => found.name == "KLC_PrototypePlayer");
            }

            if (spawnPoint == null)
            {
                spawnPoint = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(found => found.name == "MOVE_PlayerSpawn_BlueCube");
            }
        }
    }
}
