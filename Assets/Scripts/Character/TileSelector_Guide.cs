// TileSelector System - Implementation Guide
// ===========================================

// OVERVIEW
// --------
// The TileSelector system now supports multiple selection methods:
// 1. PointSelector    - Trigger-based selection (original)
// 2. RaycastSelector  - Camera/mouse raycast selection
// 3. SphereSelector   - Proximity-based selection with smart targeting
// 4. GridSelector     - Grid-based directional selection
// 5. SelectorManager  - Allows switching between different selectors

// SETUP INSTRUCTIONS
// ==================

// OPTION 1: Single Selector Setup
// --------------------------------
// 1. Add ONE of these components to your player GameObject:
//    - PointSelector: Requires a trigger collider
//    - RaycastSelector: Requires a camera reference
//    - SphereSelector: Uses sphere overlap detection
//    - GridSelector: Uses arrow keys for grid navigation
// 2. Assign the selector to PlayerController's tileSelector field

// OPTION 2: Multi-Selector Setup (RECOMMENDED)
// ---------------------------------------------
// 1. Add SelectorManager component to your player GameObject
// 2. Add multiple selector components (PointSelector, RaycastSelector, etc.)
// 3. Assign SelectorManager to PlayerController's tileSelector field
// 4. Press TAB during play to switch between selectors

// COMPONENT DETAILS
// =================

// PointSelector
// -------------
// Requirements: Trigger collider on player
// Usage: Walk near tiles to select them
// Best for: Close-range farming, exploration

// RaycastSelector
// ---------------
// Requirements: Camera reference
// Settings:
//   - maxRange: How far the ray can detect tiles
//   - useScreenCenter: true = center crosshair, false = mouse cursor
//   - tileLayerMask: Which layers contain tiles
// Usage: Look at tiles to select them
// Best for: Precision selection, FPS-style interaction

// SphereSelector
// --------------
// Requirements: None (auto-configures)
// Settings:
//   - detectionRadius: Range of tile detection
//   - preferForwardTiles: Prefer tiles in front of player
//   - forwardBias: How much to prefer forward tiles
//   - updateRate: How often to check for tiles (performance)
// Usage: Automatically selects nearest tile (optionally in front)
// Best for: Casual play, mobile controls, accessibility

// GridSelector
// ------------
// Requirements: Tiles arranged in a grid pattern
// Settings:
//   - gridSize: Dimensions of the tile grid
//   - gridSpacing: Distance between grid positions
//   - gridOrigin: Starting point of the grid
//   - Movement keys: Arrow keys to navigate
//   - wrapAroundGrid: Whether to wrap at edges
// Usage: Arrow keys to move selection in grid
// Best for: Turn-based gameplay, precise tile management

// SelectorManager
// ---------------
// Requirements: Other selector components on same GameObject
// Settings:
//   - selectors: List of available selectors (auto-populated)
//   - switchKey: Key to cycle through selectors (default: TAB)
//   - showSelectorType: Display current selector on screen
// Usage: TAB to switch between different selection methods
// Best for: Testing, different gameplay modes, player preference

// INTEGRATION EXAMPLES
// ====================

// Example 1: Setup for Different Game Modes
// ------------------------------------------
/*
GameObject playerObject;
SelectorManager manager = playerObject.AddComponent<SelectorManager>();

// Add all selector types
playerObject.AddComponent<PointSelector>();
playerObject.AddComponent<RaycastSelector>();
playerObject.AddComponent<SphereSelector>();
playerObject.AddComponent<GridSelector>();

// PlayerController will automatically use the SelectorManager
*/

// Example 2: Programmatic Selector Switching
// -------------------------------------------
/*
public class GameModeManager : MonoBehaviour
{
    public SelectorManager selectorManager;
    
    public void SetExplorationMode() 
    { 
        selectorManager.SwitchToSelectorByType<PointSelector>(); 
    }
    
    public void SetPrecisionMode() 
    { 
        selectorManager.SwitchToSelectorByType<RaycastSelector>(); 
    }
    
    public void SetCasualMode() 
    { 
        selectorManager.SwitchToSelectorByType<SphereSelector>(); 
    }
    
    public void SetGridMode() 
    { 
        selectorManager.SwitchToSelectorByType<GridSelector>(); 
    }
}
*/

// TROUBLESHOOTING
// ===============

// Problem: Selector not working
// Solution: Check that tiles have colliders and are on correct layer

// Problem: RaycastSelector not detecting tiles
// Solution: Verify camera is assigned and tiles have colliders

// Problem: GridSelector not finding tiles
// Solution: Adjust gridSpacing and ensure tiles are arranged in grid

// Problem: SelectorManager not switching
// Solution: Verify multiple selectors are added to same GameObject

// Problem: Performance issues with SphereSelector
// Solution: Reduce updateRate or detectionRadius

// PERFORMANCE TIPS
// ================
// - Use appropriate updateRate for SphereSelector (default: 10fps)
// - Use layer masks to limit raycast/overlap checks
// - GridSelector caches tile positions for better performance
// - SelectorManager only enables one selector at a time

// CUSTOMIZATION
// =============
// - Inherit from TileSelector to create custom selection methods
// - Override SetActiveTile() to add custom highlighting effects
// - Add new input methods to existing selectors
// - Create composite selectors that use multiple techniques