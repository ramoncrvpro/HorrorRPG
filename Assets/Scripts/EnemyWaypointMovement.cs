namespace HorrorRPG.Battle
{



using UnityEngine;

public class EnemyWaypointMovement : MonoBehaviour
{
    public enum PathMode
    {
        Loop,
        PingPong
    }

    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waypointReachDistance = 0.1f;

    [Header("Path Settings")]
    [SerializeField] private PathMode pathMode = PathMode.Loop;
    [SerializeField] private float waitTimeAtWaypoint = 1f;

    [Header("Gizmos Settings")]
    [SerializeField] private Color pathColor = Color.yellow;
    [SerializeField] private Color waypointColor = Color.red;
    [SerializeField] private float waypointGizmoRadius = 0.3f;

    private int currentWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool movingForward = true;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                MoveToNextWaypoint();
            }
            return;
        }

        MoveTowardsCurrentWaypoint();
    }

    private void MoveTowardsCurrentWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if (targetWaypoint == null)
            return;

        Vector3 targetPosition = targetWaypoint.position;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.position = newPosition;

        float distanceToWaypoint = Vector3.Distance(transform.position, targetPosition);
        if (distanceToWaypoint <= waypointReachDistance)
        {
            OnWaypointReached();
        }
    }

    private void OnWaypointReached()
    {
        isWaiting = true;
        waitTimer = waitTimeAtWaypoint;
    }

    private void MoveToNextWaypoint()
    {
        if (pathMode == PathMode.Loop)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        else if (pathMode == PathMode.PingPong)
        {
            if (movingForward)
            {
                currentWaypointIndex++;
                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = waypoints.Length - 2;
                    movingForward = false;
                    
                    if (currentWaypointIndex < 0)
                        currentWaypointIndex = 0;
                }
            }
            else
            {
                currentWaypointIndex--;
                if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    movingForward = true;
                    
                    if (currentWaypointIndex >= waypoints.Length)
                        currentWaypointIndex = waypoints.Length - 1;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        Gizmos.color = waypointColor;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawWireSphere(waypoint.position, waypointGizmoRadius);
            }
        }

        Gizmos.color = pathColor;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }

        if (pathMode == PathMode.Loop && waypoints.Length > 1)
        {
            if (waypoints[waypoints.Length - 1] != null && waypoints[0] != null)
            {
                Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
            }
        }
    }
}


}
