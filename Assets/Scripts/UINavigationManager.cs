using HorrorRPG.Core;
using HorrorRPG.Input;
using UnityEngine;

namespace HorrorRPG.Presentation
{
    /// <summary>Scene-local adapter that forwards cancel input to the navigation service.</summary>
    public sealed class UINavigationManager : MonoBehaviour, IGameContextReceiver
    {
        private GameInputReader inputReader;
        private UINavigationService navigationService;
        private bool subscribed;

        /// <summary>Injects navigation and input dependencies for the active scene.</summary>
        public void Initialize(GameContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));
            inputReader = context.InputReader;
            navigationService = context.Navigation;
            Subscribe();
        }

        /// <summary>Releases the cancel callback and scene navigation entries.</summary>
        public void Deinitialize()
        {
            Unsubscribe();
            navigationService?.ClearSceneEntries();
            inputReader = null;
            navigationService = null;
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Subscribe()
        {
            if (!isActiveAndEnabled || inputReader == null || subscribed) return;
            inputReader.CancelPerformed += HandleBackNavigation;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || inputReader == null) return;
            inputReader.CancelPerformed -= HandleBackNavigation;
            subscribed = false;
        }

        private void HandleBackNavigation() => navigationService?.NavigateBack();
        private void OnDestroy() => Deinitialize();
    }
}
