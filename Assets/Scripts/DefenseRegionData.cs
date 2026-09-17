using System;
using UnityEngine;

namespace HorrorRPG.Battle
{
    [Serializable]
    public sealed class DefenseRegionData
    {
        private readonly float duration;

        public DefenseRegionData(DefensePosition position, float activeDuration)
        {
            if (activeDuration <= 0f) throw new ArgumentOutOfRangeException(nameof(activeDuration));
            Position = position;
            duration = activeDuration;
        }

        public DefensePosition Position { get; }
        public float ElapsedTime { get; private set; }
        public bool IsActive { get; private set; }
        public float NormalizedTime => Mathf.Clamp01(ElapsedTime / duration);

        /// <summary>Activates this defense region from the beginning of its timing window.</summary>
        public void Activate()
        {
            IsActive = true;
            ElapsedTime = 0f;
        }

        /// <summary>Advances the active timing window.</summary>
        public void UpdateTimer(float deltaTime)
        {
            if (!IsActive) return;
            ElapsedTime += Mathf.Max(0f, deltaTime);
            if (ElapsedTime >= duration) Reset();
        }

        /// <summary>Clears this defense region.</summary>
        public void Reset()
        {
            ElapsedTime = 0f;
            IsActive = false;
        }

        /// <summary>Returns the damage multiplier for the current timing.</summary>
        public float GetDamageMultiplier()
        {
            if (!IsActive) return 1f;
            float normalizedTime = NormalizedTime;
            if (normalizedTime < 0.1f) return 0f;
            if (normalizedTime < 0.2f) return 0.5f;
            if (normalizedTime < 0.6f) return 1f;
            if (normalizedTime < 0.9f) return 1.5f;
            return 1f;
        }
    }
}
