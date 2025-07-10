using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class Manager : MonoBehaviour
{
    public FloorGridGenerator floorGrid; // assign this in the Inspector
    private float cost;
    private float newCost;
    private float bestCost;

    public GameObject layoutPrefab;
    
    public static Layout currentLayout;
    public static Layout newLayout;
    public static Layout bestLayout;

    private int modification;
    private GameObject randomFurniture;
    private GameObject swappedFurniture;
    private int randomFurnitureIndex;
    private int swappedFurnitureIndex;
    
    private int layoutCounter = 0;
    
    public GameObject layoutGO; // the actual layout GameObject in the scene
    
    void Awake()
    {
        if (currentLayout == null)
        {
            GameObject layoutGO = Instantiate(layoutPrefab);
            currentLayout = layoutGO.GetComponent<Layout>();
        }
    }

    public void Testing()
    {
        randomFurniture = currentLayout.F[4];
        randomFurniture.transform.Rotate(0f, 90f, 0f);
        Debug.Log("Cube rotation: " + transform.rotation.eulerAngles);
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
        
        // set first layout as best layout
        bestLayout = currentLayout;
        bestCost = cost;

        for (int i = 0; i < Parameters.iterations; i++)
        {
            // make new layout newLayout
            newLayout = DuplicateLayout(currentLayout);
            
            // TESTING
            //modification = Parameters.modifications[i];
            //randomFurniture = newLayout.F[Parameters.fIndex[i]];
            
            // modify newLayout
            modification = Random.Range(0, 2);
            
            // FOR PICKING RANDOM FURNITURE
            // randomFurnitureIndex = Random.Range(0, newLayout.F.Count);
            // randomFurniture = newLayout.F[randomFurnitureIndex];
            
            // FOR LOOPING THROUGH FURNITURE SYSTEMATICALLY
            int furnitureCount = newLayout.F.Count;
            int x = Parameters.iterations / (furnitureCount * Parameters.roundsPerObject);
            x = Mathf.Max(1, x);
            int currentIndex = (i / x) % furnitureCount;
            randomFurniture = newLayout.F[currentIndex];
            
            // TESTING ONE FURNITURE AT A TIME
            //randomFurniture = newLayout.F[4];
            
            if (modification == 0)
            {
                // move randomFurniture

                int maxAttempts = 10;
                bool moveApplied = false;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    float xTranslation = RandomGaussian(0f, Parameters.position);
                    float zTranslation = RandomGaussian(0f, Parameters.position);
                    Vector3 proposedMove = new Vector3(xTranslation, 0, zTranslation);

                    // Get current bounds
                    Collider[] colliders = randomFurniture.GetComponentsInChildren<Collider>();
                    if (colliders.Length == 0) break;

                    Bounds currentBounds = colliders[0].bounds;
                    for (int j = 1; j < colliders.Length; j++)
                        currentBounds.Encapsulate(colliders[j].bounds);

                    // Predict new bounds after move
                    Bounds movedBounds = new Bounds(currentBounds.center + proposedMove, currentBounds.size);

                    // Room bounds (centered at origin)
                    float roomWidth = UnitConverter.InchesToMeters(Parameters.floorSizeX);
                    float roomDepth = UnitConverter.InchesToMeters(Parameters.floorSizeZ);
                    Bounds roomBounds = new Bounds(Vector3.zero, new Vector3(roomWidth, 10f, roomDepth));

                    // Check corners of moved bounds
                    Vector3[] corners =
                    {
                        new Vector3(movedBounds.min.x, 0, movedBounds.min.z),
                        new Vector3(movedBounds.min.x, 0, movedBounds.max.z),
                        new Vector3(movedBounds.max.x, 0, movedBounds.min.z),
                        new Vector3(movedBounds.max.x, 0, movedBounds.max.z)
                    };

                    bool insideRoom = true;
                    foreach (Vector3 corner in corners)
                    {
                        if (!roomBounds.Contains(corner))
                        {
                            insideRoom = false;
                            break;
                        }
                    }

                    if (insideRoom)
                    {
                        randomFurniture.transform.position += proposedMove;
                        Debug.Log($"✅ Move applied on attempt {attempt + 1}");
                        moveApplied = true;
                        break;
                    }
                }

                if (!moveApplied)
                {
                    Debug.Log("❌ No valid move found after max attempts");
                }

                //randomFurniture.transform.position += new Vector3(Parameters.xPositions[i], 0, Parameters.zPositions[i]);
                Debug.Log("Moved Furniture");
            }
            else if (modification == 1)
            {
                // rotate randomFurniture
                //randomFurniture.transform.Rotate(0f, RandomGaussian(0f, Parameters.rotation), 0f);
                
                Vector3 currentEuler = randomFurniture.transform.rotation.eulerAngles;
                float newY = (currentEuler.y + RandomGaussian(0f, Parameters.rotation)) % 360f;
                
                randomFurniture.transform.rotation = Quaternion.Euler(0, newY, 0);
                
                // TESTING
                // Vector3 currentEuler = randomFurniture.transform.rotation.eulerAngles;
                // float newY = (currentEuler.y + Parameters.rotations[i]) % 360f;
                //
                // randomFurniture.transform.rotation = Quaternion.Euler(0, newY, 0);
                
                Debug.Log("Rotated Furniture");
            }
            else
            {
                Debug.Log("MODIFICATION ERROR!");
            }

            Physics.SyncTransforms();
            
            Rigidbody rb = randomFurniture.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;  // Prevent physics-based movement
                rb.freezeRotation = true;  // Prevent Unity physics from updating rotation
                rb.useGravity = false;
            }
            
            // compute cost of new layout, newCost
            floorGrid.UpdateTileColors(newLayout);
            newCost = Costs.TotalCost(newLayout, i);
            
            // setting beta to 1
            float densityRatio = Mathf.Exp(-newCost + cost);
            float acceptanceProbability = Mathf.Min(1f, densityRatio);
            
            // Store old layout reference BEFORE replacing
            Layout oldLayout = currentLayout;
            
            // show new layout
            //newLayout.gameObject.SetActive(true);
            //currentLayout.gameObject.SetActive(false);
            //yield return new WaitForSeconds(0.25f);
            
            // check if new cost is better than best cost
            if (newCost < bestCost)
            {
                Layout oldBestLayout = bestLayout;
                bestCost = newCost;
                bestLayout = newLayout;
                Destroy(oldBestLayout.gameObject);
            }

            if (Random.value < acceptanceProbability)
            {
                Debug.Log("LAYOUT "+ i + " ACCEPTED!");
                
                currentLayout = newLayout;
                currentLayout.gameObject.SetActive(true);
                
                // Set a name or log to confirm selection
                cost = newCost;
                
                var values = new Dictionary<string, float>
                {
                    { "Cost", newCost },
                    { "Acceptance Probability", acceptanceProbability }
                };
                
                // ✅ Only log accepted layouts
                FindFirstObjectByType<CostLogger>().Log(i, values);
                
                layoutGO = currentLayout.gameObject;
    
                // ✅ Now it's safe to destroy the previous layout
                if (oldLayout != null && oldLayout != currentLayout && oldLayout != bestLayout)
                {
                    Destroy(oldLayout.gameObject);
                }
            }
            else
            {
                // ❌ Reject new layout
                Debug.Log("LAYOUT " + i + " REJECTED!");
                //newLayout.gameObject.SetActive(false);
                //currentLayout.gameObject.SetActive(true);
                Destroy(newLayout.gameObject);
            }

            yield return new WaitForSeconds(0.000001f);
        }
        
        // destroy current layout, so only show best layout
        Destroy(currentLayout.gameObject);
    }

    public static float RandomGaussian(float mean, float stdDev)
    {
        float u1 = 1.0f - Random.value;
        float u2 = 1.0f - Random.value;
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
        return mean + stdDev * randStdNormal;
    }
    
    private Layout DuplicateLayout(Layout original)
    {
        GameObject newGO = Instantiate(layoutPrefab);
        newGO.name = "Layout_" + layoutCounter++; // ✅ Unique name

        Layout newLayout = newGO.GetComponent<Layout>();
        newLayout.F = new List<GameObject>();

        foreach (GameObject f in original.F)
        {
            Furniture furniture = f.GetComponent<Furniture>();
            GameObject prefab = furniture != null ? furniture.prefabSource : f;

            GameObject copy = Instantiate(
                prefab,
                f.transform.position,
                f.transform.rotation,
                newGO.transform
            );

            // Copy scale manually
            copy.transform.localScale = f.transform.localScale;

            // ✅ Set the prefab source again for next generation
            Furniture newFurniture = copy.GetComponent<Furniture>();
            if (newFurniture != null && furniture != null)
            {
                newFurniture.prefabSource = furniture.prefabSource;
            }
            
            copy.tag = f.tag;

            newLayout.F.Add(copy);
        }

        newLayout.R = original.R;

        return newLayout;
    }









    
}
