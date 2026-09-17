using System;
using HorrorRPG.Core;
using HorrorRPG.Inventory;

namespace HorrorRPG.Battle
{
    public enum BattlePhase { Idle, Entering, PlayerChoice, PlayerTiming, ResolvingPlayerAttack, EnemyAttack, Defense, Won, Lost, Exiting }
    public enum BattleEndReason { Won, Lost, Cancelled, SceneChanged }
    public enum BattleHealthTarget { Player, Enemy }

    public readonly struct BattleCommandResult
    {
        public BattleCommandResult(bool accepted, string reason)
        {
            Accepted = accepted;
            Reason = reason;
        }

        public bool Accepted { get; }
        public string Reason { get; }
    }

    public readonly struct ProjectileHitResult
    {
        public ProjectileHitResult(int damage, DefensePosition position)
        {
            Damage = Math.Max(0, damage);
            Position = position;
        }

        public int Damage { get; }
        public DefensePosition Position { get; }
    }

    public readonly struct BattleHealthChangedEvent
    {
        public BattleHealthChangedEvent(BattleHealthTarget target, int current, int maximum)
        {
            Target = target;
            Current = current;
            Maximum = maximum;
        }

        public BattleHealthTarget Target { get; }
        public int Current { get; }
        public int Maximum { get; }
    }

    public sealed class BattleState
    {
        public EnemyData Enemy { get; internal set; }
        public int CurrentEnemyHealth { get; internal set; }
        public BattlePhase Phase { get; internal set; } = BattlePhase.Idle;
        public int Turn { get; internal set; }
        public int DamageModifier { get; internal set; }
        public float TimingSpeedModifier { get; internal set; } = 1f;
        public WeaponData PendingWeapon { get; internal set; }
        public AttackResult? LastAttackResult { get; internal set; }
        public int LastProjectileDamage { get; internal set; }
    }

    /// <summary>Applies validated battle rules independently from arena presentation.</summary>
    public sealed class BattleService
    {
        private const float MinimumTimingSpeedModifier = 0.1f;
        private const float MaximumTimingSpeedModifier = 2f;
        private readonly GameSession session;
        private readonly InventoryService inventory;

        public BattleService(GameSession session, InventoryService inventory)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
        }

        public BattleState State { get; } = new BattleState();
        public event Action<BattlePhase> PhaseChanged;
        public event Action<BattleHealthChangedEvent> HealthChanged;
        public event Action<BattleCommandResult> CommandRejected;
        public event Action<BattleEndReason> Ended;

        /// <summary>Starts one battle after validating its enemy and current phase.</summary>
        public BattleCommandResult StartBattle(EnemyData enemy)
        {
            if (enemy == null) return Reject("Enemy is required.");
            if (State.Phase != BattlePhase.Idle) return Reject("Battle cannot start while another battle is active.");
            if (enemy.maxHealth <= 0) return Reject("Enemy health must be positive.");

            State.Enemy = enemy;
            State.CurrentEnemyHealth = enemy.maxHealth;
            State.Turn = 0;
            State.DamageModifier = 0;
            State.TimingSpeedModifier = 1f;
            State.PendingWeapon = null;
            State.LastAttackResult = null;
            State.LastProjectileDamage = 0;
            session.Player.SetBattleState(true);
            SetPhase(BattlePhase.Entering);
            HealthChanged?.Invoke(new BattleHealthChangedEvent(BattleHealthTarget.Enemy, State.CurrentEnemyHealth, enemy.maxHealth));
            HealthChanged?.Invoke(new BattleHealthChangedEvent(BattleHealthTarget.Player, session.Player.CurrentHealth, session.Player.MaxHealth));
            return Accept();
        }

        /// <summary>Completes entry presentation and opens player choice.</summary>
        public BattleCommandResult EnterPlayerChoice()
        {
            if (!RequirePhase(BattlePhase.Entering)) return Reject("Battle entry is not active.");
            SetPhase(BattlePhase.PlayerChoice);
            return Accept();
        }

        /// <summary>Validates weapon ownership and ammunition before timing starts.</summary>
        public BattleCommandResult SelectWeapon(WeaponData weapon)
        {
            if (!RequirePhase(BattlePhase.PlayerChoice)) return Reject("A weapon can only be selected during player choice.");
            if (weapon == null) return Reject("Weapon is required.");
            if (!inventory.HasItem(weapon, 1)) return Reject("Weapon is not available in the inventory.");
            if (weapon.requiresAmmo)
            {
                if (weapon.ammoType == null || !inventory.HasItem(weapon.ammoType, 1)) return Reject("Weapon has no ammunition.");
                if (!inventory.RemoveItem(weapon.ammoType, 1).IsComplete) return Reject("Weapon ammunition could not be consumed.");
            }

            session.RecentWeapons.Add(GetStableItemId(weapon));
            State.PendingWeapon = weapon;
            SetPhase(BattlePhase.PlayerTiming);
            return Accept();
        }

