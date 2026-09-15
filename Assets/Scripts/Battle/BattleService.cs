using System;

namespace HorrorRPG.Battle
{
    public enum BattlePhase { Idle, Entering, PlayerChoice, PlayerTiming, ResolvingPlayerAttack, EnemyAttack, Defense, Won, Lost, Exiting }
    public readonly struct BattleCommandResult { public bool Accepted { get; } public string Reason { get; } public BattleCommandResult(bool accepted, string reason) { Accepted = accepted; Reason = reason; } }
    public sealed class BattleState { public EnemyData Enemy { get; internal set; } public int CurrentEnemyHealth { get; internal set; } public BattlePhase Phase { get; internal set; } = BattlePhase.Idle; }
    /// <summary>Applies battle lifecycle rules independently from arena presentation.</summary>
    public sealed class BattleService
    {
        private readonly Core.GameSession session;
        private readonly Inventory.InventoryService inventory;
        public BattleState State { get; } = new BattleState();
        public event Action<BattlePhase> PhaseChanged;
        public BattleService(Core.GameSession session, Inventory.InventoryService inventory) { this.session = session; this.inventory = inventory; }
        public BattleCommandResult StartBattle(EnemyData enemy) { if (enemy == null || State.Phase != BattlePhase.Idle) return Reject("Battle cannot start."); State.Enemy = enemy; State.Phase = BattlePhase.Entering; session.Player.SetBattleState(true); PhaseChanged?.Invoke(State.Phase); return Accept(); }
        public void Abort(string reason) { State.Enemy = null; State.Phase = BattlePhase.Idle; session.Player.SetBattleState(false); PhaseChanged?.Invoke(State.Phase); }
        private static BattleCommandResult Accept() => new BattleCommandResult(true, string.Empty);
        private static BattleCommandResult Reject(string reason) => new BattleCommandResult(false, reason);
    }
}
