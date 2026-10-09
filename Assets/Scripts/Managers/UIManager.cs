using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PopupManager popupManager;
    [SerializeField] private JSFunctCalls jsFunctCalls;
    [SerializeField] private SlotView slotView;
    [SerializeField] private OrientationChange orientationChange;
    [SerializeField] private OCController ocController;

    [Header("Loading & Intro")]
    [SerializeField] private GameObject gameScreen;

    [Header("Bet Controls")]
    [SerializeField] private TMP_Text betAmountText;
    [SerializeField] private Button betPlusButton;
    [SerializeField] private Button betMinusButton;
    [Header("Bet Controls - Portrait")]
    [SerializeField] private TMP_Text betAmountTextPortrait;
    [SerializeField] private Button betPlusButtonPortrait;
    [SerializeField] private Button betMinusButtonPortrait;

    [Header("Balance & Win")]
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text winAmountText;
    [SerializeField] private GameObject winTextObject;
    [SerializeField] private GameObject goodLuckObject;
    [Header("Balance & Win - Portrait")]
    [SerializeField] private TMP_Text balanceTextPortrait;
    [SerializeField] private TMP_Text winAmountTextPortrait;
    [SerializeField] private GameObject winTextObjectPortrait;
    [SerializeField] private GameObject goodLuckObjectPortrait;

    [Header("Top Bar Settings")]
    [SerializeField] private GameObject landscapeTopBarObject;
    [SerializeField] private GameObject portraitTopBarObject;

    [Header("Win Type Popup")]
    [SerializeField] private GameObject winTypePopupObject;
    [SerializeField] private CanvasGroup winTypeCanvasGroup;
    [SerializeField] private RectTransform winTypePopupRect;
    [SerializeField] private TMP_Text winTypeWinText;
    [SerializeField] private GameObject winTypeWinTextContainer;
    [SerializeField] private GameObject bigWinTitleObject;
    [SerializeField] private GameObject megaWinTitleObject;
    [SerializeField] private GameObject legendaryWinTitleObject;
    [SerializeField] private Button winTypeFullScreenButton;

    [Header("Win Type Threshold Settings (Multipliers of Bet)")]
    [SerializeField] private double bigWinThreshold = 5.0;
    [SerializeField] private double megaWinThreshold = 10.0;
    [SerializeField] private double legendaryWinThreshold = 20.0;

    [Header("Win Type Timing Settings")]
    [SerializeField] private float maxCountDuration = 0.4f;
    [SerializeField] private float autoCloseDelay = 0.5f;

    internal double BigWinThreshold => bigWinThreshold;
    internal double MegaWinThreshold => megaWinThreshold;
    internal double LegendaryWinThreshold => legendaryWinThreshold;

    [Header("Spin Button")]
    [SerializeField] private Button spinButton;
    [SerializeField] private Button stopButton;
    [Header("Spin Button - Portrait")]
    [SerializeField] private Button spinButtonPortrait;
    [SerializeField] private Button stopButtonPortrait;

    [Header("Auto Play Stop Control")]
    [SerializeField] private Button autoSpinStopButton;
    [SerializeField] private TMP_Text autoSpinRemainingText;
    [Header("Auto Play Stop Control - Portrait")]
    [SerializeField] private Button autoSpinStopButtonPortrait;
    [SerializeField] private TMP_Text autoSpinRemainingTextPortrait;

    [Header("Auto Play Panel")]
    [SerializeField] private GameObject autoPlayPanel;
    [SerializeField] private RectTransform autoPlayPanelRect;
    [SerializeField] private Button autoPlayCloseButton;
    [Header("Auto Play Selection Buttons")]
    [SerializeField] private Button autoPlay10Button;
    [SerializeField] private Button autoPlay50Button;
    [SerializeField] private Button autoPlay100Button;
    [SerializeField] private Button autoPlay200Button;
    [SerializeField] private Button autoPlay500Button;
    [SerializeField] private Button autoPlayInfiniteButton;

    [Header("Auto Play Panel - Portrait")]
    [SerializeField] private GameObject autoPlayPanelPortrait;
    [SerializeField] private RectTransform autoPlayPanelRectPortrait;
    [SerializeField] private Button autoPlayCloseButtonPortrait;
    [SerializeField] private Button autoPlay10ButtonPortrait;
    [SerializeField] private Button autoPlay50ButtonPortrait;
    [SerializeField] private Button autoPlay100ButtonPortrait;
    [SerializeField] private Button autoPlay200ButtonPortrait;
    [SerializeField] private Button autoPlay500ButtonPortrait;
    [SerializeField] private Button autoPlayInfiniteButtonPortrait;

    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private RectTransform settingsPanelRect;
    [SerializeField] private Button settingsOpenButton;
    [SerializeField] private Button settingsCloseButton;
    [SerializeField] private Button settingsBgCloseButton;
    [SerializeField] private Button gameQuitButton;
    [Header("Settings Panel - Portrait")]
    [SerializeField] private GameObject settingsPanelPortrait;
    [SerializeField] private RectTransform settingsPanelRectPortrait;
    [SerializeField] private Button settingsOpenButtonPortrait;
    [SerializeField] private Button settingsCloseButtonPortrait;
    [SerializeField] private Button settingsBgCloseButtonPortrait;
    [SerializeField] private Button gameQuitButtonPortrait;

    [Header("Speed Buttons (Three-Layer Toggle)")]
    [SerializeField] private Button normalSpeedButton;
    [SerializeField] private Button turboSpeedButton;
    [SerializeField] private Button quickSpeedButton;
    [Header("Speed Buttons - Portrait")]
    [SerializeField] private Button normalSpeedButtonPortrait;
    [SerializeField] private Button turboSpeedButtonPortrait;
    [SerializeField] private Button quickSpeedButtonPortrait;

    [Header("Sound Panel")]
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private RectTransform soundPanelRect;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Button soundPanelCloseButton;
    [SerializeField] private Button soundPanelOpenButton;
    [SerializeField] private Button soundPanelOpenButtonPortrait;

    [Header("Game Rules Panel")]
    [SerializeField] private GameObject gameRulesPanel;
    [SerializeField] private RectTransform gameRulesPanelRect;
    [SerializeField] private Button gameRulesOpenButton;
    [SerializeField] private Button gameRulesBackButton;
    [Header("Game Rules Panel - Portrait")]
    [SerializeField] private Button gameRulesOpenButtonPortrait;

    [Header("Guide Panel")]
    [SerializeField] private GameObject guidePanel;
    [SerializeField] private RectTransform guidePanelRect;
    [SerializeField] private Button guideOpenButton;
    [SerializeField] private Button guideBackButton;
    [Header("Guide Panel - Portrait")]
    [SerializeField] private Button guideOpenButtonPortrait;

    [Header("Game Rules Dynamic Texts - 12 Payout Rows")]
    [SerializeField] private TMP_Text totalLineCountText;
    [SerializeField] private TMP_Text ruleGreenRedChilliText;    // 1000
    [SerializeField] private TMP_Text ruleGreenOrangeChilliText; // 400
    [SerializeField] private TMP_Text ruleGreenYellowChilliText; // 300
    [SerializeField] private TMP_Text ruleAllGreenChilliText;    // 200
    [SerializeField] private TMP_Text ruleChilliMultiplierText;  // MULTIPLIER
    [SerializeField] private TMP_Text ruleAllRedSevenText;       // 25
    [SerializeField] private TMP_Text ruleAllBlueSevenText;      // 15
    [SerializeField] private TMP_Text ruleMixedSevensText;       // 10
    [SerializeField] private TMP_Text ruleAllTripleBarsText;     // 10
    [SerializeField] private TMP_Text ruleAllDoubleBarsText;     // 5
    [SerializeField] private TMP_Text ruleAnyBarsSevenText;      // 3
    [SerializeField] private TMP_Text ruleMixedBarsSevensText;   // 2


    [Header("Ping Display")]
    [SerializeField] private TMP_Text pingText;
    [SerializeField] private TMP_Text pingTextPortrait;

    [Header("Platform Jackpot")]
    [SerializeField] private TMP_Text grandJackpotText;
    [SerializeField] private TMP_Text majorJackpotText;
    [SerializeField] private TMP_Text minorJackpotText;
    [SerializeField] private TMP_Text miniJackpotText;

    [Header("Platform Jackpot - Portrait")]
    [SerializeField] private TMP_Text grandJackpotTextPortrait;
    [SerializeField] private TMP_Text majorJackpotTextPortrait;
    [SerializeField] private TMP_Text minorJackpotTextPortrait;
    [SerializeField] private TMP_Text miniJackpotTextPortrait;

    [Header("Platform Jackpot Animation - Portrait")]
    [SerializeField] private RectTransform grandJackpotPortraitParent;
    [SerializeField] private RectTransform majorJackpotPortraitParent;
    [SerializeField] private RectTransform minorJackpotPortraitParent;
    [SerializeField] private RectTransform miniJackpotPortraitParent;
    [SerializeField] private bool enableJackpotPortraitLevitation = true;
    [SerializeField] private float jackpotLevitateHeight = 10f;
    [SerializeField] private float jackpotLevitateDuration = 1.4f;
    [SerializeField] private float jackpotStaggerDelay = 0.15f;

    private readonly Dictionary<Transform, Vector3> jackpotInitialLocalPositions = new Dictionary<Transform, Vector3>();
    private readonly List<Tween> jackpotPortraitTweens = new List<Tween>();

    [Header("Expand-Shrink Controls")]
    [SerializeField] private Button expandButton;
    [SerializeField] private Button shrinkButton;
    [Header("Expand-Shrink Controls - Portrait")]
    [SerializeField] private Button expandButtonPortrait;
    [SerializeField] private Button shrinkButtonPortrait;

    private bool isExpanded = false;
    private bool isSettingsPanelOpen = false;

    private Tween balanceTween;
    private Tween winTween;

    [SerializeField] private float rapidStopCooldown = 1f;
    private float lastRapidStopTime = -99f;

    [Header("UI State")]
    private double currentWinDisplayValue = 0;
    private bool isSpecialWinActive = false;
    public bool IsSpecialWinActive => isSpecialWinActive;
    public System.Action OnSpecialWinComplete;


    private Coroutine winTypeCountCoroutine;
    private Coroutine winTypeAutoCloseCoroutine;
    private bool isWinTypeCounting;
    private double finalWinTypeAmount;
    private double currentWinTypeCount;
    private double winTypeTotalBet;
    private int activeWinTypePhase;
    private System.Action onWinTypeCompleteCallback;



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HandleSpacebarPressed();
        }
    }

    private void HandleSpacebarPressed()
    {
        if (gameManager == null) return;
        if (gameManager.isAutoPlaying) return;

        if (gameManager.currentState == GameState.Idle)
        {
            //AudioManager.Instance?.PlaySpinButton();
            gameManager.RequestSpin();
        }
        else if (gameManager.currentState == GameState.Spinning)
        {
            //AudioManager.Instance?.PlayStopButton();
            gameManager.RequestStop();
        }
    }

    private void Awake()
    {
        if (jsFunctCalls != null)
        {
            jsFunctCalls.RegisterVisibilityListener(gameObject.name);
        }
    }



    public void OnFocusChanged(string value)
    {
        bool focused = value == "1";
        AudioManager.Instance?.SetMuteAll(!focused);
        if (gameManager != null && gameManager.socketManager != null)
        {
            gameManager.socketManager.HandleFocusChange(focused);
        }
    }

    private void Start()
    {
        SetupButtons();
        SetupAutoPlayPanel();
        SetupSettingsPanel();
        SetupGameRulesPanel();
        SetupGuidePanel();

        InitializeExpandShrink();

        if (gameScreen) gameScreen.SetActive(true);
        InitializeUI();
        StartCoroutine(WaitForInitialization());
        RegisterFullscreenListener();
        UpdateJackpotPortraitLevitationFromCurrentOrientation();
    }

    private void OnEnable()
    {
        var oc = GetOrientationChange();
        if (oc != null)
        {
        }
    }

    private void OnDisable()
    {
        if (orientationChange != null)
        {
        }
        StopJackpotPortraitLevitation();
    }

    private void InitializeUI()
    {
        if (soundPanel) soundPanel.SetActive(false);
        SetGameObjectActive(autoPlayPanel, autoPlayPanelPortrait, false);
        if (autoPlayPanelRect) autoPlayPanelRect.anchoredPosition = new Vector2(autoPlayPanelRect.anchoredPosition.x, -600f);
        if (autoPlayPanelRectPortrait) autoPlayPanelRectPortrait.anchoredPosition = new Vector2(autoPlayPanelRectPortrait.anchoredPosition.x, -600f);
        SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);

        SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
        UpdateSpeedButtonsVisibility(gameManager.currentSpinSpeed);

        isSettingsPanelOpen = false;
        SetGameObjectActive(settingsPanel, settingsPanelPortrait, false);
        if (gameRulesPanel) gameRulesPanel.SetActive(false);
        if (guidePanel) guidePanel.SetActive(false);
        if (winTypePopupObject != null) winTypePopupObject.SetActive(false);

        UpdateTopBarVisibility(true);
        UpdatePingDisplay("-- ms");
    }

    #region Loading & Intro Sequence

    private IEnumerator WaitForInitialization()
    {
        float initializationTimeout = 20f;
        float timer = 0f;
        while (!gameManager.isInitialized && !gameManager.initializationFailed && timer < initializationTimeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (gameManager.initializationFailed || !gameManager.isInitialized)
        {
            if (gameManager.socketManager != null)
            {
                gameManager.socketManager.SetRaycastBlocker(false);
            }

            if (popupManager != null)
            {
                string errorMsg = gameManager.initializationFailed ? "Game failed to initialize." : "Initialization timed out. Please check your connection.";
                popupManager.ShowErrorPopup("Connection Error", errorMsg, true);
            }
        }
        else
        {
            AudioManager.Instance?.PlayBgMusic();
        }
    }

    #endregion

    #region UI Synchronization Helpers

    private void SetTMPText(TMP_Text text1, TMP_Text text2, string content)
    {
        if (text1) text1.text = content;
        if (text2) text2.text = content;
    }

    private void SetGameObjectActive(GameObject obj1, GameObject obj2, bool active)
    {
        if (obj1) obj1.SetActive(active);
        if (obj2) obj2.SetActive(active);
    }

    private void SetButtonInteractable(Button btn1, Button btn2, bool interactable)
    {
        if (btn1) btn1.interactable = interactable;
        if (btn2) btn2.interactable = interactable;
    }

    private void SetButtonActive(Button btn1, Button btn2, bool active)
    {
        if (btn1) btn1.gameObject.SetActive(active);
        if (btn2) btn2.gameObject.SetActive(active);
    }

    #endregion

    #region Button Setup

    private void SetupButtons()
    {
        if (betPlusButton) betPlusButton.onClick.AddListener(() => gameManager.IncreaseBet());
        if (betMinusButton) betMinusButton.onClick.AddListener(() => gameManager.DecreaseBet());
        if (betPlusButtonPortrait) betPlusButtonPortrait.onClick.AddListener(() => gameManager.IncreaseBet());
        if (betMinusButtonPortrait) betMinusButtonPortrait.onClick.AddListener(() => gameManager.DecreaseBet());

        if (spinButton)
        {
            var holdHandler = spinButton.GetComponent<SpinButtonHoldHandler>();
            if (holdHandler != null)
            {
                holdHandler.OnClick.AddListener(OnSpinButtonPressed);
                holdHandler.OnHoldThreeSeconds.AddListener(OnSpinButtonHeld);
            }
            else
            {
                spinButton.onClick.AddListener(OnSpinButtonPressed);
            }
        }
        if (spinButtonPortrait)
        {
            var holdHandler = spinButtonPortrait.GetComponent<SpinButtonHoldHandler>();
            if (holdHandler != null)
            {
                holdHandler.OnClick.AddListener(OnSpinButtonPressed);
                holdHandler.OnHoldThreeSeconds.AddListener(OnSpinButtonHeld);
            }
            else
            {
                spinButtonPortrait.onClick.AddListener(OnSpinButtonPressed);
            }
        }

        if (stopButton) stopButton.onClick.AddListener(OnStopButtonPressed);
        if (stopButtonPortrait) stopButtonPortrait.onClick.AddListener(OnStopButtonPressed);

        if (autoSpinStopButton)
        {
            autoSpinStopButton.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayAutoplayStop();
                gameManager.StopAutoPlay();
            });
        }
        if (autoSpinStopButtonPortrait)
        {
            autoSpinStopButtonPortrait.onClick.AddListener(() =>
            {
                AudioManager.Instance?.PlayAutoplayStop();
                gameManager.StopAutoPlay();
            });
        }

        if (autoPlayCloseButton) autoPlayCloseButton.onClick.AddListener(CloseAutoPlayPanel);
        if (autoPlayCloseButtonPortrait) autoPlayCloseButtonPortrait.onClick.AddListener(CloseAutoPlayPanel);

        if (gameQuitButton) gameQuitButton.onClick.AddListener(OnExitButtonPressed);
        if (gameQuitButtonPortrait) gameQuitButtonPortrait.onClick.AddListener(OnExitButtonPressed);

        if (expandButton) expandButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); OnExpand(); });
        if (shrinkButton) shrinkButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); OnShrink(); });
        if (expandButtonPortrait) expandButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); OnExpand(); });
        if (shrinkButtonPortrait) shrinkButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); OnShrink(); });


        if (winTypeFullScreenButton != null)
        {
            winTypeFullScreenButton.onClick.RemoveAllListeners();
            winTypeFullScreenButton.onClick.AddListener(OnWinTypeScreenClicked);
        }

        if (normalSpeedButton) normalSpeedButton.onClick.AddListener(() => { AudioManager.Instance?.PlayTurboButtonClick(); SetSpeedMode(SpinSpeed.Turbo); });
        if (turboSpeedButton) turboSpeedButton.onClick.AddListener(() => { AudioManager.Instance?.PlayTurboButtonClick(); SetSpeedMode(SpinSpeed.QuickSpin); });
        if (quickSpeedButton) quickSpeedButton.onClick.AddListener(() => { AudioManager.Instance?.PlayTurboButtonClick(); SetSpeedMode(SpinSpeed.Normal); });
        if (normalSpeedButtonPortrait) normalSpeedButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayTurboButtonClick(); SetSpeedMode(SpinSpeed.Turbo); });
        if (turboSpeedButtonPortrait) turboSpeedButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayTurboButtonClick(); SetSpeedMode(SpinSpeed.QuickSpin); });
        if (quickSpeedButtonPortrait) quickSpeedButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayTurboButtonClick(); SetSpeedMode(SpinSpeed.Normal); });
    }

    private void SetupAutoPlayPanel()
    {
        if (autoPlay10Button) autoPlay10Button.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(10); });
        if (autoPlay50Button) autoPlay50Button.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(50); });
        if (autoPlay100Button) autoPlay100Button.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(100); });
        if (autoPlay200Button) autoPlay200Button.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(200); });
        if (autoPlay500Button) autoPlay500Button.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(500); });
        if (autoPlayInfiniteButton) autoPlayInfiniteButton.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(-1); });

        if (autoPlay10ButtonPortrait) autoPlay10ButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(10); });
        if (autoPlay50ButtonPortrait) autoPlay50ButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(50); });
        if (autoPlay100ButtonPortrait) autoPlay100ButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(100); });
        if (autoPlay200ButtonPortrait) autoPlay200ButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(200); });
        if (autoPlay500ButtonPortrait) autoPlay500ButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(500); });
        if (autoPlayInfiniteButtonPortrait) autoPlayInfiniteButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayButton(); StartAutoplayWithRounds(-1); });
    }

    private void SetupSettingsPanel()
    {
        if (settingsOpenButton) settingsOpenButton.onClick.AddListener(() =>
        {
            if (isSettingsPanelOpen)
                CloseSettingsPanel();
            else
                OpenSettingsPanel();
        });
        if (settingsOpenButtonPortrait) settingsOpenButtonPortrait.onClick.AddListener(() =>
        {
            if (isSettingsPanelOpen)
                CloseSettingsPanel();
            else
                OpenSettingsPanel();
        });

        if (settingsCloseButton) settingsCloseButton.onClick.AddListener(CloseSettingsPanel);
        if (settingsCloseButtonPortrait) settingsCloseButtonPortrait.onClick.AddListener(CloseSettingsPanel);
        if (settingsBgCloseButton) settingsBgCloseButton.onClick.AddListener(CloseSettingsPanel);
        if (settingsBgCloseButtonPortrait) settingsBgCloseButtonPortrait.onClick.AddListener(CloseSettingsPanel);

        SetButtonActive(settingsOpenButton, settingsOpenButtonPortrait, true);
        SetButtonActive(settingsCloseButton, settingsCloseButtonPortrait, false);
        SetButtonActive(settingsBgCloseButton, settingsBgCloseButtonPortrait, false);

        if (soundPanelOpenButton) soundPanelOpenButton.onClick.AddListener(OpenSoundPanel);
        if (soundPanelOpenButtonPortrait) soundPanelOpenButtonPortrait.onClick.AddListener(OpenSoundPanel);
        if (soundPanelCloseButton) soundPanelCloseButton.onClick.AddListener(CloseSoundPanel);

        if (musicSlider)
        {
            if (AudioManager.Instance != null) musicSlider.value = AudioManager.Instance.MusicVolume;
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }
        if (sfxSlider)
        {
            if (AudioManager.Instance != null) sfxSlider.value = AudioManager.Instance.SfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
        }
    }

    private void SetupGameRulesPanel()
    {
        if (gameRulesOpenButton) gameRulesOpenButton.onClick.AddListener(OpenGameRulesPanel);
        if (gameRulesOpenButtonPortrait) gameRulesOpenButtonPortrait.onClick.AddListener(OpenGameRulesPanel);

        if (gameRulesBackButton) gameRulesBackButton.onClick.AddListener(CloseGameRulesPanel);
    }

    private void SetupGuidePanel()
    {
        if (guideOpenButton) guideOpenButton.onClick.AddListener(OpenGuidePanel);
        if (guideOpenButtonPortrait) guideOpenButtonPortrait.onClick.AddListener(OpenGuidePanel);

        if (guideBackButton) guideBackButton.onClick.AddListener(CloseGuidePanel);
    }

    #endregion

    #region Game Events

    internal void OnGameInitialized()
    {
        currentWinDisplayValue = 0;
        UpdateBetDisplay();
        UpdateBalanceDisplay();
        UpdateWinDisplay(0);
    }

    internal void OnSpinStarted()
    {
        AudioManager.Instance?.PlaySpinStart();

        SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
        SetBetControlsEnabled(false);
        SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);

        UpdateBalanceDisplay();
        UpdateWinDisplay(0);

        CloseAutoPlayPanelImmediate();
    }

    internal void OnSpinResultReceived()
    {
        SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
    }

    internal void OnSpinStopping(SpinResult result = null)
    {
        UpdateBalanceDisplay();
        if (result != null)
        {
            double displayWin = result.winAmount - result.GetTotalFeatureDeferredWins();
            if (displayWin < 0) displayWin = 0;
            UpdateWinDisplay(displayWin);
        }
    }

    internal void OnSpinCompleted(SpinResult result = null)
    {
        if (result != null)
        {
            double displayWin = result.winAmount - result.GetTotalFeatureDeferredWins();
            if (displayWin < 0) displayWin = 0;
            UpdateWinDisplay(displayWin);
        }
        UpdateBalanceDisplay();

        if (gameManager.isAutoPlaying)
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
        }
        else
        {
            SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);

            SetBetControlsEnabled(true);
            SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);
        }
    }

    internal void TriggerWinTypePopup(double winAmount, double totalBetAmount, System.Action onComplete = null)
    {
        double totalBet = totalBetAmount > 0 ? totalBetAmount : (gameManager != null ? gameManager.currentBetAmount : 0.01);
        double multiplier = winAmount / totalBet;


        if (winTypePopupObject == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (multiplier < bigWinThreshold)
        {
            onComplete?.Invoke();
            return;
        }

        finalWinTypeAmount = winAmount;
        winTypeTotalBet = totalBet;
        onWinTypeCompleteCallback = onComplete;
        isWinTypeCounting = true;
        currentWinTypeCount = 0;
        activeWinTypePhase = 0;

        if (multiplier >= legendaryWinThreshold)
        {
            SetWinTitleActive(legendaryWinTitleObject);
        }
        else if (multiplier >= megaWinThreshold)
        {
            SetWinTitleActive(megaWinTitleObject);
        }
        else
        {
            SetWinTitleActive(bigWinTitleObject);
        }

        if (winTypeWinText != null)
        {
            winTypeWinText.text = "0.00";
        }

        isSpecialWinActive = true;
        DisableControlsDuringWinAnimation();

        winTypePopupObject.SetActive(true);

        CanvasGroup cg = winTypeCanvasGroup != null ? winTypeCanvasGroup : winTypePopupObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = winTypePopupObject.AddComponent<CanvasGroup>();
        winTypeCanvasGroup = cg;

        RectTransform rTr = winTypePopupRect != null ? winTypePopupRect : winTypePopupObject.GetComponent<RectTransform>();
        winTypePopupRect = rTr;

        cg.DOKill();
        cg.alpha = 0f;

        if (rTr != null)
        {
            rTr.DOKill();
            rTr.localScale = Vector3.one;
        }

        Sequence openSeq = DOTween.Sequence();
        openSeq.Join(cg.DOFade(1f, 0.35f).SetEase(Ease.OutQuad));

        if (winTypeWinTextContainer != null)
        {
            Transform tTr = winTypeWinTextContainer.transform;
            tTr.DOKill();
            Vector3 curScale = tTr.localScale;
            float targetX = curScale.x != 0f ? curScale.x : 1f;
            float targetZ = curScale.z != 0f ? curScale.z : 1f;
            tTr.localScale = new Vector3(targetX, 0f, targetZ);

            Sequence textSeq = DOTween.Sequence();
            textSeq.Append(tTr.DOScaleY(1.2f, 0.45f).SetEase(Ease.OutCubic));
            textSeq.Append(tTr.DOScaleY(1.0f, 0.25f).SetEase(Ease.InOutSine));
            openSeq.Join(textSeq);
        }

        AudioManager.Instance?.PlayWinTypePopupOpen();

        if (winTypeCountCoroutine != null) StopCoroutine(winTypeCountCoroutine);
        if (winTypeAutoCloseCoroutine != null) StopCoroutine(winTypeAutoCloseCoroutine);

        winTypeCountCoroutine = StartCoroutine(WinTypeCountSequence());
    }

    private IEnumerator WinTypeCountSequence()
    {
        float elapsed = 0f;
        float duration = (maxCountDuration > 0 && maxCountDuration <= 0.5f) ? maxCountDuration : 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            currentWinTypeCount = LerpDouble(0, finalWinTypeAmount, progress);

            if (winTypeWinText != null)
            {
                winTypeWinText.text = currentWinTypeCount.ToString("N2");
            }

            yield return null;
        }

        CompleteWinTypeCounting();
        winTypeAutoCloseCoroutine = StartCoroutine(WinTypeAutoCloseSequence());
    }

    private double LerpDouble(double start, double end, float progress)
    {
        return start + (end - start) * progress;
    }



    private void SetWinTitleActive(GameObject activeTitle)
    {
        if (bigWinTitleObject != null) bigWinTitleObject.SetActive(bigWinTitleObject == activeTitle);
        if (megaWinTitleObject != null) megaWinTitleObject.SetActive(megaWinTitleObject == activeTitle);
        if (legendaryWinTitleObject != null) legendaryWinTitleObject.SetActive(legendaryWinTitleObject == activeTitle);
    }

    private void CompleteWinTypeCounting()
    {
        isWinTypeCounting = false;
        currentWinTypeCount = finalWinTypeAmount;

        if (winTypeWinText != null)
        {
            winTypeWinText.text = finalWinTypeAmount.ToString("N2");
        }
    }

    private IEnumerator WinTypeAutoCloseSequence()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        CloseWinTypePopup();
    }

    public void OnWinTypeScreenClicked()
    {
        if (isWinTypeCounting)
        {
            if (winTypeCountCoroutine != null) StopCoroutine(winTypeCountCoroutine);
            CompleteWinTypeCounting();

            if (winTypeAutoCloseCoroutine != null) StopCoroutine(winTypeAutoCloseCoroutine);
            winTypeAutoCloseCoroutine = StartCoroutine(WinTypeAutoCloseSequence());
        }
        else
        {
            CloseWinTypePopup();
        }
    }

    public void CloseWinTypePopup()
    {
        if (winTypeCountCoroutine != null) StopCoroutine(winTypeCountCoroutine);
        if (winTypeAutoCloseCoroutine != null) StopCoroutine(winTypeAutoCloseCoroutine);

        AudioManager.Instance?.StopWinTypePopupOpen();

        CanvasGroup cg = winTypeCanvasGroup != null ? winTypeCanvasGroup : (winTypePopupObject != null ? winTypePopupObject.GetComponent<CanvasGroup>() : null);
        if (cg == null && winTypePopupObject != null) cg = winTypePopupObject.AddComponent<CanvasGroup>();
        winTypeCanvasGroup = cg;

        RectTransform rTr = winTypePopupRect != null ? winTypePopupRect : (winTypePopupObject != null ? winTypePopupObject.GetComponent<RectTransform>() : null);
        winTypePopupRect = rTr;

        Sequence closeSeq = DOTween.Sequence();
        if (cg != null)
        {
            cg.DOKill();
            closeSeq.Join(cg.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
        }

        if (rTr != null)
        {
            rTr.DOKill();
        }

        closeSeq.OnComplete(() =>
        {
            if (winTypePopupObject != null)
            {
                winTypePopupObject.SetActive(false);
            }

            isSpecialWinActive = false;
            EnableControlsAfterWinAnimation();

            System.Action callback = onWinTypeCompleteCallback;
            onWinTypeCompleteCallback = null;
            callback?.Invoke();
        });
    }

    internal void TriggerBigWinPopup(SpinResult result, System.Action onComplete = null)
    {
        if (result == null)
        {
            onComplete?.Invoke();
            return;
        }

        double bet = gameManager != null ? gameManager.currentBetAmount : 0.01;
        double win = result.grandTotalWin > 0 ? result.grandTotalWin : result.winAmount;
        TriggerWinTypePopup(win, bet, onComplete);
    }

    internal void DisableControlsDuringWinAnimation()
    {
        SetBetControlsEnabled(false);
        SetSpinStopButtonStates(isSpinningState: false, isInteractable: false);
    }

    internal void EnableControlsAfterWinAnimation()
    {
        if (isSpecialWinActive) return;

        if (gameManager.isAutoPlaying)
        {
            SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
        }
        else
        {
            SetBetControlsEnabled(true);
            SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);
            SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
        }
    }

    #endregion

    #region Spin Button

    public void OnSpinButtonPressed()
    {
        if (gameManager.isAutoPlaying)
        {
            AudioManager.Instance?.PlayAutoplayStop();
            gameManager.StopAutoPlay();
            return;
        }

        if (!gameManager.IsSpinning())
        {
            gameManager.RequestSpin();
        }
    }

    private void OnStopButtonPressed()
    {
        if (gameManager.isAutoPlaying)
        {
            AudioManager.Instance?.PlayAutoplayStop();
            gameManager.StopAutoPlay();
            return;
        }

        if (gameManager.IsSpinning())
        {
            if (Time.unscaledTime - lastRapidStopTime < rapidStopCooldown)
                return;

            lastRapidStopTime = Time.unscaledTime;
            AudioManager.Instance?.PlaySpinStop();
            gameManager.RequestStop();
        }
    }

    internal void DisableSpinButtonDuringStop()
    {
        if (gameManager.isAutoPlaying)
        {
            SetButtonActive(spinButton, spinButtonPortrait, false);
            SetButtonActive(stopButton, stopButtonPortrait, false);
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, true);
            SetButtonInteractable(autoSpinStopButton, autoSpinStopButtonPortrait, false);
        }
        else
        {
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);
            SetButtonActive(spinButton, spinButtonPortrait, false);
            SetButtonActive(stopButton, stopButtonPortrait, true);
            SetButtonInteractable(stopButton, stopButtonPortrait, false);
        }
    }

    internal void SetSpinStopButtonStates(bool isSpinningState, bool isInteractable)
    {
        if (gameManager.isAutoPlaying)
        {
            SetButtonActive(spinButton, spinButtonPortrait, false);
            SetButtonActive(stopButton, stopButtonPortrait, false);
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, true);
            SetButtonInteractable(autoSpinStopButton, autoSpinStopButtonPortrait, isInteractable);
        }
        else
        {
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);

            if (isSpinningState)
            {
                SetButtonActive(spinButton, spinButtonPortrait, false);
                SetButtonActive(stopButton, stopButtonPortrait, true);
                SetButtonInteractable(stopButton, stopButtonPortrait, isInteractable);
            }
            else
            {
                SetButtonActive(stopButton, stopButtonPortrait, false);
                SetButtonActive(spinButton, spinButtonPortrait, true);
                SetButtonInteractable(spinButton, spinButtonPortrait, isInteractable);
            }
        }
    }

    #endregion

    #region Bet Controls

    internal void UpdateBetDisplay()
    {
        if (gameManager.gameConfig == null) return;

        double totalPay = gameManager.GetTotalPay();

        if (betAmountText) betAmountText.text = FormatAmount(totalPay);
        if (betAmountTextPortrait) betAmountTextPortrait.text = "TOTAL PAY : " + FormatAmount(totalPay);

        UpdateBetButtonStates();
        UpdateGameRulesDynamicTexts();
    }

    private void UpdateBetButtonStates()
    {
        SetButtonInteractable(betMinusButton, betMinusButtonPortrait, true);
        SetButtonInteractable(betPlusButton, betPlusButtonPortrait, true);
    }

    #endregion

    #region Auto Play Panel

    public void OnSpinButtonHeld()
    {
        if (gameManager.currentState == GameState.Idle && !gameManager.isAutoPlaying)
        {
            AudioManager.Instance?.PlayButton();
            OpenAutoPlayPanel();
        }
    }

    private void OpenAutoPlayPanel()
    {
        AudioManager.Instance?.PlayAutoplayPanelOpen();
        if (isSettingsPanelOpen)
            CloseSettingsPanelImmediate();

        SetGameObjectActive(autoPlayPanel, autoPlayPanelPortrait, true);
        if (autoPlayPanelRect)
        {
            autoPlayPanelRect.anchoredPosition = new Vector2(autoPlayPanelRect.anchoredPosition.x, -600f);
            autoPlayPanelRect.DOAnchorPosY(0f, 0.35f).SetEase(Ease.OutCubic);
        }
        if (autoPlayPanelRectPortrait)
        {
            autoPlayPanelRectPortrait.anchoredPosition = new Vector2(autoPlayPanelRectPortrait.anchoredPosition.x, -600f);
            autoPlayPanelRectPortrait.DOAnchorPosY(0f, 0.35f).SetEase(Ease.OutCubic);
        }
    }

    private void CloseAutoPlayPanel()
    {
        AudioManager.Instance?.PlayPopupClose();

        if (autoPlayPanelRect)
        {
            autoPlayPanelRect.DOAnchorPosY(-600f, 0.35f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                if (autoPlayPanel) autoPlayPanel.SetActive(false);
            });
        }
        else
        {
            if (autoPlayPanel) autoPlayPanel.SetActive(false);
        }

        if (autoPlayPanelRectPortrait)
        {
            autoPlayPanelRectPortrait.DOAnchorPosY(-600f, 0.35f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                if (autoPlayPanelPortrait) autoPlayPanelPortrait.SetActive(false);
            });
        }
        else
        {
            if (autoPlayPanelPortrait) autoPlayPanelPortrait.SetActive(false);
        }
    }

    private void StartAutoplayWithRounds(int rounds)
    {
        CloseAutoPlayPanel();
        gameManager.StartAutoPlay(rounds);
    }

    internal void OnAutoPlayStarted()
    {
        UpdateAutoPlayCount();
        SetSpinStopButtonStates(isSpinningState: true, isInteractable: true);
        SetBetControlsEnabled(false);
    }

    internal void OnAutoPlayStopped()
    {
        SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);

        bool isRoundActive = gameManager.IsSpinning() || gameManager.lastResult != null;

        if (!isRoundActive)
        {
            SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
            SetBetControlsEnabled(true);
            SetButtonInteractable(settingsOpenButton, settingsOpenButtonPortrait, true);
        }
        else if (isRoundActive)
        {
            SetButtonActive(spinButton, spinButtonPortrait, false);
            SetButtonActive(stopButton, stopButtonPortrait, false);
            SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, true);
            SetButtonInteractable(autoSpinStopButton, autoSpinStopButtonPortrait, false);
        }
    }

    internal void UpdateAutoPlayCount()
    {
        string displayStr = "";
        if (gameManager.autoPlayTotalRounds == -1 || gameManager.autoPlayRemainingRounds < 0)
        {
            displayStr = "∞";
        }
        else
        {
            displayStr = $"{gameManager.autoPlayRemainingRounds}";
        }

        SetTMPText(autoSpinRemainingText, autoSpinRemainingTextPortrait, displayStr);
    }

    #endregion

    #region Spin Speed Universal Toggle Logic

    public void SetSpeedMode(SpinSpeed speed)
    {
        gameManager.SetSpinSpeed(speed);
        UpdateSpeedButtonsVisibility(speed);
    }

    private void UpdateSpeedButtonsVisibility(SpinSpeed speed)
    {
        SetButtonActive(normalSpeedButton, normalSpeedButtonPortrait, speed == SpinSpeed.Normal);
        SetButtonActive(turboSpeedButton, turboSpeedButtonPortrait, speed == SpinSpeed.Turbo);
        SetButtonActive(quickSpeedButton, quickSpeedButtonPortrait, speed == SpinSpeed.QuickSpin);
    }

    #endregion

    #region Sound Panel

    private void OpenSoundPanel()
    {
        AudioManager.Instance?.PlayPopupOpen();
        if (soundPanel == null) return;
        soundPanel.SetActive(true);
        if (soundPanelRect != null)
        {
            AnimatePopupOpen(soundPanelRect);
        }
        if (musicSlider && AudioManager.Instance != null)
        {
            musicSlider.value = AudioManager.Instance.MusicVolume;
        }
        if (sfxSlider && AudioManager.Instance != null)
        {
            sfxSlider.value = AudioManager.Instance.SfxVolume;
        }
    }

    private void CloseSoundPanel()
    {
        if (soundPanel == null || !soundPanel.activeSelf) return;
        if (soundPanelRect != null)
        {
            AnimatePopupClose(soundPanelRect, () =>
            {
                soundPanel.SetActive(false);
            });
        }
        else
        {
            AudioManager.Instance?.PlayPopupClose();
            soundPanel.SetActive(false);
        }
    }

    private void OnMusicSliderChanged(float val)
    {
        AudioManager.Instance?.SetMusicVolume(val);
    }

    private void OnSfxSliderChanged(float val)
    {
        AudioManager.Instance?.SetSfxVolume(val);
    }

    #endregion

    #region Settings Panel

    private void OpenSettingsPanel()
    {
        if ((autoPlayPanel && autoPlayPanel.activeSelf) || (autoPlayPanelPortrait && autoPlayPanelPortrait.activeSelf))
            CloseAutoPlayPanelImmediate();

        AudioManager.Instance?.PlayButton();
        isSettingsPanelOpen = true;

        SetButtonActive(settingsOpenButton, settingsOpenButtonPortrait, false);
        SetButtonActive(settingsCloseButton, settingsCloseButtonPortrait, true);
        SetButtonActive(settingsBgCloseButton, settingsBgCloseButtonPortrait, true);

        if (settingsPanel)
        {
            settingsPanel.SetActive(true);
            CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = settingsPanel.AddComponent<CanvasGroup>();
            cg.DOKill();
            cg.DOFade(1f, 0.35f);
        }

        if (settingsPanelPortrait)
        {
            settingsPanelPortrait.SetActive(true);
            CanvasGroup cg = settingsPanelPortrait.GetComponent<CanvasGroup>();
            if (cg == null) cg = settingsPanelPortrait.AddComponent<CanvasGroup>();
            cg.DOKill();
            cg.DOFade(1f, 0.35f);
        }
    }

    private void CloseSettingsPanel()
    {
        AudioManager.Instance?.PlayButton();
        isSettingsPanelOpen = false;

        SetButtonActive(settingsOpenButton, settingsOpenButtonPortrait, true);
        SetButtonActive(settingsCloseButton, settingsCloseButtonPortrait, false);
        SetButtonActive(settingsBgCloseButton, settingsBgCloseButtonPortrait, false);

        if (settingsPanel)
        {
            CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = settingsPanel.AddComponent<CanvasGroup>();
            cg.DOKill();
            cg.DOFade(0f, 0.35f).OnComplete(() =>
            {
                settingsPanel.SetActive(false);
            });
        }

        if (settingsPanelPortrait)
        {
            CanvasGroup cg = settingsPanelPortrait.GetComponent<CanvasGroup>();
            if (cg == null) cg = settingsPanelPortrait.AddComponent<CanvasGroup>();
            cg.DOKill();
            cg.DOFade(0f, 0.35f).OnComplete(() =>
            {
                settingsPanelPortrait.SetActive(false);
            });
        }
    }

    private void CloseSettingsPanelImmediate()
    {
        isSettingsPanelOpen = false;

        SetButtonActive(settingsOpenButton, settingsOpenButtonPortrait, true);
        SetButtonActive(settingsCloseButton, settingsCloseButtonPortrait, false);
        SetButtonActive(settingsBgCloseButton, settingsBgCloseButtonPortrait, false);

        if (settingsPanel)
        {
            CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.DOKill();
                cg.alpha = 0f;
            }
            settingsPanel.SetActive(false);
        }

        if (settingsPanelPortrait)
        {
            CanvasGroup cg = settingsPanelPortrait.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.DOKill();
                cg.alpha = 0f;
            }
            settingsPanelPortrait.SetActive(false);
        }
    }

    private void CloseAutoPlayPanelImmediate()
    {
        if (autoPlayPanelRect) autoPlayPanelRect.localScale = Vector3.one;
        if (autoPlayPanelRectPortrait) autoPlayPanelRectPortrait.localScale = Vector3.one;
        SetGameObjectActive(autoPlayPanel, autoPlayPanelPortrait, false);
    }

    #endregion

    #region Game Rules Panel

    private void OpenGameRulesPanel()
    {
        if (isSettingsPanelOpen)
        {
            CloseSettingsPanelImmediate();
        }
        ShowGameRulesPanel();
    }

    private void ShowGameRulesPanel()
    {
        if (gameRulesPanel == null) return;
        AudioManager.Instance?.PlayButton();
        UpdateGameRulesDynamicTexts();
        gameRulesPanel.SetActive(true);
    }

    public void UpdateGameRulesDynamicTexts()
    {
        double pGreenRed = 1000;
        double pGreenOrange = 400;
        double pGreenYellow = 300;
        double pAllGreen = 200;
        double pAllRed7 = 25;
        double pAllBlue7 = 15;
        double pMixed7s = 10;
        double pTripleBars = 10;
        double pDoubleBars = 5;
        double pAnyBars7 = 3;
        double pMixedBars7s = 2;

        if (gameManager != null && gameManager.gameConfig != null)
        {
            var cfg = gameManager.gameConfig;
            if (cfg.greenRedChilli != null && cfg.greenRedChilli.payout > 0) pGreenRed = cfg.greenRedChilli.payout;
            if (cfg.greenOrangeChilli != null && cfg.greenOrangeChilli.payout > 0) pGreenOrange = cfg.greenOrangeChilli.payout;
            if (cfg.greenYellowChilli != null && cfg.greenYellowChilli.payout > 0) pGreenYellow = cfg.greenYellowChilli.payout;
            if (cfg.allGreenChilli != null && cfg.allGreenChilli.payout > 0) pAllGreen = cfg.allGreenChilli.payout;
            if (cfg.allRedSeven != null && cfg.allRedSeven.payout > 0) pAllRed7 = cfg.allRedSeven.payout;
            if (cfg.allBlueSeven != null && cfg.allBlueSeven.payout > 0) pAllBlue7 = cfg.allBlueSeven.payout;
            if (cfg.mixedSevens != null && cfg.mixedSevens.payout > 0) pMixed7s = cfg.mixedSevens.payout;
            if (cfg.allTripleBars != null && cfg.allTripleBars.payout > 0) pTripleBars = cfg.allTripleBars.payout;
            if (cfg.allDoubleBars != null && cfg.allDoubleBars.payout > 0) pDoubleBars = cfg.allDoubleBars.payout;
            if (cfg.anyBarsSeven != null && cfg.anyBarsSeven.payout > 0) pAnyBars7 = cfg.anyBarsSeven.payout;
            if (cfg.mixedbarsAndSevens != null && cfg.mixedbarsAndSevens.payout > 0) pMixedBars7s = cfg.mixedbarsAndSevens.payout;
        }

        if (ruleGreenRedChilliText) ruleGreenRedChilliText.text = $"BET  x  {pGreenRed}";
        if (ruleGreenOrangeChilliText) ruleGreenOrangeChilliText.text = $"BET  x  {pGreenOrange}";
        if (ruleGreenYellowChilliText) ruleGreenYellowChilliText.text = $"BET  x  {pGreenYellow}";
        if (ruleAllGreenChilliText) ruleAllGreenChilliText.text = $"BET  x  {pAllGreen}";
        if (ruleChilliMultiplierText) ruleChilliMultiplierText.text = "BET  x  MULTIPLIER";
        if (ruleAllRedSevenText) ruleAllRedSevenText.text = $"BET  x  {pAllRed7}";
        if (ruleAllBlueSevenText) ruleAllBlueSevenText.text = $"BET  x  {pAllBlue7}";
        if (ruleMixedSevensText) ruleMixedSevensText.text = $"BET  x  {pMixed7s}";
        if (ruleAllTripleBarsText) ruleAllTripleBarsText.text = $"BET  x  {pTripleBars}";
        if (ruleAllDoubleBarsText) ruleAllDoubleBarsText.text = $"BET  x  {pDoubleBars}";
        if (ruleAnyBarsSevenText) ruleAnyBarsSevenText.text = $"BET  x  {pAnyBars7}";
        if (ruleMixedBarsSevensText) ruleMixedBarsSevensText.text = $"BET  x  {pMixedBars7s}";

        if (totalLineCountText) totalLineCountText.text = "1 LINES";
    }

    private void CloseGameRulesPanel()
    {
        if (gameRulesPanel == null) return;
        AudioManager.Instance?.PlayButton();
        gameRulesPanel.SetActive(false);
    }

    #endregion

    #region Guide Panel

    private void OpenGuidePanel()
    {
        if (isSettingsPanelOpen)
        {
            CloseSettingsPanelImmediate();
        }
        ShowGuidePanel();
    }

    private void ShowGuidePanel()
    {
        if (guidePanel == null) return;
        AudioManager.Instance?.PlayButton();
        guidePanel.SetActive(true);
    }

    private void CloseGuidePanel()
    {
        if (guidePanel == null) return;
        AudioManager.Instance?.PlayButton();
        guidePanel.SetActive(false);
    }

    #endregion



    #region Expand / Shrink

    private void InitializeExpandShrink()
    {
        SetExpandShrinkButtons(isExpanded: false);
    }

    private void OnExpand()
    {
        isExpanded = true;
        jsFunctCalls?.RequestExpandGame();
        SetExpandShrinkButtons(isExpanded: true);
    }

    private void OnShrink()
    {
        isExpanded = false;
        jsFunctCalls?.RequestShrinkGame();
        SetExpandShrinkButtons(isExpanded: false);
    }

    private void SetExpandShrinkButtons(bool isExpanded)
    {
        SetButtonActive(expandButton, expandButtonPortrait, !isExpanded);
        SetButtonActive(shrinkButton, shrinkButtonPortrait, isExpanded);
    }

    private void RegisterFullscreenListener()
    {
        jsFunctCalls?.RegisterFullscreenListener(gameObject.name);
    }

    internal void OnFullscreenChanged(string isFullscreen)
    {
        bool newExpandedState = isFullscreen == "1";

        if (isExpanded != newExpandedState)
        {
            isExpanded = newExpandedState;
            SetExpandShrinkButtons(isExpanded);
        }
    }

    #endregion

    #region Popup Animations (Generic)

    private void AnimatePopupOpen(RectTransform popupRect)
    {
        if (!popupRect) return;
        popupRect.localScale = Vector3.zero;
        popupRect.DOScale(1.4f, 0.3f).SetEase(Ease.OutBack);
    }

    private void AnimatePopupClose(RectTransform popupRect, System.Action onComplete)
    {
        if (!popupRect) return;

        AudioManager.Instance?.PlayPopupClose();

        Sequence closeSeq = DOTween.Sequence();
        closeSeq.Append(popupRect.DOScale(1.5f, 0.1f));
        closeSeq.Append(popupRect.DOScale(0f, 0.2f).SetEase(Ease.InBack));
        closeSeq.OnComplete(() =>
        {
            popupRect.localScale = Vector3.one * 1.4f;
            onComplete?.Invoke();
        });
    }

    #endregion

    #region Display Updates

    internal void UpdatePingDisplay(int pingMs)
    {
        SetTMPText(pingText, pingTextPortrait, $"{pingMs} ms");
    }

    internal void UpdatePingDisplay(string content)
    {
        SetTMPText(pingText, pingTextPortrait, content);
    }

    public void UpdateJackpotDisplay(JackpotValues values)
    {
        if (values == null) return;

        SetTMPText(grandJackpotText, grandJackpotTextPortrait, FormatJackpotValue(values.grandJackpot));
        SetTMPText(majorJackpotText, majorJackpotTextPortrait, FormatJackpotValue(values.majorJackpot));
        SetTMPText(minorJackpotText, minorJackpotTextPortrait, FormatJackpotValue(values.minorJackpot));
        SetTMPText(miniJackpotText, miniJackpotTextPortrait, FormatJackpotValue(values.miniJackpot));
    }

    private string FormatJackpotValue(string val)
    {
        if (string.IsNullOrEmpty(val)) return "$0.00";
        return val.StartsWith("$") ? val : "$" + val;
    }

    internal void UpdateBalanceDisplay()
    {
        SetTMPText(balanceText, balanceTextPortrait, "BALANCE : " + FormatAmount(gameManager.playerData.balance));
    }

    private void UpdateWinDisplay(double amount)
    {
        currentWinDisplayValue = amount;
        if (winAmountText) winAmountText.text = FormatAmount(amount);
        if (winAmountTextPortrait) winAmountTextPortrait.text = "WIN " + FormatAmount(amount);

        bool showWinText = amount > 0;

        if (showWinText)
        {
            SetGameObjectActive(goodLuckObject, goodLuckObjectPortrait, false);
            SetGameObjectActive(winTextObject, winTextObjectPortrait, true);
        }
        else
        {
            SetGameObjectActive(goodLuckObject, goodLuckObjectPortrait, true);
            SetGameObjectActive(winTextObject, winTextObjectPortrait, false);
        }
    }

    private void AnimateBalanceUpdate(double newBalance, double startBalance = -1f, float durationOverride = -1f)
    {
        if (balanceTween != null) balanceTween.Kill();
        SetTMPText(balanceText, balanceTextPortrait, "BALANCE : " + FormatAmount(newBalance));
    }

    private void AnimateWinUpdate(double targetWin, float duration = 0.8f)
    {
        if (winTween != null) winTween.Kill();
        UpdateWinDisplay(targetWin);
    }

    #endregion

    #region Helper Methods

    private string FormatAmount(double amount)
    {
        return amount.ToString("0.###");
    }

    private void SetBetControlsEnabled(bool enabled)
    {
        SetButtonInteractable(betPlusButton, betPlusButtonPortrait, enabled);
        SetButtonInteractable(betMinusButton, betMinusButtonPortrait, enabled);
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        StopJackpotPortraitLevitation();
        if (balanceTween != null) balanceTween.Kill();
        if (winTween != null) winTween.Kill();
        DOTween.KillAll();
    }
    #endregion
        

