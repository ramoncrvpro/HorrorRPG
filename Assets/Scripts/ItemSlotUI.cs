using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HorrorRPG.Inventory
{
    /// <summary>Renders one immutable inventory entry.</summary>
    public class ItemSlotUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemAmountText;
        [SerializeField] private Image backgroundImage;

        private readonly Color selectedNameColor = Color.black;
        private readonly Color selectedAmountColor = Color.black;
        private Color defaultNameColor = Color.white;
        private Color defaultAmountColor = Color.white;
        private InventoryEntry entry;
        private bool hasEntry;

        public event Action<ItemSlotUI> Clicked;

        private void Awake()
        {
            if (itemNameText != null) defaultNameColor = itemNameText.color;
            if (itemAmountText != null) defaultAmountColor = itemAmountText.color;
            SetSelected(false);
        }

        /// <summary>Renders the supplied immutable inventory entry.</summary>
        public void Setup(InventoryEntry inventoryEntry)
        {
            entry = inventoryEntry;
            hasEntry = inventoryEntry.Item != null && inventoryEntry.Quantity > 0;
            SetSelected(false);
            if (!hasEntry)
            {
                Clear();
                return;
            }
            if (itemNameText != null) itemNameText.text = entry.Item.itemName;
            if (itemAmountText != null) itemAmountText.text = $"x{entry.Quantity}";
            gameObject.SetActive(true);
        }

        /// <summary>Renders an item and quantity for battle menus without manager coupling.</summary>
        public void Setup(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                Clear();
                return;
            }
            Setup(new InventoryEntry(item, quantity));
        }


        /// <summary>Clears the entry and selection before slot reuse.</summary>
        public void Clear()
        {
            hasEntry = false;
            entry = default;
            SetSelected(false);
            gameObject.SetActive(false);
        }

        /// <summary>Forwards a serialized UI click without referencing a manager.</summary>
        public void OnSlotClicked()
        {
            if (hasEntry) Clicked?.Invoke(this);
        }

        /// <summary>Updates selection colors and background visibility.</summary>
        public void SetSelected(bool selected)
        {
            if (backgroundImage != null) backgroundImage.enabled = selected;
            if (itemNameText != null) itemNameText.color = selected ? selectedNameColor : defaultNameColor;
            if (itemAmountText != null) itemAmountText.color = selected ? selectedAmountColor : defaultAmountColor;
        }

        /// <summary>Returns the immutable entry currently rendered by this slot.</summary>
        public InventoryEntry GetEntry() => entry;

        /// <summary>Returns the item currently rendered for battle menu compatibility.</summary>
        public ItemData GetItemData() => hasEntry ? entry.Item : null;
    }
}
