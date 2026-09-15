using System;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Input;
using HorrorRPG.Inventory;

namespace HorrorRPG.Core
{
    /// <summary>Read-only service graph exposed to scene-local components.</summary>
    public sealed class GameContext
    {
        public GameSession Session { get; }
        public InputContextService Input { get; }
        public SceneFlowService SceneFlow { get; }
        public InventoryService Inventory { get; }
        public DialogueService Dialogue { get; }
        public BattleService Battle { get; }
        public UINavigationService Navigation { get; }
        public GameSession GameSession => Session;
        public InputContextService InputContextService => Input;
        public SceneFlowService SceneFlowService => SceneFlow;
        public InventoryService InventoryService => Inventory;
        public DialogueService DialogueService => Dialogue;
        public BattleService BattleService => Battle;
        public UINavigationService UINavigationService => Navigation;


        public GameContext(GameSession session, InputContextService input, SceneFlowService sceneFlow, InventoryService inventory, DialogueService dialogue, BattleService battle, UINavigationService navigation)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            SceneFlow = sceneFlow ?? throw new ArgumentNullException(nameof(sceneFlow));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
            Battle = battle ?? throw new ArgumentNullException(nameof(battle));
            Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }
    }

    public interface IGameContextReceiver
    {
        void Initialize(GameContext context);
        void Deinitialize();
    }
}
