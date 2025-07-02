using System.Collections;
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
        Debug.Log("First Layout Cost: " + cost);

        for (int i = 0; i < Parameters.iterations; i++)
        {
            // make new layout newLayout
            newLayout = DuplicateLayout(currentLayout);
            // modify newLayout
            if (newLayout.F.Count > 1)
            {
                modification = Random.Range(0, 3);
            }
            else
            {
                modification = Random.Range(0, 2);
            }

            randomFurnitureIndex = Random.Range(0, newLayout.F.Count);
            randomFurniture = newLayout.F[randomFurnitureIndex];
            if (modification == 0)
            {
                // move randomFurniture
                randomFurniture.transform.position +=
                    new Vector3(RandomGaussian(0f, Parameters.position), 0, RandomGaussian(0f, Parameters.position));
                Debug.Log("Moved Furniture");
            }
            else if (modification == 1)
            {
                // rotate randomFurniture
                randomFurniture.transform.Rotate(0f, RandomGaussian(0f, Parameters.rotation), 0f);
                Debug.Log("Rotated Furniture");
            }
            else if (modification == 2)
            {
                // swap two furniture
                swappedFurnitureIndex = Random.Range(0, newLayout.F.Count);
                while (swappedFurnitureIndex == randomFurnitureIndex)
                {
                    swappedFurnitureIndex = Random.Range(0, newLayout.F.Count);
                }

                swappedFurniture = newLayout.F[swappedFurnitureIndex];

                Vector3 posRandom = randomFurniture.transform.position;
                Quaternion rotRandom = randomFurniture.transform.rotation;

                randomFurniture.transform.position = swappedFurniture.transform.position;
                randomFurniture.transform.rotation = swappedFurniture.transform.rotation;

                swappedFurniture.transform.position = posRandom;
                swappedFurniture.transform.rotation = rotRandom;
                Debug.Log("Swapped Furniture");
            }
            else
            {
                Debug.Log("MODIFICATION ERROR!");
            }

            // compute cost of new layout, newCost
            newCost = Costs.TotalCost(newLayout);
            float acceptanceProbability = Mathf.Min(1f, Density(newCost) / Density(cost));
            
            // keep the better layout in currentLayout
            if (Random.Range(0f, 1f) < acceptanceProbability)
            {
                Debug.Log("LAYOUT ACCEPTED!");
                Debug.Log("Accepted layout Cost: " + newCost);
                // Clean up old layout GameObject
                Destroy(currentLayout.gameObject); // Destroys old layout from scene
                currentLayout = newLayout;
                layoutGO = currentLayout.gameObject; // Track new layout GameObject
                cost = newCost;
                
                Debug.Log("Accepted new layout at iteration " + i);
                foreach (var f in currentLayout.F)
                {
                    Debug.Log("  Furniture at: " + f.transform.position);
                }
                
            }
            else
            {
                // Rejected, discard new layout
                Debug.Log("LAYOUT REJECTED!");
                Destroy(newLayout.gameObject);
            }
            newLayout.F.Clear(); // In case prefab was saved with children

            yield return new WaitForSeconds(0.25f);
        }
    }

    public static float RandomGaussian(float mean, float stdDev)
    {
        float u1 = 1.0f - Random.value;
        float u2 = 1.0f - Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
    
    // setting beta to 1
    // Z cancels out in MCMC later so we omit it
    private float Density(float layoutCost)
    {
        return Mathf.Exp(-1 * layoutCost);
    }

    private float Mcmc()
    {
        return 0;
    }

    
    public Layout DuplicateLayout(Layout original)
    {
        GameObject newGO = Instantiate(layoutPrefab);
        Layout newLayout = newGO.GetComponent<Layout>();

        foreach (GameObject f in original.F)
        {
            GameObject copy = Instantiate(f, newGO.transform); // Parent to layout GameObject
            newLayout.F.Add(copy);
        }

        newLayout.R = original.R;

        return newLayout;
    }


    
}
