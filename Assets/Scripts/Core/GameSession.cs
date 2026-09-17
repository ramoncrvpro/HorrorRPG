using System;
using System.Collections.Generic;

namespace HorrorRPG.Core
{
    /// <summary>Authoritative runtime state for the current application session.</summary>
    public sealed class GameSession
    {
        public PlayerRuntimeState Player { get; } = new PlayerRuntimeState();
        public InventoryRuntimeState Inventory { get; } = new InventoryRuntimeState();
        public WorldRuntimeState World { get; } = new WorldRuntimeState();
        public RecentWeaponsRuntimeState RecentWeapons { get; } = new RecentWeaponsRuntimeState();

        /// <summary>Starts a new in-memory session and clears transient state.</summary>
        public void StartNewSession()
        {
            Player.Reset();
            Inventory.Clear();
            World.Clear();
            RecentWeapons.Clear();
        }

        /// <summary>Clears state that cannot survive a scene transition.</summary>
        public void ResetTransientState()
        {
            Player.ResetTransientState();
        }

        /// <summary>Restores player health and transient state after death.</summary>
        public void HandlePlayerDeath()
        {
            Player.ResetHealth();
            ResetTransientState();
        }
    }

    public sealed class PlayerRuntimeState
    {
        public int MaxHealth { get; private set; } = 100;
        public int CurrentHealth { get; private set; } = 100;
        public bool IsInBattle { get; private set; }
        public event Action<int, int> HealthChanged;

        /// <summary>Configures maximum health while clamping current health.</summary>
        public void ConfigureHealth(int maxHealth)
        {
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            MaxHealth = maxHealth;
            CurrentHealth = Math.Min(CurrentHealth, MaxHealth);
            NotifyHealthChanged();
        }

        /// <summary>Applies validated damage and emits the resulting health.</summary>
        public void ApplyDamage(int damage)
        {
            if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            CurrentHealth = Math.Max(0, CurrentHealth - damage);
            NotifyHealthChanged();
        }

        /// <summary>Applies validated healing and emits the resulting health.</summary>
        public void Heal(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
            NotifyHealthChanged();
        }

        /// <summary>Restores health to its configured maximum.</summary>
        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            NotifyHealthChanged();
        }

        public void SetBattleState(bool active) => IsInBattle = active;
        public void Reset() { ResetHealth(); ResetTransientState(); }
        public void ResetTransientState() => IsInBattle = false;
        private void NotifyHealthChanged() => HealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public sealed class InventoryRuntimeState
    {
        private readonly Dictionary<string, int> quantities = new Dictionary<string, int>();
        public IReadOnlyDictionary<string, int> Quantities => quantities;

        public int GetQuantity(string itemId) => string.IsNullOrWhiteSpace(itemId) ? 0 : quantities.TryGetValue(itemId, out int value) ? value : 0;
        public void SetQuantity(string itemId, int quantity)
        {
            if (string.IsNullOrWhiteSpace(itemId)) throw new ArgumentException("Item ID cannot be empty.", nameof(itemId));
            if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity));
            if (quantity == 0) quantities.Remove(itemId); else quantities[itemId] = quantity;
        }
        public void Clear() => quantities.Clear();
    }

    public sealed class WorldRuntimeState
    {
        private readonly HashSet<string> defeatedEnemies = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> completedObjects = new HashSet<string>(StringComparer.Ordinal);
        public IReadOnlyCollection<string> DefeatedEnemies => defeatedEnemies;
        public IReadOnlyCollection<string> CompletedObjects => completedObjects;

        /// <summary>Marks an enemy as defeated for the current runtime session.</summary>
        public void MarkEnemyDefeated(string worldId)
        {
            ValidateWorldId(worldId);
            defeatedEnemies.Add(worldId);
            completedObjects.Add(worldId);
        }

        /// <summary>Marks a stateful world object as completed.</summary>
        public void MarkCompleted(string worldId)
        {
            ValidateWorldId(worldId);
            completedObjects.Add(worldId);
        }

        /// <summary>Returns whether a world object completed earlier in this session.</summary>
        public bool IsCompleted(string worldId) => !string.IsNullOrWhiteSpace(worldId) && completedObjects.Contains(worldId);

        public void Clear()
        {
            defeatedEnemies.Clear();
            completedObjects.Clear();
        }

        private static void ValidateWorldId(string worldId)
        {
            if (string.IsNullOrWhiteSpace(worldId)) throw new ArgumentException("World ID cannot be empty.", nameof(worldId));
        }
    }

    public sealed class RecentWeaponsRuntimeState
    {
        private const int MaxEntries = 9;
        private readonly List<string> weaponIds = new List<string>(MaxEntries);
        public IReadOnlyList<string> WeaponIds => weaponIds;
        public void Add(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId)) throw new ArgumentException("Weapon ID cannot be empty.", nameof(weaponId));
            weaponIds.Remove(weaponId);
            weaponIds.Insert(0, weaponId);
            if (weaponIds.Count > MaxEntries) weaponIds.RemoveAt(weaponIds.Count - 1);
        }
        public void Clear() => weaponIds.Clear();
    }
}
