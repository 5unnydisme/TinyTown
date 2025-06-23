using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GridSpawner : MonoBehaviour
{
    [Header("References")]
    public DrivingSurfaceManager drivingSurfaceManager;
    public GameObject gridCellPrefab;
    
    [Header("Grid Settings")]
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float cellSize = 0.2f;
    public float cellHeight = 0.01f; // Height above the AR plane
    public float gridSpacing = 0.05f; // Space between cells
    
    [Header("Visual Settings")]
    public Material greenMaterial;
    public Material redMaterial;
    public Material blueMaterial; // Optional third material for Perlin noise
    public Material yellowMaterial; // Optional fourth material for Perlin noise
    
    public enum PatternType 
    {
        Checkerboard,
        Random,
        PerlinNoise
    }
    
    public PatternType patternType = PatternType.Checkerboard;
    [Range(0.1f, 10f)]
    public float noiseScale = 1.0f; // Controls the scale of Perlin noise
    
    // Internal variables
    private List<GameObject> spawnedCells = new List<GameObject>();
    private bool gridSpawned = false;
    private ARPlane currentPlane;
    
    private void Update()
    {
        // Get reference to the locked plane
        ARPlane lockedPlane = drivingSurfaceManager.LockedPlane;
        
        // Only spawn grid when we have a locked plane and haven't spawned yet
        if (lockedPlane != null && !gridSpawned)
        {
            SpawnGrid(lockedPlane);
            currentPlane = lockedPlane;
            gridSpawned = true;
        }
        
        // Update grid position if plane changes
        if (gridSpawned && currentPlane != null && lockedPlane != null)
        {
            if (currentPlane != lockedPlane || HasPlaneMoved())
            {
                // Plane has changed or moved significantly, respawn grid
                ClearGrid();
                SpawnGrid(lockedPlane);
                currentPlane = lockedPlane;
            }
        }
    }
    
    private bool HasPlaneMoved()
    {
        // Check if plane center has moved significantly
        if (currentPlane == null) return false;
        
        Vector3 oldCenter = currentPlane.center;
        Vector3 newCenter = drivingSurfaceManager.LockedPlane.center;
        
        // If moved more than half a cell, return true
        return Vector3.Distance(oldCenter, newCenter) > cellSize * 0.5f;
    }
    
    public void SpawnGrid(ARPlane plane)
    {
        // Calculate the grid dimensions based on the plane size
        Bounds planeBounds = GetPlaneBounds(plane);
        
        // Calculate total grid dimensions
        float totalWidth = gridWidth * (cellSize + gridSpacing) - gridSpacing;
        float totalHeight = gridHeight * (cellSize + gridSpacing) - gridSpacing;
        
        // Calculate start position (bottom left of grid, centered on plane)
        Vector3 startPosition = plane.center;
        startPosition.y += cellHeight; // Slightly above plane
        startPosition.x -= totalWidth / 2;
        startPosition.z -= totalHeight / 2;
        
        // Add some randomization to the Perlin noise offset
        float randomXOffset = Random.Range(0f, 100f);
        float randomZOffset = Random.Range(0f, 100f);
        
        // Create grid cells
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                // Calculate position
                Vector3 cellPosition = startPosition;
                cellPosition.x += x * (cellSize + gridSpacing);
                cellPosition.z += z * (cellSize + gridSpacing);
                
                // Create cell
                GameObject cell = Instantiate(gridCellPrefab, cellPosition, Quaternion.Euler(90, 0, 0));
                cell.transform.localScale = new Vector3(cellSize, cellSize, 0.01f);
                
                // Determine which material to use based on pattern type
                Material cellMaterial;
                
                switch (patternType)
                {
                    case PatternType.Checkerboard:
                        cellMaterial = ((x + z) % 2 == 0) ? greenMaterial : redMaterial;
                        break;
                        
                    case PatternType.Random:
                        cellMaterial = Random.value > 0.5f ? greenMaterial : redMaterial;
                        break;
                        
                    case PatternType.PerlinNoise:
                        // Calculate Perlin noise value for this cell
                        float perlinValue = Mathf.PerlinNoise(
                            (x + randomXOffset) * noiseScale / gridWidth,
                            (z + randomZOffset) * noiseScale / gridHeight
                        );
                        
                        // Assign material based on the Perlin noise value
                        if (perlinValue < 0.25f && yellowMaterial != null)
                            cellMaterial = yellowMaterial;
                        else if (perlinValue < 0.5f && blueMaterial != null)
                            cellMaterial = blueMaterial;
                        else if (perlinValue < 0.75f)
                            cellMaterial = redMaterial;
                        else
                            cellMaterial = greenMaterial;
                        break;
                        
                    default:
                        cellMaterial = greenMaterial;
                        break;
                }
                
                MeshRenderer renderer = cell.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material = cellMaterial;
                }
                
                // Add to list
                spawnedCells.Add(cell);
            }
        }
        
        Debug.Log($"Grid spawned with {spawnedCells.Count} cells on plane {plane.trackableId}");
    }
    
    private Bounds GetPlaneBounds(ARPlane plane)
    {
        // Get mesh bounds from the plane
        ARPlaneMeshVisualizer meshVisualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
        if (meshVisualizer != null && meshVisualizer.mesh != null)
        {
            return meshVisualizer.mesh.bounds;
        }
        
        // Fallback to a default size
        return new Bounds(plane.center, new Vector3(1, 0, 1));
    }
    
    public void ClearGrid()
    {
        if (spawnedCells.Count > 0)
        {
            foreach (GameObject cell in spawnedCells)
            {
                if (cell != null)
                {
                    Destroy(cell);
                }
            }
            
            spawnedCells.Clear();
            gridSpawned = false;
            Debug.Log("Grid cleared");
        }
    }
}
