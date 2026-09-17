using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HorrorRPG.Battle
{
    public enum ProjectileAttackStatus { Completed, InvalidConfiguration, Cancelled }

    public readonly struct ProjectileAttackHandle
    {
        internal ProjectileAttackHandle(Guid id) => Id = id;
        internal Guid Id { get; }
    }

    public readonly struct ProjectileResolution
    {
        public ProjectileResolution(DefensePosition position, int damage, DefenseType defenseType)
        {
            Position = position;
            Damage = Mathf.Max(0, damage);
            DefenseType = defenseType;
        }

        public DefensePosition Position { get; }
        public int Damage { get; }
        public DefenseType DefenseType { get; }
    }

    public sealed class ProjectileAttackResult
    {
        public ProjectileAttackResult(ProjectileAttackStatus status, IReadOnlyList<ProjectileResolution> resolutions)
        {
            Status = status;
            Resolutions = resolutions ?? Array.Empty<ProjectileResolution>();
        }

        public ProjectileAttackStatus Status { get; }
        public IReadOnlyList<ProjectileResolution> Resolutions { get; }
    }

    /// <summary>Owns pooled projectile executions and one completion callback per attack.</summary>
    public class ProjectileManager : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Transform spawnPoint;
        [Header("Defense Targets")]
        [SerializeField] private Transform leftTarget;
        [SerializeField] private Transform upTarget;
        [SerializeField] private Transform rightTarget;
        [Header("Pool Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private int poolSize = 10;
        [SerializeField] private DefenseManager defenseManager;

        private readonly List<BattleProjectile> projectilePool = new List<BattleProjectile>();
        private readonly Dictionary<Guid, ProjectileExecution> executions = new Dictionary<Guid, ProjectileExecution>();
        private readonly Dictionary<BattleProjectile, ProjectileExecution> projectileOwners = new Dictionary<BattleProjectile, ProjectileExecution>();

        private void Awake()
        {
            if (defenseManager == null) defenseManager = GetComponent<DefenseManager>();
            InitializePool();
        }

        /// <summary>Executes a configured attack and invokes resolution and completion callbacks.</summary>
        public ProjectileAttackHandle Execute(AttackData attack, int baseDamage, Action<ProjectileResolution> resolved, Action<ProjectileAttackResult> completed)
        {
            if (resolved == null) throw new ArgumentNullException(nameof(resolved));
            if (completed == null) throw new ArgumentNullException(nameof(completed));
            var handle = new ProjectileAttackHandle(Guid.NewGuid());
            if (!IsExecutionValid(attack))
            {
                completed(new ProjectileAttackResult(ProjectileAttackStatus.InvalidConfiguration, Array.Empty<ProjectileResolution>()));
                return handle;
            }

            var execution = new ProjectileExecution(handle.Id, attack.GetProjectileCount(), resolved, completed);
            executions.Add(handle.Id, execution);
            execution.SpawnCoroutine = StartCoroutine(SpawnExecution(execution, attack, Mathf.Max(0, baseDamage)));
            return handle;
        }

        /// <summary>Cancels one valid attack execution and returns its unresolved projectiles.</summary>
        public void Cancel(ProjectileAttackHandle handle)
        {
            if (!executions.TryGetValue(handle.Id, out ProjectileExecution execution) || execution.Finished) return;
            if (execution.SpawnCoroutine != null) StopCoroutine(execution.SpawnCoroutine);
            foreach (BattleProjectile projectile in new List<BattleProjectile>(execution.ActiveProjectiles)) projectile.ReturnToPool();
            FinishExecution(execution, ProjectileAttackStatus.Cancelled);
        }

        private void InitializePool()
        {
            if (projectilePrefab == null) return;
            for (int index = 0; index < Mathf.Max(0, poolSize); index++) CreatePooledProjectile(false);
        }

        private IEnumerator SpawnExecution(ProjectileExecution execution, AttackData attack, int baseDamage)
        {
            foreach (ProjectileSpawnData spawnData in attack.projectileSpawns)
            {
                if (spawnData.spawnDelay > 0f) yield return new WaitForSeconds(spawnData.spawnDelay);
                if (execution.Finished) yield break;
                int damage = Mathf.RoundToInt(baseDamage * spawnData.damageMultiplier);
                SpawnProjectile(execution, spawnData.projectileConfig, damage);
            }
            execution.SpawningComplete = true;
            TryCompleteExecution(execution);
        }

        private void SpawnProjectile(ProjectileExecution execution, ProjectileConfig config, int damage)
        {
            BattleProjectile projectile = GetProjectileFromPool();
            if (projectile == null)
            {
                ProjectileResolution resolution = new ProjectileResolution(DefensePosition.Up, damage, DefenseType.None);
                execution.Resolutions.Add(resolution);
                execution.ResolvedCount++;
                execution.Resolved?.Invoke(resolution);
                TryCompleteExecution(execution);
                return;
            }

            projectile.gameObject.SetActive(true);
            execution.ActiveProjectiles.Add(projectile);
            projectileOwners.Add(projectile, execution);
            Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position;
            projectile.Initialize(new ProjectileExecutionContext(config, center, damage, HandleProjectileReady, HandleProjectileReachedTarget, ReturnProjectileToPool));
        }

        private void HandleProjectileReady(BattleProjectile projectile)
        {
            if (!projectileOwners.ContainsKey(projectile))
            {
                projectile.ReturnToPool();
                return;
            }
            DefensePosition position = (DefensePosition)UnityEngine.Random.Range(0, 3);
            Transform target = GetTarget(position);
            if (target == null)
            {
                ResolveProjectile(projectile, position, DefenseType.None, projectile.DamageAmount);
                return;
            }
            projectile.BeginTravel(target, position);
        }

        private void HandleProjectileReachedTarget(BattleProjectile projectile)
        {
            if (!projectileOwners.ContainsKey(projectile)) return;
            DefenseResolution defense = defenseManager != null
                ? defenseManager.Resolve(projectile.TargetPosition, projectile.DamageAmount)
                : new DefenseResolution(projectile.TargetPosition, projectile.DamageAmount, DefenseType.None);
            ResolveProjectile(projectile, defense.Position, defense.Type, defense.Damage);
        }

        private void ResolveProjectile(BattleProjectile projectile, DefensePosition position, DefenseType defenseType, int damage)
        {
            if (!projectileOwners.TryGetValue(projectile, out ProjectileExecution execution)) return;
            projectileOwners.Remove(projectile);
            execution.ActiveProjectiles.Remove(projectile);
            execution.Resolutions.Add(new ProjectileResolution(position, damage, defenseType));
            execution.ResolvedCount++;
            execution.Resolved?.Invoke(execution.Resolutions[execution.Resolutions.Count - 1]);
            projectile.HitTarget();
            TryCompleteExecution(execution);
        }

        private void ReturnProjectileToPool(BattleProjectile projectile)
        {
            if (projectileOwners.TryGetValue(projectile, out ProjectileExecution execution))
            {
                projectileOwners.Remove(projectile);
                execution.ActiveProjectiles.Remove(projectile);
            }
            projectile.gameObject.SetActive(false);
        }

        private void TryCompleteExecution(ProjectileExecution execution)
        {
            if (execution.SpawningComplete && execution.ResolvedCount >= execution.ExpectedCount)
                FinishExecution(execution, ProjectileAttackStatus.Completed);
        }

        private void FinishExecution(ProjectileExecution execution, ProjectileAttackStatus status)
        {
            if (execution.Finished) return;
            execution.Finished = true;
            executions.Remove(execution.Id);
            Action<ProjectileAttackResult> callback = execution.Completed;
            execution.Completed = null;
            callback?.Invoke(new ProjectileAttackResult(status, execution.Resolutions.ToArray()));
        }

        private BattleProjectile GetProjectileFromPool()
        {
            foreach (BattleProjectile projectile in projectilePool)
            {
                if (!projectile.gameObject.activeSelf) return projectile;
            }
            return CreatePooledProjectile(true);
        }

        private BattleProjectile CreatePooledProjectile(bool active)
        {
            if (projectilePrefab == null) return null;
            GameObject instance = Instantiate(projectilePrefab, transform);
            if (!instance.TryGetComponent(out BattleProjectile projectile))
            {
                Destroy(instance);
                return null;
            }
            projectilePool.Add(projectile);
            instance.SetActive(active);
            return projectile;
        }

        private Transform GetTarget(DefensePosition position)
        {
            return position switch
            {
                DefensePosition.Left => leftTarget,
                DefensePosition.Up => upTarget,
                DefensePosition.Right => rightTarget,
                _ => null
            };
        }

        private bool IsExecutionValid(AttackData attack)
        {
            if (attack == null || projectilePrefab == null || attack.projectileSpawns == null || attack.projectileSpawns.Count == 0) return false;
            foreach (ProjectileSpawnData spawnData in attack.projectileSpawns)
            {
                if (spawnData == null || spawnData.projectileConfig == null || spawnData.damageMultiplier < 0f) return false;
            }
            return true;
        }

        private void OnDisable()
        {
            foreach (ProjectileExecution execution in new List<ProjectileExecution>(executions.Values))
                Cancel(new ProjectileAttackHandle(execution.Id));
        }

        private sealed class ProjectileExecution
        {
            public ProjectileExecution(Guid id, int expectedCount, Action<ProjectileResolution> resolved, Action<ProjectileAttackResult> completed)
            {
                Id = id;
                ExpectedCount = expectedCount;
                Resolved = resolved;
                Completed = completed;
            }

            public Guid Id { get; }
            public int ExpectedCount { get; }
            public int ResolvedCount { get; set; }
            public bool SpawningComplete { get; set; }
            public bool Finished { get; set; }
            public Coroutine SpawnCoroutine { get; set; }
            public Action<ProjectileResolution> Resolved { get; }
            public Action<ProjectileAttackResult> Completed { get; set; }
            public List<ProjectileResolution> Resolutions { get; } = new List<ProjectileResolution>();
            public HashSet<BattleProjectile> ActiveProjectiles { get; } = new HashSet<BattleProjectile>();
        }
    }
}
