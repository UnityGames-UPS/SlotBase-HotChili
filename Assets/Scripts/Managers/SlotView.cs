using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;

public partial class SlotView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;

    [Header("Symbol Sprites - Matches Backend JSON")]
    [SerializeField] private Sprite spriteGreenChilli; // ID 0
    [SerializeField] private Sprite spriteYellowChilli; // ID 1
    [SerializeField] private Sprite spriteOrangeChilli; // ID 2
    [SerializeField] private Sprite spriteRedChilli; // ID 3
    [SerializeField] private Sprite spriteTripleBar; // ID 4
    [SerializeField] private Sprite spriteDoubleBar; // ID 5
    [SerializeField] private Sprite spriteSingleBar; // ID 6
    [SerializeField] private Sprite spriteRedSeven; // ID 7
    [SerializeField] private Sprite spriteBlueSeven; // ID 8
    [SerializeField] private Sprite spriteReSpin;

   

    [Header("Spine Win Animations - Matches Backend JSON")]
    [SerializeField] private SkeletonDataAsset spineGreenChilli;
    [SerializeField] private SkeletonDataAsset spineYellowChilli;
    [SerializeField] private SkeletonDataAsset spineOrangeChilli;
    [SerializeField] private SkeletonDataAsset spineRedChilli;
    [SerializeField] private SkeletonDataAsset spineTripleBar;
    [SerializeField] private SkeletonDataAsset spineDoubleBar;
    [SerializeField] private SkeletonDataAsset spineSingleBar;
    [SerializeField] private SkeletonDataAsset spineRedSeven;
    [SerializeField] private SkeletonDataAsset spineBlueSeven;
    [SerializeField] private SkeletonDataAsset spineReSpin;

    [Header("Spine Skin Names (Optional)")]
    [Tooltip("If you combined multiple symbols into one Spine file, type the Skin Name here for each symbol ID (0 to 9).")]
    [SerializeField] internal string[] spineSkinNames = new string[10];

    [Tooltip("Global scale modifier to shrink or grow Spine Win animations.")]
    [SerializeField] internal float spineScaleMultiplier = 0.8f;
    
    private SkeletonDataAsset[] spineDataArray;

    private Sprite[] symbolSprites;


    [Header("Reel Containers")]
    [SerializeField] private Transform[] reelTransforms;

    [Header("Reel Images - 14 images per reel")]
    [SerializeField] private List<ReelImages> reelImagesList;

    [Header("Reel Stop Y Positions")]
    [SerializeField] private float case1StopY = 202f;
    [SerializeField] private float case2StopY = 0f;
    
    [Header("Spin Settings")]
    [SerializeField] private float symbolHeight = 500f;
    [SerializeField] private float spinSpeed = 2000f;
    [SerializeField] private float reelStartStagger = 0.08f;
    [SerializeField] private float reelStopStagger = 0.25f;

    [Header("Stop Animation Settings")]
    #pragma warning disable 0414
    [SerializeField] private float stopOvershootDistance = 50f;
    [SerializeField] private float quickStopOvershoot = 20f;
