using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorRPG.Battle
{
    [CreateAssetMenu(fileName = "New Attack", menuName = "Battle/Attack Data")]
    public class AttackData : ScriptableObject
    {
        [SerializeField] private string stableId;
        [Header("Attack Info")]
        [SerializeField, FormerlySerializedAs("attackName")] private string attackNameValue = "New Attack";
        [Header("Projectiles")]
        [SerializeField, FormerlySerializedAs("projectileSpawns")] private List<ProjectileSpawnData> projectileSpawnsValue = new List<ProjectileSpawnData>();

        public string Id => stableId;
        public string attackName => attackNameValue;
        public IReadOnlyList<ProjectileSpawnData> projectileSpawns => projectileSpawnsValue;

        /// <summary>Returns the sequential sum of configured spawn delays.</summary>
        public float GetTotalAttackDuration()
        {
            float duration = 0f;
            foreach (ProjectileSpawnData spawn in projectileSpawnsValue)
                if (spawn != null) duration += spawn.spawnDelay;
            return duration;
        }

        /// <summary>Returns the number of configured projectile entries.</summary>
        public int GetProjectileCount() => projectileSpawnsValue.Count;

        /// <summary>Returns whether every projectile entry has valid configuration.</summary>
        public bool IsValid()
        {
            if (projectileSpawnsValue.Count == 0) return false;
            foreach (ProjectileSpawnData spawn in projectileSpawnsValue)
                if (spawn == null || spawn.projectileConfig == null || spawn.damageMultiplier < 0f) return false;
            return true;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stableId)) stableId = Guid.NewGuid().ToString("N");
            stableId = stableId.Trim();
            attackNameValue = attackNameValue?.Trim() ?? string.Empty;
            projectileSpawnsValue ??= new List<ProjectileSpawnData>();
            foreach (ProjectileSpawnData spawn in projectileSpawnsValue) spawn?.Validate();
        }
    }
}
