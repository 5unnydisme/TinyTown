using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARImageTracker : MonoBehaviour
{
    [System.Serializable]
    public struct ImagePrefabPair
    {
        public string imageName;
        public GameObject prefabToSpawn;
    }

    [SerializeField]
    private ARTrackedImageManager trackedImageManager;

    [SerializeField]
    private ImagePrefabPair[] imagePrefabPairs;
    
    [SerializeField]
    private float rotationSpeed = 100f; // Adjust rotation speed in Inspector
    
    // Touch control variables
    private GameObject selectedObject;
    private Touch touch;
    private Vector2 touchStartPosition;
    private bool isRotating = false;
    
    // Two-finger rotation variables
    private Vector2 touchZeroStartPosition;
    private Vector2 touchOneStartPosition;
    private float startingAngle;
    private Quaternion startingRotation;

    // Dictionary to store original rotations
    private Dictionary<GameObject, Quaternion> originalRotations = new Dictionary<GameObject, Quaternion>();
    
    private void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    
    private void Update()
    {
        HandleTouchInput();
    }
    
    private void HandleTouchInput()
    {
        // Single touch handling - rotating around Y axis
        if (Input.touchCount == 1)
        {
            touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // Store the initial touch position
                    touchStartPosition = touch.position;
                    // Try to find if user touched an AR object
                    if (TryGetTouchedObject(out GameObject touchedObject))
                    {
                        selectedObject = touchedObject;
                    }
                    break;
                    
                case TouchPhase.Moved:
                    // If we have a selected object, rotate it based on finger movement
                    if (selectedObject != null)
                    {
                        // Apply rotation around the Y axis based on touch movement
                        selectedObject.transform.Rotate(0, -touch.deltaPosition.x * rotationSpeed * Time.deltaTime, 0);
                    }
                    break;
                    
                case TouchPhase.Ended:
                    // We don't reset selectedObject here to allow multi-touch gestures to continue using it
                    break;
            }
        }
        // Two-finger rotation handling
        else if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);
            
            // Check if we have a selected object from the first touch or try to select one if we don't
            if (selectedObject == null && touchZero.phase == TouchPhase.Began)
            {
                touch = touchZero;
                if (TryGetTouchedObject(out GameObject touchedObject))
                {
                    selectedObject = touchedObject;
                }
            }
            
            if (selectedObject != null)
            {
                // Get positions of both touches
                Vector2 touchZeroCurrentPos = touchZero.position;
                Vector2 touchOneCurrentPos = touchOne.position;
                
                // Track beginning of touch
                if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
                {
                    touchZeroStartPosition = touchZeroCurrentPos;
                    touchOneStartPosition = touchOneCurrentPos;
                    
                    // Calculate the angle between the two points
                    startingAngle = Mathf.Atan2(touchOneStartPosition.y - touchZeroStartPosition.y, 
                                              touchOneStartPosition.x - touchZeroStartPosition.x) * Mathf.Rad2Deg;
                    
                    // Store the complete starting rotation
                    startingRotation = selectedObject.transform.rotation;
                }
                // Handle touch movement
                else if (touchZero.phase == TouchPhase.Moved || touchOne.phase == TouchPhase.Moved)
                {
                    // Calculate current angle between points
                    float currentAngle = Mathf.Atan2(touchOneCurrentPos.y - touchZeroCurrentPos.y,
                                                   touchOneCurrentPos.x - touchZeroCurrentPos.x) * Mathf.Rad2Deg;
                    
                    // Calculate the angle difference (rotation)
                    float angleDiff = Mathf.DeltaAngle(startingAngle, currentAngle);
                    
                    // Get the midpoint between the touches
                    Vector2 midPoint = (touchZeroCurrentPos + touchOneCurrentPos) * 0.5f;
                    
                    // Calculate vertical and horizontal movement
                    Vector2 touchZeroDelta = touchZero.deltaPosition;
                    Vector2 touchOneDelta = touchOne.deltaPosition;
                    
                    // Check if both fingers are moving in similar directions (for rotation around view axes)
                    bool similarVertical = Mathf.Sign(touchZeroDelta.y) == Mathf.Sign(touchOneDelta.y);
                    bool similarHorizontal = Mathf.Sign(touchZeroDelta.x) == Mathf.Sign(touchOneDelta.x);
                    
                    // Calculate rotation axis and amount
                    Vector3 rotationAmount = Vector3.zero;
                    
                    // Apply rotations based on gesture
                    if (similarVertical && touchZeroDelta.magnitude > 1.0f && touchOneDelta.magnitude > 1.0f)
                    {
                        // For horizontal swipes with both fingers, rotate around X axis
                        float avgHorizontalDelta = (touchZeroDelta.y + touchOneDelta.y) * 0.5f;
                        rotationAmount.x = avgHorizontalDelta * rotationSpeed * 0.5f * Time.deltaTime;
                    }
                    
                    if (similarHorizontal && touchZeroDelta.magnitude > 1.0f && touchOneDelta.magnitude > 1.0f)
                    {
                        // For vertical swipes with both fingers, rotate around Z axis
                        float avgVerticalDelta = (touchZeroDelta.x + touchOneDelta.x) * 0.5f;
                        rotationAmount.z = -avgVerticalDelta * rotationSpeed * 0.5f * Time.deltaTime;
                    }
                    
                    // For rotation gesture (twisting fingers), rotate around Y axis
                    if (Mathf.Abs(angleDiff) > 0.5f)
                    {
                        rotationAmount.y = angleDiff * rotationSpeed * 0.015f * Time.deltaTime;
                    }
                    
                    // Apply rotation in all axes at once
                    selectedObject.transform.Rotate(rotationAmount, Space.World);
                }
            }
        }
        else if (Input.touchCount == 0)
        {
            // Reset selection when all touches end
            selectedObject = null;
        }
    }
    
    private bool TryGetTouchedObject(out GameObject touchedObject)
    {
        touchedObject = null;
        
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit object is a child of one of our tracked images
            foreach (var trackedImage in trackedImageManager.trackables)
            {
                if (trackedImage.transform.childCount > 0 && 
                    (hit.collider.gameObject == trackedImage.transform.GetChild(0).gameObject ||
                     hit.collider.transform.IsChildOf(trackedImage.transform.GetChild(0))))
                {
                    touchedObject = trackedImage.transform.GetChild(0).gameObject;
                    return true;
                }
            }
        }
        
        return false;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            SpawnPrefab(newImage);
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            UpdateSpawnedPrefab(updatedImage);
        }

        foreach (var removedImage in eventArgs.removed)
        {
            RemoveSpawnedPrefab(removedImage);
        }
    }

    private void SpawnPrefab(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        ImagePrefabPair matchingPair = System.Array.Find(imagePrefabPairs, 
            pair => pair.imageName == imageName);

        if (matchingPair.prefabToSpawn != null && trackedImage.transform.childCount == 0)
        {
            GameObject prefabInstance = Instantiate(matchingPair.prefabToSpawn, 
                trackedImage.transform.position,
                trackedImage.transform.rotation);
                
            prefabInstance.transform.SetParent(trackedImage.transform);
            prefabInstance.SetActive(trackedImage.trackingState == TrackingState.Tracking);
            
            // Store the original rotation when first spawned
            originalRotations[prefabInstance] = prefabInstance.transform.rotation;
        }
    }

    private void UpdateSpawnedPrefab(ARTrackedImage trackedImage)
    {
        if (trackedImage.transform.childCount > 0)
        {
            GameObject prefabInstance = trackedImage.transform.GetChild(0).gameObject;
            
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // If object was previously inactive (hidden), reset its rotation to original
                if (!prefabInstance.activeSelf && originalRotations.ContainsKey(prefabInstance))
                {
                    prefabInstance.transform.rotation = originalRotations[prefabInstance];
                    
                    // If this was our selected object, deselect it
                    if (selectedObject == prefabInstance)
                    {
                        selectedObject = null;
                    }
                }
                
                prefabInstance.SetActive(true);
                
                // Only update position - keep user's rotation adjustments
                prefabInstance.transform.position = trackedImage.transform.position;
            }
            else
            {
                prefabInstance.SetActive(false);
            }
        }
        else if (trackedImage.trackingState == TrackingState.Tracking)
        {
            SpawnPrefab(trackedImage);
        }
    }
    
    private void RemoveSpawnedPrefab(ARTrackedImage trackedImage)
    {
        foreach (Transform child in trackedImage.transform)
        {
            // Remove from dictionary before destroying
            if (originalRotations.ContainsKey(child.gameObject))
            {
                originalRotations.Remove(child.gameObject);
            }
            
            Destroy(child.gameObject);
        }
    }
}