using System;
using System.Collections;
using System.Collections.Generic;
using HorrorRPG.Core;
using HorrorRPG.Input;
using HorrorRPG.Inventory;
using HorrorRPG.Presentation;
using UnityEngine;

namespace HorrorRPG.Battle
{
    /// <summary>Coordinates battle rules with arena, timing, projectile and transition presentation.</summary>
    public class BattleManager : MonoBehaviour, IGameContextReceiver
    {
        [Header("Battle Arena")]
        [SerializeField] private GameObject battleArena;
        [SerializeField] private SpriteRenderer battleEnemyRenderer;
        [SerializeField] private BattleEnemyEffects battleEnemyEffects;
        [SerializeField] private EnemyAnimationController enemyAnimationController;
        [Header("Timing System")]
        [SerializeField] private AttackTimingBar attackTimingBar;
        [SerializeField] private AttackTimingUI attackTimingUI;
        [Header("Battle Systems")]
        [SerializeField] private BattleUIManager battleUIManager;
        [SerializeField] private DefenseManager defenseManager;
        [SerializeField] private ProjectileManager projectileManager;
        [SerializeField] private BattlePlayerEffects battlePlayerEffects;
        [Header("Transition Effects")]
        [SerializeField] private BattleTransitionEffects transitionEffects;
        [Header("Drop Presentation")]
        [SerializeField] private DropView dropView;
        [Header("Enemy Data")]
        [SerializeField] private EnemyData currentEnemyData;
        [Header("Battle Delays")]
        [SerializeField] private float enemyDeathDelay = 1f;

        private GameContext gameContext;
        private BattleService battleService;
        private BattleDropResolver dropResolver;
        private IReadOnlyList<EnemyDropEntry> activeDropEntries;
        private InputContextLease inputLease;
        private ProjectileAttackHandle activeProjectileAttack;
        private Coroutine enemyAttackCoroutine;
        private Coroutine exitCoroutine;
        private bool initialized;
        private bool battlePresentationActive;

        public EnemyData CurrentEnemyData => battleService?.State.Enemy ?? currentEnemyData;
        public int CurrentEnemyHealth => battleService?.State.CurrentEnemyHealth ?? 0;
        public bool IsProcessingTurn => battleService != null && battleService.State.Phase != BattlePhase.PlayerChoice && battleService.State.Phase != BattlePhase.Idle;
        public event Action<BattleEndReason> BattleEnded;

        private void Awake()
        {
            if (defenseManager == null) defenseManager = GetComponent<DefenseManager>();
            if (attackTimingBar == null) attackTimingBar = GetComponent<AttackTimingBar>();
            if (projectileManager == null) projectileManager = GetComponent<ProjectileManager>();
            if (dropView == null)
            {
                DropView[] sceneDropViews = FindObjectsByType<DropView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                if (sceneDropViews.Length > 0) dropView = sceneDropViews[0];
            }
            if (battleArena != null) battleArena.SetActive(false);
        }

        /// <summary>Injects battle services and connects scene-local presenters.</summary>
        public void Initialize(GameContext context)
        {
            if (initialized) return;
            gameContext = context ?? throw new ArgumentNullException(nameof(context));
            battleService = context.Battle;
            dropResolver = new BattleDropResolver();
            battleService.PhaseChanged += HandlePhaseChanged;
            battleService.Ended += HandleBattleEnded;
            if (battleUIManager != null)
            {
                battleUIManager.WeaponSelected += PlayerAttack;
                battleUIManager.ConsumableSelected += PlayerUseItem;
                battleUIManager.RunRequested += RunFromBattle;
            }
            initialized = true;
        }

        /// <summary>Cancels scene-owned executions and releases all battle dependencies.</summary>
        public void Deinitialize()
        {
            if (!initialized) return;
            battleService.PhaseChanged -= HandlePhaseChanged;
            battleService.Ended -= HandleBattleEnded;
            if (battleUIManager != null)
            {
                battleUIManager.WeaponSelected -= PlayerAttack;
                battleUIManager.ConsumableSelected -= PlayerUseItem;
                battleUIManager.RunRequested -= RunFromBattle;
            }
            StopAllCoroutines();
            attackTimingBar?.Cancel();
            projectileManager?.Cancel(activeProjectileAttack);
            defenseManager?.End();
            CleanupPresentation();
            gameContext = null;
            battleService = null;
            dropResolver = null;
            activeDropEntries = null;
            initialized = false;
        }

