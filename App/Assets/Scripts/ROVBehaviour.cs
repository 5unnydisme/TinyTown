/*
 * Based on the original CarBehaviour by Google LLC
 * Modified to handle proper ROV model rotation and add floating effect
 */

using System;
using System.Collections;
using UnityEngine;

/**
 * Modified version of CarBehaviour that ensures the ROV's front faces the direction of movement
 * and simulates a floating effect.
 */
public class ROVBehaviour : MonoBehaviour
{
    public static event Action OnROVDeath;
    public static event Action OnSecondCollision;
    public static event Action OnThirdCollision;
    public ReticleBehaviour Reticle;
    public float Speed = 1.2f;

    [Tooltip("Minimum distance to maintain from reticle (ROV will stop following when closer than this)")]
    public float followDistance = 0.5f; // Increased from 0.1f to create more space
    
    [Header("Collision Slowdown")]
    [Tooltip("Speed reduction factor after each package collision (0.8 = 20% speed reduction per collision)")]
    public float slowdownFactor = 0.8f;
    
    [Tooltip("Number of collisions before slowdown begins")]
    public int collisionsBeforeSlowdown = 2;
    
    [Tooltip("Number of collisions before ROV stops completely")]
    public int maxCollisions = 4;
    
    [Tooltip("Assign the forward direction of your ROV model (usually one of the world axes)")]
    public Vector3 ModelForwardDirection = Vector3.forward; // Change this to match your model's forward

    [Tooltip("Optional offset angle (in degrees) if the model isn't perfectly aligned")]
    public float RotationOffset = 0f;
    
    [Header("Floating Effect")]
    [Tooltip("Enable floating motion")]
    public bool enableFloatingEffect = true;
    
    [Tooltip("How high the ROV bobs up and down")]
    public float floatAmplitude = 0.05f;
    
    [Tooltip("How fast the ROV bobs up and down")]
    public float floatFrequency = 1.0f;
    
    // Tilt parameters removed - using only up/down bobbing motion

    private PackageSpawner packageSpawner;
    private Vector3 startPosition;
    private float floatTimer = 0;
    private Quaternion targetRotation;
    
    // Slowdown effect variables
    private float originalSpeed;
    private int collisionCount = 0;

    private void Start()
    {
        packageSpawner = FindObjectOfType<PackageSpawner>();
        startPosition = transform.position;
        originalSpeed = Speed; // Store the original speed
    }

    private void Update()
    {
        if (UIManager.IsROVBrokenMenuActive || UIManager.IsWarningMenuActive || UIManager.IsBatteryWarningMenuActive)
            return;

        // Check if ROV should be stopped due to too many collisions
        if (collisionCount >= maxCollisions)
        {
            Speed = 0f;
            return;
        }

        // Apply floating effect
        if (enableFloatingEffect)
        {
            ApplyFloatingEffect();
        }

        var trackingPosition = Reticle.transform.position;
        if (Vector3.Distance(trackingPosition, transform.position) < followDistance)
        {
            return;
        }

        // Calculate direction to the reticle
        Vector3 directionToTarget = trackingPosition - transform.position;
        directionToTarget.y = 0; // Ignore height differences for rotation
        
        if (directionToTarget != Vector3.zero)
        {
            // Create a rotation that aligns the ModelForwardDirection with the direction to the target
            targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);
            
            // Apply the rotation offset if needed
            if (RotationOffset != 0)
            {
                targetRotation *= Quaternion.Euler(0, RotationOffset, 0);
            }
            
            // If floating is enabled, we'll add the floating tilt in the ApplyFloatingEffect method
            if (!enableFloatingEffect)
            {
                // Smooth rotation
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        
        // Move towards the reticle, but keep our current y position when floating
        Vector3 newPosition = Vector3.MoveTowards(transform.position, 
            new Vector3(trackingPosition.x, transform.position.y, trackingPosition.z), 
            Speed * Time.deltaTime);
        
        if (!enableFloatingEffect)
        {
            // If floating is disabled, use the reticle's y position
            newPosition.y = Vector3.MoveTowards(
                new Vector3(0, transform.position.y, 0),
                new Vector3(0, trackingPosition.y, 0),
                Speed * Time.deltaTime).y;
        }
        
        transform.position = newPosition;
    }
    
    private void ApplyFloatingEffect()
    {
        // Update timer for bobbing
        floatTimer += Time.deltaTime * floatFrequency;
        
        // Calculate bobbing motion
        float yOffset = floatAmplitude * Mathf.Sin(floatTimer);
        
        // Apply bobbing to position (only up and down motion)
        Vector3 position = transform.position;
        position.y = startPosition.y + yOffset;
        transform.position = position;
        
        if (targetRotation != Quaternion.identity)
        {
            // Apply base rotation toward target (no tilt)
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var package = other.GetComponent<PackageBehaviour>();
        if (package != null)
        {
            Debug.Log("ROV collided with package");
            Destroy(other.gameObject);
            
            // Apply slowdown effect
            ApplySlowdownEffect();
            
            if (packageSpawner != null)
            {
                packageSpawner.OnPackageDelivered();
                // Only invoke OnROVDeath when we hit 5 packages
                if (packageSpawner.packagesDelivered >= 5)
                {
                    OnROVDeath?.Invoke();
                }
            }
        }
    }
    
    private void ApplySlowdownEffect()
    {
        // Increment collision count
        collisionCount++;
        
        Debug.Log($"ROV collision #{collisionCount}");
        
        // Check for second collision to show warning
        if (collisionCount == 2)
        {
            OnSecondCollision?.Invoke();
            Debug.Log("Second collision - Warning triggered!");
        }
        
        // Check for third collision to show battery warning
        if (collisionCount == 3)
        {
            OnThirdCollision?.Invoke();
            Debug.Log("Third collision - Battery Warning triggered!");
        }
        
        // Apply permanent speed reduction only after reaching the threshold
        if (collisionCount >= collisionsBeforeSlowdown)
        {
            Speed = Speed * slowdownFactor;
            Debug.Log($"Speed reduced to {Speed}");
        }
        else
        {
            Debug.Log($"No slowdown yet. Need {collisionsBeforeSlowdown - collisionCount} more collisions before slowdown begins.");
        }
        
        // Check if ROV should stop
        if (collisionCount >= maxCollisions)
        {
            Speed = 0f;
            Debug.Log("ROV has stopped after maximum collisions!");
        }
    }
}
