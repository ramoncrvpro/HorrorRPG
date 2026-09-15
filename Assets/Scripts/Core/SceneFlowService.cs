using UnityEngine;

using UnityEngine.SceneManagement;

namespace HorrorRPG.Core
{
    /// <summary>Validates and performs all scene transitions.</summary>
    public sealed class SceneFlowService
    {
        private readonly SceneCatalog catalog;
        private readonly GameSession session;
        public SceneFlowService(SceneCatalog catalog, GameSession session) { this.catalog = catalog; this.session = session; }

        public bool IsAvailableInBuild(string destination)
        {
            return catalog != null && catalog.TryGetScene(destination, out SceneCatalogEntry entry) && Application.CanStreamedLevelBeLoaded(entry.SceneName);
        }

        public void Load(string destination)
        {
            if (catalog == null || !catalog.TryGetScene(destination, out SceneCatalogEntry entry)) throw new System.InvalidOperationException($"Scene ID '{destination}' is not present in the catalog.");
            if (!Application.CanStreamedLevelBeLoaded(entry.SceneName)) throw new System.InvalidOperationException($"Scene '{entry.SceneName}' for ID '{destination}' is not enabled in Build Settings.");
            session.ResetTransientState();
            SceneManager.LoadScene(entry.SceneName);
        }

        public void ReloadCurrentScene(string reason)
        {
            session.ResetTransientState();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
