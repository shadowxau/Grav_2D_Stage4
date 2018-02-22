using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController
{
    public LayerMask objectMask;

    public Vector3[] localWaypoints;                // init waypoints to travel between
    Vector3[] globalWaypoints;

    public float speed;
    public bool isCyclic;
    public float waitTime;
    [Range(0,2)]
    public float easeAmount;

    int fromWaypointIndex;
    float percentBetweenWaypoints;
    float nextMoveTime;
   
    List<RiderMove> riderMove;
    Dictionary<Transform, Controller2D> riderDictionary = new Dictionary<Transform, Controller2D>();

    public override void Start()
    {
        base.Start();

        globalWaypoints = new Vector3[localWaypoints.Length];
        for (int i=0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }

    }

    void Update()
    {

        UpdateRaycastOrigins();

        Vector3 velocity = CalculatePlatformMove();

        CalcRiderMove(velocity);

        MoveRider(true);
        transform.Translate(velocity);
        MoveRider(false);

    }

    float Ease(float x)                                                         // calculate movement easing
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x,a) + Mathf.Pow(1-x, a));
    }

    Vector3 CalculatePlatformMove()
    {
        if(Time.time < nextMoveTime)
        {
            return Vector3.zero;                                                //stop moving
        }

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);        // apply easing

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1) {
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

    void MoveRider(bool beforeMovePlatform)
    {
        foreach(RiderMove rider in riderMove)
        {
            if (!riderDictionary.ContainsKey(rider.transform))
            {
                riderDictionary.Add(rider.transform, rider.transform.GetComponent<Controller2D>());
            }
            if (rider.moveBeforePlatform == beforeMovePlatform)
            {
                riderDictionary[rider.transform].Move(rider.velocity, rider.isGrounded);
            }
        }
    }

    void CalcRiderMove(Vector3 velocity)
    {
        HashSet<Transform> movedRiders = new HashSet<Transform>();
        riderMove = new List<RiderMove>();

        float dirX = Mathf.Sign(velocity.x);
        float dirY = Mathf.Sign(velocity.y);

        //Vertically moving platform
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < vertRayCount; i++)
            {
                Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;                              // if moving down start rays from bottom left otherwise if moving up start rays from top left
                rayOrigin += Vector2.right * (vertRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, objectMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedRiders.Contains(hit.transform))
                    {
                        movedRiders.Add(hit.transform);
                        float pushX = (dirY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * dirY;

                        riderMove.Add(new RiderMove(hit.transform, new Vector3(pushX, pushY), dirY == 1, true));
                    }
                }
            }
        }

        // Horizontal moving platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < vertRayCount; i++)
            {
                Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;                          // if moving down start rays from bottom left otherwise if moving up start rays from top left
                rayOrigin += Vector2.up * (horRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, objectMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedRiders.Contains(hit.transform))
                    {
                        movedRiders.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - skinWidth) * dirX;
                        float pushY = -skinWidth;

                        riderMove.Add(new RiderMove(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        // On top of a horizontal or downward moving platform
        if (dirY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2;

            for (int i = 0; i < vertRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (vertRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, objectMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedRiders.Contains(hit.transform))
                    {
                        movedRiders.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        riderMove.Add(new RiderMove(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }
    
    struct RiderMove
    {
        public Transform transform;
        public Vector3 velocity;
        public bool isGrounded;
        public bool moveBeforePlatform;

        public RiderMove(Transform _transform, Vector3 _velocity, bool _isGrounded, bool _moveBeforePlatform)
        {
            transform = _transform;
            velocity = _velocity;
            isGrounded = _isGrounded;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector3 globalWaypointPos = (Application.isPlaying)?globalWaypoints[i] : localWaypoints[i] + transform.position;
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
}
