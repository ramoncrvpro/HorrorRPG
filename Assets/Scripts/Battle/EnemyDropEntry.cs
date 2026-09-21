using System;
using HorrorRPG.Inventory;
using UnityEngine;

namespace HorrorRPG.Battle
{
    /// <summary>Defines one independently evaluated enemy reward entry.</summary>
    [Serializable]
    public sealed class EnemyDropEntry
    {
        [SerializeField] private ItemData item;
        [SerializeField, Min(1)] private int minimumQuantity = 1;
        [SerializeField, Min(1)] private int maximumQuantity = 1;
        [SerializeField, Range(0f, 1f)] private float dropChance = 1f;

        public ItemData Item => item;
        public int MinimumQuantity => minimumQuantity;
        public int MaximumQuantity => maximumQuantity;
        public float DropChance => dropChance;

        /// <summary>Returns whether this entry can produce a valid reward.</summary>
        public bool IsValid()
        {
            return item != null
                && minimumQuantity > 0
                && maximumQuantity >= minimumQuantity
                && dropChance >= 0f
                && dropChance <= 1f;
        }
    }
}
