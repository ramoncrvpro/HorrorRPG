namespace HorrorRPG.Inventory
{
using HorrorRPG.Battle;

using System.Collections.Generic;
using UnityEngine;

/// <summary>Scene adapter for the in-memory recent weapon order.</summary>
public class RecentWeaponsManager : MonoBehaviour
{
    public static RecentWeaponsManager Instance { get; private set; }
    [SerializeField] private WeaponDatabase weaponDatabase;
    private const int MaxRecentWeapons = 9;
    private readonly List<string> recentWeaponIds = new List<string>(MaxRecentWeapons);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    public void AddRecentWeapon(WeaponData weapon)
    {
        if (weapon == null) return;
        string weaponId = weapon.name;
        recentWeaponIds.Remove(weaponId);
        recentWeaponIds.Insert(0, weaponId);
        if (recentWeaponIds.Count > MaxRecentWeapons) recentWeaponIds.RemoveAt(recentWeaponIds.Count - 1);
    }
    public List<WeaponData> GetRecentWeapons()
    {
        var weapons = new List<WeaponData>();
        if (weaponDatabase == null) return weapons;
        foreach (string weaponId in recentWeaponIds)
        {
            WeaponData weapon = weaponDatabase.GetWeaponByName(weaponId);
            if (weapon != null) weapons.Add(weapon);
        }
        return weapons;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }
}
}
