using UnityEngine;
using UnityEngine.InputSystem;
using Farming;

namespace Character 
{
    [RequireComponent(typeof(PlayerInput))] // Input is required and we don't store a reference
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private TileSelector tileSelector; // Can now be any selector type or SelectorManager
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
            
            // Auto-find TileSelector if not assigned (search children too)
            if (tileSelector == null)
            {
                tileSelector = GetComponentInChildren<TileSelector>();
            }
            
            Debug.Assert(tileSelector, "PlayerController requires a TileSelector.");
            
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
            
            if (tileSelector is SelectorManager manager)
            {
                manager.OnSelectorSwitchInput();
            }
            else
            {
                Debug.Log($"[PlayerController] Current TileSelector is {tileSelector?.GetType().Name}. Add SelectorManager component and multiple selectors to enable switching.");
            }
        }

        public void OnInteract(InputValue value)
        {
            FarmTile tile = tileSelector.GetSelectedTile();
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
            if (tileSelector is SelectorManager manager)
            {
                manager.OnSelectorSwitchInput();
            }
            else
            {
                Debug.Log($"[PlayerController] Current TileSelector is {tileSelector?.GetType().Name}. Add SelectorManager to enable switching.");
            }
        }
    }
}