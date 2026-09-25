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

    [Header("Bonus Wheels")]
    [SerializeField] private WheelSpinController redWheel;
    [SerializeField] private WheelSpinController greenWheel;
    [SerializeField] private Coffee.UIExtensions.ShinyEffectForUGUI redWheelShineEffect;
    [SerializeField] private Coffee.UIExtensions.ShinyEffectForUGUI greenWheelShineEffect;
    [SerializeField] private Button wheelSpinButton;
    [Header("Bonus Wheel - Portrait")]
    [SerializeField] private Button wheelSpinButtonPortrait;


    [Header("Top Bar Settings")]
    [SerializeField] private GameObject landscapeTopBarObject;
    [SerializeField] private GameObject portraitTopBarObject;

    [Header("Bonus Wheel Transition Settings")]
    [SerializeField] private Transform slotObject;
    [SerializeField] private GameObject wheelLandscapeBackground;
    [SerializeField] private GameObject wheelPortraitBackground;

    [SerializeField] private Transform wheelParent;
    [SerializeField] private Transform redWheelTransform;
    [SerializeField] private Transform greenWheelTransform;
    [SerializeField] private Button redCenterSpinButton;
    [SerializeField] private Button greenCenterSpinButton;
    [SerializeField] private GameObject redWheelTitleObject;
    [SerializeField] private GameObject greenWheelTitleObject;
    [SerializeField] private GameObject dualWheelTitleObject;
    [SerializeField] private float slotBgMoveDuration = 0.5f;
    [SerializeField] private float slotBgTargetY = -1000f;
    [SerializeField] private float portraitSlotBgTargetY = -1333f;
    [SerializeField] private float wheelMoveDuration = 0.6f;
    [SerializeField] private float phase2MoveDuration = 0.6f;
    [SerializeField] private float redWheelTargetX = -175f;
    [SerializeField] private float greenWheelTargetX = 175f;
    [SerializeField] private Vector3 redWheelFinalPos = new Vector3(-140f, 25f, 0f);
    [SerializeField] private Vector3 greenWheelFinalPos = new Vector3(140f, 65f, 0f);
    [SerializeField] private Vector3 redWheelPortraitStartPos = new Vector3(-150f, 20f, 0f);
    [SerializeField] private Vector3 greenWheelPortraitStartPos = new Vector3(150f, 90f, 0f);
    [SerializeField] private Vector3 redWheelPortraitTargetPos = new Vector3(0f, -170f, 0f);
    [SerializeField] private Vector3 greenWheelPortraitTargetPos = new Vector3(0f, 170f, 0f);
    [SerializeField] private Vector3 redWheelLandscapeStartPos = new Vector3(675f, 111f, 0f);
    [SerializeField] private Vector3 greenWheelLandscapeStartPos = new Vector3(-688f, 111f, 0f);
    [SerializeField] private Vector3 wheelLandscapeStartScale = new Vector3(1.0f, 1.0f, 1.0f);
    [SerializeField] private Vector3 wheelPortraitStartScale = new Vector3(1.2f, 1.2f, 1.2f);
    [SerializeField] private Vector3 wheelLandscapeTargetScale = new Vector3(1.3f, 1.3f, 1.3f);
    [SerializeField] private Vector3 wheelPortraitTargetScale = new Vector3(1.5f, 1.5f, 1.5f);
    [SerializeField] private Vector3 wheelLandscapePopScale = new Vector3(1.5f, 1.5f, 1.5f);
    [SerializeField] private Vector3 wheelPortraitPopScale = new Vector3(1.7f, 1.7f, 1.7f);

    [Header("Rainbow Panel Settings")]
    [SerializeField] private GameObject rainbowPanel;

    [Header("Wheel Result Custom Popup")]
    [SerializeField] private GameObject wheelResultPopup;
    [SerializeField] private RectTransform wheelResultPopupRect;
    [SerializeField] private GameObject wheelResultRedDummy;
    [SerializeField] private GameObject wheelResultGreenDummy;
    [SerializeField] private RectTransform wheelResultAreaPanel;
    [SerializeField] private RectTransform wheelResultNumbersArea;
    [SerializeField] private VerticalLayoutGroup wheelResultNumbersLayoutGroup;
    [SerializeField] private TMP_Text wheelResultBetText;
    [SerializeField] private GameObject wheelResultBetRowObject;
    [SerializeField] private GameObject wheelResultRedRowObject;
    [SerializeField] private GameObject wheelResultGreenRowObject;
    [SerializeField] private TMP_Text wheelResultRedMultiplierText;
    [SerializeField] private TMP_Text wheelResultGreenMultiplierText;
    [SerializeField] private GameObject wheelResultFirstXObject;
    [SerializeField] private GameObject wheelResultSecondXObject;
    [SerializeField] private TMP_Text wheelResultWinAmountText;
    [SerializeField] private float wheelPopupOpenDuration = 0.4f;
    [SerializeField] private float wheelPopupCloseDuration = 0.3f;
    [SerializeField] private float dummyWheelStaggerDelay = 0.15f;
    [SerializeField] private float wheelResultPopupDelay = 2.0f;

    [Header("Take Button")]
    [SerializeField] private Button uwpTakeButton;
    [SerializeField] private Button uwpTakeButtonPortrait;

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
    [SerializeField] private StarFountain winTypeStarRain;

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

    [Header("Game Rules Dynamic Texts - 9 Symbols")]
    [SerializeField] private TMP_Text totalLineCountText;
    [SerializeField] private TMP_Text ruleRed3XText;
    [SerializeField] private TMP_Text ruleBlue2XText;
    [SerializeField] private TMP_Text ruleBlue7Text;
    [SerializeField] private TMP_Text ruleWhite7Text;
    [SerializeField] private TMP_Text ruleWhite7BarText;
    [SerializeField] private TMP_Text ruleRed7Text;
    [SerializeField] private TMP_Text ruleTripleBarText;
    [SerializeField] private TMP_Text ruleDoubleBarText;
    [SerializeField] private TMP_Text ruleSingleBarText;

    [Header("Game Rules Dynamic Texts - Any Payouts")]
    [SerializeField] private TMP_Text ruleAnyWildText;
    [SerializeField] private TMP_Text ruleAny7Text;
    [SerializeField] private TMP_Text ruleAnyBarText;
    [SerializeField] private TMP_Text ruleAnyOneRed3XText;
    [SerializeField] private TMP_Text ruleAnyOneBlue2XText;


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

    private bool isWheelBonusActive = false;
    private Tween redWheelPulseTween;
    private Tween greenWheelPulseTween;

    private Coroutine winTypeCountCoroutine;
    private Coroutine winTypeAutoCloseCoroutine;
    private bool isWinTypeCounting;
    private double finalWinTypeAmount;
    private double currentWinTypeCount;
    private double winTypeTotalBet;
    private int activeWinTypePhase;
    private System.Action onWinTypeCompleteCallback;



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
        DisableRainbowPanel();
        InitializeUI();
        StartCoroutine(WaitForInitialization());
        RegisterFullscreenListener();
        UpdateJackpotPortraitLevitationFromCurrentOrientation();
    }

    private void OnEnable()
    {
        OrientationChange.OnOrientationChanged += HandleOrientationChangedForWheelButtons;
        var oc = GetOrientationChange();
        if (oc != null)
        {
            oc.OnOrientationChangedInstance += HandleOrientationChangedForWheelButtons;
        }
    }

    private void OnDisable()
    {
        OrientationChange.OnOrientationChanged -= HandleOrientationChangedForWheelButtons;
        if (orientationChange != null)
        {
            orientationChange.OnOrientationChangedInstance -= HandleOrientationChangedForWheelButtons;
        }
        StopJackpotPortraitLevitation();
    }

    private void HandleOrientationChangedForWheelButtons(OrientationChange.OrientationMode mode, int width, int height)
    {
        UpdateNewWheelSpinButtonsVisibility();
        UpdateWheelPositionsForOrientation();
        if ((wheelLandscapeBackground != null && wheelLandscapeBackground.activeSelf) ||
            (wheelPortraitBackground != null && wheelPortraitBackground.activeSelf))
        {
            EnableWheelBackgroundBasedOnOrientation();
        }
        UpdateJackpotPortraitLevitation(mode);
        UpdateTopBarVisibility(!isWheelBonusActive);
    }

    private void UpdateWheelPositionsForOrientation()
    {
        if (isWheelBonusActive) return;

        var oc = GetOrientationChange();
        bool isPortraitMode = (oc != null && oc.CurrentMode == OrientationChange.OrientationMode.MobilePortrait);

        Transform redTr = GetRedWheelTransform();
        Transform greenTr = GetGreenWheelTransform();

        Vector3 redStart = isPortraitMode ? redWheelPortraitStartPos : redWheelLandscapeStartPos;
        Vector3 greenStart = isPortraitMode ? greenWheelPortraitStartPos : greenWheelLandscapeStartPos;
        Vector3 startScale = isPortraitMode ? wheelPortraitStartScale : wheelLandscapeStartScale;

        if (redTr != null)
        {
            redTr.localPosition = redStart;
            redTr.localScale = startScale;
        }
        if (greenTr != null)
        {
            greenTr.localPosition = greenStart;
            greenTr.localScale = startScale;
        }
    }

    private void EnableWheelBackgroundBasedOnOrientation()
    {
        var oc = GetOrientationChange();
        bool isPortraitMode = (oc != null && oc.CurrentMode == OrientationChange.OrientationMode.MobilePortrait);

        if (wheelLandscapeBackground != null) wheelLandscapeBackground.SetActive(!isPortraitMode);
        if (wheelPortraitBackground != null) wheelPortraitBackground.SetActive(isPortraitMode);
    }

    private void DisableWheelBackgrounds()
    {
        if (wheelLandscapeBackground != null) wheelLandscapeBackground.SetActive(false);
        if (wheelPortraitBackground != null) wheelPortraitBackground.SetActive(false);
    }

    private void UpdateNewWheelSpinButtonsVisibility()
    {
        if (!isWheelBonusActive || wheelSpinTriggered) return;

        var oc = GetOrientationChange();
        bool isPortraitMode = (oc != null && oc.CurrentMode == OrientationChange.OrientationMode.MobilePortrait);

        if (wheelSpinButton) wheelSpinButton.gameObject.SetActive(!isPortraitMode);
        if (wheelSpinButtonPortrait) wheelSpinButtonPortrait.gameObject.SetActive(isPortraitMode);
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
        FindWheelResultPopupReferences();
        if (wheelResultPopup != null) wheelResultPopup.SetActive(false);

        var redInit = GetRedWheelController();
        if (redInit != null) redInit.ResetWheelEffects();
        var greenInit = GetGreenWheelController();
        if (greenInit != null) greenInit.ResetWheelEffects();
        if (redWheelTitleObject != null) redWheelTitleObject.SetActive(false);
        if (greenWheelTitleObject != null) greenWheelTitleObject.SetActive(false);
        if (dualWheelTitleObject != null) dualWheelTitleObject.SetActive(false);
        DisableWheelBackgrounds();
        isWheelBonusActive = false;
        SetAllWheelSpinButtonsInteractable(false);
        UpdateTopBarVisibility(true);
        SetButtonActive(wheelSpinButton, wheelSpinButtonPortrait, false);
        UpdatePingDisplay("-- ms");
        UpdateWheelPositionsForOrientation();
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

        if (wheelSpinButton) wheelSpinButton.onClick.AddListener(() => { AudioManager.Instance?.PlayWheelStart(); OnWheelSpinClicked(); });
        if (wheelSpinButtonPortrait) wheelSpinButtonPortrait.onClick.AddListener(() => { AudioManager.Instance?.PlayWheelStart(); OnWheelSpinClicked(); });
        if (redCenterSpinButton) redCenterSpinButton.onClick.AddListener(() => { AudioManager.Instance?.PlayWheelStart(); OnWheelSpinClicked(); });
        if (greenCenterSpinButton) greenCenterSpinButton.onClick.AddListener(() => { AudioManager.Instance?.PlayWheelStart(); OnWheelSpinClicked(); });

        var redSetup = GetRedWheelController();
        if (redSetup != null && redSetup.CenterSpinButton != null)
        {
            redSetup.CenterSpinButton.onClick.AddListener(() => { AudioManager.Instance?.PlayWheelStart(); OnWheelSpinClicked(); });
        }
        var greenSetup = GetGreenWheelController();
        if (greenSetup != null && greenSetup.CenterSpinButton != null)
        {
            greenSetup.CenterSpinButton.onClick.AddListener(() => { AudioManager.Instance?.PlayWheelStart(); OnWheelSpinClicked(); });
        }

        if (uwpTakeButton) uwpTakeButton.onClick.AddListener(() => AudioManager.Instance?.PlayTakeButton());
        if (uwpTakeButtonPortrait) uwpTakeButtonPortrait.onClick.AddListener(() => AudioManager.Instance?.PlayTakeButton());

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
        if (winTypeStarRain != null) winTypeStarRain.PlayStarRain();
        StartWinTypeWheelPulse();

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
        if (winTypeStarRain != null) winTypeStarRain.StopStarRain();
        StopWinTypeWheelPulse();

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

    private void StartWinTypeWheelPulse()
    {
        StopWinTypeWheelPulse();

        Transform redTr = GetRedWheelTransform();
        Transform greenTr = GetGreenWheelTransform();

        float pulseDuration = 0.45f;

        if (redTr != null)
        {
            redTr.DOKill();
            redTr.localScale = Vector3.one * 1.0f;
            redWheelPulseTween = redTr.DOScale(Vector3.one * 1.2f, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        if (greenTr != null)
        {
            greenTr.DOKill();
            greenTr.localScale = Vector3.one * 1.2f;
            greenWheelPulseTween = greenTr.DOScale(Vector3.one * 1.0f, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void StopWinTypeWheelPulse()
    {
        if (redWheelPulseTween != null)
        {
            redWheelPulseTween.Kill();
            redWheelPulseTween = null;
        }

        if (greenWheelPulseTween != null)
        {
            greenWheelPulseTween.Kill();
            greenWheelPulseTween = null;
        }

        float resetDuration = 0.3f;

        Transform redTr = GetRedWheelTransform();
        if (redTr != null)
        {
            redTr.DOKill();
            redTr.DOScale(Vector3.one, resetDuration).SetEase(Ease.OutQuad);
        }

        Transform greenTr = GetGreenWheelTransform();
        if (greenTr != null)
        {
            greenTr.DOKill();
            greenTr.DOScale(Vector3.one, resetDuration).SetEase(Ease.OutQuad);
        }
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
        gameRulesPanel.SetActive(true);
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

    internal void UpdateJackpotDisplay(JackpotValues values)
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

    internal void EnableRainbowPanel()
    {
        if (rainbowPanel != null) rainbowPanel.SetActive(true);
    }

    internal void DisableRainbowPanel()
    {
        if (rainbowPanel != null) rainbowPanel.SetActive(false);
    }
    #endregion

    #region Dynamic Game Rules Updates

    private void UpdateGameRulesDynamicTexts()
    {
        if (gameManager == null || gameManager.gameConfig == null) return;

        if (totalLineCountText != null)
        {
            totalLineCountText.text = gameManager.gameConfig.paylineCount.ToString();
        }

        SetRuleSymbolText(1, ruleRed3XText);
        SetRuleSymbolText(2, ruleBlue2XText);
        SetRuleSymbolText(3, ruleBlue7Text);
        SetRuleSymbolText(4, ruleWhite7Text);
        SetRuleSymbolText(5, ruleWhite7BarText);
        SetRuleSymbolText(6, ruleRed7Text);
        SetRuleSymbolText(7, ruleTripleBarText);
        SetRuleSymbolText(8, ruleDoubleBarText);
        SetRuleSymbolText(9, ruleSingleBarText);

        if (gameManager.gameConfig.anyPayouts != null)
        {
            var any = gameManager.gameConfig.anyPayouts;
            if (ruleAnyWildText != null) ruleAnyWildText.text = $"X{any.anyWilds.ToString("0.###")}";
            if (ruleAny7Text != null) ruleAny7Text.text = $"X{any.any7.ToString("0.###")}";
            if (ruleAnyBarText != null) ruleAnyBarText.text = $"X{any.anyBar.ToString("0.###")}";
            if (ruleAnyOneRed3XText != null) ruleAnyOneRed3XText.text = $"X{any.anyOneRed3X.ToString("0.###")}";
            if (ruleAnyOneBlue2XText != null) ruleAnyOneBlue2XText.text = $"X{any.anyOneBlue2X.ToString("0.###")}";
        }
    }

    private void SetRuleSymbolText(int symbolId, TMP_Text textComponent)
    {
        if (textComponent == null || gameManager.gameConfig.symbols == null) return;
        var symbol = gameManager.gameConfig.symbols.Find(s => s.id == symbolId);
        if (symbol != null && symbol.multipliers != null && symbol.multipliers.Count > 0)
        {
            double payout = symbol.multipliers[0];
            textComponent.text = $"X{payout.ToString("0.###")}";
        }
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

    #region Bonus Game

    internal void SetupDualWheels(DualWheelsFeature feature)
    {
        if (feature == null) return;

        var red = GetRedWheelController();
        if (red != null && feature.redWheelValues != null && feature.redWheelValues.Count > 0)
        {
            red.SetupWheelWithValues(feature.redWheelValues, 8);
        }

        var green = GetGreenWheelController();
        if (green != null && feature.greenWheelValues != null && feature.greenWheelValues.Count > 0)
        {
            green.SetupWheelWithValues(feature.greenWheelValues, 12);
        }
    }

    private WheelSpinController GetRedWheelController()
    {
        if (redWheel != null) return redWheel;
        if (gameManager != null && gameManager.redWheel != null) return gameManager.redWheel;
        return null;
    }

    private WheelSpinController GetGreenWheelController()
    {
        if (greenWheel != null) return greenWheel;
        if (gameManager != null && gameManager.greenWheel != null) return gameManager.greenWheel;
        return null;
    }

    private Transform GetRedWheelTransform()
    {
        if (redWheelTransform != null) return redWheelTransform;
        var red = GetRedWheelController();
        return red != null ? red.transform : null;
    }

    private Transform GetGreenWheelTransform()
    {
        if (greenWheelTransform != null) return greenWheelTransform;
        var green = GetGreenWheelController();
        return green != null ? green.transform : null;
    }

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

    private Transform GetSlotObjectTransform()
    {
        if (slotObject != null) return slotObject;
        var oc = GetOCController();
        if (oc != null && oc.SlotObject != null) return oc.SlotObject;
        return slotView != null ? slotView.transform : null;
    }

    private IEnumerator PlayTitleImageAnimationLoop(GameObject titleObj)
    {
        if (titleObj == null) yield break;

        titleObj.SetActive(true);

        ImageAnimation imgAnim = titleObj.GetComponent<ImageAnimation>();
        if (imgAnim != null)
        {
            bool loopFinished = false;
            System.Action<int> loopCallback = (loopCount) =>
            {
                if (loopCount >= 1) loopFinished = true;
            };

            imgAnim.onLoopComplete += loopCallback;
            imgAnim.StartAnimation();

            float duration = 1.5f;
            if (imgAnim.textureArray != null && imgAnim.textureArray.Count > 0)
            {
                if (imgAnim.useDynamicFramerate)
                {
                    duration = imgAnim.dynamicLoopDuration;
                }
                else
                {
                    float speed = imgAnim.AnimationSpeed > 0 ? imgAnim.AnimationSpeed : 1f;
                    duration = (0.0416666679f * imgAnim.textureArray.Count / speed) + imgAnim.delayBetweenLoop;
                }
            }
            if (duration < 0.2f) duration = 1.5f;

            float timer = 0f;
            while (!loopFinished && timer < (duration + 0.5f))
            {
                timer += Time.deltaTime;
                yield return null;
            }

            imgAnim.onLoopComplete -= loopCallback;
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        titleObj.SetActive(false);
    }

    private Tween AnimateWheelPopScale(Transform tr, Vector3 targetScale, Vector3 popScale, float duration)
    {
        if (tr == null) return null;
        Sequence seq = DOTween.Sequence();
        seq.Append(tr.DOScale(popScale, duration * 0.5f).SetEase(Ease.OutCubic));
        seq.Append(tr.DOScale(targetScale, duration * 0.5f).SetEase(Ease.InOutCubic));
        return seq;
    }

    private void SetWheelSiblingOrder(bool redOnTop)
    {
        Transform redTr = GetRedWheelTransform();
        Transform greenTr = GetGreenWheelTransform();

        if (redTr != null && greenTr != null)
        {
            if (redOnTop)
            {
                greenTr.SetSiblingIndex(0);
                redTr.SetSiblingIndex(1);
            }
            else
            {
                redTr.SetSiblingIndex(0);
                greenTr.SetSiblingIndex(1);
            }
        }

        if (redWheelTitleObject != null) redWheelTitleObject.transform.SetAsLastSibling();
        if (greenWheelTitleObject != null) greenWheelTitleObject.transform.SetAsLastSibling();
        if (dualWheelTitleObject != null) dualWheelTitleObject.transform.SetAsLastSibling();
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







    internal void TriggerDualWheelsBonus(DualWheelsBonusData bonusData, System.Action onComplete)
    {
        if (bonusData == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DualWheelsBonusSequence(bonusData, onComplete));
    }

    private bool wheelSpinTriggered = false;

    private void SetAllWheelSpinButtonsInteractable(bool interactable)
    {
        SetButtonInteractable(wheelSpinButton, wheelSpinButtonPortrait, interactable);
        if (redCenterSpinButton) redCenterSpinButton.interactable = interactable;
        if (greenCenterSpinButton) greenCenterSpinButton.interactable = interactable;

        var red = GetRedWheelController();
        if (red != null) red.SetCenterSpinButtonInteractable(interactable);

        var green = GetGreenWheelController();
        if (green != null) green.SetCenterSpinButtonInteractable(interactable);
    }

    private void OnWheelSpinClicked()
    {
        if (wheelSpinTriggered) return;
        wheelSpinTriggered = true;
        SetAllWheelSpinButtonsInteractable(false);
    }



    private IEnumerator DualWheelsBonusSequence(DualWheelsBonusData bonusData, System.Action onComplete)
    {
        var redCtrl = GetRedWheelController();
        var greenCtrl = GetGreenWheelController();
        Transform redTr = GetRedWheelTransform();
        Transform greenTr = GetGreenWheelTransform();
        Transform targetSlotBG = GetSlotObjectTransform();

        string wType = (bonusData.wheelType ?? "").ToLower().Trim();
        bool isRedTriggered = wType.Contains("red");
        bool isGreenTriggered = wType.Contains("green");
        bool isBothTriggered = wType.Contains("both") || wType.Contains("double") || (isRedTriggered && isGreenTriggered);

        if (redWheelTitleObject != null) redWheelTitleObject.SetActive(false);
        if (greenWheelTitleObject != null) greenWheelTitleObject.SetActive(false);
        if (dualWheelTitleObject != null) dualWheelTitleObject.SetActive(false);
        DisableWheelBackgrounds();

        StopWheelShineEffects();
        if (redCtrl != null) redCtrl.ResetWheelEffects();
        if (greenCtrl != null) greenCtrl.ResetWheelEffects();

        var oc = GetOrientationChange();
        bool isPortraitMode = (oc != null && oc.CurrentMode == OrientationChange.OrientationMode.MobilePortrait);

        Vector3 curStartScale = isPortraitMode ? wheelPortraitStartScale : wheelLandscapeStartScale;
        Vector3 curTargetScale = isPortraitMode ? wheelPortraitTargetScale : wheelLandscapeTargetScale;
        Vector3 curPopScale = isPortraitMode ? wheelPortraitPopScale : wheelLandscapePopScale;

        Vector3 initialSlotPos = targetSlotBG != null ? targetSlotBG.localPosition : (isPortraitMode ? new Vector3(0f, -368f, 0f) : new Vector3(0f, -16.5f, 0f));
        Vector3 initialRedPos = redTr != null ? redTr.localPosition : (isPortraitMode ? redWheelPortraitStartPos : redWheelLandscapeStartPos);
        Vector3 initialGreenPos = greenTr != null ? greenTr.localPosition : (isPortraitMode ? greenWheelPortraitStartPos : greenWheelLandscapeStartPos);
        Vector3 initialRedScale = redTr != null ? redTr.localScale : curStartScale;
        Vector3 initialGreenScale = greenTr != null ? greenTr.localScale : curStartScale;

        isWheelBonusActive = true;
        wheelSpinTriggered = false;
        AudioManager.Instance?.PlayFeatureBg();

        SetSpinStopButtonStates(isSpinningState: false, isInteractable: false);
        SetButtonActive(spinButton, spinButtonPortrait, false);
        SetButtonActive(autoSpinStopButton, autoSpinStopButtonPortrait, false);
        SetButtonActive(wheelSpinButton, wheelSpinButtonPortrait, true);
        SetAllWheelSpinButtonsInteractable(false);
        UpdateNewWheelSpinButtonsVisibility();

        if (slotView != null) slotView.DisableAllOverlays();

        UpdateTopBarVisibility(false);
        DisableRainbowPanel();
        if (targetSlotBG != null)
        {
            float targetY = isPortraitMode ? portraitSlotBgTargetY : slotBgTargetY;
            yield return targetSlotBG.DOLocalMoveY(targetY, slotBgMoveDuration).SetEase(Ease.InOutCubic).WaitForCompletion();
        }

        EnableWheelBackgroundBasedOnOrientation();

        StartWheelShineEffects();

        List<Tween> midMoveTweens = new List<Tween>();
        if (redTr != null)
        {
            Vector3 midRedPos = isPortraitMode ? redWheelPortraitTargetPos : new Vector3(redWheelTargetX, redWheelFinalPos.y, redWheelFinalPos.z);
            midMoveTweens.Add(redTr.DOLocalMove(midRedPos, wheelMoveDuration).SetEase(Ease.OutCubic));
            midMoveTweens.Add(redTr.DOScale(curStartScale, wheelMoveDuration).SetEase(Ease.OutCubic));
        }

        if (greenTr != null)
        {
            Vector3 midGreenPos = isPortraitMode ? greenWheelPortraitTargetPos : new Vector3(greenWheelTargetX, greenWheelFinalPos.y, greenWheelFinalPos.z);
            midMoveTweens.Add(greenTr.DOLocalMove(midGreenPos, wheelMoveDuration).SetEase(Ease.OutCubic));
            midMoveTweens.Add(greenTr.DOScale(curStartScale, wheelMoveDuration).SetEase(Ease.OutCubic));
        }

        if (midMoveTweens.Count > 0)
        {
            yield return midMoveTweens[0].WaitForCompletion();
        }

        GameObject targetTitleObj = null;
        if (isBothTriggered)
        {
            targetTitleObj = dualWheelTitleObject;
        }
        else if (isRedTriggered)
        {
            targetTitleObj = redWheelTitleObject;
        }
        else
        {
            targetTitleObj = greenWheelTitleObject;
        }

        if (targetTitleObj != null)
        {
            yield return StartCoroutine(PlayTitleImageAnimationLoop(targetTitleObj));
        }

        List<Tween> finalMoveTweens = new List<Tween>();
        Vector3 targetRedFinal = isPortraitMode ? redWheelPortraitTargetPos : redWheelFinalPos;
        Vector3 targetGreenFinal = isPortraitMode ? greenWheelPortraitTargetPos : greenWheelFinalPos;

        if (isBothTriggered)
        {
            SetWheelSiblingOrder(true);
            if (redCtrl != null) redCtrl.SetFullDisable(false);
            if (greenCtrl != null) greenCtrl.SetFullDisable(false);

            if (redTr != null)
            {
                finalMoveTweens.Add(redTr.DOLocalMove(targetRedFinal, phase2MoveDuration).SetEase(Ease.OutCubic));
                finalMoveTweens.Add(AnimateWheelPopScale(redTr, curTargetScale, curPopScale, phase2MoveDuration));
            }
            if (greenTr != null)
            {
                finalMoveTweens.Add(greenTr.DOLocalMove(targetGreenFinal, phase2MoveDuration).SetEase(Ease.OutCubic));
                finalMoveTweens.Add(AnimateWheelPopScale(greenTr, curTargetScale, curPopScale, phase2MoveDuration));
            }
        }
        else if (isRedTriggered)
        {
            SetWheelSiblingOrder(true);
            if (redCtrl != null) redCtrl.SetFullDisable(false);
            if (greenCtrl != null) greenCtrl.SetFullDisable(true);

            if (redTr != null)
            {
                finalMoveTweens.Add(redTr.DOLocalMove(targetRedFinal, phase2MoveDuration).SetEase(Ease.OutCubic));
                finalMoveTweens.Add(AnimateWheelPopScale(redTr, curTargetScale, curPopScale, phase2MoveDuration));
            }
            if (greenTr != null)
            {
                finalMoveTweens.Add(greenTr.DOLocalMove(targetGreenFinal, phase2MoveDuration).SetEase(Ease.OutCubic));
                finalMoveTweens.Add(greenTr.DOScale(curTargetScale, phase2MoveDuration).SetEase(Ease.OutCubic));
            }
        }
        else
        {
            SetWheelSiblingOrder(false);
            if (redCtrl != null) redCtrl.SetFullDisable(true);
            if (greenCtrl != null) greenCtrl.SetFullDisable(false);

            if (redTr != null)
            {
                finalMoveTweens.Add(redTr.DOLocalMove(targetRedFinal, phase2MoveDuration).SetEase(Ease.OutCubic));
                finalMoveTweens.Add(redTr.DOScale(curTargetScale, phase2MoveDuration).SetEase(Ease.OutCubic));
            }
            if (greenTr != null)
            {
                finalMoveTweens.Add(greenTr.DOLocalMove(targetGreenFinal, phase2MoveDuration).SetEase(Ease.OutCubic));
                finalMoveTweens.Add(AnimateWheelPopScale(greenTr, curTargetScale, curPopScale, phase2MoveDuration));
            }
        }

        if (finalMoveTweens.Count > 0)
        {
            yield return finalMoveTweens[0].WaitForCompletion();
        }

        SetAllWheelSpinButtonsInteractable(true);

        yield return new WaitUntil(() => wheelSpinTriggered);

        int redTargetIndex = bonusData.redWheelStopIndex >= 0 ? bonusData.redWheelStopIndex : FindSegmentIndexForValue(redCtrl, bonusData.redWheelValue);
        int greenTargetIndex = bonusData.greenWheelStopIndex >= 0 ? bonusData.greenWheelStopIndex : FindSegmentIndexForValue(greenCtrl, bonusData.greenWheelValue);


        if (isBothTriggered)
        {
            if (redCtrl != null)
            {
                bool redDone = false;
                redCtrl.SpinToIndex(redTargetIndex, () => redDone = true);
                yield return new WaitUntil(() => redDone);
                redCtrl.SetResultShine(true);
                redCtrl.SetHalfDisable(true);
                yield return new WaitForSeconds(0.5f);
            }

            if (greenCtrl != null)
            {
                bool greenDone = false;
                greenCtrl.SpinToIndex(greenTargetIndex, () => greenDone = true);
                yield return new WaitUntil(() => greenDone);
                greenCtrl.SetResultShine(true);
                greenCtrl.SetHalfDisable(true);
                yield return new WaitForSeconds(0.5f);
            }
        }
        else if (isRedTriggered)
        {
            if (redCtrl != null)
            {
                bool redDone = false;
                redCtrl.SpinToIndex(redTargetIndex, () => redDone = true);
                yield return new WaitUntil(() => redDone);
                redCtrl.SetResultShine(true);
                redCtrl.SetHalfDisable(true);
                yield return new WaitForSeconds(0.5f);
            }
        }
        else
        {
            if (greenCtrl != null)
            {
                bool greenDone = false;
                greenCtrl.SpinToIndex(greenTargetIndex, () => greenDone = true);
                yield return new WaitUntil(() => greenDone);
                greenCtrl.SetResultShine(true);
                greenCtrl.SetHalfDisable(true);
                yield return new WaitForSeconds(0.5f);
            }
        }

        yield return new WaitForSeconds(wheelResultPopupDelay);

        yield return StartCoroutine(ShowWheelResultPopupRoutine(bonusData, isRedTriggered, isGreenTriggered, isBothTriggered, null));

        if (gameManager != null && gameManager.playerData != null)
        {
            double prevBalance = gameManager.playerData.balance;
            gameManager.playerData.balance += bonusData.totalWinAmount;

            double targetWin = (gameManager.lastResult != null && gameManager.lastResult.grandTotalWin > 0)
                ? gameManager.lastResult.grandTotalWin
                : (gameManager.lastResult != null ? gameManager.lastResult.winAmount : bonusData.totalWinAmount);

            AnimateWinUpdate(targetWin);
            AnimateBalanceUpdate(gameManager.playerData.balance, prevBalance);
        }

        DisableWheelBackgrounds();
        StopWheelShineEffects();
        if (redCtrl != null) redCtrl.ResetWheelEffects();
        if (greenCtrl != null) greenCtrl.ResetWheelEffects();
        if (redWheelTitleObject != null) redWheelTitleObject.SetActive(false);
        if (greenWheelTitleObject != null) greenWheelTitleObject.SetActive(false);
        if (dualWheelTitleObject != null) dualWheelTitleObject.SetActive(false);
        UpdateTopBarVisibility(true);

        if (slotView != null) slotView.DisableAllOverlays();

        List<Tween> restoreTweens = new List<Tween>();

        if (targetSlotBG != null)
        {
            targetSlotBG.DOKill();
            restoreTweens.Add(targetSlotBG.DOLocalMove(initialSlotPos, slotBgMoveDuration).SetEase(Ease.InOutCubic));
        }

        if (redTr != null)
        {
            redTr.DOKill();
            restoreTweens.Add(redTr.DOLocalMove(initialRedPos, wheelMoveDuration).SetEase(Ease.OutCubic));
            restoreTweens.Add(redTr.DOScale(initialRedScale, wheelMoveDuration).SetEase(Ease.OutCubic));
        }

        if (greenTr != null)
        {
            greenTr.DOKill();
            restoreTweens.Add(greenTr.DOLocalMove(initialGreenPos, wheelMoveDuration).SetEase(Ease.OutCubic));
            restoreTweens.Add(greenTr.DOScale(initialGreenScale, wheelMoveDuration).SetEase(Ease.OutCubic));
        }

        if (restoreTweens.Count > 0)
        {
            yield return restoreTweens[0].WaitForCompletion();
        }

        double totalBetVal = gameManager != null ? gameManager.currentBetAmount : 0.01;
        double totalWinVal = (gameManager != null && gameManager.lastResult != null && gameManager.lastResult.grandTotalWin > 0)
            ? gameManager.lastResult.grandTotalWin
            : (bonusData != null ? bonusData.totalWinAmount : 0);

        if (totalWinVal / totalBetVal >= bigWinThreshold)
        {
            bool winTypeDone = false;
            TriggerWinTypePopup(totalWinVal, totalBetVal, () => winTypeDone = true);
            yield return new WaitUntil(() => winTypeDone);
        }

        isWheelBonusActive = false;
        SetButtonActive(wheelSpinButton, wheelSpinButtonPortrait, false);
        if (gameManager != null && gameManager.isAutoPlaying)
        {
            OnAutoPlayStarted();
        }
        else
        {
            SetSpinStopButtonStates(isSpinningState: false, isInteractable: true);
            SetButtonActive(spinButton, spinButtonPortrait, true);
        }

        AudioManager.Instance?.StopFeatureBg();
        onComplete?.Invoke();
    }

    private int FindSegmentIndexForValue(WheelSpinController wheel, double targetValue)
    {
        if (wheel == null || wheel.SegmentDataList == null || wheel.SegmentDataList.Count == 0) return 0;

        for (int i = 0; i < wheel.SegmentDataList.Count; i++)
        {
            if (System.Math.Abs(wheel.SegmentDataList[i].assignedValue - targetValue) < 0.01)
            {
                return i;
            }
        }
        return 0;
    }

    private void FindWheelResultPopupReferences()
    {
        Transform parentTr = wheelParent;
        if (parentTr == null) return;

        if (wheelResultPopup == null)
        {
            Transform foundBG = parentTr.Find("ResultBG");
            if (foundBG == null)
            {
                foreach (Transform child in parentTr.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "ResultBG") { foundBG = child; break; }
                }
            }
            if (foundBG != null) wheelResultPopup = foundBG.gameObject;
        }

        if (wheelResultPopup != null)
        {
            if (wheelResultPopupRect == null) wheelResultPopupRect = wheelResultPopup.GetComponent<RectTransform>();

            Transform bgTr = wheelResultPopup.transform;

            if (wheelResultRedDummy == null)
            {
                Transform t = bgTr.Find("RedDummy");
                if (t != null) wheelResultRedDummy = t.gameObject;
            }

            if (wheelResultGreenDummy == null)
            {
                Transform t = bgTr.Find("GreenDummy");
                if (t != null) wheelResultGreenDummy = t.gameObject;
            }

            if (wheelResultAreaPanel == null)
            {
                Transform t = bgTr.Find("ResultArea");
                if (t != null) wheelResultAreaPanel = t.GetComponent<RectTransform>();
            }

            if (wheelResultAreaPanel != null)
            {
                if (wheelResultNumbersArea == null)
                {
                    Transform t = wheelResultAreaPanel.Find("NumbersArea");
                    if (t != null) wheelResultNumbersArea = t.GetComponent<RectTransform>();
                }

                if (wheelResultWinAmountText == null)
                {
                    Transform t = wheelResultAreaPanel.Find("FeatureWinText");
                    if (t != null) wheelResultWinAmountText = t.GetComponent<TMP_Text>();
                }
            }

            if (wheelResultNumbersArea != null)
            {
                TMP_Text[] texts = wheelResultNumbersArea.GetComponentsInChildren<TMP_Text>(true);
                foreach (var txt in texts)
                {
                    string nameLower = txt.gameObject.name.ToLower();
                    Transform parent = txt.transform.parent;
                    string pName = parent != null ? parent.gameObject.name.ToLower() : "";

                    if (nameLower.Contains("bet") || pName.Contains("bet"))
                    {
                        if (wheelResultBetText == null || nameLower.Contains("value")) wheelResultBetText = txt;
                    }
                    else if (nameLower.Contains("red") || pName.Contains("red"))
                    {
                        if (wheelResultRedMultiplierText == null) wheelResultRedMultiplierText = txt;
                    }
                    else if (nameLower.Contains("green") || pName.Contains("green"))
                    {
                        if (wheelResultGreenMultiplierText == null) wheelResultGreenMultiplierText = txt;
                    }
                }
            }
        }
    }

    private IEnumerator OpenWheelResultPopupRoutine(DualWheelsBonusData bonusData, bool isRed, bool isGreen, bool isBoth)
    {
        FindWheelResultPopupReferences();

        if (wheelResultPopup == null)
        {
            yield break;
        }

        if (wheelResultRedDummy != null)
        {
            wheelResultRedDummy.SetActive(true);
            RectTransform rTr = wheelResultRedDummy.GetComponent<RectTransform>();
            if (rTr != null) rTr.anchoredPosition = new Vector2(-100f, 87f);
        }

        if (wheelResultGreenDummy != null)
        {
            wheelResultGreenDummy.SetActive(true);
            RectTransform gTr = wheelResultGreenDummy.GetComponent<RectTransform>();
            if (gTr != null) gTr.anchoredPosition = new Vector2(100f, 117f);
        }

        if (wheelResultBetRowObject != null) wheelResultBetRowObject.SetActive(true);

        if (wheelResultRedRowObject != null) wheelResultRedRowObject.SetActive(isRed || isBoth);
        if (wheelResultGreenRowObject != null) wheelResultGreenRowObject.SetActive(isGreen || isBoth);

        if (wheelResultNumbersLayoutGroup == null && wheelResultNumbersArea != null)
        {
            wheelResultNumbersLayoutGroup = wheelResultNumbersArea.GetComponent<VerticalLayoutGroup>();
        }

        if (wheelResultNumbersLayoutGroup != null)
        {
            wheelResultNumbersLayoutGroup.spacing = isBoth ? 0f : -95f;
            if (wheelResultNumbersArea != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(wheelResultNumbersArea);
            }
        }

        if (wheelResultFirstXObject != null) wheelResultFirstXObject.SetActive(true);
        if (wheelResultSecondXObject != null) wheelResultSecondXObject.SetActive(isBoth);

        double bet = gameManager != null ? gameManager.currentBetAmount : 0.01;
        if (wheelResultBetText != null) wheelResultBetText.text = SlotView.FormatSpriteText(bet.ToString("F2"));

        if (wheelResultRedMultiplierText != null) wheelResultRedMultiplierText.text = "";
        if (wheelResultGreenMultiplierText != null) wheelResultGreenMultiplierText.text = "";
        if (wheelResultWinAmountText != null) wheelResultWinAmountText.text = "";

        CanvasGroup cg = wheelResultPopup.GetComponent<CanvasGroup>();
        if (cg == null) cg = wheelResultPopup.AddComponent<CanvasGroup>();

        cg.DOKill();
        cg.alpha = 0f;

        if (wheelResultPopupRect == null && wheelResultPopup != null)
        {
            wheelResultPopupRect = wheelResultPopup.GetComponent<RectTransform>();
        }

        Vector2 finalPos = new Vector2(0f, 63f);
        if (wheelResultPopupRect != null)
        {
            wheelResultPopupRect.DOKill();
            wheelResultPopupRect.anchoredPosition = Vector2.zero;
            wheelResultPopupRect.localScale = Vector3.zero;
        }

        if (wheelResultRedDummy != null)
        {
            Transform tr = wheelResultRedDummy.transform;
            tr.DOKill();
            tr.localScale = Vector3.zero;
        }

        if (wheelResultGreenDummy != null)
        {
            Transform tr = wheelResultGreenDummy.transform;
            tr.DOKill();
            tr.localScale = Vector3.zero;
        }

        if (wheelResultAreaPanel != null)
        {
            wheelResultAreaPanel.DOKill();
            wheelResultAreaPanel.localScale = Vector3.zero;
        }

        wheelResultPopup.SetActive(true);
        AudioManager.Instance?.PlayResultPopupOpen();

        bool openDone = false;
        cg.DOFade(1f, 0.25f).SetEase(Ease.OutQuad);

        if (wheelResultPopupRect != null)
        {
            wheelResultPopupRect.DOAnchorPos(finalPos, wheelPopupOpenDuration).SetEase(Ease.OutBack);
            wheelResultPopupRect.DOScale(Vector3.one, wheelPopupOpenDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => openDone = true);
        }
        else
        {
            openDone = true;
        }

        StartCoroutine(StaggeredDummyWheelPopBounce());

        StartCoroutine(StaggeredResultAreaPopBounce(isBoth));

        yield return new WaitUntil(() => openDone);

        yield return new WaitForSeconds(0.15f);

        if (isBoth)
        {
            if (wheelResultRedMultiplierText != null)
                wheelResultRedMultiplierText.text = SlotView.FormatSpriteText(bonusData.redWheelValue.ToString("F0"));

            yield return new WaitForSeconds(0.25f);

            if (wheelResultGreenMultiplierText != null)
                wheelResultGreenMultiplierText.text = SlotView.FormatSpriteText(bonusData.greenWheelValue.ToString("F0"));
        }
        else if (isRed)
        {
            if (wheelResultRedMultiplierText != null)
                wheelResultRedMultiplierText.text = SlotView.FormatSpriteText(bonusData.redWheelValue.ToString("F0"));
        }
        else
        {
            if (wheelResultGreenMultiplierText != null)
                wheelResultGreenMultiplierText.text = SlotView.FormatSpriteText(bonusData.greenWheelValue.ToString("F0"));
        }

        yield return new WaitForSeconds(0.15f);

        if (wheelResultWinAmountText != null && bonusData.totalWinAmount > 0)
        {
            wheelResultWinAmountText.text = SlotView.FormatSpriteText("0.00");
            bool countDone = false;

            DOVirtual.Float(0f, (float)bonusData.totalWinAmount, 1.0f, (val) =>
            {
                if (wheelResultWinAmountText != null)
                {
                    wheelResultWinAmountText.text = SlotView.FormatSpriteText(val.ToString("N2"));
                }
            }).OnComplete(() =>
            {
                if (wheelResultWinAmountText != null)
                {
                    wheelResultWinAmountText.text = SlotView.FormatSpriteText(bonusData.totalWinAmount.ToString("N2"));
                }
                countDone = true;
            });

            yield return new WaitUntil(() => countDone);
        }
        else if (wheelResultWinAmountText != null)
        {
            wheelResultWinAmountText.text = SlotView.FormatSpriteText(bonusData.totalWinAmount.ToString("N2"));
        }
    }

    private IEnumerator StaggeredDummyWheelPopBounce()
    {
        if (wheelResultRedDummy != null)
        {
            yield return new WaitForSeconds(0.08f);
            Transform tr = wheelResultRedDummy.transform;
            tr.DOKill();
            tr.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }

        if (wheelResultGreenDummy != null)
        {
            yield return new WaitForSeconds(0.08f);
            Transform tr = wheelResultGreenDummy.transform;
            tr.DOKill();
            tr.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }
    }

    private IEnumerator StaggeredResultAreaPopBounce(bool isBoth)
    {
        float delay = isBoth ? 0.24f : 0.18f;
        yield return new WaitForSeconds(delay);

        if (wheelResultAreaPanel != null)
        {
            wheelResultAreaPanel.DOKill();
            wheelResultAreaPanel.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }
    }

    private IEnumerator CloseWheelResultPopupRoutine()
    {
        AudioManager.Instance?.PlayPopupClose();

        CanvasGroup cg = wheelResultPopup != null ? wheelResultPopup.GetComponent<CanvasGroup>() : null;
        if (cg != null)
        {
            cg.DOKill();
            cg.DOFade(0f, wheelPopupCloseDuration).SetEase(Ease.InQuad);
        }

        bool closeDone = false;
        if (wheelResultPopupRect != null)
        {
            wheelResultPopupRect.DOKill();
            wheelResultPopupRect.DOAnchorPos(Vector2.zero, wheelPopupCloseDuration).SetEase(Ease.InBack);
            wheelResultPopupRect.DOScale(Vector3.zero, wheelPopupCloseDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (wheelResultPopup != null) wheelResultPopup.SetActive(false);
                    closeDone = true;
                });
        }
        else
        {
            if (wheelResultPopup != null) wheelResultPopup.SetActive(false);
            closeDone = true;
        }

        yield return new WaitUntil(() => closeDone);
    }

    private IEnumerator ShowWheelResultPopupRoutine(DualWheelsBonusData bonusData, bool isRed, bool isGreen, bool isBoth, System.Action onTakePressed)
    {
        SetButtonActive(uwpTakeButton, uwpTakeButtonPortrait, true);
        SetButtonInteractable(uwpTakeButton, uwpTakeButtonPortrait, false);

        yield return StartCoroutine(OpenWheelResultPopupRoutine(bonusData, isRed, isGreen, isBoth));

        SetButtonInteractable(uwpTakeButton, uwpTakeButtonPortrait, true);

        bool takePressed = false;
        UnityEngine.Events.UnityAction clickAction = () => takePressed = true;

        if (uwpTakeButton != null) uwpTakeButton.onClick.AddListener(clickAction);
        if (uwpTakeButtonPortrait != null) uwpTakeButtonPortrait.onClick.AddListener(clickAction);

        while (!takePressed)
        {
            yield return null;
        }

        StopWheelShineEffects();

        if (uwpTakeButton != null) uwpTakeButton.onClick.RemoveListener(clickAction);
        if (uwpTakeButtonPortrait != null) uwpTakeButtonPortrait.onClick.RemoveListener(clickAction);

        yield return StartCoroutine(CloseWheelResultPopupRoutine());

        SetButtonActive(uwpTakeButton, uwpTakeButtonPortrait, false);

        onTakePressed?.Invoke();
    }

    #region Shine Effect Methods

    public void StartWheelShineEffects()
    {
        if (redWheelShineEffect != null) redWheelShineEffect.PlayLoop();
        if (greenWheelShineEffect != null) greenWheelShineEffect.PlayLoop();
    }

    public void StopWheelShineEffects()
    {
        if (redWheelShineEffect != null) redWheelShineEffect.Stop();
        if (greenWheelShineEffect != null) greenWheelShineEffect.Stop();
    }

    #endregion

    #endregion
}
