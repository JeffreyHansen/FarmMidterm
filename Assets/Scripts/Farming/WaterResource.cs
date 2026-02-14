using UnityEngine;
using System;

namespace Character
{
    public class WaterResource : MonoBehaviour
    {
        [Header("Water Settings")]
        [SerializeField] private int maxWater = 10;
        [SerializeField] private int currentWater = 10;

        /// <summary>Fires whenever water count changes (currentWater, maxWater)</summary>
        public static event Action<int, int> OnWaterChanged;

        public int CurrentWater => currentWater;
        public int MaxWater => maxWater;
        public float Normalized => (float)currentWater / maxWater;
        public bool HasWater => currentWater > 0;

        void Start()
        {
            OnWaterChanged?.Invoke(currentWater, maxWater);
        }

        /// <summary>
        /// Try to consume 1 unit of water. Returns true if successful.
        /// </summary>
        public bool TryConsumeWater()
        {
            if (currentWater <= 0)
                return false;

            currentWater--;
            Debug.Log($"[Water] Used 1 water. Remaining: {currentWater}/{maxWater}");
            OnWaterChanged?.Invoke(currentWater, maxWater);
            return true;
        }

        /// <summary>
        /// Add water units (from shop purchase or refill zone).
        /// </summary>
        public void AddWater(int amount)
        {
            currentWater = Mathf.Min(currentWater + amount, maxWater);
            Debug.Log($"[Water] Refilled {amount}. Now: {currentWater}/{maxWater}");
            OnWaterChanged?.Invoke(currentWater, maxWater);
        }

        /// <summary>
        /// Fill water to maximum instantly.
        /// </summary>
        public void FillInstant()
        {
            currentWater = maxWater;
            OnWaterChanged?.Invoke(currentWater, maxWater);
        }

        /// <summary>
        /// Increase max water capacity (upgrade).
        /// </summary>
        public void UpgradeCapacity(int newMax)
        {
            maxWater = newMax;
            currentWater = Mathf.Min(currentWater, maxWater);
            OnWaterChanged?.Invoke(currentWater, maxWater);
        }
    }
}
