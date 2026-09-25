using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class OCController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OrientationChange orientationChange;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private Transform slotObject;
    internal Transform SlotObject => slotObject;
    [SerializeField] private List<RectTransform> resizedObjects = new List<RectTransform>();
    [SerializeField] private List<RectTransform> squareResizedObjects = new List<RectTransform>();

    [Header("Panel Toggle Settings")]
    [SerializeField] private GameObject landscapePanelObject;
    [SerializeField] private GameObject portraitPanelObject;

    [Header("Background Toggle Settings")]
    [SerializeField] private GameObject landscapeBackground;
    [SerializeField] private GameObject portraitBackground;

    [Header("Canvas Scaler Resolutions")]
    [SerializeField] private Vector2 landscapeReferenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080f, 1920f);

    [Header("Resized Object Dimensions")]
    [SerializeField] private Vector2 landscapeResizedObjectSize = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 portraitResizedObjectSize = new Vector2(1080f, 1920f);

    [Header("Square Resized Object Dimensions")]
    [SerializeField] private Vector2 landscapeSquareResizedObjectSize = new Vector2(1920f, 1080f);
    [SerializeField] private Vector2 portraitSquareResizedObjectSize = new Vector2(1920f, 1920f);

    [Header("Slot Object Settings")]
    [SerializeField] private Vector3 landscapeSlotScale = Vector3.one;
    [SerializeField] private Vector3 portraitSlotScale = new Vector3(0.73f, 0.73f, 0.73f);
    [SerializeField] private Vector3 landscapeSlotPosition = new Vector3(0f, -16.5f, 0f);
    [SerializeField] private Vector3 portraitSlotPosition = new Vector3(0f, -368f, 0f);
    internal Vector3 PortraitSlotPosition => portraitSlotPosition;
    internal Vector3 LandscapeSlotPosition => landscapeSlotPosition;

    [Header("Logo Object Settings")]
    [SerializeField] private RectTransform logoObject;
    [SerializeField] private Vector3 landscapeLogoScale = Vector3.one;
    [SerializeField] private Vector3 portraitLogoScale = new Vector3(1.27f, 1.27f, 1.27f);
    [SerializeField] private Vector2 landscapeLogoPosition = new Vector2(0f, 355f);
    [SerializeField] private Vector2 portraitLogoPosition = new Vector2(0f, 500f);

    [Header("Info Page & Guide Settings")]
    [SerializeField] private RectTransform infoPageScrollObject;
    [SerializeField] private RectTransform guideScrollObject;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.2f;

    private List<Tween> activeTweens = new List<Tween>();

    private void Awake()
    {
        if (orientationChange == null)
        {
            orientationChange = GetComponent<OrientationChange>();
            if (orientationChange == null)
            {
                orientationChange = Object.FindFirstObjectByType<OrientationChange>();
            }
        }
        if (canvasScaler == null && orientationChange != null)
        {
            canvasScaler = orientationChange.GetComponent<CanvasScaler>();
        }
    }

    private void OnEnable()
    {
        OrientationChange.OnOrientationChanged += HandleOrientationChange;
        if (orientationChange != null)
        {
            orientationChange.OnOrientationChangedInstance += HandleOrientationChange;
        }
    }

    private void OnDisable()
    {
        OrientationChange.OnOrientationChanged -= HandleOrientationChange;
        if (orientationChange != null)
        {
            orientationChange.OnOrientationChangedInstance -= HandleOrientationChange;
        }
    }

    private void HandleOrientationChange(OrientationChange.OrientationMode mode, int width, int height)
    {
        KillActiveTweens();

        bool isMobilePortrait = (mode == OrientationChange.OrientationMode.MobilePortrait);

        if (landscapePanelObject != null)
        {
            landscapePanelObject.SetActive(!isMobilePortrait);
        }
        if (portraitPanelObject != null)
        {
            portraitPanelObject.SetActive(isMobilePortrait);
        }

        if (landscapeBackground != null)
        {
            landscapeBackground.SetActive(!isMobilePortrait);
        }
        if (portraitBackground != null)
        {
            portraitBackground.SetActive(isMobilePortrait);
        }

        if (canvasScaler != null)
        {
            Vector2 targetRefRes = isMobilePortrait ? portraitReferenceResolution : landscapeReferenceResolution;
            canvasScaler.referenceResolution = targetRefRes;
        }

        Vector2 targetSize = isMobilePortrait ? portraitResizedObjectSize : landscapeResizedObjectSize;
        if (resizedObjects != null)
        {
            foreach (var rect in resizedObjects)
            {
                if (rect != null)
                {
                    if (transitionDuration > 0)
                    {
                        Tween t = rect.DOSizeDelta(targetSize, transitionDuration).SetEase(Ease.OutCubic);
                        activeTweens.Add(t);
                    }
                    else
                    {
                        rect.sizeDelta = targetSize;
                    }
                }
            }
        }

        Vector2 targetSquareSize = isMobilePortrait ? portraitSquareResizedObjectSize : landscapeSquareResizedObjectSize;
        if (squareResizedObjects != null)
        {
            foreach (var rect in squareResizedObjects)
            {
                if (rect != null)
                {
                    if (transitionDuration > 0)
                    {
                        Tween t = rect.DOSizeDelta(targetSquareSize, transitionDuration).SetEase(Ease.OutCubic);
                        activeTweens.Add(t);
                    }
                    else
                    {
                        rect.sizeDelta = targetSquareSize;
                    }
                }
            }
        }

        if (slotObject != null)
        {
            Vector3 targetScale = isMobilePortrait ? portraitSlotScale : landscapeSlotScale;
            Vector3 targetPosition = isMobilePortrait ? portraitSlotPosition : landscapeSlotPosition;

            if (transitionDuration > 0)
            {
                Tween scaleTween = slotObject.DOScale(targetScale, transitionDuration).SetEase(Ease.OutCubic);
                Tween posTween = slotObject.DOLocalMove(targetPosition, transitionDuration).SetEase(Ease.OutCubic);
                activeTweens.Add(scaleTween);
                activeTweens.Add(posTween);
            }
            else
            {
                slotObject.localScale = targetScale;
                slotObject.localPosition = targetPosition;
            }
        }

        if (logoObject != null)
        {
            Vector3 targetScale = isMobilePortrait ? portraitLogoScale : landscapeLogoScale;
            Vector2 targetPosition = isMobilePortrait ? portraitLogoPosition : landscapeLogoPosition;

            if (transitionDuration > 0)
            {
                Tween scaleTween = logoObject.DOScale(targetScale, transitionDuration).SetEase(Ease.OutCubic);
                Tween posTween = logoObject.DOAnchorPos(targetPosition, transitionDuration).SetEase(Ease.OutCubic);
                activeTweens.Add(scaleTween);
                activeTweens.Add(posTween);
            }
            else
            {
                logoObject.localScale = targetScale;
                logoObject.anchoredPosition = targetPosition;
            }
        }

        if (infoPageScrollObject != null)
        {
            float targetHeight = isMobilePortrait ? 1920f : 1080f;
            Vector2 targetScrollSize = new Vector2(infoPageScrollObject.sizeDelta.x, targetHeight);
            if (transitionDuration > 0)
            {
                Tween scrollTween = infoPageScrollObject.DOSizeDelta(targetScrollSize, transitionDuration).SetEase(Ease.OutCubic);
                activeTweens.Add(scrollTween);
            }
            else
            {
                infoPageScrollObject.sizeDelta = targetScrollSize;
            }
        }

        if (guideScrollObject != null)
        {
            float targetHeight = isMobilePortrait ? 1920f : 1080f;
            Vector2 targetScrollSize = new Vector2(guideScrollObject.sizeDelta.x, targetHeight);
            if (transitionDuration > 0)
            {
                Tween scrollTween = guideScrollObject.DOSizeDelta(targetScrollSize, transitionDuration).SetEase(Ease.OutCubic);
                activeTweens.Add(scrollTween);
            }
            else
            {
                guideScrollObject.sizeDelta = targetScrollSize;
            }
        }
    }

    private void KillActiveTweens()
    {
        foreach (var t in activeTweens)
        {
            if (t != null && t.IsActive())
            {
                t.Kill();
            }
        }
        activeTweens.Clear();
    }
}
