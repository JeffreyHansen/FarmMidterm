using Character;
using UnityEngine;

namespace Character
{
    public class AnimatedController : MonoBehaviour
    {
        [SerializeField] float moveSpeed; // useful to observe for debugging
        [SerializeField] private Renderer wateringCanRenderer;

        [Header("Watering State Detection")]
        [SerializeField] private string wateringStateName = "Watering"; // <-- must match Animator state name
        [SerializeField] private int wateringLayerIndex = 1;            // 1 = UpperBody Layer

        MovementController moveController;
        Animator animator;
        protected Animator Animator { get { return animator; } }

        int wateringStateHash;
        bool lastRendererEnabled;
        bool forceToolVisible; // Keep tool visible even if animator state changes

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

            if (!animator)
            {
                Debug.LogWarning($"No Animator found on {gameObject.name} or its children. AnimatedController will be disabled.", this);
            }

            if (!moveController)
            {
                Debug.LogError($"No MovementController found on {gameObject.name}. AnimatedController requires one.", this);
            }

            // Cache hash for fast comparisons
            wateringStateHash = Animator.StringToHash(wateringStateName);

            // Default: hide watering can
            if (wateringCanRenderer)
            {
                wateringCanRenderer.enabled = false;
                lastRendererEnabled = false;
            }
        }

        public void SetTrigger(string name)
        {
            if (animator)
                animator.SetTrigger(name);
        }

        void Update()
        {
            if (!animator || !moveController) return;

            moveSpeed = moveController.GetHorizontalSpeedPercent();
            animator.SetFloat("Blend", moveSpeed);

            // Enhanced animation with actual movement speed
            if (moveController is PhysicsMovement physicsMovement)
            {
                Rigidbody rb = physicsMovement.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float actualSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;
                    animator.SetFloat("MoveSpeed", actualSpeed, 0.25f, Time.deltaTime);
                }
            }

            UpdateWateringCanVisibility();
        }

        void UpdateWateringCanVisibility()
        {
            if (!wateringCanRenderer) return;

            // If forced visible (during animation), keep it visible
            if (forceToolVisible)
            {
                if (!lastRendererEnabled)
                {
                    wateringCanRenderer.enabled = true;
                    lastRendererEnabled = true;
                }
                return;
            }

            // Safety: layer index must exist
            if (wateringLayerIndex < 0 || wateringLayerIndex >= animator.layerCount)
                wateringLayerIndex = 0;

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(wateringLayerIndex);

            // If watering is in a transition, keep it visible
            bool isWateringNow = info.shortNameHash == wateringStateHash;

            if (!isWateringNow && animator.IsInTransition(wateringLayerIndex))
            {
                AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(wateringLayerIndex);
                isWateringNow = next.shortNameHash == wateringStateHash;
            }

            // Only set renderer when it changes
            if (isWateringNow != lastRendererEnabled)
            {
                wateringCanRenderer.enabled = isWateringNow;
                lastRendererEnabled = isWateringNow;
            }
        }

        public void SetWatering(bool isWatering)
        {
            if (animator)
            {
                animator.SetBool("IsWatering", isWatering);
                
                // Force animator to process the state change immediately
                if (!isWatering)
                {
                    // Force animator to update
                    animator.Update(0f);
                    
                    // Double-check after a frame if we're still stuck
                    StartCoroutine(VerifyAnimatorStateExit());
                }
            }

            // Force tool visible during entire animation
            forceToolVisible = isWatering;
        }

        /// <summary>
        /// Restart the watering animation from the beginning without transitioning out.
        /// This provides a smooth restart when chaining multiple watering actions.
        /// </summary>
        public void RestartWateringAnimation()
        {
            if (animator)
            {
                // Directly restart the watering animation at frame 0 on the UpperBody layer
                // This avoids the jumpy transition through the empty state
                animator.Play("Watering", wateringLayerIndex, 0f);
            }
        }

        private System.Collections.IEnumerator VerifyAnimatorStateExit()
        {
            yield return null; // Wait one frame
            
            if (animator != null)
            {
                AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(wateringLayerIndex);
                if (currentState.shortNameHash == wateringStateHash)
                {
                    Debug.LogWarning("[AnimatedController] Animator stuck in watering state, forcing exit to empty state");
                    // Force the UpperBody layer to the "empty" state (default state on layer 1)
                    animator.Play("empty", wateringLayerIndex, 0f);
                }
            }
        }

        /// <summary>
        /// Animation Event callback for showing/hiding tools.
        /// Called by animation events in the Animator.
        /// </summary>
        public void SetTool(string tool)
        {
            if (wateringCanRenderer)
            {
                // Show watering can only if tool is "WateringCan"
                wateringCanRenderer.enabled = (tool == "WateringCan");
            }
        }
    }
}
