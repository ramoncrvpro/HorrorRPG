namespace HorrorRPG.Inventory
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

public enum ConsumableEffectType
{
    HealHealth,
    IncreaseDamage,
    DecreaseMarkerSpeed
}

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class ConsumableData : ItemData
{
    [Header("Consumable Properties")]
    public ConsumableEffectType effectType = ConsumableEffectType.HealHealth;
    public int effectValue;
    public float effectDuration = 1f;

    public string GetEffectDescription()
    {
        switch (effectType)
        {
            case ConsumableEffectType.HealHealth:
                return $"Restaura {effectValue} pontos de vida";
            
            case ConsumableEffectType.IncreaseDamage:
                return $"Aumenta o dano em {effectValue} no próximo ataque";
            
            case ConsumableEffectType.DecreaseMarkerSpeed:
                return $"Reduz a velocidade do marker em {effectValue}%";
            
            default:
                return "Efeito desconhecido";
        }
    }
}


}
