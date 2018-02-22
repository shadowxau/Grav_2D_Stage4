using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Controller2D))]
public class HitCollider : MonoBehaviour {

    public float defaultHitBoxKillTimer = 1.0f;
    float killTimer;
    public GameObject hitBoxOwner;
    Player ownerScript;
    public int hitBoxDir;
    public bool isEnabled;


	// Use this for initialization
	void Start () {

        if (hitBoxOwner != null)
        {
            ownerScript = hitBoxOwner.GetComponent<Player>();
        }

        killTimer = defaultHitBoxKillTimer;
	}
	
	// Update is called once per frame
	void Update () {

        if (isEnabled)
        {
            killTimer -= Time.deltaTime;

            CheckKillTimer();
        }
           Move();
    }

    void CheckKillTimer()
    {
        if (killTimer <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    void Move()
    {
        transform.position = ownerScript.attackHitBoxPos;
    }
}
