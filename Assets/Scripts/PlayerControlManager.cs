using System;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorRPG.Player
{
    [Flags]
    public enum ControlBlock { None = 0, Movement = 1, Interaction = 2, All = Movement | Interaction }

    public sealed class ControlLease : IDisposable
    {
        private readonly Action<ControlLease> release;

        internal ControlLease(Guid token, ControlBlock block, Action<ControlLease> releaseAction)
        {
            Token = token;
            Block = block;
            release = releaseAction ?? throw new ArgumentNullException(nameof(releaseAction));
        }

        internal Guid Token { get; }
        public ControlBlock Block { get; }
        public bool IsReleased { get; internal set; }

        /// <summary>Releases this control block exactly once.</summary>
        public void Dispose()
        {
            if (!IsReleased) release(this);
        }
    }

    /// <summary>Applies local typed movement and interaction blocks to the player.</summary>
    public class PlayerControlManager : MonoBehaviour
    {
        private readonly Dictionary<Guid, ControlLease> leases = new Dictionary<Guid, ControlLease>();
        private FirstPersonController playerController;
        private PlayerInteraction playerInteraction;

        private void Awake()
        {
            playerController = GetComponent<FirstPersonController>();
            playerInteraction = GetComponent<PlayerInteraction>();
        }

        /// <summary>Acquires a typed control lease until it is released.</summary>
        public ControlLease Acquire(ControlBlock block)
        {
            if (block == ControlBlock.None) throw new ArgumentException("A control lease must block at least one capability.", nameof(block));
            var lease = new ControlLease(Guid.NewGuid(), block, Release);
            leases.Add(lease.Token, lease);
            UpdateControlState();
            return lease;
        }

        /// <summary>Releases a previously acquired control lease.</summary>
        public void Release(ControlLease lease)
        {
            if (lease == null || lease.IsReleased || !leases.Remove(lease.Token)) return;
            lease.IsReleased = true;
            UpdateControlState();
        }

        /// <summary>Returns whether any active lease blocks the supplied capability.</summary>
        public bool IsBlocked(ControlBlock block)
        {
            foreach (ControlLease lease in leases.Values)
                if ((lease.Block & block) != ControlBlock.None) return true;
            return false;
        }

        /// <summary>Invalidates all local control leases.</summary>
        public void ClearTransientBlocks()
        {
            foreach (ControlLease lease in leases.Values) lease.IsReleased = true;
            leases.Clear();
            UpdateControlState();
        }

        private void UpdateControlState()
        {
            playerController?.SetMovementEnabled(!IsBlocked(ControlBlock.Movement));
            playerInteraction?.SetInteractionEnabled(!IsBlocked(ControlBlock.Interaction));
        }

        private void OnDisable() => ClearTransientBlocks();
    }
}
