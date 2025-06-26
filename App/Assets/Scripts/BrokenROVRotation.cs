using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenROVRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 50f;
    public bool smoothRotation = true;
    public float smoothingFactor = 5f;
    
    [Header("Mouse Controls (for testing in editor)")]
    public bool enableMouseControls = true;
    public float mouseRotationSpeed = 100f;
    
    [Header("Placement Control")]
    public PlaceContent placeContentScript; // Reference to the PlaceContent script
    
    private Vector2 lastTouchDelta;
    private bool isTwoFingerGesture = false;
    private Vector3 targetRotation;
    private Quaternion initialRotation;
    private bool isObjectPlaced = false;
    
    void Start()
    {
        initialRotation = transform.rotation;
        targetRotation = transform.eulerAngles;
        
        // Try to find PlaceContent script if not assigned
        if (placeContentScript == null)
        {
            placeContentScript = GetComponentInParent<PlaceContent>();
            if (placeContentScript == null)
            {
                placeContentScript = FindObjectOfType<PlaceContent>();
            }
        }
    }

    void Update()
    {
        // Check if object has been placed to disable further position changes
        CheckPlacementStatus();
        
        // Only allow rotation if object has been placed, preventing position changes
        if (isObjectPlaced)
        {
            HandleTwoFingerRotation();
            
            // Mouse controls for testing in Unity Editor
            if (enableMouseControls && Application.isEditor)
            {
                HandleMouseRotation();
            }
            
            // Apply smooth rotation if enabled
            if (smoothRotation)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * smoothingFactor);
            }
        }
    }
    
    void CheckPlacementStatus()
    {
        if (placeContentScript != null)
        {
            // Use reflection to access the private field since public property might not be recognized yet
            var field = placeContentScript.GetType().GetField("hasBeenPlaced", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                isObjectPlaced = (bool)field.GetValue(placeContentScript);
            }
            else
            {
                // Fallback - check if we can place (if canPlace is false and we've been running for a bit, assume placed)
                isObjectPlaced = Time.time > 2f;
            }
        }
        else
        {
            // Fallback: assume object is placed if no PlaceContent script is found
            // or after a short delay to allow for initial placement
            isObjectPlaced = Time.time > 2f; // Allow rotation after 2 seconds if no PlaceContent found
        }
    }
    
    void HandleTwoFingerRotation()
    {
        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);
            
            // Get current positions and calculate delta
            Vector2 touch1Pos = touch1.position;
            Vector2 touch2Pos = touch2.position;
            Vector2 currentDelta = touch2Pos - touch1Pos;
            
            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                // Initialize two-finger gesture
                isTwoFingerGesture = true;
                lastTouchDelta = currentDelta;
            }
            else if (isTwoFingerGesture && (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved))
            {
                // Calculate rotation based on delta change
                Vector2 deltaChange = currentDelta - lastTouchDelta;
                
                // Only horizontal movement rotates around Y-axis
                float yRotation = deltaChange.x * rotationSpeed * Time.deltaTime;
                
                // Apply only Y-axis rotation
                if (smoothRotation)
                {
                    targetRotation.y += yRotation;
                }
                else
                {
                    // Rotate only around Y-axis using Space.Self
                    transform.Rotate(0, yRotation, 0, Space.Self);
                }
                
                lastTouchDelta = currentDelta;
            }
        }
        else
        {
            isTwoFingerGesture = false;
        }
    }
    
    void HandleMouseRotation()
    {
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            
            // Only Y-axis rotation from horizontal mouse movement
            float yRotation = mouseX * mouseRotationSpeed * Time.deltaTime;
            
            if (smoothRotation)
            {
                targetRotation.y += yRotation;
            }
            else
            {
                // Only Y-axis rotation - rotate around object center
                transform.Rotate(0, yRotation, 0, Space.Self);
            }
        }
    }
    
    // Public method to reset rotation
    public void ResetRotation()
    {
        if (smoothRotation)
        {
            targetRotation = initialRotation.eulerAngles;
        }
        else
        {
            transform.rotation = initialRotation;
        }
    }
    
    // Public method to set rotation speed
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
}
