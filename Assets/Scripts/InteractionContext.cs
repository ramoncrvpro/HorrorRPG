using System;
using HorrorRPG.Core;
using HorrorRPG.Dialogue;
using HorrorRPG.Input;
using HorrorRPG.Inventory;
using UnityEngine;

namespace HorrorRPG.Interaction
{
    /// <summary>Read-only dependencies available during one player interaction.</summary>
    public readonly struct InteractionContext
    {
        public InteractionContext(GameContext game, GameObject player)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));
            Player = player != null ? player : throw new ArgumentNullException(nameof(player));
        }

        public GameContext Game { get; }
        public GameObject Player { get; }
        public InventoryService Inventory => Game.Inventory;
        public DialogueService Dialogue => Game.Dialogue;
        public SceneFlowService SceneFlow => Game.SceneFlow;
        public InputContextService Input => Game.Input;
    }
}
