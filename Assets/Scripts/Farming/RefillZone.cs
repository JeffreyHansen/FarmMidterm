using UnityEngine;
using Character;

public class RefillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        WaterResource water = other.GetComponent<WaterResource>();
        if (water)
        {
            water.StartRefill();
            Debug.Log("Started refilling water");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WaterResource water = other.GetComponent<WaterResource>();
        if (water)
        {
            water.StopRefill();
            Debug.Log("Stopped refilling water");
        }
    }
}
