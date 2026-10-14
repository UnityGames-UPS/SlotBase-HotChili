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

    internal GameConfig gameConfig { get; private set; }
    internal PlayerData playerData { get; private set; }
    internal SpinResult lastResult { get; private set; }

    internal GameState currentState { get; private set; }
    internal SpinSpeed currentSpinSpeed { get; private set; }

    internal int currentBetIndex { get; private set; }
    internal double currentBetAmount { get; private set; }

    internal bool isAutoPlaying { get; private set; }
    internal int autoPlayTotalRounds { get; private set; }
    internal int autoPlayRemainingRounds { get; private set; }

    internal bool isInitialized { get; private set; }
    internal bool initializationFailed { get; private set; }

    private Coroutine spinCoroutine;
    private bool stopRequested;
    private bool waitingForSpecialWin;
    private bool isRespinActive = false;
    internal bool IsRespinActive => isRespinActive;
    private bool isRespinPending = false;
    internal bool IsRespinPending => isRespinPending;

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

    internal void SetInitializationFailed(bool failed = true)
    {
        initializationFailed = failed;
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

    private void SetBetIndex(int index)
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
        if (isRespinPending)
        {
            ExecuteRespin();
            return;
        }

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

    /// <summary>Starts the respin feature: keeps Reel 1 locked, spins outer reels (0 & 2), and resolves the respinMatrix.</summary>
    internal void ExecuteRespin()
    {
        if (!isRespinPending || lastResult == null) return;

        isRespinPending = false;
        isRespinActive = true;
        currentState = GameState.Spinning;
        stopRequested = false;

        uiManager.OnSpinStarted();

        if (slotView != null)
        {
            slotView.StartSpin();
        }

        if (spinCoroutine != null)
            StopCoroutine(spinCoroutine);
        spinCoroutine = StartCoroutine(RespinRoutine());
    }

    /// <summary>Handles spin duration timing and reel stopping for the respin round.</summary>
    private IEnumerator RespinRoutine()
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

        currentState = GameState.Stopping;

        if (slotView != null && lastResult.respinMatrix != null)
        {
            if (currentSpinSpeed == SpinSpeed.QuickSpin || stopRequested)
            {
                slotView.QuickStop(lastResult.respinMatrix, OnRespinReelsStoppedComplete);
            }
            else if (currentSpinSpeed == SpinSpeed.Turbo)
            {
                slotView.StopSpin(lastResult.respinMatrix, OnRespinReelsStoppedComplete, isTurbo: true);
            }
            else
            {
                slotView.StopSpin(lastResult.respinMatrix, OnRespinReelsStoppedComplete, isTurbo: false);
            }
        }
        else
        {
            OnRespinReelsStoppedComplete();
        }
    }

    /// <summary>Callback triggered when respin reels stop: unlocks Reel 1, calculates respin win, and plays win animations.</summary>
    private void OnRespinReelsStoppedComplete()
    {
        isRespinActive = false;
        if (slotView != null) slotView.SetCenterWildLocked(false);
        if (uiManager != null) uiManager.SetRespinButtonState(false);

        double bet = currentBetAmount > 0 ? currentBetAmount : 0.01;
        double winVal = lastResult != null ? lastResult.respinWinAmount : 0;
        double multiplier = bet > 0 ? (winVal / bet) : 0;

        if (lastResult != null && winVal > 0)
        {
            if (playerData != null)
            {
                playerData.balance += winVal;
            }
            if (uiManager != null)
            {
                uiManager.UpdateBalanceDisplay();
                uiManager.UpdateWinDisplay(winVal);
            }

            if (multiplier >= WinThreshold)
            {
                uiManager.DisableControlsDuringWinAnimation();
                currentState = GameState.Idle;
                waitingForSpecialWin = true;
                StartCoroutine(TriggerWinPopupWithDelay(0.3f, lastResult));
                if (lastResult.respinLineWins != null && lastResult.respinLineWins.Count > 0 && slotView != null)
                {
                    slotView.ShowWinLineAnimation(lastResult.respinLineWins, OnRespinWinAnimationComplete);
                }
                else
                {
                    OnRespinWinAnimationComplete();
                }
            }
            else
            {
                uiManager.EnableControlsAfterWinAnimation();
                currentState = GameState.Idle;
                if (lastResult.respinLineWins != null && lastResult.respinLineWins.Count > 0 && slotView != null)
                {
                    slotView.ShowWinLineAnimation(lastResult.respinLineWins, OnRespinWinAnimationComplete);
                }
                else
                {
                    OnRespinWinAnimationComplete();
                }
            }
        }
        else
        {
            currentState = GameState.Idle;
            OnRespinWinAnimationComplete();
        }
    }

    /// <summary>Callback triggered when respin win animations finish: resumes gameplay or next autoplay round.</summary>
    private void OnRespinWinAnimationComplete()
    {
        ResumeAfterSpecialFeature();
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

    /// <summary>Checks for triggered features after win animations: detects respinTriggered and initiates respin pending mode.</summary>
    private IEnumerator ProcessSpecialFeaturesAfterWin()
    {
        while (waitingForSpecialWin || (uiManager != null && uiManager.IsSpecialWinActive))
        {
            yield return null;
        }

        if (lastResult != null && lastResult.isRespinTriggered && !isRespinActive)
        {
            isRespinPending = true;
            if (slotView != null) slotView.SetCenterWildLocked(true);
            if (uiManager != null) uiManager.SetRespinButtonState(true);
            AudioManager.Instance?.PlayWildChilliHit();

            // Automatic anticipation delay: 1.5s delay before auto-spinning (player can also tap early)
            float delay = 2f;
            float timer = 0f;
            while (timer < delay && isRespinPending)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (isRespinPending)
            {
                ExecuteRespin();
            }
            yield break;
        }

        if (isRespinActive)
        {
            isRespinActive = false;
            if (slotView != null) slotView.SetCenterWildLocked(false);
            if (uiManager != null) uiManager.SetRespinButtonState(false);
        }

        ResumeAfterSpecialFeature();
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
        return currentBetAmount;
    }

    internal bool IsSpinning()
    {
        return currentState == GameState.Spinning || currentState == GameState.Stopping;
    }

    #endregion
}
