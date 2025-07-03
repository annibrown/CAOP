using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public FloorGridGenerator floorGrid; // assign this in the Inspector
    private float cost;
    private float newCost;

    public GameObject layoutPrefab;
    
    public static Layout currentLayout;
    public static Layout newLayout;

    private int modification;
    private GameObject randomFurniture;
    private GameObject swappedFurniture;
    private int randomFurnitureIndex;
    private int swappedFurnitureIndex;
    
    public GameObject layoutGO; // the actual layout GameObject in the scene
    
    void Awake()
    {
        if (currentLayout == null)
        {
            GameObject layoutGO = Instantiate(layoutPrefab);
            currentLayout = layoutGO.GetComponent<Layout>();
        }
    }


    public void StartCalculation()
    {
        StartCoroutine(Calculate());
    }

    
    IEnumerator Calculate()
    {
        if (currentLayout.F.Count == 0)
        {
            Debug.Log("Add furniture before calculation");
            yield break;
        }
        
        // compute cost of first layout
        floorGrid.UpdateTileColors(currentLayout);
        cost = Costs.TotalCost(currentLayout);

        for (int i = 0; i < Parameters.iterations; i++)
        {
            // make new layout newLayout
            newLayout = DuplicateLayout(currentLayout);
            
            // modify newLayout
            modification = Random.Range(0, 2);
            //modification = Parameters.modifications[i];
            
            randomFurnitureIndex = Random.Range(0, newLayout.F.Count);
            Debug.Log("Random Furniture Index: " + randomFurnitureIndex);
            randomFurniture = newLayout.F[randomFurnitureIndex];
            //randomFurniture = newLayout.F[Parameters.fIndex[i]];
            
            if (modification == 0)
            {
                // move randomFurniture
                randomFurniture.transform.position += new Vector3(RandomGaussian(0f, Parameters.position), 0, RandomGaussian(0f, Parameters.position));
                //randomFurniture.transform.position += new Vector3(Parameters.xPositions[i], 0, Parameters.zPositions[i]);
                Debug.Log("Moved Furniture");
            }
            else if (modification == 1)
            {
                // rotate randomFurniture
                randomFurniture.transform.Rotate(0f, RandomGaussian(0f, Parameters.rotation), 0f);
                //randomFurniture.transform.Rotate(0f, Parameters.rotations[i], 0f);
                Debug.Log("Rotated Furniture");
            }
            else
            {
                Debug.Log("MODIFICATION ERROR!");
            }

            // compute cost of new layout, newCost
            newCost = Costs.TotalCost(newLayout);
            
            // setting beta to 1
            float densityRatio = Mathf.Exp(-newCost + cost);
            float acceptanceProbability = Mathf.Min(1f, densityRatio);

            // Store old layout reference BEFORE replacing
            Layout oldLayout = currentLayout;

            if (Random.value < acceptanceProbability)
            {
                Debug.Log("LAYOUT ACCEPTED!");
                currentLayout = newLayout;
                cost = newCost;
                layoutGO = currentLayout.gameObject;
    
                // ✅ Now it's safe to destroy the previous layout
                if (oldLayout != null && oldLayout != newLayout)
                {
                    Destroy(oldLayout.gameObject);
                }
            }
            else
            {
                // ❌ Reject new layout
                Destroy(newLayout.gameObject);
            }

            yield return new WaitForSeconds(0.25f);
            //Debug.Log("4: currentLayout has " + currentLayout.F.Count + " items");
        }
    }

    public static float RandomGaussian(float mean, float stdDev)
    {
        float u1 = 1.0f - Random.value;
        float u2 = 1.0f - Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }

    private float Mcmc()
    {
        return 0;
    }

    
    public Layout DuplicateLayout(Layout original)
    {
        GameObject newGO = Instantiate(layoutPrefab); // layoutPrefab should be empty!
        Layout newLayout = newGO.GetComponent<Layout>();

        newLayout.F = new List<GameObject>();

        foreach (GameObject f in original.F)
        {
            GameObject copy = Instantiate(f, newGO.transform);
            newLayout.F.Add(copy);
        }

        newLayout.R = original.R; // shared wall references
        return newLayout;
    }






    
}
