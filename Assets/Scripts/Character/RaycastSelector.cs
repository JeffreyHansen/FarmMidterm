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
            // Always raycast from mouse position for precise control
            Vector2 mousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            Ray ray = raycastCamera.ScreenPointToRay(mousePosition);

            FarmTile hitTile = null;

            // Get all hits and find the first valid farm tile
            RaycastHit[] hits = Physics.RaycastAll(ray, maxRange, tileLayerMask);
            
            foreach (RaycastHit hit in hits)
            {
                GameObject hitObject = hit.collider.gameObject;
                
                // Skip if this object is on ignore layer (like player character)
                if (((1 << hitObject.layer) & ignoreLayerMask) != 0)
                {
                    continue;
                }
                
                // Skip if this is the player character by name (backup check)
                if (hitObject.name.Contains("Ch19") || hitObject.name.ToLower().Contains("player"))
                {
                    continue;
                }
                
                // Check for FarmTile on the hit object or its parent
                if (hit.collider.TryGetComponent<FarmTile>(out hitTile))
                {
                    break; // Found a valid tile, stop looking
                }
                else if (hit.collider.transform.parent != null && hit.collider.transform.parent.TryGetComponent<FarmTile>(out hitTile))
                {
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
                            break;
                        }
                        current = current.parent;
                    }
                    
                    if (hitTile != null)
                        break; // Found a valid tile, stop looking
                }
            }

            SetActiveTile(hitTile);

        }

        void OnDrawGizmosSelected()
        {
            if (!raycastCamera) return;
            
            // Visualize the raycast in scene view (from mouse position)
            Vector3 rayOrigin = raycastCamera.transform.position;
            Vector2 mousePosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            Ray mouseRay = raycastCamera.ScreenPointToRay(mousePosition);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rayOrigin, rayOrigin + mouseRay.direction * maxRange);
        }
    }
}