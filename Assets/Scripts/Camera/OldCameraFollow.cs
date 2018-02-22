using UnityEngine;
using System.Collections;

public class OldCameraFollow : MonoBehaviour {

    public Controller2D target;
    public Vector2 focusSize;
    public float lookAheadDistX;
    public float lookSmoothTimeX;
    public float vSmoothTime;
    public float vOffset;

    FocusArea focusArea;

    float currentLookAheadX;
    float targetLookAheadX;
    float lookAheadDirX;
    float smoothLookVelX;
    float smoothVelY;

    bool lookAheadStopped;

    public bool bounds;


    void Start()
    {
        focusArea = new FocusArea(target.col.bounds, focusSize);
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        focusArea.Update(target.col.bounds);
        Debug.Log("focusArea.Update(target.col.bounds) = focusArea.Update(" + target.col.bounds + ")");

        Vector3 focusPos = focusArea.centre + Vector2.up * vOffset;
        Debug.Log("focusPos" + focusPos + " = focusArea.centre" + focusArea.centre + " + Vector2.up" + Vector2.up + " * vOffset(" + vOffset +")"); 

        if (focusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(focusArea.velocity.x);

            if (Mathf.Sign(target.moveInput.x) == Mathf.Sign(focusArea.velocity.x) && target.moveInput.x != 0)
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

        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelX, lookSmoothTimeX);

        focusPos.y = Mathf.SmoothDamp(transform.position.y, focusPos.y, ref smoothVelY, vSmoothTime);
        focusPos.x += currentLookAheadX;
        focusPos.z = transform.position.z;
        transform.position = focusPos;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, .5f);
        Gizmos.DrawCube(focusArea.centre, focusSize);
    }

    struct FocusArea
    {
        public Vector2 centre;
        public Vector2 velocity;
        float left, right;
        float top, bottom;

        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x/2;
            right = targetBounds.center.x + size.x/2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y + size.y;

            velocity = Vector2.zero;
            centre = new Vector2((left + right)/2, (top+bottom)/2);
        }

        public void Update(Bounds targetBounds)
        {
            float shiftX = 0;
            Debug.Log("float shiftX = " + shiftX);

            if (targetBounds.min.x < left)
            {
                Debug.Log("if (targetBounds.min.x(" + targetBounds.min.x + ") < left(" + left + "))");
                shiftX = targetBounds.min.x - left;
                Debug.Log("shiftX(" + shiftX + ") = targetBounds.min.x(" + targetBounds.min.x + ") - left(" + left + ")");
            }
            else if (targetBounds.max.x > right)
            {
                Debug.Log("if (targetBounds.max.x(" + targetBounds.max.x + ") < right(" + right + "))");
                shiftX = targetBounds.max.x - right;
                Debug.Log("shiftX(" + shiftX + ") = targetBounds.max.x(" + targetBounds.max.x + ") - right(" + right + ")");
            }

            left += shiftX;
            Debug.Log("left(" + left + ") += shiftX(" + shiftX + ")");
            right += shiftX;
            Debug.Log("right(" + right + ") += shiftX(" + shiftX + ")");


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

            centre = new Vector2((left + right)/2, (top + bottom)/2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }
}
