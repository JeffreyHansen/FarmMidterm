using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Farming;

public class FarmingProgressUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Canvas progressCanvas;
    [SerializeField] private Transform progressBarParent;
    [SerializeField] private Texture2D borderTexture;
    [SerializeField] private Texture2D fillTexture;
    [SerializeField] private int maxProgressSquares = 10;
    
    [Header("Progress Tracking")]
    [SerializeField] private int pointsPerTill = 2;
    [SerializeField] private int pointsPerWater = 3;
    [SerializeField] private int pointsRequiredPerSquare = 15; // Points needed to fully fill one square
    
    [Header("UI Positioning")]
    [SerializeField] private float squareSize = 40f;
    [SerializeField] private float spacing = 5f;
    [SerializeField] private Vector2 topMargin = new Vector2(20f, 20f);
    
    private List<Image> borderImages = new List<Image>();
    private List<Image> fillImages = new List<Image>();
    private int currentProgress = 0;
    private int totalProgressRequired;

    void Start()
    {
        totalProgressRequired = maxProgressSquares * pointsRequiredPerSquare;
        SetupProgressCanvas();
        CreateProgressBar();
        UpdateProgressBar();
    }

    void OnEnable()
    {
        // Subscribe to farming events
        FarmingEvents.OnTileFarmed += HandleTileFarmed;
    }

    void OnDisable()
    {
        // Unsubscribe from farming events
        FarmingEvents.OnTileFarmed -= HandleTileFarmed;
    }

    void SetupProgressCanvas()
    {
        if (progressCanvas == null)
        {
            // Create canvas if not assigned
            GameObject canvasObj = new GameObject("Farming Progress Canvas");
            progressCanvas = canvasObj.AddComponent<Canvas>();
            progressCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            progressCanvas.sortingOrder = 100; // Above other UI
            
            // Add CanvasScaler for responsive UI
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            // Add GraphicRaycaster
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (progressBarParent == null)
        {
            // Create progress bar parent
            GameObject parentObj = new GameObject("Progress Bar");
            parentObj.transform.SetParent(progressCanvas.transform, false);
            
            RectTransform parentRect = parentObj.AddComponent<RectTransform>();
            parentRect.anchorMin = new Vector2(0.5f, 1f); // Top center
            parentRect.anchorMax = new Vector2(0.5f, 1f);
            parentRect.pivot = new Vector2(0.5f, 1f);
            
            float totalWidth = (maxProgressSquares * squareSize) + ((maxProgressSquares - 1) * spacing);
            parentRect.sizeDelta = new Vector2(totalWidth, squareSize);
            parentRect.anchoredPosition = new Vector2(0, -topMargin.y);
            
            progressBarParent = parentObj.transform;
        }
    }

    void CreateProgressBar()
    {
        // Clear existing UI elements
        foreach (Transform child in progressBarParent)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
        
        borderImages.Clear();
        fillImages.Clear();

        // Create sprites from textures if needed
        Sprite borderSprite = borderTexture != null ? Sprite.Create(borderTexture, new Rect(0, 0, borderTexture.width, borderTexture.height), Vector2.one * 0.5f) : null;
        Sprite fillSprite = fillTexture != null ? Sprite.Create(fillTexture, new Rect(0, 0, fillTexture.width, fillTexture.height), Vector2.one * 0.5f) : null;

        // Calculate starting position to center the squares
        float totalWidth = (maxProgressSquares * squareSize) + ((maxProgressSquares - 1) * spacing);
        float startOffset = -totalWidth * 0.5f + squareSize * 0.5f;

        for (int i = 0; i < maxProgressSquares; i++)
        {
            // Create border (background)
            GameObject borderObj = new GameObject($"Border_{i}");
            borderObj.transform.SetParent(progressBarParent, false);
            
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.sprite = borderSprite;
            borderImage.color = Color.white;
            borderImage.preserveAspect = true; // Maintain aspect ratio
            
            RectTransform borderRect = borderImage.rectTransform;
            borderRect.sizeDelta = new Vector2(squareSize, squareSize);
            
            // Center the squares properly
            float xPosition = startOffset + (i * (squareSize + spacing));
            borderRect.anchoredPosition = new Vector2(xPosition, 0);
            borderRect.anchorMin = new Vector2(0.5f, 0.5f);
            borderRect.anchorMax = new Vector2(0.5f, 0.5f);
            borderRect.pivot = new Vector2(0.5f, 0.5f);
            
            borderImages.Add(borderImage);

            // Create fill (foreground)
            GameObject fillObj = new GameObject($"Fill_{i}");
            fillObj.transform.SetParent(borderObj.transform, false);
            
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = fillSprite;
            fillImage.color = new Color(0.2f, 0.8f, 0.3f, 0f); // Start with 0 opacity
            fillImage.preserveAspect = true; // Maintain aspect ratio
            
            RectTransform fillRect = fillImage.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillRect.anchoredPosition = Vector2.zero;
            
            fillImages.Add(fillImage);
        }
    }

    void HandleTileFarmed(FarmTile tile, FarmTile.Condition previousCondition, FarmTile.Condition newCondition)
    {
        int points = 0;
        
        // Calculate points based on farming action
        if (previousCondition == FarmTile.Condition.Grass && newCondition == FarmTile.Condition.Tilled)
        {
            points = pointsPerTill;
        }
        else if (previousCondition == FarmTile.Condition.Tilled && newCondition == FarmTile.Condition.Watered)
        {
            points = pointsPerWater;
        }
        
        if (points > 0)
        {
            AddProgress(points);
        }
    }

    public void AddProgress(int points)
    {
        currentProgress = Mathf.Min(currentProgress + points, totalProgressRequired);
        UpdateProgressBar();
        
        // Check if progress bar is full
        if (currentProgress >= totalProgressRequired)
        {
            OnProgressBarFull();
        }
    }

    public void ResetProgress()
    {
        currentProgress = 0;
        squaresCompleted = 0;
        UpdateProgressBar();
    }

    void UpdateProgressBar()
    {
        // Check if any new squares completed (award money)
        int newSquaresCompleted = currentProgress / pointsRequiredPerSquare;
        if (newSquaresCompleted > squaresCompleted)
        {
            for (int s = squaresCompleted; s < newSquaresCompleted; s++)
            {
                OnSquareCompleted(s);
            }
            squaresCompleted = newSquaresCompleted;
        }
        
        for (int i = 0; i < fillImages.Count; i++)
        {
            // Calculate progress for this specific square
            int squareStartPoints = i * pointsRequiredPerSquare;
            
            float squareProgress = 0f;
            
            if (currentProgress > squareStartPoints)
            {
                int pointsInThisSquare = Mathf.Min(currentProgress - squareStartPoints, pointsRequiredPerSquare);
                squareProgress = (float)pointsInThisSquare / pointsRequiredPerSquare;
            }
            
            // Set opacity based on progress (0 to 1)
            Color fillColor = fillImages[i].color;
            fillColor.a = squareProgress;
            fillImages[i].color = fillColor;
            
            // Always show the fill image, but with varying opacity
            fillImages[i].gameObject.SetActive(true);
        }
    }

    private int squaresCompleted = 0; // Track completed squares for money earning
    private PlayerEconomy playerEconomy;

    void FindEconomy()
    {
        if (playerEconomy == null)
        {
            playerEconomy = FindFirstObjectByType<PlayerEconomy>();
        }
    }

    void OnProgressBarFull()
    {
        Debug.Log("Farming progress bar completed!");
        Invoke(nameof(ResetProgress), 1f);
    }

    /// <summary>
    /// Called every time a square reaches 100% opacity (every pointsRequiredPerSquare points).
    /// </summary>
    void OnSquareCompleted(int squareIndex)
    {
        FindEconomy();
        if (playerEconomy != null)
        {
            int moneyEarned = playerEconomy.MoneyPerSquare;
            playerEconomy.EarnMoney(moneyEarned);
            Debug.Log($"[ProgressUI] Square {squareIndex} completed! Earned ${moneyEarned}");
        }
    }

    // Public methods for external control
    public int GetCurrentProgress() => currentProgress;
    public int GetTotalProgressRequired() => totalProgressRequired;
    public float GetProgressPercentage() => (float)currentProgress / totalProgressRequired;
    
    public void SetMaxProgress(int newMax)
    {
        maxProgressSquares = newMax;
        totalProgressRequired = maxProgressSquares * pointsRequiredPerSquare;
        CreateProgressBar();
        UpdateProgressBar();
    }
}