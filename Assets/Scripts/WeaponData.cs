using HorrorRPG.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorRPG.Battle
{
    public enum EnemyType { None, Demon, Ghost, Zombie }
    public enum AttackResult { Miss, Hit, Critical }

    [CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
    public class WeaponData : ItemData
    {
        [Header("Weapon Stats")]
        [SerializeField, FormerlySerializedAs("baseDamage")] private int baseDamageValue = 10;
        [SerializeField, FormerlySerializedAs("effectiveAgainst")] private EnemyType effectiveAgainstValue = EnemyType.None;
        [SerializeField, FormerlySerializedAs("effectivenessMultiplier")] private float effectivenessMultiplierValue = 2f;
        [SerializeField] private int maxLevelValue = 5;
        [SerializeField] private float damageBonusPerLevelValue = 0.20f;
        [Header("Timing Bar Settings")]
        [SerializeField, FormerlySerializedAs("markerSpeed"), Range(0.5f, 5f)] private float markerSpeedValue = 2f;
        [Header("Hit Zones (values from 0 to 1)")]
        [SerializeField, FormerlySerializedAs("criticalZoneCenter"), Range(0f, 1f)] private float criticalZoneCenterValue = 0.5f;
        [SerializeField, FormerlySerializedAs("criticalZoneWidth"), Range(0.01f, 0.2f)] private float criticalZoneWidthValue = 0.05f;
        [SerializeField, FormerlySerializedAs("hitZoneWidth"), Range(0.1f, 0.5f)] private float hitZoneWidthValue = 0.4f;
        [SerializeField, FormerlySerializedAs("criticalMultiplier"), Range(1f, 3f)] private float criticalMultiplierValue = 1.5f;

        public int baseDamage => baseDamageValue;
        public EnemyType effectiveAgainst => effectiveAgainstValue;
        public float effectivenessMultiplier => effectivenessMultiplierValue;
        public int maxLevel => maxLevelValue;
        public float damageBonusPerLevel => damageBonusPerLevelValue;
        public float markerSpeed => markerSpeedValue;
        public float criticalZoneCenter => criticalZoneCenterValue;
        public float criticalZoneWidth => criticalZoneWidthValue;
        public float hitZoneWidth => hitZoneWidthValue;
        public float criticalMultiplier => criticalMultiplierValue;

        /// <summary>Returns the configured base damage scaled linearly for the requested level.</summary>
        public int GetBaseDamageAtLevel(int level)
        {
            int clampedLevel = Mathf.Clamp(level, 1, maxLevelValue);
            return Mathf.RoundToInt(baseDamageValue * (1f + damageBonusPerLevelValue * (clampedLevel - 1)));
        }

        /// <summary>Returns damage after applying level scaling and target effectiveness.</summary>
        public int GetEffectiveDamage(EnemyType targetType, int level = 1)
        {
            int levelDamage = GetBaseDamageAtLevel(level);
            return targetType == effectiveAgainstValue
                ? Mathf.RoundToInt(levelDamage * effectivenessMultiplierValue)
                : levelDamage;
        }

        /// <summary>Evaluates a normalized marker position against timing zones.</summary>
        public AttackResult EvaluateTimingPosition(float normalizedPosition)
        {
            float position = Mathf.Clamp01(normalizedPosition);
            float criticalMinimum = criticalZoneCenterValue - criticalZoneWidthValue * 0.5f;
            float criticalMaximum = criticalZoneCenterValue + criticalZoneWidthValue * 0.5f;
            if (position >= criticalMinimum && position <= criticalMaximum) return AttackResult.Critical;
            float hitLeftMinimum = Mathf.Max(0f, criticalMinimum - hitZoneWidthValue);
            float hitRightMaximum = Mathf.Min(1f, criticalMaximum + hitZoneWidthValue);
            if ((position >= hitLeftMinimum && position < criticalMinimum) || (position > criticalMaximum && position <= hitRightMaximum))
                return AttackResult.Hit;
            return AttackResult.Miss;
        }

        /// <summary>Returns final damage for timing quality, level and target type.</summary>
        public int GetDamageByResult(AttackResult result, EnemyType targetType, int level = 1)
        {
            int effectiveDamage = GetEffectiveDamage(targetType, level);
            return result switch
            {
                AttackResult.Critical => Mathf.RoundToInt(effectiveDamage * criticalMultiplierValue),
                AttackResult.Hit => effectiveDamage,
                AttackResult.Miss => 0,
                _ => effectiveDamage
            };
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            baseDamageValue = Mathf.Max(1, baseDamageValue);
            effectivenessMultiplierValue = Mathf.Max(0f, effectivenessMultiplierValue);
            maxLevelValue = Mathf.Max(1, maxLevelValue);
            damageBonusPerLevelValue = Mathf.Max(0f, damageBonusPerLevelValue);
            markerSpeedValue = Mathf.Clamp(markerSpeedValue, 0.5f, 5f);
            criticalZoneCenterValue = Mathf.Clamp01(criticalZoneCenterValue);
            float maximumCriticalWidth = Mathf.Max(0.01f, 2f * Mathf.Min(criticalZoneCenterValue, 1f - criticalZoneCenterValue));
            criticalZoneWidthValue = Mathf.Clamp(criticalZoneWidthValue, 0.01f, Mathf.Min(0.2f, maximumCriticalWidth));
            hitZoneWidthValue = Mathf.Clamp(hitZoneWidthValue, 0.1f, 0.5f);
            criticalMultiplierValue = Mathf.Clamp(criticalMultiplierValue, 1f, 3f);
        }
    }
}
