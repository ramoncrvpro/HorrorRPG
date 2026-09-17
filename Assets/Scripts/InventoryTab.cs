namespace HorrorRPG.Inventory
{
using HorrorRPG.Presentation;
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
