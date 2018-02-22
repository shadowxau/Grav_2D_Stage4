using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimControl : MonoBehaviour {

    [SerializeField]
    private GameObject thisObject;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Controller2D controller;

    Player playerMain;
    Enemy enemyMain;

    Vector2 defaultScale;
    float stateStartTime;

    const string kIdleAnim = "Idle";
    const string kRunAnim = "Run";
    const string kJumpAnim = "Jump";
    const string kFallAnim = "Fall";
    const string kDieAnim = "Die";
    const string kWallHangAnim = "WallHang";
    const string kAttackAnim = "Attack";
    const string kHurtAnim = "Hurt";

    enum State
    {
        Idle,
        RunLeft,
        RunRight,
        Jump,
        Fall,
        WallHang,
        Die,
        Attack,
        AttackLeft,
        AttackRight,
        Hurt
    }

    State state;


    // Use this for initialization
    void Start()
    {
        defaultScale = new Vector2(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y));
        state = State.Idle;

        if (thisObject.tag == "Player")
        {
            playerMain = thisObject.GetComponent<Player>();
            Face(playerMain.GetFacingDir());
        }

        if (thisObject.tag == "Enemy")
        {
            enemyMain = thisObject.GetComponent<Enemy>();
            Face(enemyMain.GetFacingDir());
        }
    }

    // Update is called once per frame
    void Update()
    {
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
            case State.Attack:
                animator.Play(kAttackAnim);
                break;
            case State.AttackLeft:
                animator.Play(kAttackAnim);
                Face(-1);
                break;
            case State.AttackRight:
                animator.Play(kAttackAnim);
                Face(1);
                break;
            case State.Hurt:
                animator.Play(kHurtAnim);
                break;
        }

        this.state = state;
        stateStartTime = Time.time;
    }

    void ContinueState()
    {
        if (playerMain != null)
        {
            switch (state)
            {
                case State.Idle:
                    CheckAction();
                    break;
                case State.RunLeft:
                case State.RunRight:
                    // change to idle when no action
                    if (!CheckAction()) EnterState(State.Idle);
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
                    // change to attack
                    if (playerMain.playerAttacking) EnterState(State.Attack);
                    break;
                case State.Fall:
                    // change direction in mid air
                    if (playerMain.directionalInput.x > 0) Face(1);
                    else if (playerMain.directionalInput.x < 0) Face(-1);
                    // change to idle anim if colliding with ground
                    if (controller.collisions.below && !CheckAction()) EnterState(State.Idle);
                    // jump anim (double jump)
                    if (playerMain.playerJumping && playerMain.velocity.y > 0) EnterState(State.Jump);
                    // wall hang
                    if (playerMain.wallSliding) EnterState(State.WallHang);
                    // change to die
                    if (playerMain.playerIsDead) EnterState(State.Die);
                    // change to attack
                    if (playerMain.playerAttacking) EnterState(State.Attack);
                    break;
                case State.WallHang:
                    // change to jump
                    //if (playerJumping) EnterState(State.Jump);
                    // change to fall
                    if (playerMain.velocity.y > 0 && playerMain.playerJumping && !playerMain.wallSliding) EnterState(State.Fall);
                    // change to idle on collision with ground
                    if (controller.collisions.below && !CheckAction()) EnterState(State.Idle);
                    // change to die
                    if (playerMain.playerIsDead) EnterState(State.Die);
                    break;
                case State.Die:
                    // die
                    break;
                case State.Attack:
                case State.AttackLeft:
                case State.AttackRight:
                    // change to idle when no action
                    if (!CheckAction()) EnterState(State.Idle);
                    // change to idle when playerAttacking flag is false
                    if (playerMain.playerAttacking == false) EnterState(State.Idle);
                    break;
            }
        }
        else if (enemyMain != null)
        {
            switch (state)
            {
                case State.Idle:
                    CheckAction();
                    break;
                case State.RunLeft:
                case State.RunRight:
                    // change to idle when no action
                    if (!CheckAction()) EnterState(State.Idle);
                    // change to falling anim
                    if (enemyMain.enemyJumping && enemyMain.velocity.y < 0) EnterState(State.Fall);
                    // change to die
                    if (enemyMain.enemyIsDead) EnterState(State.Die);
                    break;
                case State.Jump:
                    // change direction in mid air
                    if (enemyMain.directionalInput.x > 0) Face(1);
                    else if (enemyMain.directionalInput.x < 0) Face(-1);
                    // falling anim
                    if (enemyMain.enemyJumping && enemyMain.velocity.y < 0) EnterState(State.Fall);
                    // change to die
                    if (enemyMain.enemyIsDead) EnterState(State.Die);
                    // change to attack
                    if (enemyMain.enemyAttacking) EnterState(State.Attack);
                    break;
                case State.Fall:
                    // change direction in mid air
                    if (enemyMain.directionalInput.x > 0) Face(1);
                    else if (enemyMain.directionalInput.x < 0) Face(-1);
                    // change to idle anim if colliding with ground
                    if (controller.collisions.below && !CheckAction()) EnterState(State.Idle);
                    // jump anim (double jump)
                    if (enemyMain.enemyJumping && enemyMain.velocity.y > 0) EnterState(State.Jump);
                    // change to die
                    if (enemyMain.enemyIsDead) EnterState(State.Die);
                    // change to attack
                    if (enemyMain.enemyAttacking) EnterState(State.Attack);
                    break;
                case State.WallHang:
                    // change to jump
                    //if (playerJumping) EnterState(State.Jump);
                    // change to fall
                    if (enemyMain.velocity.y > 0 && enemyMain.enemyJumping) EnterState(State.Fall);
                    // change to idle on collision with ground
                    if (controller.collisions.below && !CheckAction()) EnterState(State.Idle);
                    // change to die
                    if (enemyMain.enemyIsDead) EnterState(State.Die);
                    break;
                case State.Die:
                    // die
                    break;
                case State.Attack:
                case State.AttackLeft:
                case State.AttackRight:
                    // change to idle when no action
                    if (!CheckAction()) EnterState(State.Idle);
                    // change to idle when playerAttacking flag is false
                    if (enemyMain.enemyAttacking == false) EnterState(State.Idle);
                    break;
            }
        }


    }

    bool CheckAction()
    {
        // player animation state checks here
        if (playerMain != null)
        {
            if (playerMain.playerJumping) SetOrKeepState(State.Jump);
            else if (playerMain.directionalInput.x < 0) SetOrKeepState(State.RunLeft);
            else if (playerMain.directionalInput.x > 0) SetOrKeepState(State.RunRight);
            else if (playerMain.playerIsDead) SetOrKeepState(State.Die);
            else if (playerMain.playerAttacking && playerMain.directionalInput.x == 0) SetOrKeepState(State.Attack);
            else if (playerMain.playerAttacking && playerMain.directionalInput.x < 0) SetOrKeepState(State.AttackLeft);
            else if (playerMain.playerAttacking && playerMain.directionalInput.x > 0) SetOrKeepState(State.AttackRight);
            else return false;
            return true;
        }
        // enemy animation state checks here
        else if (enemyMain != null)
        {
            if (enemyMain.enemyJumping) SetOrKeepState(State.Jump);
            else if (enemyMain.directionalInput.x < 0) SetOrKeepState(State.RunLeft);
            else if (enemyMain.directionalInput.x > 0) SetOrKeepState(State.RunRight);
            else if (enemyMain.enemyIsDead) SetOrKeepState(State.Die);
            else if (enemyMain.enemyAttacking && playerMain.directionalInput.x == 0) SetOrKeepState(State.Attack);
            else if (enemyMain.enemyAttacking && playerMain.directionalInput.x < 0) SetOrKeepState(State.AttackLeft);
            else if (enemyMain.enemyAttacking && playerMain.directionalInput.x > 0) SetOrKeepState(State.AttackRight);
            else return false;
            return true;
        }
        return true;
    }

    void Face(int direction)
    {
        transform.localScale = new Vector2(defaultScale.x * direction, defaultScale.y);
    }
}
