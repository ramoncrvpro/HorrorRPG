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
        private readonly Action<InputContextLease> release;

        internal InputContextLease(Guid token, InputContext context, InputBlockReason reason, Action<InputContextLease> releaseAction)
        {
            Token = token;
            Context = context;
            Reason = reason;
            release = releaseAction ?? throw new ArgumentNullException(nameof(releaseAction));
        }

        internal Guid Token { get; }
        public InputContext Context { get; }
        public InputBlockReason Reason { get; }
        public bool IsReleased { get; internal set; }

        /// <summary>Releases this input context lease once.</summary>
        public void Dispose()
        {
            if (!IsReleased) release(this);
        }
    }

    /// <summary>Owns the typed stack of active input contexts.</summary>
    public sealed class InputContextService
    {
        private readonly List<InputContextLease> leases = new List<InputContextLease>();

        public InputContext CurrentContext { get; private set; } = InputContext.Gameplay;
        public event Action<InputContext> ContextChanged;

        /// <summary>Pushes an input context until the returned lease is released.</summary>
        public InputContextLease Acquire(InputContext context, InputBlockReason reason)
        {
            var lease = new InputContextLease(Guid.NewGuid(), context, reason, Release);
            leases.Add(lease);
            SetCurrentContext(context);
            return lease;
        }

        /// <summary>Releases a valid lease and restores the previous context.</summary>
        public void Release(InputContextLease lease)
        {
            if (lease == null || lease.IsReleased || !leases.Remove(lease)) return;
            lease.IsReleased = true;
            SetCurrentContext(leases.Count == 0 ? InputContext.Gameplay : leases[leases.Count - 1].Context);
        }

        /// <summary>Clears all leases and activates gameplay input.</summary>
        public void ResetToGameplay()
        {
            ClearSceneLeases();
        }

        /// <summary>Invalidates every scene lease during scene changes or death.</summary>
        public void ClearSceneLeases()
        {
            foreach (InputContextLease lease in leases) lease.IsReleased = true;
            leases.Clear();
            SetCurrentContext(InputContext.Gameplay);
        }

        private void SetCurrentContext(InputContext context)
        {
            if (CurrentContext == context) return;
            CurrentContext = context;
            ContextChanged?.Invoke(context);
        }
    }
}
