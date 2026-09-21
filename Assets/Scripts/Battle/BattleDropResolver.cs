using System;
using System.Collections.Generic;
using HorrorRPG.Inventory;
using UnityEngine;

namespace HorrorRPG.Battle
{
    /// <summary>Resolves independent enemy drops and grants the effective quantities to inventory.</summary>
    public sealed class BattleDropResolver
    {
        /// <summary>Rolls valid drops, aggregates duplicate item IDs and grants them to inventory.</summary>
        public IReadOnlyList<InventoryEntry> ResolveAndGrant(IReadOnlyList<EnemyDropEntry> entries, InventoryService inventory)
        {
            if (inventory == null) throw new ArgumentNullException(nameof(inventory));
            if (entries == null || entries.Count == 0) return Array.Empty<InventoryEntry>();

            var aggregated = new Dictionary<string, InventoryEntry>(StringComparer.Ordinal);
            for (int index = 0; index < entries.Count; index++)
            {
                EnemyDropEntry entry = entries[index];
                if (entry == null || !entry.IsValid() || string.IsNullOrWhiteSpace(entry.Item.Id)) continue;
                if (entry.DropChance <= 0f || (entry.DropChance < 1f && UnityEngine.Random.value >= entry.DropChance)) continue;

                int quantity = UnityEngine.Random.Range(entry.MinimumQuantity, entry.MaximumQuantity + 1);
                if (aggregated.TryGetValue(entry.Item.Id, out InventoryEntry existing))
                    aggregated[entry.Item.Id] = new InventoryEntry(existing.Item, existing.Quantity + quantity);
                else
                    aggregated.Add(entry.Item.Id, new InventoryEntry(entry.Item, quantity));
            }

            var result = new List<InventoryEntry>(aggregated.Count);
            foreach (InventoryEntry requested in aggregated.Values)
            {
                InventoryOperationResult operation = inventory.AddItem(requested.Item, requested.Quantity);
                if (operation.ProcessedQuantity > 0)
                    result.Add(new InventoryEntry(requested.Item, operation.ProcessedQuantity));
            }
            return result;
        }
    }
}
