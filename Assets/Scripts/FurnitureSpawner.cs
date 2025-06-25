using System.Collections;
using UnityEngine;

public class FurnitureSpawner : MonoBehaviour
{
    public GameObject chairToCreate;
    public GameObject tableToCreate;
    
    void Start()
    {
        // create chairs
        for (int i = 0; i < Parameters.numberOfChairs; i++)
        {
            Vector3 position = new Vector3(1.0f - (i * 2.0f), 0.4f, 1.0f + (0.0f * i));
            float angle = 0.0f + (i * 0.0f); // example: 0°, 20°, 40°, 60°, etc.

            GameObject newChair = Instantiate(
                chairToCreate,
                position,
                Quaternion.Euler(0, angle, 0)
            );

            newChair.tag = "Chair";
            Layout.F.Add(newChair);
        }

        
        // create tables
        for (int i = 0; i < Parameters.numberOfTables; i++)
        {
            GameObject newTable = Instantiate(tableToCreate, new Vector3(1.2f, 0.2f, 2.0f), Quaternion.identity);
            newTable.tag = "Table";
            Layout.F.Add(newTable);
        }
        
        StartCoroutine(DelayedTileUpdate());

    }
    
    IEnumerator DelayedTileUpdate()
    {
        yield return null; // wait one frame for all GameObjects to initialize
        Layout.CollectWalls(); // just in case
        FloorGridGenerator grid = FindFirstObjectByType<FloorGridGenerator>();
        grid.UpdateTileColors();
    }
    
}
