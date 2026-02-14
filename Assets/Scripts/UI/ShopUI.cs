using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Character;
using Farming;

/// <summary>
/// Shop UI that appears when the player approaches a shop zone.
/// Allows buying water with earned money.
/// </summary>
public class ShopUI : MonoBehaviour
{
    [Header("Shop Settings")]
    [SerializeField] private int waterRefillCost = 5;    // Cost per water unit

    
    [Header("UI Sizing")]
    [SerializeField] private float panelWidth = 300f;
    [SerializeField] private float panelHeight = 200f;
    
    private Canvas shopCanvas;
    private GameObject shopPanel;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI infoText;
    private Button buyOneButton;
    private Button buyFullButton;
    private TextMeshProUGUI buyOneText;
    private TextMeshProUGUI buyFullText;

    private WaterResource playerWater;
    private PlayerEconomy playerEconomy;
    private bool isShopOpen;

    void Start()
    {
        CreateShopUI();
        shopPanel.SetActive(false); // Hidden by default
    }

    void CreateShopUI()
    {
        // =============================
        // CANVAS
        // =============================
        GameObject canvasObj = new GameObject("Shop Canvas");
        shopCanvas = canvasObj.AddComponent<Canvas>();
        shopCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        shopCanvas.sortingOrder = 200; // Above everything else

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // =============================
        // SHOP PANEL (center of screen)
        // =============================
        shopPanel = new GameObject("Shop Panel");
        shopPanel.transform.SetParent(shopCanvas.transform, false);

        Image panelBg = shopPanel.AddComponent<Image>();
        panelBg.color = new Color(0.15f, 0.1f, 0.05f, 0.9f); // Dark brown

        RectTransform panelRect = panelBg.rectTransform;
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(panelWidth, panelHeight);

        // =============================
        // TITLE
        // =============================
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(shopPanel.transform, false);

        titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "Water Shop";
        titleText.fontSize = 28f;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.9f, 0.3f); // Gold
        titleText.fontStyle = FontStyles.Bold;

        RectTransform titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 0.75f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.sizeDelta = Vector2.zero;
        titleRect.offsetMin = new Vector2(10f, titleRect.offsetMin.y);
        titleRect.offsetMax = new Vector2(-10f, titleRect.offsetMax.y);

        // =============================
        // INFO TEXT (water status + money)
        // =============================
        GameObject infoObj = new GameObject("Info");
        infoObj.transform.SetParent(shopPanel.transform, false);

        infoText = infoObj.AddComponent<TextMeshProUGUI>();
        infoText.text = "Water: 0/10 | Money: $0";
        infoText.fontSize = 18f;
        infoText.alignment = TextAlignmentOptions.Center;
        infoText.color = Color.white;

        RectTransform infoRect = infoText.rectTransform;
        infoRect.anchorMin = new Vector2(0f, 0.55f);
        infoRect.anchorMax = new Vector2(1f, 0.75f);
        infoRect.sizeDelta = Vector2.zero;
        infoRect.offsetMin = new Vector2(10f, infoRect.offsetMin.y);
        infoRect.offsetMax = new Vector2(-10f, infoRect.offsetMax.y);

        // =============================
        // BUY 1 WATER BUTTON
        // =============================
        buyOneButton = CreateButton(shopPanel.transform, "Buy 1 Water", 0.15f, 0.5f);
        buyOneText = buyOneButton.GetComponentInChildren<TextMeshProUGUI>();
        buyOneButton.onClick.AddListener(OnBuyOneWater);

        // =============================
        // BUY FULL REFILL BUTTON
        // =============================
        buyFullButton = CreateButton(shopPanel.transform, "Full Refill", 0.0f, 0.15f);
        buyFullText = buyFullButton.GetComponentInChildren<TextMeshProUGUI>();
        buyFullButton.onClick.AddListener(OnBuyFullRefill);

        UpdateShopInfo();
    }

    Button CreateButton(Transform parent, string label, float yMin, float yMax)
    {
        GameObject btnObj = new GameObject($"Button_{label}");
        btnObj.transform.SetParent(parent, false);

        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0.3f, 0.5f, 0.2f, 1f); // Green

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.4f, 0.7f, 0.3f);
        colors.pressedColor = new Color(0.2f, 0.4f, 0.15f);
        btn.colors = colors;

        RectTransform btnRect = btnBg.rectTransform;
        btnRect.anchorMin = new Vector2(0.1f, yMin);
        btnRect.anchorMax = new Vector2(0.9f, yMax);
        btnRect.sizeDelta = Vector2.zero;
        btnRect.offsetMin = new Vector2(10f, 5f);
        btnRect.offsetMax = new Vector2(-10f, -5f);

        // Button label
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(btnObj.transform, false);

        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 18f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;

        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return btn;
    }

    void UpdateShopInfo()
    {
        int water = playerWater != null ? playerWater.CurrentWater : 0;
        int maxWater = playerWater != null ? playerWater.MaxWater : 10;
        int money = playerEconomy != null ? playerEconomy.CurrentMoney : 0;

        if (infoText != null)
            infoText.text = $"Water: {water}/{maxWater} | Money: ${money}";

        if (buyOneText != null)
            buyOneText.text = $"Buy 1 Water (${waterRefillCost})";

        if (buyFullText != null)
        {
            int waterNeeded = maxWater - water;
            int cost = waterNeeded * waterRefillCost;
            buyFullText.text = $"Full Refill (${cost})";
        }

        // Disable buttons if can't afford
        if (buyOneButton != null)
            buyOneButton.interactable = money >= waterRefillCost && water < maxWater;

        if (buyFullButton != null)
        {
            int waterNeeded = maxWater - water;
            buyFullButton.interactable = money >= waterNeeded * waterRefillCost && waterNeeded > 0;
        }
    }

    void OnBuyOneWater()
    {
        if (playerEconomy == null || playerWater == null) return;

        if (playerWater.CurrentWater >= playerWater.MaxWater)
        {
            Debug.Log("[Shop] Water is already full!");
            return;
        }

        if (playerEconomy.TrySpend(waterRefillCost))
        {
            playerWater.AddWater(1);
            Debug.Log("[Shop] Bought 1 water!");
            UpdateShopInfo();
        }
    }

    void OnBuyFullRefill()
    {
        if (playerEconomy == null || playerWater == null) return;

        int waterNeeded = playerWater.MaxWater - playerWater.CurrentWater;
        if (waterNeeded <= 0)
        {
            Debug.Log("[Shop] Water is already full!");
            return;
        }

        int totalCost = waterNeeded * waterRefillCost;
        if (playerEconomy.TrySpend(totalCost))
        {
            playerWater.FillInstant();
            Debug.Log($"[Shop] Full refill! Bought {waterNeeded} water for ${totalCost}");
            UpdateShopInfo();
        }
    }

    public void OpenShop(WaterResource water, PlayerEconomy economy)
    {
        playerWater = water;
        playerEconomy = economy;
        isShopOpen = true;
        shopPanel.SetActive(true);
        UpdateShopInfo();
        Debug.Log("[Shop] Shop opened!");
    }

    public void CloseShop()
    {
        isShopOpen = false;
        shopPanel.SetActive(false);
        Debug.Log("[Shop] Shop closed.");
    }

    public bool IsOpen => isShopOpen;

    void Update()
    {
        // Keep info updated while shop is open
        if (isShopOpen)
        {
            UpdateShopInfo();
        }
    }
}
