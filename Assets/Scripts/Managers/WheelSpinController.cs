using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using TMPro;

[Serializable]
public class WheelSegmentData
{
    public TextMeshProUGUI valueText;
    [HideInInspector] public double assignedValue;
    public int serverIndex = -1;
}

public class WheelSpinController : MonoBehaviour
{
    [Header("Wheel Configuration")]
    [SerializeField] private RectTransform wheelRect;
    [SerializeField] private RectTransform arrowRect;

    [Header("Segments (CLOCKWISE ORDER)")]
    [SerializeField] private List<WheelSegmentData> segments = new List<WheelSegmentData>();

    [Header("Angle Settings")]
    [Tooltip("Angle where segment[0] center is located. 90 = Top.")]
    [SerializeField] private float startOffsetAngle = 90f;

    [Header("Text Alignment")]
    [SerializeField] private float textRadius = 250f;
    [SerializeField] private bool faceOutward = false;

    [Header("Spin Settings")]
    [SerializeField] private float spinDuration = 5f;
    [SerializeField] private int extraSpins = 4;
    [SerializeField] private Ease spinEase = Ease.OutCubic;
    [Tooltip("Fine-tune the landing position. Positive shifts clockwise.")]
    [SerializeField] private float alignmentOffset = 0f;

    [Header("Visual Effects & Overlays")]
    [SerializeField] private GameObject fullDisableObject;
    [SerializeField] private GameObject halfDisableObject;
    [SerializeField] private GameObject resultShineObject;
    [SerializeField] private Button centerSpinButton;

    [Header("Wheel Border Animation")]
    [SerializeField] private Image borderImage;
    [SerializeField] private Sprite normalBorderSprite;
    [SerializeField] private Sprite spinBorderSprite1;
    [SerializeField] private Sprite spinBorderSprite2;
    [SerializeField] private float borderSpriteToggleInterval = 0.12f;

    private Coroutine borderAnimationCoroutine;
    private float segmentAngle;
    private bool isSpinning;
    private int currentTargetIndex = -1;

    internal bool IsSpinning => isSpinning;
    internal List<WheelSegmentData> SegmentDataList => segments;
    public Button CenterSpinButton => centerSpinButton;

    public void SetFullDisable(bool active)
    {
        if (fullDisableObject != null) fullDisableObject.SetActive(active);
    }

    public void SetHalfDisable(bool active)
    {
        if (halfDisableObject != null) halfDisableObject.SetActive(active);
        if (active)
        {
            StopBorderAnimation();
        }
    }

    public void SetResultShine(bool active)
    {
        if (resultShineObject != null) resultShineObject.SetActive(active);
        if (active) AudioManager.Instance?.PlayWheelStop();
    }

    public void SetCenterSpinButtonInteractable(bool interactable)
    {
        if (centerSpinButton != null) centerSpinButton.interactable = interactable;
    }

    public void StartBorderAnimation()
    {
        StopBorderAnimation();
        if (borderImage != null && spinBorderSprite1 != null && spinBorderSprite2 != null)
        {
            borderAnimationCoroutine = StartCoroutine(BorderSpriteLoopRoutine());
        }
    }

    public void StopBorderAnimation()
    {
        if (borderAnimationCoroutine != null)
        {
            StopCoroutine(borderAnimationCoroutine);
            borderAnimationCoroutine = null;
        }
        if (borderImage != null && normalBorderSprite != null)
        {
            borderImage.sprite = normalBorderSprite;
        }
    }

    private IEnumerator BorderSpriteLoopRoutine()
    {
        if (borderImage == null || spinBorderSprite1 == null || spinBorderSprite2 == null) yield break;

        bool useFirst = true;
        while (true)
        {
            borderImage.sprite = useFirst ? spinBorderSprite1 : spinBorderSprite2;
            useFirst = !useFirst;
            yield return new WaitForSeconds(borderSpriteToggleInterval);
        }
    }

    public void ResetWheelEffects()
    {
        if (fullDisableObject != null) fullDisableObject.SetActive(false);
        if (halfDisableObject != null) halfDisableObject.SetActive(false);
        if (resultShineObject != null) resultShineObject.SetActive(false);
        SetCenterSpinButtonInteractable(false);
        StopBorderAnimation();
    }

    private void Awake()
    {
        Initialize(segments.Count);
        ResetWheelEffects();
    }

    internal void Initialize(int segmentCount)
    {
        if (segmentCount <= 0) return;
        segmentAngle = 360f / segmentCount;
    }

    public void SetupWheelWithValues(List<double> values, int targetSegmentCount)
    {
        if (values == null || values.Count == 0 || targetSegmentCount <= 0) return;

        Initialize(targetSegmentCount);

        if (segments == null) segments = new List<WheelSegmentData>();

        for (int i = 0; i < segments.Count; i++)
        {
            var localSeg = segments[i];
            localSeg.serverIndex = i;
            localSeg.assignedValue = values[i % values.Count];
        }

        SetupSegmentTexts();
    }

