using UnityEngine;
using UnityEngine.InputSystem;

namespace HW3
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float walkSpeed = 2.5f;
        [SerializeField] private float runSpeed = 5.5f;

        [Header("Animation")]
        [SerializeField] private Animator animator;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;

        private Vector2 moveInput;

        void Awake()
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationZ;

            // Fallback if not assigned
            if (!cameraTransform)
                cameraTransform = Camera.main.transform;
        }

        // ===== INPUT =====
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        // One-shot watering (trigger-based)
        public void OnWater(InputValue value)
        {
            if (!value.isPressed || !animator) return;
            animator.SetTrigger("Water");
        }

        void FixedUpdate()
        {
            ApplyVelocity();
            UpdateFacingDirection();
            UpdateAnimator();
        }

        // ===== MOVEMENT (CAMERA-RELATIVE) =====
        private Vector3 currentMoveDirection;

        private void ApplyVelocity()
        {
            bool isRunning = Keyboard.current.leftShiftKey.isPressed;
            float speed = isRunning ? runSpeed : walkSpeed;

            // Camera directions (flattened)
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight   = cameraTransform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            camForward.Normalize();
            camRight.Normalize();

            // Build movement relative to camera
            currentMoveDirection =
                camForward * moveInput.y +
                camRight   * moveInput.x;

            rb.linearVelocity = new Vector3(
                currentMoveDirection.x * speed,
                rb.linearVelocity.y,
                currentMoveDirection.z * speed
            );
        }

        // ===== ROTATION =====
        private void UpdateFacingDirection()
        {
            Vector3 horizontal = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

            if (horizontal.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(horizontal);
            }
        }

        // ===== ANIMATION =====
        private void UpdateAnimator()
        {
            if (!animator) return;

            float speed = new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            ).magnitude;

            animator.SetFloat("MoveSpeed", speed, 0.25f, Time.deltaTime);
        }
    }
}
