using System;
using System.Collections.Generic;

namespace HorrorRPG.Input
{
    public enum InputContext
    {
        Gameplay,
        UI,
        Battle,
        Dialogue
    }

    public enum InputBlockReason
    {
        Menu,
        Dialogue,
        Battle,
        Transition,
        Cutscene
    }

    public sealed class InputContextLease : IDisposable
    {
        internal InputContextLease(Guid token, InputContext context) { Token = token; Context = context; }
        internal Guid Token { get; }
        public InputContext Context { get; }
        public bool IsReleased { get; internal set; }
        public void Dispose() { IsReleased = true; }
    }

    /// <summary>Owns the typed stack of active input contexts.</summary>
    public sealed class InputContextService
    {
        private readonly List<InputContextLease> leases = new List<InputContextLease>();
        public InputContext CurrentContext { get; private set; } = InputContext.Gameplay;

        public InputContextLease Acquire(InputContext context, InputBlockReason reason)
        {
            var lease = new InputContextLease(Guid.NewGuid(), context);
            leases.Add(lease);
            CurrentContext = context;
            return lease;
        }

        public void Release(InputContextLease lease)
        {
            if (lease == null || lease.IsReleased || !leases.Remove(lease)) return;
            lease.IsReleased = true;
            CurrentContext = leases.Count == 0 ? InputContext.Gameplay : leases[leases.Count - 1].Context;
        }

        public void ResetToGameplay()
        {
            ClearSceneLeases();
            CurrentContext = InputContext.Gameplay;
        }

        public void ClearSceneLeases()
        {
            foreach (InputContextLease lease in leases) lease.IsReleased = true;
            leases.Clear();
            CurrentContext = InputContext.Gameplay;
        }
    }
}
