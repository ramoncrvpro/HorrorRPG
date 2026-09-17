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
        public GameContext(
            GameSession session,
            GameInputReader inputReader,
            InputContextService input,
            SceneFlowService sceneFlow,
            InventoryService inventory,
            DialogueService dialogue,
            BattleService battle,
            UINavigationService navigation)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            InputReader = inputReader != null ? inputReader : throw new ArgumentNullException(nameof(inputReader));
            Input = input ?? throw new ArgumentNullException(nameof(input));
            SceneFlow = sceneFlow ?? throw new ArgumentNullException(nameof(sceneFlow));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
            Battle = battle ?? throw new ArgumentNullException(nameof(battle));
            Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
        }

        public GameSession Session { get; }
        public GameInputReader InputReader { get; }
        public InputContextService Input { get; }
        public SceneFlowService SceneFlow { get; }
        public InventoryService Inventory { get; }
        public DialogueService Dialogue { get; }
        public BattleService Battle { get; }
        public UINavigationService Navigation { get; }
    }
}
