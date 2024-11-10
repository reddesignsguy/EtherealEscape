using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EndBehavior : ushort
{
    TeleportToStart,
    MoveToStart,
    Stop
}

public class WaypointMovement : MonoBehaviour
{
    [Header("Waypoint Settings")]
    public GameObject waypointsHolder;        // Array of waypoint positions
    public float moveSpeed = 5f;         // Movement speed
    public float waypointRadius = 0.1f;  // How close to get to waypoint before moving to next
    public EndBehavior endBehavior;         // Whether to loop back to first waypoint
    
    [Header("Optional Settings")]
    public bool startOnAwake = true;     // Start moving immediately
    public float waitTime = 0f;          // Time to wait at each waypoint
    
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitCounter = 0f;
    private bool isMoving = false;
    private Transform[] waypoints;

    private void Start()
    {
        if (startOnAwake)
        {
            isMoving = true;
        }
        waypoints = waypointsHolder.GetComponentsInChildren<Transform>();
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("No waypoints assigned to WaypointMovement script!");
            return;
        }
    }

    private void Update()
    {
        if (!isMoving || waypoints.Length == 0) return;

        if (isWaiting)
        {
            waitCounter += Time.deltaTime;
            if (waitCounter >= waitTime)
            {
                isWaiting = false;
                waitCounter = 0f;
            }
            return;
        }

        // Get current waypoint
        Transform currentWaypoint = waypoints[currentWaypointIndex];

        // Move towards waypoint
        transform.position = Vector3.MoveTowards(
            transform.position, 
            currentWaypoint.position, 
            moveSpeed * Time.deltaTime
        );

        // Check if we've reached the waypoint
        if (Vector3.Distance(transform.position, currentWaypoint.position) < waypointRadius)
        {
            // Move to next waypoint
            currentWaypointIndex++;

            // Check if we've completed the path
            if (currentWaypointIndex >= waypoints.Length)
            {
                switch (endBehavior)
                {
                    case EndBehavior.MoveToStart:
                        currentWaypointIndex = 0;
                        break;
                    case EndBehavior.TeleportToStart:
                        currentWaypointIndex = 0;
                        transform.position = waypoints[0].position;
                        break;
                    default:
                        isMoving = false;
                        break;
                }
            }

            // Wait at waypoint if wait time is set
            if (waitTime > 0)
            {
                isWaiting = true;
            }
        }
    }

    // Public methods to control movement
    public void StartMoving()
    {
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    public void ResetToStart()
    {
        currentWaypointIndex = 0;
        transform.position = waypoints[0].position;
        isWaiting = false;
        waitCounter = 0f;
    }

    // Optional: Draw waypoint path in editor for debugging
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Gizmos.color = Color.blue;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                Gizmos.DrawWireSphere(waypoints[i].position, waypointRadius);
                
                if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
                else if (endBehavior == EndBehavior.MoveToStart && waypoints[0] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                }
            }
        }
    }
}