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
        [SerializeField] private int wateringLayerIndex = 0;            // 0 = Base Layer

        MovementController moveController;
        Animator animator;
        protected Animator Animator { get { return animator; } }

        int wateringStateHash;
        bool lastRendererEnabled;

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
            else
            {
                Debug.Log($"Animator found on: {animator.gameObject.name}");
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

            // Safety: layer index must exist
            if (wateringLayerIndex < 0 || wateringLayerIndex >= animator.layerCount)
                wateringLayerIndex = 0;

            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(wateringLayerIndex);

            // If watering is in a transition, you might want to keep it visible while either state is watering.
            // We'll check both current and next state during transitions.
            bool isWateringNow = info.shortNameHash == wateringStateHash;

            if (!isWateringNow && animator.IsInTransition(wateringLayerIndex))
            {
                AnimatorStateInfo next = animator.GetNextAnimatorStateInfo(wateringLayerIndex);
                isWateringNow = next.shortNameHash == wateringStateHash;
            }

            // Only set renderer when it changes (avoids spam + tiny perf win)
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
                Debug.Log($"SetWatering: {isWatering}");
            }

            // No direct renderer toggle here—renderer follows Animator state automatically.
        }
    }
}
