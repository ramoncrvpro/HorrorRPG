using HorrorRPG.Core;
using HorrorRPG.Inventory;
using UnityEngine;

namespace HorrorRPG.Interaction
{
    public class ItemInteraction : MonoBehaviour, IInteractable, IGameContextReceiver
    {
        private const string DefaultPrompt = "Press E to pick up item";

        [Header("Item Configuration")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;
        [Header("Persistence")]
        [SerializeField] private WorldObjectId worldObjectId;
        [Header("Interaction Settings")]
        [SerializeField] private bool canBePickedUp = true;
        [SerializeField] private string customPrompt = string.Empty;

        private GameContext gameContext;

        /// <summary>Connects this pickup to runtime world completion state.</summary>
        public void Initialize(GameContext context)
        {
            gameContext = context ?? throw new System.ArgumentNullException(nameof(context));
            if (worldObjectId == null) worldObjectId = GetComponent<WorldObjectId>();
            if (worldObjectId != null && context.Session.World.IsCompleted(worldObjectId.Value)) gameObject.SetActive(false);
        }

        /// <summary>Releases the runtime context.</summary>
        public void Deinitialize() => gameContext = null;

        /// <summary>Adds the available quantity through the authoritative inventory service.</summary>
        public void Interact(in InteractionContext context)
        {
            if (!CanInteract(in context)) return;
            InventoryOperationResult result = context.Inventory.AddItem(itemData, quantity);
            quantity -= result.ProcessedQuantity;
            if (quantity > 0) return;
            if (worldObjectId != null) context.Game.Session.World.MarkCompleted(worldObjectId.Value);
            gameObject.SetActive(false);
        }

        /// <summary>Builds the pickup prompt for the remaining quantity.</summary>
        public string GetInteractionPrompt(in InteractionContext context)
        {
            if (!string.IsNullOrWhiteSpace(customPrompt)) return customPrompt;
            if (itemData == null) return DefaultPrompt;
            return quantity > 1 ? $"Press E to pick up {itemData.itemName} x{quantity}" : $"Press E to pick up {itemData.itemName}";
        }

        /// <summary>Returns whether this item has a valid definition and remaining quantity.</summary>
        public bool CanInteract(in InteractionContext context)
        {
            if (!canBePickedUp || itemData == null || quantity <= 0) return false;
            return context.Inventory.GetQuantity(itemData) < context.Inventory.GetMaximumQuantity(itemData);
        }

        private void OnValidate()
        {
            quantity = Mathf.Max(1, quantity);
        }
    }
}
