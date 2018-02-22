using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour {

    Enemy enemy;

    public enum MoveType
    {
        MoveBetweenPoints,
        Chase,
        Flee
    }

    public MoveType AIMoveType;

    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    public bool isCyclic;
    public float waitTime;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;


    // Use this for initialization
    void Start () {
        enemy = GetComponent<Enemy>();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }
    }
	
	// Update is called once per frame
	void Update () {
		if (AIMoveType == MoveType.MoveBetweenPoints)
        {
            Vector2 directionalInput = CalculateEnemyMove();
            enemy.SetDirectionalInput(directionalInput);
        }
	}


    Vector3 CalculateEnemyMove()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;                                                //stop moving
        }

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * enemy.moveSpeed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        //float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);        // apply easing

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], 1);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (!isCyclic)                                                      // cycle back and forth through waypoints
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;                                // reset move timer
        }

        return newPos - transform.position;
    }

    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.blue;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
}
