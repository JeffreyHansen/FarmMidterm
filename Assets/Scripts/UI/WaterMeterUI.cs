using UnityEngine;
using UnityEngine.UI;
using Character;

public class WaterMeterUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaterResource waterResource;
    [SerializeField] private Slider slider;

    void Start()
    {
        // Auto-find references if not set
        if (!slider)
            slider = GetComponent<Slider>();

        if (!waterResource)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
                waterResource = player.GetComponent<WaterResource>();
        }

        if (!waterResource)
            Debug.LogError("WaterMeterUI: No WaterResource found.");
    }

    void Update()
    {
        if (!waterResource || !slider) return;

        slider.value = waterResource.Normalized; // 0–1
    }
}
