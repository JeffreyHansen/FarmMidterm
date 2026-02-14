using UnityEngine;
using UnityEngine.InputSystem;
using Farming;

namespace Character
{
    public class PlayerFarmingController : MonoBehaviour
    {
        [SerializeField] private SelectorManager selectorManager;

        AnimatedController animatedController;
        WaterResource waterResource;

        bool isWateringActive; // prevents multi-drain per animation
        float wateringStartTime;
        const float WATERING_ANIMATION_DURATION = 5.6f;
        const float MAX_WATERING_DURATION = 8f; // Safety timeout (extra buffer beyond animation)

        void Start()
        {
            animatedController = GetComponent<AnimatedController>();
            waterResource = GetComponent<WaterResource>();

            if (selectorManager == null)
                selectorManager = GetComponent<SelectorManager>();

            Debug.Assert(selectorManager, "PlayerFarmingController requires a SelectorManager.");

            if (!animatedController)
                Debug.LogWarning("No AnimatedController found.");

            if (!waterResource)
                Debug.LogError("No WaterResource found on Player.");
        }

        void Update()
        {
            // Safety: force-reset watering state if stuck for too long
            if (isWateringActive && Time.time - wateringStartTime > MAX_WATERING_DURATION)
            {
                Debug.LogWarning("[PlayerFarmingController] Watering stuck for too long - forcing reset!");
                StopWatering();
            }
        }

        // =============================
        // SWITCH SELECTOR
        // =============================
        public void OnSwitchSelector(InputValue inputValue)
        {
            if (!inputValue.isPressed) return;
            selectorManager?.OnSelectorSwitchInput();
        }

        // =============================
        // TILE INTERACT
        // =============================
        public void OnInteract(InputValue value)
        {
            // IMPORTANT: E key is bound to Interact but we don't want it to trigger watering
            // Only allow Space (primary interact key) to actually interact
            // User should remove E binding from Input Actions, or we filter it here:
            if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
            {
                return; // Ignore E key presses
            }
            
            if (selectorManager == null)
            {
                Debug.LogError("[PlayerFarmingController] SelectorManager is null!");
                return;
            }
            
            FarmTile tile = selectorManager.GetSelectedTile();
            if (tile == null)
            {
                return;
            }

            // Use InteractWithWater to gate watering behind water resource
            bool success = tile.InteractWithWater(waterResource);

            if (success && tile.GetCondition == FarmTile.Condition.Watered)
            {
                TryWater();
            }
        }

        // =============================
        // DIRECT WATER BUTTON (E)
        // =============================
        public void OnWater(InputValue value)
        {
            if (!value.isPressed) return;
            TryWater();
        }

        // =============================
        // CORE WATER LOGIC (animation only - water consumption handled by FarmTile.InteractWithWater)
        // =============================
        void TryWater()
        {
            if (!animatedController)
                return;

            // Cancel any pending StopWatering calls (safety)
            CancelInvoke(nameof(StopWatering));

            // If already watering, restart the animation smoothly
            if (isWateringActive)
            {
                // Restart animation from beginning without transitioning out
                animatedController.RestartWateringAnimation();
            }
            else
            {
                // Start fresh animation
                animatedController.SetWatering(true);
            }

            // Lock until animation ends
            isWateringActive = true;
            wateringStartTime = Time.time;

            // Schedule stop after animation completes
            Invoke(nameof(StopWatering), WATERING_ANIMATION_DURATION);
        }

        void StopWatering()
        {
            Debug.Log("[PlayerFarmingController] StopWatering called - resetting animation state");
            isWateringActive = false;

            if (animatedController)
            {
                animatedController.SetWatering(false);
                Debug.Log("[PlayerFarmingController] Called SetWatering(false)");
            }
        }

        void OnDisable()
        {
            // Safety: cancel any pending invokes when disabled
            CancelInvoke(nameof(StopWatering));
            
            // Reset state
            if (isWateringActive && animatedController)
            {
                animatedController.SetWatering(false);
                isWateringActive = false;
            }
        }
    }
}
