namespace HorrorRPG.Inventory
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;

using UnityEngine;

public class InventoryTab : BaseTab
{
    private ItemCategory category;

    public void Initialize(ItemCategory itemCategory, string displayText)
    {
        category = itemCategory;
        SetTabText(displayText);
        SetSelected(false);
    }

    public ItemCategory GetCategory()
    {
        return category;
    }
}


}
