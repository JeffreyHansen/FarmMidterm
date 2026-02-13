using UnityEngine;

// TODO: Consider the benefits of refactoring to namespace Movement
namespace Character
{
    public class PhysicsMovement : MovementController
    {
        [SerializeField] float drag = 0.5f;
        [SerializeField] float rotationSpeed = 0.1f;

        [Header("Jump")]
        [SerializeField] float jumpForce = 5f;

        bool isGrounded;

        protected override void Start()
        {
            base.Start();
            rb.linearDamping = drag; // prevents sliding
        }

        public override float GetHorizontalSpeedPercent()
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            return Mathf.Clamp01(horizontalVelocity.magnitude / maxVelocity);
        }

        public override void Jump()
        {
            // Called externally by input system
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate(); // remove later if parent handles movement
            ApplyMovement();
            ClampVelocity();
            ApplyRotation();
            ApplyJump();
        }

        void ApplyMovement()
        {
            // Basic physics movement (camera-relative optional later)
            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);

            if (moveDir.sqrMagnitude > 1f)
                moveDir.Normalize();

            rb.AddForce(moveDir * acceleration, ForceMode.Acceleration);
        }

        void ApplyJump()
        {
            // If parent class buffers jump input, handle it here
            if (jumpInput && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
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

        void CheckGround(Collision collision)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                // If surface is mostly upward-facing, treat as ground
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
            // Clamp horizontal velocity while preserving vertical
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (horizontalVelocity.magnitude > maxVelocity)
            {
                horizontalVelocity = horizontalVelocity.normalized * maxVelocity;
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
        }

        void ApplyRotation()
        {
            Vector3 direction = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if (direction.magnitude > 0.5f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );
            }
        }
    }
}
