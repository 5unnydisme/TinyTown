# Enhanced Component Info System - Unity Setup Guide

## Step-by-Step Unity Scene Setup

### Phase 1: Script Setup
1. **Add the Scripts to Your Project**
   - Ensure all scripts are in your `Assets/Scripts/` folder:
     - `ComponentInfo.cs` - Main component information script
     - `InfoLabel3D.cs` - 3D floating label script
     - `EnhancedGazeTracking.cs` - Enhanced gaze tracking
     - `ComponentUIManager.cs` - UI panel manager

### Phase 2: Create 3D Label Prefab

1. **Create Label Prefab**
   - Right-click in Project window → Create → Empty GameObject
   - Name it "InfoLabel3D_Prefab"
   - Add Component → Mesh Renderer
   - Add Component → Text Mesh
   - Add Component → InfoLabel3D script

2. **Configure Text Mesh**
   - Font: Arial or any Unity font
   - Font Size: 20
   - Anchor: Middle Center
   - Alignment: Center
   - Color: White

3. **Configure Mesh Renderer**
   - Material: Default-Material (or create a text material)

4. **Save as Prefab**
   - Drag from Hierarchy to Project window
   - Delete from scene

### Phase 3: Setup UI Canvas and Panels

**Note:** This system uses TextMeshPro for all text elements. If you haven't imported TextMeshPro yet:
- Window → TextMeshPro → Import TMP Essential Resources
- Window → TextMeshPro → Import TMP Examples and Extras (optional)

1. **Create Main Canvas**
   - Right-click in Hierarchy → UI → Canvas
   - Name it "ComponentInfoUI"
   - Canvas Scaler → UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

2. **Create Detail Panel**
   - Right-click on Canvas → UI → Panel
   - Name it "ComponentDetailPanel"
   - Anchor: Center-Middle
   - Width: 800, Height: 600
   - Add Component → Canvas Group (for fade animations)

3. **Add UI Elements to Detail Panel**
   Create these as children of ComponentDetailPanel (use TextMeshPro - UI elements):

   **Header Section:**
   - TextMeshPro - Text (UI): "ComponentNameText" (large, bold)
   - TextMeshPro - Text (UI): "ComponentTypeText" 
   - Image: "DamageTypeIcon" (64x64)

   **Damage Assessment:**
   - TextMeshPro - Text (UI): "DamageStatusText"
   - Slider: "DamageSlider" (Red/Yellow/Green colors)
   - TextMeshPro - Text (UI): "RepairPriorityText"

   **Repair Information:**
   - TextMeshPro - Text (UI): "RepairTimeText"
   - TextMeshPro - Text (UI): "RepairCostText"

   **Description:**
   - TextMeshPro - Text (UI): "DamageDescriptionText" (larger text area)

   **Requirements:**
   - TextMeshPro - Text (UI): "RequiredToolsText"
   - TextMeshPro - Text (UI): "ReplacementPartsText"

   **Controls:**
   - Button: "CloseButton" (top-right corner)

4. **Create ComponentUIManager GameObject**
   - Create Empty GameObject named "ComponentUIManager"
   - Add ComponentUIManager script
   - Assign all UI references in inspector

**Important: Creating TextMeshPro UI Elements**
To create TextMeshPro text elements:
- Right-click on ComponentDetailPanel → UI → TextMeshPro - Text (UI)
- This creates TextMeshProUGUI components (not regular Text components)
- Assign these TextMeshProUGUI components to the corresponding fields in ComponentUIManager

### Phase 4: Setup ROV with Component Info

1. **Prepare ROV Model**
   - Import your broken ROV 3D model
   - Ensure it has separate child objects for different components
   - Add colliders to each component that should be interactive

