using UnityEngine;

namespace RobloxBasicProject.GameKit.Interaction
{
    public sealed class GameKitInteractionRaycastSource : MonoBehaviour
    {
        public enum RaycastMode
        {
            ForwardFromOrigin,
            DownFromActor
        }

        [SerializeField] private GameKitInteractionDriver driver;
        [SerializeField] private Transform origin;
        [SerializeField] private RaycastMode mode;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private LayerMask layerMask = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        private GameKitInteractionTarget currentTarget;

        private void Awake()
        {
            if (driver == null)
            {
                driver = FindFirstObjectByType<GameKitInteractionDriver>();
            }

            if (origin == null)
            {
                origin = transform;
            }
        }

        private void OnDisable()
        {
            ClearCurrent();
        }

        private void Update()
        {
            if (driver == null || origin == null)
            {
                ClearCurrent();
                return;
            }

            var ray = CreateRay();
            var hit = FindFirstValidHit(ray);
            if (!hit.HasValue)
            {
                ClearCurrent();
                return;
            }

            var target = hit.Value.collider.GetComponentInParent<GameKitInteractionTarget>();
            if (target == null || !target.CanInteract(driver.Actor))
            {
                ClearCurrent();
                return;
            }

            currentTarget = target;
            driver.SetCandidate(this, currentTarget);
        }

        private Ray CreateRay()
        {
            if (mode == RaycastMode.DownFromActor)
            {
                var actor = driver != null ? driver.Actor.transform : origin;
                return new Ray(actor.position + Vector3.up * 0.2f, Vector3.down);
            }

            return new Ray(origin.position, origin.forward);
        }

        private RaycastHit? FindFirstValidHit(Ray ray)
        {
            var hits = Physics.RaycastAll(ray, maxDistance, layerMask, triggerInteraction);
            if (hits.Length == 0)
            {
                return null;
            }

            System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
            foreach (var hit in hits)
            {
                if (driver != null && driver.IsActorCollider(hit.collider))
                {
                    continue;
                }

                return hit;
            }

            return null;
        }

        private void ClearCurrent()
        {
            if (currentTarget == null)
            {
                return;
            }

            currentTarget = null;
            driver?.ClearCandidate(this);
        }
    }
}
