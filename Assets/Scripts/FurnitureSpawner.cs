using System.Collections;
using UnityEngine;

public class FurnitureSpawner : MonoBehaviour
{
    public GameObject chairToCreate;
    public GameObject tableToCreate;
    
    void Start()
    {
        
        StartCoroutine(DelayedTileUpdate());

    }
    
    IEnumerator DelayedTileUpdate()
    {
        yield return null; // wait one frame for all GameObjects to initialize
        Manager.currentLayout.CollectWalls(); // just in case
        FloorGridGenerator grid = FindFirstObjectByType<FloorGridGenerator>();
        grid.UpdateTileColors(Manager.currentLayout);
    }

    public void SpawnChair()
    {
        Debug.Log("Spawning chair");
        
        GameObject newChair = Instantiate(chairToCreate, new Vector3(0.0f, 0.4f, 0.0f), Quaternion.identity);
        newChair.tag = "Chair";
        Manager.currentLayout.F.Add(newChair);
    }
    
    public void SpawnTable()
    {
        GameObject newTable = Instantiate(tableToCreate, new Vector3(0.0f, 0.2f, 0.0f), Quaternion.identity);
        newTable.tag = "Table";
        Manager.currentLayout.F.Add(newTable);
    }
    
}
