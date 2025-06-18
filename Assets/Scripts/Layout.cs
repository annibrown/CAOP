using UnityEngine;
using System.Collections.Generic;

public class Layout : MonoBehaviour
{
    
    public static List<GameObject> F = new List<GameObject>();                         // furniture
    public static List<GameObject> R = new List<GameObject>();                         // walls
    public static List<List<GameObject>> G = new List<List<GameObject>>();             // groups of furniture
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize all lists
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // runs before Start
    void Awake()
    {
        Layout.F.Clear();
        Layout.R.Clear();
        Layout.G.Clear();
        
        // initialize all lists
        
        
    }

}
