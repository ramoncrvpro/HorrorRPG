namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Config", menuName = "Battle/Projectile Config")]
public class ProjectileConfig : ScriptableObject
{
    [Header("Loop Settings")]
    public float loopRadius = 2f;
    public float loopSpeed = 5f;
    
    [Header("Attack Settings")]
    public float minLoopTime = 2f;
    public float maxLoopTime = 5f;
    public float minTravelSpeed = 3f;
    public float maxTravelSpeed = 8f;
    
    [Header("Visual")]
    public Material projectileMaterial;
}


}
