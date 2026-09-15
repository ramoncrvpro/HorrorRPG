namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Attack", menuName = "Battle/Attack Data")]
public class AttackData : ScriptableObject
{
    [Header("Attack Info")]
    [Tooltip("Nome do ataque para identificação")]
    public string attackName = "New Attack";
    
    [Header("Projectiles")]
    [Tooltip("Lista de projéteis que serão spawnados neste ataque")]
    public List<ProjectileSpawnData> projectileSpawns = new List<ProjectileSpawnData>();
    
    public float GetTotalAttackDuration()
    {
        if (projectileSpawns == null || projectileSpawns.Count == 0)
            return 0f;
        
        float maxDelay = 0f;
        foreach (var spawn in projectileSpawns)
        {
            if (spawn.spawnDelay > maxDelay)
                maxDelay = spawn.spawnDelay;
        }
        
        return maxDelay;
    }
    
    public int GetProjectileCount()
    {
        return projectileSpawns?.Count ?? 0;
    }
    
    public bool IsValid()
    {
        if (projectileSpawns == null || projectileSpawns.Count == 0)
            return false;
        
        foreach (var spawn in projectileSpawns)
        {
            if (spawn.projectileConfig == null)
                return false;
        }
        
        return true;
    }
}


}
