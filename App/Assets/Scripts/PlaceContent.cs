using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceContent : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public GraphicRaycaster raycaster;
    private bool canPlace = false;
    private bool hasBeenPlaced = false;
    private List<GameObject> childObjects = new List<GameObject>();
    private InputManager inputManager;

    // Public property to check if object has been placed
    public bool HasBeenPlaced => hasBeenPlaced;
    public bool CanPlace => canPlace;

    void Start()
    {
        // Disable placement at start
        canPlace = false;
        Debug.Log("PlaceContent: Start() - canPlace set to FALSE initially");

        // Find InputManager for callbacks
        inputManager = FindObjectOfType<InputManager>();
        Debug.Log($"PlaceContent: Found InputManager: {inputManager != null}");

        //Get all child objects and store them in a list to hide
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            childObjects.Add(child);
            child.SetActive(false); // Hide all child objects initially
        }
        Debug.Log($"PlaceContent: Start() - Found and hid {childObjects.Count} child objects");
    }

    public void EnablePlacement()
    {
        // Enable placement when called
        canPlace = true;
        Debug.Log("PlaceContent: EnablePlacement() called - canPlace is now TRUE");
    }

    private void Update()
    {
        // REMOVE OR COMMENT OUT THE INPUT HANDLING
        // Let InputManager handle all input instead
        
        /*
        // Only allow placement if enabled and not already placed
        if (canPlace && !hasBeenPlaced && Input.GetMouseButtonDown(0) && !IsClickOverUI())
        {
            List<ARRaycastHit> hitPoints = new List<ARRaycastHit>();
            raycastManager.Raycast(Input.mousePosition, hitPoints, TrackableType.Planes);

            if (hitPoints.Count > 0)
            {
                Pose pose = hitPoints[0].pose;
                transform.rotation = pose.rotation;
                transform.position = pose.position;

                // Show all child objects when placement occurs
                if (!hasBeenPlaced)
                {
                    foreach (GameObject child in childObjects)
                    {
                        child.SetActive(true); // Show all child objects
                    }
                    hasBeenPlaced = true; // Ensure this only happens once
                    canPlace = false; // Disable further placement
                }
            }
        }
        */
    }

    // Add public method for InputManager to call
    public void TryPlaceROV(Vector2 screenPosition)
    {
        Debug.Log($"PlaceContent: TryPlaceROV called with position {screenPosition}");
        Debug.Log($"PlaceContent: Screen size: {Screen.width}x{Screen.height}");
        Debug.Log($"PlaceContent: canPlace={canPlace}, hasBeenPlaced={hasBeenPlaced}");
        
        // Only allow placement if enabled and not already placed
        if (!canPlace || hasBeenPlaced)
        {
            Debug.LogWarning($"PlaceContent: Cannot place ROV - canPlace:{canPlace}, hasBeenPlaced:{hasBeenPlaced}");
            return;
        }
        
        if (IsClickOverUI(screenPosition))
        {
            Debug.Log("PlaceContent: Click is over UI, ignoring placement");
            return;
        }
        
        Debug.Log("PlaceContent: Conditions met, attempting AR raycast");
        List<ARRaycastHit> hitPoints = new List<ARRaycastHit>();
        
        // Ensure screen position is valid
        if (screenPosition.x < 0 || screenPosition.x > Screen.width || 
            screenPosition.y < 0 || screenPosition.y > Screen.height)
        {
            Debug.LogWarning($"PlaceContent: Screen position {screenPosition} is outside screen bounds!");
            return;
        }
        
        // Use the actual screen position for AR raycast
        bool raycastHit = raycastManager.Raycast(screenPosition, hitPoints, TrackableType.Planes);
        
        Debug.Log($"PlaceContent: AR raycast found {hitPoints.Count} planes at screen position {screenPosition}");

        if (hitPoints.Count > 0)
        {
            Debug.Log($"PlaceContent: Placing ROV at world position {hitPoints[0].pose.position}!");
            Pose pose = hitPoints[0].pose;
            transform.rotation = pose.rotation;
            transform.position = pose.position;

            // Show all child objects when placement occurs
            if (!hasBeenPlaced)
            {
                foreach (GameObject child in childObjects)
                {
                    child.SetActive(true); // Show all child objects
                }
                hasBeenPlaced = true; // Ensure this only happens once
                canPlace = false; // Disable further placement
                Debug.Log("PlaceContent: ROV successfully placed and children activated");
                
                // Notify InputManager that ROV was placed
                if (inputManager != null)
                {
                    inputManager.OnROVPlaced(gameObject);
                }
            }
        }
        else
        {
            Debug.LogWarning("PlaceContent: No AR planes detected for placement!");
        }
    }

    bool IsClickOverUI(Vector2 screenPosition)
    {
        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = screenPosition  // Use the provided screen position instead of Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(data, results);
        return results.Count > 0; 
    }
}