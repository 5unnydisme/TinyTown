using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class ComponentInfo : MonoBehaviour
{
    [Header("Basic Component Info")]
    public string componentName = "ROV Component";
    public string manufacturerModel = "TinyTown Marine Systems";
    public ComponentType componentType;
    
    [Header("Damage Assessment")]
    public DamageType damageType;
    [Range(0, 100)]
    public float damagePercentage = 75f;
    [TextArea(3, 5)]
    public string damageDescription = "Detailed damage assessment...";
    
    [Header("Visual Elements")]
    public GameObject damageHighlight; // Red outline or effect
    public GameObject repairableIndicator; // Green checkmark if repairable
    public Material damagedMaterial;
    public Material normalMaterial;
    
    [Header("3D Label Settings")]
    public GameObject labelPrefab;
    public Vector3 labelOffset = new Vector3(0, 0.5f, 0);
    public bool showLabelOnGaze = true;
    
    [Header("Animation Settings")]
    public float highlightPulseSpeed = 2f;
    public float labelFadeSpeed = 3f;

    // Enums for categorization
    public enum ComponentType
    {
        Engine,
        Propeller,
        Camera,
        Lights,
        Hull,
        Electronics,
        BatteryCompartment,
        Sensor,
        ThrusterMount,
        Frame
    }
    
    public enum DamageType 
    { 
        Broken,      // Completely non-functional
        Cracked,     // Partially damaged, still works
        Missing,     // Component is gone
        Corroded,    // Water damage/rust
        Electrical,  // Short circuit/wiring issues
        Mechanical   // Gear/motor problems
    }
    
    // Private variables
    private GameObject currentLabel;
    private bool isInspecting = false;
    private bool isGazedAt = false;
    private Renderer componentRenderer;
    private Color originalEmissionColor;
    private ComponentUIManager uiManager;
    
    void Start()
    {
        SetupComponent();
        FindUIManager();
    }
    
    void SetupComponent()
    {
        // Get renderer component
        componentRenderer = GetComponent<Renderer>();
        
        // Setup visual feedback
        SetupVisualFeedback();
        
        // Ensure collider has proper tag
        if (!CompareTag("hasInfo"))
        {
            tag = "hasInfo";
        }
        
        // Setup child colliders if any
        SetupChildColliders();
    }
    
    void FindUIManager()
    {
        // Try to find ComponentUIManager in scene
        uiManager = ComponentUIManager.Instance;
        if (uiManager == null)
        {
            Debug.LogWarning($"ComponentUIManager not found for {componentName}. Detailed panels won't work.");
        }
    }
    
    void SetupVisualFeedback()
    {
        // Apply damaged material based on damage type
        if (componentRenderer != null && damagedMaterial != null)
        {
            componentRenderer.material = damagedMaterial;
            
            // Store original emission for pulsing effect
            if (componentRenderer.material.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = componentRenderer.material.GetColor("_EmissionColor");
            }
        }
        
        // Setup damage highlight
        if (damageHighlight != null)
        {
            damageHighlight.SetActive(false);
        }
        
        // Setup repairable indicator
        if (repairableIndicator != null)
        {
            repairableIndicator.SetActive(true);
        }
    }
    
    void SetupChildColliders()
    {
        // Add "hasInfo" tag to all colliders in this object's children
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            if (!col.CompareTag("hasInfo"))
            {
                col.tag = "hasInfo";
            }
        }
    }
    
    void Update()
    {
        // Handle highlight pulsing effect when gazed at
        if (isGazedAt && componentRenderer != null && componentRenderer.material.HasProperty("_EmissionColor"))
        {
            float pulse = Mathf.Sin(Time.time * highlightPulseSpeed) * 0.5f + 0.5f;
            Color pulseColor = Color.Lerp(originalEmissionColor, GetDamageColor(), pulse * 0.3f);
            componentRenderer.material.SetColor("_EmissionColor", pulseColor);
        }
    }
    
    public void OnGazeEnter()
    {
        isGazedAt = true;
        
        if (showLabelOnGaze && !isInspecting)
        {
            ShowSimpleLabel();
        }
        
        // Activate damage highlight
        if (damageHighlight != null)
        {
            damageHighlight.SetActive(true);
        }
        
        Debug.Log($"Gazing at {componentName} - {damageType}");
    }
    
    public void OnGazeExit()
    {
        isGazedAt = false;
        
        if (!isInspecting)
        {
            HideSimpleLabel();
        }
        
        // Deactivate damage highlight
        if (damageHighlight != null && !isInspecting)
        {
            damageHighlight.SetActive(false);
        }
        
        // Reset emission color
        if (componentRenderer != null && componentRenderer.material.HasProperty("_EmissionColor"))
        {
            componentRenderer.material.SetColor("_EmissionColor", originalEmissionColor);
        }
    }
    
    public void OnTapInspect()
    {
        isInspecting = true;
        HideSimpleLabel();
        ShowDetailedInformation();
        
        Debug.Log($"Inspecting {componentName} - Opening detailed panel");
    }
    
    public void OnInspectionClosed()
    {
        isInspecting = false;
        
        // If still being gazed at, show simple label again
        if (isGazedAt)
        {
            ShowSimpleLabel();
        }
        else
        {
            OnGazeExit(); // Clean up highlights
        }
    }
    
    void ShowSimpleLabel()
    {
        if (labelPrefab != null && currentLabel == null)
        {
            Vector3 labelPosition = transform.position + labelOffset;
            currentLabel = Instantiate(labelPrefab, labelPosition, Quaternion.identity);
            currentLabel.transform.SetParent(transform);
            
            // Set simple label text
            TextMesh textMesh = currentLabel.GetComponent<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = $"{componentName}\n{damageType} - {damagePercentage:F0}% damaged";
                textMesh.color = GetDamageColor();
                
                // Make label face camera
                StartCoroutine(FaceCameraCoroutine());
            }
            
            // Fade in effect
            StartCoroutine(FadeInLabel());
        }
    }
    
    void HideSimpleLabel()
    {
        if (currentLabel != null)
        {
            StartCoroutine(FadeOutAndDestroy());
        }
    }
    
    void ShowDetailedInformation()
    {
        if (uiManager != null)
        {
            uiManager.ShowComponentDetails(this);
        }
        else
        {
            Debug.LogWarning("ComponentUIManager not found - cannot show detailed information panel");
        }
    }
    
    Color GetDamageColor()
    {
        switch (damageType)
        {
            case DamageType.Broken: return Color.red;
            case DamageType.Missing: return Color.magenta;
            case DamageType.Cracked: return Color.yellow;
            case DamageType.Corroded: return new Color(1f, 0.5f, 0f); // Orange
            case DamageType.Electrical: return Color.cyan;
            case DamageType.Mechanical: return Color.gray;
            default: return Color.white;
        }
    }
    
    // Coroutines for smooth animations
    IEnumerator FaceCameraCoroutine()
    {
        while (currentLabel != null)
        {
            if (Camera.main != null)
            {
                currentLabel.transform.LookAt(Camera.main.transform);
                currentLabel.transform.Rotate(0, 180, 0); // Flip to face camera properly
            }
            yield return null;
        }
    }
    
    IEnumerator FadeInLabel()
    {
        if (currentLabel == null) yield break;
        
        TextMesh textMesh = currentLabel.GetComponent<TextMesh>();
        if (textMesh == null) yield break;
        
        Color originalColor = textMesh.color;
        Color transparentColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        textMesh.color = transparentColor;
        
        float elapsedTime = 0;
        while (elapsedTime < 1f / labelFadeSpeed)
        {
            float alpha = Mathf.Lerp(0, originalColor.a, elapsedTime * labelFadeSpeed);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        textMesh.color = originalColor;
    }
    
    IEnumerator FadeOutAndDestroy()
    {
        if (currentLabel == null) yield break;
        
        TextMesh textMesh = currentLabel.GetComponent<TextMesh>();
        if (textMesh == null) 
        {
            Destroy(currentLabel);
            yield break;
        }
        
        Color originalColor = textMesh.color;
        float elapsedTime = 0;
        
        while (elapsedTime < 1f / labelFadeSpeed)
        {
            float alpha = Mathf.Lerp(originalColor.a, 0, elapsedTime * labelFadeSpeed);
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(currentLabel);
    }
    
    // Public methods for external access
    public string GetFormattedDamageReport()
    {
        return $"<b>{componentName}</b>\n" +
               $"Type: {componentType}\n" +
               $"Damage: {damageType} ({damagePercentage:F0}%)\n\n" +
               $"{damageDescription}";
    }
}
