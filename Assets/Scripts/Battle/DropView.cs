using System;
using System.Collections.Generic;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Inventory;
using UnityEngine;

namespace HorrorRPG.Battle
{
    /// <summary>Displays effective post-battle rewards as a modal, non-interactive item list.</summary>
    public sealed class DropView : MonoBehaviour, IGameContextReceiver
    {
        [SerializeField] private GameObject dropCanvas;
        [SerializeField] private ItemSlotUI[] itemSlots;

        private GameInputReader inputReader;
        private InputContextService inputContext;
        private InputContextLease inputLease;
        private UINavigationService navigationService;
        private UINavigationHandle navigationHandle;
        private bool initialized;
        private bool isOpen;

        public bool IsOpen => isOpen;
        public event Action Closed;

        /// <summary>Injects input and navigation services and ensures the view starts closed.</summary>
        public void Initialize(GameContext context)
        {
            if (initialized) return;
            if (context == null) throw new ArgumentNullException(nameof(context));
            inputReader = context.InputReader;
            inputContext = context.Input;
            navigationService = context.Navigation;
            inputReader.SubmitPerformed += HandleSubmit;
            initialized = true;
            Close();
        }

        /// <summary>Closes the view and releases its scene-owned subscriptions and leases.</summary>
        public void Deinitialize()
        {
            if (!initialized) return;
            inputReader.SubmitPerformed -= HandleSubmit;
            Close();
            inputReader = null;
            inputContext = null;
            navigationService = null;
            initialized = false;
        }

        /// <summary>Opens the modal and renders only the supplied effective reward quantities.</summary>
        public void Open(IReadOnlyList<InventoryEntry> entries)
        {
            if (!initialized || entries == null || entries.Count == 0) return;
            Close();
            ClearSlots();
            int slotCount = itemSlots?.Length ?? 0;
            for (int index = 0; index < slotCount && index < entries.Count; index++)
            {
                InventoryEntry entry = entries[index];
                if (entry.Item != null && entry.Quantity > 0) itemSlots[index].Setup(entry.Item, entry.Quantity);
            }

            isOpen = true;
            inputLease = inputContext?.Acquire(InputContext.UI, InputBlockReason.Menu);
            navigationHandle = navigationService.Push(UIScreenId.Drop, Close);
            if (dropCanvas != null) dropCanvas.SetActive(true);
        }

        /// <summary>Closes the modal idempotently and releases its input/navigation ownership.</summary>
        public void Close()
        {
            bool wasOpen = isOpen;
            isOpen = false;
            if (dropCanvas != null) dropCanvas.SetActive(false);
            ClearSlots();
            navigationService?.Pop(navigationHandle);
            navigationHandle = default;
            inputLease?.Dispose();
            inputLease = null;
            if (wasOpen) Closed?.Invoke();
        }

        private void HandleSubmit()
        {
            if (isOpen) Close();
        }

        private void ClearSlots()
        {
            if (itemSlots == null) return;
            foreach (ItemSlotUI slot in itemSlots) slot?.Clear();
        }

        private void OnDestroy() => Deinitialize();
    }
}
