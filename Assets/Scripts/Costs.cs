using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

public class Costs : MonoBehaviour
{
    private static float clearanceCost;
    private static float pairwiseCost;
    private float cdCost;
    private float caCost;

    public float personRadius = UnitConverter.InchesToMeters(18.0f); // about 18 inches
    public float gridResolution = 0.1f; // how fine the walkable grid is
    public Vector3 roomMin; // room bounds
    public Vector3 roomMax;

    public GameObject frontWall;
    public GameObject backWall;
    public GameObject leftWall;
    public GameObject rightWall;

    private static float distanceBetweenObjects;


    public static float TotalCost()
    {
        ClearanceViolation();
        Circulation();
        PairwiseDistance();
        Debug.Log("Calculating total cost");
        return 1;
    }

    // takes in a layout, composed of furniture F, walls R
    private static float ClearanceViolation()
    {
        List<GameObject> furniture = Layout.F;

        // --- STEP 1: Furniture vs Furniture Overlap ---
        for (int i = 0; i < furniture.Count; i++)
        {
            GameObject objA = furniture[i];
            Collider[] collidersA = objA.GetComponentsInChildren<Collider>();
            if (collidersA.Length == 0) continue;

            // Merge bounds for A
            Bounds boundsA = collidersA[0].bounds;
            for (int k = 1; k < collidersA.Length; k++)
                boundsA.Encapsulate(collidersA[k].bounds);

            for (int j = i + 1; j < furniture.Count; j++)
            {
                GameObject objB = furniture[j];
                Collider[] collidersB = objB.GetComponentsInChildren<Collider>();
                if (collidersB.Length == 0) continue;

                // Merge bounds for B
                Bounds boundsB = collidersB[0].bounds;
                for (int l = 1; l < collidersB.Length; l++)
                    boundsB.Encapsulate(collidersB[l].bounds);

                if (boundsA.Intersects(boundsB))
                {
                    // Calculate 2D overlap (XZ plane)
                    float minX = Mathf.Max(boundsA.min.x, boundsB.min.x);
                    float maxX = Mathf.Min(boundsA.max.x, boundsB.max.x);
                    float minZ = Mathf.Max(boundsA.min.z, boundsB.min.z);
                    float maxZ = Mathf.Min(boundsA.max.z, boundsB.max.z);

                    float overlapX = Mathf.Max(0f, maxX - minX);
                    float overlapZ = Mathf.Max(0f, maxZ - minZ);
                    float overlapArea = overlapX * overlapZ;

                    clearanceCost += overlapArea;
                    Debug.Log($"[Overlap] {objA.name} ↔ {objB.name} | Area: {overlapArea:F3} m²");
                }
            }
        }

        // --- STEP 2: Furniture vs Room Boundary ---
        float roomWidth = UnitConverter.InchesToMeters(Parameters.floorSizeX);
        float roomDepth = UnitConverter.InchesToMeters(Parameters.floorSizeZ);
        Bounds roomBounds = new Bounds(Vector3.zero, new Vector3(roomWidth, 10f, roomDepth));

        foreach (GameObject obj in furniture)
        {
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0) continue;

            // Merge bounds
            Bounds merged = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++)
                merged.Encapsulate(colliders[i].bounds);

            // 2D bounds (XZ)
            float minX = merged.min.x;
            float maxX = merged.max.x;
            float minZ = merged.min.z;
            float maxZ = merged.max.z;

            float roomMinX = roomBounds.min.x;
            float roomMaxX = roomBounds.max.x;
            float roomMinZ = roomBounds.min.z;
            float roomMaxZ = roomBounds.max.z;

            float overlapMinX = Mathf.Max(minX, roomMinX);
            float overlapMaxX = Mathf.Min(maxX, roomMaxX);
            float overlapMinZ = Mathf.Max(minZ, roomMinZ);
            float overlapMaxZ = Mathf.Min(maxZ, roomMaxZ);

            float overlapWidth = Mathf.Max(0f, overlapMaxX - overlapMinX);
            float overlapDepth = Mathf.Max(0f, overlapMaxZ - overlapMinZ);

            float objectArea = (maxX - minX) * (maxZ - minZ);
            float overlapArea = overlapWidth * overlapDepth;

            float outsideArea = objectArea - overlapArea;

