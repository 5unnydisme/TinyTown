using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceEnvironmentAssets : MonoBehaviour
{
    [SerializeField]
    private GameObject[] obstaclePrefabs;

    [SerializeField]
    private ARRaycastManager raycastManager;

    [SerializeField]
    private ARPlaneManager planeManager;

    [SerializeField]
    private int obstaclesPerPlane = 3;

    [SerializeField]
    private float minDistanceBetweenObstacles = 0.8f;  // Reduced from 1.5f for tighter obstacle placement

    [SerializeField]
    private float maxHeightDifference = 0.3f;  // Maximum allowed height difference from first plane

    private float? firstPlaneHeight = null;  // Tracks the Y position of the first detected plane
    
    // Debug settings removed

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private HashSet<ARPlane> processedPlanes = new HashSet<ARPlane>();
    private int totalObstaclesPlaced = 0;

    private void Start()
    {
        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
            Debug.Log($"[PlaceEnvironmentAssets] Found RaycastManager: {raycastManager != null}");
        }
        
        if (planeManager == null)
        {
            planeManager = FindObjectOfType<ARPlaneManager>();
            Debug.Log($"[PlaceEnvironmentAssets] Found PlaneManager: {planeManager != null}");
        }

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogError("[PlaceEnvironmentAssets] No obstacle prefabs assigned!");
            enabled = false;
            return;
        }

        planeManager.planesChanged += OnPlanesChanged;
    }

    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        
        foreach (ARPlane plane in args.added)
        {
            float planeHeight = plane.transform.position.y;
            
            // Initialize first plane height if not set
            if (!firstPlaneHeight.HasValue)
            {
                firstPlaneHeight = planeHeight;
            }

            // Check if plane is too high compared to first plane
            float heightDifference = Mathf.Abs(planeHeight - firstPlaneHeight.Value);
            if (heightDifference > maxHeightDifference)
            {
                continue;
            }

            if (!processedPlanes.Contains(plane) && plane.size.x > 0.5f && plane.size.y > 0.5f)
            {
                StartCoroutine(PlaceObstaclesOnPlane(plane));
                processedPlanes.Add(plane);
            }
        }
    }

    private IEnumerator PlaceObstaclesOnPlane(ARPlane plane)
    {
        List<Vector3> placedPositions = new List<Vector3>();
        int attempts = 0;
        int maxAttempts = obstaclesPerPlane * 3;

        // Determine the height of the first plane for reference
        if (firstPlaneHeight == null)
        {
            firstPlaneHeight = plane.transform.position.y;
        }

        while (placedPositions.Count < obstaclesPerPlane && attempts < maxAttempts)
        {
            attempts++;

            Vector2 randomPoint = Random.insideUnitCircle * Mathf.Min(plane.size.x, plane.size.y) * 0.4f;
            Vector3 worldPoint = plane.transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);

            // Point visualization removed

            bool tooClose = false;
            foreach (Vector3 existingPosition in placedPositions)
            {
                float distance = Vector3.Distance(worldPoint, existingPosition);
                if (distance < minDistanceBetweenObstacles)
                {
                    tooClose = true;
                    // Distance check visualization and debug logging removed
                    break;
                }
            }

            if (!tooClose)
            {
                // Check height difference from the first plane
                float heightDifference = Mathf.Abs(worldPoint.y - firstPlaneHeight.Value);
                if (heightDifference <= maxHeightDifference)
                {
                    int randomPrefabIndex = Random.Range(0, obstaclePrefabs.Length);
                    GameObject prefabToSpawn = obstaclePrefabs[randomPrefabIndex];

                    if (prefabToSpawn != null)
                    {
                        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        GameObject placedObject = Instantiate(prefabToSpawn, worldPoint, randomRotation);
                        float randomScale = Random.Range(0.1f, 0.5f);
                        placedObject.transform.localScale *= randomScale;

                        placedPositions.Add(worldPoint);
                        totalObstaclesPlaced++;
                        
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }

        // Placement complete debug log removed

        yield return null;
    }

    // OnDrawGizmos removed to disable visualization
}