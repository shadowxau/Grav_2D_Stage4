using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnim : MonoBehaviour {

    [SerializeField]
    private Player playerMain;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Controller2D controller;

    Vector2 defaultScale;
    float stateStartTime;

    const string kIdleAnim = "PlayerIdle";
    const string kRunAnim = "PlayerRun";
    const string kJumpAnim = "PlayerJump";
    const string kFallAnim = "PlayerFall";
    const string kDieAnim = "PlayerDie";
    const string kWallHangAnim = "PlayerWallHang";

    enum State
    {
        Idle,
        RunLeft,
        RunRight,
        Jump,
        Fall,
        WallHang,
        Die
    }

    State state;


    // Use this for initialization
    void Start () {
        defaultScale = new Vector2(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y));
        state = State.Idle;
        Face(GameController.gameControl.prevPlayerDirX);
    }
	
	// Update is called once per frame
	void Update () {
		// Update state
        ContinueState();
	}

    // used to determine change in animation state
    void SetOrKeepState(State state)
    {
        if (this.state == state) return;
        EnterState(state);
    }

    // used to initiate animation state
    void EnterState(State state)
    {
        switch (state)
        {
            case State.Idle:
                animator.Play(kIdleAnim);
                break;
            case State.RunLeft:
                animator.Play(kRunAnim);
                Face(-1);
                break;
            case State.RunRight:
                animator.Play(kRunAnim);
                Face(1);
                break;
            case State.Jump:
                animator.Play(kJumpAnim);
                break;
            case State.Fall:
                animator.Play(kFallAnim);
                break;
            case State.Die:
                animator.Play(kDieAnim);
                break;
            case State.WallHang:
                if (playerMain.wallDirX == 1) Face(-1);
                else if (playerMain.wallDirX == -1) Face(1);
                animator.Play(kWallHangAnim);
                break;
        }

        this.state = state;
        stateStartTime = Time.time;
    }

    void ContinueState()
    {
        switch (state)
        {
            case State.Idle:
                CheckRunOrJump();
                break;
            case State.RunLeft:
            case State.RunRight:
                // change to idle when not running
                if (!CheckRunOrJump()) EnterState(State.Idle);
                // change to falling anim
                if (playerMain.playerJumping && playerMain.velocity.y < 0) EnterState(State.Fall);
                // change to die
                if (playerMain.playerIsDead) EnterState(State.Die);
                break;
            case State.Jump:
                // change direction in mid air
                if (playerMain.directionalInput.x > 0) Face(1);
                else if (playerMain.directionalInput.x < 0) Face(-1);
                // falling anim
                if (playerMain.playerJumping && playerMain.velocity.y < 0) EnterState(State.Fall);
                // wall hang
                if (playerMain.wallSliding) EnterState(State.WallHang);
                // change to die
                if (playerMain.playerIsDead) EnterState(State.Die);
                break;
            case State.Fall:
                // change direction in mid air
                if (playerMain.directionalInput.x > 0) Face(1);
                else if (playerMain.directionalInput.x < 0) Face(-1);
                // change to idle anim if colliding with ground
                if (controller.collisions.below && !CheckRunOrJump()) EnterState(State.Idle);
                // jump anim (double jump)
                if (playerMain.playerJumping && playerMain.velocity.y > 0) EnterState(State.Jump);
                // wall hang
                if (playerMain.wallSliding) EnterState(State.WallHang);
                // change to die
                if (playerMain.playerIsDead) EnterState(State.Die);
                break;
            case State.WallHang:
                // change to jump
                //if (playerJumping) EnterState(State.Jump);
                // change to fall
                if (playerMain.velocity.y > 0 && playerMain.playerJumping && !playerMain.wallSliding) EnterState(State.Fall);
                // change to idle on collision with ground
                if (controller.collisions.below && !CheckRunOrJump()) EnterState(State.Idle);
                // change to die
                if (playerMain.playerIsDead) EnterState(State.Die);
                break;
            case State.Die:
                // die
                break;
        }
    }

    bool CheckRunOrJump()
    {
        if (playerMain.playerJumping) SetOrKeepState(State.Jump);
        else if (playerMain.directionalInput.x < 0) SetOrKeepState(State.RunLeft);
        else if (playerMain.directionalInput.x > 0) SetOrKeepState(State.RunRight);
        else if (playerMain.playerIsDead) SetOrKeepState(State.Die);
        else return false;
        return true;
    }

    void Face(int direction)
    {
        transform.localScale = new Vector2(defaultScale.x * direction, defaultScale.y);
    }

    void OnGUI()
    {
        //GUI.Label(new Rect(10, 10, 200, 20), "State: " + state.ToString());
    }
}
