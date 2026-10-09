using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public partial class SlotView
{
    #region Spin Animation
    
        public void StartSpin()
        {
            if (isSpinning) return;
    
            if (symbolInfoCard != null) symbolInfoCard.HideCard();
    
            isSpinning = true;
            KillAllTweens();
    
            for (int i = 0; i < reelCurveIntensity.Length; i++)
            {
                if (reelSettleCurveTweens[i] != null)
                {
                    reelSettleCurveTweens[i].Kill();
                    reelSettleCurveTweens[i] = null;
                }
            }
    
            DisableAllOverlays();
            AudioManager.Instance?.PlayReelSpinLoop();
    
            UpdateCylindricalSpinEffect(force: false);

        for (int i = 0; i < reelCycleCount.Count; i++)
            {
                reelCycleCount[i] = 0;
            }
    
            int reelCount = currentDisplayMatrix != null ? currentDisplayMatrix.Count : (gameManager?.gameConfig != null ? gameManager.gameConfig.reelCount : 3);
            int maxCols = Mathf.Min(reelCount, reelTransforms != null ? reelTransforms.Length : 3);
    
            for (int col = 0; col < maxCols; col++)
            {
                StartReelCycleWithDelay(col, col * reelStartStagger);
            }
        }
    
        private void StartReelCycleWithDelay(int columnIndex, float delay)
        {
            if (columnIndex >= reelTransforms.Length) return;

            Sequence startSequence = DOTween.Sequence();
            if (delay > 0)
            {
                startSequence.AppendInterval(delay);
            }

            Transform slotTransform = reelTransforms[columnIndex];
            float startY = slotTransform.localPosition.y;

            // Wind-Up / Anticipation Effect:
            startSequence.Append(slotTransform.DOLocalMoveY(startY + 45f, 0.12f).SetEase(Ease.OutCubic));
            startSequence.Append(slotTransform.DOLocalMoveY(startY - 30f, 0.08f).SetEase(Ease.InQuad));

            startSequence.OnComplete(() =>
            {
                if (isSpinning)
                {
                    StartReelCycle(columnIndex);
                }
            });
            startSequence.Play();

            if (spinTweens.Count <= columnIndex)
                spinTweens.Add(startSequence);
            else
                spinTweens[columnIndex] = startSequence;
        }
        
    
        private void StartReelCycle(int columnIndex)
        {
            if (columnIndex >= reelTransforms.Length) return;
            if (!isSpinning) return;
    
            if (columnIndex < reelCurveIntensity.Length)
            {
                if (reelSettleCurveTweens[columnIndex] != null)
                {
                    reelSettleCurveTweens[columnIndex].Kill();
                    reelSettleCurveTweens[columnIndex] = null;
                }
                reelCurveIntensity[columnIndex] = 1f;
            }
    
            Transform slotTransform = reelTransforms[columnIndex];
            var reel = (columnIndex < reelImagesList.Count) ? reelImagesList[columnIndex] : null;
            int totalImages = (reel != null && reel.images != null && reel.images.Count > 0) ? reel.images.Count : 14;
    
            int bufferCount = totalImages - 3;
            float fullDistance = bufferCount * symbolHeight;
            float halfDistance = fullDistance / 2f;
    
            float spinTopY = middlePosition + halfDistance;
            float spinBottomY = middlePosition - halfDistance;
    
            slotTransform.localPosition = new Vector3(slotTransform.localPosition.x, spinTopY, 0);
    
            float currentSpeed = spinSpeed;
            float loopDuration = fullDistance / currentSpeed;
    
            Tweener loopTweener = slotTransform.DOLocalMoveY(spinBottomY, loopDuration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .OnStepComplete(() =>
                {
                    if (columnIndex < reelCycleCount.Count)
                    {
                        reelCycleCount[columnIndex]++;
                    }
                });
    
            if (spinTweens.Count <= columnIndex)
                spinTweens.Add(loopTweener);
            else
                spinTweens[columnIndex] = loopTweener;
        }
    
        #endregion
    
    #region Stop Spin
    
        public void StopSpin(List<List<int>> resultMatrix, System.Action onComplete, bool isTurbo = false)
        {
            int reelCount = resultMatrix != null ? resultMatrix.Count : (gameManager?.gameConfig != null ? gameManager.gameConfig.reelCount : 3);
            int maxCols = Mathf.Min(reelCount, reelTransforms != null ? reelTransforms.Length : 3);
    
            if (!isSpinning)
            {
                currentDisplayMatrix = resultMatrix;
                for (int col = 0; col < maxCols; col++)
                {
                    SetReelSymbols(col, resultMatrix[col], false);
                }
                onComplete?.Invoke();
                return;
            }
    
            StartCoroutine(StopSpinSequence(resultMatrix, onComplete, false, isTurbo));
        }
    
        private IEnumerator StopSpinSequence(List<List<int>> resultMatrix, System.Action onComplete, bool isQuickStop, bool isTurbo = false)
        {
            currentDisplayMatrix = resultMatrix;
            int reelCount = resultMatrix != null ? resultMatrix.Count : 3;
            int maxCols = Mathf.Min(reelCount, reelTransforms != null ? reelTransforms.Length : 3);
    
            if (!isQuickStop && !isTurbo)
            {
                while (true)
                {
                    bool allReelsReady = true;
                    for (int col = 0; col < maxCols; col++)
                    {
                        if (col < reelCycleCount.Count && reelCycleCount[col] < minSpinCyclesBeforeStop)
                        {
                            allReelsReady = false;
                            break;
                        }
                    }
    
                    if (allReelsReady) break;
                    yield return null;
                }
            }
    
            AudioManager.Instance?.StopReelSpinLoop();
    
            bool isWheelTriggered = false;
    
            float stagger = isQuickStop ? quickStopStagger : (isTurbo ? (reelStopStagger * 0.5f) : reelStopStagger);
    
            for (int col = 0; col < maxCols; col++)
            {
                bool isLastReel = (col == maxCols - 1);
                float delay = col * stagger;
                if (isLastReel && isWheelTriggered)
                {
                    delay += tensionSpinExtraDuration;
                }
                StartCoroutine(StopSingleReel(col, resultMatrix[col], delay, isQuickStop || isTurbo, isLastReel && isWheelTriggered, stagger));
            }
    
            float extraDelay = isWheelTriggered ? tensionSpinExtraDuration : 0f;
            float longestStopTime;
            if (isQuickStop)
            {
                longestStopTime = ((maxCols - 1) * stagger) + extraDelay + quickStopDuration;
            }
            else if (isTurbo)
            {
                longestStopTime = ((maxCols - 1) * stagger) + extraDelay + (stopOvershootDuration * 0.5f) + (stopSettleDuration * 0.5f);
            }
            else
            {
                longestStopTime = ((maxCols - 1) * stagger) + extraDelay + stopOvershootDuration + stopSettleDuration;
            }
    
            yield return new WaitForSeconds(longestStopTime);
    
            AudioManager.Instance?.StopTensionBuilder();
    
            isSpinning = false;
    
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
    
            
    
            onComplete?.Invoke();
        }
    
        private IEnumerator StopSingleReel(int columnIndex, List<int> targetSymbols, float delay, bool isQuickStop, bool isTensionSpin = false, float currentStagger = 0.2f)
        {
            if (isTensionSpin)
            {
                float frameEnableDelay = (columnIndex > 0) ? (columnIndex - 1) * currentStagger : 0f;
                if (delay > frameEnableDelay)
                {
                    if (frameEnableDelay > 0f)
                    {
                        yield return new WaitForSeconds(frameEnableDelay);
                    }
                    
                    AudioManager.Instance?.PlayTensionBuilder();
                    yield return new WaitForSeconds(delay - frameEnableDelay);
                }
                else
                {
                    
                    AudioManager.Instance?.PlayTensionBuilder();
                    if (delay > 0)
                    {
                        yield return new WaitForSeconds(delay);
                    }
                }
            }
            else if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
    
            if (columnIndex < spinTweens.Count && spinTweens[columnIndex] != null)
            {
                spinTweens[columnIndex].Kill();
            }
    
            Transform slotTransform = reelTransforms[columnIndex];
            slotTransform.DOKill();
    
            float targetY = GetTargetYForResult(targetSymbols);
    
            SetReelSymbols(columnIndex, targetSymbols, false);
    
            bool isCase1 = targetSymbols != null && targetSymbols.Count >= 3 && targetSymbols[1] != 0;
            if (isCase1)
            {
                if (columnIndex < reelCurveIntensity.Length)
                {
                    if (reelSettleCurveTweens[columnIndex] != null) reelSettleCurveTweens[columnIndex].Kill();
                    reelCurveIntensity[columnIndex] = 1f;
                }
            }
            else
            {
                if (columnIndex < reelCurveIntensity.Length)
                {
                    if (reelSettleCurveTweens[columnIndex] != null) reelSettleCurveTweens[columnIndex].Kill();
                    float settleDuration = isQuickStop ? (quickStopDuration * 0.7f) : stopSettleDuration;
                    int colIdx = columnIndex;
                    reelSettleCurveTweens[colIdx] = DOVirtual.Float(reelCurveIntensity[colIdx], 0f, settleDuration, (val) =>
                    {
                        if (colIdx < reelCurveIntensity.Length) reelCurveIntensity[colIdx] = val;
                    });
                }
                UpdateCylindricalSpinEffect(force: false);
            
        }
    
            float landingStartTopY = targetY + (2f * symbolHeight);
            slotTransform.localPosition = new Vector3(
                slotTransform.localPosition.x,
                landingStartTopY,
                0
            );
    
            AudioManager.Instance?.PlayReelStop();
    
            if (currentDisplayMatrix != null && columnIndex < currentDisplayMatrix.Count)
            {
                bool hasWild = false;
                int wildId = gameManager?.gameConfig != null ? gameManager.gameConfig.wildSymbolId : 10;
                foreach (int sym in currentDisplayMatrix[columnIndex])
                {
                    if (sym == wildId) hasWild = true;
                }
                if (hasWild) AudioManager.Instance?.PlayWildChilliHit();
            }
    
            Sequence stopSequence = DOTween.Sequence();
            float overshoot = isQuickStop ? 14f : 30f;
            float dropDuration = isQuickStop ? 0.12f : 0.16f;
            float recoilDuration = isQuickStop ? 0.10f : 0.18f;

            // Fast smooth drop through overshoot
            stopSequence.Append(
                slotTransform.DOLocalMoveY(targetY - overshoot, dropDuration)
                    .SetEase(Ease.OutQuad)
            );

            // Crisp mechanical recoil into final target position
            stopSequence.Append(
                slotTransform.DOLocalMoveY(targetY, recoilDuration)
                    .SetEase(Ease.OutBack, 1.15f)
            );

            stopSequence.OnUpdate(() =>
            {
                UpdateCylindricalSpinEffect(force: false);
            });

            stopSequence.OnComplete(() =>
            {
                UpdateCylindricalSpinEffect(force: true);
            });

            if (spinTweens.Count <= columnIndex)
                spinTweens.Add(stopSequence);
            else
                spinTweens[columnIndex] = stopSequence;

        }
    
        #endregion
    
    #region Quick Spin
    
        public void QuickStop(List<List<int>> resultMatrix, System.Action onComplete = null)
        {
            if (!isSpinning)
            {
                currentDisplayMatrix = resultMatrix;
                int reelCount = resultMatrix != null ? resultMatrix.Count : 3;
                int maxCols = Mathf.Min(reelCount, reelTransforms != null ? reelTransforms.Length : 3);
    
                for (int col = 0; col < maxCols; col++)
                {
                    if (col < reelTransforms.Length)
                    {
                        SetReelSymbols(col, resultMatrix[col], false);
                        float targetY = GetTargetYForResult(resultMatrix[col]);
                        reelTransforms[col].localPosition = new Vector3(
                            reelTransforms[col].localPosition.x,
                            targetY,
                            0
                        );
                    }
                }
    
                onComplete?.Invoke();
                return;
            }
    
            StartCoroutine(StopSpinSequence(resultMatrix, onComplete, true));
        }
    
        #endregion
        #region 3D Cylindrical Drum Curvature

    private void LateUpdate()
    {
        if (isSpinning)
        {
            UpdateCylindricalSpinEffect(force: false);
        }
    }

    public void UpdateCylindricalSpinEffect(bool force = false)
    {
        if (!enableCylindricalEffect || reelTransforms == null || reelImagesList == null) return;

        int maxCols = Mathf.Min(reelTransforms.Length, reelImagesList.Count);

        float effectiveVisibleHalfHeight = visibleHalfHeight;
        if (visibleAreaRectTransform != null && visibleAreaRectTransform.rect.height > 0)
        {
            effectiveVisibleHalfHeight = visibleAreaRectTransform.rect.height * 0.5f;
        }
        float effectiveOuterHalfHeight = Mathf.Max(outerHalfHeight, effectiveVisibleHalfHeight * 1.5f);

        float invVisibleHalfHeight = 1f / Mathf.Max(1f, effectiveVisibleHalfHeight);
        float invOuterRange = 1f / Mathf.Max(1f, effectiveOuterHalfHeight - effectiveVisibleHalfHeight);

        for (int col = 0; col < maxCols; col++)
        {
            Transform slotTransform = reelTransforms[col];
            if (slotTransform == null) continue;

            var reel = reelImagesList[col];
            if (reel == null || reel.images == null) continue;

            float intensity = (col < reelCurveIntensity.Length) ? reelCurveIntensity[col] : 1f;

            float centerImageLocalY = (reel.images.Count > 7 && reel.images[7] != null) ? reel.images[7].rectTransform.localPosition.y : -305.5f;
            float slotOffsetFromCase1 = slotTransform.localPosition.y - case1StopY;

            int imgCount = reel.images.Count;
            for (int i = 0; i < imgCount; i++)
            {
                Image img = reel.images[i];
                if (img == null) continue;

                RectTransform rect = img.rectTransform;
                if (rect == null) continue;

                float yRel = (rect.localPosition.y - centerImageLocalY) + slotOffsetFromCase1;
                float absY = Mathf.Abs(yRel);

                // Skip offscreen symbols
                if (absY > effectiveOuterHalfHeight && !force)
                {
                    continue;
                }

                float targetX = 0f;
                float targetScale = 1f;
                float barrelT = (absY <= effectiveVisibleHalfHeight) ? (absY * invVisibleHalfHeight) : 1f;

                if (absY <= effectiveVisibleHalfHeight)
                {
                    float curveFactor = barrelT * barrelT * intensity;

                    if (col == 0)
                    {
                        targetX = Mathf.Lerp(0f, leftReelEdgeX, curveFactor);
                    }
                    else if (col == maxCols - 1)
                    {
                        targetX = Mathf.Lerp(0f, rightReelEdgeX, curveFactor);
                    }

                    targetScale = Mathf.Lerp(1f, edgeScale, curveFactor);
                }
                else
                {
                    float extraT = Mathf.Clamp01((absY - effectiveVisibleHalfHeight) * invOuterRange);

                    if (col == 0)
                    {
                        targetX = Mathf.Lerp(leftReelEdgeX, leftReelOuterX, extraT) * intensity;
                    }
                    else if (col == maxCols - 1)
                    {
                        targetX = Mathf.Lerp(rightReelEdgeX, rightReelOuterX, extraT) * intensity;
                    }

                    targetScale = Mathf.Lerp(1f, Mathf.Lerp(edgeScale, outerScale, extraT), intensity);
                }

                // 3D Cylinder Tilt & Squash with smooth intensity scaling
                float squashY = Mathf.Lerp(1f, Mathf.Lerp(1f, minSquashY, intensity), barrelT * barrelT);
                float tiltX = (Mathf.Lerp(0f, maxTiltAngle, barrelT) * intensity) * -Mathf.Sign(yRel);

                // Apply 3D tilt rotation
                rect.localRotation = Quaternion.Euler(tiltX, 0f, 0f);

                // Apply horizontal fish-eye bow X
                Vector2 anchoredPos = rect.anchoredPosition;
                if (force || !Mathf.Approximately(anchoredPos.x, targetX))
                {
                    rect.anchoredPosition = new Vector2(targetX, anchoredPos.y);
                }

                // Apply 3D squashed & curved scale
                Vector3 targetLocalScale = new Vector3(targetScale, squashY * targetScale, targetScale);
                if (force || rect.localScale != targetLocalScale)
                {
                    rect.localScale = targetLocalScale;
                }
            }
        }
    }

    public void ResetCylindricalTransforms()
    {
        if (reelImagesList == null) return;
        foreach (var reel in reelImagesList)
        {
            if (reel?.images == null) continue;
            foreach (var img in reel.images)
            {
                if (img == null) continue;
                img.rectTransform.localRotation = Quaternion.identity;
                img.rectTransform.localScale = Vector3.one;
                Vector2 pos = img.rectTransform.anchoredPosition;
                if (pos.x != 0f)
                {
                    img.rectTransform.anchoredPosition = new Vector2(0f, pos.y);
                }
            }
        }
    }

    #endregion
}