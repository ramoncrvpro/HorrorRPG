using UnityEngine;
using UnityEngine.Events;

namespace HorrorRPG.Interaction
{
    public class GenericInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "Press E to interact";
        [SerializeField] private bool canInteract = true;
        [SerializeField] private UnityEvent onInteract;

        /// <summary>Invokes the configured interaction event when enabled.</summary>
        public void Interact(in InteractionContext context)
        {
            if (CanInteract(in context)) onInteract?.Invoke();
        }

        /// <summary>Returns the configured prompt.</summary>
        public string GetInteractionPrompt(in InteractionContext context) => interactionPrompt;

        /// <summary>Returns whether this generic interaction is enabled.</summary>
        public bool CanInteract(in InteractionContext context) => canInteract;

        /// <summary>Enables or disables this interaction.</summary>
        public void SetCanInteract(bool value) => canInteract = value;
    }
}
