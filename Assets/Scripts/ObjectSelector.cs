using UnityEngine;

public class ObjectSelector : MonoBehaviour
{
    private DraggableObject selected;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Chair") || hit.collider.CompareTag("Table"))
                {
                    Select(hit.collider.GetComponentInParent<DraggableObject>());
                }
                else
                {
                    Deselect();
                }
                
                
                if (hit.collider != null)
                {
                    Debug.Log("Hit object: " + hit.collider.name);
                }

                
            }
            else
            {
                Deselect();
            }
        }

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
