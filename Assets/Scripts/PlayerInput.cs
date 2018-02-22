using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour {

    Player player;

    void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        if (player.playerNumber == Player.PlayerNumber.One)
        {
            Vector2 directionalInput = new Vector2(Input.GetAxisRaw("P1Hor"), Input.GetAxisRaw("P1Vert"));
            player.SetDirectionalInput(directionalInput);

            if (Input.GetButtonDown("P1Jump"))
            {
                player.OnJumpInputDown();
            }

            if (Input.GetButtonUp("P1Jump"))
            {
                player.OnJumpInputUp();
            }

            if (Input.GetButtonDown("P1Action"))
            {
                player.OnAttackInputDown();
            }
        }

        else if (player.playerNumber == Player.PlayerNumber.Two)
        {
            Vector2 directionalInput = new Vector2(Input.GetAxisRaw("P2Hor"), Input.GetAxisRaw("P2Vert"));
            player.SetDirectionalInput(directionalInput);

            if (Input.GetButtonDown("P2Jump"))
            {
                player.OnJumpInputDown();
            }

            if (Input.GetButtonUp("P2Jump"))
            {
                player.OnJumpInputUp();
            }

            if (Input.GetButtonDown("P2Action"))
            {
                player.OnAttackInputDown();
            }
        }

        else if (player.playerNumber == Player.PlayerNumber.Three)
        {
            Vector2 directionalInput = new Vector2(Input.GetAxisRaw("P3Hor"), Input.GetAxisRaw("P3Vert"));
            player.SetDirectionalInput(directionalInput);

            if (Input.GetButtonDown("P3Jump"))
            {
                player.OnJumpInputDown();
            }

            if (Input.GetButtonUp("P3Jump"))
            {
                player.OnJumpInputUp();
            }

            if (Input.GetButtonDown("P3Action"))
            {
                player.OnAttackInputDown();
            }
        }
        else if (player.playerNumber == Player.PlayerNumber.Four)
        {
            Vector2 directionalInput = new Vector2(Input.GetAxisRaw("P4Hor"), Input.GetAxisRaw("P4Vert"));
            player.SetDirectionalInput(directionalInput);

            if (Input.GetButtonDown("P4Jump"))
            {
                player.OnJumpInputDown();
            }

            if (Input.GetButtonUp("P4Jump"))
            {
                player.OnJumpInputUp();
            }

            if (Input.GetButtonDown("P4Action"))
            {
                player.OnAttackInputDown();
            }
        }
    }
}
