namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;

    [Header("Defense Targets")]
    [SerializeField] private Transform leftTarget;
    [SerializeField] private Transform upTarget;
    [SerializeField] private Transform rightTarget;

    [Header("Pool Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize = 10;

    private List<BattleProjectile> projectilePool = new List<BattleProjectile>();
    private BattleProjectile activeProjectile;
    private Dictionary<BattleProjectile, System.Action<int, DefenseType>> projectileCallbacks = new Dictionary<BattleProjectile, System.Action<int, DefenseType>>();
    private System.Action<int, DefenseType> onProjectileResolved;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(projectilePrefab, transform);
            obj.SetActive(false);
            BattleProjectile projectile = obj.GetComponent<BattleProjectile>();
            if (projectile != null)
            {
                projectilePool.Add(projectile);
            }
        }
    }

    private void Update()
    {
    }

    public IEnumerator ExecuteAttack(AttackData attackData, int baseDamage)
    {
        if (attackData == null || !attackData.IsValid())
        {
            Debug.LogError("AttackData inválido!");
            onProjectileResolved?.Invoke(baseDamage, DefenseType.None);
            yield break;
        }

        List<BattleProjectile> activeProjectiles = new List<BattleProjectile>();
        List<bool> projectileResolved = new List<bool>();

        for (int i = 0; i < attackData.projectileSpawns.Count; i++)
        {
            projectileResolved.Add(false);
        }

        for (int i = 0; i < attackData.projectileSpawns.Count; i++)
        {
            ProjectileSpawnData spawnData = attackData.projectileSpawns[i];
            
            if (spawnData.spawnDelay > 0f)
            {
                yield return new WaitForSeconds(spawnData.spawnDelay);
            }

            int projectileDamage = Mathf.RoundToInt(baseDamage * spawnData.damageMultiplier);
            int projectileIndex = i;

            System.Action<int, DefenseType> projectileCallback = (damage, defenseType) =>
            {
                projectileResolved[projectileIndex] = true;
                onProjectileResolved?.Invoke(damage, defenseType);
            };

            StartCoroutine(SpawnSingleProjectile(spawnData.projectileConfig, projectileDamage, projectileCallback, activeProjectiles));
        }

        while (!AllProjectilesResolved(projectileResolved))
        {
            yield return null;
        }

        activeProjectiles.Clear();
    }

    private bool AllProjectilesResolved(List<bool> resolvedStates)
    {
        foreach (bool resolved in resolvedStates)
        {
            if (!resolved)
                return false;
        }
        return true;
    }

    private IEnumerator SpawnSingleProjectile(ProjectileConfig config, int damage, System.Action<int, DefenseType> callback, List<BattleProjectile> activeList)
    {
        BattleProjectile projectile = GetProjectileFromPool();
        if (projectile == null)
        {
            Debug.LogError("Nenhum projétil disponível no pool!");
            callback?.Invoke(damage, DefenseType.None);
            yield break;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        projectile.gameObject.SetActive(true);
        projectile.Initialize(config, spawnPosition, damage);
        
        projectileCallbacks[projectile] = callback;
        activeList.Add(projectile);
    }

    public IEnumerator ExecuteProjectileAttack(ProjectileConfig config, int damage)
    {
        BattleProjectile projectile = GetProjectileFromPool();
        if (projectile == null)
        {
            Debug.LogError("Nenhum projétil disponível no pool!");
            onProjectileResolved?.Invoke(damage, DefenseType.None);
            yield break;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;

        projectile.gameObject.SetActive(true);
        projectile.Initialize(config, spawnPosition, damage);
        activeProjectile = projectile;

        while (activeProjectile != null && activeProjectile.CurrentState == ProjectileState.Looping)
        {
            yield return null;
        }

        while (activeProjectile != null && 
               activeProjectile.CurrentState == ProjectileState.Traveling)
        {
            yield return null;
        }

        activeProjectile = null;
    }

    public void OnProjectileReadyToAttack(BattleProjectile projectile)
    {
        bool isFromNewSystem = projectileCallbacks.ContainsKey(projectile);
        bool isFromOldSystem = projectile == activeProjectile;
        
        if (!isFromNewSystem && !isFromOldSystem)
            return;

        DefensePosition randomPosition = (DefensePosition)Random.Range(0, 3);
        Transform target = GetTargetTransform(randomPosition);

        if (target != null)
        {
            projectile.StartTravelToTarget(target, randomPosition);
        }
        else
        {
            Debug.LogError("Target transform não encontrado!");
            
            if (isFromNewSystem)
            {
                if (projectileCallbacks.TryGetValue(projectile, out var callback))
                {
                    callback?.Invoke(0, DefenseType.None);
                    projectileCallbacks.Remove(projectile);
                }
            }
            
            ReturnProjectileToPool(projectile);
        }
    }

    public void OnProjectileReachedTarget(BattleProjectile projectile)
    {
        bool isFromNewSystem = projectileCallbacks.ContainsKey(projectile);
        bool isFromOldSystem = projectile == activeProjectile;
        
        if (!isFromNewSystem && !isFromOldSystem)
            return;

        DefensePosition position = projectile.TargetPosition;
        int baseDamage = projectile.DamageAmount;
        
        int finalDamage = baseDamage;
        DefenseType defenseType = DefenseType.None;
        
        if (DefenseManager.Instance != null)
        {
            finalDamage = DefenseManager.Instance.OnProjectileHit(position, baseDamage, out defenseType);
        }

        projectile.HitTarget();
        
        if (isFromNewSystem)
        {
            if (projectileCallbacks.TryGetValue(projectile, out var callback))
            {
                callback?.Invoke(finalDamage, defenseType);
                projectileCallbacks.Remove(projectile);
            }
        }
        else
        {
            onProjectileResolved?.Invoke(finalDamage, defenseType);
        }
    }

    private Transform GetTargetTransform(DefensePosition position)
    {
        switch (position)
        {
            case DefensePosition.Left:
                return leftTarget;
            case DefensePosition.Up:
                return upTarget;
            case DefensePosition.Right:
                return rightTarget;
            default:
                return null;
        }
    }

    private BattleProjectile GetProjectileFromPool()
    {
        foreach (BattleProjectile projectile in projectilePool)
        {
            if (!projectile.gameObject.activeInHierarchy)
            {
                return projectile;
            }
        }

        GameObject newObj = Instantiate(projectilePrefab, transform);
        BattleProjectile newProjectile = newObj.GetComponent<BattleProjectile>();
        if (newProjectile != null)
        {
            projectilePool.Add(newProjectile);
            return newProjectile;
        }

        return null;
    }

    public void ReturnProjectileToPool(BattleProjectile projectile)
    {
        if (projectile != null)
        {
            projectile.gameObject.SetActive(false);
        }
    }

    public void SetDamageCallback(System.Action<int, DefenseType> callback)
    {
        onProjectileResolved = callback;
    }
}


}
