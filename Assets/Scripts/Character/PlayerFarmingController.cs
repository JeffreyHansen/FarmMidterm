using UnityEngine;
using UnityEngine.InputSystem;
using Farming;

namespace Character
{
    public class PlayerFarmingController : MonoBehaviour
    {
        [SerializeField] private SelectorManager selectorManager;

        AnimatedController animatedController;

        void Start()
        {
            animatedController = GetComponent<AnimatedController>();

            if (selectorManager == null)
                selectorManager = GetComponent<SelectorManager>();

            Debug.Assert(selectorManager, "PlayerFarmingController requires a SelectorManager.");

            if (!animatedController)
            {
                Debug.LogWarning($"No AnimatedController found on {gameObject.name}. Farming animations disabled.", this);
            }
        }

        // =============================
        // SELECTOR SWITCHING
        // =============================
        public void OnSwitchSelector(InputValue inputValue)
        {
            if (!inputValue.isPressed) return;

            selectorManager?.OnSelectorSwitchInput();
        }

        // =============================
        // INTERACT WITH FARM TILE
        // =============================
        public void OnInteract(InputValue value)
        {
            Debug.Log("[Farming] OnInteract called");

            FarmTile tile = selectorManager.GetSelectedTile();
            Debug.Log($"[Farming] Selected tile: {(tile != null ? tile.name : "null")}");

            if (tile == null) return;

            tile.Interact();

            switch (tile.GetCondition)
            {
                case FarmTile.Condition.Tilled:
                    // Future: hoe animation
                    break;

                case FarmTile.Condition.Watered:
                    StartWatering();
                    break;
            }
        }

        // =============================
        // DIRECT WATER INPUT
        // =============================
        public void OnWater(InputValue value)
        {
            if (!value.isPressed) return;
            StartWatering();
        }

        void StartWatering()
        {
            if (!animatedController) return;

            animatedController.SetWatering(true);

            // Temporary timer (can remove if using Animator State detection)
            Invoke(nameof(StopWatering), 2.5f);
        }

        void StopWatering()
        {
            if (animatedController)
                animatedController.SetWatering(false);
        }

        // =============================
        // DEBUG TOOL
        // =============================
        [ContextMenu("Switch to Next Selector")]
        void SwitchSelector()
        {
            selectorManager?.OnSelectorSwitchInput();
        }
    }
}
