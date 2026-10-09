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

    [Header("Spin Settings")]
    [SerializeField] private float normalSpinDuration = 3.5f;
    [SerializeField] private float turboSpinDuration = 2.0f;
    [SerializeField] private float quickSpinCycleDuration = 0.8f;

    [SerializeField] private double WinThreshold = 5.0;

    public GameConfig gameConfig { get; private set; }
    public PlayerData playerData { get; private set; }
    public SpinResult lastResult { get; private set; }

    public GameState currentState { get; private set; }
    public SpinSpeed currentSpinSpeed { get; private set; }

    public int currentBetIndex { get; private set; }
    public double currentBetAmount { get; private set; }

    public bool isAutoPlaying { get; private set; }
    public int autoPlayTotalRounds { get; private set; }
    public int autoPlayRemainingRounds { get; private set; }

    public bool isInitialized { get; private set; }
    public bool initializationFailed { get; private set; }

    private Coroutine spinCoroutine;
    private bool stopRequested;
    private bool waitingForSpecialWin;
    private bool isRespinActive = false;
    public bool IsRespinActive => isRespinActive;

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

    public void SetInitializationFailed(bool failed = true)
    {
        initializationFailed = failed;
    }

    public void OnInitDataReceived(GameConfig config, PlayerData player, List<List<int>> initialMatrix)
    {
      
        gameConfig = config;
        playerData = player;
        currentBetIndex = playerData.currentBetIndex;
        UpdateBetAmount();

        if (initialMatrix != null && slotView != null)
        {
            slotView.SetInitialMatrix(initialMatrix);
        }


        isInitialized = true;
        currentState = GameState.Idle;

        uiManager.OnGameInitialized();
    }

    #endregion

    #region Bet Management

    public void IncreaseBet()
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

    public void DecreaseBet()
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

    public void SetBetIndex(int index)
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
    
    public void RequestSpin()
    {
        if (currentState != GameState.Idle) return;
        if (!socketManager.isConnected) return;

        // [BALANCE CHECK #1] Verify player has enough funds before initiating spin
        double totalPay = GetTotalPay();
        if (playerData == null || playerData.balance < totalPay)
        {
            if (isAutoPlaying) StopAutoPlay();
            if (popupManager != null)
            {
                popupManager.ShowInsufficientFundsError();
            }
            return; // STOP: Do not spin if insufficient balance
        }

        StartSpin();
    }

    public void RequestStop()
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

    private void StartRespin()
    {
        currentState = GameState.Spinning;
        stopRequested = false;

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

    private void StartSpin()
    {
        // [BALANCE CHECK #2] Secondary safety guard: prevent spin if balance is below bet amount
        double totalPay = GetTotalPay();
        if (playerData == null || playerData.balance < totalPay)
        {
            if (isAutoPlaying) StopAutoPlay();
            if (popupManager != null) popupManager.ShowInsufficientFundsError();
            currentState = GameState.Idle;
            return; // STOP: Do not execute reel spin or server call without money
        }

        currentState = GameState.Spinning;
        stopRequested = false;

        // [BALANCE DEDUCTION] Deduct spin bet from player balance
        playerData.balance -= totalPay;
        if (playerData.balance < 0) playerData.balance = 0;

        // [AUTOPLAY ROUND DECREMENT] Decrement round counter exactly once when spin starts
        if (isAutoPlaying && autoPlayTotalRounds != -1)
        {
            autoPlayRemainingRounds--;
            // autoplay log silenced

            if (uiManager != null) uiManager.UpdateAutoPlayCount();
        }

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

        bool isFeatureTriggered = false;

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

        AudioManager.Instance?.PlayBigWin();
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

    private bool CheckForCenterWild(List<List<int>> matrix)
    {
        if (matrix == null || matrix.Count < 3) return false;
        var centerCol = matrix[1];
        if (centerCol == null || centerCol.Count == 0) return false;
        int paylineRow = centerCol.Count >= 3 ? 1 : 0;
        if (paylineRow >= centerCol.Count) return false;
        int centerSymbolId = centerCol[paylineRow];
        return centerSymbolId >= 0 && centerSymbolId <= 3;
    }

    private IEnumerator ProcessSpecialFeaturesAfterWin()
    {
        while (waitingForSpecialWin || (uiManager != null && uiManager.IsSpecialWinActive))
        {
            yield return null;
        }

        if (isRespinActive)
        {
            isRespinActive = false;
            if (slotView != null) slotView.SetCenterWildLocked(false);
        }

        ResumeAfterSpecialFeature();
    }

    private IEnumerator ExecuteCenterWildRespinRoutine()
    {
        isRespinActive = true;
        if (slotView != null)
        {
            int row = (lastResult?.resultMatrix != null && lastResult.resultMatrix.Count > 1 && lastResult.resultMatrix[1].Count >= 3) ? 1 : 0;
            int centerId = (lastResult?.resultMatrix != null && lastResult.resultMatrix.Count > 1 && row < lastResult.resultMatrix[1].Count) ? lastResult.resultMatrix[1][row] : 0;
            slotView.SetCenterWildLocked(true, centerId);
        }

        yield return new WaitForSeconds(0.6f);

        StartRespin();
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

    public void OnSpinResultReceived(SpinResult result)
    {
        lastResult = result;
        // Debug.Log(result);
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
        if (lastResult != null && lastResult.playerData != null)
        {
            playerData = lastResult.playerData;
        }

        if (uiManager != null)
        {
            uiManager.OnSpinCompleted(lastResult);
        }

        lastResult = null;

        // [AUTOPLAY CONTINUATION LOGIC]
        if (isAutoPlaying)
        {
            // 1. Target round count reached (e.g. 10 of 10 completed) -> STOP immediately
            if (autoPlayTotalRounds != -1 && autoPlayRemainingRounds <= 0)
            {
                // autoplay log silenced
                currentState = GameState.Idle;
                StopAutoPlay();
                return;
            }

            // 2. Insufficient balance for next round -> STOP immediately
            double totalPay = GetTotalPay();
            if (playerData == null || playerData.balance < totalPay)
            {
                currentState = GameState.Idle;
                StopAutoPlay();
                if (popupManager != null) popupManager.ShowInsufficientFundsError();
                return;
            }

            // 3. Proceed to next round
            currentState = GameState.Idle;
            RequestSpin();
        }
        else
        {
            currentState = GameState.Idle;
        }
    }

    #endregion

    #region Spin Speed Control

    public void SetSpinSpeed(SpinSpeed speed)
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

    public void StartAutoPlay(int rounds)
    {
        if (currentState != GameState.Idle) return;
        // autoplay log silenced
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

    public void StopAutoPlay()
    {
        isAutoPlaying = false;
        autoPlayRemainingRounds = 0;

        uiManager.OnAutoPlayStopped();
    }

    #endregion



    #region Connection Events

    public void OnDisconnected()
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

    public void ExitGame()
    {
        socketManager.CloseSocket();

    }

    #endregion

    #region Helper Methods

        public double GetTotalPay()
    {
        return currentBetAmount;
    }

    public bool IsSpinning()
    {
        return currentState == GameState.Spinning || currentState == GameState.Stopping;
    }

    #endregion
}
