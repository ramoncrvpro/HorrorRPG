using System;
using System.Collections.Generic;
using System.Linq;

namespace HorrorRPG.Inventory
{
    public enum InventoryOperationStatus { Completed, Partial, Rejected }
    public readonly struct InventoryOperationResult
    {
        public int RequestedQuantity { get; }
        public int ProcessedQuantity { get; }
        public int RemainingQuantity => RequestedQuantity - ProcessedQuantity;
        public InventoryOperationStatus Status { get; }
        public bool IsComplete => RemainingQuantity == 0 && Status == InventoryOperationStatus.Completed;
        public InventoryOperationResult(int requested, int processed, InventoryOperationStatus status) { RequestedQuantity = requested; ProcessedQuantity = processed; Status = status; }
    }
    public readonly struct InventoryEntry
    {
        public ItemData Item { get; }
        public int Quantity { get; }
        public InventoryEntry(ItemData item, int quantity) { Item = item; Quantity = quantity; }
    }
    public readonly struct InventoryChangedEvent
    {
        public ItemData Item { get; }
        public int Quantity { get; }
        public InventoryChangedEvent(ItemData item, int quantity) { Item = item; Quantity = quantity; }
    }

    /// <summary>Authoritative synchronous inventory rules.</summary>
    public sealed class InventoryService
    {
        private readonly Core.GameSession session;
        public event Action<InventoryChangedEvent> Changed;
        public InventoryService(Core.GameSession session) { this.session = session ?? throw new ArgumentNullException(nameof(session)); }
        private string Id(ItemData item) => item.itemName + "|" + item.GetInstanceID();
        public InventoryOperationResult AddItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return new InventoryOperationResult(quantity, 0, InventoryOperationStatus.Rejected);
            string id = Id(item); int current = session.Inventory.GetQuantity(id); int processed = Math.Min(quantity, Math.Max(0, item.maxStackSize - current));
            session.Inventory.SetQuantity(id, current + processed); Changed?.Invoke(new InventoryChangedEvent(item, current + processed));
            return new InventoryOperationResult(quantity, processed, processed == quantity ? InventoryOperationStatus.Completed : InventoryOperationStatus.Partial);
        }
        public InventoryOperationResult RemoveItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0) return new InventoryOperationResult(quantity, 0, InventoryOperationStatus.Rejected);
            string id = Id(item); int current = session.Inventory.GetQuantity(id); int processed = Math.Min(quantity, current);
            session.Inventory.SetQuantity(id, current - processed); Changed?.Invoke(new InventoryChangedEvent(item, current - processed));
            return new InventoryOperationResult(quantity, processed, processed == quantity ? InventoryOperationStatus.Completed : InventoryOperationStatus.Partial);
        }
        public bool HasItem(ItemData item, int quantity) => item != null && quantity > 0 && GetQuantity(item) >= quantity;
        public int GetQuantity(ItemData item) => item == null ? 0 : session.Inventory.GetQuantity(Id(item));
        public IReadOnlyList<InventoryEntry> GetItems(ItemCategory? category = null) => new List<InventoryEntry>();
    }
}
