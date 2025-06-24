using UnityEngine;
using System.Collections.Generic;

public class FloorGridGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public int rows = Mathf.RoundToInt(UnitConverter.InchesToMeters(Parameters.floorSizeZ) * 10.0f); // z
    public int cols = Mathf.RoundToInt(UnitConverter.InchesToMeters(Parameters.floorSizeX) * 10.0f); // x
    public float tileSize = 0.1f;
    public Material defaultMaterial;
    public Material highlightMaterial;

    private GameObject[,] tiles;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        tiles = new GameObject[cols, rows];

        float offsetX = (cols - 1) * tileSize / 2f;
        float offsetZ = (rows - 1) * tileSize / 2f;

        for (int x = 0; x < cols; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                Vector3 pos = new Vector3(x * tileSize - offsetX, 0, z * tileSize - offsetZ);
                GameObject tile = Instantiate(tilePrefab, pos, tilePrefab.transform.rotation, transform);
                tile.name = $"Tile_{x}_{z}";
                tile.GetComponent<Renderer>().material = defaultMaterial;
                tiles[x, z] = tile;
            }
        }
    }


    public void UpdateTileColors()
    {
        float gridOffsetX = (cols - 1) * tileSize / 2f;
        float gridOffsetZ = (rows - 1) * tileSize / 2f;
        float radius = UnitConverter.InchesToMeters(18.0f); // 18 inches in meters
        float radiusSqr = radius * radius;

        // Step 1: Reset all tiles
        foreach (var tile in tiles)
        {
            tile.GetComponent<Renderer>().material = defaultMaterial;
        }

        // Step 2: Compute and highlight furniture tiles
        foreach (GameObject obj in Layout.F)
        {
            Furniture f = obj.GetComponent<Furniture>();
            if (f == null) continue;

            f.ComputeOccupiedTiles(tileSize, cols, rows);

            // Highlight tiles directly under the object
            foreach (Vector2Int coord in f.occupiedTiles)
            {
                HighlightTile(coord.x, coord.y);
            }

            // Highlight tiles within 18 inches of the object
            foreach (Vector2Int coord in f.radiusTiles)
            {
                HighlightTile(coord.x, coord.y);
            }
        }

        // Step 3: Radius highlight around wall objects (optional)
        foreach (GameObject wallObj in Layout.R)
        {
            Renderer rend = wallObj.GetComponentInChildren<Renderer>();
            if (rend == null) continue;

            Bounds bounds = rend.bounds;

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            int minX = Mathf.Max(0, Mathf.FloorToInt((min.x - radius + gridOffsetX) / tileSize));
            int maxX = Mathf.Min(cols - 1, Mathf.CeilToInt((max.x + radius + gridOffsetX) / tileSize));
            int minZ = Mathf.Max(0, Mathf.FloorToInt((min.z - radius + gridOffsetZ) / tileSize));
            int maxZ = Mathf.Min(rows - 1, Mathf.CeilToInt((max.z + radius + gridOffsetZ) / tileSize));

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    Vector3 tileCenter = new Vector3(x * tileSize - gridOffsetX, 0, z * tileSize - gridOffsetZ);

                    // 💡 Same logic as for furniture — distance to edge
                    Vector3 closestPoint = bounds.ClosestPoint(tileCenter);
                    float distSqr = (tileCenter - closestPoint).sqrMagnitude;

                    if (distSqr <= radiusSqr)
                    {
                        HighlightTile(x, z);
                    }
                }
            }
        }
        
        // END OF CIRCULATION DISPLAY
        Manager.readyToCalculate = true;
        
    }
    
    void HighlightTile(int x, int z)
    {
        if (x >= 0 && x < cols && z >= 0 && z < rows)
        {
            tiles[x, z].GetComponent<Renderer>().material = highlightMaterial;
        }
    }
    
    // CIRCULATION CALCULATION
    public int CountWalkableRegions()
    {
        bool[,] visited = new bool[cols, rows];
        int regionCount = 0;

        for (int x = 0; x < cols; x++)
        {
            for (int z = 0; z < rows; z++)
            {
                if (!visited[x, z] && IsDefaultTile(x, z))
                {
                    FloodFill(x, z, visited);
                    regionCount++;
                }
            }
        }

        return regionCount;
    }

    bool IsDefaultTile(int x, int z)
    {
        return tiles[x, z].GetComponent<Renderer>().sharedMaterial == defaultMaterial;
    }

    void FloodFill(int startX, int startZ, bool[,] visited)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startZ));
        visited[startX, startZ] = true;

        int[] dx = { 1, -1, 0, 0 };
        int[] dz = { 0, 0, 1, -1 };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int nx = current.x + dx[i];
                int nz = current.y + dz[i];

                if (nx >= 0 && nx < cols && nz >= 0 && nz < rows &&
                    !visited[nx, nz] && IsDefaultTile(nx, nz))
                {
                    visited[nx, nz] = true;
                    queue.Enqueue(new Vector2Int(nx, nz));
                }
            }
        }
    }
    
}