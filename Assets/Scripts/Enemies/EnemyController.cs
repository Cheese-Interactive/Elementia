using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyController : EntityController {

    [Header("Pathfinding")]
    [SerializeField] private List<Waypoint> waypoints;
    [SerializeField] private float waypointLeniency;
    private Waypoint currWaypoint; // last waypoint reached (not waypoint moving towards)
    private Coroutine pathfindingCoroutine;

    protected void Start() {

        currWaypoint = waypoints[0]; // set current waypoint to first waypoint
        transform.position = currWaypoint.transform.position; // set starting position to first waypoint

        if (waypoints.Count > 1) // if there are waypoints to move to
            pathfindingCoroutine = StartCoroutine(HandlePathfinding());

    }

    private IEnumerator HandlePathfinding() {

        int waypointIndex = waypoints.FindIndex(waypoint => waypoint == currWaypoint) + 1; // find index of next waypoint

        if (waypointIndex >= waypoints.Count) // if index is out of bounds, reset to 0
            waypointIndex = 0;

        anim.SetBool("isMoving", true);

        while (true) {

            CheckFlip();

            Waypoint nextWaypoint = waypoints[waypointIndex];
            float distance = Vector2.Distance(currWaypoint.transform.position, nextWaypoint.transform.position);
            float duration = nextWaypoint.GetTravelDuration();

            if (transform.position.x < nextWaypoint.GetStartPosition().x)
                rb.velocity = new Vector2(distance / duration, 0f); // move to the right
            else
                rb.velocity = new Vector2(-(distance / duration), 0f); // moves to the left

            if (Vector2.Distance(transform.position, nextWaypoint.transform.position) < waypointLeniency) { // check if waypoint has been reached

                // stop enemy and wait
                rb.velocity = Vector2.zero;
                anim.SetBool("isMoving", false);
                yield return new WaitForSeconds(duration);

                anim.SetBool("isMoving", true);

                // update waypoint
                currWaypoint = nextWaypoint;
                waypointIndex++;

                if (waypointIndex >= waypoints.Count) // if index is out of bounds, reset to 0
                    waypointIndex = 0;

            }

            yield return null;

        }
    }

    private void CheckFlip() {

        if (isFacingRight && rb.velocity.x < 0f || !isFacingRight && rb.velocity.x > 0f) {

            transform.Rotate(0f, 180f, 0f);
            isFacingRight = !isFacingRight;

        }
    }

    private void OnDrawGizmos() {

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Count; i++) {

            Gizmos.DrawWireSphere(waypoints[i].transform.position, 0.25f);

            if (i + 1 < waypoints.Count)
                Gizmos.DrawLine(waypoints[i].transform.position, waypoints[i + 1].transform.position);
            else
                Gizmos.DrawLine(waypoints[i].transform.position, waypoints[0].transform.position);

        }
    }
}