#region Jackpot Portrait Levitation Animation

    private void UpdateJackpotPortraitLevitationFromCurrentOrientation()
    {
        var oc = GetOrientationChange();
        if (oc != null)
        {
            UpdateJackpotPortraitLevitation(oc.CurrentMode);
        }
    }

    private void UpdateJackpotPortraitLevitation(OrientationChange.OrientationMode mode)
    {
        bool isPortrait = (mode == OrientationChange.OrientationMode.MobilePortrait || mode == OrientationChange.OrientationMode.DesktopPortrait);
        if (isPortrait)
        {
            StartJackpotPortraitLevitation();
        }
        else
        {
            StopJackpotPortraitLevitation();
        }
    }

    private List<Transform> GetJackpotPortraitTransforms()
    {
        List<Transform> list = new List<Transform>();

        Transform grandTr = grandJackpotPortraitParent != null ? grandJackpotPortraitParent : (grandJackpotTextPortrait != null ? grandJackpotTextPortrait.transform.parent : null);
        Transform majorTr = majorJackpotPortraitParent != null ? majorJackpotPortraitParent : (majorJackpotTextPortrait != null ? majorJackpotTextPortrait.transform.parent : null);
        Transform minorTr = minorJackpotPortraitParent != null ? minorJackpotPortraitParent : (minorJackpotTextPortrait != null ? minorJackpotTextPortrait.transform.parent : null);
        Transform miniTr = miniJackpotPortraitParent != null ? miniJackpotPortraitParent : (miniJackpotTextPortrait != null ? miniJackpotTextPortrait.transform.parent : null);

        if (grandTr != null) list.Add(grandTr);
        if (majorTr != null) list.Add(majorTr);
        if (minorTr != null) list.Add(minorTr);
        if (miniTr != null) list.Add(miniTr);

        return list;
    }

    private void StartJackpotPortraitLevitation()
    {
        if (!enableJackpotPortraitLevitation) return;

        StopJackpotPortraitLevitation();

        List<Transform> portraitJackpots = GetJackpotPortraitTransforms();
        if (portraitJackpots.Count == 0) return;

        for (int i = 0; i < portraitJackpots.Count; i++)
        {
            Transform tr = portraitJackpots[i];
            if (tr == null) continue;

            if (!jackpotInitialLocalPositions.ContainsKey(tr))
            {
                jackpotInitialLocalPositions[tr] = tr.localPosition;
            }

            Vector3 startPos = jackpotInitialLocalPositions[tr];
            tr.localPosition = startPos;

            float targetY = startPos.y + jackpotLevitateHeight;
            float delay = i * jackpotStaggerDelay;

            Tween posTween = tr.DOLocalMoveY(targetY, jackpotLevitateDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(delay);

            jackpotPortraitTweens.Add(posTween);
        }
    }

    private void StopJackpotPortraitLevitation()
    {
        for (int i = 0; i < jackpotPortraitTweens.Count; i++)
        {
            if (jackpotPortraitTweens[i] != null && jackpotPortraitTweens[i].IsActive())
            {
                jackpotPortraitTweens[i].Kill();
            }
        }
        jackpotPortraitTweens.Clear();

        foreach (var kvp in jackpotInitialLocalPositions)
        {
            if (kvp.Key != null)
            {
                DOTween.Kill(kvp.Key);
                kvp.Key.localPosition = kvp.Value;
            }
        }
    }

    #endregion



    #region Connection Popup Management

    private void OnExitButtonPressed()
    {
        if (popupManager != null)
        {
            popupManager.ShowExitGamePopup();
        }
        else if (gameManager != null)
        {
            gameManager.ExitGame();
        }
    }

    #endregion

    private OrientationChange GetOrientationChange()
    {
        if (orientationChange != null) return orientationChange;
        orientationChange = Object.FindFirstObjectByType<OrientationChange>();
        return orientationChange;
    }

    private OCController GetOCController()
    {
        if (ocController != null) return ocController;
        ocController = Object.FindFirstObjectByType<OCController>();
        return ocController;
    }

    private void UpdateTopBarVisibility(bool visible = true)
    {
        if (!visible)
        {
            if (landscapeTopBarObject != null) landscapeTopBarObject.SetActive(false);
            if (portraitTopBarObject != null) portraitTopBarObject.SetActive(false);
            return;
        }

        var oc = GetOrientationChange();
        bool isPortrait = (oc != null && oc.CurrentMode == OrientationChange.OrientationMode.MobilePortrait);

        if (landscapeTopBarObject != null) landscapeTopBarObject.SetActive(!isPortrait);
        if (portraitTopBarObject != null) portraitTopBarObject.SetActive(isPortrait);
    }
}
