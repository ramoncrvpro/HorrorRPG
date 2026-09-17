using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorRPG.Battle
{
    [Serializable]
    public class ProjectileSpawnData
    {
        [Header("Projectile Settings")]
        [SerializeField, FormerlySerializedAs("projectileConfig")] private ProjectileConfig projectileConfigValue;
        [Header("Timing")]
        [SerializeField, FormerlySerializedAs("spawnDelay")] private float spawnDelayValue;
        [Header("Damage")]
        [SerializeField, FormerlySerializedAs("damageMultiplier"), Range(0f, 5f)] private float damageMultiplierValue = 1f;

        public ProjectileConfig projectileConfig => projectileConfigValue;
        public float spawnDelay => spawnDelayValue;
        public float damageMultiplier => damageMultiplierValue;

        /// <summary>Clamps serialized projectile spawn invariants.</summary>
        public void Validate()
        {
            spawnDelayValue = Mathf.Max(0f, spawnDelayValue);
            damageMultiplierValue = Mathf.Clamp(damageMultiplierValue, 0f, 5f);
        }
    }
}
