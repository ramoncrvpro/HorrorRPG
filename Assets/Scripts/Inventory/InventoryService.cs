using System;
using System.Collections.Generic;
using HorrorRPG.Core;

namespace HorrorRPG.Inventory
{
    public enum InventoryOperationStatus { Completed, Partial, Rejected }
    public enum InventoryOperationReason { None, InvalidItem, InvalidQuantity, StackCapacity, ConsumableSlotLimit, InsufficientQuantity }

    public readonly struct InventoryOperationResult
    {
        public InventoryOperationResult(int requested, int processed, InventoryOperationStatus status, InventoryOperationReason reason)
        {
            RequestedQuantity = Math.Max(0, requested);
            ProcessedQuantity = Math.Clamp(processed, 0, RequestedQuantity);
            Status = status;
            Reason = reason;
        }

        public int RequestedQuantity { get; }
        public int ProcessedQuantity { get; }
        public int RemainingQuantity => RequestedQuantity - ProcessedQuantity;
        public InventoryOperationStatus Status { get; }
        public InventoryOperationReason Reason { get; }
        public bool IsComplete => RemainingQuantity == 0 && Status == InventoryOperationStatus.Completed;
    }

    public readonly struct InventoryEntry
    {
        public InventoryEntry(ItemData item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public ItemData Item { get; }
        public int Quantity { get; }
    }

    public readonly struct InventoryChangedEvent
    {
        public InventoryChangedEvent(ItemData item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        public ItemData Item { get; }
        public int Quantity { get; }
    }

    /// <summary>Authoritative synchronous inventory rules backed by the current session.</summary>
    public sealed class InventoryService
    {
        private const int MaximumConsumableSlots = 9;
        private readonly GameSession session;
        private readonly Dictionary<string, ItemData> itemDefinitions = new Dictionary<string, ItemData>(StringComparer.Ordinal);

        public InventoryService(GameSession session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public event Action<InventoryChangedEvent> Changed;

        /// <summary>Adds a validated quantity while respecting stack and consumable slot capacity.</summary>
        public InventoryOperationResult AddItem(ItemData item, int quantity)
        {
            if (item == null) return Rejected(quantity, InventoryOperationReason.InvalidItem);
            if (quantity <= 0) return Rejected(quantity, InventoryOperationReason.InvalidQuantity);

            string itemId = Register(item);
            int current = session.Inventory.GetQuantity(itemId);
            if (current == 0 && item.category == ItemCategory.Consumable && GetUsedConsumableSlots() >= MaximumConsumableSlots)
                return Rejected(quantity, InventoryOperationReason.ConsumableSlotLimit);

            int capacity = Math.Max(0, item.maxStackSize - current);
            int processed = Math.Min(quantity, capacity);
            if (processed > 0)
            {
                session.Inventory.SetQuantity(itemId, current + processed);
                Changed?.Invoke(new InventoryChangedEvent(item, current + processed));
            }
            return CreateResult(quantity, processed, InventoryOperationReason.StackCapacity);
        }

        /// <summary>Removes at most the available validated quantity.</summary>
        public InventoryOperationResult RemoveItem(ItemData item, int quantity)
        {
            if (item == null) return Rejected(quantity, InventoryOperationReason.InvalidItem);
            if (quantity <= 0) return Rejected(quantity, InventoryOperationReason.InvalidQuantity);

            string itemId = Register(item);
            int current = session.Inventory.GetQuantity(itemId);
            int processed = Math.Min(quantity, current);
            if (processed > 0)
            {
                session.Inventory.SetQuantity(itemId, current - processed);
                Changed?.Invoke(new InventoryChangedEvent(item, current - processed));
            }
            return CreateResult(quantity, processed, InventoryOperationReason.InsufficientQuantity);
        }

        /// <summary>Returns whether a positive item quantity is available.</summary>
        public bool HasItem(ItemData item, int quantity) => item != null && quantity > 0 && GetQuantity(item) >= quantity;

        /// <summary>Returns the current quantity for an item.</summary>
        public int GetQuantity(ItemData item)
        {
            if (item == null) return 0;
            return session.Inventory.GetQuantity(Register(item));
        }

        /// <summary>Returns immutable entries, optionally filtered by category.</summary>
        public IReadOnlyList<InventoryEntry> GetItems(ItemCategory? category = null)
        {
            var result = new List<InventoryEntry>();
            foreach (KeyValuePair<string, int> quantity in session.Inventory.Quantities)
            {
                if (quantity.Value <= 0 || !itemDefinitions.TryGetValue(quantity.Key, out ItemData item)) continue;
                if (category.HasValue && item.category != category.Value) continue;
                result.Add(new InventoryEntry(item, quantity.Value));
            }
            result.Sort((left, right) => string.Compare(left.Item.itemName, right.Item.itemName, StringComparison.Ordinal));
            return result;
        }

        private string Register(ItemData item)
        {
            if (string.IsNullOrWhiteSpace(item.Id)) throw new ArgumentException($"Item '{item.name}' requires a stable ID.", nameof(item));
            itemDefinitions[item.Id] = item;
            return item.Id;
        }

        private int GetUsedConsumableSlots()
        {
            int count = 0;
            foreach (KeyValuePair<string, int> quantity in session.Inventory.Quantities)
            {
                if (quantity.Value > 0 && itemDefinitions.TryGetValue(quantity.Key, out ItemData item) && item.category == ItemCategory.Consumable) count++;
            }
            return count;
        }

        private static InventoryOperationResult Rejected(int requested, InventoryOperationReason reason)
        {
            return new InventoryOperationResult(requested, 0, InventoryOperationStatus.Rejected, reason);
        }

        private static InventoryOperationResult CreateResult(int requested, int processed, InventoryOperationReason partialReason)
        {
            InventoryOperationStatus status = processed == requested ? InventoryOperationStatus.Completed : InventoryOperationStatus.Partial;
            InventoryOperationReason reason = status == InventoryOperationStatus.Completed ? InventoryOperationReason.None : partialReason;
            return new InventoryOperationResult(requested, processed, status, reason);
        }
    }
}
