using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorRPG.Inventory
{
    public enum ConsumableEffectType { HealHealth, IncreaseDamage, DecreaseMarkerSpeed }

    [CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
    public class ConsumableData : ItemData
    {
        [Header("Consumable Properties")]
        [SerializeField, FormerlySerializedAs("effectType")] private ConsumableEffectType effectTypeValue = ConsumableEffectType.HealHealth;
        [SerializeField, FormerlySerializedAs("effectValue")] private int effectValueValue;
        [SerializeField, FormerlySerializedAs("effectDuration")] private float effectDurationValue = 1f;

        public ConsumableEffectType effectType => effectTypeValue;
        public int effectValue => effectValueValue;
        public float effectDuration => effectDurationValue;

        /// <summary>Returns a localized description for the configured effect.</summary>
        public string GetEffectDescription()
        {
            return effectTypeValue switch
            {
                ConsumableEffectType.HealHealth => $"Restaura {effectValueValue} pontos de vida",
                ConsumableEffectType.IncreaseDamage => $"Aumenta o dano em {effectValueValue} no próximo ataque",
                ConsumableEffectType.DecreaseMarkerSpeed => $"Reduz a velocidade do marker em {effectValueValue}%",
                _ => "Efeito desconhecido"
            };
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            effectValueValue = Mathf.Max(0, effectValueValue);
            effectDurationValue = Mathf.Max(0f, effectDurationValue);
        }
    }
}
