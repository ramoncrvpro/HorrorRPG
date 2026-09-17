using HorrorRPG.Core;
using HorrorRPG.Interaction;
using UnityEngine;

namespace HorrorRPG.Dialogue
{
    public class DialogueTrigger : MonoBehaviour, IInteractable, IGameContextReceiver
    {
        private const string DefaultPrompt = "Pressione E para interagir";

        [Header("Dialogue Settings")]
        [SerializeField] private DialogueData dialogueData;
        [Header("Trigger Settings")]
        [SerializeField] private bool canInteract = true;
        [SerializeField] private bool oneTimeOnly;
        [Header("Persistence")]
        [SerializeField] private WorldObjectId worldObjectId;

        private GameContext gameContext;
        private DialogueService dialogueService;
        private DialogueHandle activeHandle;
        private bool hasActiveHandle;
        private bool hasInteracted;

        /// <summary>Injects the dialogue service used to observe completion.</summary>
        public void Initialize(GameContext context)
        {
            gameContext = context ?? throw new System.ArgumentNullException(nameof(context));
            dialogueService = context.Dialogue;
            if (worldObjectId == null) worldObjectId = GetComponent<WorldObjectId>();
            if (oneTimeOnly && worldObjectId != null && context.Session.World.IsCompleted(worldObjectId.Value)) hasInteracted = true;
        }

        /// <summary>Releases dialogue callbacks owned by this scene object.</summary>
        public void Deinitialize()
        {
            UnsubscribeFromCompletion();
            dialogueService = null;
            gameContext = null;
        }

        /// <summary>Starts the configured dialogue if no other request is active.</summary>
        public void Interact(in InteractionContext context)
        {
            if (!CanInteract(in context)) return;
            dialogueService = context.Dialogue;
            activeHandle = dialogueService.Start(new DialogueRequest(dialogueData.sentences, dialogueData.requiresConfirmation, dialogueData.fastText));
            hasActiveHandle = true;
            dialogueService.Ended += HandleDialogueEnded;
        }

        /// <summary>Returns the dialogue interaction prompt.</summary>
        public string GetInteractionPrompt(in InteractionContext context) => DefaultPrompt;

        /// <summary>Checks local completion state and global dialogue availability.</summary>
        public bool CanInteract(in InteractionContext context)
        {
            return canInteract
                && dialogueData != null
                && (!oneTimeOnly || !hasInteracted)
                && !hasActiveHandle
                && !IsDialogueActive(context.Dialogue.State);
        }

        /// <summary>Enables or disables this trigger.</summary>
        public void SetCanInteract(bool value) => canInteract = value;

        /// <summary>Clears the one-time completion state for a new session.</summary>
        public void ResetInteraction() => hasInteracted = false;

        private void HandleDialogueEnded(DialogueEndedEvent result)
        {
            if (!hasActiveHandle || !result.Handle.Equals(activeHandle)) return;
            if (oneTimeOnly && result.Reason == DialogueEndReason.Completed)
            {
                hasInteracted = true;
                if (worldObjectId != null && gameContext != null) gameContext.Session.World.MarkCompleted(worldObjectId.Value);
            }
            UnsubscribeFromCompletion();
        }

        private void UnsubscribeFromCompletion()
        {
            if (dialogueService != null) dialogueService.Ended -= HandleDialogueEnded;
            hasActiveHandle = false;
            activeHandle = default;
        }

        private static bool IsDialogueActive(DialogueState state)
        {
            return state == DialogueState.Typing
                || state == DialogueState.AwaitingAdvance
                || state == DialogueState.AwaitingConfirmation;
        }
    }
}
