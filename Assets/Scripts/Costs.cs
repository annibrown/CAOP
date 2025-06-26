using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine.ProBuilder;

public class Costs : MonoBehaviour
{
    private static float clearanceCost;
    private static float pairwiseCost;
    private static float cdCost;
    private static float caCost;

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
        //ClearanceViolation();
        //Circulation();
        //PairwiseDistance();
        //ConversationDistance();
        //ConversationAngle();
        //Alignment();
        //WallAlignment();
        //Balance();
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

                    //Debug.Log("Edge-to-edge distance: " + distanceBetweenObjects);
                    //Debug.Log("T: " + t);
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

    // calculates t value for if seats are within 4-8 feet of each other
    // AKA 1.2192 - 2.4384 meters
    private static float ConversationDistance()
    {
        cdCost = 0;
        for (int i = 0; i < Layout.F.Count; i++)
        {
            for (int j = i + 1; j < Layout.F.Count; j++)
            {
                // if both are seats
                if (Layout.F[i].CompareTag("Chair") && Layout.F[j].CompareTag("Chair"))
                {
                    distanceBetweenObjects = Vector3.Distance(Layout.F[i].transform.position, Layout.F[j].transform.position);
                    //Debug.Log("Distance(for cd): " + distanceBetweenObjects);
                    float t = Tfunction(distanceBetweenObjects, UnitConverter.InchesToMeters(4.0f * 12.0f), UnitConverter.InchesToMeters(8.0f * 12.0f), 2);
                    cdCost += t;
                    //Debug.Log("cd Cost: " + t);
                }
            }
        }
        //Debug.Log("Total Conversation Distance: " + cdCost);
        return cdCost * -1;
    }

    private static float ConversationAngle()
    {
        caCost = 0;
        for (int i = 0; i < Layout.F.Count; i++)
        {
            for (int j = i + 1; j < Layout.F.Count; j++)
            {
                // if both are seats
                if (Layout.F[i].CompareTag("Chair") && Layout.F[j].CompareTag("Chair"))
                {
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
        }
        Debug.Log("Conversation Angle: " + caCost);
        return caCost * -1;
    }

    private static float Balance()
    {
        float balanceCost = 0;
        
        Vector2 weightedPosition = new Vector2(0, 0);
        float totalArea = 0;

        foreach (var t in Layout.F)
        {
            Vector2 footprint = GetBaseXZArea(t);
            float area = footprint.x * footprint.y;
            totalArea += area;
            
            float scaledX = t.transform.position.x * area;
            float scaledZ = t.transform.position.z * area;
            Vector2 scaled = new Vector2(scaledX, scaledZ);
            
            weightedPosition += scaled;
        }

        if (totalArea == 0)
        {
            return -1;
        }
        
        Vector2 centerOfMass = weightedPosition / totalArea;

        Vector2 centerOfRoom = GetRoomCenterXZ(Layout.R);

        balanceCost = (centerOfMass - centerOfRoom).magnitude;

        Debug.Log("Balance: " + balanceCost);
        
        return balanceCost;
    }
    
    private static Vector2 GetRoomCenterXZ(List<GameObject> roomObjects)
    {
        if (roomObjects == null || roomObjects.Count == 0)
            return Vector2.zero;

        Bounds combinedBounds = new Bounds(roomObjects[0].transform.position, Vector3.zero);

        foreach (GameObject obj in roomObjects)
        {
            Renderer r = obj.GetComponent<Renderer>();
            if (r != null)
            {
                combinedBounds.Encapsulate(r.bounds);
            }
        }

        Vector3 center3D = combinedBounds.center;
        return new Vector2(center3D.x, center3D.z);
    }

    
    private static Vector2 GetBaseXZArea(GameObject obj)
    {
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) return Vector2.zero;

        // If only one cube: just use its bounds
        if (renderers.Length == 1)
        {
            Bounds bounds = renderers[0].bounds;
            return new Vector2(bounds.size.x, bounds.size.z);
        }

        // If multiple cubes: find the one lowest to the ground
        MeshRenderer lowest = renderers[0];
        float minY = lowest.bounds.center.y - lowest.bounds.extents.y;

        foreach (MeshRenderer r in renderers)
        {
            float y = r.bounds.center.y - r.bounds.extents.y;
            if (y < minY)
            {
                minY = y;
                lowest = r;
            }
        }

        Bounds baseBounds = lowest.bounds;
        return new Vector2(baseBounds.size.x, baseBounds.size.z);
    }



    private static float Alignment()
    {
        float alignmentCost = 0;
        
        for (int i = 0; i < Layout.F.Count; i++)
        {
            for (int j = i + 1; j < Layout.F.Count; j++)
            {
                GameObject f = Layout.F[i];
                GameObject g = Layout.F[j];
                
                Vector3 global = Vector3.forward;
                
                Vector3 forwardF = f.transform.forward;
                float angleF = Vector3.Angle(forwardF, global);

                Vector3 forwardG = g.transform.forward;
                float angleG = Vector3.Angle(forwardG, global);
                
                alignmentCost += Mathf.Cos(4 * ((angleF * Mathf.Deg2Rad) - (angleG * Mathf.Deg2Rad)));
            }
        }
        Debug.Log("Alignment cost: " + alignmentCost);
        return alignmentCost * -1;
    }
    
    private static float WallAlignment()
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
            
            Vector3 global = Vector3.forward;
                
            Vector3 forwardF = f.transform.forward;
            float angleF = Vector3.Angle(forwardF, global);

            Vector3 wallDirection = nearestWall.transform.forward;
            float angleWall = Vector3.Angle(wallDirection, global);

            wallAlignmentCost += Mathf.Cos(4 * ((angleF * Mathf.Deg2Rad) - (angleWall * Mathf.Deg2Rad)));
        }
        Debug.Log("Wall alignment cost: " + (wallAlignmentCost * -1) );
        return wallAlignmentCost * -1;
    }
    
}    

