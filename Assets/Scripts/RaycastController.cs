using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour {

    public LayerMask solidCollisionMask;
    public LayerMask enemyCollisionMask;
    public LayerMask triggerCollisionMask;

    public const float skinWidth = .015f;                                                                                                               // Ray inset amount
    public int horRayCount = 4;
    public int vertRayCount = 4;

    [HideInInspector]
    public float horRaySpacing;
    [HideInInspector]
    public float vertRaySpacing;

    [HideInInspector]
    public BoxCollider2D col;
    public RaycastOrigins raycastOrigins;

    public Bounds bounds;

    public virtual void Awake()
    {
        col = GetComponent<BoxCollider2D>();
    }

    public virtual void Start()
    {
        CalculateRaySpacing();
    }


    //Find Raycast Origins
    public void UpdateRaycastOrigins()
    {
        bounds = col.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    // Calculate the space between each ray. Allows auto-recalculation based on size of object.
    public void CalculateRaySpacing()
    {
        bounds = col.bounds;
        bounds.Expand(skinWidth * -2);

        horRayCount = Mathf.Clamp(horRayCount, 2, int.MaxValue);
        vertRayCount = Mathf.Clamp(vertRayCount, 2, int.MaxValue);

        horRaySpacing = bounds.size.y / (horRayCount - 1);
        vertRaySpacing = bounds.size.x / (vertRayCount - 1);
    }

    public struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

}
