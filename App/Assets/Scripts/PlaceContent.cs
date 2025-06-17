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

    void Start()
    {
        // Disable placement at start
        canPlace = false;

        //Get all child objects and store them in a list to hide
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            childObjects.Add(child);
            child.SetActive(false); // Hide all child objects initially
        }
    }

    public void EnablePlacement()
    {
        // Enable placement when called
        canPlace = true;
    }

    private void Update()
    {
        // Only allow placement if enabled
        if (canPlace && Input.GetMouseButtonDown(0) && !IsClickOverUI())
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
                }
            }
        }
    }

    bool IsClickOverUI()
    {
        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(data, results);
        return results.Count > 0; 
    }
}
