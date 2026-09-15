namespace HorrorRPG.Battle
{
using HorrorRPG.Presentation;
using HorrorRPG.Inventory;
using HorrorRPG.Battle;
using HorrorRPG.Dialogue;
using HorrorRPG.Core;
using HorrorRPG.Input;


using UnityEngine;
using System.Collections;

public enum ProjectileState
{
    Looping,
    Traveling,
    Hit
}

public enum DefensePosition
{
    Left = 0,
    Up = 1,
    Right = 2
}

public class BattleProjectile : MonoBehaviour
{
    [SerializeField] private MeshRenderer projectileMeshRenderer;
    
    private ProjectileConfig config;
    private ProjectileState currentState;
    private DefensePosition targetPosition;
    private Transform targetTransform;
    private Vector3 loopCenter;
    private float loopAngle;
    private float loopTimer;
    private float loopDuration;
    private int damageAmount;
    private float initialDistanceToTarget;

    public ProjectileState CurrentState => currentState;
    public DefensePosition TargetPosition => targetPosition;
    public int DamageAmount => damageAmount;

    private void Awake()
    {
        if (projectileMeshRenderer == null)
        {
            projectileMeshRenderer = GetComponent<MeshRenderer>();
        }
    }

    public void Initialize(ProjectileConfig projectileConfig, Vector3 center, int damage)
    {
        config = projectileConfig;
        loopCenter = center;
        damageAmount = damage;
        currentState = ProjectileState.Looping;
        loopAngle = Random.Range(0f, 360f);
        loopTimer = 0f;
        loopDuration = Random.Range(config.minLoopTime, config.maxLoopTime);
        
        Vector3 initialOffset = new Vector3(
            Mathf.Cos(loopAngle) * config.loopRadius,
            Mathf.Sin(loopAngle) * config.loopRadius,
            0f
        );
        transform.position = loopCenter + initialOffset;
        
        if (projectileMeshRenderer != null && config.projectileMaterial != null)
        {
            projectileMeshRenderer.material = config.projectileMaterial;
        }
    }

    public void StartTravelToTarget(Transform target, DefensePosition position)
    {
        targetTransform = target;
        targetPosition = position;
        currentState = ProjectileState.Traveling;
        initialDistanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
    }

    public void HitTarget()
    {
        currentState = ProjectileState.Hit;
        StartCoroutine(DestroyAfterDelay(0.2f));
    }

    private void Update()
    {
        switch (currentState)
        {
            case ProjectileState.Looping:
                UpdateLooping();
                break;
            case ProjectileState.Traveling:
                UpdateTraveling();
                break;
        }
    }

    private void UpdateLooping()
    {
        loopTimer += Time.deltaTime;
        loopAngle += config.loopSpeed * Time.deltaTime;
        
        Vector3 offset = new Vector3(
            Mathf.Cos(loopAngle) * config.loopRadius,
            Mathf.Sin(loopAngle) * config.loopRadius,
            0f
        );
        
        transform.position = loopCenter + offset;
        
        if (loopTimer >= loopDuration)
        {
            ProjectileManager.Instance.OnProjectileReadyToAttack(this);
        }
    }

    private void UpdateTraveling()
    {
        if (targetTransform == null)
        {
            ReturnToPool();
            return;
        }
        
        float currentDistance = Vector3.Distance(transform.position, targetTransform.position);
        float proximityFactor = 1f - (currentDistance / initialDistanceToTarget);
        proximityFactor = Mathf.Clamp01(proximityFactor);
        
        float currentSpeed = Mathf.Lerp(config.minTravelSpeed, config.maxTravelSpeed, proximityFactor);
        
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetTransform.position,
            currentSpeed * Time.deltaTime
        );
        
        if (currentDistance < 0.1f)
        {
            ProjectileManager.Instance.OnProjectileReachedTarget(this);
        }
    }

    public float GetDistanceToTarget()
    {
        if (targetTransform == null)
            return float.MaxValue;
        
        return Vector3.Distance(transform.position, targetTransform.position);
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (ProjectileManager.Instance != null)
        {
            ProjectileManager.Instance.ReturnProjectileToPool(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}


}
