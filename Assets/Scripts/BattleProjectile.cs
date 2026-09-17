using System;
using System.Collections;
using UnityEngine;

namespace HorrorRPG.Battle
{
    public enum ProjectileState { Inactive, Looping, Ready, Traveling, Hit }
    public enum DefensePosition { Left, Up, Right }

    public readonly struct ProjectileExecutionContext
    {
        public ProjectileExecutionContext(
            ProjectileConfig config,
            Vector3 loopCenter,
            int damage,
            Action<BattleProjectile> readyToAttack,
            Action<BattleProjectile> reachedTarget,
            Action<BattleProjectile> returnRequested)
        {
            Config = config != null ? config : throw new ArgumentNullException(nameof(config));
            LoopCenter = loopCenter;
            Damage = Mathf.Max(0, damage);
            ReadyToAttack = readyToAttack ?? throw new ArgumentNullException(nameof(readyToAttack));
            ReachedTarget = reachedTarget ?? throw new ArgumentNullException(nameof(reachedTarget));
            ReturnRequested = returnRequested ?? throw new ArgumentNullException(nameof(returnRequested));
        }

        public ProjectileConfig Config { get; }
        public Vector3 LoopCenter { get; }
        public int Damage { get; }
        public Action<BattleProjectile> ReadyToAttack { get; }
        public Action<BattleProjectile> ReachedTarget { get; }
        public Action<BattleProjectile> ReturnRequested { get; }
    }

    /// <summary>Executes movement for one pooled projectile using its owning execution context.</summary>
    public class BattleProjectile : MonoBehaviour
    {
        private const float HitDistance = 0.1f;
        private const float ReturnDelay = 0.2f;

        [SerializeField] private SpriteRenderer projectileSpriteRenderer;

        private ProjectileExecutionContext executionContext;
        private Transform targetTransform;
        private float loopAngle;
        private float loopTimer;
        private float loopDuration;
        private float initialDistanceToTarget;
        private bool initialized;

        public ProjectileState CurrentState { get; private set; } = ProjectileState.Inactive;
        public DefensePosition TargetPosition { get; private set; }
        public int DamageAmount => executionContext.Damage;

        private void Awake()
        {
            if (projectileSpriteRenderer == null) projectileSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>Initializes this pooled projectile for one owner execution.</summary>
        public void Initialize(ProjectileExecutionContext context)
        {
            StopAllCoroutines();
            executionContext = context;
            initialized = true;
            targetTransform = null;
            CurrentState = ProjectileState.Looping;
            loopAngle = UnityEngine.Random.Range(0f, 360f);
            loopTimer = 0f;
            loopDuration = UnityEngine.Random.Range(context.Config.minLoopTime, context.Config.maxLoopTime);
            Vector3 initialOffset = new Vector3(Mathf.Cos(loopAngle) * context.Config.loopRadius, Mathf.Sin(loopAngle) * context.Config.loopRadius, 0f);
            transform.position = context.LoopCenter + initialOffset;
            if (projectileSpriteRenderer != null && context.Config.ProjectileVisual != null)
                projectileSpriteRenderer.sprite = context.Config.ProjectileVisual;
        }

        /// <summary>Starts travel toward the defense target selected by the owner.</summary>
        public void BeginTravel(Transform target, DefensePosition position)
        {
            if (!initialized || target == null) throw new InvalidOperationException("Projectile requires an initialized context and target.");
            targetTransform = target;
            TargetPosition = position;
            initialDistanceToTarget = Mathf.Max(HitDistance, Vector3.Distance(transform.position, target.position));
            CurrentState = ProjectileState.Traveling;
        }

        /// <summary>Marks a resolved hit and schedules return to the pool.</summary>
        public void HitTarget()
        {
            if (!initialized) return;
            CurrentState = ProjectileState.Hit;
            StartCoroutine(ReturnAfterDelay());
        }

        /// <summary>Returns this projectile through the owner that initialized it.</summary>
        public void ReturnToPool()
        {
            if (!initialized) return;
            StopAllCoroutines();
            initialized = false;
            CurrentState = ProjectileState.Inactive;
            targetTransform = null;
            executionContext.ReturnRequested(this);
        }

        private void Update()
        {
            if (!initialized) return;
            if (CurrentState == ProjectileState.Looping) UpdateLooping();
            else if (CurrentState == ProjectileState.Traveling) UpdateTraveling();
        }

        private void UpdateLooping()
        {
            ProjectileConfig config = executionContext.Config;
            loopTimer += Time.deltaTime;
            loopAngle += config.loopSpeed * Time.deltaTime;
            Vector3 offset = new Vector3(Mathf.Cos(loopAngle) * config.loopRadius, Mathf.Sin(loopAngle) * config.loopRadius, 0f);
            transform.position = executionContext.LoopCenter + offset;
            if (loopTimer < loopDuration) return;
            CurrentState = ProjectileState.Ready;
            executionContext.ReadyToAttack(this);
        }

        private void UpdateTraveling()
        {
            if (targetTransform == null)
            {
                executionContext.ReachedTarget(this);
                return;
            }
            float currentDistance = Vector3.Distance(transform.position, targetTransform.position);
            float proximity = Mathf.Clamp01(1f - currentDistance / initialDistanceToTarget);
            float speed = Mathf.Lerp(executionContext.Config.minTravelSpeed, executionContext.Config.maxTravelSpeed, proximity);
            transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, speed * Time.deltaTime);
            if (currentDistance <= HitDistance) executionContext.ReachedTarget(this);
        }

        private IEnumerator ReturnAfterDelay()
        {
            yield return new WaitForSeconds(ReturnDelay);
            ReturnToPool();
        }
    }
}
