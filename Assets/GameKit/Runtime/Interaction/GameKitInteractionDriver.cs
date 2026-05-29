using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RobloxBasicProject.GameKit.Interaction
{
    public sealed class GameKitInteractionDriver : MonoBehaviour
    {
        [SerializeField] private GameObject actor;
        [SerializeField] private GameKitInteractionPromptView promptView;
        [SerializeField] private KeyCode fallbackKey = KeyCode.E;
        [SerializeField, Min(0f)] private float progressReleaseSpeed = 4f;

        private readonly Dictionary<Object, GameKitInteractionTarget> candidates = new Dictionary<Object, GameKitInteractionTarget>();
        private GameKitInteractionTarget currentTarget;
        private float holdProgress;
        private bool externalHold;

        public GameObject Actor => actor != null ? actor : gameObject;
        public GameKitInteractionTarget CurrentTarget => currentTarget;

        private void Awake()
        {
            if (actor == null)
            {
                actor = gameObject;
            }
        }

        private void Update()
        {
            currentTarget = SelectCurrentTarget();
            if (currentTarget == null)
            {
                holdProgress = 0f;
                promptView?.Hide();
                return;
            }

            var holdSeconds = Mathf.Max(0.05f, currentTarget.HoldSeconds);
            var isHeld = externalHold || ReadKeyboardHold();
            holdProgress = isHeld
                ? Mathf.Min(holdSeconds, holdProgress + Time.unscaledDeltaTime)
                : Mathf.Max(0f, holdProgress - Time.unscaledDeltaTime * progressReleaseSpeed);

            promptView?.Show(currentTarget, holdProgress / holdSeconds);

            if (holdProgress < holdSeconds)
            {
                return;
            }

            var target = currentTarget;
            holdProgress = 0f;
            target.Interact(Actor);
            currentTarget = SelectCurrentTarget();
        }

        public void SetExternalHold(bool isHeld)
        {
            externalHold = isHeld;
        }

        public void SetCandidate(Object source, GameKitInteractionTarget target)
        {
            if (source == null)
            {
                return;
            }

            if (target == null)
            {
                ClearCandidate(source);
                return;
            }

            candidates[source] = target;
        }

        public void ClearCandidate(Object source)
        {
            if (source == null)
            {
                return;
            }

            candidates.Remove(source);
        }

        public bool IsActorCollider(Collider other)
        {
            var actorTransform = Actor.transform;
            return other != null && (other.transform == actorTransform || other.transform.IsChildOf(actorTransform));
        }

        private GameKitInteractionTarget SelectCurrentTarget()
        {
            var actorPosition = Actor.transform.position;
            var deadSources = candidates
                .Where(pair => pair.Key == null || pair.Value == null || !pair.Value.CanInteract(Actor))
                .Select(pair => pair.Key)
                .ToArray();

            foreach (var source in deadSources)
            {
                candidates.Remove(source);
            }

            return candidates.Values
                .Where(target => target != null && target.CanInteract(Actor))
                .OrderByDescending(target => target.Priority)
                .ThenBy(target => (target.transform.position - actorPosition).sqrMagnitude)
                .FirstOrDefault();
        }

        private bool ReadKeyboardHold()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.eKey.isPressed;
#else
            return Input.GetKey(fallbackKey);
#endif
        }
    }
}
