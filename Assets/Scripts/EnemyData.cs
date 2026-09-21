using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorRPG.Battle
{
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Battle/Enemy")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] private string stableId;
        [Header("Enemy Info")]
        [SerializeField, FormerlySerializedAs("enemyName")] private string enemyNameValue = "Enemy";
        [Header("Enemy Stats")]
        [SerializeField, FormerlySerializedAs("maxHealth")] private int maxHealthValue = 50;
        [SerializeField, FormerlySerializedAs("baseDamage")] private int baseDamageValue = 10;
        [SerializeField, FormerlySerializedAs("category")] private EnemyType categoryValue = EnemyType.None;
        [Header("Attack System")]
        [SerializeField, FormerlySerializedAs("availableAttacks")] private List<AttackData> availableAttacksValue = new List<AttackData>();
        [Header("Drop System")]
        [SerializeField] private List<EnemyDropEntry> dropEntriesValue = new List<EnemyDropEntry>();

        public string Id => stableId;
        public string enemyName => enemyNameValue;
        public int maxHealth => maxHealthValue;
        public int baseDamage => baseDamageValue;
        public EnemyType category => categoryValue;
        public IReadOnlyList<AttackData> availableAttacks => availableAttacksValue;
        public IReadOnlyList<EnemyDropEntry> dropEntries => dropEntriesValue;

        /// <summary>Selects a valid attack without allocating a filtered list.</summary>
        public AttackData GetRandomAttack()
        {
            int validCount = 0;
            foreach (AttackData attack in availableAttacksValue)
                if (attack != null && attack.IsValid()) validCount++;
            if (validCount == 0) return null;
            int selectedValidIndex = UnityEngine.Random.Range(0, validCount);
            foreach (AttackData attack in availableAttacksValue)
            {
                if (attack == null || !attack.IsValid()) continue;
                if (selectedValidIndex-- == 0) return attack;
            }
            return null;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stableId)) stableId = Guid.NewGuid().ToString("N");
            stableId = stableId.Trim();
            enemyNameValue = enemyNameValue?.Trim() ?? string.Empty;
            maxHealthValue = Mathf.Max(1, maxHealthValue);
            baseDamageValue = Mathf.Max(1, baseDamageValue);
            availableAttacksValue ??= new List<AttackData>();
            dropEntriesValue ??= new List<EnemyDropEntry>();
            dropEntriesValue.RemoveAll(entry => entry == null);
        }
    }
}
