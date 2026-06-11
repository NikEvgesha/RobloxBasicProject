using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace RobloxBasicProject.GameKit.Interaction
{
    public enum GameKitInteractionActivationMode
    {
        Hold,
        Press
    }

    public sealed class GameKitInteractionTarget : MonoBehaviour
    {
        [SerializeField] private string promptKey = "E";
        [SerializeField] private string promptText = "Interact";
        [SerializeField] private GameKitInteractionActivationMode activationMode = GameKitInteractionActivationMode.Hold;
        [SerializeField, Min(0.05f)] private float holdSeconds = 0.65f;
        [SerializeField] private int priority;
        [SerializeField] private bool interactable = true;
        [SerializeField] private UnityEvent interacted = new UnityEvent();
        [SerializeField] private UnityEvent<GameObject> actorInteracted = new UnityEvent<GameObject>();

        public string PromptKey => promptKey;
        public string PromptText => promptText;
        public GameKitInteractionActivationMode ActivationMode => activationMode;
        public float HoldSeconds => holdSeconds;
        public bool ShowsProgress => activationMode == GameKitInteractionActivationMode.Hold;
        public int Priority => priority;
        public UnityEvent Interacted => interacted;
        public UnityEvent<GameObject> ActorInteracted => actorInteracted;

        public void SetInteractable(bool value)
        {
            interactable = value;
        }

        public void SetPrompt(string key, string text)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                promptKey = key;
            }

            if (!string.IsNullOrWhiteSpace(text))
            {
                promptText = text;
            }
        }

        public void SetPromptText(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                promptText = text;
            }
        }

        public bool CanInteract(GameObject actor)
        {
            if (!interactable || !isActiveAndEnabled)
            {
                return false;
            }

            return GetComponents<MonoBehaviour>()
                .OfType<GameKitInteractionCondition>()
                .All(condition => condition.CanInteract(actor));
        }

        public void Interact(GameObject actor)
        {
            if (!CanInteract(actor))
            {
                return;
            }

            interacted.Invoke();
            actorInteracted.Invoke(actor);
        }
    }
}
