using Character;
using UnityEngine;

namespace Character {
    public class AnimatedController : MonoBehaviour
    {
        [SerializeField] float moveSpeed; // useful to observe for debugging
        MovementController moveController;
        Animator animator;
        protected Animator Animator { get { return animator; } }
        void Start()
        {
            // First try to find animator on this GameObject
            animator = GetComponent<Animator>();
            
            // If not found, search in children
            if (!animator)
            {
                animator = GetComponentInChildren<Animator>();
            }
            
            moveController = GetComponent<MovementController>();
            
            // Warn if animator is missing
            if (!animator)
            {
                Debug.LogWarning($"No Animator found on {gameObject.name} or its children. AnimatedController will be disabled.", this);
            }
            else
            {
                Debug.Log($"Animator found on: {animator.gameObject.name}");
            }
            
            if (!moveController)
            {
                Debug.LogError($"No MovementController found on {gameObject.name}. AnimatedController requires one.", this);
            }
        }

        public void SetTrigger(string name)
        {
            if (animator)
                animator.SetTrigger(name);
        }

        void Update()
        {
            // Skip animation updates if no animator or moveController
            if (!animator || !moveController) return;
            
            moveSpeed = moveController.GetHorizontalSpeedPercent();
            animator.SetFloat("Blend", moveSpeed);
            
            // Enhanced animation with actual movement speed (integrated from Doozy PlayerController)
            if (moveController is PhysicsMovement physicsMovement)
            {
                Rigidbody rb = physicsMovement.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float actualSpeed = new Vector3(
                        rb.linearVelocity.x,
                        0f,
                        rb.linearVelocity.z
                    ).magnitude;
                    
                    animator.SetFloat("MoveSpeed", actualSpeed, 0.25f, Time.deltaTime);
                }
            }
        }
        
        public void SetWatering(bool isWatering)
        {
            if (animator)
            {
                animator.SetBool("IsWatering", isWatering);
                Debug.Log($"SetWatering: {isWatering}"); // Debug to verify it's being called
            }
        }
    }
}
