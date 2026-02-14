using UnityEngine;
using Character;
using Farming;

/// <summary>
/// Place this on a trigger collider in the scene (e.g. a shop building).
/// When the player enters the trigger, the shop UI opens.
/// When the player leaves, it closes.
/// </summary>
public class ShopZone : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;

    void Start()
    {
        // Auto-find ShopUI if not assigned
        if (shopUI == null)
        {
            shopUI = FindFirstObjectByType<ShopUI>();
        }

        if (shopUI == null)
        {
            Debug.LogError("[ShopZone] No ShopUI found in scene! Add a ShopUI component to a GameObject.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Search from the root of the player hierarchy for WaterResource
        Transform root = other.transform.root;
        WaterResource water = root.GetComponentInChildren<WaterResource>();
        
        // PlayerEconomy may be on a separate GameObject, so search the entire scene
        PlayerEconomy economy = FindFirstObjectByType<PlayerEconomy>();

        if (water == null)
            Debug.LogWarning($"[ShopZone] No WaterResource found in {root.name} hierarchy.");
        if (economy == null)
            Debug.LogWarning($"[ShopZone] No PlayerEconomy found in scene.");

        if (water != null)
        {
            shopUI?.OpenShop(water, economy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Search from the root of the player hierarchy
        Transform root = other.transform.root;
        WaterResource water = root.GetComponentInChildren<WaterResource>();
                           
        if (water != null)
        {
            shopUI?.CloseShop();
        }
    }
}
