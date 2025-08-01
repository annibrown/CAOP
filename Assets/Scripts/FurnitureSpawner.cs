using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureSpawner : MonoBehaviour
{
    public GameObject chairToCreate;
    public GameObject tableToCreate;
    public GameObject emphasisToCreate;
    
    IEnumerator Start()
    {
        // Wait one frame to ensure Manager.Awake() has run
        yield return null;

        // for (int i = 0; i < Parameters.numberOfChairs; i++)
        // {
        //     Vector3 position = new Vector3(2.0f - (i * 1.0f), 0.4f, 0.0f);
        //     GameObject newChair = Instantiate(
        //         chairToCreate,
        //         position,
        //         Quaternion.Euler(0, 0, 0),
        //         Manager.currentLayout.transform
        //     );
        //     newChair.tag = "Chair";
        //     Manager.currentLayout.F.Add(newChair);
        // }
        
        for (int i = 0; i < Parameters.numberOfGroups; i++)
        {
            Manager.currentLayout.G.Add(new List<GameObject>());
        }
        
        Vector3 position = new Vector3(0.0f, 0.2f, 2.25f);
        GameObject newEmphasis = Instantiate(
            emphasisToCreate,
            position,
            Quaternion.Euler(0, 180, 0),
            Manager.currentLayout.transform
        );
        Furniture emphasisScript = newEmphasis.GetComponent<Furniture>();
        if (emphasisScript != null)
        {
            emphasisScript.prefabSource = emphasisToCreate;
        }
        Manager.currentLayout.Emphasis.Add(newEmphasis);
        
        position = new Vector3(0.0f, 0.0f, -2.25f);
        newEmphasis = Instantiate(
            emphasisToCreate,
            position,
            Quaternion.Euler(0, 180, 0),
            Manager.currentLayout.transform
        );
        emphasisScript = newEmphasis.GetComponent<Furniture>();
        if (emphasisScript != null)
        {
            emphasisScript.prefabSource = emphasisToCreate;
        }
        Manager.currentLayout.Emphasis.Add(newEmphasis);
        
        // FIRST GROUP
        // position = new Vector3(0.0f, 0.4f, 3.6f);
        position = new Vector3(-0.5f, 0.4f, 3f);
        GameObject newChair = Instantiate(
            chairToCreate,
            position,
            Quaternion.Euler(0, 45, 0),
            Manager.currentLayout.transform
        );
        newChair.tag = "Chair";
        Furniture chairScript = newChair.GetComponent<Furniture>();
        if (chairScript != null)
        {
            chairScript.prefabSource = chairToCreate;
        }
        Manager.currentLayout.F.Add(newChair);
        Manager.currentLayout.G[0].Add(newChair);
        
        //position = new Vector3(1.2f, 0.4f, 2.25f); // 1.2, 0.4, 0
        position = new Vector3(1f, 0.4f, 2.5f);
        newChair = Instantiate(
            chairToCreate,
            position,
            Quaternion.Euler(0, 10, 0), // -90
            Manager.currentLayout.transform
        );
        newChair.tag = "Chair";
        chairScript = newChair.GetComponent<Furniture>();
        if (chairScript != null)
        {
            chairScript.prefabSource = chairToCreate;
        }
        Manager.currentLayout.F.Add(newChair);
        Manager.currentLayout.G[0].Add(newChair);
        
        position = new Vector3(0.0f, 0.4f, 0.9f); // -1.2, 0.4, 0
        position = new Vector3(-0.5f, 0.4f, 0f);
        newChair = Instantiate(
            chairToCreate,
            position,
            Quaternion.Euler(0, 46, 0), // 90
            Manager.currentLayout.transform
        );
        newChair.tag = "Chair";
        chairScript = newChair.GetComponent<Furniture>();
        if (chairScript != null)
        {
            chairScript.prefabSource = chairToCreate;
        }
        Manager.currentLayout.F.Add(newChair);
        Manager.currentLayout.G[0].Add(newChair);
        
        
        // SPAWN TABLE
        for (int i = 0; i < Parameters.numberOfTables; i++)
        {
            GameObject newTable = Instantiate(
                tableToCreate,
                //new Vector3(0.0f, 0.2f, 2.25f),
                new Vector3(1f, 0.2f, 1f),
                //Quaternion.Euler(0, 0, 0),
                Quaternion.Euler(0, 184, 0),
                Manager.currentLayout.transform
            );
            newTable.tag = "Table";
            Furniture tableScript = newTable.GetComponent<Furniture>();
            if (tableScript != null)
            {
                tableScript.prefabSource = tableToCreate;
            }

            Manager.currentLayout.F.Add(newTable);
            Manager.currentLayout.G[0].Add(newTable);
        }
        
        // SECOND GROUP
        //position = new Vector3(-0.7f, 0.4f, -1.45f);
        position = new Vector3(-1f, 0.4f, -2f);
        newChair = Instantiate(
            chairToCreate,
            position,
            Quaternion.Euler(0, 59, 0),
            Manager.currentLayout.transform
        );
        newChair.tag = "Chair";
        chairScript = newChair.GetComponent<Furniture>();
        if (chairScript != null)
        {
            chairScript.prefabSource = chairToCreate;
        }
        Manager.currentLayout.F.Add(newChair);
        Manager.currentLayout.G[1].Add(newChair);
        
        //position = new Vector3(0.7f, 0.4f, -1.45f);
        position = new Vector3(-0.5f, 0.4f, 2f);
        newChair = Instantiate(
            chairToCreate,
            position,
            Quaternion.Euler(0, 128, 0),
            Manager.currentLayout.transform
        );
        newChair.tag = "Chair";
        chairScript = newChair.GetComponent<Furniture>();
        if (chairScript != null)
        {
            chairScript.prefabSource = chairToCreate;
        }
        Manager.currentLayout.F.Add(newChair);
        Manager.currentLayout.G[1].Add(newChair);
        
        //position = new Vector3(-0.7f, 0.4f, -3.05f);
        position = new Vector3(-0.5f, 0.4f, -3f);
        newChair = Instantiate(
            chairToCreate,
            position,
            Quaternion.Euler(0, -84, 0),
            Manager.currentLayout.transform
        );
        newChair.tag = "Chair";
        chairScript = newChair.GetComponent<Furniture>();
        if (chairScript != null)
        {
            chairScript.prefabSource = chairToCreate;
        }
        Manager.currentLayout.F.Add(newChair);
        Manager.currentLayout.G[1].Add(newChair);
        
        
        
        // SPAWN CHAIR TO MOVE
        position = new Vector3(2.5f, 0.4f, 0.0f);
        GameObject newChair2 = Instantiate(
            chairToCreate,
            position,
            Quaternion.Euler(0, -90, 0),
            Manager.currentLayout.transform
        );
        newChair2.tag = "Chair";
        Furniture chairScript2 = newChair2.GetComponent<Furniture>();
        if (chairScript2 != null)
        {
            chairScript2.prefabSource = chairToCreate;
        }
        Manager.currentLayout.F.Add(newChair2);
        
        HandTracker.targetObject = newChair2;
    
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
        //Debug.Log("Spawning chair");

        GameObject newChair = Instantiate(
            chairToCreate,
            new Vector3(0.0f, 0.4f, 0.0f),
            Quaternion.identity,
            Manager.currentLayout.transform // ✅ Parent to layout
        );

        newChair.tag = "Chair";
        Furniture chairScript = newChair.GetComponent<Furniture>();
        if (chairScript != null)
        {
            chairScript.prefabSource = chairToCreate;
        }
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
        Furniture tableScript = newTable.GetComponent<Furniture>();
        if (tableScript != null)
        {
            tableScript.prefabSource = tableToCreate;
        }

        Manager.currentLayout.F.Add(newTable);
    }
    
}
