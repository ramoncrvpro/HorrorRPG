using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorRPG.Inventory
{
    public enum ItemCategory { Consumable, Equipable, Key }

    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string stableId;
        [Header("Item Info")]
        [SerializeField, FormerlySerializedAs("itemName")] private string itemNameValue;
        [SerializeField, FormerlySerializedAs("description")] private string descriptionValue;
        [SerializeField, FormerlySerializedAs("icon")] private Sprite iconValue;
        [Header("Classification")]
        [SerializeField, FormerlySerializedAs("category")] private ItemCategory categoryValue = ItemCategory.Consumable;
        [Header("Properties")]
        [SerializeField, FormerlySerializedAs("maxStackSize")] private int maxStackSizeValue = 99;
        [SerializeField, FormerlySerializedAs("disposable")] private bool disposableValue = true;

        public string Id => stableId;
        public string itemName => itemNameValue;
        public string description => descriptionValue;
        public Sprite icon => iconValue;
        public ItemCategory category => categoryValue;
        public int maxStackSize => maxStackSizeValue;
        public bool disposable => disposableValue;
        public bool isStackable => maxStackSizeValue > 1;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stableId)) stableId = Guid.NewGuid().ToString("N");
            stableId = stableId.Trim();
            itemNameValue = itemNameValue?.Trim() ?? string.Empty;
            descriptionValue ??= string.Empty;
            maxStackSizeValue = Mathf.Max(1, maxStackSizeValue);
        }
    }
}