        /// <summary>Applies the result of the active weapon timing window.</summary>
        public BattleCommandResult ResolvePlayerAttack(AttackResult result)
        {
            if (!RequirePhase(BattlePhase.PlayerTiming)) return Reject("Player attack is not awaiting timing.");
            if (State.PendingWeapon == null) return Reject("No pending weapon is selected.");

            WeaponData weapon = State.PendingWeapon;
            State.LastAttackResult = result;
            SetPhase(BattlePhase.ResolvingPlayerAttack);
            int damage = Math.Max(0, weapon.GetDamageByResult(result, State.Enemy.category) + State.DamageModifier);
            State.DamageModifier = 0;
            State.TimingSpeedModifier = 1f;
            State.PendingWeapon = null;
            State.CurrentEnemyHealth = Math.Max(0, State.CurrentEnemyHealth - damage);
            HealthChanged?.Invoke(new BattleHealthChangedEvent(BattleHealthTarget.Enemy, State.CurrentEnemyHealth, State.Enemy.maxHealth));

            if (State.CurrentEnemyHealth == 0) Finish(BattleEndReason.Won, BattlePhase.Won);
            else SetPhase(BattlePhase.EnemyAttack);
            return Accept();
        }

        /// <summary>Consumes and applies one available consumable.</summary>
        public BattleCommandResult UseConsumable(ConsumableData item)
        {
            if (!RequirePhase(BattlePhase.PlayerChoice)) return Reject("Consumables can only be used during player choice.");
            if (item == null || !inventory.HasItem(item, 1)) return Reject("Consumable is not available.");
            if (!inventory.RemoveItem(item, 1).IsComplete) return Reject("Consumable could not be removed from the inventory.");

            switch (item.effectType)
            {
                case ConsumableEffectType.HealHealth:
                    session.Player.Heal(item.effectValue);
                    HealthChanged?.Invoke(new BattleHealthChangedEvent(BattleHealthTarget.Player, session.Player.CurrentHealth, session.Player.MaxHealth));
                    break;
                case ConsumableEffectType.IncreaseDamage:
                    State.DamageModifier = Math.Max(0, item.effectValue);
                    break;
                case ConsumableEffectType.DecreaseMarkerSpeed:
                    double modifier = 1d - item.effectValue / 100d;
                    State.TimingSpeedModifier = (float)Math.Clamp(modifier, MinimumTimingSpeedModifier, MaximumTimingSpeedModifier);
                    break;
            }
            SetPhase(BattlePhase.EnemyAttack);
            return Accept();
        }

        /// <summary>Begins defense for the current enemy attack.</summary>
        public BattleCommandResult BeginEnemyAttack()
        {
            if (!RequirePhase(BattlePhase.EnemyAttack)) return Reject("Enemy attack cannot begin in the current phase.");
            SetPhase(BattlePhase.Defense);
            return Accept();
        }

        /// <summary>Applies one projectile resolution during defense.</summary>
        public BattleCommandResult ResolveProjectile(ProjectileHitResult result)
        {
            if (!RequirePhase(BattlePhase.Defense)) return Reject("Projectile resolution requires the defense phase.");
            State.LastProjectileDamage = result.Damage;
            if (result.Damage > 0) session.Player.ApplyDamage(result.Damage);
            HealthChanged?.Invoke(new BattleHealthChangedEvent(BattleHealthTarget.Player, session.Player.CurrentHealth, session.Player.MaxHealth));
            if (session.Player.CurrentHealth == 0) Finish(BattleEndReason.Lost, BattlePhase.Lost);
            return Accept();
        }

        /// <summary>Completes a surviving defense phase and starts the next turn.</summary>
        public BattleCommandResult CompleteEnemyAttack()
        {
            if (!RequirePhase(BattlePhase.Defense)) return Reject("Enemy attack completion requires the defense phase.");
            State.Turn++;
            SetPhase(BattlePhase.PlayerChoice);
            return Accept();
        }

        /// <summary>Runs from battle during player choice.</summary>
        public BattleCommandResult Run()
        {
            if (!RequirePhase(BattlePhase.PlayerChoice)) return Reject("Run is only available during player choice.");
            Finish(BattleEndReason.Cancelled, BattlePhase.Exiting);
            return Accept();
        }

        /// <summary>Aborts the active battle for an external lifecycle reason.</summary>
        public void Abort(BattleEndReason reason)
        {
            if (State.Phase == BattlePhase.Idle) return;
            session.Player.SetBattleState(false);
            SetPhase(BattlePhase.Idle);
            Ended?.Invoke(reason);
            ResetState();
        }

        /// <summary>Resets completed visual exit state to idle.</summary>
        public void CompleteExit()
        {
            if (State.Phase != BattlePhase.Won && State.Phase != BattlePhase.Lost && State.Phase != BattlePhase.Exiting) return;
            SetPhase(BattlePhase.Idle);
            ResetState();
        }

        private void Finish(BattleEndReason reason, BattlePhase phase)
        {
            session.Player.SetBattleState(false);
            SetPhase(phase);
            Ended?.Invoke(reason);
        }

        private void ResetState()
        {
            State.Enemy = null;
            State.CurrentEnemyHealth = 0;
            State.PendingWeapon = null;
            State.DamageModifier = 0;
            State.TimingSpeedModifier = 1f;
            State.LastAttackResult = null;
            State.LastProjectileDamage = 0;
        }

        private bool RequirePhase(BattlePhase expected) => State.Phase == expected;
        private static BattleCommandResult Accept() => new BattleCommandResult(true, string.Empty);

        private BattleCommandResult Reject(string reason)
        {
            var result = new BattleCommandResult(false, reason);
            CommandRejected?.Invoke(result);
            return result;
        }

        private void SetPhase(BattlePhase phase)
        {
            State.Phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        private static string GetStableItemId(ItemData item) => item.Id;
    }
}
