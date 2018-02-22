using UnityEngine;
using Com.LuisPedroFonseca.ProCamera2D;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

    #region Variables

    public enum PlayerNumber
    {   One, 
        Two, 
        Three, 
        Four
    }
    public PlayerNumber playerNumber;   

    [SerializeField]
    private Controller2D playerController;

    Camera playerCamera;

    const float increment = .001f;

    [SerializeField][RangeAttribute(increment, 10f)]
    private float maxJumpHeight = 3;
    [SerializeField][RangeAttribute(increment, 10f)]
    private float minJumpHeight = 0.5f;
    [SerializeField][RangeAttribute(increment, 3f)]
    private float jumpSpeed = .4f;
    [SerializeField][RangeAttribute(increment, 1f)]
    private float accelerationSpeedAir = .2f;
    [SerializeField][RangeAttribute(increment, 2f)]
    private float accelerationSpeedGround = .1f;
    [SerializeField][RangeAttribute(increment, 25f)]
    private float moveSpeed = 6;

    Vector3 viewPosition;

    private Vector2 wallJumpUp;
    [SerializeField][RangeAttribute(increment, 25f)]
    private float wallJumpUpX;
    [SerializeField][RangeAttribute(increment, 25f)]
    private float wallJumpUpY;

    private Vector2 wallJumpDown;
    [SerializeField][RangeAttribute(increment, 25f)]
    private float wallJumpDownX;
    [SerializeField][RangeAttribute(increment, 25f)]
    private float wallJumpDownY;
    
    private Vector2 wallJumpAway;
    [SerializeField][RangeAttribute(increment, 25f)]
    private float wallJumpAwayX;
    [SerializeField][RangeAttribute(increment, 25f)]
    private float wallJumpAwayY;

    [SerializeField][RangeAttribute(increment, 10f)]
    private float wallSlideSpeedMax = 3;
    [SerializeField][RangeAttribute(increment, 1f)]
    private float wallStickTime = .10f;

    private Vector2 knockBack;
    [SerializeField]
    [Range(increment, 20f)]
    private float knockBackX;
    [SerializeField]
    [Range(increment, 20f)]
    private float knockBackY;

    float timeToWallUnstick;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;

    public Vector3 velocity { get; set; }

    float velocityXSmoothing;
    float velocityYSmoothing;

    public Vector2 directionalInput { get; set; }
    public bool wallSliding { get; set; }
    public int wallDirX { get; set; }

    bool canDoubleJump;
    int numberOfJumps = 0;

    int respawnTimer;
    bool respawnTimerActive;

    bool playerCanMove;

    public bool playerIsDead { get; set; }
    public bool playerJumping { get; set; }
    public bool playerAttacking { get; set; }

    float attackTimer;

    [SerializeField][Range(increment, 3f)]
    private float defaultAttackTimer;

    ProCamera2DNumericBoundaries cameraBounds;

    float vertExtent; 
    float horzExtent;
    float cameraRightPos;
    float cameraLeftPos;
    float cameraTopPos; 
    float cameraBottomPos;

    public GameObject playerAttackHitBox;

    [SerializeField][Range(increment, 10f)]
    private float attackLocationX = 2.0f;

    public Vector2 attackHitBoxPos { get; set; }
    bool playerIsHit = false;
    float playerHitTimer;

    [SerializeField]
    [Range(increment, 3f)]
    private float defaultHitTimer;

    #endregion

    void Start()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        cameraBounds = playerCamera.GetComponent<ProCamera2DNumericBoundaries>();

        ProCamera2D.Instance.AddCameraTarget(transform, 1f, 1f, 0f);

        wallJumpUp = new Vector2(wallJumpUpX, wallJumpUpY);
        wallJumpDown = new Vector2(wallJumpDownX, wallJumpDownY);
        wallJumpAway = new Vector2(wallJumpAwayX, wallJumpAwayY);

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(jumpSpeed, 2);                  // calculate gravity
        playerController.myGrav = Controller2D.GravDir.Down;
        maxJumpVelocity = Mathf.Abs(gravity) * jumpSpeed;                          // calculate jumpVelocity
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);       // calculate jumpVelocity

        playerCanMove = true;
        playerIsDead = false;
        playerJumping = false;
        respawnTimerActive = false;
        respawnTimer = 60;

        knockBack = new Vector2(knockBackX, knockBackY);
    }

    void FixedUpdate()
    {
        RecalculateMovement();
        RecalculateAttackHitBox();

        CalculateVelocity();
        HandleWallSliding();

        playerController.Move(velocity * Time.deltaTime, directionalInput);

        GameController.gameControl.prevPlayerDirX = playerController.collisions.facingDir;


        if (playerController.myGrav == Controller2D.GravDir.Down)
        {
            if (playerController.collisions.above || playerController.collisions.below)
            {
                if (playerController.collisions.below) playerJumping = false;

                if (playerController.collisions.slidingDownMaxSlope)
                {
                    Vector3 v = velocity;
                    v.y += playerController.collisions.slopeNormal.y * -gravity * Time.deltaTime;
                    velocity = v;
                }
                else
                {
                    Vector3 v = velocity;
                    v.y = 0;
                    velocity = v;
                }
            }

            if (!playerController.collisions.below && numberOfJumps == 0)
            {
                playerJumping = true;
                canDoubleJump = true;
            }
        }
        else if (playerController.myGrav == Controller2D.GravDir.Up)
        {
            if (playerController.collisions.above || playerController.collisions.below)
            {
                if (playerController.collisions.above) playerJumping = false;

                if (playerController.collisions.slidingDownMaxSlope)
                {
                    Vector3 v = velocity;
                    v.y -= playerController.collisions.slopeNormal.y * -gravity * Time.deltaTime;
                    velocity = v;
                }
                else
                {
                    Vector3 v = velocity;
                    v.y = 0;
                    velocity = v;
                }
            }

            if (!playerController.collisions.below && numberOfJumps == 0)
            {
                playerJumping = true;
                canDoubleJump = true;
            }
        }
        else if (playerController.myGrav == Controller2D.GravDir.Left)
        {
            if (playerController.collisions.left || playerController.collisions.right)
            {
                if (playerController.collisions.left) playerJumping = false;

                if (playerController.collisions.slidingDownMaxSlope)
                {
                    Vector3 v = velocity;
                    v.x += playerController.collisions.slopeNormal.x * -gravity * Time.deltaTime;
                    velocity = v;
                }
                else
                {
                    Vector3 v = velocity;
                    v.x = 0;
                    velocity = v;
                }
            }

            if (!playerController.collisions.left && numberOfJumps == 0)
            {
                playerJumping = true;
                canDoubleJump = true;
            }
        }       

        if (!playerController.collisions.below && !playerController.collisions.above && !playerController.collisions.left && !playerController.collisions.right)
        {
            playerJumping = true;
        }

        CheckTouchItem();
        CheckTouchHazard();
        CheckForDamageCollision();
        CheckTouchCheckpoint();
        CheckTouchNPC();
        CheckRoomPos();
        CheckRespawn();
        CheckTimers();
    }

    //=====================================================================================================
    #region Public methods
    
    // update directional input to move player
    public void SetDirectionalInput(Vector2 input)
    {
        if (playerCanMove)
        {
            directionalInput = input;
        }
    }

    // find and return the facing direction
    public int GetFacingDir()
    {
        int direction = playerController.collisions.facingDir;

        return (direction);
    }

    // initiate attack routine on button press
    public void OnAttackInputDown()
    {
        // do attack
        if (playerCanMove)
        {
            if (!playerAttacking)
            {
                playerAttacking = true;
                Debug.Log("playerAttacking = true");

                // Create an invisible collider in front of the player to check for collision with another interactable object
                GameObject newHitBox = Instantiate(playerAttackHitBox, new Vector3(attackHitBoxPos.x, attackHitBoxPos.y), transform.rotation);
                HitCollider hitBox = newHitBox.GetComponent<HitCollider>();

                hitBox.hitBoxOwner = gameObject;
                hitBox.hitBoxDir = playerController.collisions.facingDir;

                attackTimer = defaultAttackTimer;
            }
        }
    }

    // initiate or action jump routine on button press
    public void OnJumpInputDown()
    {
        if (playerCanMove)
        {
            playerJumping = true;

            if (canDoubleJump)
            {
                Vector3 v = velocity;

                if (playerController.myGrav == Controller2D.GravDir.Down)
                {
                    v.y = maxJumpVelocity;
                }
                else if (playerController.myGrav == Controller2D.GravDir.Up)
                {
                    v.y = -maxJumpVelocity;
                }
                else if (playerController.myGrav == Controller2D.GravDir.Left)
                {
                    v.x = maxJumpVelocity;
                }
                else if (playerController.myGrav == Controller2D.GravDir.Right)
                {
                    v.x = -maxJumpVelocity;
                }

                velocity = v;
                canDoubleJump = false;
                numberOfJumps++;
            }

            if (wallSliding)
            {
                if (wallDirX == directionalInput.x)
                {
                    Vector3 v = velocity;
                    v.x = -wallDirX * wallJumpUp.x;

                    if (playerController.myGrav == Controller2D.GravDir.Down)
                    {
                        v.y = wallJumpUp.y;
                    }
                    else if (playerController.myGrav == Controller2D.GravDir.Up)
                    {
                        v.y = -wallJumpUp.y;
                    }

                    velocity = v;
                    canDoubleJump = true;
                }
                else if (directionalInput.x == 0)
                {
                    Vector3 v = velocity;
                    v.x = -wallDirX * wallJumpDown.x;

                    if (playerController.myGrav == Controller2D.GravDir.Down)
                    {
                        v.y = wallJumpDown.y;
                    }
                    else if (playerController.myGrav == Controller2D.GravDir.Up)
                    {
                        v.y = -wallJumpDown.y;
                    }

                    velocity = v;
                    canDoubleJump = true;
                }
                else
                {
                    Vector3 v = velocity;
                    v.x = -wallDirX * wallJumpAway.x;

                    if (playerController.myGrav == Controller2D.GravDir.Down)
                    {
                        v.y = wallJumpAway.y;
                    }
                    else if (playerController.myGrav == Controller2D.GravDir.Up)
                    {
                        v.y = -wallJumpAway.y;
                    }

                    velocity = v;
                    canDoubleJump = true;
                }
            }

            if (playerController.myGrav == Controller2D.GravDir.Down)
            {
                if (playerController.collisions.below)
                {
                    numberOfJumps = 0;

                    if (playerController.collisions.slidingDownMaxSlope)
                    {
                        if (directionalInput.x != -Mathf.Sign(playerController.collisions.slopeNormal.x))
                        { // not jumping against max slope
                            Vector3 v = velocity;
                            v.y = maxJumpVelocity * playerController.collisions.slopeNormal.y;
                            v.x = maxJumpVelocity * playerController.collisions.slopeNormal.x;
                            velocity = v;
                        }
                    }
                    else
                    {
                        Vector3 v = velocity;
                        v.y = maxJumpVelocity;
                        velocity = v;

                        canDoubleJump = true;
                    }
                }
            }
            else if (playerController.myGrav == Controller2D.GravDir.Up)
            {
                if (playerController.collisions.above)
                {
                    numberOfJumps = 0;

                    if (playerController.collisions.slidingDownMaxSlope)
                    {
                        if (directionalInput.x != -Mathf.Sign(playerController.collisions.slopeNormal.x))
                        { // not jumping against max slope
                            Vector3 v = velocity;
                            v.y = -maxJumpVelocity * playerController.collisions.slopeNormal.y;
                            v.x = -maxJumpVelocity * playerController.collisions.slopeNormal.x;
                            velocity = v;
                        }
                    }
                    else
                    {
                        Vector3 v = velocity;
                        v.y = -maxJumpVelocity;
                        velocity = v;

                        canDoubleJump = true;
                    }
                }
            }
            else if (playerController.myGrav == Controller2D.GravDir.Left)
            {
                if (playerController.collisions.left)
                {
                    numberOfJumps = 0;

                    if (playerController.collisions.slidingDownMaxSlope)
                    {
                        if (directionalInput.y != -Mathf.Sign(playerController.collisions.slopeNormal.y))
                        { // not jumping against max slope
                            Vector3 v = velocity;
                            v.x = maxJumpVelocity * playerController.collisions.slopeNormal.x;
                            v.y = maxJumpVelocity * playerController.collisions.slopeNormal.y;
                            velocity = v;
                        }
                    }
                    else
                    {
                        Vector3 v = velocity;
                        v.x = maxJumpVelocity;
                        velocity = v;

                        canDoubleJump = true;
                    }
                }
            }
        }
    }

    // slow the players jump on button release (allowing player to make smaller jumps by releasing button before apex)
    public void OnJumpInputUp()
    {
        if (playerController.myGrav == Controller2D.GravDir.Down)
        {
            if (velocity.y > minJumpVelocity)
            {
                Vector3 v = velocity;
                v.y = minJumpVelocity;
                velocity = v;
            }
        }
        else if (playerController.myGrav == Controller2D.GravDir.Up)
        {
            if (velocity.y < -minJumpVelocity)
            {
                Vector3 v = velocity;
                v.y = -minJumpVelocity;
                velocity = v;
            }
        }
        else if (playerController.myGrav == Controller2D.GravDir.Left)
        {
            if (velocity.x > minJumpVelocity)
            {
                Vector3 v = velocity;
                v.x = minJumpVelocity;
                velocity = v;
            }
        }

    }
    
    #endregion
    //=====================================================================================================
    #region Private methods

    // used to action and reset the playerAttacking flag and timer
    void CheckTimers()
    {
        if (playerAttacking)
        {
            if (attackTimer <= 0)
            {
                playerAttacking = false;
                Debug.Log("playerAttacking = false");
            }
            else
            {
                attackTimer -= Time.deltaTime;
            }
        }

        if (playerIsHit)
        {
            if (playerHitTimer <= 0)
            {
                playerIsHit = false;
                Debug.Log("playerIsHit = false");
            }
            else
            {
                playerHitTimer -= Time.deltaTime;
            }
        }
    }

    // used during development to tweak player movement settings during runtime (not required in runtime)
    void RecalculateMovement()
    {
        wallJumpUp = new Vector2(wallJumpUpX, wallJumpUpY);
        wallJumpDown = new Vector2(wallJumpDownX, wallJumpDownY);
        wallJumpAway = new Vector2(wallJumpAwayX, wallJumpAwayY);

        CheckGravity();
        maxJumpVelocity = Mathf.Abs(gravity) * jumpSpeed;                          // calculate jumpVelocity
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);       // calculate jumpVelocity

        knockBack = new Vector2(knockBackX, knockBackY);
    }


    // Check gravity state and adjust if necessary
    void CheckGravity()
    {
        if ((playerController.myGrav == Controller2D.GravDir.Down) || (playerController.myGrav == Controller2D.GravDir.Left))
        {
            gravity = -(2 * maxJumpHeight) / Mathf.Pow(jumpSpeed, 2);
        }
        else if (playerController.myGrav == Controller2D.GravDir.Up)
        {
            gravity = (2 * maxJumpHeight) / Mathf.Pow(jumpSpeed, 2);
        }
    }

    // used to recalculate the players attack hitbox based on the current player position (allows the hitbox to move with the player)
    void RecalculateAttackHitBox()
    {
        if (playerController.collisions.facingDir == 1)
        {
            attackHitBoxPos = new Vector2(transform.position.x + attackLocationX, transform.position.y);
        }
        else if (playerController.collisions.facingDir == -1)
        {
            attackHitBoxPos = new Vector2(transform.position.x - attackLocationX, transform.position.y);
        }
    }

    // check if player touched an item and take appropriate action
    void CheckTouchItem()
    {
        if (playerController.collisions.touchItem)
        {
            //itemCount++;
            playerController.collisions.touchItem = false;
        }
    }

    // checks if player touched hazard and take appropriate action
    void CheckTouchHazard()
    {
        if (playerController.collisions.touchHazard)
        {
            Die();
            playerController.collisions.touchHazard = false;
        }
    }

    // checks if player touched an object that can damage the player and take appropriate action
    void CheckForDamageCollision()
    {
        if (playerController.collisions.touchEnemyAttack || playerController.collisions.touchEnemy && !playerIsHit)
        {
            Debug.Log("Enemy>Player collision");

            int knockBackDir = playerController.collisions.facingDir;

            Vector3 v = velocity;
            v.x = knockBackDir * -knockBack.x;
            v.y = knockBack.y;
            velocity = v;
            Debug.Log("velocity = " + velocity);

            playerIsHit = true;
            playerHitTimer = defaultHitTimer;
        }
    }

    // activate the die routine
    void Die()
    {
        // prevent player movement
        playerCanMove = false;
        velocity = Vector3.zero;
        GameController.gameControl.prevPlayerVelocity = Vector3.zero;

        // die (play animation)
        playerIsDead = true;

        // start respawn timer
        respawnTimerActive = true;
    }

    // checks if player touches checkpoint then updates gameController checkpoint
    void CheckTouchCheckpoint()
    {
        if (playerController.collisions.touchCheckpoint)
        {
            // update current checkpoint
            GameController.gameControl.currentCheckpoint = playerController.collisions.collidedWith;
            playerController.collisions.touchCheckpoint = false;
        }
    }

    // checks if player touches NPC then activates text
    void CheckTouchNPC()
    {
        if (playerController.collisions.touchNPC)
        {
            NPC npc = playerController.collisions.collidedWith.GetComponent<NPC>();
            npc.NPCIsTalking = true;
            playerController.collisions.touchNPC = false;
        }
    }

    // checks if player moves to edge of screen then loads next scene
    void CheckRoomPos()
    {    
        viewPosition = playerCamera.WorldToViewportPoint(transform.position);

        ProCamera2DNumericBoundaries cameraBounds = playerCamera.GetComponent<ProCamera2DNumericBoundaries>();

        vertExtent = playerCamera.orthographicSize;
        horzExtent = vertExtent * Screen.width / Screen.height;
        cameraRightPos = Mathf.Round(playerCamera.transform.position.x + horzExtent * 10f) / 10f;
        cameraLeftPos = Mathf.Round(playerCamera.transform.position.x - horzExtent * 10f) / 10f;
        cameraTopPos = Mathf.Round(playerCamera.transform.position.y + vertExtent * 10f) / 10f;
        cameraBottomPos = Mathf.Round(playerCamera.transform.position.y - vertExtent * 10f) / 10f;

        /*
        Debug.Log("horzExtent = " + horzExtent);
        Debug.Log("camera.transform.position(" + playerCamera.transform.position);
        Debug.Log("cameraBounds.RightBoundary(" + cameraBounds.RightBoundary + ")");
        Debug.Log("cameraRightPos = " + cameraRightPos);
        Debug.Log("cameraLeftPos = " + cameraLeftPos);
        Debug.Log("cameraBottomPos = " + cameraBottomPos);
        Debug.Log("cameraTopPos = " + cameraTopPos);
        */

        if (viewPosition.x > 1.01f)
        {
            // move right
            // check if camera is at boundary first
            if (cameraRightPos >= cameraBounds.RightBoundary)
            {
                Debug.Log("Bingo");
                // check if there is a scene available
                if (GameController.gameControl.currentRoomControl.moveSceneRightString != "")
                {
                    GameController.gameControl.NextScene(GameController.gameControl.currentRoomControl.moveSceneRightString, 1);
                    GameController.gameControl.prevPlayerVelocity = velocity;
                }
                //  if not kill of the player character
                else
                {
                    Die();
                }
            }
            // kill player if too far off screen (just in case)
            else if (cameraRightPos >= cameraBounds.RightBoundary + 5)
            {
                Die();
            }
        }
        else if (viewPosition.x < -0.01f)
        {
            // move left
            // check if camera is at boundary 
            if (cameraLeftPos <= cameraBounds.LeftBoundary)
            {
                if (GameController.gameControl.currentRoomControl.moveSceneLeftString != "")
                {
                    GameController.gameControl.NextScene(GameController.gameControl.currentRoomControl.moveSceneLeftString, 0);
                    GameController.gameControl.prevPlayerVelocity = velocity;
                }
                else
                {
                    Die();
                }
            }
            // kill player if too far off screen (just in case)
            else if (cameraLeftPos <= cameraBounds.LeftBoundary - 5)
            {
                Die();
            }
        }
        else if (viewPosition.y > 1.01f)
        {
            // move up
            // check if camera is at boundary 
            if (cameraTopPos >= cameraBounds.TopBoundary)
            {
                if (GameController.gameControl.currentRoomControl.moveSceneUpString != "")
                {
                    GameController.gameControl.NextScene(GameController.gameControl.currentRoomControl.moveSceneUpString, 2);
                    GameController.gameControl.prevPlayerVelocity = velocity;
                }
            }
        }
        else if (viewPosition.y < -0.01f)
        {
            // move down
            if (cameraBottomPos <= cameraBounds.BottomBoundary)
            {
                if (GameController.gameControl.currentRoomControl.moveSceneDownString != "")
                {
                    GameController.gameControl.NextScene(GameController.gameControl.currentRoomControl.moveSceneDownString, 3);
                    GameController.gameControl.prevPlayerVelocity = velocity;
                }
                // if you fall off the screen and there is no attached room then die
                else
                {
                    Die();
                }
            }
            // kill player if too far off the screen (just in case)
            else if (cameraBottomPos <= cameraBounds.BottomBoundary - 10)
            {
                Die();
            }
        }
    }

    // chceks if player needs to be respawned and takes appropriate action
    void CheckRespawn()
    {
        // wait before respawn
        if (respawnTimerActive == true)
        {
            respawnTimer -= 1;
        }

        if (respawnTimer == 0)
        {
            //respawn player prefab at spawn point and destroy this player object
            GameController.gameControl.SpawnPlayerFromCheckPoint();
            ProCamera2D.Instance.RemoveCameraTarget(transform);
            Destroy(this.gameObject);
        }
    }

    // checks if player is colliding with a wall and initiates wall slide
    void HandleWallSliding()
    {
        wallDirX = (playerController.collisions.left) ? -1 : 1;
        wallSliding = false;

        if (playerController.myGrav == Controller2D.GravDir.Down)
        {
            if ((playerController.collisions.left || playerController.collisions.right) && !playerController.collisions.below && velocity.y < 0)
            {
                wallSliding = true;
                playerJumping = false;
                canDoubleJump = false;

                if (velocity.y < -wallSlideSpeedMax)
                {
                    Vector3 v = velocity;
                    v.y = -wallSlideSpeedMax;
                    velocity = v;
                }

                if (timeToWallUnstick > 0)
                {
                    velocityXSmoothing = 0;

                    Vector3 v = velocity;
                    v.x = 0;
                    velocity = v;

                    if (directionalInput.x != wallDirX && directionalInput.x != 0)
                    {
                        timeToWallUnstick -= Time.deltaTime;
                    }
                    else
                    {
                        timeToWallUnstick = wallStickTime;
                    }
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
        }
        else if (playerController.myGrav == Controller2D.GravDir.Up)
        {
            if ((playerController.collisions.left || playerController.collisions.right) && !playerController.collisions.above && velocity.y > 0)
            {
                wallSliding = true;
                playerJumping = false;
                canDoubleJump = false;

                if (velocity.y > wallSlideSpeedMax)
                {
                    Vector3 v = velocity;
                    v.y = wallSlideSpeedMax;
                    velocity = v;
                }

                if (timeToWallUnstick > 0)
                {
                    velocityXSmoothing = 0;

                    Vector3 v = velocity;
                    v.x = 0;
                    velocity = v;

                    if (directionalInput.x != wallDirX && directionalInput.x != 0)
                    {
                        timeToWallUnstick -= Time.deltaTime;
                    }
                    else
                    {
                        timeToWallUnstick = wallStickTime;
                    }
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
        }

    }

    // calculates the velocity of the player based on directional input * move speed
    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        float targetVelocityY = directionalInput.y * moveSpeed;

        Vector3 v = velocity;

        if (playerController.myGrav == Controller2D.GravDir.Down)
        {
            v.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (playerController.collisions.below) ? accelerationSpeedGround : accelerationSpeedAir);
            v.y += gravity * Time.deltaTime;
        }
        else if (playerController.myGrav == Controller2D.GravDir.Up)
        {
            v.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (playerController.collisions.above) ? accelerationSpeedGround : accelerationSpeedAir);
            v.y += gravity * Time.deltaTime;
        }
        else if (playerController.myGrav == Controller2D.GravDir.Left)
        {
            v.y = Mathf.SmoothDamp(velocity.y, targetVelocityY, ref velocityYSmoothing, (playerController.collisions.left) ? accelerationSpeedGround : accelerationSpeedAir);
            v.x += gravity * Time.deltaTime;
        }

        velocity = v;
    }

    // used to display debug info on screen during development
    void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 200, 20), "Velocity X: " + velocity.x.ToString());
        //GUI.Label(new Rect(10, 24, 200, 20), "Velocity Y: " + velocity.y.ToString());
        //GUI.Label(new Rect(10, 38, 200, 20), "Item Count: " + itemCount.ToString());
        //GUI.Label(new Rect(10, 52, 200, 20), "ViewPositionX: " + viewPosition.x.ToString());
        //GUI.Label(new Rect(10, 66, 200, 20), "ViewPositionY: " + viewPosition.y.ToString());

        //GUI.Label(new Rect(10, 80, 500, 20), "camera.transform.position" + playerCamera.transform.position);
        //GUI.Label(new Rect(10, 94, 500, 20), "cameraBounds.RightBoundary(" + cameraBounds.RightBoundary + ")");
        //GUI.Label(new Rect(10, 108, 500, 20), "cameraBounds.LeftBoundary(" + cameraBounds.LeftBoundary + ")");
        //GUI.Label(new Rect(10, 122, 500, 20), "cameraBounds.TopBoundary(" + cameraBounds.TopBoundary + ")");
        //GUI.Label(new Rect(10, 136, 500, 20), "cameraBounds.BottomBoundary(" + cameraBounds.BottomBoundary + ")");

        //GUI.Label(new Rect(10, 150, 500, 20), "cameraRightPos = " + cameraRightPos);
        //GUI.Label(new Rect(10, 164, 500, 20), "cameraLeftPos = " + cameraLeftPos);
        //GUI.Label(new Rect(10, 178, 500, 20), "cameraTopPos = " + cameraTopPos);
        //GUI.Label(new Rect(10, 192, 500, 20), "cameraBottomPos = " + cameraBottomPos);

        GUI.Label(new Rect(52, 52, 100, 20), "Below:" + playerController.collisions.below.ToString());
        GUI.Label(new Rect(52, 72, 100, 20), "Above:" + playerController.collisions.above.ToString());
        GUI.Label(new Rect(52, 92, 100, 20), "Left:" + playerController.collisions.left.ToString());
        GUI.Label(new Rect(52, 112, 100, 20), "Right:" + playerController.collisions.right.ToString());
        GUI.Label(new Rect(52, 132, 300, 20), "Climbing Slope:" + playerController.collisions.climbingSlope.ToString());
        GUI.Label(new Rect(52, 152, 300, 20), "Descending Slope:" + playerController.collisions.descendingSlope.ToString());
        GUI.Label(new Rect(52, 172, 300, 20), "Slope Angle:" + playerController.collisions.slopeAngle.ToString());



    }

    // used to display debug gizmos on screen during development
    void OnDrawGizmos()
    {
        // shows player hit box
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, new Vector3(this.gameObject.GetComponent<BoxCollider2D>().size.x, this.gameObject.GetComponent<BoxCollider2D>().size.y, 1));
    }
    #endregion
    //=====================================================================================================
}
