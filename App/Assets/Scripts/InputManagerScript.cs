using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private EnhancedGazeTracking gazeTracker;
    private Camera mainCamera;
    private PlaceContent placeContentScript;
    private ComponentUIManager uiManager;
    
    [Header("Touch Settings")]
    public float tapThreshold = 50f; // Maximum pixels moved to count as tap
    public float tapTimeLimit = 0.5f; // Maximum time for a tap
    
    [Header("ROV Centering")]
    public bool autoCenterROV = true; // Auto-center ROV when placed
    
    private Vector2 touchStartPosition;
    private float touchStartTime;
    private bool isTouchMoving = false;
    
    void Start()
    {
        gazeTracker = FindObjectOfType<EnhancedGazeTracking>();
        mainCamera = Camera.main;
        placeContentScript = FindObjectOfType<PlaceContent>();
        uiManager = ComponentUIManager.Instance;
        
        // Debug logging for startup
        Debug.Log($"InputManager: Found EnhancedGazeTracking: {gazeTracker != null}");
        Debug.Log($"InputManager: Found PlaceContent: {placeContentScript != null}");
        Debug.Log($"InputManager: Found ComponentUIManager: {uiManager != null}");
        Debug.Log($"InputManager: Main Camera: {mainCamera != null}");
        
        // Auto-enable placement for testing
        if (placeContentScript != null)
        {
            placeContentScript.EnablePlacement();
            Debug.Log("InputManager: Auto-enabled placement for testing");
        }
        else
        {
            Debug.LogError("InputManager: PlaceContent script not found!");
        }
    }
    
    void Update()
    {
        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPosition = touch.position;
                    touchStartTime = Time.time;
                    isTouchMoving = false;
                    break;
                    
                case TouchPhase.Moved:
                    // Check if touch moved beyond threshold
                    float distance = Vector2.Distance(touch.position, touchStartPosition);
                    if (distance > tapThreshold)
                    {
                        isTouchMoving = true;
                        
                        // Notify UI manager of potential rotation
                        if (uiManager != null && placeContentScript != null && placeContentScript.HasBeenPlaced)
                        {
                            uiManager.OnROVInteraction();
                        }
                    }
                    break;
                    
                case TouchPhase.Ended:
                    // Only handle as tap if not moving and within time limit
                    if (!isTouchMoving && (Time.time - touchStartTime) <= tapTimeLimit)
                    {
                        Vector2 adjustedPosition = ConvertScreenPosition(touch.position);
                        HandleTap(adjustedPosition);
                    }
                    break;
            }
        }
        // Handle mouse input (for testing)
        else if (Input.GetMouseButtonDown(0))
        {
            Vector2 adjustedPosition = ConvertScreenPosition(Input.mousePosition);
            HandleTap(adjustedPosition);
        }
    }
    
    /// <summary>
    /// Converts screen position to ensure proper coordinate system for AR
    /// </summary>
    Vector2 ConvertScreenPosition(Vector2 rawPosition)
    {
        // For AR Foundation, screen coordinates should be in the range [0, Screen.width] and [0, Screen.height]
        // Touch coordinates are already in this format, but let's ensure they're valid
        Vector2 adjustedPosition = rawPosition;
        
        // Clamp to screen bounds
        adjustedPosition.x = Mathf.Clamp(adjustedPosition.x, 0, Screen.width);
        adjustedPosition.y = Mathf.Clamp(adjustedPosition.y, 0, Screen.height);
        
        Debug.Log($"InputManager: Raw position: {rawPosition}, Adjusted: {adjustedPosition}");
        
        return adjustedPosition;
    }
    
    void HandleTap(Vector2 screenPosition)
    {
        Debug.Log($"InputManager: Processing tap at {screenPosition}");
        Debug.Log($"InputManager: Screen size: {Screen.width}x{Screen.height}");
        Debug.Log($"InputManager: Screen position normalized: ({screenPosition.x/Screen.width:F2}, {screenPosition.y/Screen.height:F2})");
        
        if (mainCamera == null)
        {
            Debug.LogError("InputManager: Main camera is null!");
            return;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Debug.Log($"InputManager: Ray origin: {ray.origin}, direction: {ray.direction}");
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log($"InputManager: Raycast hit {hit.collider.gameObject.name} with tag {hit.collider.tag} at world position {hit.point}");
            
            if (hit.collider.CompareTag("hasInfo"))
            {
                // Check if inspection is allowed
                if (uiManager != null && !uiManager.CanInspect())
                {
                    Debug.Log("InputManager: Component inspection blocked by UI manager");
                    return;
                }
                
                Debug.Log("InputManager: Tap hit component - handling inspection");
                if (gazeTracker != null)
                {
                    gazeTracker.InspectCurrentComponent();
                }
                return;
            }
        }
        
        // Handle ROV placement if no component hit
        Debug.Log("InputManager: Tap didn't hit component - trying ROV placement");
        if (placeContentScript != null && !placeContentScript.HasBeenPlaced)
        {
            if (!placeContentScript.CanPlace)
            {
                placeContentScript.EnablePlacement();
            }
            Debug.Log($"InputManager: Passing screen position {screenPosition} to PlaceContent");
            placeContentScript.TryPlaceROV(screenPosition);
        }
    }
    
    /// <summary>
    /// Centers all child objects of the ROV around the parent's pivot point
    /// This fixes rotation issues by ensuring the ROV rotates around its center
    /// FIXED: Moves parent to center while keeping children in place to maintain placement position
    /// Includes both active and inactive children (like highlight outlines)
    /// </summary>
    public void CenterROVComponents(GameObject rovParent)
    {
        if (rovParent == null)
        {
            Debug.LogError("InputManager: ROV parent is null, cannot center components");
            return;
        }
        
        // Get ALL children including inactive ones (includeInactive = true)
        Transform[] allChildren = rovParent.GetComponentsInChildren<Transform>(true);
        if (allChildren.Length <= 1) // Only parent, no children
        {
            Debug.Log("InputManager: No child components found to center");
            return;
        }
        
        Debug.Log($"InputManager: Centering {allChildren.Length - 1} ROV components (including inactive highlights)");
        
        // Calculate center of all child components (excluding parent)
        Vector3 centerPosition = Vector3.zero;
        int childCount = 0;
        
        foreach (Transform child in allChildren)
        {
            if (child != rovParent.transform) // Skip parent transform
            {
                centerPosition += child.position; // Use WORLD position
                childCount++;
            }
        }
        
        if (childCount == 0)
        {
            Debug.Log("InputManager: No valid child components found");
            return;
        }
        
        centerPosition /= childCount;
        
        // ✅ FIXED: Move PARENT to calculated center instead of moving children
        // This keeps the ROV at the original placement location while centering the pivot
        Vector3 offset = centerPosition - rovParent.transform.position;
        
        Debug.Log($"InputManager: Calculated ROV center: {centerPosition}, Parent position: {rovParent.transform.position}");
        Debug.Log($"InputManager: Applying offset to center pivot: {offset}");
        
        // Move parent to the calculated center
        rovParent.transform.position = centerPosition;
        
        // Move all children back by the same offset to maintain their original world positions
        foreach (Transform child in allChildren)
        {
            if (child != rovParent.transform)
            {
                child.position -= offset;
            }
        }
        
        Debug.Log("InputManager: ✅ FIXED - ROV components centered while maintaining placement position");
    }
    
    /// <summary>
    /// Called when ROV is successfully placed to perform post-placement setup
    /// </summary>
    public void OnROVPlaced(GameObject placedROV)
    {
        if (placedROV == null)
        {
            Debug.LogError("InputManager: Placed ROV is null");
            return;
        }
        
        Debug.Log($"InputManager: ROV placed successfully: {placedROV.name}");
        
        // Auto-center the ROV if enabled
        if (autoCenterROV)
        {
            CenterROVComponents(placedROV);
        }
    }
}