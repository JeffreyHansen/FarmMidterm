using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Character;
using Farming;

/// <summary>
/// Hotbar UI that shows the watering can icon with remaining water count above it,
/// and the player's current money.
/// </summary>
public class HotbarUI : MonoBehaviour
{
    [Header("References (auto-created if null)")]
    [SerializeField] private Canvas hotbarCanvas;
    [SerializeField] private Texture2D wateringCanTexture; // Art/UI/Adament_Watering_can.webp
    
    [Header("Sizing")]
    [SerializeField] private float iconSize = 64f;
    [SerializeField] private float bottomMargin = 20f;
    [SerializeField] private float fontSize = 24f;

    private Image wateringCanImage;
    private TextMeshProUGUI waterCountText;
    private TextMeshProUGUI moneyText;

    void Start()
    {
        SetupCanvas();
        CreateHotbar();
    }

    void OnEnable()
    {
        WaterResource.OnWaterChanged += UpdateWaterDisplay;
        PlayerEconomy.OnMoneyChanged += UpdateMoneyDisplay;
    }

    void OnDisable()
    {
        WaterResource.OnWaterChanged -= UpdateWaterDisplay;
        PlayerEconomy.OnMoneyChanged -= UpdateMoneyDisplay;
    }

    void SetupCanvas()
    {
        if (hotbarCanvas != null) return;

        GameObject canvasObj = new GameObject("Hotbar Canvas");
        hotbarCanvas = canvasObj.AddComponent<Canvas>();
        hotbarCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hotbarCanvas.sortingOrder = 90;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();
    }

    void CreateHotbar()
    {
        // =============================
        // HOTBAR CONTAINER (bottom center)
        // =============================
        GameObject hotbarObj = new GameObject("Hotbar");
        hotbarObj.transform.SetParent(hotbarCanvas.transform, false);

        RectTransform hotbarRect = hotbarObj.AddComponent<RectTransform>();
        hotbarRect.anchorMin = new Vector2(0.5f, 0f); // Bottom center
        hotbarRect.anchorMax = new Vector2(0.5f, 0f);
        hotbarRect.pivot = new Vector2(0.5f, 0f);
        hotbarRect.anchoredPosition = new Vector2(0, bottomMargin);
        hotbarRect.sizeDelta = new Vector2(iconSize + 40f, iconSize + 50f);

        // Semi-transparent background
        Image hotbarBg = hotbarObj.AddComponent<Image>();
        hotbarBg.color = new Color(0f, 0f, 0f, 0.4f);

        // =============================
        // WATERING CAN ICON
        // =============================
        GameObject iconObj = new GameObject("Watering Can Icon");
        iconObj.transform.SetParent(hotbarObj.transform, false);

        wateringCanImage = iconObj.AddComponent<Image>();
        if (wateringCanTexture != null)
        {
            Sprite canSprite = Sprite.Create(
                wateringCanTexture,
                new Rect(0, 0, wateringCanTexture.width, wateringCanTexture.height),
                Vector2.one * 0.5f
            );
            wateringCanImage.sprite = canSprite;
        }
        else
        {
            wateringCanImage.color = new Color(0.3f, 0.6f, 1f, 1f); // Blue placeholder
        }
        wateringCanImage.preserveAspect = true;

        RectTransform iconRect = wateringCanImage.rectTransform;
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.sizeDelta = new Vector2(iconSize, iconSize);
        iconRect.anchoredPosition = new Vector2(0, -5f); // Slightly below center to make room for text

        // =============================
        // WATER COUNT TEXT (above the watering can)
        // =============================
        GameObject waterTextObj = new GameObject("Water Count");
        waterTextObj.transform.SetParent(hotbarObj.transform, false);

        waterCountText = waterTextObj.AddComponent<TextMeshProUGUI>();
        waterCountText.text = "10";
        waterCountText.fontSize = fontSize;
        waterCountText.alignment = TextAlignmentOptions.Center;
        waterCountText.color = new Color(0.4f, 0.8f, 1f); // Light blue

        RectTransform waterTextRect = waterCountText.rectTransform;
        waterTextRect.anchorMin = new Vector2(0.5f, 1f); // Top of hotbar
        waterTextRect.anchorMax = new Vector2(0.5f, 1f);
        waterTextRect.pivot = new Vector2(0.5f, 1f);
        waterTextRect.sizeDelta = new Vector2(100f, 30f);
        waterTextRect.anchoredPosition = new Vector2(0, -2f);

        // =============================
        // MONEY DISPLAY (top right)
        // =============================
        GameObject moneyObj = new GameObject("Money Display");
        moneyObj.transform.SetParent(hotbarCanvas.transform, false);

        // Background
        Image moneyBg = moneyObj.AddComponent<Image>();
        moneyBg.color = new Color(0f, 0f, 0f, 0.4f);

        RectTransform moneyRect = moneyBg.rectTransform;
        moneyRect.anchorMin = new Vector2(1f, 1f); // Top right
        moneyRect.anchorMax = new Vector2(1f, 1f);
        moneyRect.pivot = new Vector2(1f, 1f);
        moneyRect.sizeDelta = new Vector2(160f, 40f);
        moneyRect.anchoredPosition = new Vector2(-20f, -20f);

        // Money text
        GameObject moneyTextObj = new GameObject("Money Text");
        moneyTextObj.transform.SetParent(moneyObj.transform, false);

        moneyText = moneyTextObj.AddComponent<TextMeshProUGUI>();
        moneyText.text = "$0";
        moneyText.fontSize = fontSize;
        moneyText.alignment = TextAlignmentOptions.Center;
        moneyText.color = new Color(1f, 0.9f, 0.3f); // Gold color

        RectTransform moneyTextRect = moneyText.rectTransform;
        moneyTextRect.anchorMin = Vector2.zero;
        moneyTextRect.anchorMax = Vector2.one;
        moneyTextRect.sizeDelta = Vector2.zero;
        moneyTextRect.anchoredPosition = Vector2.zero;
    }

    void UpdateWaterDisplay(int current, int max)
    {
        if (waterCountText != null)
        {
            waterCountText.text = current.ToString();

            // Color changes based on water level
            float ratio = (float)current / max;
            if (ratio <= 0f)
                waterCountText.color = Color.red;
            else if (ratio <= 0.3f)
                waterCountText.color = new Color(1f, 0.5f, 0.2f); // Orange warning
            else
                waterCountText.color = new Color(0.4f, 0.8f, 1f); // Light blue
        }
    }

    void UpdateMoneyDisplay(int amount)
    {
        if (moneyText != null)
        {
            moneyText.text = $"${amount}";
        }
    }
}
