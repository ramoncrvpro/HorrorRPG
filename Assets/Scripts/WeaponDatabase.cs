namespace HorrorRPG.Inventory
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Database/Weapon Database")]
public class WeaponDatabase : ScriptableObject
{
    [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();

    public WeaponData GetWeaponByName(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName))
            return null;

        foreach (WeaponData weapon in weapons)
        {
            if (weapon != null && weapon.name == weaponName)
            {
                return weapon;
            }
        }

        return null;
    }

    public List<WeaponData> GetAllWeapons()
    {
        return new List<WeaponData>(weapons);
    }

    public bool ContainsWeapon(WeaponData weapon)
    {
        return weapons.Contains(weapon);
    }
}


}
