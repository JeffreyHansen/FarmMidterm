using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character 
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] public GameObject player;
        
        [Header("Camera Position")]
        [SerializeField] private float distance = 8f;              // Distance from player
        [SerializeField] private float height = 6f;                // Height above player
        
        [Header("Camera Controls")]
        [SerializeField] private bool allowZoom = true;
        [SerializeField] private float minDistance = 4f;
        [SerializeField] private float maxDistance = 15f;
        [SerializeField] private float zoomSpeed = 2f;
        
        [SerializeField] private bool allowRotation = true;
        [SerializeField] private float rotationSpeed = 100f;
        
        [Header("Smoothing")]
        [SerializeField] private float positionSmoothTime = 0.15f;
        [SerializeField] private float rotationSmoothSpeed = 5f;

        private Vector3 velocity = Vector3.zero;
        private float currentRotationY = 0f;  // Manual camera rotation around player

        void Start()
        {
            Debug.Assert(player, "CameraFollow requires a player (GameObject).");
            
            // Initialize camera position and rotation
            if (player)
            {
                // Start behind the player
                currentRotationY = player.transform.eulerAngles.y;
                
                Vector3 targetPosition = CalculateTargetPosition();
                transform.position = targetPosition;
                transform.LookAt(player.transform.position + Vector3.up * 1f); // Look at player's chest height
            }
        }

        void LateUpdate()
        {
            if (!player) return;
            
            HandleCameraControls();
            UpdateCameraPosition();
        }

        private void HandleCameraControls()
        {
            // Zoom with mouse wheel
            if (allowZoom)
            {
                float scroll = Mouse.current?.scroll.ReadValue().y ?? 0f;
                if (scroll != 0f)
                {
                    // Normalize scroll value (Mouse.current.scroll is in pixels, typically ±120)
                    distance -= (scroll / 120f) * zoomSpeed;
                    distance = Mathf.Clamp(distance, minDistance, maxDistance);
                }
            }
            
            // Rotate with Q/E keys
            if (allowRotation)
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard != null)
                {
                    if (keyboard.qKey.isPressed)
                    {
                        currentRotationY -= rotationSpeed * Time.deltaTime;
                    }
                    if (keyboard.eKey.isPressed)
                    {
                        currentRotationY += rotationSpeed * Time.deltaTime;
                    }
                }
            }
        }

        private Vector3 CalculateTargetPosition()
        {
            // Calculate position based on distance, height, and rotation
            float radians = currentRotationY * Mathf.Deg2Rad;
            
            // Position camera at an angle behind the player
            Vector3 offset = new Vector3(
                Mathf.Sin(radians) * distance,
                height,
                Mathf.Cos(radians) * distance
            );
            
            return player.transform.position + offset;
        }

        private void UpdateCameraPosition()
        {
            // Smoothly move to target position
            Vector3 targetPosition = CalculateTargetPosition();
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                positionSmoothTime
            );
            
            // Smoothly rotate to look at player (slightly above ground level)
            Vector3 lookAtPoint = player.transform.position + Vector3.up * 1f;
            Quaternion targetRotation = Quaternion.LookRotation(lookAtPoint - transform.position);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }
    }
}
