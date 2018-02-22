using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Controller2D))]
public class NPC : MonoBehaviour {

    public bool NPCIsTalking;
    public float talkTimer = 2;
    float defaultTalkTimer;
    public string NPCDialog;
    public Text NPCText;
    public GameObject NPCCanvas;

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float jumpSpeed = .4f;
    public float accelerationSpeedAir = .2f;
    public float accelerationSpeedGround = .1f;
    public float moveSpeed = 6;
    
    float gravity;
    Vector3 velocity;
    float velocityXSmoothing;
    float maxJumpVelocity;
    float minJumpVelocity;

    bool NPCJumping;
    bool canDoubleJump;
    int numberOfJumps = 0;
    
    Vector2 directionalInput;
    Controller2D NPCController;

    // Use this for initialization
    void Start () {
        NPCController = gameObject.GetComponent<Controller2D>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(jumpSpeed, 2);                  // calculate gravity
        maxJumpVelocity = Mathf.Abs(gravity) * jumpSpeed;                          // calculate jumpVelocity
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);       // calculate jumpVelocity

        NPCCanvas.SetActive(false);

        defaultTalkTimer = talkTimer;
    }

    // Update is called once per frame
    void Update () {

        CalculateVelocity();
        NPCController.Move(velocity * Time.deltaTime, directionalInput);

        if (NPCController.collisions.above || NPCController.collisions.below)
        {
            if (NPCController.collisions.below) NPCJumping = false;

            if (NPCController.collisions.slidingDownMaxSlope)
            {
                velocity.y += NPCController.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }
        }

        if (!NPCController.collisions.below && numberOfJumps == 0)
        {
            NPCJumping = true;
            canDoubleJump = true;
        }

        if (!NPCController.collisions.below && !NPCController.collisions.above && !NPCController.collisions.left && !NPCController.collisions.right)
        {
            NPCJumping = true;
        }

        if (NPCIsTalking)
        {
            // display text on screen
            print("NPC is Talking");

            ShowText(NPCDialog);


            talkTimer -= Time.deltaTime;

            if (talkTimer <= 0)
            {
                NPCCanvas.SetActive(false);

                print("NPC stopped talking");
                NPCIsTalking = false;

                talkTimer = defaultTalkTimer;
            }
        }

	}

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (NPCController.collisions.below) ? accelerationSpeedGround : accelerationSpeedAir);
        velocity.y += gravity * Time.deltaTime;
    }

    void ShowText(string dialog)
    {
        NPCCanvas.SetActive(true);
        NPCText.text = dialog;
    }

}
