using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HorrorRPG.Core
{
    public enum SceneReloadReason { Manual, PlayerDeath, Transition }

    /// <summary>Validates and performs all scene transitions.</summary>
    public sealed class SceneFlowService
    {
        private readonly SceneCatalog catalog;
        private readonly GameSession session;

        public event Action TransitionStarted;

        public SceneFlowService(SceneCatalog catalog, GameSession session)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public bool IsAvailableInBuild(string destination)
        {
            return catalog.TryGetScene(destination, out SceneCatalogEntry entry) && Application.CanStreamedLevelBeLoaded(entry.SceneName);
        }

        public void Load(string destination)
        {
            if (!catalog.TryGetScene(destination, out SceneCatalogEntry entry)) throw new InvalidOperationException($"Scene ID '{destination}' is not present in the catalog.");
            if (!Application.CanStreamedLevelBeLoaded(entry.SceneName)) throw new InvalidOperationException($"Scene '{entry.SceneName}' for ID '{destination}' is not enabled in Build Settings.");
            BeginTransition();
            SceneManager.LoadScene(entry.SceneName);
        }

        public void ReloadCurrentScene(SceneReloadReason reason)
        {
            if (reason == SceneReloadReason.PlayerDeath) session.HandlePlayerDeath();
            else session.ResetTransientState();
            BeginTransition();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReloadCurrentScene(string reason)
        {
            ReloadCurrentScene(SceneReloadReason.Manual);
        }

        private void BeginTransition()
        {
            TransitionStarted?.Invoke();
            session.ResetTransientState();
        }
    }
}
