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

[CreateAssetMenu(fileName = "New Enemy", menuName = "Battle/Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy Info")]
    public string enemyName = "Enemy";
    
    [Header("Enemy Stats")]
    public int maxHealth = 50;
    public int baseDamage = 10;
    public EnemyType category = EnemyType.None;
    
    [Header("Attack System")]
    [Tooltip("Lista de ataques disponíveis para este inimigo")]
    public List<AttackData> availableAttacks = new List<AttackData>();
    
    public AttackData GetRandomAttack()
    {
        if (availableAttacks == null || availableAttacks.Count == 0)
            return null;
        
        List<AttackData> validAttacks = new List<AttackData>();
        foreach (var attack in availableAttacks)
        {
            if (attack != null && attack.IsValid())
            {
                validAttacks.Add(attack);
            }
        }
        
        if (validAttacks.Count == 0)
            return null;
        
        int randomIndex = Random.Range(0, validAttacks.Count);
        return validAttacks[randomIndex];
    }
}


}
