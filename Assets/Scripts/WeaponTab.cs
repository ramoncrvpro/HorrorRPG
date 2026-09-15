namespace HorrorRPG.Inventory
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

public enum WeaponCategory
{
    Used,
    Basic,
    Limited
}

public class WeaponTab : BaseTab
{
    private WeaponCategory category;

    public void Initialize(WeaponCategory weaponCategory, string displayText)
    {
        category = weaponCategory;
        SetTabText(displayText);
        SetSelected(false);
    }

    public WeaponCategory GetCategory()
    {
        return category;
    }
}


}
