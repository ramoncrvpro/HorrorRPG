using HorrorRPG.Core;
using UnityEngine;

namespace HorrorRPG.Player
{
    /// <summary>Scene adapter exposing session-owned player health to local presentation.</summary>
    public class PlayerStats : MonoBehaviour, IGameContextReceiver
    {
        [SerializeField] private int maxHealth = 100;

        private PlayerRuntimeState runtimeState;

        public int MaxHealth => runtimeState?.MaxHealth ?? maxHealth;
        public int CurrentHealth => runtimeState?.CurrentHealth ?? maxHealth;
        public event System.Action<int, int> HealthChanged;

        /// <summary>Connects this adapter to session-owned player health.</summary>
        public void Initialize(GameContext context)
        {
            runtimeState = context?.Session.Player ?? throw new System.ArgumentNullException(nameof(context));
            runtimeState.ConfigureHealth(maxHealth);
            runtimeState.HealthChanged += HandleHealthChanged;
            HealthChanged?.Invoke(runtimeState.CurrentHealth, runtimeState.MaxHealth);
        }

        /// <summary>Releases the session health callback.</summary>
        public void Deinitialize()
        {
            if (runtimeState != null) runtimeState.HealthChanged -= HandleHealthChanged;
            runtimeState = null;
        }

        /// <summary>Applies non-negative damage to session health.</summary>
        public void TakeDamage(int damage) => runtimeState?.ApplyDamage(Mathf.Max(0, damage));

        /// <summary>Heals session health by a non-negative amount.</summary>
        public void Heal(int amount) => runtimeState?.Heal(Mathf.Max(0, amount));

        /// <summary>Returns whether the session player is alive.</summary>
        public bool IsAlive() => CurrentHealth > 0;

        /// <summary>Restores session health to its maximum.</summary>
        public void ResetHealth() => runtimeState?.ResetHealth();

        private void HandleHealthChanged(int current, int maximum) => HealthChanged?.Invoke(current, maximum);

        private void OnValidate() => maxHealth = Mathf.Max(1, maxHealth);
        private void OnDestroy() => Deinitialize();
    }
}
