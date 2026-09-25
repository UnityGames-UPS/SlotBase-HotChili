using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class StarFountain : MonoBehaviour
{
    [Header("Star Rain Settings")]
    [SerializeField] private GameObject starPrefab;
    [SerializeField] private Transform starSpawnContainer;
    [SerializeField] private int starPoolSize = 200;

    [Header("Behavior Settings")]
    [Tooltip("If true, items maintain initial scale and alpha throughout travel (fade variation 0-10% max).")]
    [SerializeField] private bool disableEndFadeAndScale = false;

    [Header("Rain Speed Settings")]
    [SerializeField] private float minFallDuration = 1.2f;
    [SerializeField] private float maxFallDuration = 2.2f;

    [Header("Rain Density & Frequency Settings")]
    [SerializeField] private float minSpawnInterval = 0.09f;
    [SerializeField] private float maxSpawnInterval = 0.20f;
    [SerializeField] private int minPrewarmCount = 10;
    [SerializeField] private int maxPrewarmCount = 30;
    [SerializeField] private int itemsPerSpawn = 1;

    private List<GameObject> starPool = new List<GameObject>();
    private Coroutine starRainCoroutine;

    private void Start()
    {
        InitializeStarPool();
    }

    private void InitializeStarPool()
    {
        if (starPrefab == null) return;
        Transform parentContainer = starSpawnContainer != null ? starSpawnContainer : transform;

        for (int i = 0; i < starPoolSize; i++)
        {
            GameObject star = Instantiate(starPrefab, parentContainer);
            star.SetActive(false);
            starPool.Add(star);
        }
    }

    private GameObject GetPooledStar()
    {
        for (int i = 0; i < starPool.Count; i++)
        {
            if (starPool[i] != null && !starPool[i].activeSelf)
            {
                return starPool[i];
            }
        }

        Transform parentContainer = starSpawnContainer != null ? starSpawnContainer : transform;
        if (starPrefab != null)
        {
            GameObject star = Instantiate(starPrefab, parentContainer);
            star.SetActive(false);
            starPool.Add(star);
            return star;
        }

        return null;
    }

    internal void PlayStarRain()
    {
        StopStarRain();
        if (starPrefab == null) return;

        int prewarmCount = Random.Range(minPrewarmCount, maxPrewarmCount + 1);
        for (int i = 0; i < prewarmCount; i++)
        {
            float initialProgress = Random.Range(0.08f, 1.2f);
            PrewarmSingleRainStar(initialProgress);
        }

        starRainCoroutine = StartCoroutine(StarRainRoutine());
    }

    internal void StopStarRain()
    {
        if (starRainCoroutine != null)
        {
            StopCoroutine(starRainCoroutine);
            starRainCoroutine = null;
        }

        for (int i = 0; i < starPool.Count; i++)
        {
            if (starPool[i] != null)
            {
                RecycleStar(starPool[i]);
            }
        }
    }

    private void RecycleStar(GameObject star)
    {
        if (star == null) return;

        DOTween.Kill(star);
        DOTween.Kill(star.transform);
        RectTransform rt = star.GetComponent<RectTransform>();
        if (rt != null) DOTween.Kill(rt);

        star.SetActive(false);

        CanvasGroup cg = star.GetComponent<CanvasGroup>();
        Image img = star.GetComponent<Image>();
        if (cg != null) cg.alpha = 0f;
        if (img != null)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }

        if (rt != null) rt.anchoredPosition = Vector2.zero;
        else star.transform.localPosition = Vector3.zero;
    }

    private IEnumerator StarRainRoutine()
    {
        while (gameObject.activeInHierarchy)
        {
            int count = Mathf.Max(1, itemsPerSpawn);
            for (int i = 0; i < count; i++)
            {
                SpawnSingleRainStar();
            }
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    private void GetContainerDimensions(out float width, out float height)
    {
        width = 800f;
        height = 600f;
        Transform container = starSpawnContainer != null ? starSpawnContainer : transform;
        RectTransform containerRect = container.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            if (containerRect.rect.width > 0) width = containerRect.rect.width;
            if (containerRect.rect.height > 0) height = containerRect.rect.height;
        }
    }

    #region Rain Mode (Top to Bottom)

    private void PrewarmSingleRainStar(float progress)
    {
        GameObject star = GetPooledStar();
        if (star == null) return;

        RecycleStar(star);

        GetContainerDimensions(out float width, out float height);
        float hw = width * 0.5f;
        float hh = height * 0.5f;

        float startX = Random.Range(-hw, hw);
        float startY = hh + 30f;
        Vector2 startPos = new Vector2(startX, startY);

        float endX = startX + Random.Range(-60f, 60f);
        float endY = -hh - 40f;
        Vector2 targetPos = new Vector2(endX, endY);

        float totalDuration = Random.Range(minFallDuration, maxFallDuration);
        float remainingDuration = totalDuration * (1f - progress);

        Vector2 currentPos = Vector2.Lerp(startPos, targetPos, progress);

        float randomScale = Random.Range(0.4f, 1.1f);
        star.transform.localScale = Vector3.one * randomScale;

        float currentRotation = Random.Range(0f, 360f);
        star.transform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);

        RectTransform starRect = star.GetComponent<RectTransform>();
        if (starRect != null) starRect.anchoredPosition = currentPos;
        else star.transform.localPosition = currentPos;

        CanvasGroup cg = star.GetComponent<CanvasGroup>();
        Image img = star.GetComponent<Image>();

        float startAlpha = disableEndFadeAndScale ? Random.Range(0.85f, 1.0f) : Mathf.Clamp01(1.2f - progress);
        if (cg != null) cg.alpha = startAlpha;
        if (img != null)
        {
            Color c = img.color;
            c.a = startAlpha;
            img.color = c;
        }

        star.SetActive(true);

        if (!disableEndFadeAndScale)
        {
            if (cg != null) cg.DOFade(0f, remainingDuration).SetEase(Ease.InQuad);
            else if (img != null) img.DOFade(0f, remainingDuration).SetEase(Ease.InQuad);
        }

        Sequence starSeq = DOTween.Sequence();
        if (starRect != null)
            starSeq.Join(starRect.DOAnchorPos(targetPos, remainingDuration).From(currentPos).SetEase(Ease.Linear));
        else
            starSeq.Join(star.transform.DOLocalMove(targetPos, remainingDuration).From(currentPos).SetEase(Ease.Linear));

        float extraRot = Random.Range(-180f, 180f);
        starSeq.Join(star.transform.DORotate(new Vector3(0, 0, currentRotation + extraRot), remainingDuration, RotateMode.FastBeyond360));

        starSeq.OnComplete(() => RecycleStar(star));
    }

    private void SpawnSingleRainStar()
    {
        GameObject star = GetPooledStar();
        if (star == null) return;

        RecycleStar(star);

        GetContainerDimensions(out float width, out float height);
        float hw = width * 0.5f;
        float hh = height * 0.5f;

        float startX = Random.Range(-hw, hw);
        float startY = hh + 30f;
        Vector2 startPos = new Vector2(startX, startY);

        float endX = startX + Random.Range(-60f, 60f);
        float endY = -hh - 40f;
        Vector2 targetPos = new Vector2(endX, endY);

        float animDuration = Random.Range(minFallDuration, maxFallDuration);

        float randomScale = Random.Range(0.4f, 1.1f);
        star.transform.localScale = Vector3.one * randomScale;

        float randomRotation = Random.Range(0f, 360f);
        star.transform.localRotation = Quaternion.Euler(0f, 0f, randomRotation);

        RectTransform starRect = star.GetComponent<RectTransform>();
        if (starRect != null) starRect.anchoredPosition = startPos;
        else star.transform.localPosition = startPos;

        CanvasGroup cg = star.GetComponent<CanvasGroup>();
        Image img = star.GetComponent<Image>();

        float startAlpha = disableEndFadeAndScale ? Random.Range(0.85f, 1.0f) : 1f;
        if (cg != null) cg.alpha = startAlpha;
        if (img != null)
        {
            Color c = img.color;
            c.a = startAlpha;
            img.color = c;
        }

        star.SetActive(true);

        if (!disableEndFadeAndScale)
        {
            if (cg != null) cg.DOFade(0f, animDuration).SetEase(Ease.InQuad);
            else if (img != null) img.DOFade(0f, animDuration).SetEase(Ease.InQuad);
        }

        Sequence starSeq = DOTween.Sequence();
        if (starRect != null)
            starSeq.Join(starRect.DOAnchorPos(targetPos, animDuration).From(startPos).SetEase(Ease.Linear));
        else
            starSeq.Join(star.transform.DOLocalMove(targetPos, animDuration).From(startPos).SetEase(Ease.Linear));

        float extraRot = Random.Range(-180f, 180f);
        starSeq.Join(star.transform.DORotate(new Vector3(0, 0, randomRotation + extraRot), animDuration, RotateMode.FastBeyond360));

        starSeq.OnComplete(() => RecycleStar(star));
    }

    #endregion

    private void OnDisable()
    {
        StopStarRain();
    }
}
