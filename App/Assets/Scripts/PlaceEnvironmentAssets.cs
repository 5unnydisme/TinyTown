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
    
    [Header("Debug Settings")]
    [SerializeField]
    private bool showDebugLogs = true;
    
    [SerializeField]
    private bool visualizePoints = true;
    
    [SerializeField]
    private Color debugColor = Color.red;
    
    [SerializeField]
    private Color invalidPointColor = Color.yellow;
    
    [SerializeField]
    private float debugLineDuration = 3f;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private HashSet<ARPlane> processedPlanes = new HashSet<ARPlane>();
    private int totalObstaclesPlaced = 0;
    private Dictionary<Vector3, bool> activeDebugPoints = new Dictionary<Vector3, bool>();

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
        if (showDebugLogs)
        {
            Debug.Log($"[PlaceEnvironmentAssets] Planes Changed - Added: {args.added.Count}, Updated: {args.updated.Count}, Removed: {args.removed.Count}");
            
            foreach (ARPlane plane in args.updated)
            {
                Debug.Log($"[PlaceEnvironmentAssets] Plane {plane.trackableId} updated - Size: {plane.size.x:F2}m x {plane.size.y:F2}m");
            }
        }
        
        foreach (ARPlane plane in args.added)
        {
            float planeHeight = plane.transform.position.y;
            
            // Initialize first plane height if not set
            if (!firstPlaneHeight.HasValue)
            {
                firstPlaneHeight = planeHeight;
                if (showDebugLogs)
                {
                    Debug.Log($"[PlaceEnvironmentAssets] First plane detected at height: {planeHeight:F2}m");
                }
            }

            // Check if plane is too high compared to first plane
            float heightDifference = Mathf.Abs(planeHeight - firstPlaneHeight.Value);
            if (heightDifference > maxHeightDifference)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[PlaceEnvironmentAssets] Plane rejected - Too high: {heightDifference:F2}m above first plane");
                }
                continue;
            }

            if (!processedPlanes.Contains(plane) && plane.size.x > 0.5f && plane.size.y > 0.5f)
            {
                Debug.Log($"[PlaceEnvironmentAssets] Processing new plane - Size: {plane.size.x:F2}m x {plane.size.y:F2}m, Height: {planeHeight:F2}m");
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
        activeDebugPoints.Clear();

        // Determine the height of the first plane for reference
        if (firstPlaneHeight == null)
        {
            firstPlaneHeight = plane.transform.position.y;
            Debug.Log($"[PlaceEnvironmentAssets] First plane height set to {firstPlaneHeight}");
        }

        while (placedPositions.Count < obstaclesPerPlane && attempts < maxAttempts)
        {
            attempts++;

            Vector2 randomPoint = Random.insideUnitCircle * Mathf.Min(plane.size.x, plane.size.y) * 0.4f;
            Vector3 worldPoint = plane.transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);

            // Visualize attempt
            if (visualizePoints)
            {
                Debug.DrawLine(worldPoint, worldPoint + Vector3.up * 0.5f, debugColor, debugLineDuration);
            }

            bool tooClose = false;
            foreach (Vector3 existingPosition in placedPositions)
            {
                float distance = Vector3.Distance(worldPoint, existingPosition);
                if (distance < minDistanceBetweenObstacles)
                {
                    tooClose = true;
                    if (visualizePoints)
                    {
                        // Draw distance check line
                        Debug.DrawLine(worldPoint, existingPosition, invalidPointColor, debugLineDuration);
                        Vector3 midPoint = (worldPoint + existingPosition) * 0.5f;
                        Debug.DrawLine(midPoint, midPoint + Vector3.up * 0.2f, invalidPointColor, debugLineDuration);
                    }
                    if (showDebugLogs)
                    {
                        Debug.Log($"[PlaceEnvironmentAssets] Position rejected - Distance: {distance:F2}m");
                    }
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
                        activeDebugPoints[worldPoint] = true;

                        if (showDebugLogs)
                        {
                            Debug.Log($"[PlaceEnvironmentAssets] Placed obstacle at {worldPoint}, Scale: {randomScale:F2}");
                        }
                        
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                else
                {
                    activeDebugPoints[worldPoint] = false;
                    if (showDebugLogs)
                    {
                        Debug.Log($"[PlaceEnvironmentAssets] Position rejected - Height difference: {heightDifference:F2}m");
                    }
                }
            }
            else
            {
                activeDebugPoints[worldPoint] = false;
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"[PlaceEnvironmentAssets] Placement complete - Success: {placedPositions.Count}/{obstaclesPerPlane}, Attempts: {attempts}");
        }

        yield return null;
    }

    private void OnDrawGizmos()
    {
        if (visualizePoints && Application.isPlaying)
        {
            // Draw plane bounds
            foreach (ARPlane plane in processedPlanes)
            {
                if (plane != null)
                {
                    Gizmos.color = debugColor;
                    Vector3 center = plane.transform.position;
                    Vector3 size = new Vector3(plane.size.x, 0.01f, plane.size.y);
                    Gizmos.DrawWireCube(center, size);
                }
            }

            // Draw debug points
            foreach (var point in activeDebugPoints)
            {
                Gizmos.color = point.Value ? debugColor : invalidPointColor;
                Gizmos.DrawSphere(point.Key, 0.1f);
            }
        }
    }
}