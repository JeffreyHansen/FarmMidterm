using System;
using UnityEngine;

namespace Character 
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] public GameObject player;
        [SerializeField] private Vector3 offset = new(0f, 5f, -3f);
        [SerializeField] private bool followRotation = true;
        [SerializeField] private bool followYAxisOnly = true; // Only follow horizontal rotation, not pitch
        [SerializeField] private float rotationSmoothness = 5f;
        [SerializeField] private bool lookAtPlayer = true; // Camera always looks down at player

        void Start()
        {
            Debug.Assert(player, "CameraFollow requires a player (GameObject).");
        }

        void LateUpdate()
        {
            // Calculate position with offset
            Vector3 rotatedOffset = offset;
            if (followRotation)
            {
                if (followYAxisOnly)
                {
                    // Only rotate the offset around Y-axis (horizontal rotation only)
                    float yRotation = player.transform.eulerAngles.y;
                    rotatedOffset = Quaternion.AngleAxis(yRotation, Vector3.up) * offset;
                }
                else
                {
                    // Full rotation following
                    rotatedOffset = player.transform.rotation * offset;
                }
            }
            
            transform.position = player.transform.position + rotatedOffset;
            
            // Handle camera rotation
            if (lookAtPlayer)
            {
                // Make camera look at the player
                transform.LookAt(player.transform.position);
            }
            else if (followRotation)
            {
                // Follow player's rotation (original behavior)
                if (followYAxisOnly)
                {
                    // Only follow Y-axis rotation, maintain camera's X and Z rotation
                    Vector3 eulerAngles = transform.eulerAngles;
                    float targetYRotation = player.transform.eulerAngles.y;
                    
                    if (rotationSmoothness > 0f)
                    {
                        eulerAngles.y = Mathf.LerpAngle(eulerAngles.y, targetYRotation, rotationSmoothness * Time.deltaTime);
                    }
                    else
                    {
                        eulerAngles.y = targetYRotation;
                    }
                    
                    transform.eulerAngles = eulerAngles;
                }
                else
                {
                    // Full rotation following
                    if (rotationSmoothness > 0f)
                    {
                        transform.rotation = Quaternion.Lerp(transform.rotation, player.transform.rotation, rotationSmoothness * Time.deltaTime);
                    }
                    else
                    {
                        transform.rotation = player.transform.rotation;
                    }
                }
            }
        }
    }
}
