# ROV Auto-Centering Feature

## Overview
The ROV inspection system now includes automatic centering functionality to fix rotation issues. When an ROV is placed, all child components are automatically centered around the parent's pivot point, ensuring smooth rotation.

## How It Works

### Automatic Centering
- **Enabled by default**: The `InputManager` has `autoCenterROV = true` by default
- **Triggered on placement**: When an ROV is successfully placed via AR, the centering happens automatically
- **Algorithm**: Calculates the center point of ALL child components (active and inactive) and adjusts their positions relative to the parent
- **Includes inactive objects**: Highlight outlines and other disabled child objects are also centered properly

### Screen Coordinate Placement ✅ **ACTIVE**
- **User Action**: Tap directly where you want the ROV to appear
- **ROV Placement**: Appears at the exact tap location on detected AR planes
- **Precise Control**: ROV places exactly where your finger touches the screen
- **Benefits**: 
  - Direct placement control
  - Intuitive tap-to-place behavior
  - Places ROV exactly where intended
  - Natural AR interaction pattern

### Manual Centering (Optional)
If you need to center an ROV manually or disable auto-centering:

```csharp
// Get reference to InputManager
InputManager inputManager = FindObjectOfType<InputManager>();

// Center specific ROV manually
inputManager.CenterROVComponents(yourROVGameObject);

// Disable auto-centering if needed
inputManager.autoCenterROV = false;
```

## ROV Structure Requirements

For proper centering and rotation, your ROV should be structured as:

```
ROV_Parent (Empty GameObject)
├── PlaceContent script
├── Component_1 (with ComponentInfo)
│   └── Highlight_1 (disabled GameObject for damage outline)
├── Component_2 (with ComponentInfo)
│   └── Highlight_2 (disabled GameObject for damage outline)
├── Component_3 (with ComponentInfo)
│   └── Highlight_3 (disabled GameObject for damage outline)
└── ... other components
```

### Key Points:
1. **Parent GameObject**: Should be empty (no mesh/visual components)
2. **All visible parts**: Should be children of the parent
3. **Highlight objects**: Inactive highlight/outline objects are automatically included in centering
4. **Colliders**: Each interactive component needs a collider with "hasInfo" tag
5. **ComponentInfo**: Each inspectable component needs the ComponentInfo script

## Debug Information

The system provides detailed debug logs:
- `"All ROV components (including highlights) successfully centered around parent pivot"`
- `"PlaceContent: Placing ROV at world position (x, y, z)!"`
- `"PlaceContent: AR raycast found X planes at screen position (x, y)"`
- `"InputManager: Screen position normalized: (x, y)"` - should be between 0-1
- Component count and offset calculations (includes inactive objects)
- Placement confirmation messages

## Troubleshooting

### ROV Rotates Around Wrong Point
- **Cause**: Components not properly centered
- **Solution**: System auto-centers on placement, but you can manually call `CenterROVComponents()`

### Highlight Outlines Not Centered
- **Cause**: Previous version only centered active objects
- **Solution**: ✅ **FIXED** - System now centers ALL child objects (active and inactive)
- **Verification**: Check debug log shows correct count including highlights

### Components Not Interactive After Centering
- **Cause**: Colliders or tags missing
- **Solution**: Ensure each component has:
  - Collider component (Is Trigger: unchecked)
  - "hasInfo" tag
  - ComponentInfo script

### Auto-Centering Not Working
- **Check**: `InputManager.autoCenterROV` is true
- **Check**: InputManager and PlaceContent are properly connected
- **Debug**: Look for "InputManager: ROV placed successfully" message

### ROV Placement Issues (Not Placing Where You Tap)
- **Cause**: Screen coordinate conversion problems or AR tracking issues
- **Solutions**:
  1. **Check AR plane detection**: Ensure AR planes are being detected (look for plane visualization)
  2. **Validate screen coordinates**: Debug logs will show if coordinates are within screen bounds
  3. **Camera setup**: Ensure AR Camera is properly configured
  4. **Lighting**: AR tracking works better with good lighting
- **Debug logs to check**:
  - `"InputManager: Screen position normalized: (x, y)"` - should be between 0-1
  - `"PlaceContent: AR raycast found X planes"` - should be > 0
  - `"PlaceContent: Screen position (x,y) is outside screen bounds!"` - indicates coordinate issue

### Current Placement Behavior:
1. **User taps specific location** on detected AR plane
2. **System uses exact tap coordinates** for AR raycast
3. **ROV appears at tap location** (precise placement)
4. **Auto-centering activates** for perfect rotation
5. **Component inspection ready** - tap individual parts to inspect

## Performance Notes
- Centering happens once per ROV placement
- Screen coordinate placement provides direct user control
- Minimal performance impact during placement
- No ongoing calculations during rotation
- Debug logs can be removed for production builds

## Integration Status
✅ **Complete**: Auto-centering is fully integrated into the placement workflow
✅ **Active**: Screen coordinate placement for precise ROV positioning
✅ **Tested**: Works with existing AR placement and component inspection
✅ **Backward Compatible**: Existing ROV setups will work without modification
✅ **Direct Control**: ROV places exactly where user taps
