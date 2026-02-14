using UnityEngine;

// TODO: Consider the benefits of refactoring to namespace Movement
namespace Character
{
    public class PhysicsMovement : MovementController
    {
        [SerializeField] float drag = 0.5f;
        [SerializeField] float rotationSpeed = 10f; // Degrees-per-second feel (used as Slerp speed)
        
        [Header("Speed Settings")]
        [SerializeField] float walkSpeed = 2.5f;
        [SerializeField] float runSpeed = 5.5f;

        [Header("Jump")]
        [SerializeField] float jumpForce = 5f;
        
        [Header("Camera")]
        [SerializeField] Transform cameraTransform;

        bool isGrounded;
        bool isRunning;
        Vector3 lastInputDirection; // Track last valid input direction for rotation

        protected override void Start()
        {
            base.Start();
            rb.linearDamping = drag; // prevents sliding
            
            // Setup camera reference if not assigned
            if (!cameraTransform)
                cameraTransform = Camera.main.transform;
                
            // Setup rigidbody constraints to prevent unwanted rotation
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                           RigidbodyConstraints.FreezeRotationZ;
                           
            // Ensure rigidbody settings prevent floating
            rb.useGravity = true;
            if (rb.mass <= 0) rb.mass = 1f; // Ensure reasonable mass
        }

        public override float GetHorizontalSpeedPercent()
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float currentMaxSpeed = isRunning ? runSpeed : walkSpeed;
            return Mathf.Clamp01(horizontalVelocity.magnitude / currentMaxSpeed);
        }

        // Called by the base controller when jump input is triggered
        public override void Jump()
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }

        protected override void FixedUpdate()
        {
            // DON'T call base.FixedUpdate() to avoid double movement
            // base.FixedUpdate(); // REMOVED - was causing double movement
            ApplyMovement();
            ClampVelocity();
            ApplyRotation();
        }

        void ApplyMovement()
        {
            // Skip if no input
            if (moveInput.sqrMagnitude < 0.01f)
                return;
            
            // Camera-relative movement direction
            Vector3 moveDirection;
            if (cameraTransform)
            {
                Vector3 camForward = cameraTransform.forward;
                Vector3 camRight = cameraTransform.right;
                
                camForward.y = 0f;
                camRight.y = 0f;
                
                camForward.Normalize();
                camRight.Normalize();
                
                moveDirection = camForward * moveInput.y + camRight * moveInput.x;
            }
            else
            {
                moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            }

            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();

            // Store for rotation (only when there's actual input)
            lastInputDirection = moveDirection;

            // Apply force
            rb.AddForce(moveDirection * acceleration, ForceMode.Acceleration);
        }

        // -------- COLLISION SUPPORT (Week 2–3 integration) --------
        void OnCollisionEnter(Collision collision)
        {
            CheckGround(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            CheckGround(collision);
        }
        
        void OnCollisionExit(Collision collision)
        {
            // Simplified ground exit detection
            // Wait a frame then check if we're still touching ground
            Invoke(nameof(CheckGroundExit), 0.1f);
        }
        
        void CheckGroundExit()
        {
            // Use a small downward raycast to check if still grounded
            if (Physics.Raycast(transform.position, Vector3.down, 0.1f))
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
        }

        void CheckGround(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // Upward-facing surface = ground
                if (Vector3.Dot(contact.normal, Vector3.up) > 0.5f)
                {
                    isGrounded = true;
                    return;
                }
            }
        }
        // ----------------------------------------------------------

        void ClampVelocity()
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            
            // Use current speed setting based on running state
            float currentMaxSpeed = isRunning ? runSpeed : walkSpeed;

            if (horizontalVelocity.magnitude > currentMaxSpeed)
            {
                horizontalVelocity = horizontalVelocity.normalized * currentMaxSpeed;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }

        void ApplyRotation()
        {
            // Only rotate when there is active input (prevents phantom spinning)
            if (moveInput.sqrMagnitude < 0.01f)
                return;
            
            // Rotate toward input direction, not velocity
            if (lastInputDirection.sqrMagnitude < 0.01f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(lastInputDirection.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
        
        public void SetRunning(bool running)
        {
            isRunning = running;
        }
    }
}
