using UnityEngine;
using System.Collections;


public class Controller2D : RaycastController {

    public float maxSlopeAngle = 80;
    
    public CollisionInfo collisions;
    [HideInInspector]
    public Vector2 moveInput;

    public enum GravDir
    {
        Down,
        Up,
        Left,
        Right
    }

    public GravDir myGrav;

    public override void Start()
    {
        base.Start();
        collisions.facingDir = 1;
        collisions.hitDir = 1;
    }

    public void Move (Vector2 velocity, bool isGrounded)
    {
        Move(velocity, Vector2.zero, isGrounded);
    }

    public void Move(Vector2 velocity, Vector2 input, bool isGrounded = false)
    {
        UpdateRaycastOrigins();
        collisions.ResetCollisionInfo();
        collisions.velocityOld = velocity;
        moveInput = input;

        if (myGrav == GravDir.Down)
        {
            if (velocity.y < 0)
            {
                SlideSlope(ref velocity);
            }

            if (velocity.x != 0)
            {
                collisions.facingDir = (int)Mathf.Sign(velocity.x);
            }

            // check for horizontal collisions
            HorizontalCollisionsGravDown(ref velocity);

            // Check for vertical collisions
            if (velocity.y != 0)
            {
                VerticalCollisionsGravDown(ref velocity);
            }

            //Move the object
            transform.Translate(velocity);

            if (isGrounded)
            {
                collisions.below = true;
            }
        }
        else if (myGrav == GravDir.Up)
        {
            if (velocity.y > 0)
            {
                SlideSlope(ref velocity);
            }

            if (velocity.x != 0)
            {
                collisions.facingDir = (int)Mathf.Sign(velocity.x);
            }

            // check for horizontal collisions
            HorizontalCollisionsGravUp(ref velocity);

            // Check for vertical collisions
            if (velocity.y != 0)
            {
                VerticalCollisionsGravUp(ref velocity);
            }

            //Move the object
            transform.Translate(velocity);

            if (isGrounded)
            {
                collisions.above = true;
            }
        }
        else if (myGrav == GravDir.Left)
        {
            if (velocity.x < 0)
            {
                SlideSlope(ref velocity);
            }

            if (velocity.y != 0)
            {
                collisions.facingDir = (int)Mathf.Sign(velocity.y);
            }

            // check for horizontal collisions
            HorizontalCollisionsGravLeft(ref velocity);

            // Check for vertical collisions
            if (velocity.x != 0)
            {
                VerticalCollisionsGravLeft(ref velocity);
            }

            //Move the object
            transform.Translate(velocity);

            if (isGrounded)
            {
                collisions.left = true;
            }
        }
    }

