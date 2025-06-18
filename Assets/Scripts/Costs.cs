using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

public class Costs : MonoBehaviour
{
    public float clearanceCost;
    public float pairwiseCost;
    public float cdCost;
    public float caCost;

    public float personRadius = 0.45f; // about 18 inches
    public float gridResolution = 0.1f; // how fine the walkable grid is
    public Vector3 roomMin; // room bounds
    public Vector3 roomMax;

    public GameObject frontWall;
    public GameObject backWall;
    public GameObject leftWall;
    public GameObject rightWall;

    public float distanceBetweenObjects;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // takes in a layout, composed of furniture F, walls R, and groups of furniture G
    public float ClearanceViolation()
    {
        clearanceCost = 0;

        List<GameObject> combined = Layout.F.Concat(Layout.R).ToList();

        for (int i = 0; i < combined.Count; i++)
        {
            Collider fCollider = combined[i].GetComponent<Collider>();
            if (fCollider == null)
            {
                continue;
            }

            for (int j = i + 1; j < combined.Count; j++)
            {
                Collider gCollider = combined[j].GetComponent<Collider>();
                if (gCollider == null)
                {
                    continue;
                }

                Bounds f = fCollider.bounds;
                Bounds g = gCollider.bounds;

                if (f.Intersects(g))
                {
                    float overlapX = Mathf.Max(0, Mathf.Min(f.max.x, g.max.x) - Mathf.Max(0, f.min.x, g.min.x));
                    float overlapZ = Mathf.Max(0, Mathf.Min(f.max.z, g.max.z) - Mathf.Max(0, f.min.z, g.min.z));
                    float overlapArea = overlapX * overlapZ;

                    clearanceCost += overlapArea;
                }
            }
        }

        return clearanceCost;
    }

    // NEED TO MODIFY TO SUPPORT NON-RECTANGULAR ROOMS
    public float Circulation()
    {
        roomMin = new Vector3(
            leftWall.transform.position.x,
            0,
            backWall.transform.position.z
        );

        roomMax = new Vector3(
            rightWall.transform.position.x,
            0,
            frontWall.transform.position.z
        );

        List<Vector2> walkablePoints = new List<Vector2>();

        for (float x = roomMin.x; x <= roomMax.x; x += gridResolution)
        {
            for (float z = roomMin.z; z <= roomMax.z; z += gridResolution)
            {
                Vector3 worldPoint = new Vector3(x, 0.5f, z); // use y=0.5 to raycast through colliders

                // If no obstacle is within personRadius, it's walkable
                if (!Physics.CheckSphere(worldPoint, personRadius))
                {
                    walkablePoints.Add(new Vector2(x, z));
                }
            }
        }

        int components = CountConnectedWalkableAreas(walkablePoints, gridResolution);

        // Ideally 1 component → open circulation
        return components - 1;

    }

    int CountConnectedWalkableAreas(List<Vector2> points, float tolerance)
    {
        HashSet<Vector2> visited = new HashSet<Vector2>();
        int components = 0;

        foreach (var start in points)
        {
            if (visited.Contains(start)) continue;

            components++;
            Queue<Vector2> queue = new Queue<Vector2>();
            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                Vector2 current = queue.Dequeue();

                // Check 4 neighbors (up/down/left/right)
                Vector2[] neighbors = new Vector2[]
                {
                    current + new Vector2(gridResolution, 0),
                    current + new Vector2(-gridResolution, 0),
                    current + new Vector2(0, gridResolution),
                    current + new Vector2(0, -gridResolution)
                };

                foreach (var neighbor in neighbors)
                {
                    if (points.Contains(neighbor) && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return components;
    }

    public float PairwiseDistance()
    {
        pairwiseCost = 0;
        for (int i = 0; i < Layout.F.Count; i++)
        {
            for (int j = i + 1; j < Layout.F.Count; j++)
            {
                // if there's a relationship:
                // set m1 and m2 to the correct values based on what the objects are
                distanceBetweenObjects =
                    Vector3.Distance(Layout.F[i].transform.position, Layout.F[j].transform.position);
                //pairwiseCost += Tfunction(distanceBetweenObjects, m1, m2, 2);
            }
        }

        return pairwiseCost * -1;
    }

    private float Tfunction(float d, float m1, float m2, float a)
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
        foreach (List<GameObject> group in Layout.G)
        {
            for (int i = 0; i < group.Count; i++)
            {
                for (int j = i + 1; j < group.Count; j++)
                {
                    // if both are seats
                    distanceBetweenObjects = Vector3.Distance(group[i].transform.position, group[j].transform.position);
                    cdCost += Tfunction(distanceBetweenObjects, 4, 8, 2);
                }
            }
        }

        return cdCost * -1;
    }

    public float ConversationAngle()
    {
        caCost = 0;
        foreach (List<GameObject> group in Layout.G)
        {
            for (int i = 0; i < group.Count; i++)
            {
                for (int j = i + 1; j < group.Count; j++)
                {
                    // if both are seats
                    GameObject f = group[i];
                    GameObject g = group[j];
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
        return caCost * -1;
    }

    public float Balance()
    {
        return 0;
    }

    public float Alignment()
    {
        float alignmentCost = 0;
        foreach (List<GameObject> group in Layout.G)
        {
            for (int i = 0; i < group.Count; i++)
            {
                for (int j = i + 1; j < group.Count; j++)
                {
                    GameObject f = group[i];
                    GameObject g = group[j];
                    
                }
            }

        }
        return alignmentCost * -1;
    }
    
    public float WallAlignment()
    {
        float wallAlignmentCost = 0;
        foreach (List<GameObject> group in Layout.G)
        {
            foreach (GameObject f in group)
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
        }
        return wallAlignmentCost * -1;
    }
    
}    

