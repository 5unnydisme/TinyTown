using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ROVCenteringTool : MonoBehaviour
{
    [Header("Centering Settings")]
    [Tooltip("Auto-center on Start (useful for testing)")]
    public bool centerOnStart = false;
    
    [Tooltip("Show debug information")]
    public bool showDebugInfo = true;
    
    [Header("Preview")]
    [Tooltip("Show the calculated center point in the scene view")]
    public bool showCenterGizmo = true;
    
    private Vector3 calculatedCenter;
    private bool hasCenterCalculated = false;
    
    void Start()
    {
        if (centerOnStart)
        {
            CenterROVComponents();
        }
    }
    
    /// <summary>
    /// Centers all child components around this GameObject's pivot point
    /// </summary>
    [ContextMenu("Center ROV Components")]
    public void CenterROVComponents()
    {
        if (transform.childCount == 0)
        {
            Debug.LogWarning("ROVCenteringTool: No child objects found to center!");
            return;
        }
        
        // Calculate the center point of all child objects
        Vector3 centerPoint = CalculateCenterOfChildren();
        
        if (showDebugInfo)
        {
            Debug.Log($"ROVCenteringTool: Calculated center point: {centerPoint}");
            Debug.Log($"ROVCenteringTool: Current parent position: {transform.position}");
        }
        
        // Calculate the offset needed to center everything
        Vector3 offset = transform.position - centerPoint;
        
        if (showDebugInfo)
        {
            Debug.Log($"ROVCenteringTool: Applying offset: {offset}");
        }
        
        // Move all child objects by the offset
        foreach (Transform child in transform)
        {
            child.position += offset;
        }
        
        calculatedCenter = centerPoint;
        hasCenterCalculated = true;
        
        if (showDebugInfo)
        {
            Debug.Log($"ROVCenteringTool: Centering complete! ROV should now rotate around its true center.");
            Debug.Log($"ROVCenteringTool: Moved {transform.childCount} child objects.");
        }
    }
    
    /// <summary>
    /// Calculates the geometric center of all child objects
    /// </summary>
    private Vector3 CalculateCenterOfChildren()
    {
        if (transform.childCount == 0)
            return transform.position;
        
        Vector3 totalPosition = Vector3.zero;
        int validChildren = 0;
        
        // Sum up all child positions
        foreach (Transform child in transform)
        {
            // Skip inactive children
            if (!child.gameObject.activeInHierarchy)
                continue;
                
            totalPosition += child.position;
            validChildren++;
        }
        
        if (validChildren == 0)
            return transform.position;
        
        // Return the average position (geometric center)
        return totalPosition / validChildren;
    }
    
    /// <summary>
    /// Resets all child positions to their original state (undo centering)
    /// Note: This only works if you haven't moved the objects since centering
    /// </summary>
    [ContextMenu("Reset to Original Positions")]
    public void ResetToOriginalPositions()
    {
        if (!hasCenterCalculated)
        {
            Debug.LogWarning("ROVCenteringTool: No centering operation to undo!");
            return;
        }
        
        Vector3 reverseOffset = calculatedCenter - transform.position;
        
        foreach (Transform child in transform)
        {
            child.position += reverseOffset;
        }
        
        hasCenterCalculated = false;
        
        if (showDebugInfo)
        {
            Debug.Log("ROVCenteringTool: Reset to original positions complete.");
        }
    }
    
    /// <summary>
    /// Shows information about the current ROV setup
    /// </summary>
    [ContextMenu("Show ROV Info")]
    public void ShowROVInfo()
    {
        Debug.Log($"=== ROV Centering Tool Info ===");
        Debug.Log($"Parent GameObject: {gameObject.name}");
        Debug.Log($"Parent Position: {transform.position}");
        Debug.Log($"Child Count: {transform.childCount}");
        Debug.Log($"Has Been Centered: {hasCenterCalculated}");
        
        if (transform.childCount > 0)
        {
            Vector3 currentCenter = CalculateCenterOfChildren();
            Debug.Log($"Current Center of Children: {currentCenter}");
            Debug.Log($"Distance from Parent: {Vector3.Distance(transform.position, currentCenter)}");
        }
        
        Debug.Log($"=== Child Objects ===");
        foreach (Transform child in transform)
        {
            Debug.Log($"- {child.name}: {child.position} (Active: {child.gameObject.activeInHierarchy})");
        }
    }
    
    // Gizmos for visualization in Scene view
    void OnDrawGizmos()
    {
        if (!showCenterGizmo)
            return;
            
        // Draw parent position
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
        
        // Draw calculated center if available
        if (hasCenterCalculated)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(calculatedCenter, 0.08f);
        }
        else if (transform.childCount > 0)
        {
            // Draw current center of children
            Vector3 currentCenter = CalculateCenterOfChildren();
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentCenter, 0.08f);
            
            // Draw line between parent and center
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, currentCenter);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showCenterGizmo)
            return;
            
        // Draw more detailed info when selected
        if (transform.childCount > 0)
        {
            Vector3 currentCenter = CalculateCenterOfChildren();
            
            // Draw all child positions
            Gizmos.color = Color.cyan;
            foreach (Transform child in transform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    Gizmos.DrawWireCube(child.position, Vector3.one * 0.05f);
                }
            }
        }
    }
}
