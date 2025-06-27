using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnhancedGazeTracking : MonoBehaviour
{
    [Header("Gaze Settings")]
    public LayerMask gazeLayerMask = -1;
    public float maxGazeDistance = 10f;
    
    private List<ComponentInfo> componentInfos = new List<ComponentInfo>();
    private ComponentInfo currentGazedComponent = null;
    
    void Start()
    {
        // Find all ComponentInfo components in the scene
        componentInfos = FindObjectsOfType<ComponentInfo>().ToList();
        Debug.Log($"Found {componentInfos.Count} components with info");
    }

    void Update()
    {
        HandleGazeTracking();
    }
    
    void HandleGazeTracking()
    {
        ComponentInfo gazedComponent = null;
        
        // Cast ray from camera forward
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxGazeDistance, gazeLayerMask))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Check if the hit object has ComponentInfo or if its parent does
            if (hitObject.CompareTag("hasInfo"))
            {
                gazedComponent = hitObject.GetComponent<ComponentInfo>();
                if (gazedComponent == null)
                {
                    gazedComponent = hitObject.GetComponentInParent<ComponentInfo>();
                }
            }
        }
        
        // Handle gaze enter/exit
        if (gazedComponent != currentGazedComponent)
        {
            // Exit previous component
            if (currentGazedComponent != null)
            {
                currentGazedComponent.OnGazeExit();
            }
            
            // Enter new component
            currentGazedComponent = gazedComponent;
            if (currentGazedComponent != null)
            {
                currentGazedComponent.OnGazeEnter();
            }
        }
    }
    
    // Public method to get currently gazed component
    public ComponentInfo GetCurrentGazedComponent()
    {
        return currentGazedComponent;
    }
    
    // Public method to trigger inspection of currently gazed component
    public void InspectCurrentComponent()
    {
        if (currentGazedComponent != null)
        {
            currentGazedComponent.OnTapInspect();
        }
    }
}
