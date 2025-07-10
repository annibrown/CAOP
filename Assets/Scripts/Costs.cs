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

    public GameObject tempEmphasis;
    
    public static GameObject emphasisPoint;
    public static bool reflectOverX = true;
    public static bool onWall = false;

    void Awake()
    {
        emphasisPoint = tempEmphasis;
    }
    
    public static float TotalCost(Layout layout, int iteration = -1)
    {
        float clearance = ClearanceViolation(layout);
        float circulation = Circulation(layout);
        float pDist = PairwiseDistance(layout);     // important
        float pAngle = PairwiseAngle(layout);       // important
        float cDist = ConversationDistance(layout); 
        float cAngle = ConversationAngle(layout);   
        float balance = Balance(layout);            // important
        float align = Alignment(layout);            // important
        float wallAlign = WallAlignment(layout);    // important
        float symmetry = Symmetry(layout);          // important
        float emphasis = Emphasis(layout);          // important

        //float total = symmetry * Parameters.w_symmetry;
        
        float total = clearance * Parameters.w_clearanceViolation +
                      circulation * Parameters.w_circulation +
                      pDist * Parameters.w_pairwiseDistance +
                      pAngle * Parameters.w_pairwiseAngle +
                      cDist * Parameters.w_conversationDistance +
                      cAngle * Parameters.w_conversationAngle +
                      balance * Parameters.w_balance +
                      align * Parameters.w_alignment +
                      wallAlign * Parameters.w_wallAlignment +
                      symmetry * Parameters.w_symmetry +
                      emphasis * Parameters.w_emphasis;

        // if (iteration >= 0)
        // {
        //     var values = new Dictionary<string, float>
        //     {
        //         { "Total", total },
        //         { "Clearance", clearance },
        //         { "Circulation", circulation },
        //         { "PairwiseDistance", pDist },
        //         { "PairwiseAngle", pAngle },
        //         { "ConversationDistance", cDist },
        //         { "ConversationAngle", cAngle },
        //         { "Balance", balance },
        //         { "Alignment", align },
        //         { "WallAlignment", wallAlign },
        //         { "Symmetry", symmetry },
        //         { "Emphasis", emphasis }
        //     };
        //
        //     GameObject.FindFirstObjectByType<CostLogger>()?.Log(iteration, values);
        // }

        return total;
    }


    // takes in a layout, composed of furniture F, walls R
    private static float ClearanceViolation(Layout layout)
    {
        clearanceCost = 0;
        List<GameObject> furniture = layout.F;

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
                    //Debug.Log($"[Overlap] {objA.name} ↔ {objB.name} | Area: {overlapArea:F3} m²");
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
                //Debug.Log($"[Boundary] {obj.name} exceeds room boundary. Outside area: {outsideArea:F3} m²");
            }
        }
        //Debug.Log("Clearance cost: " + clearanceCost);
        return clearanceCost;
    }

    // NEED TO MODIFY TO SUPPORT NON-RECTANGULAR ROOMS
    private static float Circulation(Layout layout)
    {
        FloorGridGenerator grid = GameObject.FindFirstObjectByType<FloorGridGenerator>();
        int regionCount = grid.CountWalkableRegions();
        //Debug.Log("circulation: " + (regionCount-1));
        return regionCount - 1;
    }
    
    private static float PairwiseDistance(Layout layout)
    {
        pairwiseCost = 0;

        for (int i = 0; i < layout.F.Count; i++)
        {
            for (int j = i + 1; j < layout.F.Count; j++)
            {
                if ((layout.F[i].CompareTag("Chair") && layout.F[j].CompareTag("Table")) ||
                    (layout.F[i].CompareTag("Table") && layout.F[j].CompareTag("Chair")))
                {
                    float m1 = UnitConverter.InchesToMeters(16.0f);
                    float m2 = UnitConverter.InchesToMeters(18.0f);

                    Renderer rendA = layout.F[i].GetComponentInChildren<Renderer>();
                    Renderer rendB = layout.F[j].GetComponentInChildren<Renderer>();
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
                        //Debug.Log("distance between objects "+ distanceBetweenObjects);
                    }

                    float t = Tfunction(distanceBetweenObjects, m1, m2, 2);
                    pairwiseCost += t;
                }
            }
        }
        //Debug.Log("Pairwise Distance: " + pairwiseCost*-1);
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
    
    // returns lower number if chair is looking at table more
    private static float PairwiseAngle(Layout layout)
    {
        float pairwiseAngleCost = 0;
        
        foreach (GameObject i in layout.F)
        {
            foreach (GameObject j in layout.F)
            {
                if ((i.CompareTag("Chair") && j.CompareTag("Table")))
                {
                    // make sure table is in front of chair
                    Vector3 forwardChair = i.transform.forward;
                    Vector3 toTable = (j.transform.position - i.transform.position).normalized;
                    float angleToTable = Vector3.Angle(forwardChair, toTable);
                    //Debug.Log("angle to table: " + angleToTable);
                    
                    pairwiseAngleCost += Mathf.Cos(angleToTable * Mathf.Deg2Rad);
                    //Debug.Log("this angle cost: " + pairwiseAngleCost);
                }
            }
        }
        //Debug.Log("Pairwise Angle: " + pairwiseAngleCost*-1);
        return pairwiseAngleCost * -1;
    }

    // calculates t value for if seats are within 4-8 feet of each other
    // AKA 1.2192 - 2.4384 meters
    private static float ConversationDistance(Layout layout)
    {
        cdCost = 0;
        for (int i = 0; i < layout.F.Count; i++)
        {
            for (int j = i + 1; j < layout.F.Count; j++)
            {
                // if both are seats
                if (layout.F[i].CompareTag("Chair") && layout.F[j].CompareTag("Chair"))
                {
                    distanceBetweenObjects = Vector3.Distance(layout.F[i].transform.position, layout.F[j].transform.position);
                    //Debug.Log("Distance(for cd): " + distanceBetweenObjects);
                    float t = Tfunction(distanceBetweenObjects, UnitConverter.InchesToMeters(4.0f * 12.0f), UnitConverter.InchesToMeters(8.0f * 12.0f), 2);
                    cdCost += t;
                    //Debug.Log("cd Cost: " + t);
                }
            }
        }
        //Debug.Log("Total Conversation Distance: " + cdCost*-1);
        return cdCost * -1;
    }

    private static float ConversationAngle(Layout layout)
    {
        caCost = 0;
        for (int i = 0; i < layout.F.Count; i++)
        {
            for (int j = i + 1; j < layout.F.Count; j++)
            {
                // if both are seats
                if (layout.F[i].CompareTag("Chair") && layout.F[j].CompareTag("Chair"))
                {
                    GameObject f = layout.F[i];
                    GameObject g = layout.F[j];
                    Vector3 forwardF = f.transform.forward;
                    Vector3 toG = (g.transform.position - f.transform.position).normalized;
                    float angleF = Vector3.Angle(forwardF, toG);

                    Vector3 forwardG = g.transform.forward;
                    Vector3 toF = (f.transform.position - g.transform.position).normalized;
                    float angleG = Vector3.Angle(forwardG, toF);
                    //Debug.Log("c angles: " + angleF + " " + angleG);
                    caCost += (Mathf.Cos(angleF * Mathf.Deg2Rad) + 1) *
                              (Mathf.Cos(angleG * Mathf.Deg2Rad) + 1);
                }
            }
        }
        //Debug.Log("Conversation Angle: " + caCost*-1);
        return caCost * -1;
    }

    private static float Balance(Layout layout)
    {
        float balanceCost = 0;
        
        Vector2 weightedPosition = new Vector2(0, 0);
        float totalArea = 0;

        foreach (var t in layout.F)
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

        Vector2 centerOfRoom = GetRoomCenterXZ(layout.R);

        balanceCost = (centerOfMass - centerOfRoom).magnitude;

        //Debug.Log("Balance: " + balanceCost);
        
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



    private static float Alignment(Layout layout)
    {
        float alignmentCost = 0;
        
        for (int i = 0; i < layout.F.Count; i++)
        {
            for (int j = i + 1; j < layout.F.Count; j++)
            {
                GameObject f = layout.F[i];
                GameObject g = layout.F[j];
                
                Vector3 global = Vector3.forward;
                
                Vector3 forwardF = f.transform.forward;
                float angleF = Vector3.Angle(forwardF, global);

                Vector3 forwardG = g.transform.forward;
                float angleG = Vector3.Angle(forwardG, global);
                
                alignmentCost += Mathf.Cos(4 * ((angleF * Mathf.Deg2Rad) - (angleG * Mathf.Deg2Rad)));
            }
        }
        //Debug.Log("Alignment cost: " + alignmentCost*-1);
        return alignmentCost * -1;
    }
    
    private static float WallAlignment(Layout layout)
    {
        float wallAlignmentCost = 0;
        
        foreach (GameObject f in layout.F)
        {
            GameObject nearestWall = layout.R[0];
            float minDistance = float.MaxValue;

            foreach (GameObject wall in layout.R)
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
        //Debug.Log("Wall alignment cost: " + (wallAlignmentCost * -1) );
        return wallAlignmentCost * -1;
    }

    private static float Emphasis(Layout layout)
    {
        float emphasisCost = 0;

        //foreach (List<GameObject> group in Layout.G)
        //{
            foreach (GameObject obj in layout.F)
            {
                if (obj.CompareTag("Chair"))
                {
                    Vector3 forward = obj.transform.forward;
                    Vector3 toEmphasis = (emphasisPoint.transform.position - obj.transform.position).normalized;
                    float angle = Vector3.Angle(forward, toEmphasis);

                    emphasisCost += Mathf.Cos(angle * Mathf.Deg2Rad);
                }
            }
        //}
        //Debug.Log("Emphasis: " + emphasisCost*-1);
        return emphasisCost * -1;
    }

    private static float Symmetry(Layout layout)
    {
        float symmetryCost = 0;
        float max = 0;
        
        // Check symmetry axis
        
        Collider[] collidersE = emphasisPoint.GetComponentsInChildren<Collider>();
        Bounds boundsE = collidersE[0].bounds;
        for (int k = 1; k < collidersE.Length; k++)
            boundsE.Encapsulate(collidersE[k].bounds);

        foreach (GameObject wall in layout.R)
        {
            Collider[] collidersW = wall.GetComponentsInChildren<Collider>();
            Bounds boundsW = collidersW[0].bounds;
            for (int l = 1; l < collidersW.Length; l++)
                boundsW.Encapsulate(collidersW[l].bounds);

            if (boundsE.Intersects(boundsW))
            {
                Debug.Log("COLLIDED!");
                onWall = true;
                
                // wall is longer along x axis
                if ((boundsW.max.x - boundsW.min.x) > (boundsW.max.z - boundsW.min.z))
                {
                    Debug.Log("Collide Z");
                    reflectOverX = false;
                }
                else
                {
                    Debug.Log("Collide X");
                    reflectOverX = true;
                }
            }
        }
        //foreach (List<GameObject> group in Layout.G)
        //{
        for (int i = 0; i < layout.F.Count; i++)
        {
            max = 0;
            bool maxAssigned = false;
            GameObject f = layout.F[i];
            for (int j = 0; j < layout.F.Count; j++)
            {
                GameObject g = layout.F[j];
                if (f.CompareTag(g.tag) && (i != j))
                {
                    float newMax = SFunction(f, g, emphasisPoint);
                    if (newMax > max || maxAssigned == false)
                    {
                        max = newMax;
                        maxAssigned = true;
                    }
                }
            }
            if (maxAssigned)
            {
                symmetryCost += max; 
            }
        }
        //}
        Debug.Log("Symmetry: " + symmetryCost*-1);
        Debug.Log(reflectOverX == true ? "Reflect over X" : "Reflect over Z");
        return symmetryCost * -1;
    }

    public static void SymmetricX()
    {
            //Debug.Log("X");
            reflectOverX = true;
    }

    public static void SymmetricZ()
    {
            //Debug.Log("Z");
            reflectOverX = false;
    }

    private static float SFunction(GameObject f, GameObject g, GameObject p)
    {
        Vector3 forwardF = f.transform.forward;
        Vector3 toEmphasis = (emphasisPoint.transform.position - f.transform.position).normalized;
        float angleF = Vector3.Angle(forwardF, toEmphasis);
        
        Vector3 reflectedPos = ReflectPosition(g.transform.position, p.transform.position, reflectOverX);
        Vector3 reflectedForward = ReflectDirection(g.transform.forward, reflectOverX);

        Vector3 toE = p.transform.position - reflectedPos;
        float angleG = Vector3.Angle(reflectedForward, toE);

        float combinedAngle = angleF - angleG;
        
        return Mathf.Cos(combinedAngle * Mathf.Deg2Rad) - Vector3.Distance(f.transform.position, reflectedPos);
    }
    
    private static Vector3 ReflectPosition(Vector3 original, Vector3 lineOrigin, bool overX)
    {
        Vector3 reflected = original;

        if (!overX)
        {
            // Reflect over vertical line at lineOrigin.x
            float dx = original.x - lineOrigin.x;
            reflected.x = lineOrigin.x - dx;
        }
        else
        {
            // Reflect over horizontal line at lineOrigin.z
            float dz = original.z - lineOrigin.z;
            reflected.z = lineOrigin.z - dz;
        }

        return reflected;
    }

    private static Vector3 ReflectDirection(Vector3 direction, bool overX)
    {
        Vector3 reflected = direction;

        if (!overX)
        {
            // Reflect over vertical line → flip x direction
            reflected.x = -reflected.x;
        }
        else
        {
            // Reflect over horizontal line → flip z direction
            reflected.z = -reflected.z;
        }

        return reflected;
    }
    
}    

