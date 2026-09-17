namespace HorrorRPG.Interaction
{
    /// <summary>Contract implemented by player-facing world interactions.</summary>
    public interface IInteractable
    {
        /// <summary>Returns whether the target can currently be used.</summary>
        bool CanInteract(in InteractionContext context);

        /// <summary>Returns the prompt shown for the current interaction.</summary>
        string GetInteractionPrompt(in InteractionContext context);

        /// <summary>Executes the interaction using scene-injected dependencies.</summary>
        void Interact(in InteractionContext context);
    }
}
