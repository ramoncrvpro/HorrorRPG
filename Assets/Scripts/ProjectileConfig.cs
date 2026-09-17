using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace HorrorRPG.Battle
{
    [CreateAssetMenu(fileName = "New Projectile Config", menuName = "Battle/Projectile Config")]
    public class ProjectileConfig : ScriptableObject
    {
        [SerializeField] private string stableId;
        [Header("Loop Settings")]
        [SerializeField, FormerlySerializedAs("loopRadius")] private float loopRadiusValue = 2f;
        [SerializeField, FormerlySerializedAs("loopSpeed")] private float loopSpeedValue = 5f;
        [Header("Attack Settings")]
        [SerializeField, FormerlySerializedAs("minLoopTime")] private float minLoopTimeValue = 2f;
        [SerializeField, FormerlySerializedAs("maxLoopTime")] private float maxLoopTimeValue = 5f;
        [SerializeField, FormerlySerializedAs("minTravelSpeed")] private float minTravelSpeedValue = 3f;
        [SerializeField, FormerlySerializedAs("maxTravelSpeed")] private float maxTravelSpeedValue = 8f;
        [Header("Visual")]
        [SerializeField, FormerlySerializedAs("projectileVisual")] private Sprite projectileVisual;

        public string Id => stableId;
        public float loopRadius => loopRadiusValue;
        public float loopSpeed => loopSpeedValue;
        public float minLoopTime => minLoopTimeValue;
        public float maxLoopTime => maxLoopTimeValue;
        public float minTravelSpeed => minTravelSpeedValue;
        public float maxTravelSpeed => maxTravelSpeedValue;
        public Sprite ProjectileVisual => projectileVisual;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stableId)) stableId = Guid.NewGuid().ToString("N");
            stableId = stableId.Trim();
            loopRadiusValue = Mathf.Max(0f, loopRadiusValue);
            loopSpeedValue = Mathf.Max(0f, loopSpeedValue);
            minLoopTimeValue = Mathf.Max(0f, minLoopTimeValue);
            maxLoopTimeValue = Mathf.Max(minLoopTimeValue, maxLoopTimeValue);
            minTravelSpeedValue = Mathf.Max(0.01f, minTravelSpeedValue);
            maxTravelSpeedValue = Mathf.Max(minTravelSpeedValue, maxTravelSpeedValue);
        }
    }
}
