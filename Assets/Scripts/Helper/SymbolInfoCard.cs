using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SymbolInfoCard : MonoBehaviour
{
    [Header("UI Component References")]
    [SerializeField] private Image cardBgImage;
    [SerializeField] private TMP_Text infoText;

    [Header("Pointer Sprites")]
    [Tooltip("Sprite used when card is on the RIGHT side of symbol (1st & 2nd reel - pointer points left)")]
    [SerializeField] private Sprite rightSideCardSprite;
    [Tooltip("Sprite used when card is on the LEFT side of symbol (3rd reel - pointer points right)")]
    [SerializeField] private Sprite leftSideCardSprite;

    [Header("Layout & Auto-Close Settings")]
    [Tooltip("Horizontal spacing from symbol center")]
    [SerializeField] private float xSpacing = 220f;
    [Tooltip("Vertical offset adjustment")]
    [SerializeField] private float yOffset = 0f;
    [Tooltip("Auto close duration in seconds")]
    [SerializeField] private float autoCloseDuration = 2.0f;

    private RectTransform rectTransform;
    private int activeCol = -1;
    private int activeRow = -1;
    private int activeSymbolId = -1;
    private GameManager cachedGameManager;
    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private Sprite GetRightSideSprite() => rightSideCardSprite;
    private Sprite GetLeftSideSprite() => leftSideCardSprite;

    public void ShowCard(int symbolId, int colIndex, int rowIndex, RectTransform symbolRect, GameManager gameManager, float customYOffset = 0f)
    {
        if (gameObject.activeSelf && activeCol == colIndex && activeRow == rowIndex)
        {
            HideCard();
            return;
        }

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        activeCol = colIndex;
        activeRow = rowIndex;
        activeSymbolId = symbolId;
        cachedGameManager = gameManager;

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        Vector3 symbolWorldPos = symbolRect != null ? symbolRect.position : transform.position;
        Vector3 localPos = transform.parent != null ? transform.parent.InverseTransformPoint(symbolWorldPos) : symbolWorldPos;

        float offsetDir = (colIndex < 2) ? Mathf.Abs(xSpacing) : -Mathf.Abs(xSpacing);
        rectTransform.localPosition = new Vector3(localPos.x + offsetDir, localPos.y + yOffset + customYOffset, localPos.z);

        if (cardBgImage != null)
        {
            Sprite targetSprite = (colIndex < 2) ? GetRightSideSprite() : GetLeftSideSprite();
            if (targetSprite != null)
            {
                cardBgImage.sprite = targetSprite;
            }
        }

        SetupCardContent(symbolId, gameManager);

        gameObject.SetActive(true);

        autoCloseCoroutine = StartCoroutine(AutoCloseTimer(autoCloseDuration));
    }

    private IEnumerator AutoCloseTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        HideCard();
    }

    public void RefreshCard(GameManager gameManager = null)
    {
        if (!gameObject.activeSelf || activeSymbolId < 0) return;
        if (gameManager != null) cachedGameManager = gameManager;
        SetupCardContent(activeSymbolId, cachedGameManager);

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }
        autoCloseCoroutine = StartCoroutine(AutoCloseTimer(autoCloseDuration));
    }

    private void SetupCardContent(int symbolId, GameManager gameManager)
    {
        if (infoText == null) return;

        double betFactor = 1.0;
        if (gameManager != null)
        {
            if (gameManager.currentBetAmount > 0)
            {
                betFactor = gameManager.currentBetAmount;
            }
            else if (gameManager.gameConfig != null && gameManager.gameConfig.availableBets != null &&
                     gameManager.gameConfig.availableBets.Count > gameManager.currentBetIndex &&
                     gameManager.currentBetIndex >= 0)
            {
                betFactor = gameManager.gameConfig.availableBets[gameManager.currentBetIndex];
            }
        }

        // HotChili Wild Multipliers (0: 2X Green, 1: 3X Yellow, 2: 4X Orange, 3: 5X Red)
        if (symbolId >= 0 && symbolId <= 3)
        {
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.enableWordWrapping = true;
            int mult = symbolId == 0 ? 2 : (symbolId == 1 ? 3 : (symbolId == 2 ? 4 : 5));
            infoText.text = $"<color=#FFC700>{mult}X WILD</color>\nSubstitutes all symbols\nMultiplies line win by {mult}X";
            return;
        }

        // Standard Symbols (7s and Bars)
        infoText.alignment = TextAlignmentOptions.Flush;
        infoText.enableWordWrapping = false;

        SymbolInfo symbolInfo = null;
        if (gameManager != null && gameManager.gameConfig != null && gameManager.gameConfig.symbols != null)
        {
            symbolInfo = gameManager.gameConfig.symbols.Find(s => s.id == symbolId);
        }

        if (symbolInfo != null && symbolInfo.multipliers != null && symbolInfo.multipliers.Count > 0)
        {
            List<string> lines = new List<string>();
            for (int m = 0; m < symbolInfo.multipliers.Count; m++)
            {
                double payout = symbolInfo.multipliers[m] * betFactor;
                lines.Add($"<color=#FFC700>X3</color>   {payout.ToString("0.###")}");
            }
            infoText.text = string.Join("\n", lines);
        }
        else
        {
            // Built-in fallback multipliers for HotChili
            double defaultMultiplier = GetDefaultMultiplier(symbolId);
            if (defaultMultiplier > 0)
            {
                double payout = defaultMultiplier * betFactor;
                infoText.text = $"<color=#FFC700>X3</color>   {payout.ToString("0.###")}";
            }
            else
            {
                infoText.text = "";
            }
        }
    }

    private double GetDefaultMultiplier(int symbolId)
    {
        switch (symbolId)
        {
            case 7: return 25.0; // Red 7
            case 8: return 25.0; // Blue 7
            case 4: return 10.0; // Triple Bar
            case 5: return 5.0;  // Double Bar
            case 6: return 3.0;  // Single Bar
            default: return 0.0;
        }
    }

    public void HideCard()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        activeCol = -1;
        activeRow = -1;
        activeSymbolId = -1;

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }
}
