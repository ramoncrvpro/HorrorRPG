namespace HorrorRPG.Inventory
{
using HorrorRPG.Presentation;

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