    [ContextMenu("Setup Segment Texts")]
    public void SetupSegmentTexts()
    {
        if (segments == null || segments.Count == 0) return;
        
        float angleStep = 360f / segments.Count;
        
        for (int i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            if (segment.valueText == null) continue;
            
            string valStr = "";
            if (segment.assignedValue > 0)
            {
                valStr = "X" + segment.assignedValue.ToString();
            }
            else
            {
                string currentText = segment.valueText.text != null ? segment.valueText.text.Replace("\n", "").Replace("X", "").Trim() : "";
                if (string.IsNullOrEmpty(currentText) || currentText.Contains("<")) currentText = "20";
                valStr = "X" + currentText;
            }
            
            segment.valueText.text = valStr;
            
            float angle = startOffsetAngle - (i * angleStep) - alignmentOffset;
            float rad = angle * Mathf.Deg2Rad;
            
            Vector3 localDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
            segment.valueText.rectTransform.localPosition = localDir * textRadius;
            
            float zRot = faceOutward ? angle : (angle + 180f);
            segment.valueText.rectTransform.localRotation = Quaternion.Euler(0, 0, zRot);
        }
    }

    internal void SpinToIndex(int targetIndex, Action onComplete = null)
    {
        if (isSpinning) return;
        StartCoroutine(SpinRoutine(targetIndex, onComplete));
    }

    private IEnumerator SpinRoutine(int targetIndex, Action onComplete)
    {
        isSpinning = true;
        currentTargetIndex = targetIndex;
        AudioManager.Instance?.PlayWheelSpinBg();
        StartBorderAnimation();

        float targetSegmentAngle = (targetIndex * segmentAngle) + alignmentOffset;
        float wheelLocalAngle = startOffsetAngle - targetSegmentAngle;
        
        Transform parent = wheelRect.parent;
        Vector3 worldWinningDir = (arrowRect.position - wheelRect.position).normalized;
        Vector3 localWinningDir = parent != null ? parent.InverseTransformDirection(worldWinningDir) : worldWinningDir;
        
        if (localWinningDir.sqrMagnitude < 0.1f) localWinningDir = Vector3.right;
        
        float arrowAngle = Mathf.Atan2(localWinningDir.y, localWinningDir.x) * Mathf.Rad2Deg;
        
        float finalTargetLocalRotation = arrowAngle - wheelLocalAngle;

        float currentLocalRotation = wheelRect.localEulerAngles.z;
        
        float targetAbs = finalTargetLocalRotation;
        while (targetAbs > currentLocalRotation) targetAbs -= 360f;
        
        float totalRotation = targetAbs - (extraSpins * 360f);


        wheelRect.DORotate(
            new Vector3(0, 0, totalRotation),
            spinDuration,
            RotateMode.FastBeyond360
        ).SetEase(spinEase);

        yield return new WaitForSeconds(spinDuration);
        AudioManager.Instance?.StopWheelSpinBg();

        wheelRect.localRotation = Quaternion.Euler(0, 0, finalTargetLocalRotation);

        isSpinning = false;
        StopBorderAnimation();
        onComplete?.Invoke();
    }



    private void OnDrawGizmos()
    {
        DrawWheelGizmos(false);
    }

    private void OnDrawGizmosSelected()
    {
        DrawWheelGizmos(true);
    }

    private void DrawWheelGizmos(bool selected)
    {
        if (wheelRect == null) return;
        
        int count = (segments != null && segments.Count > 0) ? segments.Count : 18;
        float angleStep = 360f / count;
        Vector3 center = wheelRect.position;
        
        float radius = (wheelRect.rect.width > 0) ? (wheelRect.rect.width * 0.5f * wheelRect.lossyScale.x) : 100f;

        Gizmos.color = selected ? Color.white : new Color(1, 1, 1, 0.2f);
        Gizmos.DrawWireSphere(center, radius * 0.05f);

        for (int i = 0; i < count; i++)
        {
            float angle = startOffsetAngle - (i * angleStep) - alignmentOffset;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 localDir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
            Vector3 worldDir = wheelRect.rotation * localDir;
            
            if (selected)
            {
                if (currentTargetIndex != -1 && i == currentTargetIndex)
                    Gizmos.color = Color.magenta;
                else
                    Gizmos.color = (i == 0) ? Color.green : Color.red;

                Gizmos.DrawLine(center, center + worldDir * radius);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(center + worldDir * radius * 1.05f, i.ToString());
#endif
            }
            else
            {
                Gizmos.color = new Color(1, 1, 1, 0.1f);
                Gizmos.DrawLine(center, center + worldDir * radius * 0.5f);
            }
        }

        if (arrowRect != null)
        {
            Gizmos.color = selected ? Color.yellow : new Color(1, 0.92f, 0.016f, 0.3f);
            Vector3 arrowPos = arrowRect.position;
            Vector3 worldWinningDir = (arrowPos - wheelRect.position).normalized;
            Vector3 endPos = wheelRect.position + worldWinningDir * radius;
            
            Gizmos.DrawLine(wheelRect.position, endPos);
            
            if (selected)
            {
                float headSize = radius * 0.05f;
                Vector3 right = Vector3.Cross(worldWinningDir, Vector3.forward).normalized;
                Vector3 headLeft = endPos - worldWinningDir * headSize + right * headSize * 0.5f;
                Vector3 headRight = endPos - worldWinningDir * headSize - right * headSize * 0.5f;
                
                Gizmos.DrawLine(endPos, headLeft);
                Gizmos.DrawLine(endPos, headRight);
                Gizmos.DrawLine(headLeft, headRight);
            }
        }
    }
}
