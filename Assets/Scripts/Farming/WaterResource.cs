using UnityEngine;

namespace Character
{
    public class WaterResource : MonoBehaviour
    {
        [Header("Water Settings")]
        [Range(0f, 100f)]
        [SerializeField] private float waterPercent = 100f;

        [SerializeField] private float drainPerUse = 10f;
        [SerializeField] private float refillRatePerSecond = 60f;

        public float WaterPercent => waterPercent;
        public float Normalized => waterPercent / 100f;
        public bool HasWater => waterPercent > 0f;

        bool isRefilling;

        void Update()
        {
            if (isRefilling)
            {
                waterPercent += refillRatePerSecond * Time.deltaTime;
                waterPercent = Mathf.Clamp(waterPercent, 0f, 100f);
            }
        }

        // Called when player waters
        public bool TryConsumeWater()
        {
            if (waterPercent <= 0f)
                return false;

            waterPercent -= drainPerUse;
            waterPercent = Mathf.Clamp(waterPercent, 0f, 100f);

            Debug.Log($"Water remaining: {waterPercent}%");
            return true;
        }

        public void StartRefill() => isRefilling = true;
        public void StopRefill() => isRefilling = false;

        public void FillInstant()
        {
            waterPercent = 100f;
        }
    }
}
