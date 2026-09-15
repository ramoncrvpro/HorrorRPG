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

        public void ConfigureHealth(int maxHealth)
        {
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            MaxHealth = maxHealth;
            CurrentHealth = Math.Min(CurrentHealth, MaxHealth);
        }

        public void ApplyDamage(int damage)
        {
            if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
            CurrentHealth = Math.Max(0, CurrentHealth - damage);
        }

        public void Heal(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        }

        public void ResetHealth() => CurrentHealth = MaxHealth;
        public void SetBattleState(bool active) => IsInBattle = active;
        public void Reset() { ResetHealth(); ResetTransientState(); }
        public void ResetTransientState() => IsInBattle = false;
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
        private readonly HashSet<string> defeatedEnemies = new HashSet<string>();
        public IReadOnlyCollection<string> DefeatedEnemies => defeatedEnemies;
        public void MarkEnemyDefeated(string worldId)
        {
            if (string.IsNullOrWhiteSpace(worldId)) throw new ArgumentException("World ID cannot be empty.", nameof(worldId));
            defeatedEnemies.Add(worldId);
        }
        public void Clear() => defeatedEnemies.Clear();
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