        /// <summary>Validates visual dependencies and starts a battle once.</summary>
        public bool StartBattle(SpriteRenderer sourceEnemyRenderer, SpriteRendererAnimator sourceEnemyAnimator, EnemyData enemyData, IReadOnlyList<EnemyDropEntry> dropEntries)
        {
            if (!initialized || battlePresentationActive || sourceEnemyRenderer == null || enemyData == null) return false;
            if (battleArena == null || battleEnemyRenderer == null || battleUIManager == null || attackTimingBar == null || defenseManager == null || projectileManager == null)
            {
                Debug.LogError($"{nameof(BattleManager)} has incomplete battle presentation references on {name}.", this);
                return false;
            }
            BattleCommandResult result = battleService.StartBattle(enemyData);
            if (!result.Accepted) return false;
            currentEnemyData = enemyData;
            activeDropEntries = dropEntries;
            StartCoroutine(StartBattleSequence(sourceEnemyRenderer, sourceEnemyAnimator));
            return true;
        }

        /// <summary>Selects a weapon and begins its cancelable timing window.</summary>
        public void PlayerAttack(WeaponData weapon)
        {
            if (battleService == null || !battleService.SelectWeapon(weapon).Accepted) return;
            attackTimingBar.SetSpeedModifier(battleService.State.TimingSpeedModifier);
            attackTimingBar.Begin(weapon, ResolvePlayerAttack);
        }

        /// <summary>Uses one consumable through battle rules.</summary>
        public void PlayerUseItem(ConsumableData consumable)
        {
            if (battleService == null) return;
            BattleCommandResult result = battleService.UseConsumable(consumable);
            if (result.Accepted) battlePlayerEffects?.PlayItemUseEffects();
        }

        /// <summary>Returns whether the battle presentation is active.</summary>
        public bool IsInBattle() => battlePresentationActive;

        /// <summary>Requests a normal battle exit.</summary>
        public void EndBattle()
        {
            if (battleService?.State.Phase == BattlePhase.PlayerChoice) battleService.Run();
        }

        private IEnumerator StartBattleSequence(SpriteRenderer sourceEnemyRenderer, SpriteRendererAnimator sourceEnemyAnimator)
        {
            battlePresentationActive = true;
            battleEnemyRenderer.sprite = sourceEnemyRenderer.sprite;
            battleEnemyRenderer.color = sourceEnemyRenderer.color;
            battleEnemyRenderer.flipX = sourceEnemyRenderer.flipX;
            battleEnemyRenderer.flipY = sourceEnemyRenderer.flipY;
            SpriteRendererAnimator battleEnemyAnimator = battleEnemyRenderer.GetComponent<SpriteRendererAnimator>();
            if (battleEnemyAnimator != null && sourceEnemyAnimator != null && sourceEnemyAnimator.Animation != null)
            {
                battleEnemyAnimator.Play(sourceEnemyAnimator.Animation);
            }
            inputLease = gameContext.Input.Acquire(InputContext.Battle, InputBlockReason.Battle);
            bool transitionComplete = transitionEffects == null;
            transitionEffects?.PlayBattleStartEffects(() => transitionComplete = true);
            yield return new WaitUntil(() => transitionComplete);
            battleArena.SetActive(true);
            enemyAnimationController?.PlayStartAnimation();
            battleUIManager.InitializeBattle();
            battleService.EnterPlayerChoice();
        }

        private void ResolvePlayerAttack(AttackResult result)
        {
            if (result != AttackResult.Miss) battleEnemyEffects?.PlayHitEffects();
            battleService.ResolvePlayerAttack(result);
        }

        private void HandlePhaseChanged(BattlePhase phase)
        {
            if (phase == BattlePhase.EnemyAttack && enemyAttackCoroutine == null)
                enemyAttackCoroutine = StartCoroutine(ProcessEnemyAttack());
        }

