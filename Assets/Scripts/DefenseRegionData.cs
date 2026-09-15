namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

[System.Serializable]
public class DefenseRegionData
{
    public DefensePosition position;
    public float defenseTimer;
    public bool isActive;

    private const float MAX_DEFENSE_TIME = 1.0f;

    public DefenseRegionData(DefensePosition pos)
    {
        position = pos;
        defenseTimer = 0f;
        isActive = false;
    }

    public void Activate()
    {
        isActive = true;
        defenseTimer = 0f;
    }

    public void UpdateTimer(float deltaTime)
    {
        if (isActive)
        {
            defenseTimer += deltaTime;
            
            if (defenseTimer >= MAX_DEFENSE_TIME)
            {
                Reset();
            }
        }
    }

    public void Reset()
    {
        defenseTimer = 0f;
        isActive = false;
    }

    public float GetDamageMultiplier()
    {
        if (!isActive)
            return 1.0f;

        float normalizedTime = defenseTimer / MAX_DEFENSE_TIME;
        
        if (normalizedTime < 0.1f) return 0.0f;
        if (normalizedTime < 0.2f) return 0.5f;
        if (normalizedTime < 0.6f) return 1.0f;
        if (normalizedTime < 0.9f) return 1.5f;
        return 1.0f;
    }

    public bool HasDefense()
    {
        return isActive;
    }
}


}
