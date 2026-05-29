using UnityEngine;

namespace RobloxBasicProject.GameKit.Interaction
{
    [RequireComponent(typeof(Collider))]
    public sealed class GameKitInteractionTriggerSource : MonoBehaviour
    {
        [SerializeField] private GameKitInteractionDriver driver;
        [SerializeField] private GameKitInteractionTarget target;

        private bool actorInside;

        private void Awake()
        {
            if (driver == null)
            {
                driver = FindFirstObjectByType<GameKitInteractionDriver>();
            }

            if (target == null)
            {
                target = GetComponent<GameKitInteractionTarget>();
            }

            var trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
        }

        private void OnDisable()
        {
            actorInside = false;
            driver?.ClearCandidate(this);
        }

        private void Update()
        {
            if (!actorInside || driver == null || target == null)
            {
                return;
            }

            driver.SetCandidate(this, target);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (driver == null || target == null || !driver.IsActorCollider(other))
            {
                return;
            }

            actorInside = true;
            driver.SetCandidate(this, target);
        }

        private void OnTriggerExit(Collider other)
        {
            if (driver == null || !driver.IsActorCollider(other))
            {
                return;
            }

            actorInside = false;
            driver.ClearCandidate(this);
        }
    }
}
