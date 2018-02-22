using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Vector2 focusSize;
    public GameObject player;
    public bool clampToBoundary;

    public Vector3 minCameraPos;
    public Vector3 maxCameraPos;

    public float lookAheadDistX;
    public float lookSmoothTimeX;
    public float vSmoothTime;
    public float vOffset;

    FocusAreaTransPos focusAreaTransform;

    public Controller2D target;
    FocusAreaBox focusAreaBox;
    
    public float currentLookAheadX;
    public float targetLookAheadX;
    public float lookAheadDirX;
    public float smoothLookVelX;
    public float smoothVelY;

    public bool lookAheadStopped;
    public bool FocusAreaEqualsTransPos;

    // Use this for initialization
    void Start()
    {
        focusAreaTransform = new FocusAreaTransPos(player, focusSize);
        focusAreaBox = new FocusAreaBox(target.col.bounds, focusSize);
       
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }


    }

    void LateUpdate()
    {
        Vector3 focusPos;

        if(target == null) { return; }

        if (FocusAreaEqualsTransPos)
        {
            focusAreaTransform.Update(player, focusSize);
            focusPos = focusAreaTransform.centre + Vector2.up * vOffset;

            if (focusAreaTransform.velocity.x != 0)
            {
                lookAheadDirX = Mathf.Sign(focusAreaTransform.velocity.x);

                if (Mathf.Sign(player.GetComponent<Controller2D>().moveInput.x) == Mathf.Sign(focusAreaTransform.velocity.x) && player.GetComponent<Controller2D>().moveInput.x != 0)
                {
                    lookAheadStopped = false;
                    targetLookAheadX = lookAheadDirX * lookAheadDistX;
                }
                else
                {
                    if (!lookAheadStopped)
                    {
                        lookAheadStopped = true;
                        targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistX - currentLookAheadX) / 4f;
                    }
                }
            }
        }
        else
        {
            focusAreaBox.Update(target.col.bounds, focusSize);
            focusPos = focusAreaBox.centre + Vector2.up * vOffset;

            Debug.Log("focusAreaBox.velocity.x = " + focusAreaBox.velocity.x);

            if (focusAreaBox.velocity.x != 0)
            {
                lookAheadDirX = Mathf.Sign(focusAreaBox.velocity.x);
                Debug.Log("lookAheadDirX = " + Mathf.Sign(focusAreaBox.velocity.x));

                float x = target.moveInput.x;


                //if (Mathf.Sign(player.GetComponent<Controller2D>().moveInput.x) == Mathf.Sign(focusAreaBox.velocity.x) && player.GetComponent<Controller2D>().moveInput.x != 0)
                if (Mathf.Sign(x) == lookAheadDirX && x != 0)
                {
                    Debug.Log("if (Mathf.Sign(x) == lookAheadDirX && x != 0)");
                    Debug.Log(Mathf.Sign(x) + " == " + lookAheadDirX + " && " + x + "!= 0");


                    lookAheadStopped = false;
                    targetLookAheadX = lookAheadDirX * lookAheadDistX;
                    Debug.Log("targetLookAheadX(" + targetLookAheadX + ") = lookAheadDirX(" + lookAheadDirX + ") * lookAheadDistX(" + lookAheadDistX + ")");
                }
                else
                {
                    Debug.Log("else");
                    Debug.Log(Mathf.Sign(x) + " == " + lookAheadDirX + " || " + x +  "== 0");

                    if (!lookAheadStopped)
                    {
                        lookAheadStopped = true;
                        targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistX - currentLookAheadX) / 4f;
                        Debug.Log("targetLookAheadX(" + targetLookAheadX + ") = currentLookAheadX(" + currentLookAheadX + ") + (lookAheadDirX(" + lookAheadDirX + ") * lookAheadDistX(" + lookAheadDistX + ") - currentLookAhead(" + currentLookAheadX + ") / 4f");
                    }
                }
            }
        }

        /*
        Vector3 focusPos = focusAreaTransform.centre + Vector2.up * vOffset;

        if (focusAreaTransform.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(focusAreaTransform.velocity.x);

            if (Mathf.Sign(player.GetComponent<Controller2D>().moveInput.x) == Mathf.Sign(focusAreaTransform.velocity.x) && player.GetComponent<Controller2D>().moveInput.x != 0)
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDistX;
            }
            else
            {
                if (!lookAheadStopped)
                {
                    lookAheadStopped = true;
                    targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistX - currentLookAheadX) / 4f;
                }
            }
        }
        */

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelX, lookSmoothTimeX);
        focusPos.x += currentLookAheadX;
        focusPos.y = Mathf.SmoothDamp(transform.position.y, focusPos.y, ref smoothVelY, vSmoothTime);
        focusPos.z = transform.position.z;

        // move to the new position
        transform.position = focusPos;

        // clamp view to the boundary
        if (clampToBoundary)
        {
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, minCameraPos.x, maxCameraPos.x),
                Mathf.Clamp(transform.position.y, minCameraPos.y, maxCameraPos.y),
                Mathf.Clamp(transform.position.z, minCameraPos.z, maxCameraPos.z));
        }
    }

    void OnDrawGizmos()
    {
        if (FocusAreaEqualsTransPos)
        {
            Gizmos.color = new Color(0, 1, 0, .5f);
            Gizmos.DrawCube(focusAreaTransform.centre, focusSize);
        }
        else
        {
            Gizmos.color = new Color(0, 1, 0, .5f);
            Gizmos.DrawCube(focusAreaBox.centre, focusSize);
        }
    }

    struct FocusAreaTransPos
    {
        public Vector2 centre;
        public Vector2 velocity;
        float left, right;
        float top, bottom;

        public FocusAreaTransPos(GameObject target, Vector2 size)
        {
            left = target.transform.position.x - size.x /2;
            right = target.transform.position.x + size.x /2;
            bottom = target.transform.position.y - size.y;
            top = target.transform.position.y + size.y;

            velocity = Vector2.zero;
            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
        }

        public void Update(GameObject target, Vector2 size)
        {
            float shiftX = 0;

            /*
            left = target.transform.position.x - size.x;
            right = target.transform.position.x + size.x;
            bottom = target.transform.position.y - size.y;
            top = target.transform.position.y + size.y;

            velocity = Vector2.zero;
            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
            */

            Debug.Log((target.transform.position.x - size.x) + "::" + target.transform.position.x + "::" + (target.transform.position.x + size.x));
            Debug.Log((target.transform.position.y - size.y) + "::" + target.transform.position.y + "::" + (target.transform.position.y + size.y));

            Debug.Log("left = " + left + ", right = " + right + ", bottom = " + bottom + ", top = " + top + ", centre = " + centre);

            if ((target.transform.position.x - size.x) < left)
            {
                Debug.Log("if ((target.transform.position.x - size.x) < left)");
                Debug.Log("if (" + (target.transform.position.x - size.x) + " < " + left+ ")");

                shiftX = (target.transform.position.x - size.x) - left;
                Debug.Log("shiftX = (" + ((target.transform.position.x - size.x) - left) + ");");
            }
            else if ((target.transform.position.x + size.x) > right)
            {
                Debug.Log("else if ((target.transform.position.x + size.x) > right)");
                Debug.Log("else if (" + (target.transform.position.x + size.x) + " > " + right + ")");

                shiftX = (target.transform.position.x + size.x) - right;
                Debug.Log("shiftX = (" + ((target.transform.position.x + size.x) - right) + ");");
            }


            left += shiftX;
            Debug.Log("left = " + left);

            right += shiftX;
            Debug.Log("right = " + right);



            float shiftY = 0;

            if (target.transform.position.y < bottom)
            {
                shiftY = target.transform.position.y - bottom;
            }
            else if (target.transform.position.y > top)
            {
                shiftY = target.transform.position.y - top;
            }

            top += shiftY;
            bottom += shiftY;

            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }

    struct FocusAreaBox
    {
        public Vector2 centre;
        public Vector2 velocity;
        float left, right;
        float top, bottom;


        public FocusAreaBox(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y + size.y;

            velocity = Vector2.zero;
            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
        }

        public void Update(Bounds targetBounds, Vector2 size)
        {

            float shiftX = 0;
            Debug.Log("velocity = " + velocity);

            Debug.Log ("targetBounds.min.x = " + targetBounds.min.x + ", targetBounds.center = " + targetBounds.center.x + ", targetBounds.max.x = " + targetBounds.max.x);
            Debug.Log("left = " + left + ", right = " + right + ", bottom = " + bottom + ", top = " + top + ", centre = " + centre);

            if (targetBounds.min.x < left)
            {
                Debug.Log("if (targetBounds.min.x < left)");
                Debug.Log("if (" + targetBounds.min.x + " < " + left + ")");

                shiftX = targetBounds.min.x - left;
                Debug.Log("shiftX = " + (targetBounds.min.x - left));
            }
            else if (targetBounds.max.x > right)
            {
                Debug.Log("if (targetBounds.max.x > right)");
                Debug.Log("if (" + targetBounds.max.x + " > " + right + ")");

                shiftX = targetBounds.max.x - right;
                Debug.Log("shiftX = " + (targetBounds.max.x - right));
            }


            left += shiftX;
            Debug.Log("left = " + left);

            right += shiftX;
            Debug.Log("right = " + right);


            float shiftY = 0;
            if (targetBounds.min.y < bottom)
            {
                shiftY = targetBounds.min.y - bottom;
            }
            else if (targetBounds.max.y > top)
            {
                shiftY = targetBounds.max.y - top;
            }


            top += shiftY;
            bottom += shiftY;


            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
            Debug.Log("new velocity = " + velocity);

        }
    }
}
