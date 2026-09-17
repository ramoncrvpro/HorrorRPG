using HorrorRPG.Interaction;
using UnityEngine;

namespace HorrorRPG.Interaction
{
    public class SceneLoadInteraction : MonoBehaviour, IInteractable
    {
        private const string DefaultPrompt = "Press E to enter";

        [Header("Scene Settings")]
        [SerializeField] private string sceneToLoad;
        [SerializeField] private string customPrompt = DefaultPrompt;
        [Header("Interaction Settings")]
        [SerializeField] private bool canInteract = true;

        /// <summary>Loads the configured catalog destination through the scene flow service.</summary>
        public void Interact(in InteractionContext context)
        {
            if (!CanInteract(in context)) return;
            context.SceneFlow.Load(sceneToLoad.Trim());
        }

        /// <summary>Returns the configured transition prompt.</summary>
        public string GetInteractionPrompt(in InteractionContext context) => customPrompt;

        /// <summary>Checks local state and whether the destination is enabled in the build.</summary>
        public bool CanInteract(in InteractionContext context)
        {
            return canInteract
                && !string.IsNullOrWhiteSpace(sceneToLoad)
                && context.SceneFlow.IsAvailableInBuild(sceneToLoad.Trim());
        }
    }
}
