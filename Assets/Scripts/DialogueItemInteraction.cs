using HorrorRPG.Core;
using HorrorRPG.Interaction;
using HorrorRPG.Inventory;
using UnityEngine;

namespace HorrorRPG.Dialogue
{
    public class DialogueItemInteraction : MonoBehaviour, IInteractable, IGameContextReceiver
    {
        private const string DefaultPrompt = "Pressione E para interagir";

        [Header("Dialogue Settings")]
        [SerializeField] private DialogueData dialogueData;
        [Header("Item Configuration")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;
        [Header("Interaction Settings")]
        [SerializeField] private bool canBePickedUp = true;
        [SerializeField] private string customPrompt = string.Empty;
        [SerializeField] private bool oneTimeOnly;
        [Header("Persistence")]
        [SerializeField] private WorldObjectId worldObjectId;

        private GameContext gameContext;
        private DialogueHandle activeHandle;
        private bool hasActiveHandle;
        private bool hasInteracted;

        /// <summary>Injects services used by the dialogue and inventory transaction.</summary>
        public void Initialize(GameContext context)
        {
            gameContext = context ?? throw new System.ArgumentNullException(nameof(context));
            if (worldObjectId == null) worldObjectId = GetComponent<WorldObjectId>();
            if (worldObjectId != null && context.Session.World.IsCompleted(worldObjectId.Value))
            {
                hasInteracted = true;
                canBePickedUp = false;
            }
        }

        /// <summary>Releases the single completion callback owned by this interaction.</summary>
        public void Deinitialize()
        {
            UnsubscribeFromCompletion();
            gameContext = null;
        }

        /// <summary>Starts one confirmation dialogue and resolves the item transaction on completion.</summary>
        public void Interact(in InteractionContext context)
        {
            if (!CanInteract(in context)) return;
            gameContext = context.Game;
            activeHandle = context.Dialogue.Start(new DialogueRequest(dialogueData.sentences, true, dialogueData.fastText));
            hasActiveHandle = true;
            context.Dialogue.Ended += HandleDialogueEnded;
        }

        /// <summary>Returns the configured interaction prompt.</summary>
        public string GetInteractionPrompt(in InteractionContext context)
        {
            return string.IsNullOrWhiteSpace(customPrompt) ? DefaultPrompt : customPrompt;
        }

        /// <summary>Checks item, dialogue and one-time completion state.</summary>
        public bool CanInteract(in InteractionContext context)
        {
            return canBePickedUp
                && itemData != null
                && dialogueData != null
                && quantity > 0
                && (!oneTimeOnly || !hasInteracted)
                && !hasActiveHandle
                && !IsDialogueActive(context.Dialogue.State);
        }

        /// <summary>Enables or disables item collection.</summary>
        public void SetCanInteract(bool value) => canBePickedUp = value;

        /// <summary>Clears the one-time completion state for a new session.</summary>
        public void ResetInteraction() => hasInteracted = false;

        private void HandleDialogueEnded(DialogueEndedEvent result)
        {
            if (!hasActiveHandle || !result.Handle.Equals(activeHandle)) return;
            if (result.Reason == DialogueEndReason.Completed && gameContext != null)
            {
                InventoryOperationResult inventoryResult = gameContext.Inventory.AddItem(itemData, quantity);
                quantity -= inventoryResult.ProcessedQuantity;
                if (quantity <= 0)
                {
                    canBePickedUp = false;
                    if (oneTimeOnly) hasInteracted = true;
                    if (worldObjectId != null) gameContext.Session.World.MarkCompleted(worldObjectId.Value);
                }
            }
            UnsubscribeFromCompletion();
        }

        private void UnsubscribeFromCompletion()
        {
            if (gameContext != null) gameContext.Dialogue.Ended -= HandleDialogueEnded;
            hasActiveHandle = false;
            activeHandle = default;
        }

        private static bool IsDialogueActive(DialogueState state)
        {
            return state == DialogueState.Typing
                || state == DialogueState.AwaitingAdvance
                || state == DialogueState.AwaitingConfirmation;
        }

        private void OnValidate()
        {
            quantity = Mathf.Max(1, quantity);
        }
    }
}
