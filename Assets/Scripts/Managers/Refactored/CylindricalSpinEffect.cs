using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CylindricalSpinEffect : MonoBehaviour
{
    [Header("Settings")]
    public bool enableCylindricalEffect = true;
    public float visibleHalfHeight = 310f;
    public float outerHalfHeight = 450f;
    public float edgeScale = 0.88f;
    public float outerScale = 0.70f;

    [Header("3D Barrel / Cylinder Curve")]
    [Tooltip("Maximum X tilt angle (in degrees) when symbol reaches the top/bottom curve.")]
    [Range(0f, 60f)]
    public float maxTiltAngle = 35f;

    [Tooltip("Minimum Y squash scale at the curve edges.")]
    [Range(0.2f, 1f)]
    public float minSquashY = 0.65f;

    [Header("References")]
    public RectTransform visibleAreaRectTransform;
    public Transform[] reelTransforms;
    
    private List<ReelImages> reelImagesList;
    private float[] reelCurveIntensity;
    
    private Coroutine effectCoroutine;
    private System.Func<bool> isSpinningCheck;

    public void Initialize(List<ReelImages> reels, float[] intensities, System.Func<bool> checkSpinning)
    {
        reelImagesList = reels;
        reelCurveIntensity = intensities;
        isSpinningCheck = checkSpinning;
        UpdateCylindricalSpinEffect(force: true);
    }

    public void Initialize(List<ReelImages> reels, Transform[] transforms, RectTransform visibleArea, float[] intensities, System.Func<bool> checkSpinning)
    {
        reelImagesList = reels;
        if (transforms != null && transforms.Length > 0) reelTransforms = transforms;
        if (visibleArea != null) visibleAreaRectTransform = visibleArea;
        reelCurveIntensity = intensities;
        isSpinningCheck = checkSpinning;
        UpdateCylindricalSpinEffect(force: true);
    }

    public void StartEffect()
    {
        if (!enableCylindricalEffect) return;
        
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
        }
        effectCoroutine = StartCoroutine(EffectRoutine());
    }

    public void StopEffect()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }
        UpdateCylindricalSpinEffect(force: true);
    }

    private IEnumerator EffectRoutine()
    {
        while (isSpinningCheck != null && isSpinningCheck.Invoke())
        {
            UpdateCylindricalSpinEffect(force: false);
            yield return null;
        }
        UpdateCylindricalSpinEffect(force: true);
        effectCoroutine = null;
    }

    private void LateUpdate()
    {
        if (isSpinningCheck != null && isSpinningCheck.Invoke())
        {
            UpdateCylindricalSpinEffect(force: false);
        }
    }

    public void UpdateCylindricalSpinEffect(bool force = false)
    {
        if (!enableCylindricalEffect)
        {
            ResetTransforms();
            return;
        }

        if (reelImagesList == null || reelImagesList.Count == 0) return;

        float effectiveVisibleHalfHeight = visibleHalfHeight;
        if (visibleAreaRectTransform != null && visibleAreaRectTransform.rect.height > 0)
        {
            effectiveVisibleHalfHeight = visibleAreaRectTransform.rect.height * 0.5f;
        }
        float effectiveOuterHalfHeight = Mathf.Max(outerHalfHeight, effectiveVisibleHalfHeight * 1.5f);

        float invVisibleHalfHeight = 1f / Mathf.Max(1f, effectiveVisibleHalfHeight);
        float invOuterRange = 1f / Mathf.Max(1f, effectiveOuterHalfHeight - effectiveVisibleHalfHeight);

        for (int col = 0; col < reelImagesList.Count; col++)
        {
            var reel = reelImagesList[col];
            if (reel == null || reel.images == null) continue;

            float intensity = (reelCurveIntensity != null && col < reelCurveIntensity.Length) ? reelCurveIntensity[col] : 1f;

            int imgCount = reel.images.Count;
            for (int i = 0; i < imgCount; i++)
            {
                Image img = reel.images[i];
                if (img == null) continue;

                RectTransform rect = img.rectTransform;
                if (rect == null) continue;

                // Calculate vertical offset from visible viewport center
                float yRel = 0f;
                if (visibleAreaRectTransform != null)
                {
                    Vector3 localPoint = visibleAreaRectTransform.InverseTransformPoint(rect.position);
                    yRel = localPoint.y;
                }
                else
                {
                    yRel = rect.localPosition.y;
                }

                float absY = Mathf.Abs(yRel);
                float targetScale = 1f;
                float barrelT = (absY <= effectiveVisibleHalfHeight) ? (absY * invVisibleHalfHeight) : 1f;

                if (absY <= effectiveVisibleHalfHeight)
                {
                    float curveFactor = barrelT * barrelT * intensity;
                    targetScale = Mathf.Lerp(1f, edgeScale, curveFactor);
                }
                else
                {
                    float extraT = Mathf.Clamp01((absY - effectiveVisibleHalfHeight) * invOuterRange);
                    targetScale = Mathf.Lerp(1f, Mathf.Lerp(edgeScale, outerScale, extraT), intensity);
                }

                // 3D Cylinder / Drum barrel curvature
                float squashY = Mathf.Lerp(1f, minSquashY, barrelT * barrelT);
                float tiltX = Mathf.Lerp(0f, maxTiltAngle, barrelT) * -Mathf.Sign(yRel); // tilt back at top, tilt forward at bottom

                rect.localRotation = Quaternion.Euler(tiltX, 0f, 0f);

                Vector3 targetLocalScale = new Vector3(targetScale, squashY * targetScale, targetScale);
                if (force || rect.localScale != targetLocalScale)
                {
                    rect.localScale = targetLocalScale;
                }
            }
        }
    }

    public void ResetTransforms()
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
            }
        }
    }
}