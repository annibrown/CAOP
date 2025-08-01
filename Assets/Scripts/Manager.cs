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
    private int furnitureGroup;
    
    private int layoutCounter = 0;
    
    public GameObject layoutGO; // the actual layout GameObject in the scene
    
    public static Manager current;
    
    void Awake()
    {
        if (currentLayout == null)
        {
            GameObject layoutGO = Instantiate(layoutPrefab);
            currentLayout = layoutGO.GetComponent<Layout>();
        }
        
        current = this;
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
        
        // start from messy layout
        floorGrid.UpdateTileColors(currentLayout);
        var costComponents = Costs.ComputeAllCosts(currentLayout);
        cost = costComponents["Total"];
        
        // set first layout as best layout
        bestLayout = currentLayout;
        bestCost = cost;
        
        // run MCMC
        for (int i = 0; i < Parameters.iterations; i++)
        {
            MCMC(i, false);
            yield return new WaitForSeconds(0.000001f);
        }
        Destroy(currentLayout.gameObject);

        // ✅ Activate and show best layout
        bestLayout.gameObject.SetActive(true);
        currentLayout = bestLayout;

        // run adjustment
        int furnitureGroupCounter = bestLayout.G[0].Count;
        for (int i = 0; i < bestLayout.F.Count - 1; i++)
        {
            if (i < furnitureGroupCounter)
            {
                CleanupAlignment(bestLayout.F[i], bestLayout.G[0], bestLayout);
            }
            else
            {
                CleanupAlignment(bestLayout.F[i], bestLayout.G[1], bestLayout);
            }
        }
        
        //wait for user input
        Vector3 chairPosition = currentLayout.F[7].transform.position;
        HandTracker.canMove = true;
        HandTracker.targetObject = currentLayout.F[7];
        while (currentLayout.F[7].transform.position == chairPosition)
        {
           // wait
           Debug.Log("WAITING FOR HAND GESTURE");
           yield return null;
        }
        Debug.Log("DETECTED HAND GESTURE");
        
        chairPosition = currentLayout.F[7].transform.position;
        if (chairPosition.z < 0)
        {
            // group 1
            //Debug.Log("Group 1");
            currentLayout.G[1].Add(currentLayout.F[7]);
            furnitureGroup = 1;
        }
        else
        {
            // group 0
            //Debug.Log("Group 0");
            currentLayout.G[0].Add(currentLayout.F[7]);
            furnitureGroup = 0;
        }
        
        // compute cost of first layout
        floorGrid.UpdateTileColors(currentLayout);
        
        costComponents = Costs.ComputeAllCosts(currentLayout);
        cost = costComponents["Total"];
        
        // set first layout as best layout
        //bestLayout = currentLayout;
        //bestCost = cost;
        
        for (int i = 0; i < Parameters.iterations; i++)
        {
            Debug.Log("BEFORE MCMC");
            MCMC(i, true);
            Debug.Log("AFTER MCMC");
            yield return new WaitForSeconds(0.000001f);
        }
        // END OF MCMC
        
        Destroy(currentLayout.gameObject);
        
        // ✅ Activate and show best layout
        bestLayout.gameObject.SetActive(true);
        currentLayout = bestLayout;
        
        // ✅ Optional: update layout name or reassign visibility
        bestLayout.name = "BestLayout";
        
        CleanupAlignment(bestLayout.F[7], bestLayout.G[furnitureGroup], bestLayout);
        //randomFurniture.transform.Rotate(Vector3.up, 180f);
        //Debug.Log("Cleaned up Results");
        
        HandTracker.targetObject = bestLayout.F[7];
        //HandTracker.canMove = true;
        
    }
    
    private IEnumerator WaitForUserMove()
    {
        HandTracker.canMove = true;

        Vector3 initialPosition = currentLayout.F[7].transform.position;

        // Wait until the object has moved (in a frame-by-frame non-blocking way)
        while (Vector3.Distance(currentLayout.F[7].transform.position, initialPosition) < 0.01f)
        {
            yield return null;  // Wait 1 frame
        }

        HandTracker.canMove = false;
    }


    void MCMC (int i, bool secondRun)
    {
        Debug.Log("SECOND RUN 1: " + secondRun);
        // make new layout newLayout
        Debug.Log($"currentLayout.F.Count: {currentLayout.F.Count}");
        newLayout = DuplicateLayout(currentLayout); 
        //Debug.Log($"newLayout.G.Count: {newLayout.G.Count}, Emphasis.Count: {newLayout.Emphasis.Count}"); 
        //Debug.Log($"newLayout.F.Count: {newLayout.F.Count}");
        //Debug.Log($"G[0].Count: {newLayout.G[0].Count}, G[1].Count: {newLayout.G[1].Count}");
            
            // TESTING
            //modification = Parameters.modifications[i];
            //randomFurniture = newLayout.F[Parameters.fIndex[i]];
            
        // modify newLayout
        modification = Random.Range(0, 2);
            
            // FOR PICKING RANDOM FURNITURE
            // randomFurnitureIndex = Random.Range(0, newLayout.F.Count);
            // randomFurniture = newLayout.F[randomFurnitureIndex];
            Debug.Log("SECOND RUN 2: " + secondRun);
            // FOR LOOPING THROUGH FURNITURE SYSTEMATICALLY
            if (!secondRun)
            {
                Debug.Log("PICKING EACH FURNITURE");
                int furnitureCount = newLayout.F.Count - 1;
                int x = Parameters.iterations / (furnitureCount * Parameters.roundsPerObject);
                x = Mathf.Max(1, x);
                int currentIndex = (i / x) % furnitureCount;
                randomFurniture = newLayout.F[currentIndex];
            }
            
            // TESTING ONE FURNITURE AT A TIME
            if (secondRun)
            {
                Debug.Log("ONLY PICKING CHAIR");
                randomFurniture = newLayout.F[7];
            }
            
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
                    float roomWidth = Parameters.floorSizeX;
                    float roomDepth = Parameters.floorSizeZ;
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
                        //Debug.Log($"✅ Move applied on attempt {attempt + 1}");
                        moveApplied = true;
                        break;
                    }
                    
                    //Debug.Log("Proposed Position: " + (randomFurniture.transform.position + proposedMove));
                }

                // if (!moveApplied)
                // {
                //     Debug.Log("❌ No valid move found after max attempts");
                // }

                //randomFurniture.transform.position += new Vector3(Parameters.xPositions[i], 0, Parameters.zPositions[i]);
                //Debug.Log("Moved Furniture");
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
                
                //Debug.Log("Rotated Furniture, new position: " + newY);
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
            
            var newCostComponents = Costs.ComputeAllCosts(newLayout);
            newCost = newCostComponents["Total"];
            
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
                
                newCostComponents["AcceptanceProbability"] = acceptanceProbability;
                newCostComponents["Iteration"] = i;
                
                // ✅ Only log accepted layouts
                FindFirstObjectByType<CostLogger>().Log(i, newCostComponents);
                
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
    }
    
    void CleanupAlignment(GameObject target, List<GameObject> group, Layout layout)
    {
        GameObject nearestWall = null;
        float minWallDist = float.MaxValue;
        
        // SYMMETRY CHECK

        // find angle of nearest wall
        foreach (GameObject wall in layout.R)
        {
            float dist = Vector3.Distance(target.transform.position, wall.transform.position);
            if (dist < minWallDist)
            {
                minWallDist = dist;
                nearestWall = wall;
            }
        }

        if (nearestWall == null) return;

        // Step 2: Get current directions
        Vector3 forwardTarget = target.transform.forward;
        Vector3 forwardWall = nearestWall.transform.forward;

        float angle = Vector3.SignedAngle(forwardTarget, forwardWall, Vector3.up);
        float absAngle = Mathf.Abs(angle);

        // do smallest rotation so that item is parallel or perpendicular to that item
        float targetAngle = angle % 90f;
        if (targetAngle > 45f)
        {
            targetAngle = -1 * (90 - targetAngle);
        }

        if (targetAngle < -45f)
        {
            targetAngle = 90 + targetAngle;
        }
        //Debug.Log("Clean up Rotation: " + targetAngle);

        // Step 4: Apply rotation
        target.transform.Rotate(Vector3.up, targetAngle);

        // Step 5: Snap to line (assume alignment along X or Z)
        // find object that is most aligned with it in both directions, and set it to that
        
        float minDistX = float.MaxValue;
        float minDistZ = float.MaxValue;
        
        foreach (GameObject other in group)
        {
            if (other == target) continue;
            float distX = other.transform.position.x - target.transform.position.x;
            float distZ = other.transform.position.z - target.transform.position.z;
            if (distX < Parameters.maxDist && distX > -1 * Parameters.maxDist)
            {
                if (distX < minDistX) minDistX = distX;
            }
            if (distZ < Parameters.maxDist && distZ > -1 * Parameters.maxDist)
            {
                if (distZ < minDistZ) minDistZ = distZ;
            }
        }
        
        Vector3 pos = target.transform.position;

        if (minDistX < float.MaxValue)
        {
            pos.x += minDistX;
            //Debug.Log("Clean up x Position: " + minDistX);
        }

        if (minDistZ < float.MaxValue)
        {
            pos.z += minDistZ;
            //Debug.Log("Clean up z Position: " + minDistZ);
        }

        target.transform.position = pos;
        
        // MAYBE: rotate 90 degress and check cost
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
        newLayout.G = new List<List<GameObject>>();
        newLayout.Emphasis = new List<GameObject>();

        Dictionary<GameObject, GameObject> cloneMap = new();

        Debug.Log($"original.F.Count: {original.F.Count}");
        
        foreach (GameObject f in original.F)
        {
            if (f == null)
            {
                Debug.Log("FURNITURE IS NULL");
            };
            
            Furniture furniture = f.GetComponent<Furniture>(); // THIS LINE
            GameObject prefab = furniture != null ? furniture.prefabSource : f;

            GameObject copy = Instantiate(
                prefab,
                f.transform.position,
                f.transform.rotation,
                newGO.transform
            );

            copy.transform.localScale = f.transform.localScale;

            Furniture newFurniture = copy.GetComponent<Furniture>();
            if (newFurniture != null && furniture != null)
            {
                newFurniture.prefabSource = furniture.prefabSource;
            }

            copy.tag = f.tag;

            newLayout.F.Add(copy);
            cloneMap[f] = copy; 
        }
        
        // Duplicate groups
        foreach (List<GameObject> group in original.G)
        {
            List<GameObject> newGroup = new List<GameObject>();
            foreach (GameObject member in group)
            {
                if (cloneMap.ContainsKey(member))
                    newGroup.Add(cloneMap[member]);
            }
            newLayout.G.Add(newGroup);
        }

        // Duplicate emphasis objects
        foreach (GameObject e in original.Emphasis)
        {
            GameObject copy = Instantiate(e, e.transform.position, e.transform.rotation, newGO.transform);
            copy.transform.localScale = e.transform.localScale;
            newLayout.Emphasis.Add(copy);
        }

        newLayout.R = original.R;

        return newLayout;
    }









    
}
