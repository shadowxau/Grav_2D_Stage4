using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : RaycastController
{

    public float speed;

    public Vector3[] localWaypoints;                // init waypoints to travel between
    Vector3[] globalWaypoints;

    int fromWaypointIndex;
    public float percentBetweenWaypoints;
    float nextMoveTime;
    public float waitTime;
    [Range(0, 2)]
    public float easeAmount;

    public bool opensByItem;
    public bool opensByTimer;

    public bool forceOpen;

    public bool doorMove = false;
    public bool triggeredOnce = false;

    public bool isOpen;

    public enum StartingPos { closed, open };

    public StartingPos startingPos;


    RoomController room;
    Player player;
    GameController gameControl;

    float Ease(float x)                                                         // calculate movement easing
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    public override void Start()
    {
        base.Start();

        gameControl = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }


        if (startingPos == StartingPos.closed)
        {
            transform.position = globalWaypoints[1];
            fromWaypointIndex = 1;
        }
        else if (startingPos == StartingPos.open)
        {
            transform.position = globalWaypoints[0];
            fromWaypointIndex = 0;
            isOpen = true;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        CheckRoomState();

        UpdateRaycastOrigins();

        // force door open
        if (!forceOpen)
        {
            Vector3 velocity = CalculateDoorMove();
            transform.Translate(velocity);
        }
        else
        {
            transform.position = globalWaypoints[0];
            fromWaypointIndex = 0;
            isOpen = true;
        }

    }

    void CheckRoomState()
    {
        if (GameObject.FindGameObjectWithTag("Player") == true)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            room = GameObject.FindGameObjectWithTag("RoomController").GetComponent<RoomController>();

            /*
            if (player.itemCount == room.items.Length && opensByItem == true && triggeredOnce == false)
            {
                doorMove = true;
                triggeredOnce = true;       // prevent door from repeatedly opening/closing
                SetIsOpenFlag();
            }
            */

        if (room.items.Count == 0 && triggeredOnce == false)
            {
                doorMove = true;
                triggeredOnce = true;       // prevent door from repeatedly opening/closing
                SetIsOpenFlag();
            }

        }
    }

    Vector3 CalculateDoorMove()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;                                                //stop moving
        }

        if (opensByItem && doorMove)
        {
            fromWaypointIndex %= globalWaypoints.Length;
            int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
            float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
            percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
            percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
            float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);        // apply easing

            Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

            if (percentBetweenWaypoints >= 1)
            {
                doorMove = false;
                percentBetweenWaypoints = 0;
                fromWaypointIndex++;
            }



            return newPos - transform.position;
        }

        if (opensByTimer)
        {
            fromWaypointIndex %= globalWaypoints.Length;
            int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
            float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
            percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
            percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
            float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);        // apply easing

            Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

            if (percentBetweenWaypoints >= 1)
            {
                percentBetweenWaypoints = 0;
                fromWaypointIndex++;

                nextMoveTime = Time.time + waitTime;                                // reset move timer

                SetIsOpenFlag();

            }

            return newPos - transform.position;
        }

        return Vector3.zero;

    }

    void SetIsOpenFlag()
    {
        if (!isOpen)
        {
            isOpen = true;
            print("isOpen = true;");
            gameControl.openedDoors.Add(this.gameObject.name);
        }
        else
        {
            isOpen = false;
            print("isOpen = false;");
            gameControl.openedDoors.Remove(this.gameObject.name);
        }
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
