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
    public float leftReelEdgeX = 30f;
    public float rightReelEdgeX = -30f;
    public float leftReelOuterX = 60f;
    public float rightReelOuterX = -60f;
    public float edgeScale = 0.85f;
    public float outerScale = 0.7f;
    public float case1StopY = -305.5f;

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

    private void UpdateCylindricalSpinEffect(bool force = false)
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

                float targetX = 0f;
                float targetScale = 1f;

                if (absY <= effectiveVisibleHalfHeight)
                {
                    float t = absY * invVisibleHalfHeight;
                    float curveFactor = t * t * intensity;

                    if (col == 0)
                    {
                        targetX = Mathf.Lerp(0f, leftReelEdgeX, curveFactor);
                    }
                    else if (col == 2)
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
                    else if (col == 2)
                    {
                        targetX = Mathf.Lerp(rightReelEdgeX, rightReelOuterX, extraT) * intensity;
                    }

                    targetScale = Mathf.Lerp(1f, Mathf.Lerp(edgeScale, outerScale, extraT), intensity);
                }

                Vector2 anchoredPos = rect.anchoredPosition;
                // Remove horizontal curving, keep X centered for Barrel effect
                targetX = 0f;
                if (force || !Mathf.Approximately(anchoredPos.x, targetX))
                {
                    rect.anchoredPosition = new Vector2(targetX, anchoredPos.y);
                }

                // 3D BARREL DRUM EFFECT
                // Calculate how squashed and tilted the symbol should be based on its Y position
                float barrelT = (absY <= effectiveVisibleHalfHeight) ? (absY * invVisibleHalfHeight) : 1f;
                float squashY = Mathf.Lerp(1f, 0.4f, barrelT * barrelT);
                float tiltX = Mathf.Lerp(0f, 60f, barrelT) * -Mathf.Sign(yRel); // tilt back at top, forward at bottom

                rect.localRotation = Quaternion.Euler(tiltX, 0f, 0f);

                Vector3 localScale = rect.localScale;
                if (force || !Mathf.Approximately(localScale.x, targetScale) || !Mathf.Approximately(localScale.y, squashY * targetScale))
                {
                    rect.localScale = new Vector3(targetScale, squashY * targetScale, targetScale);
                }
            }
        }
    }
}