            if (outsideArea > 0.0001f)
            {
                clearanceCost += outsideArea;
                Debug.Log($"[Boundary] {obj.name} exceeds room boundary. Outside area: {outsideArea:F3} m²");
            }
        }
        Debug.Log("Clearance cost: " + clearanceCost);
        return clearanceCost;
    }





    // NEED TO MODIFY TO SUPPORT NON-RECTANGULAR ROOMS
    private static float Circulation()
    {
        FloorGridGenerator grid = GameObject.FindFirstObjectByType<FloorGridGenerator>();
        int regionCount = grid.CountWalkableRegions();
        Debug.Log("Number of connected empty regions: " + regionCount);
        return regionCount - 1;
    }
    
    private static float PairwiseDistance()
    {
        pairwiseCost = 0;

        for (int i = 0; i < Layout.F.Count; i++)
        {
            for (int j = i + 1; j < Layout.F.Count; j++)
            {
                if ((Layout.F[i].CompareTag("Chair") && Layout.F[j].CompareTag("Table")) ||
                    (Layout.F[i].CompareTag("Table") && Layout.F[j].CompareTag("Chair")))
                {
                    float m1 = UnitConverter.InchesToMeters(16.0f);
                    float m2 = UnitConverter.InchesToMeters(18.0f);

                    Renderer rendA = Layout.F[i].GetComponentInChildren<Renderer>();
                    Renderer rendB = Layout.F[j].GetComponentInChildren<Renderer>();
                    if (rendA == null || rendB == null) continue;

                    Bounds boundsA = rendA.bounds;
                    Bounds boundsB = rendB.bounds;

                    if (boundsA.Intersects(boundsB))
                    {
                        distanceBetweenObjects = 0f;
                    }
                    else
                    {
                        Vector3 pointA = boundsA.ClosestPoint(boundsB.center);
                        Vector3 pointB = boundsB.ClosestPoint(boundsA.center);
                        distanceBetweenObjects = Vector3.Distance(pointA, pointB);
                    }

                    float t = Tfunction(distanceBetweenObjects, m1, m2, 2);
                    pairwiseCost += t;

                    Debug.Log("Edge-to-edge distance: " + distanceBetweenObjects);
                    Debug.Log("T: " + t);
                }
            }
        }

        return pairwiseCost * -1;
    }


    private static float Tfunction(float d, float m1, float m2, float a)
    {
        if (d < m1)
        {
            return Mathf.Pow(d / m1, a);
        }
        else if ((m1 <= d) && (d <= m2))
        {
            return 1;
        }
        else if (d > m2)
        {
            return Mathf.Pow(m2 / d, a);
        }

        return 0;
    }

    public float ConversationDistance()
    {
        cdCost = 0;
        for (int i = 0; i < Layout.F.Count; i++)
        {
            for (int j = i + 1; j < Layout.F.Count; j++)
            {
                // if both are seats
                distanceBetweenObjects = Vector3.Distance(Layout.F[i].transform.position, Layout.F[j].transform.position);
                cdCost += Tfunction(distanceBetweenObjects, 4, 8, 2);
            }
        }

        return cdCost * -1;
    }

    public float ConversationAngle()
    {
        caCost = 0;
        for (int i = 0; i < Layout.F.Count; i++)
        {
            for (int j = i + 1; j < Layout.F.Count; j++)
            {
                // if both are seats
                GameObject f = Layout.F[i];
                GameObject g = Layout.F[j];
                Vector3 forwardF = f.transform.forward;
                Vector3 toG = (g.transform.position - f.transform.position).normalized;
                float angleF = Vector3.Angle(forwardF, toG);

                Vector3 forwardG = g.transform.forward;
                Vector3 toF = (f.transform.position - g.transform.position).normalized;
                float angleG = Vector3.Angle(forwardG, toF);

                caCost += (Mathf.Cos(angleF * Mathf.Deg2Rad) + 1) *
                          (Mathf.Cos(angleG * Mathf.Deg2Rad) + 1);
            }
        }
        return caCost * -1;
    }

    public float Balance()
    {
        return 0;
    }

    public float Alignment()
    {
        float alignmentCost = 0;
        
        for (int i = 0; i < Layout.F.Count; i++)
        {
            for (int j = i + 1; j < Layout.F.Count; j++)
            {
                GameObject f = Layout.F[i];
                GameObject g = Layout.F[j];
                    
            }
        }
        return alignmentCost * -1;
    }
    
    public float WallAlignment()
    {
        float wallAlignmentCost = 0;
        
        foreach (GameObject f in Layout.F)
        {
            GameObject nearestWall = Layout.R[0];
            float minDistance = float.MaxValue;

            foreach (GameObject wall in Layout.R)
            {
                float distance = Vector3.Distance(f.transform.position, wall.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestWall = wall;
                }
            }

            Vector3 wallDirection = nearestWall.transform.forward;
                
            Vector3 globalX = Vector3.right;
            float wallAngle = Vector3.SignedAngle(globalX, wallDirection, Vector3.up);
                
            Vector3 furnitureDirection = f.transform.forward;
            float furnitureAngle = Vector3.SignedAngle(globalX, furnitureDirection, Vector3.up);

            wallAlignmentCost += Mathf.Cos(4 * (furnitureAngle - wallAngle));
        }
        return wallAlignmentCost * -1;
    }
    
}    

