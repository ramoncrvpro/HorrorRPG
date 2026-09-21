using System;
using System.Collections.Generic;
using HorrorRPG.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HorrorRPG.Inventory
{
    /// <summary>Renders inventory entries and exposes navigation outcomes without owning inventory rules.</summary>
    public sealed class InventoryView : MonoBehaviour
    {
        private const string DiscardConfirmationMessage = "Tem certeza que deseja descartar este item?";
        private static readonly ItemCategory[] Categories =
        {
            ItemCategory.Consumable,
            ItemCategory.Equipable,
            ItemCategory.Key
        };
        private static readonly string[] CategoryLabels = { "CONS", "EQUIP", "KEY" };

        [SerializeField] private GameObject inventoryCanvas;
        [SerializeField] private ItemSlotUI[] itemSlots;
        [SerializeField] private InventoryTab[] tabs;
        [SerializeField] private Image itemIconDisplay;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private ConfirmationMenu discardConfirmationMenu;
        [SerializeField] private GameObject emptyMessageObject;
        [SerializeField] private GameObject descriptionBackground;
        [SerializeField] private GameObject iconBackground;

        private IReadOnlyList<InventoryEntry> entries = Array.Empty<InventoryEntry>();
        private int selectedIndex = -1;
        private int tabIndex;
        private bool confirmationOpen;

        public event Action CloseRequested;
        public event Action<InventoryEntry> DiscardRequested;
        public event Action<ItemCategory> CategoryChanged;
        public event Action ConfirmationOpened;
        public event Action ConfirmationClosed;

        public ItemCategory CurrentCategory => Categories[tabIndex];
        public bool IsOpen => inventoryCanvas != null && inventoryCanvas.activeSelf;

        private void Awake()
        {
            if (itemSlots != null)
            {
                foreach (ItemSlotUI slot in itemSlots)
                {
                    if (slot != null) slot.Clicked += HandleSlotClicked;
                }
            }
            if (tabs != null)
            {
                for (int index = 0; index < tabs.Length && index < Categories.Length; index++)
                {
                    if (tabs[index] != null) tabs[index].Initialize(Categories[index], CategoryLabels[index]);
                }
            }
            if (discardConfirmationMenu != null)
            {
                discardConfirmationMenu.Confirmed += ConfirmDiscard;
                discardConfirmationMenu.Cancelled += CancelDiscard;
            }
            Close();
        }

        private void OnDestroy()
        {
            if (itemSlots != null)
            {
                foreach (ItemSlotUI slot in itemSlots)
                {
                    if (slot != null) slot.Clicked -= HandleSlotClicked;
                }
            }
            if (discardConfirmationMenu != null)
            {
                discardConfirmationMenu.Confirmed -= ConfirmDiscard;
                discardConfirmationMenu.Cancelled -= CancelDiscard;
            }
        }

        /// <summary>Opens the inventory and resets selection to the consumables tab.</summary>
        public void Open()
        {
            CloseConfirmation(false);
            tabIndex = 0;
            selectedIndex = -1;
            if (inventoryCanvas != null) inventoryCanvas.SetActive(true);
            UpdateTabs();
        }

        /// <summary>Closes the inventory and any nested confirmation.</summary>
        public void Close()
        {
            CloseConfirmation(false);
            if (inventoryCanvas != null) inventoryCanvas.SetActive(false);
            entries = Array.Empty<InventoryEntry>();
            selectedIndex = -1;
            ClearItemDisplay();
        }

        /// <summary>Renders immutable entries for the active category.</summary>
        public void Render(IReadOnlyList<InventoryEntry> visibleEntries)
        {
            entries = visibleEntries ?? Array.Empty<InventoryEntry>();
            int slotCount = itemSlots?.Length ?? 0;
            for (int index = 0; index < slotCount; index++)
            {
                if (itemSlots[index] == null) continue;
                if (index < entries.Count) itemSlots[index].Setup(entries[index]);
                else itemSlots[index].Clear();
            }

            bool hasEntries = entries.Count > 0;
            if (emptyMessageObject != null) emptyMessageObject.SetActive(!hasEntries);
            if (descriptionBackground != null) descriptionBackground.SetActive(hasEntries);
            if (iconBackground != null) iconBackground.SetActive(hasEntries);
            SelectIndex(hasEntries ? Mathf.Clamp(selectedIndex, 0, entries.Count - 1) : -1);
        }

        /// <summary>Moves tab or item selection based on a UI navigation vector.</summary>
        public void Navigate(Vector2 navigation)
        {
            if (confirmationOpen)
            {
                discardConfirmationMenu?.HandleNavigation(navigation);
                return;
            }
            if (navigation.sqrMagnitude < 0.25f) return;
            if (Mathf.Abs(navigation.x) > Mathf.Abs(navigation.y)) ChangeTab(navigation.x > 0f ? 1 : -1);
            else ChangeSelection(navigation.y < 0f ? 1 : -1);
        }

        /// <summary>Opens discard confirmation or confirms its active selection.</summary>
        public void Submit()
        {
            if (confirmationOpen)
            {
                discardConfirmationMenu?.ExecuteCurrentSelection();
                return;
            }
            if (selectedIndex < 0 || selectedIndex >= entries.Count || !entries[selectedIndex].Item.disposable) return;
            confirmationOpen = true;
            if (itemDescriptionText != null) itemDescriptionText.text = DiscardConfirmationMessage;
            discardConfirmationMenu?.Show();
            ConfirmationOpened?.Invoke();
        }

        /// <summary>Cancels confirmation or requests closing the inventory.</summary>
        public void Cancel()
        {
            if (confirmationOpen) CancelDiscard();
            else CloseRequested?.Invoke();
        }

        private void ChangeTab(int direction)
        {
            tabIndex = (tabIndex + direction + Categories.Length) % Categories.Length;
            selectedIndex = -1;
            UpdateTabs();
            CategoryChanged?.Invoke(CurrentCategory);
        }

        private void ChangeSelection(int direction)
        {
            if (entries.Count == 0) return;
            int next = selectedIndex < 0 ? 0 : (selectedIndex + direction + entries.Count) % entries.Count;
            SelectIndex(next);
        }

        private void SelectIndex(int index)
        {
            selectedIndex = index;
            int slotCount = itemSlots?.Length ?? 0;
            for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
            {
                itemSlots[slotIndex]?.SetSelected(slotIndex == selectedIndex);
            }
            if (selectedIndex < 0 || selectedIndex >= entries.Count) ClearItemDisplay();
            else DisplayItem(entries[selectedIndex]);
        }

        private void HandleSlotClicked(ItemSlotUI slot)
        {
            if (itemSlots == null) return;
            SelectIndex(Array.IndexOf(itemSlots, slot));
        }

        private void DisplayItem(InventoryEntry entry)
        {
            if (itemIconDisplay != null)
            {
                itemIconDisplay.sprite = entry.Item.icon;
                itemIconDisplay.enabled = entry.Item.icon != null;
            }
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = entry.Item.disposable
                    ? entry.Item.description
                    : $"{entry.Item.description}. Cannot be discarded";
            }
        }

        private void ClearItemDisplay()
        {
            if (itemIconDisplay != null)
            {
                itemIconDisplay.sprite = null;
                itemIconDisplay.enabled = false;
            }
            if (itemDescriptionText != null) itemDescriptionText.text = string.Empty;
        }

        private void UpdateTabs()
        {
            if (tabs == null) return;
            for (int index = 0; index < tabs.Length; index++) tabs[index]?.SetSelected(index == tabIndex);
        }

        private void ConfirmDiscard()
        {
            if (selectedIndex >= 0 && selectedIndex < entries.Count) DiscardRequested?.Invoke(entries[selectedIndex]);
            CloseConfirmation(true);
        }

        private void CancelDiscard()
        {
            CloseConfirmation(true);
            if (selectedIndex >= 0 && selectedIndex < entries.Count) DisplayItem(entries[selectedIndex]);
        }

        private void CloseConfirmation(bool notify)
        {
            bool wasOpen = confirmationOpen;
            confirmationOpen = false;
            discardConfirmationMenu?.Hide();
            if (notify && wasOpen) ConfirmationClosed?.Invoke();
        }
    }
}
