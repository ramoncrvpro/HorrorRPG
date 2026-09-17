using System;
using HorrorRPG.Core;
using HorrorRPG.Input;
using UnityEngine;

namespace HorrorRPG.Battle
{
    /// <summary>Resolves one cancelable weapon timing window.</summary>
    public class AttackTimingBar : MonoBehaviour, IGameContextReceiver
    {
        private const float InputDelay = 0.15f;
        private const float MinimumSpeedModifier = 0.1f;
        private const float MaximumSpeedModifier = 2f;

        [SerializeField] private AttackTimingUI timingView;

        private GameInputReader inputReader;
        private WeaponData currentWeapon;
        private Action<AttackResult> completed;
        private float currentPosition;
        private float direction = 1f;
        private float speedModifier = 1f;
        private float inputDelayTimer;
        private bool active;
        private bool subscribed;

        /// <summary>Injects timing input for the active scene.</summary>
        public void Initialize(GameContext context)
        {
            inputReader = context?.InputReader ?? throw new ArgumentNullException(nameof(context));
            Subscribe();
        }

        /// <summary>Cancels timing and releases input callbacks.</summary>
        public void Deinitialize()
        {
            Cancel();
            Unsubscribe();
            inputReader = null;
        }

        /// <summary>Begins one timing request with exactly one completion callback.</summary>
        public void Begin(WeaponData weapon, Action<AttackResult> completion)
        {
            if (weapon == null) throw new ArgumentNullException(nameof(weapon));
            if (completion == null) throw new ArgumentNullException(nameof(completion));
            Cancel();
            currentWeapon = weapon;
            completed = completion;
            currentPosition = 0f;
            direction = 1f;
            inputDelayTimer = InputDelay;
            active = true;
            timingView?.Show();
            timingView?.SetupZones(weapon);
            timingView?.UpdateMarkerPosition(currentPosition);
        }

        /// <summary>Cancels the active request without invoking its callback.</summary>
        public void Cancel()
        {
            active = false;
            currentWeapon = null;
            completed = null;
            speedModifier = 1f;
            inputDelayTimer = 0f;
            timingView?.Hide();
        }

        /// <summary>Sets the speed modifier used by the next active timing request.</summary>
        public void SetSpeedModifier(float modifier)
        {
            speedModifier = Mathf.Clamp(modifier, MinimumSpeedModifier, MaximumSpeedModifier);
        }

        private void OnEnable() => Subscribe();
        private void OnDisable() => Unsubscribe();

        private void Update()
        {
            if (!active || currentWeapon == null) return;
            inputDelayTimer = Mathf.Max(0f, inputDelayTimer - Time.deltaTime);
            currentPosition += direction * currentWeapon.markerSpeed * speedModifier * Time.deltaTime;
            if (currentPosition >= 1f)
            {
                currentPosition = 1f;
                direction = -1f;
            }
            else if (currentPosition <= 0f)
            {
                currentPosition = 0f;
                direction = 1f;
            }
            timingView?.UpdateMarkerPosition(currentPosition);
        }

        private void HandleTimingConfirm()
        {
            if (!active || inputDelayTimer > 0f || currentWeapon == null) return;
            AttackResult result = currentWeapon.EvaluateTimingPosition(currentPosition);
            Action<AttackResult> callback = completed;
            active = false;
            currentWeapon = null;
            completed = null;
            speedModifier = 1f;
            timingView?.Hide();
            callback?.Invoke(result);
        }

        private void Subscribe()
        {
            if (!isActiveAndEnabled || inputReader == null || subscribed) return;
            inputReader.TimingConfirmPerformed += HandleTimingConfirm;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || inputReader == null) return;
            inputReader.TimingConfirmPerformed -= HandleTimingConfirm;
            subscribed = false;
        }
    }
}
