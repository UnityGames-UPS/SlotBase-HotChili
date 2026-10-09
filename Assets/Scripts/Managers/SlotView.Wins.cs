using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public partial class SlotView
{
    #region Win Line Animation
    
        public void ShowWinLineAnimation(List<WinLine> winLines, System.Action onComplete)
        {
            if (winLines == null || winLines.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }
    
            KillWinTweens();
            winAnimationCoroutine = StartCoroutine(PlaySingleWinLineAnimation(winLines, onComplete));
        }
    
        private IEnumerator PlaySingleWinLineAnimation(List<WinLine> winLines, System.Action onComplete)
        {
            if (winLines == null || winLines.Count == 0)
            {
                onComplete?.Invoke();
                yield break;
            }
    
            HashSet<int> flatPositions = new HashSet<int>();
            double totalWinAmount = 0;
            foreach (var line in winLines)
            {
                if (line != null)
                {
                    totalWinAmount += line.winAmount;
                    if (line.positions != null)
                    {
                        foreach (int pos in line.positions)
                        {
                            flatPositions.Add(pos);
                        }
                    }
                }
            }
    
            if (flatPositions.Count == 0)
            {
                onComplete?.Invoke();
                yield break;
            }
    
    
            AudioManager.Instance?.PlayWinLinePhase1Start();
    
            bool isAutoPlaying = (gameManager != null && gameManager.isAutoPlaying);
    
            if (isAutoPlaying)
            {
                yield return StartCoroutine(AnimateWinPositionsSingleLoop(flatPositions));
                yield return new WaitForSeconds(0.15f);
                onComplete?.Invoke();
            }
            else
            {
                StartContinuousWinAnimation(flatPositions);
                onComplete?.Invoke();
            }
        }
    
        private IEnumerator AnimateWinPositionsSingleLoop(IEnumerable<int> flatPositions)
        {
            if (flatPositions == null) yield break;
    
            int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;
            int rowLimit = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;
    
            List<ImageAnimation> activeAnims = new List<ImageAnimation>();
            int completedCount = 0;
            bool isCompleted = false;
    
            foreach (int flatIndex in flatPositions)
            {
                int row = flatIndex / reelCount;
                int col = flatIndex % reelCount;
    
                if (col < 0 || col >= 5 || row < 0 || row >= rowLimit) continue;
    
                Image symbolImage = GetSymbolImage(col, row);
                if (symbolImage == null) continue;
    
                var animGO = WinBox(winAnimationColumns, col, row);
                if (animGO == null) continue;
    
                SpineAnimController spineAnim = animGO.GetComponentInChildren<SpineAnimController>();
                if (currentDisplayMatrix != null && col < currentDisplayMatrix.Count && row < currentDisplayMatrix[col].Count)
                {
                    int sId = currentDisplayMatrix[col][row];
                    if (spineAnim != null) spineAnim.SetSkeletonData(GetSpineData(sId), GetSpineSkin(sId), GetSpineSkin(sId));
                }
    
                ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
                if (imageAnim == null) continue;
    
                if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;
                int symbolId = currentDisplayMatrix[col][row];
                if (symbolId < 0 || symbolId >= animationSpriteArrays.Length) continue;
    
                List<Sprite> animSprites = animationSpriteArrays[symbolId];
                
    
                if (animSprites != null)
                {
                    imageAnim.textureArray = animSprites;
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
    
                activeAnims.Add(imageAnim);
    
                imageAnim.onLoopComplete = (currentLoop) =>
                {
                    if (currentLoop >= 1)
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
                        if (completedCount >= activeAnims.Count)
                        {
                            isCompleted = true;
                        }
                    }
                };
            }
    
            foreach (var imageAnim in activeAnims)
            {
                SpineAnimController spine = (imageAnim != null) ? imageAnim.GetComponentInParent<SpineAnimController>() : null;
                if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null) 
                { 
                    var ar = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<UnityEngine.UI.Image>(); 
                    if (ar != null) ar.enabled = false; 
                    spine.Play(true); 
                }
                else 
                { 
                    imageAnim.StartAnimation(); 
                }
            }

            foreach (int flatIndex in flatPositions)
            {
                int row = flatIndex / reelCount;
                int col = flatIndex % reelCount;
                if (col >= 0 && col < 5 && row >= 0 && row < rowLimit)
                {
                    Image symbolImage = GetSymbolImage(col, row);
                    if (symbolImage != null)
                    {
                        symbolImage.DOKill();
                        Color c = symbolImage.color;
                        symbolImage.color = new Color(c.r, c.g, c.b, 0f);
                        symbolImage.enabled = false;
                        symbolImage.gameObject.SetActive(false);
                    }
                }
            }
    
            float loopDuration = winSymbolLoopDuration > 0 ? winSymbolLoopDuration : 1.2f;
            float elapsed = 0f;
            while (!isCompleted && elapsed < loopDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Clean up overlays
            foreach (var imageAnim in activeAnims)
            {
                if (imageAnim != null)
                {
                    imageAnim.onLoopComplete = null;
                    imageAnim.StopAnimation();
                    var spine = imageAnim.GetComponentInParent<SpineAnimController>();
                    if (spine != null) spine.Pause();
                }
            }
        }
    
        private void StartContinuousWinAnimation(IEnumerable<int> flatPositions)
        {
            if (flatPositions == null) return;
    
            int reelCount = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.reelCount : 3;
            int rowLimit = (gameManager != null && gameManager.gameConfig != null) ? gameManager.gameConfig.rowCount : 3;
    
            if (winAnimationParent && !winAnimationParent.activeSelf)
            {
                winAnimationParent.SetActive(true);
            }

        if (winBorderAnimationParent) winBorderAnimationParent.SetActive(true);

        foreach (int flatIndex in flatPositions)
            {
                int row = flatIndex / reelCount;
                int col = flatIndex % reelCount;
    
                if (col < 0 || col >= 5 || row < 0 || row >= rowLimit) continue;
    
                Image symbolImage = GetSymbolImage(col, row);
                if (symbolImage == null) continue;
    
                var animGO = WinBox(winAnimationColumns, col, row);
                if (animGO == null) continue;
    
                SpineAnimController spineAnim = animGO.GetComponentInChildren<SpineAnimController>();
                if (currentDisplayMatrix != null && col < currentDisplayMatrix.Count && row < currentDisplayMatrix[col].Count)
                {
                    int sId = currentDisplayMatrix[col][row];
                    var data = GetSpineData(sId);
                    if (data == null) Debug.LogWarning($"[SlotView] No Spine Data found for symbol ID: {sId} on Col: {col}, Row: {row}");
                    // log silenced
                    if (spineAnim != null) spineAnim.SetSkeletonData(data, null, GetSpineSkin(sId));
                }
    
                ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
                if (imageAnim == null) continue;
    
                if (currentDisplayMatrix == null || col >= currentDisplayMatrix.Count || row >= currentDisplayMatrix[col].Count) continue;
                int symbolId = currentDisplayMatrix[col][row];
                if (symbolId < 0 || symbolId >= animationSpriteArrays.Length) continue;
    
                List<Sprite> animSprites = animationSpriteArrays[symbolId];
                
    
                if (animSprites != null)
                {
                    imageAnim.textureArray = animSprites;
                }
                imageAnim.animationMode = ImageAnimation.AnimationMode.SINGLE_PHASE;
                imageAnim.useDynamicFramerate = true;
                imageAnim.dynamicLoopDuration = winSymbolLoopDuration;
                imageAnim.doLoopAnimation = true;
                imageAnim.delayBetweenLoop = 0f;
                imageAnim.onLoopComplete = null;
    
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
    
                
                SpineAnimController spine = (imageAnim != null) ? imageAnim.GetComponentInParent<SpineAnimController>() : null;
                if (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset != null) 
                { 
                    // log silenced
                    var ar = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<UnityEngine.UI.Image>(); 
                    if (ar != null) ar.enabled = false; 
                    // spine.SkeletonGraphic.MatchRectTransformWithBounds(); 
                    spine.Play(true); 
                }
                else 
                { 
                    bool isSpineNull = (spine == null);
                    bool isSGNull = (spine != null && spine.SkeletonGraphic == null);
                    bool isAssetNull = (spine != null && spine.SkeletonGraphic != null && spine.SkeletonGraphic.skeletonDataAsset == null);
                    // log silenced
                    imageAnim.StartAnimation(); 
                }
            }
        }
    
        private void HideAllWinLineTexts()
        {
            if (reelImagesList == null) return;
            foreach (var reel in reelImagesList)
            {
                if (reel.images != null)
                {
                    foreach (var image in reel.images)
                    {
                        if (image != null)
                        {
                            Transform textTransform = image.transform.Find("WinLineText");
                            if (textTransform != null)
                            {
                                textTransform.DOKill();
                                textTransform.localScale = Vector3.one;
                                textTransform.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
    
        public static string FormatSpriteText(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
    
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in input)
            {
                if (c >= '0' && c <= '9')
                {
                    sb.Append("<sprite=").Append(c - '0').Append(">");
                }
                else if (c == '=')
                {
                    sb.Append("<sprite=10>");
                }
                else if (c == '.' || c == ',')
                {
                    sb.Append("<sprite=11>");
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    
        public static string FormatSpriteText(double amount)
        {
            return FormatSpriteText(amount.ToString("0.###"));
        }

        private void KillWinTweens(bool stopCoroutine = true)
        {
            foreach (var tween in winTweens)
            {
                tween?.Kill();
            }
            winTweens.Clear();
    
            if (stopCoroutine && winAnimationCoroutine != null)
            {
                StopCoroutine(winAnimationCoroutine);
                winAnimationCoroutine = null;
            }
    
            if (winAnimationColumns != null)
            {
                foreach (var col in winAnimationColumns)
                {
                    if (col?.rows != null)
                    {
                        foreach (var animGO in col.rows)
                        {
                            if (animGO != null)
                            {
                                ImageAnimation imageAnim = animGO.GetComponentInChildren<ImageAnimation>();
                                if (imageAnim != null)
                                {
                                    imageAnim.onLoopComplete = null;
                                    Image animRenderer = imageAnim.rendererDelegate != null ? imageAnim.rendererDelegate : imageAnim.GetComponent<Image>();
                                    if (animRenderer == null) animRenderer = animGO.GetComponentInChildren<Image>();
                                    if (animRenderer != null)
                                    {
                                        animRenderer.DOKill();
                                        Color ac = animRenderer.color;
                                        animRenderer.color = new Color(ac.r, ac.g, ac.b, 1f);
                                    }
                                    imageAnim.StopAnimation();
                                }
                                if (animGO.activeSelf)
                                {
                                    animGO.SetActive(false);
                                }
                            }
                        }
                    }
                }
            }
    
            DisableColumns(winAnimationColumns);
            if (winAnimationParent) winAnimationParent.SetActive(false);
            if (winBorderAnimationParent) winBorderAnimationParent.SetActive(false);
            HideAllWinLineTexts();
    
            foreach (var reel in reelImagesList)
            {
                if (reel.images != null)
                {
                    foreach (var image in reel.images)
                    {
                        if (image != null)
                        {
                            image.DOKill();
                            image.transform.localScale = Vector3.one;
                            Color c = image.color;
                            image.color = new Color(c.r, c.g, c.b, 1f);
                            image.enabled = true;
                            if (!image.gameObject.activeSelf)
                            {
                                image.gameObject.SetActive(true);
                            }
                        }
                    }
                }
            }
        }
    
        #endregion
}