#pragma warning restore 0414
    [SerializeField] private float stopOvershootDuration = 0.20f;
    [SerializeField] private float stopSettleDuration = 0.30f;

    [Header("Quick Spin Settings")]
    [SerializeField] private float quickStopStagger = 0.06f;
        [SerializeField] private float quickStopDuration = 0.2f;
    [SerializeField] private int minSpinCyclesBeforeStop = 3;


    [Header("Win Animation Settings")]
    [SerializeField] private float winSymbolLoopDuration = 1.2f;


    [Header("Win Animation Objects — Col 0..4  (each has 2 rows, contains ImageAnimation component)")]
    [SerializeField] private GameObject winAnimationParent;
    [SerializeField] private GameObject winBorderAnimationParent;
    [SerializeField] private GameObject winLineAnimation;
    [SerializeField] internal GameObject reSpinAnimationBorder;
    [SerializeField] internal GameObject respinReelsGlowObject;
    [Tooltip("GameObject references for win animations. Each should have an ImageAnimation component attached.")]
    [SerializeField] private ColumnOverlays[] winAnimationColumns = new ColumnOverlays[5];

    [Header("Tension / Anticipation Settings")]
    [Tooltip("Extra spin duration in seconds for the last slot during tension spin.")]
    [SerializeField] private float tensionSpinExtraDuration = 2.0f;


    [Header("Symbol Info Card")]
    [SerializeField] private SymbolInfoCard symbolInfoCard;

    [Header("3D Cylindrical Drum Settings")]
    [SerializeField] private bool enableCylindricalEffect = true;
    [SerializeField] private float visibleHalfHeight = 310f;
    [SerializeField] private float outerHalfHeight = 450f;
    [SerializeField] private float edgeScale = 0.84f;
    [SerializeField] private float outerScale = 0.68f;
    [Range(0f, 60f)]
    [SerializeField] private float maxTiltAngle = 26f;
    [Range(0.2f, 1f)]
    [SerializeField] private float minSquashY = 0.78f;
    [Header("Horizontal Edge Bow Settings (Fish-Eye)")]
    [SerializeField] private float leftReelEdgeX = -16f;
    [SerializeField] private float rightReelEdgeX = 16f;
    [SerializeField] private float leftReelOuterX = -32f;
    [SerializeField] private float rightReelOuterX = 32f;
    [Tooltip("Optional parent RectTransform reference (e.g. reel viewport frame) to automatically measure visible half height from parent rect height.")]
    [SerializeField] private RectTransform visibleAreaRectTransform;

    private float[] reelCurveIntensity = new float[3] { 1f, 1f, 1f };
    private Tween[] reelSettleCurveTweens = new Tween[3];


    private float middlePosition = 0f;
    private float cycleDistance;


    private List<Tween> spinTweens = new List<Tween>();
    private List<Tween> winTweens = new List<Tween>();
    private List<int> reelCycleCount = new List<int>();
    private Coroutine winAnimationCoroutine;


    internal List<List<int>> currentDisplayMatrix { get; private set; }

    internal bool isSpinning { get; private set; }

    #region Initialization

    private Dictionary<GameObject, Vector3> originalWinBoxLocalPositions;

    private void CacheOriginalWinBoxPositions()
    {
        if (winAnimationColumns == null) return;
        if (originalWinBoxLocalPositions == null)
            originalWinBoxLocalPositions = new Dictionary<GameObject, Vector3>();
        else
            originalWinBoxLocalPositions.Clear();

        foreach (var colOverlay in winAnimationColumns)
        {
            if (colOverlay != null && colOverlay.rows != null)
            {
                foreach (var go in colOverlay.rows)
                {
                    if (go != null && !originalWinBoxLocalPositions.ContainsKey(go))
                    {
                        originalWinBoxLocalPositions[go] = go.transform.localPosition;
                    }
                }
            }
        }
    }


    private void ResetWinBoxPosition(GameObject go)
    {
        if (go != null)
        {
            if (originalWinBoxLocalPositions != null && originalWinBoxLocalPositions.TryGetValue(go, out Vector3 origPos))
            {
                go.transform.localPosition = origPos;
            }
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one * spineScaleMultiplier;
        }
    }

    private void Awake()
    {
        BuildSymbolSpriteArray();
        BuildSpineArray();
        InitializeReels();
    }

    private void Start()
    {
        UpdateCylindricalSpinEffect(force: true);

        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            BuildSymbolSpriteArray();
            BuildSpineArray();
        }

        CacheOriginalWinBoxPositions();
        DisableAllOverlays();
        SetupSymbolButtons();
    }

    private void DisableAllOverlays()
    {
        DisableColumns(winAnimationColumns);
        if (lockedColumns.Count == 0)
        {
            if (winAnimationParent) winAnimationParent.SetActive(false);
            if (reSpinAnimationBorder) reSpinAnimationBorder.SetActive(false);
            if (respinReelsGlowObject) respinReelsGlowObject.SetActive(false);
        }
        if (winBorderAnimationParent) winBorderAnimationParent.SetActive(false);
        if (winLineAnimation) winLineAnimation.SetActive(false);
        if (symbolInfoCard) symbolInfoCard.HideCard();
        AudioManager.Instance?.StopTensionBuilder();
        AudioManager.Instance?.StopReelSpinLoop();
    }

    private void SetupSymbolButtons()
    {
        if (reelImagesList == null) return;
        for (int col = 0; col < reelImagesList.Count; col++)
        {
            var reel = reelImagesList[col];
            if (reel == null || reel.images == null) continue;
            int visibleStartIndex = 4;
            int rowCount = 3;
            for (int row = 0; row < rowCount; row++)
            {
                int imageIndex = visibleStartIndex + row;
                if (imageIndex < reel.images.Count && reel.images[imageIndex] != null)
                {
                    Image img = reel.images[imageIndex];
                    SymbolButtonHandler btnHandler = img.GetComponent<SymbolButtonHandler>();
                    if (btnHandler == null)
                    {
                        btnHandler = img.gameObject.AddComponent<SymbolButtonHandler>();
                    }
                    btnHandler.Init(col, row, this);
                }
            }
        }
    }

    internal void OnBetChanged()
    {
        if (symbolInfoCard != null && symbolInfoCard.gameObject.activeSelf)
        {
            symbolInfoCard.RefreshCard(gameManager);
        }
    }

    private Dictionary<Image, int> imageToSymbolIdMap = new Dictionary<Image, int>();

    private void SetImageSymbol(Image img, int symbolId)
    {
        if (img == null) return;
        
        if (symbolId == 99 || symbolId == 9 || symbolId < 0)
        {
            img.sprite = null;
            img.color = new Color(1f, 1f, 1f, 0f);
            img.enabled = false;

            Button btn = img.GetComponent<Button>();
            if (btn != null) btn.interactable = false;
        }
        else
        {
            img.sprite = GetSymbolSprite(symbolId);
            img.color = new Color(1f, 1f, 1f, 1f);
            img.enabled = true;

            Button btn = img.GetComponent<Button>();
            if (btn != null) btn.interactable = true;
        }
        
        imageToSymbolIdMap[img] = symbolId;
    }

    private int GetRandomNonBlankSymbolId(List<int> nonBlankIds = null)
    {
        if (nonBlankIds == null || nonBlankIds.Count == 0)
        {
            nonBlankIds = new List<int>();
            if (symbolSprites != null)
            {
                for (int i = 0; i < symbolSprites.Length; i++)
                {
                    if (symbolSprites[i] != null && i != 9) nonBlankIds.Add(i);
                }
            }
        }
        if (nonBlankIds.Count == 0) return 1;
        return nonBlankIds[Random.Range(0, nonBlankIds.Count)];
    }

    internal void OnSymbolClicked(int col, int row, RectTransform symbolRect)
    {
        if (gameManager != null && gameManager.IsRespinPending)
        {
            gameManager.ExecuteRespin();
            return;
        }

        if (isSpinning)
        {
            if (symbolInfoCard != null) symbolInfoCard.HideCard();
            return;
        }

        if (col >= reelImagesList.Count) return;

        var reel = reelImagesList[col];
        if (reel == null || reel.images == null) return;

        float customYOffset = 0f;
        if (reelTransforms != null && col < reelTransforms.Length && reelTransforms[col] != null)
        {
            float reelY = reelTransforms[col].localPosition.y;
            bool isCase1ReelPos = Mathf.Abs(reelY - (-160f)) < 30f;
            bool isCase1Matrix = (currentDisplayMatrix != null && col < currentDisplayMatrix.Count &&
                                  currentDisplayMatrix[col] != null && currentDisplayMatrix[col].Count >= 3 &&
                                  currentDisplayMatrix[col][1] != 9);

            if (isCase1ReelPos || isCase1Matrix)
            {
                if (row == 0) customYOffset = -10f;
                else if (row == 2) customYOffset = 10f;
            }
        }

        Image clickedImage = GetSymbolImage(col, row);
        if (clickedImage != null)
        {
            if (imageToSymbolIdMap.TryGetValue(clickedImage, out int symbolId))
            {
                if (symbolId == 9 || symbolId == 99 || symbolId < 0) return;
                if (symbolInfoCard != null)
                {
                    symbolInfoCard.ShowCard(symbolId, col, row, symbolRect, gameManager, customYOffset);
                }
                return;
            }
        }

        if (currentDisplayMatrix != null && col < currentDisplayMatrix.Count && row < currentDisplayMatrix[col].Count)
        {
            int fallbackId = currentDisplayMatrix[col][row];
            if (fallbackId == 9 || fallbackId == 99 || fallbackId < 0) return;
            if (symbolInfoCard != null)
            {
                symbolInfoCard.ShowCard(fallbackId, col, row, symbolRect, gameManager, customYOffset);
            }
        }
    }

    private void DisableColumns(ColumnOverlays[] cols)
    {
        if (cols == null) return;
        for (int c = 0; c < cols.Length; c++)
        {
            if (IsColumnLocked(c)) continue;
            var col = cols[c];
            if (col?.rows != null)
            {
                foreach (var go in col.rows)
                {
                    if (go)
                    {
                        ResetWinBoxPosition(go);
                        go.SetActive(false);
                    }
                }
            }
        }
    }

    private Image GetSymbolImage(int col, int row)
    {
        if (reelImagesList == null || col < 0 || col >= reelImagesList.Count) return null;
        var reel = reelImagesList[col];
        if (reel == null || reel.images == null) return null;

        bool isCase1 = currentDisplayMatrix != null && col < currentDisplayMatrix.Count &&
                       currentDisplayMatrix[col] != null && currentDisplayMatrix[col].Count >= 3 &&
                       currentDisplayMatrix[col][1] != 9;

        int imageIndex;
        if (isCase1)
        {
            imageIndex = 4 + row;
        }
        else
        {
            if (row == 0) imageIndex = 4;
            else if (row == 2) imageIndex = 5;
            else return null;
        }

        if (imageIndex >= 0 && imageIndex < reel.images.Count)
        {
            return reel.images[imageIndex];
        }
        return null;
    }

    private GameObject GetWinBoxObject(int col, int row)
    {
        if (winAnimationColumns == null || col < 0 || col >= winAnimationColumns.Length) return null;
        var overlay = winAnimationColumns[col];
        if (overlay == null || overlay.rows == null || overlay.rows.Length == 0) return null;

        if (winAnimationParent && !winAnimationParent.activeSelf)
        {
            winAnimationParent.SetActive(true);
        }

        GameObject animGO = null;
        if (row == 0)
        {
            animGO = overlay.rows[0];
        }
        else if (row == 2)
        {
            animGO = overlay.rows.Length > 1 ? overlay.rows[1] : overlay.rows[0];
        }
        else if (row == 1)
        {
            animGO = overlay.rows[0];
        }

        if (animGO != null)
        {
            Image targetSymbolImage = GetSymbolImage(col, row);
            if (targetSymbolImage != null)
            {
                animGO.transform.position = targetSymbolImage.transform.position;
                animGO.transform.rotation = targetSymbolImage.transform.rotation;
                animGO.transform.localScale = Vector3.one * spineScaleMultiplier;
            }
        }

        return animGO;
    }

    private GameObject WinBox(ColumnOverlays[] cols, int col, int row)
    {
        if (cols == winAnimationColumns)
        {
            return GetWinBoxObject(col, row);
        }
        return (col >= 0 && col < cols?.Length && cols[col]?.rows != null && row >= 0 && row < cols[col].rows.Length)
            ? cols[col].rows[row] : null;
    }

    
    private void BuildSpineArray()
    {
        spineDataArray = new SkeletonDataAsset[10];
        spineDataArray[0] = spineGreenChilli;
        spineDataArray[1] = spineYellowChilli;
        spineDataArray[2] = spineOrangeChilli;
        spineDataArray[3] = spineRedChilli;
        spineDataArray[4] = spineTripleBar;
        spineDataArray[5] = spineDoubleBar;
        spineDataArray[6] = spineSingleBar;
        spineDataArray[7] = spineRedSeven;
        spineDataArray[8] = spineBlueSeven;
    }

    private SkeletonDataAsset GetSpineData(int symbolId)
    {
        if (spineDataArray == null || symbolId < 0 || symbolId >= spineDataArray.Length) return null;
        return spineDataArray[symbolId];
    }

    private string GetSpineSkin(int symbolId)
    {
        if (spineSkinNames == null || symbolId < 0 || symbolId >= spineSkinNames.Length) return null;
        return spineSkinNames[symbolId];
    }

    private void BuildSymbolSpriteArray()
    {
        symbolSprites = new Sprite[10];
        symbolSprites[0] = spriteGreenChilli;
        symbolSprites[1] = spriteYellowChilli;
        symbolSprites[2] = spriteOrangeChilli;
        symbolSprites[3] = spriteRedChilli;
        symbolSprites[4] = spriteTripleBar;
        symbolSprites[5] = spriteDoubleBar;
        symbolSprites[6] = spriteSingleBar;
        symbolSprites[7] = spriteRedSeven;
        symbolSprites[8] = spriteBlueSeven;

        Sprite defaultSprite = null;
        for (int i = 0; i < symbolSprites.Length; i++)
        {
            if (symbolSprites[i] != null)
            {
                defaultSprite = symbolSprites[i];
                break;
            }
        }

        for (int i = 0; i < symbolSprites.Length; i++)
        {
            if (symbolSprites[i] == null)
            {
                symbolSprites[i] = defaultSprite;
            }
        }
    }

    private void InitializeReels()
    {
        cycleDistance = symbolHeight;
        middlePosition = 0f;

        // Initialize with random sprites so they aren't default Blue 7s
        if (reelImagesList != null)
        {
            for (int col = 0; col < reelImagesList.Count; col++)
            {
                SetReelSymbols(col, null, true);
            }
        }



        int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;
        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        currentDisplayMatrix = new List<List<int>>();
        reelCycleCount = new List<int>();
        for (int col = 0; col < reelCount; col++)
        {
            var defaultCol = new List<int>();
            for (int r = 0; r < rowCount; r++)
            {
                defaultCol.Add(0);
            }
            currentDisplayMatrix.Add(defaultCol);
            reelCycleCount.Add(0);
        }
    }

    internal void SetInitialMatrix(List<List<int>> matrix)
    {
        if (matrix == null || matrix.Count == 0) return;

        int reelCount = matrix.Count;
        int rowCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;

        for (int col = 0; col < reelCount; col++)
        {
            if (matrix[col] != null && matrix[col].Count != rowCount) return;
        }

        currentDisplayMatrix = matrix;

        for (int col = 0; col < reelCount; col++)
        {
            if (col < reelCurveIntensity.Length && matrix[col] != null && matrix[col].Count >= 3)
            {
                bool isCase1 = matrix[col][1] != 9;
                reelCurveIntensity[col] = 1f;
            }

            if (col < reelImagesList.Count)
            {
                SetReelSymbols(col, matrix[col], true);
            }
        }

        
    }

    #endregion

    #region Symbol Display

    private float GetTargetYForResult(List<int> columnSymbols)
    {
        if (columnSymbols != null && columnSymbols.Count >= 3)
        {
            if (columnSymbols[1] == 9)
            {
                return middlePosition + case2StopY;
            }
        }
        return middlePosition + case1StopY;
    }

    private void SetReelSymbols(int columnIndex, List<int> visibleSymbolIds, bool isInitial = false)
    {
        if (columnIndex >= reelImagesList.Count) return;

        var reel = reelImagesList[columnIndex];
        if (reel.images == null || reel.images.Count < 9) return;

        bool isCase1 = visibleSymbolIds != null && visibleSymbolIds.Count >= 3 && visibleSymbolIds[1] != 9;

        List<int> nonBlankIds = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        if (visibleSymbolIds != null)
        {
            foreach (int sId in visibleSymbolIds)
            {
                if (sId != 9) nonBlankIds.Remove(sId);
            }
        }

        for (int i = nonBlankIds.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            int temp = nonBlankIds[i];
            nonBlankIds[i] = nonBlankIds[randomIndex];
            nonBlankIds[randomIndex] = temp;
        }

        int bufferIndex = 0;
        HashSet<int> reservedIndices = new HashSet<int>();

        if (isCase1)
        {
            reservedIndices.Add(4);
            reservedIndices.Add(5);
            reservedIndices.Add(6);

            // Case 1: Centered stop (Row 1 has symbol)
            // Image 4 = Row 0 (Upper)
            // Image 5 = Row 1 (Center Payline)
            // Image 6 = Row 2 (Lower)
            SetImageSymbol(reel.images[4], visibleSymbolIds[0]);
            SetImageSymbol(reel.images[5], visibleSymbolIds[1]);
            SetImageSymbol(reel.images[6], visibleSymbolIds[2]);

            // Buffers 3 and 7 blanked on spin stop so stray icons never peek
            if (!isInitial)
            {
                if (reel.images.Count > 3) SetImageSymbol(reel.images[3], 9);
                if (reel.images.Count > 7) SetImageSymbol(reel.images[7], 9);
            }
        }
        else if (visibleSymbolIds != null && visibleSymbolIds.Count >= 3)
        {
            // Case 2: Split / Half-Step stop (Row 1 is 9 / Blank)
            // Image 4 = Row 0 (Upper)
            // Image 5 = Row 2 (Lower)
            reservedIndices.Add(4);
            reservedIndices.Add(5);
            reservedIndices.Add(6);
            reservedIndices.Add(3);
            reservedIndices.Add(7);

            SetImageSymbol(reel.images[4], visibleSymbolIds[0]);
            SetImageSymbol(reel.images[5], visibleSymbolIds[2]);

            // In Case 2, reel lands at 0f, placing Image 6 just below viewport.
            // Image 6, 3, and 7 MUST be set to 9 (blank) so stale icons never peek into view:
            SetImageSymbol(reel.images[6], 9);
            if (reel.images.Count > 3) SetImageSymbol(reel.images[3], 9);
            if (reel.images.Count > 7) SetImageSymbol(reel.images[7], 9);
        }
        else
        {
            // Fallback (initialization before matrix is received)
            reservedIndices.Add(4);
            reservedIndices.Add(5);
            reservedIndices.Add(6);

            SetImageSymbol(reel.images[4], GetRandomNonBlankSymbolId(nonBlankIds));
            SetImageSymbol(reel.images[5], GetRandomNonBlankSymbolId(nonBlankIds));
            SetImageSymbol(reel.images[6], GetRandomNonBlankSymbolId(nonBlankIds));
        }

        // Only randomize non-reserved buffer symbols on initial setup, NEVER during a spin stop
        // so that symbols currently passing through the window are never modified on screen.
        if (isInitial)
        {
            for (int i = 0; i < reel.images.Count; i++)
            {
                if (reservedIndices.Contains(i)) continue;

                int symId = nonBlankIds[bufferIndex % nonBlankIds.Count];
                bufferIndex++;
                SetImageSymbol(reel.images[i], symId);
            }

            if (reelTransforms != null && columnIndex < reelTransforms.Length && reelTransforms[columnIndex] != null)
            {
                reelTransforms[columnIndex].localPosition = new Vector3(
                    reelTransforms[columnIndex].localPosition.x,
                    0f,
                    0f
                );
            }
        }
    }

    private Sprite GetSymbolSprite(int symbolId)
    {
        if (symbolId < 0 || symbolId >= symbolSprites.Length || symbolId == 9)
        {
            return null;
        }

        if (symbolSprites[symbolId] == null)
        {
            return null;
        }

        return symbolSprites[symbolId];
    }

    #endregion

    

    

    


    #region Center Wild Lock & Respin
    private readonly HashSet<int> lockedColumns = new HashSet<int>();
    private Tween centerWildLockTween;

    internal bool IsColumnLocked(int col) => lockedColumns.Contains(col);

    /// <summary>Locks Reel 1 in place and toggles the Re-Spin overlay (spriteReSpin) via SlotAnimHolder for the respin feature.</summary>
    /// <param name="locked">True to lock Reel 1 and show the Re-Spin overlay; False to unlock and hide.</param>
    /// <param name="wildSymbolId">Optional symbol ID of the center wild symbol.</param>
    internal void SetCenterWildLocked(bool locked, int wildSymbolId = -1)
    {
        if (locked)
        {
            lockedColumns.Add(1);
            ShowRespinOverlay(true);
            SetMiddleReelGlow(true);
            //SetRespinReelsGlow(true); // Respin appeared -> turn ON glow for Slot 0 and Slot 2
        }
        else
        {
            lockedColumns.Clear();
            centerWildLockTween?.Kill();
            centerWildLockTween = null;
            ShowRespinOverlay(false);
            SetMiddleReelGlow(false);
            SetRespinReelsGlow(false); // Respin finished -> turn OFF glow
        }
    }

    /// <summary>Controls the glow border on Reel 1 (center) during respin lock using simple SetActive.</summary>
    internal void SetMiddleReelGlow(bool show)
    {
        if (reSpinAnimationBorder != null)
        {
            if (show)
            {
                if (winBorderAnimationParent != null) winBorderAnimationParent.SetActive(false);
                if (winLineAnimation != null) winLineAnimation.SetActive(false);
            }
            reSpinAnimationBorder.SetActive(show);
        }
    }

    /// <summary>Controls the glow border for Slot 0 & Slot 2 (outer reels) during respin using simple SetActive.</summary>
    internal void SetRespinReelsGlow(bool show)
    {
        if (respinReelsGlowObject != null)
        {
            if (show)
            {
                if (winBorderAnimationParent != null) winBorderAnimationParent.SetActive(false);
                if (winLineAnimation != null) winLineAnimation.SetActive(false);
            }
            respinReelsGlowObject.SetActive(show);
        }

    }

    /// <summary>Alias for backwards-compatibility.</summary>
    internal void ShowRespinRollingGlow(bool show) => SetRespinReelsGlow(show);

    /// <summary>Displays the respin animation (spineReSpin or spriteReSpin) on the center reel overlay and enables click-to-respin interaction.</summary>
    /// <param name="show">True to show the Re-Spin overlay on Reel 1; False to hide it.</param>
    private void ShowRespinOverlay(bool show)
    {
        if (winAnimationColumns == null || winAnimationColumns.Length <= 1) return;
        var col1 = winAnimationColumns[1];
        if (col1 == null || col1.rows == null || col1.rows.Length == 0) return;
        GameObject animGO = col1.rows[0];
        if (animGO == null) return;

        if (show)
        {
            if (winAnimationParent != null && !winAnimationParent.activeSelf)
            {
                winAnimationParent.SetActive(true);
            }

            // Align position with center payline of Reel 1
            int paylineRow = GetPaylineRow(1);
            Image centerSymbol = GetSymbolImage(1, paylineRow);
            if (centerSymbol != null)
            {
                animGO.transform.position = centerSymbol.transform.position;
                animGO.transform.rotation = centerSymbol.transform.rotation;
            }

            // Stop sprite image animation
            ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>(true);
            if (imageAnim != null)
            {
                imageAnim.StopAnimation();
            }

            SpineAnimController spine = animGO.GetComponentInChildren<SpineAnimController>(true);
            Image animImage = animGO.GetComponent<Image>();
            if (animImage == null) animImage = animGO.GetComponentInChildren<Image>(true);

            // If spineReSpin is available, play Spine animation:
            if (spineReSpin != null && spine != null)
            {
                if (animImage != null) animImage.enabled = false;
                spine.SetSkeletonData(spineReSpin, "animation");
                spine.Play(true);
            }
            else
            {
                // Fallback to static spriteReSpin with pulsing DOTween
                if (spine != null)
                {
                    spine.Stop();
                    if (spine.SkeletonGraphic != null)
                    {
                        spine.SkeletonGraphic.gameObject.SetActive(false);
                    }
                }

                if (animImage != null)
                {
                    if (spriteReSpin != null)
                    {
                        animImage.sprite = spriteReSpin;
                        animImage.overrideSprite = spriteReSpin;
                    }
                    animImage.enabled = true;
                    animImage.color = Color.white;
                }

                animGO.transform.DOKill();
                animGO.transform.localScale = Vector3.one * (spineScaleMultiplier * 0.7f);
                animGO.transform.DOScale(spineScaleMultiplier * 1.25f, 0.25f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        animGO.transform.DOScale(spineScaleMultiplier * 1.12f, 0.45f)
                            .SetEase(Ease.InOutSine)
                            .SetLoops(-1, LoopType.Yoyo);
                    });
            }

            animGO.SetActive(true);

            // Make it clickable so clicking the Re-Spin icon on the reel triggers respin immediately
            Button btn = animGO.GetComponent<Button>();
            if (btn == null)
            {
                btn = animGO.AddComponent<Button>();
            }
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                if (gameManager != null && gameManager.IsRespinPending)
                {
                    gameManager.ExecuteRespin();
                }
            });
            btn.interactable = true;
        }
        else
        {
            animGO.transform.DOKill();
            animGO.transform.localScale = Vector3.one * spineScaleMultiplier;
            animGO.SetActive(false);

            SpineAnimController spine = animGO.GetComponentInChildren<SpineAnimController>(true);
            if (spine != null)
            {
                spine.Stop();
                if (spine.SkeletonGraphic != null)
                {
                    spine.SkeletonGraphic.gameObject.SetActive(false);
                }
            }

            if (winAnimationParent != null)
            {
                winAnimationParent.SetActive(false);
            }
        }
    }

    private int GetPaylineRow(int col)
    {
        if (currentDisplayMatrix != null && col < currentDisplayMatrix.Count && currentDisplayMatrix[col] != null)
        {
            return currentDisplayMatrix[col].Count >= 3 ? 1 : 0;
        }
        return 1;
    }
    #endregion


    internal bool IsSpinning()
    {
        return isSpinning;
    }


    private void KillAllTweens()
    {
        foreach (var tween in spinTweens)
        {
            tween?.Kill();
        }
        spinTweens.Clear();

        if (reelSettleCurveTweens != null)
        {
            for (int i = 0; i < reelSettleCurveTweens.Length; i++)
            {
                if (reelSettleCurveTweens[i] != null)
                {
                    reelSettleCurveTweens[i].Kill();
                    reelSettleCurveTweens[i] = null;
                }
            }
        }

        UpdateCylindricalSpinEffect(force: true);

        KillWinTweens();
    }

    

    #region Cleanup

    private void OnDestroy()
    {
        KillAllTweens();
    }

    #endregion
}

[System.Serializable]
internal class ReelImages
{
    [SerializeField] internal List<Image> images = new List<Image>(16);
}


[System.Serializable]
internal class ColumnOverlays
{
    [Tooltip("Row 0 = top (Case 2) / middle (Case 1 y=6.5), Row 1 = bottom (Case 2)")]
    [SerializeField] internal GameObject[] rows = new GameObject[2];
}
