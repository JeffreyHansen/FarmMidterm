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
            FarmTile tile = selectorManager.GetSelectedTile();
            if (tile == null) return;

            tile.Interact();

            if (tile.GetCondition == FarmTile.Condition.Watered)
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
        // CORE WATER LOGIC
        // =============================
        void TryWater()
        {
            if (!animatedController || waterResource == null)
                return;

            // 🚫 Already watering → block extra drains
            if (isWateringActive)
                return;

            // 🚫 No water left
            if (!waterResource.TryConsumeWater())
            {
                Debug.Log("Out of water! Return to the shack to refill.");
                return;
            }

            // ✅ Lock draining until animation ends
            isWateringActive = true;

            // Play watering animation
            animatedController.SetWatering(true);

            // NOTE:
            // If using Animator State detection, you can remove this Invoke later
            Invoke(nameof(StopWatering), 2.5f);
        }

        void StopWatering()
        {
            isWateringActive = false;

            if (animatedController)
                animatedController.SetWatering(false);
        }
    }
}
