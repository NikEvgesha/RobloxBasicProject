using System.Collections.Generic;
using UnityEngine;

namespace RobloxBasicProject.Games.KickLuckyCube
{
    [RequireComponent(typeof(Collider))]
    public sealed class KickLuckyCubeKickZonePreview : MonoBehaviour
    {
        [SerializeField] private KickLuckyCubeKickController kickController;

        private readonly Dictionary<GameObject, int> actorContacts = new();

        private void Awake()
        {
            kickController ??= FindFirstObjectByType<KickLuckyCubeKickController>(FindObjectsInactive.Include);

            var triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }

        private void OnDisable()
        {
            foreach (var actor in actorContacts.Keys)
            {
                kickController?.HideCubeInHands(actor);
            }

            actorContacts.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            var actor = ResolvePlayerActor(other);
            if (actor == null)
            {
                return;
            }

            actorContacts.TryGetValue(actor, out var contactCount);
            actorContacts[actor] = contactCount + 1;

            if (contactCount == 0)
            {
                kickController?.ShowCubeInHands(actor);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var actor = ResolvePlayerActor(other);
            if (actor == null || !actorContacts.TryGetValue(actor, out var contactCount))
            {
                return;
            }

            contactCount--;
            if (contactCount > 0)
            {
                actorContacts[actor] = contactCount;
                return;
            }

            actorContacts.Remove(actor);
            kickController?.HideCubeInHands(actor);
        }

        private static GameObject ResolvePlayerActor(Collider other)
        {
            return other != null
                ? other.GetComponentInParent<KickLuckyCubePlayerController>()?.gameObject
                : null;
        }
    }
}
