using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private bool isDragging = false;
    private float yOffset;
    private Plane dragPlane;

    void Update()
    {
        if (isDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance;

            if (dragPlane.Raycast(ray, out distance))
            {
                Vector3 point = ray.GetPoint(distance);
                transform.position = new Vector3(point.x, yOffset, point.z);
            }

            // Optional: Rotate with Q/E
            if (Input.GetKey(KeyCode.Q))
                transform.Rotate(Vector3.up, -100 * Time.deltaTime);
            if (Input.GetKey(KeyCode.E))
                transform.Rotate(Vector3.up, 100 * Time.deltaTime);
        }
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
