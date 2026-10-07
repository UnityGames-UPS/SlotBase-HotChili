using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;
using Best.HTTP.SecureProtocol.Org.BouncyCastle.Math.Field;

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
    
    [Header("Spine Skin Names (Optional)")]
    [Tooltip("If you combined multiple symbols into one Spine file, type the Skin Name here for each symbol ID (0 to 9).")]
    public string[] spineSkinNames = new string[10];

    [Tooltip("Global scale modifier to shrink or grow Spine Win animations.")]
    public float spineScaleMultiplier = 0.8f;
    
    private SkeletonDataAsset[] spineDataArray;

    private Sprite[] symbolSprites;

    [Header("Win Animation Sprite Arrays for All Icons")]
    private List<Sprite> animSpritesRed3X;
    private List<Sprite> animSpritesBlue2X;
    private List<Sprite> animSpritesBlue7;
    private List<Sprite> animSpritesWhite7;
    private List<Sprite> animSpritesWhite7Bar;
    private List<Sprite> animSpritesRed7;
    private List<Sprite> animSpritesTripleBar;
    private List<Sprite> animSpritesDoubleBar;
    private List<Sprite> animSpritesSingleBar;
    private List<Sprite> animSpritesSpin;
    private List<Sprite> animSpritesGreenWheel;
    private List<Sprite> animSpritesDoubleWheel;
    private List<Sprite> animSpritesRedWheel;

    private List<Sprite>[] animationSpriteArrays;

    [Header("Reel Containers")]
    [SerializeField] private Transform[] reelTransforms;

    [Header("Reel Images - 14 images per reel")]
    [SerializeField] private List<ReelImages> reelImagesList;

    [Header("Reel Stop Y Positions")]
    [SerializeField] private float case1StopY = 150.5f;
    [SerializeField] private float case2StopY = 0f;
    
    [Header("Spin Settings")]
    [SerializeField] private float symbolHeight = 500f;
    [SerializeField] private float spinSpeed = 2000f;
    [SerializeField] private float reelStartStagger = 0.08f;
    [SerializeField] private float reelStopStagger = 0.12f;

    [Header("Stop Animation Settings")]
    [SerializeField] private float stopOvershootDistance = 50f;
    [SerializeField] private float stopOvershootDuration = 0.20f;
    [SerializeField] private float stopSettleDuration = 0.30f;

    [Header("Quick Spin Settings")]
    [SerializeField] private float quickStopStagger = 0.06f;
    [SerializeField] private float quickStopOvershoot = 20f;
    [SerializeField] private float quickStopDuration = 0.2f;
    [SerializeField] private int minSpinCyclesBeforeStop = 3;


    [Header("Win Animation Settings")]
    [SerializeField] private float winSymbolLoopDuration = 1.2f;

    [Header("Phase 1 Total Win Presentation")]
    [SerializeField] private TMPro.TMP_Text phase1TotalWinText;

    [Header("Win Animation Objects — Col 0..4  (each has 2 rows, contains ImageAnimation component)")]
    [SerializeField] private GameObject winAnimationParent;
    [SerializeField] private GameObject winBorderAnimationParent;
    [Tooltip("GameObject references for win animations. Each should have an ImageAnimation component attached.")]
    [SerializeField] private ColumnOverlays[] winAnimationColumns = new ColumnOverlays[5];

    [Header("Tension / Anticipation Settings")]
    [Tooltip("Frame object to enable on the last slot during tension extra spin when wheel feature is triggered.")]
    [SerializeField] private GameObject lastSlotTensionFrame;
    [Tooltip("Extra spin duration in seconds for the last slot during tension spin.")]
    [SerializeField] private float tensionSpinExtraDuration = 2.0f;


    [Header("Symbol Info Card")]
    [SerializeField] private SymbolInfoCard symbolInfoCard;

    [Header("Cylindrical Spin Effect Settings")]
        [Tooltip("Optional parent RectTransform reference (e.g. reel viewport frame) to automatically measure visible half height from parent rect height.")]
    [SerializeField] private RectTransform visibleAreaRectTransform;
                                
    private float[] reelCurveIntensity = new float[3] { 1f, 1f, 1f };
    private Tween[] reelSettleCurveTweens = new Tween[3];
    public CylindricalSpinEffect cylindricalEffect;


    private float middlePosition = 0f;
    private float cycleDistance;


    private List<Tween> spinTweens = new List<Tween>();
    private List<Tween> winTweens = new List<Tween>();
    private List<int> reelCycleCount = new List<int>();
    private Coroutine winAnimationCoroutine;


    internal List<List<int>> currentDisplayMatrix;

    private bool isSpinning;

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

    private Vector3 GetOriginalWinBoxPosition(GameObject go)
    {
        if (go != null && originalWinBoxLocalPositions != null && originalWinBoxLocalPositions.TryGetValue(go, out Vector3 origPos))
        {
            return origPos;
        }
        return go != null ? go.transform.localPosition : Vector3.zero;
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
        if (cylindricalEffect == null)
        {
            cylindricalEffect = GetComponent<CylindricalSpinEffect>();
            if (cylindricalEffect == null)
            {
                cylindricalEffect = gameObject.AddComponent<CylindricalSpinEffect>();
            }
        }
        BuildSymbolSpriteArray();
        BuildSpineArray();
        InitializeReels();
    }
    private void Start()
    {
        if (cylindricalEffect != null)
        {
            cylindricalEffect.Initialize(reelImagesList, reelTransforms, visibleAreaRectTransform, reelCurveIntensity, () => isSpinning);
        }
        if (symbolSprites == null || symbolSprites.Length == 0)
        {
            BuildSymbolSpriteArray();
            BuildSpineArray();
        }

        CacheOriginalWinBoxPositions();
        DisableAllOverlays();
        SetupSymbolButtons();
    }

    internal void DisableAllOverlays()
    {
        DisableColumns(winAnimationColumns);
        if (winAnimationParent) winAnimationParent.SetActive(false);
        if(winBorderAnimationParent) winBorderAnimationParent.SetActive(false);
        HidePhase1TotalWinText(false);
        if (symbolInfoCard) symbolInfoCard.HideCard();
        if (lastSlotTensionFrame) lastSlotTensionFrame.SetActive(false);
        AudioManager.Instance?.StopTensionBuilder();
        AudioManager.Instance?.StopReelSpinLoop();
        AudioManager.Instance?.StopWheelTriggerWinLine();
    }

    private void SetupSymbolButtons()
    {
        if (reelImagesList == null) return;
        for (int col = 0; col < reelImagesList.Count; col++)
        {
            var reel = reelImagesList[col];
            if (reel == null || reel.images == null) continue;
            int visibleStartIndex = 6;
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
        
        if (symbolId == 99)
        {
            img.sprite = null;
            img.color = new Color(1f, 1f, 1f, 0f);
        }
        else
        {
            img.sprite = GetSymbolSprite(symbolId);
            img.color = new Color(1f, 1f, 1f, 1f);
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
                    if (symbolSprites[i] != null && i != 0) nonBlankIds.Add(i);
                }
            }
        }
        if (nonBlankIds.Count == 0) return 1;
        return nonBlankIds[Random.Range(0, nonBlankIds.Count)];
    }

    internal void OnSymbolClicked(int col, int row, RectTransform symbolRect)
    {
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
                                  currentDisplayMatrix[col][1] != 0);

            if (isCase1ReelPos || isCase1Matrix)
            {
                if (row == 0) customYOffset = -10f;
                else if (row == 2) customYOffset = 10f;
            }
        }

        int imageIndex = 6 + row;
        if (imageIndex < reel.images.Count && reel.images[imageIndex] != null)
        {
            Image clickedImage = reel.images[imageIndex];
            if (imageToSymbolIdMap.TryGetValue(clickedImage, out int symbolId))
            {
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
            if (symbolInfoCard != null)
            {
                symbolInfoCard.ShowCard(fallbackId, col, row, symbolRect, gameManager, customYOffset);
            }
        }
    }

    private void DisableColumns(ColumnOverlays[] cols)
    {
        if (cols == null) return;
        foreach (var col in cols)
        {
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

    internal Image GetSymbolImage(int col, int row)
    {
        if (reelImagesList == null || col < 0 || col >= reelImagesList.Count) return null;
        var reel = reelImagesList[col];
        if (reel == null || reel.images == null) return null;

        bool isCase1 = currentDisplayMatrix != null && col < currentDisplayMatrix.Count &&
                       currentDisplayMatrix[col] != null && currentDisplayMatrix[col].Count >= 3 &&
                       currentDisplayMatrix[col][1] != 0;

        int imageIndex;
        if (isCase1)
        {
            imageIndex = 6 + row;
        }
        else
        {
            if (row == 0) imageIndex = 6;
            else if (row == 2) imageIndex = 7;
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
                Vector3 symScale = targetSymbolImage.transform.localScale;
                animGO.transform.localScale = new Vector3(
                    symScale.x * spineScaleMultiplier,
                    symScale.y * spineScaleMultiplier,
                    symScale.z * spineScaleMultiplier
                );
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

    internal string GetSpineSkin(int symbolId)
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
        animationSpriteArrays = new List<Sprite>[15];
        animationSpriteArrays[1] = animSpritesRed3X;
        animationSpriteArrays[2] = animSpritesBlue2X;
        animationSpriteArrays[3] = animSpritesBlue7;
        animationSpriteArrays[4] = animSpritesWhite7;
        animationSpriteArrays[5] = animSpritesWhite7Bar;
        animationSpriteArrays[6] = animSpritesRed7;
        animationSpriteArrays[7] = animSpritesTripleBar;
        animationSpriteArrays[8] = animSpritesDoubleBar;
        animationSpriteArrays[9] = animSpritesSingleBar;
        animationSpriteArrays[10] = animSpritesSpin;
        animationSpriteArrays[11] = animSpritesGreenWheel;
        animationSpriteArrays[12] = animSpritesDoubleWheel;
        animationSpriteArrays[13] = animSpritesRedWheel;
        animationSpriteArrays[14] = animSpritesRedWheel;
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
                bool isCase1 = matrix[col][1] != 0;
                reelCurveIntensity[col] = isCase1 ? 1f : 0f;
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
            if (columnSymbols[1] == 0)
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

        bool isCase1 = visibleSymbolIds != null && visibleSymbolIds.Count >= 3 && visibleSymbolIds[1] != 0;

        List<int> nonBlankIds = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        if (visibleSymbolIds != null)
        {
            foreach (int sId in visibleSymbolIds)
            {
                if (sId != 0) nonBlankIds.Remove(sId);
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
            reservedIndices.Add(6);
            reservedIndices.Add(7);
            reservedIndices.Add(8);

            SetImageSymbol(reel.images[6], visibleSymbolIds[0]);
            SetImageSymbol(reel.images[7], visibleSymbolIds[1]);
            SetImageSymbol(reel.images[8], visibleSymbolIds[2]);
        }
        else if (visibleSymbolIds != null && visibleSymbolIds.Count >= 3)
        {
            reservedIndices.Add(6);
            reservedIndices.Add(7);

            SetImageSymbol(reel.images[6], visibleSymbolIds[0]);
            SetImageSymbol(reel.images[7], visibleSymbolIds[2]);
        }
        else
        {
            reservedIndices.Add(6);
            reservedIndices.Add(7);
            reservedIndices.Add(8);

            SetImageSymbol(reel.images[6], GetRandomNonBlankSymbolId(nonBlankIds));
            SetImageSymbol(reel.images[7], GetRandomNonBlankSymbolId(nonBlankIds));
            SetImageSymbol(reel.images[8], GetRandomNonBlankSymbolId(nonBlankIds));
        }

        for (int i = 0; i < reel.images.Count; i++)
        {
            if (reservedIndices.Contains(i)) continue;

            int symId = nonBlankIds[bufferIndex % nonBlankIds.Count];
            bufferIndex++;
            SetImageSymbol(reel.images[i], symId);
        }

        if (isInitial && reelTransforms[columnIndex] != null)
        {
            reelTransforms[columnIndex].localPosition = new Vector3(
                reelTransforms[columnIndex].localPosition.x,
                0f,
                0f
            );
        }
    }

    private Sprite GetSymbolSprite(int symbolId)
    {
        if (symbolId < 0 || symbolId >= symbolSprites.Length)
        {
            return symbolSprites[0];
        }

        if (symbolSprites[symbolId] == null)
        {
            return symbolSprites[0];
        }

        return symbolSprites[symbolId];
    }

    #endregion

    

    

    

    internal void AnimateDualWheelWin(System.Action onComplete = null)
    {
        if (currentDisplayMatrix == null)
        {
            onComplete?.Invoke();
            return;
        }

        KillWinTweens();
        AudioManager.Instance?.PlayWheelTriggerWinLine();
        if (gameManager != null && gameManager.uiManager != null)
        {
            gameManager.uiManager.EnableRainbowPanel();
        }

        List<ImageAnimation> activeWheelAnims = new List<ImageAnimation>();
        int completedCount = 0;
        int targetLoops = 2;

        for (int col = 0; col < 5; col++)
        {
            if (col >= currentDisplayMatrix.Count) continue;
            for (int row = 0; row < currentDisplayMatrix[col].Count; row++)
            {
                int symId = currentDisplayMatrix[col][row];
                if (symId >= 10 && symId <= 13)
                {
                    var animGO = WinBox(winAnimationColumns, col, row);
                    if (animGO != null)
                    {
                        ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
                        int imageIndex = 6 + row;
                        Image symbolImage = (col < reelImagesList.Count && reelImagesList[col].images != null && imageIndex < reelImagesList[col].images.Count)
                            ? reelImagesList[col].images[imageIndex]
                            : null;

                        if (imageAnim != null)
                        {
                            activeWheelAnims.Add(imageAnim);

                            List<Sprite> animSprites = (animationSpriteArrays != null && symId >= 0 && symId < animationSpriteArrays.Length) ? animationSpriteArrays[symId] : null;
                            if (animSprites != null && animSprites.Count > 0)
                            {
                                if (animSprites != null)
            {
                imageAnim.textureArray = animSprites;
            }
                            }
                            imageAnim.animationMode = ImageAnimation.AnimationMode.SINGLE_PHASE;
                            imageAnim.useDynamicFramerate = true;
                            imageAnim.dynamicLoopDuration = winSymbolLoopDuration;
                            imageAnim.doLoopAnimation = true;
                            imageAnim.delayBetweenLoop = 0f;

                            animGO.SetActive(true);
                            Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<Image>();
                            if (animRenderer == null && animGO != null) animRenderer = animGO.GetComponentInChildren<Image>();
                            if (animRenderer != null)
                            {
                                animRenderer.DOKill();
                                Color c = animRenderer.color;
                                animRenderer.color = new Color(c.r, c.g, c.b, 1f);
                                animRenderer.enabled = true;
                                animRenderer.gameObject.SetActive(true);
                            }
                            if (symbolImage != null)
                            {
                                symbolImage.DOKill();
                                Color c = symbolImage.color;
                                symbolImage.color = new Color(c.r, c.g, c.b, 0f);
                                symbolImage.enabled = false;
                                symbolImage.gameObject.SetActive(false);
                            }

                            imageAnim.onLoopComplete = (loopCount) =>
                            {
                                if (loopCount >= targetLoops)
                                {
                                    imageAnim.onLoopComplete = null;
                                    imageAnim.StopAnimation();
                                    if (animGO != null)
                                    {
                                        ResetWinBoxPosition(animGO);
                                        animGO.SetActive(false);
                                    }

                                    if (symbolImage != null)
                                    {
                                        symbolImage.DOKill();
                                        Color c = symbolImage.color;
                                        symbolImage.color = new Color(c.r, c.g, c.b, 1f);
                                        symbolImage.enabled = true;
                                        symbolImage.gameObject.SetActive(true);
                                    }

                                    completedCount++;
                                    if (completedCount >= activeWheelAnims.Count)
                                    {
                                        DisableAllOverlays();
                                        onComplete?.Invoke();
                                    }
                                }
                            };

                            
            SpineAnimController spine = (imageAnim != null) ? imageAnim.GetComponentInParent<SpineAnimController>() : null;
            if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null) 
                            { var ar = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<UnityEngine.UI.Image>();
                                if (ar != null) ar.enabled = false; spine.SkeletonGraphic.MatchRectTransformWithBounds(); 
                                spine.Play(true); }
            else { imageAnim.StartAnimation(); }
                        }
                    }
                }
            }
        }

        if (activeWheelAnims.Count == 0)
        {
            DisableAllOverlays();
            onComplete?.Invoke();
        }
    }



    



    internal List<List<int>> GetCurrentDisplayMatrix()
    {
        return currentDisplayMatrix;
    }

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

        if (cylindricalEffect != null)
        {
            cylindricalEffect.StopEffect();
            
        }

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
public class ReelImages
{
    public List<Image> images = new List<Image>(16);
}


[System.Serializable]
public class ColumnOverlays
{
    [Tooltip("Row 0 = top (Case 2) / middle (Case 1 y=6.5), Row 1 = bottom (Case 2)")]
    public GameObject[] rows = new GameObject[2];
}
