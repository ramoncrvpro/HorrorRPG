using System.Linq;
using HorrorRPG.Core;
using HorrorRPG.Presentation;

using UnityEngine;

namespace HorrorRPG.Battle
{
    /// <summary>Starts one battle and persists enemy defeat in the runtime world state.</summary>
    public class BattleTrigger3D : MonoBehaviour, IGameContextReceiver
    {
        [Header("Enemy Renderer")]
        [SerializeField] private SpriteRenderer enemyRenderer;
        [Header("Enemy Data")]
        [SerializeField] private EnemyData enemyData;
        [Header("Dependencies")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private WorldObjectId worldObjectId;
        [Header("Trigger Settings")]
        [SerializeField] private bool disableAfterTrigger = true;

        private GameContext gameContext;
        private Collider triggerCollider;
        private bool hasTriggered;

        private void Awake()
        {
            triggerCollider = GetComponent<Collider>();
            if (worldObjectId == null) worldObjectId = GetComponent<WorldObjectId>();
        }

        /// <summary>Injects session state and observes the local battle coordinator.</summary>
        public void Initialize(GameContext context)
        {
            gameContext = context ?? throw new System.ArgumentNullException(nameof(context));
            if (battleManager == null)
            {
                Debug.LogError($"{nameof(BattleTrigger3D)} requires a {nameof(BattleManager)} reference on {name}.", this);
                return;
            }
            battleManager.BattleEnded += HandleBattleEnded;
            if (worldObjectId != null && gameContext.Session.World.DefeatedEnemies.Contains(worldObjectId.Value)) gameObject.SetActive(false);
        }

        /// <summary>Releases the battle result callback.</summary>
        public void Deinitialize()
        {
            if (battleManager != null) battleManager.BattleEnded -= HandleBattleEnded;
            gameContext = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered || !IsPlayer(other) || battleManager == null) return;
            if (enemyRenderer == null || enemyData == null)
            {
                Debug.LogError($"{nameof(BattleTrigger3D)} has incomplete enemy references on {name}.", this);
                return;
            }
            SpriteRendererAnimator enemyAnimator = enemyRenderer.GetComponent<SpriteRendererAnimator>();
            if (!battleManager.StartBattle(enemyRenderer, enemyAnimator, enemyData)) return;
            hasTriggered = true;
            if (triggerCollider != null) triggerCollider.enabled = false;
        }

        private void HandleBattleEnded(BattleEndReason reason)
        {
            if (reason == BattleEndReason.Won)
            {
                if (worldObjectId != null && gameContext != null) gameContext.Session.World.MarkEnemyDefeated(worldObjectId.Value);
                if (disableAfterTrigger) gameObject.SetActive(false);
                return;
            }
            hasTriggered = false;
            if (triggerCollider != null) triggerCollider.enabled = true;
        }

        private static bool IsPlayer(Collider other)
        {
            return other.CompareTag("Player") || other.GetComponent<CharacterController>() != null;
        }

        private void OnDestroy() => Deinitialize();
    }
}
