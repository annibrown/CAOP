using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Layout : MonoBehaviour
{
    public List<GameObject> F = new List<GameObject>();                 // furniture
    public List<List<GameObject>> G = new List<List<GameObject>>();      // groups
    public List<GameObject> R;                                          // Assigned once for all layouts
    public List<GameObject> Emphasis = new List<GameObject>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CollectWalls();
    }
    
    public void CollectWalls()
    {
        R = new List<GameObject>(GameObject.FindGameObjectsWithTag("Wall"));
    }
}
