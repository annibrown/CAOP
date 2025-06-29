using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Layout : MonoBehaviour
{
    
    public static List<GameObject> F = new List<GameObject>();      // furniture
    public static List<GameObject> R = new List<GameObject>();      // walls
    public static List<List<GameObject>> G = new List<List<GameObject>>();      // groups
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CollectWalls();
    }
    
    // runs before Start
    void Awake()
    {
        Layout.F.Clear();
        Layout.R.Clear();
        
        // initialize all lists
    }
    
    public static void CollectWalls()
    {
        R = new List<GameObject>(GameObject.FindGameObjectsWithTag("Wall"));
    }

}
