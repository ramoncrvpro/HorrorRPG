using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HorrorRPG.Core
{
    /// <summary>Composes scene-local receivers in a deterministic order.</summary>
    public sealed class SceneCompositionRoot : MonoBehaviour
    {
        private readonly List<IGameContextReceiver> receivers = new List<IGameContextReceiver>();
        private bool composed;

        public void Compose(GameContext context)
        {
            if (composed) return;
            if (context == null) throw new System.ArgumentNullException(nameof(context));
            MonoBehaviour[] components = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            receivers.Clear();
            receivers.AddRange(components
                .Where(component => component != null && component.gameObject.scene == gameObject.scene)
                .Select(component => component as IGameContextReceiver)
                .Where(receiver => receiver != null)
                .OrderBy(receiver => receiver.GetType().FullName));
            foreach (IGameContextReceiver receiver in receivers) receiver.Initialize(context);
            composed = true;
        }

        public void Release()
        {
            if (!composed) return;
            for (int index = receivers.Count - 1; index >= 0; index--) receivers[index].Deinitialize();
            receivers.Clear();
            composed = false;
        }

        private void OnDestroy() => Release();
    }
}
