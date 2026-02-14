using UnityEngine;
using UnityEngine.InputSystem;

namespace Character 
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        MovementController moveController;
        PhysicsMovement physicsMovement;
        bool isRunning;

        void Start()
        {
            moveController = GetComponent<MovementController>();
            physicsMovement = GetComponent<PhysicsMovement>();

            Debug.Assert(moveController, "PlayerController requires a MovementController");
        }

        public void OnMove(InputValue inputValue)
        {
            Vector2 inputVector = inputValue.Get<Vector2>();
            moveController.Move(inputVector);
        }

        public void OnSprint(InputValue inputValue)
        {
            isRunning = inputValue.isPressed;
            physicsMovement?.SetRunning(isRunning);
        }

        public void OnJump(InputValue inputValue)
        {
            moveController.Jump();
        }
    }
}
