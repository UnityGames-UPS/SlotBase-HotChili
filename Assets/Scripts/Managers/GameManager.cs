using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] internal SocketIOManager socketManager;
    [SerializeField] internal UIManager uiManager;
    [SerializeField] private PopupManager popupManager;
    [SerializeField] private SlotView slotView;
    [Header("Dual Wheel Controllers")]
    [SerializeField] internal WheelSpinController redWheel;
    [SerializeField] internal WheelSpinController greenWheel;

    [Header("Spin Settings")]
    [SerializeField] private float normalSpinDuration = 3.5f;
    [SerializeField] private float turboSpinDuration = 2.0f;
    [SerializeField] private float quickSpinCycleDuration = 0.8f;

    [SerializeField] private double WinThreshold = 5.0;

    internal GameConfig gameConfig;
    internal PlayerData playerData;
    internal SpinResult lastResult;

    internal GameState currentState;
    internal SpinSpeed currentSpinSpeed;

    internal int currentBetIndex;
    internal double currentBetAmount;

    internal bool isAutoPlaying;
    internal int autoPlayTotalRounds;
    internal int autoPlayRemainingRounds;

    internal bool isInitialized;
    internal bool initializationFailed;

    private Coroutine spinCoroutine;
    private bool stopRequested;
    private bool waitingForSpecialWin;

    #region Initialization

    private void Start()
    {
        if (uiManager != null)
        {
            WinThreshold = uiManager.BigWinThreshold;
        }
        currentState = GameState.Initializing;
        currentSpinSpeed = SpinSpeed.Normal;
        isInitialized = false;
        initializationFailed = false;
    }

    internal void OnInitDataReceived(GameConfig config, PlayerData player, List<List<int>> initialMatrix)
    {
      
        gameConfig = config;
        playerData = player;
        currentBetIndex = playerData.currentBetIndex;
        UpdateBetAmount();

        if (initialMatrix != null && slotView != null)
        {
            slotView.SetInitialMatrix(initialMatrix);
        }

        if (uiManager != null && gameConfig != null && gameConfig.dualWheels != null)
        {
            uiManager.SetupDualWheels(gameConfig.dualWheels);
        }

        isInitialized = true;
        currentState = GameState.Idle;

        uiManager.OnGameInitialized();
    }

    #endregion

    #region Bet Management

    internal void IncreaseBet()
    {
        if (currentState != GameState.Idle || isAutoPlaying) return;
        if (gameConfig == null || gameConfig.availableBets == null || gameConfig.availableBets.Count == 0) return;

        int maxIndex = gameConfig.availableBets.Count - 1;
        int nextIndex = currentBetIndex + 1;
        if (nextIndex > maxIndex)
        {
            nextIndex = 0;
        }

        if (nextIndex == maxIndex)
        {
            AudioManager.Instance?.PlayMaxBetReached();
        }
        else
        {
            AudioManager.Instance?.PlayBetPlusMinus();
        }

        SetBetIndex(nextIndex);
    }

    internal void DecreaseBet()
    {
        if (currentState != GameState.Idle || isAutoPlaying) return;
        if (gameConfig == null || gameConfig.availableBets == null || gameConfig.availableBets.Count == 0) return;

        int maxIndex = gameConfig.availableBets.Count - 1;
        int nextIndex = currentBetIndex - 1;
        if (nextIndex < 0)
        {
            nextIndex = maxIndex;
        }

        if (nextIndex == maxIndex)
        {
            AudioManager.Instance?.PlayMaxBetReached();
        }
        else
        {
            AudioManager.Instance?.PlayBetPlusMinus();
        }

        SetBetIndex(nextIndex);
    }

    internal void SetBetIndex(int index)
    {
        currentBetIndex = index;
        UpdateBetAmount();
        uiManager.UpdateBetDisplay();
        if (slotView != null) slotView.OnBetChanged();
    }

    private void UpdateBetAmount()
    {
        currentBetAmount = gameConfig.availableBets[currentBetIndex];
    }

    #endregion

    #region Spin Control
    
    internal void RequestSpin()
    {
        if (currentState != GameState.Idle) return;
        if (!socketManager.isConnected) return;

        double totalPay = GetTotalPay();
        if (playerData.balance < totalPay)
        {
            if (popupManager != null)
            {
                popupManager.ShowInsufficientFundsError();
            }
            return;
        }

        StartSpin();
    }

    internal void RequestStop()
    {
        if (currentState == GameState.Spinning)
        {
            if (isAutoPlaying)
            {
                StopAutoPlay();
            }
            else
            {
                stopRequested = true;
                uiManager.DisableSpinButtonDuringStop();
            }
        }
    }

    private void StartSpin()
    {
        if (lastResult != null)
        {
            ProcessSpinResult();
        }

        lastResult = null;
        currentState = GameState.Spinning;
        stopRequested = false;

        playerData.balance -= GetTotalPay();
        if (playerData.balance < 0) playerData.balance = 0;

        uiManager.OnSpinStarted();

        if (slotView != null)
        {
            slotView.StartSpin();
        }

        socketManager.SendSpinRequest(currentBetIndex);

        if (spinCoroutine != null)
            StopCoroutine(spinCoroutine);
        spinCoroutine = StartCoroutine(SpinRoutine());
    }

    private IEnumerator SpinRoutine()
    {
        float elapsed = 0f;

        while (elapsed < GetSpinDuration() && !stopRequested)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (stopRequested)
        {
            yield return new WaitForSeconds(0.5f);
        }

        while (lastResult == null)
        {
            yield return null;
        }

        currentState = GameState.Stopping;

        if (slotView != null && lastResult.resultMatrix != null)
        {
            if (currentSpinSpeed == SpinSpeed.QuickSpin || stopRequested)
            {
                slotView.QuickStop(lastResult.resultMatrix, OnReelsStoppedComplete);
            }
            else if (currentSpinSpeed == SpinSpeed.Turbo)
            {
                slotView.StopSpin(lastResult.resultMatrix, OnReelsStoppedComplete, isTurbo: true);
            }
            else
            {
                slotView.StopSpin(lastResult.resultMatrix, OnReelsStoppedComplete, isTurbo: false);
            }
        }
        else
        {
            OnReelsStoppedComplete();
        }
    }

    private void OnReelsStoppedComplete()
    {
        if (lastResult != null)
        {
            double featureDeferredWin = lastResult.GetTotalFeatureDeferredWins();
            double reelStopBalance = lastResult.playerData != null ? (lastResult.playerData.balance - featureDeferredWin) : 0;

            playerData = new PlayerData
            {
                balance = reelStopBalance,
                currentBetIndex = lastResult.playerData != null ? lastResult.playerData.currentBetIndex : currentBetIndex
            };
        }

        double bet = currentBetAmount > 0 ? currentBetAmount : 0.01;
        double winVal = lastResult != null ? (lastResult.grandTotalWin > 0 ? lastResult.grandTotalWin : lastResult.winAmount) : 0;
        double multiplier = bet > 0 ? (winVal / bet) : 0;

        bool isFeatureTriggered = lastResult != null && lastResult.dualWheelsBonusData != null && lastResult.dualWheelsBonusData.isTriggered;

        if (lastResult != null && winVal > 0 && !isFeatureTriggered)
        {
            if (multiplier >= WinThreshold)
            {
                uiManager.DisableControlsDuringWinAnimation();
                currentState = GameState.Idle;
                waitingForSpecialWin = true;
                StartCoroutine(TriggerWinPopupWithDelay(0.3f, lastResult));
                if (lastResult.winLines != null && lastResult.winLines.Count > 0 && slotView != null)
                {
                    slotView.ShowWinLineAnimation(lastResult.winLines, OnWinAnimationComplete);
                }
                else
                {
                    OnWinAnimationComplete();
                }
            }
            else
            {
                uiManager.OnSpinStopping(lastResult);
                uiManager.EnableControlsAfterWinAnimation();
                uiManager.OnSpinCompleted(lastResult);
                currentState = GameState.Idle;
                if (lastResult.winLines != null && lastResult.winLines.Count > 0 && slotView != null)
                {
                    slotView.ShowWinLineAnimation(lastResult.winLines, OnWinAnimationComplete);
                }
                else
                {
                    OnWinAnimationComplete();
                }
            }
        }
        else
        {
            uiManager.OnSpinStopping(lastResult);
            currentState = GameState.Idle;
            OnWinAnimationComplete();
        }
    }

    private IEnumerator TriggerWinPopupWithDelay(float delay, SpinResult result)
    {
        if (result == null) yield break;
        double bet = currentBetAmount > 0 ? currentBetAmount : 0.01;
        double winVal = result.grandTotalWin > 0 ? result.grandTotalWin : result.winAmount;
        double multiplier = bet > 0 ? (winVal / bet) : 0;

        if (multiplier < WinThreshold)
        {
            waitingForSpecialWin = false;
            yield break;
        }

        waitingForSpecialWin = true;

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        uiManager.TriggerBigWinPopup(result, () =>
        {
            waitingForSpecialWin = false;
        });
    }

    private void OnWinAnimationComplete()
    {
        if (lastResult != null)
        {
            double bet = currentBetAmount > 0 ? currentBetAmount : 0.01;
            double winVal = lastResult.grandTotalWin > 0 ? lastResult.grandTotalWin : lastResult.winAmount;
            double multiplier = bet > 0 ? (winVal / bet) : 0;

            if (multiplier >= WinThreshold)
            {
                uiManager.OnSpinStopping(lastResult);
            }
        }

        StartCoroutine(ProcessSpecialFeaturesAfterWin());
    }

    private IEnumerator ProcessSpecialFeaturesAfterWin()
    {
        while (waitingForSpecialWin || uiManager.IsSpecialWinActive)
        {
            yield return null;
        }

        if (lastResult != null && lastResult.dualWheelsBonusData != null && lastResult.dualWheelsBonusData.isTriggered)
        {
            yield return StartCoroutine(DelayDualWheelsTriggerResult());
            yield break;
        }





        ResumeAfterSpecialFeature();
    }

    private IEnumerator DelayDualWheelsTriggerResult()
    {
        bool animDone = false;
        if (slotView != null)
        {
            slotView.AnimateDualWheelWin(() => { animDone = true; });
            yield return new WaitUntil(() => animDone);
            slotView.DisableAllOverlays();
        }
        else
        {
            yield return new WaitForSeconds(1.0f);
        }

        if (uiManager != null)
        {
            uiManager.TriggerDualWheelsBonus(lastResult.dualWheelsBonusData, () =>
            {
                if (lastResult != null && lastResult.dualWheelsBonusData != null)
                {
                    lastResult.dualWheelsBonusData.isTriggered = false;
                }
                ResumeAfterSpecialFeature();
            });
        }
        else
        {
            ResumeAfterSpecialFeature();
        }
    }

    private void ResumeAfterSpecialFeature()
    {
        if (isAutoPlaying)
        {
            StartCoroutine(DelayBeforeNextRound());
        }
        else
        {
            ProcessSpinResult();
        }
    }







    private IEnumerator DelayBeforeNextRound()
    {
        float delayTime = currentSpinSpeed == SpinSpeed.QuickSpin ? 0.3f : 0.5f;
        yield return new WaitForSeconds(delayTime);

        while (waitingForSpecialWin || uiManager.IsSpecialWinActive)
        {
            yield return null;
        }

        ProcessSpinResult();
    }

    private float GetSpinDuration()
    {
        return currentSpinSpeed switch
        {
            SpinSpeed.Normal => normalSpinDuration,
            SpinSpeed.Turbo => turboSpinDuration,
            SpinSpeed.QuickSpin => quickSpinCycleDuration,
            _ => normalSpinDuration
        };
    }

    internal void OnSpinResultReceived(SpinResult result)
    {
        lastResult = result;

        if (result.winLines != null)
        {
            for (int i = 0; i < result.winLines.Count; i++)
            {
                var line = result.winLines[i];

            }
        }
    }

    private void ProcessSpinResult()
    {
        playerData = lastResult.playerData;

        uiManager.OnSpinCompleted(lastResult);

        lastResult = null;

        if (isAutoPlaying)
        {
            if (autoPlayTotalRounds != -1)
            {
                autoPlayRemainingRounds--;
            }

            uiManager.UpdateAutoPlayCount();

            if (autoPlayTotalRounds != -1 && autoPlayRemainingRounds <= 0)
            {
                currentState = GameState.Idle;
                StopAutoPlay();
            }
            else
            {
                double totalPay = GetTotalPay();
                if (playerData.balance < totalPay)
                {
                    currentState = GameState.Idle;
                    StopAutoPlay();
                    if (popupManager != null) popupManager.ShowInsufficientFundsError();
                }
                else
                {
                    currentState = GameState.Idle;
                    RequestSpin();
                }
            }
        }
        else
        {
            currentState = GameState.Idle;
        }
    }

    #endregion

    #region Spin Speed Control

    internal void SetSpinSpeed(SpinSpeed speed)
    {
        currentSpinSpeed = speed;

        if (currentState == GameState.Stopping && speed == SpinSpeed.QuickSpin)
        {
            if (slotView != null && lastResult != null && lastResult.resultMatrix != null)
            {
                slotView.QuickStop(lastResult.resultMatrix);
            }
        }
    }

    #endregion



    #region Auto Play

    internal void StartAutoPlay(int rounds)
    {
        if (currentState != GameState.Idle) return;

        double totalPay = GetTotalPay();
        if (playerData.balance < totalPay)
        {
            if (popupManager != null) popupManager.ShowInsufficientFundsError();
            return;
        }

        isAutoPlaying = true;
        autoPlayTotalRounds = rounds;
        autoPlayRemainingRounds = rounds;

        uiManager.OnAutoPlayStarted();
        RequestSpin();
    }

    internal void StopAutoPlay()
    {
        isAutoPlaying = false;
        autoPlayRemainingRounds = 0;

        uiManager.OnAutoPlayStopped();
    }

    #endregion



    #region Connection Events

    internal void OnDisconnected()
    {
        if (spinCoroutine != null)
        {
            StopCoroutine(spinCoroutine);
            spinCoroutine = null;
        }

        if (isAutoPlaying)
        {
            StopAutoPlay();
        }

        currentState = GameState.Idle;
    }

    internal void ExitGame()
    {
        socketManager.CloseSocket();

    }

    #endregion

    #region Helper Methods

    internal double GetTotalPay()
    {
        double divisor = (gameConfig != null && gameConfig.paylineCount > 0) ? gameConfig.paylineCount : 25;
        return currentBetAmount * divisor;
    }

    internal bool IsSpinning()
    {
        return currentState == GameState.Spinning || currentState == GameState.Stopping;
    }

    #endregion
}