        private IEnumerator ProcessEnemyAttack()
        {
            if (!battleService.BeginEnemyAttack().Accepted)
            {
                enemyAttackCoroutine = null;
                yield break;
            }

            defenseManager.Begin(new DefenseRequest(0f));
            EnemyData enemy = battleService.State.Enemy;
            AttackData attack = enemy?.GetRandomAttack();
            ProjectileAttackResult attackResult = null;
            bool completed = false;
            if (attack != null)
            {
                activeProjectileAttack = projectileManager.Execute(
                    attack,
                    enemy.baseDamage,
                    ApplyProjectileResolution,
                    result =>
                    {
                        attackResult = result;
                        completed = true;
                    });
                yield return new WaitUntil(() => completed);
            }
            else
            {
                attackResult = new ProjectileAttackResult(
                    ProjectileAttackStatus.InvalidConfiguration,
                    new[] { new ProjectileResolution(DefensePosition.Up, enemy != null ? enemy.baseDamage : 0, DefenseType.None) });
            }

            if (attackResult.Status == ProjectileAttackStatus.InvalidConfiguration && attackResult.Resolutions.Count == 0)
            {
                ApplyProjectileResolution(new ProjectileResolution(DefensePosition.Up, enemy != null ? enemy.baseDamage : 0, DefenseType.None));
            }
            else if (attack == null)
            {
                foreach (ProjectileResolution resolution in attackResult.Resolutions) ApplyProjectileResolution(resolution);
            }

            defenseManager.End();
            if (battleService.State.Phase == BattlePhase.Defense) battleService.CompleteEnemyAttack();
            enemyAttackCoroutine = null;
        }

        private void RunFromBattle()
        {
            battleService?.Run();
        }

        private void HandleBattleEnded(BattleEndReason reason)
        {
            if (reason == BattleEndReason.SceneChanged)
            {
                CleanupPresentation();
                BattleEnded?.Invoke(reason);
                return;
            }
            if (exitCoroutine == null) exitCoroutine = StartCoroutine(FinishBattleSequence(reason));
        }

        private void ApplyProjectileResolution(ProjectileResolution resolution)
        {
            if (battleService == null || battleService.State.Phase != BattlePhase.Defense) return;
            battleService.ResolveProjectile(new ProjectileHitResult(resolution.Damage, resolution.Position));
            battlePlayerEffects?.PlayDamageEffects(resolution.DefenseType);
        }

        private IEnumerator FinishBattleSequence(BattleEndReason reason)
        {
            attackTimingBar?.Cancel();
            projectileManager?.Cancel(activeProjectileAttack);
            defenseManager?.End();
            if (reason == BattleEndReason.Won)
            {
                enemyAnimationController?.PlayDeadAnimation();
                yield return new WaitForSeconds(enemyDeathDelay);
                IReadOnlyList<InventoryEntry> rewards = dropResolver.ResolveAndGrant(activeDropEntries, gameContext.Inventory);
                activeDropEntries = null;
                if (rewards.Count > 0 && dropView != null)
                {
                    dropView.Open(rewards);
                    yield return new WaitUntil(() => !dropView.IsOpen);
                }
            }

            bool transitionComplete = transitionEffects == null;
            transitionEffects?.PlayBattleEndEffects(() => transitionComplete = true);
            yield return new WaitUntil(() => transitionComplete);
            CleanupPresentation();
            battleService.CompleteExit();
            BattleEnded?.Invoke(reason);
            exitCoroutine = null;
            if (reason == BattleEndReason.Lost) gameContext.SceneFlow.ReloadCurrentScene(SceneReloadReason.PlayerDeath);
        }

        private void CleanupPresentation()
        {
            battlePresentationActive = false;
            if (battleArena != null) battleArena.SetActive(false);
            battleUIManager?.CloseBattleUI();
            dropView?.Close();
            inputLease?.Dispose();
            inputLease = null;
            activeDropEntries = null;
        }

        private void OnValidate()
        {
            enemyDeathDelay = Mathf.Max(0f, enemyDeathDelay);
        }

        private void OnDestroy() => Deinitialize();
    }
}
