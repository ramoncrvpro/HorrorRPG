using System;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorRPG.Core
{
    [Serializable]
    public struct SceneCatalogEntry
    {
        [SerializeField] private string id;
        [SerializeField] private string sceneName;
        public string Id => id;
        public string SceneName => sceneName;
    }

    [CreateAssetMenu(fileName = "SceneCatalog", menuName = "Horror RPG/Scene Catalog")]
    public sealed class SceneCatalog : ScriptableObject
    {
        [SerializeField] private List<SceneCatalogEntry> entries = new List<SceneCatalogEntry>();
        public IReadOnlyList<SceneCatalogEntry> Entries => entries;

        public bool TryGetScene(string id, out SceneCatalogEntry entry)
        {
            for (int index = 0; index < entries.Count; index++)
            {
                if (string.Equals(entries[index].Id, id, StringComparison.Ordinal)) { entry = entries[index]; return true; }
            }
            entry = default;
            return false;
        }

        private void OnValidate()
        {
            if (entries == null) entries = new List<SceneCatalogEntry>();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (SceneCatalogEntry entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Id) || string.IsNullOrWhiteSpace(entry.SceneName) || !ids.Add(entry.Id)) Debug.LogError($"Invalid or duplicate scene catalog entry: '{entry.Id}'.", this);
            }
        }
    }
}
