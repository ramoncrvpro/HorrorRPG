namespace HorrorRPG.Player
{
using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum ControlBlock { None = 0, Movement = 1, Interaction = 2, All = Movement | Interaction }
public sealed class ControlLease : IDisposable
{
    internal ControlLease(Guid token, ControlBlock block) { Token = token; Block = block; }
    internal Guid Token { get; }
    public ControlBlock Block { get; }
    internal bool Released { get; set; }
    public void Dispose() { Released = true; }
}

public class PlayerControlManager : MonoBehaviour
{
    public static PlayerControlManager Instance { get; private set; }
    private readonly Dictionary<Guid, ControlLease> leases = new Dictionary<Guid, ControlLease>();
    private readonly HashSet<string> legacyLocks = new HashSet<string>();
    private FirstPersonController playerController;
    private PlayerInteraction playerInteraction;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        playerController = GetComponent<FirstPersonController>();
        playerInteraction = GetComponent<PlayerInteraction>();
    }

    /// <summary>Acquires a typed control lease until it is released.</summary>
    public ControlLease Acquire(ControlBlock block)
    {
        if (block == ControlBlock.None) throw new ArgumentException("A control lease must block at least one capability.", nameof(block));
        var lease = new ControlLease(Guid.NewGuid(), block);
        leases.Add(lease.Token, lease);
        UpdateControlState();
        return lease;
    }
    /// <summary>Releases a previously acquired control lease.</summary>
    public void Release(ControlLease lease)
    {
        if (lease == null || lease.Released || !leases.Remove(lease.Token)) return;
        lease.Released = true;
        UpdateControlState();
    }
    public bool IsBlocked(ControlBlock block)
    {
        foreach (ControlLease lease in leases.Values) if ((lease.Block & block) != ControlBlock.None) return true;
        return legacyLocks.Count > 0;
    }
    public void ClearTransientBlocks() { foreach (ControlLease lease in leases.Values) lease.Released = true; leases.Clear(); legacyLocks.Clear(); UpdateControlState(); }

    // Compatibility adapter for legacy scene references.
    public void LockControl(string lockId) { if (!string.IsNullOrWhiteSpace(lockId) && legacyLocks.Add(lockId)) UpdateControlState(); }
    // Compatibility adapter for legacy scene references.
    public void UnlockControl(string lockId) { if (legacyLocks.Remove(lockId)) UpdateControlState(); }
    public bool IsControlLocked() => legacyLocks.Count > 0 || leases.Count > 0;
    public bool IsLockedBy(string lockId) => legacyLocks.Contains(lockId);
    // Compatibility adapter for legacy scene references.
    public void ClearAllLocks() => ClearTransientBlocks();

    private void UpdateControlState()
    {
        bool movementEnabled = !IsBlocked(ControlBlock.Movement) && !IsBlocked(ControlBlock.All);
        bool interactionEnabled = !IsBlocked(ControlBlock.Interaction) && !IsBlocked(ControlBlock.All);
        playerController?.SetMovementEnabled(movementEnabled);
        playerInteraction?.SetInteractionEnabled(interactionEnabled);
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }
}
}
