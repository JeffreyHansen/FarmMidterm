using UnityEngine;
using UnityEngine.InputSystem;
using Farming;

namespace Character 
{
    [RequireComponent(typeof(PlayerInput))] // Input is required and we don't store a reference
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private SelectorManager selectorManager; // Use SelectorManager instead of single selector
        MovementController moveController;
        PhysicsMovement physicsMovement; // Reference to physics movement for running
        AnimatedController animatedController;
        
        bool isRunning;

        void Start()
        {
            moveController = GetComponent<MovementController>();
            physicsMovement = GetComponent<PhysicsMovement>();
            animatedController = GetComponent<AnimatedController>();

            // TODO: Consider Debug.Assert vs RequireComponent(typeof(...))
            Debug.Assert(moveController, "PlayerController requires a MovementController");
            
            // Auto-find SelectorManager if not assigned
            if (selectorManager == null)
            {
                selectorManager = GetComponent<SelectorManager>();
            }
            
            Debug.Assert(selectorManager, "PlayerController requires a SelectorManager.");
            
            if (!animatedController)
            {
                Debug.LogWarning($"No AnimatedController found on {gameObject.name}. Animations will be disabled.", this);
            }
        }
        public void OnMove(InputValue inputValue)
        {
            Vector2 inputVector = inputValue.Get<Vector2>();
            moveController.Move(inputVector);
            
            // Update running state based on shift key (integrated from Doozy PlayerController)
            bool newRunningState = Keyboard.current.leftShiftKey.isPressed;
            if (newRunningState != isRunning)
            {
                isRunning = newRunningState;
                if (physicsMovement)
                    physicsMovement.SetRunning(isRunning);
            }
        }

        public void OnJump(InputValue inputValue)
        {
            moveController.Jump();
        }
        
        public void OnSwitchSelector(InputValue inputValue)
        {
            if (!inputValue.isPressed) return;
            
            if (selectorManager != null)
            {
                selectorManager.OnSelectorSwitchInput();
            }
            else
            {
                Debug.LogWarning("[PlayerController] No SelectorManager found for selector switching.");
            }
        }

        public void OnInteract(InputValue value)
        {
            Debug.Log("[PlayerController] OnInteract called");
            FarmTile tile = selectorManager.GetSelectedTile();
            Debug.Log($"[PlayerController] Selected tile: {(tile != null ? tile.gameObject.name : "null")}");
            if (tile != null)
            {
                tile.Interact(); // updates the condition, play the anim after
                switch (tile.GetCondition)
                {
                    case FarmTile.Condition.Tilled: 
                        // Handle tilling animation if needed
                        break;
                    case FarmTile.Condition.Watered: 
                        if (animatedController)
                        {
                            // Use IsWatering bool with proper timing
                            animatedController.SetWatering(true);
                            // Adjust timing to match your animation length (try 2.5f or longer)
                            Invoke(nameof(StopWatering), 2.5f);
                        }
                        break;
                    default: break;
                }
            }
        }
        
        // Direct watering trigger (integrated from Doozy PlayerController)
        public void OnWater(InputValue value)
        {
            if (!value.isPressed || !animatedController) return;
            // Use IsWatering bool parameter since that's what you have
            animatedController.SetWatering(true);
            // Adjust 2.5f to match your animation duration
            Invoke(nameof(StopWatering), 2.5f);
        }
        
        void StopWatering()
        {
            if (animatedController)
                animatedController.SetWatering(false);
        }
        
        // Debug method to switch selector types (for testing)
        [ContextMenu("Switch to Next Selector")]
        void SwitchSelector()
        {
            if (selectorManager != null)
            {
                selectorManager.OnSelectorSwitchInput();
            }
            else
            {
                Debug.Log("[PlayerController] No SelectorManager found. Add SelectorManager component to enable switching.");
            }
        }
    }
}