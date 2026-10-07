using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public partial class SlotView
{
    #region Spin Animation
    
        internal void StartSpin()
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
    
            if (cylindricalEffect != null) cylindricalEffect.StartEffect();
    
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
    
        internal void StopSpin(List<List<int>> resultMatrix, System.Action onComplete, bool isTurbo = false)
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
    
            bool isWheelTriggered = gameManager != null &&
                                   gameManager.lastResult != null &&
                                   gameManager.lastResult.dualWheelsBonusData != null &&
                                   gameManager.lastResult.dualWheelsBonusData.isTriggered;
    
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
    
            if (lastSlotTensionFrame != null)
            {
                lastSlotTensionFrame.SetActive(false);
            }
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
    
            if (cylindricalEffect != null)
            {
                cylindricalEffect.StopEffect();
                
            }
    
            
    
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
                    if (lastSlotTensionFrame != null) lastSlotTensionFrame.SetActive(true);
                    AudioManager.Instance?.PlayTensionBuilder();
                    yield return new WaitForSeconds(delay - frameEnableDelay);
                }
                else
                {
                    if (lastSlotTensionFrame != null) lastSlotTensionFrame.SetActive(true);
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
                if (cylindricalEffect != null) cylindricalEffect.StartEffect();
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
                if (hasWild) AudioManager.Instance?.PlayReelStop();
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
                if (cylindricalEffect != null)
                {
                    cylindricalEffect.UpdateCylindricalSpinEffect(force: false);
                }
            });

            stopSequence.OnComplete(() =>
            {
                if (cylindricalEffect != null)
                {
                    cylindricalEffect.UpdateCylindricalSpinEffect(force: true);
                }
            });

            if (spinTweens.Count <= columnIndex)
                spinTweens.Add(stopSequence);
            else
                spinTweens[columnIndex] = stopSequence;

        }
    
        #endregion
    
    #region Quick Spin
    
        internal void QuickStop(List<List<int>> resultMatrix, System.Action onComplete = null)
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
                        reelTransforms[col].localPosition = new Vector3(
                            reelTransforms[col].localPosition.x,
                            middlePosition,
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
}
