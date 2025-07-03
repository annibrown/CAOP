using System.Collections;
using UnityEngine;

public class FurnitureSpawner : MonoBehaviour
{
    public GameObject chairToCreate;
    public GameObject tableToCreate;
    
    IEnumerator Start()
    {
        // Wait one frame to ensure Manager.Awake() has run
        yield return null;

        for (int i = 0; i < Parameters.numberOfChairs; i++)
        {
            Vector3 position = new Vector3(2.0f - (i * 1.0f), 0.4f, 0.0f);
            GameObject newChair = Instantiate(
                chairToCreate,
                position,
                Quaternion.Euler(0, 0, 0),
                Manager.currentLayout.transform
            );
            newChair.tag = "Chair";
            Manager.currentLayout.F.Add(newChair);
        }

        for (int i = 0; i < Parameters.numberOfTables; i++)
        {
            GameObject newTable = Instantiate(
                tableToCreate,
                new Vector3(1.0f, 0.2f, 2.0f),
                Quaternion.identity,
                Manager.currentLayout.transform
            );
            newTable.tag = "Table";
            Manager.currentLayout.F.Add(newTable);
        }

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

        GameObject newChair = Instantiate(
            chairToCreate,
            new Vector3(0.0f, 0.4f, 0.0f),
            Quaternion.identity,
            Manager.currentLayout.transform // ✅ Parent to layout
        );

        newChair.tag = "Chair";
        Manager.currentLayout.F.Add(newChair);
    }

    public void SpawnTable()
    {
        GameObject newTable = Instantiate(
            tableToCreate,
            new Vector3(0.0f, 0.2f, 0.0f),
            Quaternion.identity,
            Manager.currentLayout.transform // ✅ Parent to layout
        );

        newTable.tag = "Table";
        Manager.currentLayout.F.Add(newTable);
    }

    
}