2. **Add ComponentInfo to Each Damaged Part**
   
   **Example: Broken Propeller**
   - Select propeller child object
   - Add Component → ComponentInfo script
   - Configure in Inspector:
     ```
     Component Name: "Starboard Propeller"
     Component Type: Propeller
     Damage Type: Broken
     Damage Percentage: 85
     Repair Level: Medium
     Estimated Repair Time: 1.5
     Estimated Cost: 125
     Required Tools: ["Wrench Set", "Propeller Puller"]
     Replacement Parts: ["Propeller Blade Assembly"]
     Damage Description: "Two blades completely severed..."
     Label Prefab: Drag InfoLabel3D_Prefab here
     Label Offset: (0, 0.5, 0)
     ```

   **Example: Corroded Camera**
   - Select camera child object
   - Add ComponentInfo script
   - Configure for camera-specific damage

   **Repeat for all damaged components**

3. **Setup Visual Materials**
   - Create materials for damaged components
   - Assign to Damaged Material field in ComponentInfo
   - Use emissive properties for highlight effects

### Phase 5: Camera and Gaze Tracking

1. **Setup Main Camera**
   - Ensure Main Camera has the tag "MainCamera"
   - Add EnhancedGazeTracking script to camera
   - Configure gaze settings:
     ```
     Max Gaze Distance: 10
     Gaze Layer Mask: Default (or create specific layer)
     ```

2. **Layer Setup (Optional)**
   - Create layer "Interactive" for ROV components
   - Assign all component GameObjects to this layer
   - Set EnhancedGazeTracking to only raycast this layer

### Phase 6: Input Handling for Touch

1. **Create Input Manager GameObject**
   - Create Empty GameObject named "InputManager"
   - Add this script:

```csharp
using UnityEngine;

public class ComponentInputManager : MonoBehaviour
{
    private EnhancedGazeTracking gazeTracker;
    
    void Start()
    {
        gazeTracker = FindObjectOfType<EnhancedGazeTracking>();
    }
    
    void Update()
    {
        // Handle touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleTouch();
        }
        // Handle mouse input (for testing)
        else if (Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
    }
    
    void HandleTouch()
    {
        if (gazeTracker != null)
        {
            gazeTracker.InspectCurrentComponent();
        }
    }
}
```

### Phase 7: Testing and Refinement

1. **Test Gaze Tracking**
   - Play scene
   - Look at different ROV components
   - Verify labels appear/disappear correctly

2. **Test Touch Interaction**
   - Tap on components while gazing
   - Verify detail panels open with correct information
   - Test close button functionality

3. **Adjust Visual Settings**
   - Fine-tune label offsets
   - Adjust highlight effects
   - Test fade animations

### Phase 8: Advanced Features (Optional)

1. **Add Sound Effects**
   - Hover sounds for gaze enter/exit
   - Click sounds for tap interactions
   - Ambient ROV sounds

2. **Add Particle Effects**
   - Damage sparks for electrical components
   - Rust particles for corroded parts
   - Water drip effects

3. **Add Animation**
   - Floating ROV movement
   - Rotating damaged propellers
   - Flickering lights

## Troubleshooting Common Issues

**Labels not appearing:**
- Check "hasInfo" tag on colliders
- Verify Label Prefab assignment
- Check camera raycast distance

**UI panels not showing:**
- Verify ComponentUIManager singleton
- Check UI references are assigned
- Ensure Canvas is active

**Gaze tracking not working:**
- Check camera forward direction
- Verify colliders on components
- Check layer mask settings

**Touch not working:**
- Ensure ComponentInputManager is active
- Check if UI elements block touches
- Verify camera raycast setup

## Component Configuration Examples

### Broken Engine
```
Component Name: "Main Thruster Engine"
Component Type: Engine
Damage Type: Mechanical
Damage Percentage: 90
Repair Level: Professional
Estimated Repair Time: 8.0
Estimated Cost: 850
Required Tools: ["Engine Lift", "Precision Tools", "Diagnostic Equipment"]
Replacement Parts: ["Engine Block", "Motor Assembly", "Cooling System"]
```

### Missing Battery
```
Component Name: "Power Supply Unit"
Component Type: BatteryCompartment  
Damage Type: Missing
Damage Percentage: 100
Repair Level: Replace
Estimated Repair Time: 0.5
Estimated Cost: 200
Required Tools: ["Screwdriver Set"]
Replacement Parts: ["Complete Battery Housing", "Power Cables"]
```

This setup creates a professional, educational ROV inspection experience with detailed component information and intuitive interaction!
