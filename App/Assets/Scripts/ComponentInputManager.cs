using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInputManager : MonoBehaviour
{
    [Header("Input Settings")]
    public LayerMask uiLayerMask = 5; // UI layer to prevent interaction conflicts
    public bool enableTouchInput = true;
    public bool enableMouseInput = true; // For testing in editor
    
    private EnhancedGazeTracking gazeTracker;
    private ComponentUIManager uiManager;
    
    void Start()
    {
        gazeTracker = FindObjectOfType<EnhancedGazeTracking>();
        uiManager = ComponentUIManager.Instance;
        
        if (gazeTracker == null)
        {
            Debug.LogError("EnhancedGazeTracking not found! Component interaction won't work.");
        }
    }
    
    void Update()
    {
        // Don't process input if UI panel is open
        if (uiManager != null && uiManager.IsPanelOpen())
        {
            return;
        }
        
        HandleInput();
    }
    
    void HandleInput()
    {
        bool inputDetected = false;
        
        // Handle touch input
        if (enableTouchInput && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (!IsTouchOverUI(touch.position))
                {
                    inputDetected = true;
                }
            }
        }
        // Handle mouse input (for testing in editor)
        else if (enableMouseInput && Input.GetMouseButtonDown(0))
        {
            if (!IsMouseOverUI())
            {
                inputDetected = true;
            }
        }
        
        if (inputDetected)
        {
            HandleComponentInteraction();
        }
    }
    
    void HandleComponentInteraction()
    {
        if (gazeTracker != null)
        {
            ComponentInfo currentComponent = gazeTracker.GetCurrentGazedComponent();
            if (currentComponent != null)
            {
                // Trigger detailed inspection
                gazeTracker.InspectCurrentComponent();
                Debug.Log($"Inspecting component: {currentComponent.componentName}");
            }
            else
            {
                Debug.Log("No component currently being gazed at");
            }
        }
    }
    
    bool IsTouchOverUI(Vector2 touchPosition)
    {
        // Check if touch is over UI elements
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
    }
    
    bool IsMouseOverUI()
    {
        // Check if mouse is over UI elements
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }
    
    // Public methods for external control
    public void EnableInput()
    {
        enableTouchInput = true;
        enableMouseInput = true;
    }
    
    public void DisableInput()
    {
        enableTouchInput = false;
        enableMouseInput = false;
    }
    
    public void SetTouchEnabled(bool enabled)
    {
        enableTouchInput = enabled;
    }
    
    public void SetMouseEnabled(bool enabled)
    {
        enableMouseInput = enabled;
    }
}
