using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    private DraggableObject selected;

    void Update()
    {
        // Left mouse button down = try to select
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Ray hit: " + hit.collider.name + " (GameObject: " + hit.collider.gameObject.name + ")");

                // Try to find a DraggableObject on the parent of the clicked collider
                DraggableObject draggable = hit.collider.GetComponentInParent<DraggableObject>();
                if (draggable != null)
                {
                    Debug.Log("✅ Selected: " + draggable.name);
                    Select(draggable);
                }
                else
                {
                    Debug.LogWarning("❌ No DraggableObject found on parent of: " + hit.collider.name);
                    Deselect();
                }
            }
            else
            {
                Debug.Log("Raycast hit nothing.");
                Deselect();
            }
        }

        // Left mouse button up = stop dragging
        if (Input.GetMouseButtonUp(0))
        {
            Deselect();
        }
    }

    void Select(DraggableObject obj)
    {
        if (selected != null)
            selected.EndDrag();

        selected = obj;
        selected.BeginDrag();
    }

    void Deselect()
    {
        if (selected != null)
        {
            selected.EndDrag();
            selected = null;
        }
    }
}