namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

public enum EnemyType
{
    None,
    Demon,
    Ghost,
    Zombie
}

public enum AttackResult
{
    Miss,
    Hit,
    Critical
}

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class WeaponData : ItemData
{
    [Header("Weapon Stats")]
    public int baseDamage = 10;
    public EnemyType effectiveAgainst = EnemyType.None;
    public float effectivenessMultiplier = 2f;
    
    [Header("Ammo System")]
    public ItemData ammoType;
    
    [Header("Timing Bar Settings")]
    [Range(0.5f, 5f)]
    public float markerSpeed = 2f;
    
    [Header("Hit Zones (valores de 0 a 1)")]
    [Range(0f, 1f)]
    public float criticalZoneCenter = 0.5f;
    
    [Range(0.01f, 0.2f)]
    public float criticalZoneWidth = 0.05f;
    
    [Range(0.1f, 0.5f)]
    public float hitZoneWidth = 0.4f;
    
    [Range(1f, 3f)]
    public float criticalMultiplier = 1.5f;
    
    public bool requiresAmmo => ammoType != null;
    public bool IsDefaultWeapon => !requiresAmmo;
    
    public int GetEffectiveDamage(EnemyType targetType)
    {
        if (targetType == effectiveAgainst)
        {
            return Mathf.RoundToInt(baseDamage * effectivenessMultiplier);
        }
        return baseDamage;
    }
    
    public bool CanUse(InventoryManager inventory)
    {
        if (!requiresAmmo) 
            return true;
        
        return inventory.HasItem(ammoType, 1);
    }
    
    public AttackResult EvaluateTimingPosition(float normalizedPosition)
    {
        float criticalMin = criticalZoneCenter - (criticalZoneWidth / 2f);
        float criticalMax = criticalZoneCenter + (criticalZoneWidth / 2f);
        
        if (normalizedPosition >= criticalMin && normalizedPosition <= criticalMax)
        {
            return AttackResult.Critical;
        }
        
        float hitZoneLeftMin = Mathf.Max(0f, criticalMin - hitZoneWidth);
        float hitZoneLeftMax = criticalMin;
        
        float hitZoneRightMin = criticalMax;
        float hitZoneRightMax = Mathf.Min(1f, criticalMax + hitZoneWidth);
        
        if ((normalizedPosition >= hitZoneLeftMin && normalizedPosition < hitZoneLeftMax) ||
            (normalizedPosition > hitZoneRightMin && normalizedPosition <= hitZoneRightMax))
        {
            return AttackResult.Hit;
        }
        
        return AttackResult.Miss;
    }
    
    public int GetDamageByResult(AttackResult result, EnemyType targetType)
    {
        int baseDmg = GetEffectiveDamage(targetType);
        
        switch (result)
        {
            case AttackResult.Critical:
                return Mathf.RoundToInt(baseDmg * criticalMultiplier);
            case AttackResult.Hit:
                return baseDmg;
            case AttackResult.Miss:
                return 0;
            default:
                return baseDmg;
        }
    }
}


}
