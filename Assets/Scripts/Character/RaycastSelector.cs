using UnityEngine;
using Farming;

namespace Character
{
    public class RaycastSelector : TileSelector
    {
        [Header("Raycast Settings")]
        [SerializeField] float maxRange = 10f;
        [SerializeField] LayerMask tileLayerMask = -1;
        [SerializeField] bool useScreenCenter = true;
        
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
                // Raycast from center of screen (good for FPS-style selection)
                ray = raycastCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            }
            else
            {
                // Raycast from mouse position (good for mouse-based selection)
                ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
            }

            FarmTile hitTile = null;

            if (Physics.Raycast(ray, out RaycastHit hit, maxRange, tileLayerMask))
            {
                if (hit.collider.TryGetComponent<FarmTile>(out hitTile))
                {
                    // Successfully found a tile
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