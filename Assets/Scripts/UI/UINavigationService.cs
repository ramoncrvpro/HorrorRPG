using System;
using System.Collections.Generic;

namespace HorrorRPG.Core
{
    public readonly struct UINavigationHandle { internal UINavigationHandle(Guid id) { Id = id; } internal Guid Id { get; } }
    public enum UIScreenId { Gameplay, Inventory, Dialogue, Battle, Confirmation }
    /// <summary>Scene-scoped navigation stack with disposable handles.</summary>
    public sealed class UINavigationService
    {
        private readonly List<(UINavigationHandle handle, UIScreenId screen, Action back)> entries = new List<(UINavigationHandle, UIScreenId, Action)>();
        public UINavigationHandle Push(UIScreenId screen, Action backAction) { var handle = new UINavigationHandle(Guid.NewGuid()); entries.Add((handle, screen, backAction)); return handle; }
        public void Pop(UINavigationHandle handle) { entries.RemoveAll(entry => entry.handle.Id == handle.Id); }
        public void NavigateBack() { if (entries.Count == 0) return; var entry = entries[entries.Count - 1]; entries.RemoveAt(entries.Count - 1); entry.back?.Invoke(); }
        public void ClearSceneEntries() => entries.Clear();
    }
}