    // Horizontal collision checking for gravity falling down
    void HorizontalCollisionsGravDown(ref Vector2 velocity)
    {
        float dirX = collisions.facingDir;                                                                                    // if moving down = -1, if moving up = +1
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        if (Mathf.Abs(velocity.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        // Check front rays
        #region check front ray
        for (int i = 0; i < horRayCount; i++)
        {
            Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;                          // if moving down start rays from bottom left otherwise if moving up start rays from top left
            rayOrigin += Vector2.up * (horRaySpacing * i);
            Debug.DrawRay(rayOrigin, Vector2.right * dirX * rayLength, Color.red);

            #region enemyHit
            RaycastHit2D enemyHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, enemyCollisionMask);
            if (enemyHit)
            {
                CheckObjectCollisions(enemyHit);
            }
            
            #endregion

            #region triggerHit
            RaycastHit2D triggerHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, triggerCollisionMask);
            if (triggerHit)
            {
                Debug.Log("front triggerHit = " + triggerHit.collider.name);
                CheckObjectCollisions(triggerHit);
            }
            #endregion

            #region solidHit
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, solidCollisionMask);
            if (solidHit)
            {
                if (solidHit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(solidHit.normal, Vector2.up);                                                       // find the angle of the slope

                // where slope angle is less than defined maximum
                if (i == 0 && slopeAngle <= maxSlopeAngle)                                                                      // move character onto slope instead of hovering above
                {
                    // allow player to climb slope
                    if (collisions.descendingSlope)                                                                             // prevent change of speed when moving between two high angle slopes
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }

                    float distanceToSlope = 0;

                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlope = solidHit.distance - skinWidth;
                        velocity.x -= distanceToSlope * dirX;
                    }

                    ClimbSlope(ref velocity, slopeAngle, solidHit.normal);
                    velocity.x += distanceToSlope * dirX;
                }

                // when trying to climb a slope angled higher than the defined maximum
                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    velocity.x = (solidHit.distance - skinWidth) * dirX;
                    rayLength = solidHit.distance;                                                                                   // reduce ray length if hit

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);                  // prevent jitter when hitting side of character on slope
                    }

                    collisions.left = dirX == -1;                                                                               // if hit and going left then collisions.left is true
                    collisions.right = dirX == 1;
                }              
            }
            #endregion

        }
        #endregion

        // Check Back rays
        #region Check Back Ray 
        for (int i = 0; i < horRayCount; i++)
        {
            Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;                          // if moving down start rays from bottom left otherwise if moving up start rays from top left
            rayOrigin += Vector2.up * (horRaySpacing * i);
            Debug.DrawRay(rayOrigin, Vector2.right * dirX * rayLength, Color.red);

            #region enemyHit
            RaycastHit2D enemyHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, enemyCollisionMask);
            if (enemyHit)
            {
                CheckObjectCollisions(enemyHit);
            }
            #endregion

            #region triggerHit
            RaycastHit2D triggerHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, triggerCollisionMask);
            if (triggerHit)
            {
                Debug.Log(" back triggerHit = " + triggerHit.collider.name);
                CheckObjectCollisions(triggerHit);
            }
            #endregion

            #region solidHit
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, solidCollisionMask);
            if (solidHit)
            {
                if (solidHit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(solidHit.normal, Vector2.up);                                                       // find the angle of the slope

                if (i == 0 && slopeAngle <= maxSlopeAngle)                                                                      // move character onto slope instead of hovering above
                {
                    if (collisions.descendingSlope)                                                                             // prevent change of speed when moving between two high angle slopes
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
                    float distanceToSlope = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlope = solidHit.distance - skinWidth;
                        velocity.x -= distanceToSlope * dirX;
                    }
                    ClimbSlope(ref velocity, slopeAngle, solidHit.normal);
                    velocity.x += distanceToSlope * dirX;
                }

                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    velocity.x = (solidHit.distance - skinWidth) * dirX;
                    rayLength = solidHit.distance;                                                                                   // reduce ray length if hit

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);                  // prevent jitter when hitting side of character on slope
                    }

                    collisions.left = dirX == -1;                                                                               // if hit and going left then collisions.left is true
                    collisions.right = dirX == 1;
                }                
            }
            #endregion

        }
        #endregion
    }

    // Horizontal collision checking for gravity falling left
    void HorizontalCollisionsGravLeft(ref Vector2 velocity)
    {
        float dirY = collisions.facingDir;                                                                                    // if moving down = -1, if moving up = +1
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        if (Mathf.Abs(velocity.y) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        // Check front rays
        #region check front ray
        for (int i = 0; i < horRayCount; i++)
        {
            Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;                          // if moving down start rays from bottom left otherwise if moving up start rays from top left
            rayOrigin += Vector2.right * (vertRaySpacing * i);
            Debug.DrawRay(rayOrigin, Vector2.up * dirY * rayLength, Color.red);

            #region enemyHit
            RaycastHit2D enemyHit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, enemyCollisionMask);
            if (enemyHit)
            {
                CheckObjectCollisions(enemyHit);
            }

            #endregion

            #region triggerHit
            RaycastHit2D triggerHit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, triggerCollisionMask);
            if (triggerHit)
            {
                Debug.Log("front triggerHit = " + triggerHit.collider.name);
                CheckObjectCollisions(triggerHit);
            }
            #endregion

            #region solidHit
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, solidCollisionMask);
            if (solidHit)
            {
                if (solidHit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(solidHit.normal, Vector2.right);                                                       // find the angle of the slope

                // where slope angle is less than defined maximum
                if (i == 0 && slopeAngle <= maxSlopeAngle)                                                                      // move character onto slope instead of hovering above
                {
                    // allow player to climb slope
                    if (collisions.descendingSlope)                                                                             // prevent change of speed when moving between two high angle slopes
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }

                    float distanceToSlope = 0;

                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlope = solidHit.distance - skinWidth;
                        velocity.y -= distanceToSlope * dirY;
                    }

                    ClimbSlope(ref velocity, slopeAngle, solidHit.normal);
                    velocity.y += distanceToSlope * dirY;
                }

                // when trying to climb a slope angled higher than the defined maximum
                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    velocity.y = (solidHit.distance - skinWidth) * dirY;
                    rayLength = solidHit.distance;                                                                                   // reduce ray length if hit

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.y);                  // prevent jitter when hitting side of character on slope
                    }

                    collisions.below = dirY == -1;                                                                               // if hit and going left then collisions.above is true
                    collisions.above = dirY == 1;
                }
            }
            #endregion

        }
        #endregion

        // Check Back rays
        #region Check Back Ray 
        for (int i = 0; i < horRayCount; i++)
        {
            Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;                          // if moving down start rays from bottom left otherwise if moving up start rays from top left
            rayOrigin += Vector2.right * (vertRaySpacing * i);
            Debug.DrawRay(rayOrigin, Vector2.up * dirY * rayLength, Color.red);

            #region enemyHit
            RaycastHit2D enemyHit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, enemyCollisionMask);
            if (enemyHit)
            {
                CheckObjectCollisions(enemyHit);
            }
            #endregion

            #region triggerHit
            RaycastHit2D triggerHit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, triggerCollisionMask);
            if (triggerHit)
            {
                Debug.Log(" back triggerHit = " + triggerHit.collider.name);
                CheckObjectCollisions(triggerHit);
            }
            #endregion

            #region solidHit
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, solidCollisionMask);
            if (solidHit)
            {
                if (solidHit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(solidHit.normal, Vector2.right);                                                       // find the angle of the slope

                if (i == 0 && slopeAngle <= maxSlopeAngle)                                                                      // move character onto slope instead of hovering above
                {
                    if (collisions.descendingSlope)                                                                             // prevent change of speed when moving between two high angle slopes
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
                    float distanceToSlope = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlope = solidHit.distance - skinWidth;
                        velocity.y -= distanceToSlope * dirY;
                    }
                    ClimbSlope(ref velocity, slopeAngle, solidHit.normal);
                    velocity.y += distanceToSlope * dirY;
                }

                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    velocity.y = (solidHit.distance - skinWidth) * dirY;
                    rayLength = solidHit.distance;                                                                                   // reduce ray length if hit

                    if (collisions.climbingSlope)
                    {
                        velocity.x = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.y);                  // prevent jitter when hitting side of character on slope
                    }

                    collisions.below = dirY == -1;                                                                               // if hit and going left then collisions.left is true
                    collisions.above = dirY == 1;
                }
            }
            #endregion

        }
        #endregion
    }

    // Horizontal collision checking for gravity falling up
    void HorizontalCollisionsGravUp(ref Vector2 velocity)
    {
        float dirX = collisions.facingDir;                                                                                    // if moving down = -1, if moving up = +1
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        if (Mathf.Abs(velocity.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        // Check front rays
        #region check front ray
        for (int i = 0; i < horRayCount; i++)
        {
            Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.topLeft : raycastOrigins.topRight;                          // if moving left start rays from top left otherwise if moving right start rays from top right
            rayOrigin += Vector2.down * (horRaySpacing * i);
            Debug.DrawRay(rayOrigin, Vector2.right * dirX * rayLength, Color.green);

            #region enemyHit
            RaycastHit2D enemyHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, enemyCollisionMask);
            if (enemyHit)
            {
                CheckObjectCollisions(enemyHit);
            }

            #endregion

            #region triggerHit
            RaycastHit2D triggerHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, triggerCollisionMask);
            if (triggerHit)
            {
                Debug.Log("front triggerHit = " + triggerHit.collider.name);
                CheckObjectCollisions(triggerHit);
            }
            #endregion

            #region solidHit
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, solidCollisionMask);

            if (solidHit)
            {
                if (solidHit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(solidHit.normal, -Vector2.up);                                                       // find the angle of the slope

                // where slope angle is less than defined maximum (using grav up value)
                if (i == 0 && slopeAngle <= maxSlopeAngle)                                                                      // move character onto slope instead of hovering above
                {

                    Debug.Log("if (i == 0 && slopeAngle <= maxSlopeAngle) ");

                    // allow player to climb slope
                    if (collisions.descendingSlope)                                                                             // prevent change of speed when moving between two high angle slopes
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }

                    float distanceToSlope = 0;

                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlope = solidHit.distance - skinWidth;
                        velocity.x -= distanceToSlope * dirX;
                    }

                    ClimbSlope(ref velocity, slopeAngle, solidHit.normal);
                    velocity.x += distanceToSlope * dirX;
                }

                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    velocity.x = (solidHit.distance - skinWidth) * dirX;
                    rayLength = solidHit.distance;                                                                                   // reduce ray length if hit

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);                  // prevent jitter when hitting side of character on slope
                    }

                    collisions.left = dirX == -1;                                                                               // if hit and going left then collisions.left is true
                    collisions.right = dirX == 1;
                }               
            }
            #endregion

        }
        #endregion

        // Check Back rays
        #region Check Back Ray 
        for (int i = 0; i < horRayCount; i++)
        {
            Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.topRight : raycastOrigins.topLeft;                          // if moving left start rays from top right otherwise if moving right start rays from top left
            rayOrigin += Vector2.down * (horRaySpacing * i);
            Debug.DrawRay(rayOrigin, Vector2.right * dirX * rayLength, Color.green);

            #region enemyHit
            RaycastHit2D enemyHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, enemyCollisionMask);
            if (enemyHit)
            {
                CheckObjectCollisions(enemyHit);
            }
            #endregion

            #region triggerHit
            RaycastHit2D triggerHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, triggerCollisionMask);
            if (triggerHit)
            {
                Debug.Log(" back triggerHit = " + triggerHit.collider.name);
                CheckObjectCollisions(triggerHit);
            }
            #endregion

            #region solidHit
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, solidCollisionMask);
            if (solidHit)
            {
                if (solidHit.distance == 0)
                {
                    continue;
                }

                float slopeAngle = Vector2.Angle(solidHit.normal, -Vector2.up);                                                       // find the angle of the slope

                if (i == 0 && slopeAngle >= maxSlopeAngle)                                                                      // move character onto slope instead of hovering above
                {
                    if (collisions.descendingSlope)                                                                             // prevent change of speed when moving between two high angle slopes
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
                    float distanceToSlope = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlope = solidHit.distance - skinWidth;
                        velocity.x -= distanceToSlope * dirX;
                    }
                    ClimbSlope(ref velocity, slopeAngle, solidHit.normal);
                    velocity.x += distanceToSlope * dirX;
                }

                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    velocity.x = (solidHit.distance - skinWidth) * dirX;
                    rayLength = solidHit.distance;                                                                                   // reduce ray length if hit

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);                  // prevent jitter when hitting side of character on slope
                    }

                    collisions.left = dirX == -1;                                                                               // if hit and going left then collisions.left is true
                    collisions.right = dirX == 1;
                }                
            }
            #endregion

        }
        #endregion
    }

    // Vertical collision checking for gravity falling down
    void VerticalCollisionsGravDown(ref Vector2 velocity)
    {
        float dirY = Mathf.Sign(velocity.y);                                                                                    // if moving down = -1, if moving up = +1
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < vertRayCount; i++)
        {
            Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;                              // if moving down start rays from bottom left otherwise if moving up start rays from top left
            rayOrigin += Vector2.right * (vertRaySpacing * i + velocity.x);

            //DEBUG - Show Rays
            Debug.DrawRay(rayOrigin, Vector2.up * dirY * rayLength, Color.red);

            #region solidHit
            // Check if object collided with "Solid" collision mask object
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, solidCollisionMask);

            // check against collidables
            if (solidHit)
            {
                // check if object has hit a collision object with Through tag
                if (solidHit.collider.tag == "Through")
                {
                    if (dirY == 1 || solidHit.distance == 0)
                    {
                        continue;
                    }
                    if (collisions.dropDown)
                    {
                        continue;
                    }
                    if (moveInput.y == -1)
                    {
                        collisions.dropDown = true;
                        Invoke("ResetDropDown", .1f);
                        continue;
                    }
                }

                velocity.y = (solidHit.distance - skinWidth) * dirY;
                rayLength = solidHit.distance;                                                                                           // reduce ray length if hit

                if (collisions.climbingSlope)
                {
                velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);            // prevent jitter when hitting top of character on slope
                }

                collisions.below = dirY == -1;                                                                                      // if hit and going down then collisions.below is true
                collisions.above = dirY == 1;
            }
            #endregion
        }

        if (collisions.climbingSlope)                                                                                               // prevent player moving inside slope where two slopes meet
        {
            float dirX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((dirX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;      // if moving down start rays from bottom left otherwise if moving up start rays from top left

            #region solidHit
            // Check if object collided with "Solid" collision mask object
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, solidCollisionMask);
            if (solidHit)
            {
                float slopeAngle = Vector2.Angle(solidHit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (solidHit.distance - skinWidth) * dirX;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = solidHit.normal;
                }
            }
            #endregion
        }
    }

    // Vertical collision checking for gravity falling down
    void VerticalCollisionsGravLeft(ref Vector2 velocity)
    {
        float dirX = Mathf.Sign(velocity.x);                                                                                    // if moving down = -1, if moving up = +1
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < vertRayCount; i++)
        {
            Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.topLeft : raycastOrigins.topRight;                              // if moving down start rays from bottom left otherwise if moving up start rays from top left
            rayOrigin += Vector2.down * (horRaySpacing * i + velocity.y);

            //DEBUG - Show Rays
            Debug.DrawRay(rayOrigin, Vector2.right * dirX * rayLength, Color.red);

            #region solidHit
            // Check if object collided with "Solid" collision mask object
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, solidCollisionMask);

            // check against collidables
            if (solidHit)
            {
                // check if object has hit a collision object with Through tag
                if (solidHit.collider.tag == "Through")
                {
                    if (dirX == 1 || solidHit.distance == 0)
                    {
                        continue;
                    }
                    if (collisions.dropDown)
                    {
                        continue;
                    }
                    if (moveInput.x == -1)
                    {
                        collisions.dropDown = true;
                        Invoke("ResetDropDown", .1f);
                        continue;
                    }
                }

                velocity.x = (solidHit.distance - skinWidth) * dirX;
                rayLength = solidHit.distance;                                                                                           // reduce ray length if hit

                if (collisions.climbingSlope)
                {
                    velocity.y = velocity.x / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.y);            // prevent jitter when hitting top of character on slope
                }

                collisions.left = dirX == -1;                                                                                      // if hit and going down then collisions.below is true
                collisions.right = dirX == 1;
            }
            #endregion
        }

        if (collisions.climbingSlope)                                                                                               // prevent player moving inside slope where two slopes meet
        {
            float dirY = Mathf.Sign(velocity.y);
            rayLength = Mathf.Abs(velocity.y) + skinWidth;
            Vector2 rayOrigin = ((dirY == -1) ? raycastOrigins.topLeft : raycastOrigins.bottomLeft) + Vector2.right * velocity.x;      // if moving down start rays from bottom left otherwise if moving up start rays from top left

            #region solidHit
            // Check if object collided with "Solid" collision mask object
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.down * dirY, rayLength, solidCollisionMask);
            if (solidHit)
            {
                float slopeAngle = Vector2.Angle(solidHit.normal, Vector2.right);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.y = (solidHit.distance - skinWidth) * dirY;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = solidHit.normal;
                }
            }
            #endregion
        }
    }

    // Vertical collision checking for gravity falling up
    void VerticalCollisionsGravUp(ref Vector2 velocity)
    {
        float dirY = Mathf.Sign(velocity.y);                                                                                    // if moving down = -1, if moving up = +1
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < vertRayCount; i++)
        {
            Vector2 rayOrigin = (dirY == 1) ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;                              // if moving up start rays from top left otherwise if moving down start rays from bottom left
            rayOrigin += Vector2.right * (vertRaySpacing * i + velocity.x);

            //DEBUG - Show Rays
            Debug.DrawRay(rayOrigin, Vector2.up * dirY * rayLength, Color.green);

            #region solidHit
            // Check if object collided with "Solid" collision mask object
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.up * dirY, rayLength, solidCollisionMask);

            // check against collidables
            if (solidHit)
            {
                // check if object has hit a collision object with Through tag
                if (solidHit.collider.tag == "Through")
                {
                    if (dirY == 1 || solidHit.distance == 0)
                    {
                        continue;
                    }
                    if (collisions.dropDown)
                    {
                        continue;
                    }
                    if (moveInput.y == -1)
                    {
                        collisions.dropDown = true;
                        Invoke("ResetDropDown", .1f);
                        continue;
                    }
                }

                velocity.y = (solidHit.distance - skinWidth) * dirY;
                rayLength = solidHit.distance;                                                                                           // reduce ray length if hit

                if (collisions.climbingSlope)
                {
                    ///////////////////
                    /// Do I need to do something here for reversed grav?????
                    ///////////////////
                    velocity.x = velocity.y / -Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);            // prevent jitter when hitting top of character on slope
                }

                collisions.below = dirY == -1;                                                                                      // if hit and going down then collisions.below is true
                collisions.above = dirY == 1;
            }
            #endregion
        }

        if (collisions.climbingSlope)                                                                                               // prevent player moving inside slope where two slopes meet
        {
            float dirX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((dirX == -1) ? raycastOrigins.topLeft : raycastOrigins.topRight) + Vector2.up * velocity.y;      // if moving left start rays from top left otherwise if moving right start rays from top right

            #region solidHit
            // Check if object collided with "Solid" collision mask object
            RaycastHit2D solidHit = Physics2D.Raycast(rayOrigin, Vector2.right * dirX, rayLength, solidCollisionMask);

            //DEBUG - Show Rays
            Debug.DrawRay(rayOrigin, Vector2.right * dirX * rayLength, Color.black);

            if (solidHit)
            {
                float slopeAngle = Vector2.Angle(solidHit.normal, Vector2.down);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (solidHit.distance - skinWidth) * dirX;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = solidHit.normal;
                }
            }
            #endregion
        }
    }

    bool CheckObjectCollisions(RaycastHit2D hit)
    {
        // check if object has hit an collision object with Item tag
        if (hit.collider.gameObject.tag == "Item")
        {
            Debug.Log(gameObject.ToString() + " hit Item object: " + hit.collider.gameObject.ToString());

            // testing (refreshing stored items in rooms)
            RoomController room = GameObject.FindGameObjectWithTag("RoomController").GetComponent<RoomController>();
            room.items.Remove(hit.collider.gameObject);

            //room.CollectItemsUpdate(hit.collider.name);
            GameController gameControl = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            gameControl.CollectItemsUpdate(hit.collider.name);

            collisions.touchItem = true;
            Destroy(hit.collider.gameObject);
            return true;
        }

        // check if object has hit a collision object with Hazard tag
        else if (hit.collider.gameObject.tag == "Hazard")
        {
            Debug.Log(gameObject.ToString() + " hit Hazard object: " + hit.collider.gameObject.ToString());
            collisions.touchHazard = true;
            return true;
        }

        // check if object has hit a collision object with Checkpoint tag
        else if (hit.collider.gameObject.tag == "Checkpoint")
        {
            Debug.Log(gameObject.ToString() + " hit CheckPoint object: " + hit.collider.gameObject.ToString());
            // store the trigger object collided with so that we can check attached scripts (used to change scenes)
            collisions.collidedWith = hit.collider.gameObject;
            collisions.touchCheckpoint = true;
            return true;
        }

        // check if object has hit a collision object with the NPC tag
        else if (hit.collider.gameObject.tag == "NPC")
        {
            Debug.Log(gameObject.ToString() + " hit NPC object: " + hit.collider.gameObject.ToString());
            collisions.collidedWith = hit.collider.gameObject;
            collisions.touchNPC = true;
            return true;
        }

        // check if this object has hit a collision object with the PlayerAttack tag
        else if (hit.collider.gameObject.tag == "PlayerAttack")
        {
            Debug.Log(gameObject.ToString() + " hit PlayerAttack object: " + hit.collider.gameObject.ToString());
            collisions.collidedWith = hit.collider.gameObject;
            collisions.touchPlayerAttack = true;
            collisions.hitDir = hit.collider.GetComponent<HitCollider>().hitBoxDir;
            return true;
        }

        // check if this object has hit a collision object with the Attack tag
        else if (hit.collider.gameObject.tag == "Attack")
        {
            Debug.Log(gameObject.ToString() + " hit Attack object: " + hit.collider.gameObject.ToString());
            collisions.collidedWith = hit.collider.gameObject;
            collisions.touchPlayerAttack = true;
            collisions.hitDir = hit.collider.GetComponent<HitCollider>().hitBoxDir;
            return true;
        }

        // check if this object has collided with an object with EnemyAttack tag
        else if (hit.collider.gameObject.tag == "EnemyAttack")
        {
            Debug.Log(gameObject.ToString() + " hit EnemyAttack object: " + hit.collider.gameObject.ToString());
            collisions.collidedWith = hit.collider.gameObject;
            collisions.touchEnemyAttack = true;
            collisions.hitDir = hit.collider.GetComponent<HitCollider>().hitBoxDir;
            return true;
        }

        // check if this object has collided with an object with Enemy tag
        else if (hit.collider.gameObject.tag == "Enemy")
        {
            if (hit.collider.gameObject != gameObject)
            {
                Debug.Log(gameObject.ToString() + " hit Enemy object: " + hit.collider.gameObject.ToString());
                collisions.collidedWith = hit.collider.gameObject;
                collisions.touchEnemy = true;
                collisions.hitDir = hit.collider.GetComponent<Enemy>().enemyController.collisions.facingDir;
                return true;
            }
        }

        return false;
    }

    void ClimbSlope(ref Vector2 velocity, float slopeAngle, Vector2 slopeNormal)                                                                         // climb slope
    {
        float moveDistance = Mathf.Abs(velocity.x);
        //float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (myGrav == GravDir.Down)
        {
            float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (velocity.y <= climbVelocityY)
            {
                velocity.y = climbVelocityY;
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                collisions.below = true;
                collisions.climbingSlope = true;
                collisions.slopeAngle = slopeAngle;
                collisions.slopeNormal = slopeNormal;
            }
        }
        else if (myGrav == GravDir.Up)
        {
            float climbVelocityY = -Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (velocity.y >= climbVelocityY)
            {
                velocity.y = climbVelocityY;
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                collisions.above = true;
                collisions.climbingSlope = true;
                collisions.slopeAngle = slopeAngle;
                collisions.slopeNormal = slopeNormal;
            }
        }
        else if (myGrav == GravDir.Left)
        {
            float climbVelocityX = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (velocity.x <= climbVelocityX)
            {
                velocity.x = climbVelocityX;
                velocity.y = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.y);
                collisions.left = true;
                collisions.climbingSlope = true;
                collisions.slopeAngle = slopeAngle;
                collisions.slopeNormal = slopeNormal;
            }
        }
    }

    void SlideSlope(ref Vector2 velocity)
    {
        if (myGrav == GravDir.Down)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, rayLength, solidCollisionMask);
            RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, rayLength, solidCollisionMask);

            // Debugging
            Debug.DrawRay(raycastOrigins.bottomLeft, Vector2.down * rayLength, Color.yellow);
            Debug.DrawRay(raycastOrigins.bottomRight, Vector2.down * rayLength, Color.yellow);

            if (maxSlopeHitLeft ^ maxSlopeHitRight)
            {
                SlideDownMaxSlope(maxSlopeHitLeft, ref velocity);
                SlideDownMaxSlope(maxSlopeHitRight, ref velocity);
            }

            if (!collisions.slidingDownMaxSlope)
            {
                float dirX = Mathf.Sign(velocity.x);
                Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, solidCollisionMask);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                    {
                        if (Mathf.Sign(hit.normal.x) == dirX)
                        {
                            if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                            {
                                float moveDistance = Mathf.Abs(velocity.x);
                                float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                                velocity.y -= descendVelocityY;

                                collisions.slopeAngle = slopeAngle;
                                collisions.descendingSlope = true;
                                collisions.below = true;
                            }
                        }
                    }
                }
            }
        }
        else if (myGrav == GravDir.Up)
        {
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.topLeft, Vector2.up, rayLength, solidCollisionMask);
            RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.topRight, Vector2.up, rayLength, solidCollisionMask);

            //Debugging
            Debug.DrawRay(raycastOrigins.topLeft, Vector2.up * rayLength, Color.yellow);
            Debug.DrawRay(raycastOrigins.topRight, Vector2.up * rayLength, Color.yellow);

            if (maxSlopeHitLeft ^ maxSlopeHitRight)
            {
                SlideDownMaxSlope(maxSlopeHitLeft, ref velocity);
                SlideDownMaxSlope(maxSlopeHitRight, ref velocity);
            }

            if (!collisions.slidingDownMaxSlope)
            {
                float dirX = Mathf.Sign(velocity.x);
                Vector2 rayOrigin = (dirX == -1) ? raycastOrigins.topRight : raycastOrigins.topLeft;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, Mathf.Infinity, solidCollisionMask);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.down);
                    if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                    {
                        if (Mathf.Sign(hit.normal.x) == dirX)
                        {
                            if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                            {
                                float moveDistance = Mathf.Abs(velocity.x);
                                float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                                velocity.y += descendVelocityY;

                                collisions.slopeAngle = slopeAngle;
                                collisions.descendingSlope = true;
                                collisions.above = true;
                            }
                        }
                    }
                }
            }
        }
        else if (myGrav == GravDir.Left)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.topLeft, Vector2.left, rayLength, solidCollisionMask);
            RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.left, rayLength, solidCollisionMask);

            // Debugging
            Debug.DrawRay(raycastOrigins.topLeft, Vector2.left * rayLength, Color.yellow);
            Debug.DrawRay(raycastOrigins.bottomLeft, Vector2.left * rayLength, Color.yellow);

            if (maxSlopeHitLeft ^ maxSlopeHitRight)
            {
                SlideDownMaxSlope(maxSlopeHitLeft, ref velocity);
                SlideDownMaxSlope(maxSlopeHitRight, ref velocity);
            }

            if (!collisions.slidingDownMaxSlope)
            {
                float dirY = Mathf.Sign(velocity.y);
                Vector2 rayOrigin = (dirY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.left, Mathf.Infinity, solidCollisionMask);

                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.right);
                    if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                    {
                        if (Mathf.Sign(hit.normal.y) == dirY)
                        {
                            if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.y))
                            {
                                float moveDistance = Mathf.Abs(velocity.y);
                                float descendVelocityX = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                                velocity.y = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.y);
                                velocity.x -= descendVelocityX;

                                collisions.slopeAngle = slopeAngle;
                                collisions.descendingSlope = true;
                                collisions.left = true;
                            }
                        }
                    }
                }
            }
        }

    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        
        if (hit)
        {
            if(myGrav == GravDir.Down)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                    collisions.slopeAngle = slopeAngle;
                    collisions.slidingDownMaxSlope = true;
                    collisions.slopeNormal = hit.normal;
                }
            }
            else if (myGrav == GravDir.Up)
            {
                float slopeAngle = Vector2.Angle(hit.normal, -Vector2.up);
                if (slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                    collisions.slopeAngle = slopeAngle;
                    collisions.slidingDownMaxSlope = true;
                    collisions.slopeNormal = hit.normal;
                }
            }
            else if (myGrav == GravDir.Left)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.right);
                if (slopeAngle > maxSlopeAngle)
                {
                    moveAmount.y = Mathf.Sign(hit.normal.y) * (Mathf.Abs(moveAmount.x) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                    collisions.slopeAngle = slopeAngle;
                    collisions.slidingDownMaxSlope = true;
                    collisions.slopeNormal = hit.normal;
                }
            }

        }
    }

    void ResetDropDown()
    {
        collisions.dropDown = false;
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;
        public Vector3 velocityOld;
        public int facingDir;
        public int hitDir;
        public bool dropDown;

        public bool touchItem;
        public bool touchHazard;
        public bool touchCheckpoint;
        public bool touchNPC;
        public bool touchEnemy;
        public bool touchPlayerAttack;
        public bool touchEnemyAttack;

        public GameObject collidedWith;

        public void ResetCollisionInfo()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;
            slopeNormal = Vector2.zero;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;

            touchItem = false;
            touchHazard = false;
            touchCheckpoint = false;
            touchNPC = false;
            touchEnemy = false;
            touchEnemyAttack = false;
            touchPlayerAttack = false;

            collidedWith = null;
        }
    }

}
