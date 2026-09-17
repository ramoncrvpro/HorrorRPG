using System;
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

        /// <summary>Initializes every scene-local receiver exactly once.</summary>
        public void Compose(GameContext context)
        {
            if (composed) return;
            if (context == null) throw new ArgumentNullException(nameof(context));

            MonoBehaviour[] components = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            receivers.Clear();
            receivers.AddRange(components
                .Where(component => component != null && component.gameObject.scene == gameObject.scene)
                .OfType<IGameContextReceiver>()
                .OrderBy(receiver => receiver.GetType().FullName, StringComparer.Ordinal)
                .ThenBy(receiver => ((MonoBehaviour)receiver).GetInstanceID()));

            int initializedCount = 0;
            try
            {
                foreach (IGameContextReceiver receiver in receivers)
                {
                    receiver.Initialize(context);
                    initializedCount++;
                }
                composed = true;
            }
            catch
            {
                for (int index = initializedCount - 1; index >= 0; index--) receivers[index].Deinitialize();
                receivers.Clear();
                throw;
            }
        }

        /// <summary>Deinitializes receivers in reverse composition order.</summary>
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
