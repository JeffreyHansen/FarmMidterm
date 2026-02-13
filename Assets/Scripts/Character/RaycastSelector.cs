using UnityEngine;
using UnityEngine.InputSystem;
using Farming;

namespace Character
{
    public class RaycastSelector : TileSelector
    {
        [Header("Raycast Settings")]
        [SerializeField] float maxRange = 10f;
        [SerializeField] LayerMask tileLayerMask = -1;
        [SerializeField] bool useScreenCenter = true; // Default to true for rotating camera
        [SerializeField] LayerMask ignoreLayerMask = 0; // Player and other objects to ignore
        
        [Header("Camera Setup")]
        [SerializeField] Camera raycastCamera;
        
        void Start()
        {
            // Auto-assign camera if not set
            if (!raycastCamera)
                raycastCamera = Camera.main;
        }

        void Update()
        {
            PerformRaycast();
        }

        void PerformRaycast()
        {
            Ray ray;
            
            if (useScreenCenter)
            {
                // Raycast from center of screen (ideal for rotating camera that follows player facing)
                ray = raycastCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            }
            else
            {
                // Raycast from mouse position (good for free-look camera or mouse-based selection)
                Vector2 mousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
                ray = raycastCamera.ScreenPointToRay(mousePosition);
            }

            FarmTile hitTile = null;

            // Get all hits and find the first valid farm tile
            RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, tileLayerMask);
            Debug.Log($"[RaycastSelector] Found {hits.Length} raycast hits");
            
            foreach (RaycastHit hit in hits)
            {
                GameObject hitObject = hit.collider.gameObject;
                Debug.Log($"[RaycastSelector] Checking hit: {hitObject.name} on layer {hitObject.layer}");
                
                // Skip if this object is on ignore layer (like player character)
                if (((1 << hitObject.layer) & ignoreLayerMask) != 0)
                {
                    Debug.Log($"[RaycastSelector] Ignoring {hitObject.name} (ignore layer)");
                    continue;
                }
                
                // Skip if this is the player character by name (backup check)
                if (hitObject.name.Contains("Ch19") || hitObject.name.ToLower().Contains("player"))
                {
                    Debug.Log($"[RaycastSelector] Ignoring {hitObject.name} (player character)");
                    continue;
                }
                
                // Check for FarmTile on the hit object or its parent
                if (hit.collider.TryGetComponent<FarmTile>(out hitTile))
                {
                    Debug.Log($"[RaycastSelector] Found FarmTile on {hitObject.name}");
                    break; // Found a valid tile, stop looking
                }
                else if (hit.collider.transform.parent != null && hit.collider.transform.parent.TryGetComponent<FarmTile>(out hitTile))
                {
                    Debug.Log($"[RaycastSelector] Found FarmTile on parent of {hitObject.name}");
                    break; // Found a valid tile, stop looking
                }
                else
                {
                    // Check if we hit a child of a FarmTile
                    Transform current = hit.collider.transform;
                    while (current != null && hitTile == null)
                    {
                        if (current.TryGetComponent<FarmTile>(out hitTile))
                        {
                            Debug.Log($"[RaycastSelector] Found FarmTile on ancestor {current.name} of {hitObject.name}");
                            break;
                        }
                        current = current.parent;
                    }
                    
                    if (hitTile != null)
                        break; // Found a valid tile, stop looking
                    else
                        Debug.Log($"[RaycastSelector] No FarmTile found on {hitObject.name} or its hierarchy");
                }
            }

            SetActiveTile(hitTile);

        }

        void OnDrawGizmosSelected()
        {
            if (!raycastCamera) return;
            
            // Visualize the raycast in scene view
            Vector3 rayOrigin = raycastCamera.transform.position;
            Vector3 rayDirection;
            
            if (useScreenCenter)
            {
                rayDirection = raycastCamera.transform.forward;
            }
            else
            {
                Vector3 mousePos = Input.mousePosition;
                Ray mouseRay = raycastCamera.ScreenPointToRay(mousePos);
                rayDirection = mouseRay.direction;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * maxRange);
        }
    }
}