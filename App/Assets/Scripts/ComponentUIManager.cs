using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ComponentUIManager : MonoBehaviour
{
    [Header("UI Panel References")]
    public GameObject componentDetailPanel;
    
    [Header("TextMeshPro Text Fields")]
    public TextMeshProUGUI componentNameText;
    public TextMeshProUGUI componentTypeText;
    public TextMeshProUGUI damageStatusText;
    public TextMeshProUGUI damageDescriptionText;
    
    [Header("Other UI Elements")]
    public Button closeButton;
    public Image damageTypeIcon;
    
    [Header("Damage Type Icons")]
    public Sprite brokenIcon;
    public Sprite crackedIcon;
    public Sprite missingIcon;
    public Sprite corrodedIcon;
    public Sprite electricalIcon;
    public Sprite mechanicalIcon;
    
    [Header("State Management")]
    public bool allowInspection = true;
    public float rotationCooldown = 1f; // Seconds to wait after rotation
    
    private ComponentInfo currentComponent;
    private static ComponentUIManager instance;
    
    private float lastRotationTime = 0f;
    private float lastInteractionTime = 0f;
    
    public static ComponentUIManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ComponentUIManager>();
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Hide panel initially
        if (componentDetailPanel != null)
        {
            componentDetailPanel.SetActive(false);
        }
        
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseComponentDetails);
        }
    }
    
    public void ShowComponentDetails(ComponentInfo component)
    {
        if (component == null || componentDetailPanel == null) return;
        
        // Don't show panel if inspection is disabled or in cooldown
        if (!allowInspection || IsInRotationCooldown())
        {
            Debug.Log("ComponentUIManager: Inspection blocked - rotation cooldown active or inspection disabled");
            return;
        }
        
        currentComponent = component;
        
        // Update interaction time
        lastInteractionTime = Time.time;
        
        // Populate UI elements
        PopulateComponentInfo(component);
        
        // Show panel
        componentDetailPanel.SetActive(true);
        
        // Optional: Add fade-in animation
        StartCoroutine(FadeInPanel());
    }
    
    void PopulateComponentInfo(ComponentInfo component)
    {
        // Basic info
        if (componentNameText != null)
            componentNameText.text = component.componentName;
            
        if (componentTypeText != null)
            componentTypeText.text = $"Type: {component.componentType}";
        
        // Damage status
        if (damageStatusText != null)
            damageStatusText.text = $"Damage: {component.damageType}";
        
        // Description
        if (damageDescriptionText != null)
            damageDescriptionText.text = component.damageDescription;
        
        // Damage type icon
        if (damageTypeIcon != null)
        {
            damageTypeIcon.sprite = GetDamageTypeIcon(component.damageType);
        }
    }
    
    Sprite GetDamageTypeIcon(ComponentInfo.DamageType damageType)
    {
        switch (damageType)
        {
            case ComponentInfo.DamageType.Broken: return brokenIcon;
            case ComponentInfo.DamageType.Cracked: return crackedIcon;
            case ComponentInfo.DamageType.Missing: return missingIcon;
            case ComponentInfo.DamageType.Corroded: return corrodedIcon;
            case ComponentInfo.DamageType.Electrical: return electricalIcon;
            case ComponentInfo.DamageType.Mechanical: return mechanicalIcon;
            default: return null;
        }
    }
    
    public void CloseComponentDetails()
    {
        if (componentDetailPanel != null)
        {
            StartCoroutine(FadeOutPanel());
        }
        
        // Notify the component that inspection is closed
        if (currentComponent != null)
        {
            currentComponent.OnInspectionClosed();
            currentComponent = null;
        }
    }
    
    IEnumerator FadeInPanel()
    {
        CanvasGroup canvasGroup = componentDetailPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = componentDetailPanel.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 0f;
        float elapsedTime = 0f;
        float duration = 0.3f;
        
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    IEnumerator FadeOutPanel()
    {
        CanvasGroup canvasGroup = componentDetailPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) 
        {
            componentDetailPanel.SetActive(false);
            yield break;
        }
        
        float elapsedTime = 0f;
        float duration = 0.3f;
        float startAlpha = canvasGroup.alpha;
        
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        componentDetailPanel.SetActive(false);
    }
    
    // Public method to check if panel is open
    public bool IsPanelOpen()
    {
        return componentDetailPanel != null && componentDetailPanel.activeSelf;
    }
    
    // State management methods
    public void OnROVRotated()
    {
        lastRotationTime = Time.time;
        Debug.Log("ComponentUIManager: ROV rotation detected");
        
        // Close panel if it's open during rotation
        if (IsPanelOpen())
        {
            Debug.Log("ComponentUIManager: Closing panel due to ROV rotation");
            CloseComponentDetails();
        }
    }
    
    public void OnROVInteraction()
    {
        lastInteractionTime = Time.time;
        lastRotationTime = Time.time; // Treat any ROV interaction as potential rotation
    }
    
    public void SetInspectionEnabled(bool enabled)
    {
        allowInspection = enabled;
        Debug.Log($"ComponentUIManager: Inspection {(enabled ? "enabled" : "disabled")}");
        
        // Close panel if disabling inspection
        if (!enabled && IsPanelOpen())
        {
            CloseComponentDetails();
        }
    }
    
    bool IsInRotationCooldown()
    {
        bool inCooldown = Time.time - lastRotationTime < rotationCooldown;
        if (inCooldown)
        {
            Debug.Log($"ComponentUIManager: In rotation cooldown ({rotationCooldown - (Time.time - lastRotationTime):F1}s remaining)");
        }
        return inCooldown;
    }
    
    public bool CanInspect()
    {
        return allowInspection && !IsInRotationCooldown();
    }
}
