namespace HorrorRPG.Core
{
    /// <summary>Receives and releases the scene's runtime service graph.</summary>
    public interface IGameContextReceiver
    {
        /// <summary>Initializes the component with the active game context.</summary>
        void Initialize(GameContext context);

        /// <summary>Releases subscriptions and scene-scoped state.</summary>
        void Deinitialize();
    }
}
