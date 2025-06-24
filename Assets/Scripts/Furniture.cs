using UnityEngine;
using System.Collections.Generic;

public class Furniture : MonoBehaviour
{
    public List<Vector2Int> occupiedTiles = new List<Vector2Int>();
    public List<Vector2Int> radiusTiles = new List<Vector2Int>();
    
    public void ComputeOccupiedTiles(float tileSize, int gridCols, int gridRows)
    {
        occupiedTiles.Clear();
        radiusTiles.Clear();

        Collider col = GetComponentInChildren<Collider>();
        if (col == null) return;

        Bounds bounds = col.bounds;
        float gridOffsetX = (gridCols - 1) * tileSize / 2f;
        float gridOffsetZ = (gridRows - 1) * tileSize / 2f;

        // --- Step 1: Directly under object using bounds ---
        int minX = Mathf.Max(0, Mathf.FloorToInt((bounds.min.x + gridOffsetX) / tileSize));
        int maxX = Mathf.Min(gridCols - 1, Mathf.FloorToInt((bounds.max.x + gridOffsetX) / tileSize));
        int minZ = Mathf.Max(0, Mathf.FloorToInt((bounds.min.z + gridOffsetZ) / tileSize));
        int maxZ = Mathf.Min(gridRows - 1, Mathf.FloorToInt((bounds.max.z + gridOffsetZ) / tileSize));

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Vector2Int tileCoord = new Vector2Int(x, z);
                occupiedTiles.Add(tileCoord);
            }
        }

        // --- Step 2: Radius highlight using actual collider shape ---
        float radius = UnitConverter.InchesToMeters(18.0f);
        float radiusSqr = radius * radius;

        int radiusMinX = Mathf.Max(0, Mathf.FloorToInt((bounds.min.x - radius + gridOffsetX) / tileSize));
        int radiusMaxX = Mathf.Min(gridCols - 1, Mathf.CeilToInt((bounds.max.x + radius + gridOffsetX) / tileSize));
        int radiusMinZ = Mathf.Max(0, Mathf.FloorToInt((bounds.min.z - radius + gridOffsetZ) / tileSize));
        int radiusMaxZ = Mathf.Min(gridRows - 1, Mathf.CeilToInt((bounds.max.z + radius + gridOffsetZ) / tileSize));

        for (int x = radiusMinX; x <= radiusMaxX; x++)
        {
            for (int z = radiusMinZ; z <= radiusMaxZ; z++)
            {
                Vector3 tileCenter = new Vector3(x * tileSize - gridOffsetX, bounds.center.y, z * tileSize - gridOffsetZ);

                Vector3 closest = col.ClosestPoint(tileCenter);
                float distSqr = (closest - tileCenter).sqrMagnitude;

                Vector2Int tileCoord = new Vector2Int(x, z);

                if (distSqr <= radiusSqr && !occupiedTiles.Contains(tileCoord))
                {
                    radiusTiles.Add(tileCoord);
                }
            }
        }
    }
}
