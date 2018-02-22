using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Controller2D))]
public class Enemy : MonoBehaviour
{
    public bool enemyIsHit;
    public bool enemyIsTalking;
    public float talkTimer = 2;
    float defaultTalkTimer;
    public float hitTimer = 1;
    float defaultHitTimer;

    public string enemyDialog;
    public Text enemyText;
    public GameObject enemyCanvas;

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float jumpSpeed = .4f;
    public float accelerationSpeedAir = .2f;
    public float accelerationSpeedGround = .1f;
    public float moveSpeed = 6;
    public Vector2 knockBack;
 
    float gravity;
    public Vector3 velocity { get; set; }
    float velocityXSmoothing;
    float maxJumpVelocity;
    float minJumpVelocity;

    public int facingDirX { get; set; }

    public bool enemyJumping { get; set; }
    bool canDoubleJump;
    int numberOfJumps = 0;

    public bool enemyIsDead { get; set; }
    public bool enemyAttacking { get; set; }

    public Vector2 directionalInput { get; set; }
    public Controller2D enemyController;

    public bool enemyCanMove;
    public SpriteRenderer enemySprite;

    // Use this for initialization
    void Start()
    {
        //EnemyController = gameObject.GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(jumpSpeed, 2);                  // calculate gravity
        maxJumpVelocity = Mathf.Abs(gravity) * jumpSpeed;                          // calculate jumpVelocity
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);       // calculate jumpVelocity

        enemyCanvas.SetActive(false);

        defaultHitTimer = hitTimer;
        defaultTalkTimer = talkTimer;
        enemyIsHit = false;
    }

    // Update is called once per frame
    void Update()
    {

        CalculateVelocity();
        enemyController.Move(velocity * Time.deltaTime, directionalInput);

        if (enemyController.collisions.above || enemyController.collisions.below)
        {
            if (enemyController.collisions.below) enemyJumping = false;

            if (enemyController.collisions.slidingDownMaxSlope)
            {
                Vector3 v = velocity;
                v.y += enemyController.collisions.slopeNormal.y * -gravity * Time.deltaTime;
                velocity = v;
            }
            else
            {
                Vector3 v = velocity;
                v.y = 0;
                velocity = v;
            }
        }

        if (!enemyController.collisions.below && numberOfJumps == 0)
        {
            enemyJumping = true;
            canDoubleJump = true;
        }

        if (!enemyController.collisions.below && !enemyController.collisions.above && !enemyController.collisions.left && !enemyController.collisions.right)
        {
            enemyJumping = true;
        }


        CheckIfTalking();
        CheckForAttackCollision();
    }

    // check if enemy has collided with player attack
    void CheckForAttackCollision()
    {
        if (enemyController.collisions.touchPlayerAttack)
        {
            if (!enemyIsHit)
            {
                enemyIsHit = true;
                Debug.Log("EnemyIsHit = true;");

                int knockBackDir = enemyController.collisions.hitDir;

                // Knock back enemy unless a stationary object
                if (enemyCanMove)
                {
                    Vector3 v = velocity;
                    v.x = knockBackDir * knockBack.x;
                    v.y = knockBack.y;
                    velocity = v;
                    Debug.Log("velocity = " + velocity);
                }
            }
        }

        if (enemyIsHit)
        {
            // change color/flash
            enemySprite.material.SetFloat("_FlashAmount", 1);

            hitTimer -= Time.deltaTime;

            if (hitTimer <= 0)
            {
                hitTimer = defaultHitTimer;
                enemyIsHit = false;
            }
        }
    }

    // activated text if enemy is talking
    void CheckIfTalking()
    {
        if (enemyIsTalking)
        {
            // display text on screen
            print("NPC is Talking");

            ShowText(enemyDialog);


            talkTimer -= Time.deltaTime;

            if (talkTimer <= 0)
            {
                enemyCanvas.SetActive(false);

                print("NPC stopped talking");
                enemyIsTalking = false;

                talkTimer = defaultTalkTimer;
            }
        }
    }

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;

        Vector3 v = velocity;
        v.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (enemyController.collisions.below) ? accelerationSpeedGround : accelerationSpeedAir);
        v.y += gravity * Time.deltaTime;
        velocity = v;
    }

    void ShowText(string dialog)
    {
        enemyCanvas.SetActive(true);
        enemyText.text = dialog;
    }

    public int GetFacingDir()
    {
        int direction = enemyController.collisions.facingDir;

        return (direction);
    }

    public void SetDirectionalInput(Vector2 input)
    {
        if (enemyCanMove)
        {
            directionalInput = input;
        }
    }

}
