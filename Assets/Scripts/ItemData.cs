namespace HorrorRPG.Inventory
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

public enum ItemCategory
{
    Consumable,
    Equipable,
    Key
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public string description;
    public Sprite icon;

    [Header("Classification")]
    public ItemCategory category = ItemCategory.Consumable;

    [Header("Properties")]
    public int maxStackSize = 99;
    public bool disposable = true;
    
    public bool isStackable => maxStackSize > 1;
}

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int quantity;

    public InventorySlot(ItemData data, int qty)
    {
        itemData = data;
        quantity = qty;
    }

    public bool CanStack(ItemData data)
    {
        return itemData == data && itemData.isStackable && quantity < itemData.maxStackSize;
    }

    public int AddQuantity(int amount)
    {
        int totalAmount = quantity + amount;
        
        if (totalAmount > itemData.maxStackSize)
        {
            quantity = itemData.maxStackSize;
            return totalAmount - itemData.maxStackSize;
        }
        else
        {
            quantity = totalAmount;
            return 0;
        }
    }
}


}
