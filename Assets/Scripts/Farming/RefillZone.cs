using UnityEngine;
using Character;

public class RefillZone : MonoBehaviour
{
    [SerializeField] private int refillAmount = 5;
    [SerializeField] private bool fillToMax = true;

    private void OnTriggerEnter(Collider other)
    {
        WaterResource water = other.GetComponent<WaterResource>();
        if (water)
        {
            if (fillToMax)
                water.FillInstant();
            else
                water.AddWater(refillAmount);
            Debug.Log("Water refilled!");
        }
    }
}
