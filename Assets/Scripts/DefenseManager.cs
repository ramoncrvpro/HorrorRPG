using System;
using System.Collections.Generic;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Player;
using UnityEngine;

namespace HorrorRPG.Battle
{
    public readonly struct DefenseRequest
    {
        public DefenseRequest(float activeDuration)
        {
            ActiveDuration = activeDuration;
        }

        public float ActiveDuration { get; }
    }

    public readonly struct DefenseResolution
    {
        public DefenseResolution(DefensePosition position, int damage, DefenseType type)
        {
            Position = position;
            Damage = Mathf.Max(0, damage);
            Type = type;
        }

        public DefensePosition Position { get; }
        public int Damage { get; }
        public DefenseType Type { get; }
    }

    /// <summary>Resolves defense timing for the active enemy attack.</summary>
    public class DefenseManager : MonoBehaviour, IGameContextReceiver
    {
        private const float DefaultActiveDuration = 1f;

        [SerializeField] private float globalCooldown = 1.5f;
        [SerializeField] private float defenseActiveDuration = DefaultActiveDuration;
        [SerializeField] private DefenseEffectsManager defenseEffects;
        [SerializeField] private HandAnimationManager handAnimationManager;

        private readonly Dictionary<DefensePosition, DefenseRegionData> defenseRegions = new Dictionary<DefensePosition, DefenseRegionData>();
        private GameInputReader inputReader;
        private float globalCooldownTimer;
        private bool defenseEnabled;
        private bool subscribed;

        public bool IsDefenseEnabled => defenseEnabled;

        /// <summary>Injects typed defense input for this scene.</summary>
        public void Initialize(GameContext context)
        {
            inputReader = context?.InputReader ?? throw new ArgumentNullException(nameof(context));
            Subscribe();
        }

        /// <summary>Ends defense and releases input callbacks.</summary>
        public void Deinitialize()
        {
            End();
            Unsubscribe();
            inputReader = null;
        }

        /// <summary>Begins a defense request and resets all three regions.</summary>
        public void Begin(DefenseRequest request)
        {
            float duration = request.ActiveDuration > 0f ? request.ActiveDuration : defenseActiveDuration;
            defenseRegions.Clear();
            defenseRegions.Add(DefensePosition.Left, new DefenseRegionData(DefensePosition.Left, duration));
            defenseRegions.Add(DefensePosition.Up, new DefenseRegionData(DefensePosition.Up, duration));
            defenseRegions.Add(DefensePosition.Right, new DefenseRegionData(DefensePosition.Right, duration));
            globalCooldownTimer = 0f;
            defenseEnabled = true;
        }

        /// <summary>Resolves projectile damage against a defense region and consumes that region.</summary>
        public DefenseResolution Resolve(DefensePosition position, int damage)
        {
            if (!defenseRegions.TryGetValue(position, out DefenseRegionData region) || !region.IsActive)
                return new DefenseResolution(position, damage, DefenseType.None);

            float multiplier = region.GetDamageMultiplier();
            int finalDamage = Mathf.CeilToInt(Mathf.Max(0, damage) * multiplier);
            DefenseType type = multiplier <= 0f ? DefenseType.Perfect : multiplier < 1f ? DefenseType.Partial : DefenseType.None;
            region.Reset();
            return new DefenseResolution(position, finalDamage, type);
        }

        /// <summary>Ends defense and clears every active timing region and effect.</summary>
        public void End()
        {
            defenseEnabled = false;
            foreach (DefenseRegionData region in defenseRegions.Values) region.Reset();
            defenseRegions.Clear();
            globalCooldownTimer = 0f;
            defenseEffects?.HideAllEffects();
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            if (!defenseEnabled) return;
            globalCooldownTimer = Mathf.Max(0f, globalCooldownTimer - Time.deltaTime);
            foreach (DefenseRegionData region in defenseRegions.Values) region.UpdateTimer(Time.deltaTime);
        }

        private void TryActivateDefense(DefensePosition position)
        {
            if (!defenseEnabled || globalCooldownTimer > 0f) return;
            if (!defenseRegions.TryGetValue(position, out DefenseRegionData region) || region.IsActive) return;
            region.Activate();
            globalCooldownTimer = globalCooldown;
            defenseEffects?.TriggerEffect(position);
            if (handAnimationManager == null) return;
            switch (position)
            {
                case DefensePosition.Left:
                    handAnimationManager.PlayReachAnimationLeftHand();
                    break;
                case DefensePosition.Up:
                    handAnimationManager.PlayReachAnimation();
                    break;
                case DefensePosition.Right:
                    handAnimationManager.PlayReachAnimationRightHand();
                    break;
            }
        }

        private void Subscribe()
        {
            if (!isActiveAndEnabled || inputReader == null || subscribed) return;
            inputReader.DefendLeftPerformed += HandleLeftDefense;
            inputReader.DefendUpPerformed += HandleUpDefense;
            inputReader.DefendRightPerformed += HandleRightDefense;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || inputReader == null) return;
            inputReader.DefendLeftPerformed -= HandleLeftDefense;
            inputReader.DefendUpPerformed -= HandleUpDefense;
            inputReader.DefendRightPerformed -= HandleRightDefense;
            subscribed = false;
        }

        private void HandleLeftDefense() => TryActivateDefense(DefensePosition.Left);
        private void HandleUpDefense() => TryActivateDefense(DefensePosition.Up);
        private void HandleRightDefense() => TryActivateDefense(DefensePosition.Right);

        private void OnValidate()
        {
            globalCooldown = Mathf.Max(0f, globalCooldown);
            defenseActiveDuration = Mathf.Max(0.01f, defenseActiveDuration);
        }
    }
}
