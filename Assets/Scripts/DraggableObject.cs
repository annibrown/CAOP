using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private bool isDragging = false;
    private float yOffset;
    private Plane dragPlane;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance;

            if (dragPlane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(distance);
                Vector3 targetPos = new Vector3(point.x, rb.position.y, point.z);

                // Let Unity handle collision — this obeys physics
                rb.MovePosition(targetPos);
            }

            if (Input.GetKey(KeyCode.Q))
                transform.Rotate(Vector3.up, -100 * Time.deltaTime);
            if (Input.GetKey(KeyCode.E))
                transform.Rotate(Vector3.up, 100 * Time.deltaTime);
        }
    }


    private bool CanMoveTo(Vector3 targetPosition)
    {
        // Get all child colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (colliders.Length == 0) return true;

        // Combine all bounds
        Bounds combinedBounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            combinedBounds.Encapsulate(colliders[i].bounds);
        }

        // Offset the bounds to the target position
        Vector3 offset = targetPosition - transform.position;
        Vector3 testCenter = combinedBounds.center + offset;

        // Shrink slightly to prevent tiny overlaps
        Vector3 halfExtents = combinedBounds.extents - new Vector3(0.05f, 0.01f, 0.05f);

        // Check if new position overlaps any wall
        Collider[] hits = Physics.OverlapBox(
            testCenter,
            halfExtents,
            Quaternion.identity, // no rotation
            LayerMask.GetMask("Wall")
        );

        return hits.Length == 0;
    }



    public void BeginDrag()
    {
        isDragging = true;
        yOffset = transform.position.y;
        dragPlane = new Plane(Vector3.up, Vector3.up * yOffset);
    }

    public void EndDrag()
    {
        isDragging = false;
    }
}
