using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimation : MonoBehaviour
{
    public enum ImageState
    {
        NONE,
        PLAYING,
        PAUSED
    }

    public enum AnimationMode
    {
        SINGLE_PHASE,
        TWO_PHASE
    }

    public List<Sprite> textureArray;
    public Image rendererDelegate;
    public bool useSharedMaterial = true;
    public bool doLoopAnimation = true;
    
    [Header("Dynamic Timing")]
    public bool useDynamicFramerate = false;
    public float dynamicLoopDuration = 1.0f;
    [Tooltip("Target frame rate (FPS) for smooth animation. When useDynamicFramerate is true, loop count adaptively scales to maintain smooth speed near this FPS.")]
    public float targetFPS = 24.0f;
    
    public System.Action<int> onLoopComplete;
    private int currentLoopCount = 0;
    
    [SerializeField] private bool StartOnAwake;
    [SerializeField] private bool StartonEnable;

    [HideInInspector]
    public ImageState currentAnimationState;

    private int indexOfTexture;
    private float idealFrameRate = 0.0416666679f;
    private float delayBetweenAnimation;

    public float AnimationSpeed = 5f;
    public float delayBetweenLoop;

    [Header("Two Phase Animation (Optional)")]
    public AnimationMode animationMode = AnimationMode.SINGLE_PHASE;
    
    [Tooltip("Index where Phase 2 starts (Phase 1 is 0 to this index-1)")]
    public int phase2StartIndex = 0;
    
    [Tooltip("How many times Phase 1 should loop (-1 = infinite, 0 = skip phase 1, 1+ = specific count)")]
    public int phase1LoopCount = 1;
    
    [Tooltip("How many times Phase 2 should loop (-1 = infinite, 0 = skip phase 2, 1+ = specific count)")]
    public int phase2LoopCount = -1;

    [Tooltip("If enabled, allows separate animation speeds for Phase 1 and Phase 2. Phase 1 uses AnimationSpeed, while Phase 2 uses phase2AnimationSpeed.")]
    public bool useCustomSpeed = false;

    [Tooltip("Animation speed for Phase 2 when useCustomSpeed is enabled.")]
    public float phase2AnimationSpeed = 5f;

    private int currentPhase = 1;
    private int phase1CurrentLoop = 0;
    private int phase2CurrentLoop = 0;

    private float animStartTime;
    private float pauseStartTime;

    private void Awake()
    {
        EnsureRenderer();
        if (StartOnAwake)
        {
            StartAnimation();
        }
    }

    private void EnsureRenderer()
    {
        if (rendererDelegate == null)
        {
            rendererDelegate = GetComponent<Image>();
        }
    }

    void Start()
    {
        EnsureRenderer();
    }

    private void OnEnable()
    {
        EnsureRenderer();
        if (StartonEnable)
        {
            StartAnimation();
        }
    }

    private void OnDisable()
    {
        StopAnimation();
    }

    public void CalculateFrameDelay()
    {
        if (textureArray == null || textureArray.Count == 0)
        {
            delayBetweenAnimation = 0.0416666679f;
            return;
        }

        if (useDynamicFramerate && dynamicLoopDuration > 0f)
        {
            int frameCount = textureArray.Count;
            if (animationMode == AnimationMode.TWO_PHASE)
            {
                if (currentPhase == 1 && phase2StartIndex > 0)
                {
                    frameCount = phase2StartIndex;
                }
                else if (currentPhase == 2 && phase2StartIndex < textureArray.Count)
                {
                    frameCount = textureArray.Count - phase2StartIndex;
                }
            }

            if (frameCount > 0)
            {
                delayBetweenAnimation = dynamicLoopDuration / frameCount;
            }
            else
            {
                delayBetweenAnimation = 0.0416666679f;
            }
        }
        else
        {
            float currentSpeed = AnimationSpeed;
            if (animationMode == AnimationMode.TWO_PHASE && useCustomSpeed)
            {
                currentSpeed = (currentPhase == 2) ? phase2AnimationSpeed : AnimationSpeed;
            }
            if (currentSpeed <= 0f) currentSpeed = 0.001f;

            delayBetweenAnimation = idealFrameRate * (float)textureArray.Count / currentSpeed;
            if (delayBetweenAnimation <= 0) delayBetweenAnimation = 0.05f;
        }
    }

    private void ScheduleNextFrame()
    {
        if (currentAnimationState != ImageState.PLAYING) return;
        if (textureArray == null || textureArray.Count == 0) return;

        float nextDelay = delayBetweenAnimation;

        if (useDynamicFramerate && dynamicLoopDuration > 0f)
        {
            float elapsedTime = Time.time - animStartTime;
            int totalFrames = textureArray.Count;
            if (animationMode == AnimationMode.TWO_PHASE)
            {
                totalFrames = (currentPhase == 1) ? phase2StartIndex : (textureArray.Count - phase2StartIndex);
            }

            if (totalFrames > 0)
            {
                float frameDuration = dynamicLoopDuration / totalFrames;
                float currentFrameProgress = (elapsedTime % dynamicLoopDuration) / dynamicLoopDuration;
                int currentExpectedFrame = Mathf.Clamp(Mathf.FloorToInt(currentFrameProgress * totalFrames), 0, totalFrames - 1);
                
                float currentLoopIndex = Mathf.Floor(elapsedTime / dynamicLoopDuration);
                float nextFrameTargetTime = (currentLoopIndex * dynamicLoopDuration) + ((currentExpectedFrame + 1) * frameDuration);
                
                nextDelay = nextFrameTargetTime - elapsedTime;
                if (nextDelay < 0.001f) nextDelay = 0.001f;
            }
        }

        Invoke(nameof(AnimationProcess), nextDelay);
    }

    private void AnimationProcess()
    {
        if (textureArray == null || textureArray.Count == 0) return;

        if (useDynamicFramerate && dynamicLoopDuration > 0f)
        {
            float elapsedTime = Time.time - animStartTime;
            int totalFrames = textureArray.Count;
            if (animationMode == AnimationMode.TWO_PHASE)
            {
                totalFrames = (currentPhase == 1) ? phase2StartIndex : (textureArray.Count - phase2StartIndex);
            }

            if (totalFrames > 0)
            {
                int completedLoops = Mathf.FloorToInt(elapsedTime / dynamicLoopDuration);
                if (completedLoops > currentLoopCount)
                {
                    currentLoopCount = completedLoops;
                    onLoopComplete?.Invoke(currentLoopCount);
                    if (!doLoopAnimation)
                    {
                        indexOfTexture = totalFrames - 1;
                        SetTextureOfIndex();
                        currentAnimationState = ImageState.NONE;
                        return;
                    }
                }

                float loopProgress = (elapsedTime % dynamicLoopDuration) / dynamicLoopDuration;
                int frameOffset = (animationMode == AnimationMode.TWO_PHASE && currentPhase == 2) ? phase2StartIndex : 0;
                indexOfTexture = frameOffset + Mathf.Clamp(Mathf.FloorToInt(loopProgress * totalFrames), 0, totalFrames - 1);
            }
            else
            {
                indexOfTexture++;
            }

            if (animationMode == AnimationMode.SINGLE_PHASE)
            {
            }
            else
            {
                HandleTwoPhaseAnimation();
                if (currentAnimationState == ImageState.NONE) return;
            }

            SetTextureOfIndex();
            ScheduleNextFrame();
        }
        else
        {
            SetTextureOfIndex();
            indexOfTexture++;

            if (animationMode == AnimationMode.SINGLE_PHASE)
            {
                if (indexOfTexture >= textureArray.Count)
                {
                    indexOfTexture = 0;
                    currentLoopCount++;
                    onLoopComplete?.Invoke(currentLoopCount);
                    
                    if (doLoopAnimation)
                    {
                        Invoke(nameof(AnimationProcess), delayBetweenAnimation + delayBetweenLoop);
                    }
                    else
                    {
                        currentAnimationState = ImageState.NONE;
                    }
                }
                else
                {
                    Invoke(nameof(AnimationProcess), delayBetweenAnimation);
                }
            }
            else
            {
                HandleTwoPhaseAnimation();
            }
        }
    }

    private void HandleTwoPhaseAnimation()
    {
        if (currentPhase == 1)
        {
            if (indexOfTexture >= phase2StartIndex)
            {
                phase1CurrentLoop++;
                
                if (phase1LoopCount == -1 || phase1CurrentLoop < phase1LoopCount)
                {
                    indexOfTexture = 0;
                    if (!useDynamicFramerate)
                    {
                        Invoke(nameof(AnimationProcess), delayBetweenAnimation + delayBetweenLoop);
                    }
                }
                else
                {
                    currentPhase = 2;
                    indexOfTexture = phase2StartIndex;
                    CalculateFrameDelay();
                    
                    if (phase2LoopCount == 0)
                    {
                        currentAnimationState = ImageState.NONE;
                        return;
                    }
                    if (!useDynamicFramerate)
                    {
                        Invoke(nameof(AnimationProcess), delayBetweenAnimation + delayBetweenLoop);
                    }
                }
            }
            else
            {
                if (!useDynamicFramerate)
                {
                    Invoke(nameof(AnimationProcess), delayBetweenAnimation);
                }
            }
        }
        else if (currentPhase == 2)
        {
            if (indexOfTexture >= textureArray.Count)
            {
                phase2CurrentLoop++;
                
                if (phase2LoopCount == -1 || phase2CurrentLoop < phase2LoopCount)
                {
                    indexOfTexture = phase2StartIndex;
                    if (!useDynamicFramerate)
                    {
                        Invoke(nameof(AnimationProcess), delayBetweenAnimation + delayBetweenLoop);
                    }
                }
                else
                {
                    currentAnimationState = ImageState.NONE;
                }
            }
            else
            {
                if (!useDynamicFramerate)
                {
                    Invoke(nameof(AnimationProcess), delayBetweenAnimation);
                }
            }
        }
    }

    public void StartAnimation()
    {
        if (textureArray == null || textureArray.Count == 0) return;

        EnsureRenderer();
        if (rendererDelegate == null) return;

        CancelInvoke(nameof(AnimationProcess));
        indexOfTexture = 0;
        currentLoopCount = 0;
        animStartTime = Time.time;

        currentPhase = 1;
        phase1CurrentLoop = 0;
        phase2CurrentLoop = 0;

        currentAnimationState = ImageState.PLAYING;

        RevertToInitialState();

        if (animationMode == AnimationMode.TWO_PHASE && phase1LoopCount == 0)
        {
            currentPhase = 2;
            indexOfTexture = phase2StartIndex;
        }

        CalculateFrameDelay();

        if (useDynamicFramerate)
        {
            ScheduleNextFrame();
        }
        else
        {
            Invoke(nameof(AnimationProcess), delayBetweenAnimation);
        }
    }

    public void StopAnimation()
    {
        if (currentAnimationState != ImageState.NONE)
        {
            EnsureRenderer();
            if (rendererDelegate != null && textureArray != null && textureArray.Count > 0)
            {
                rendererDelegate.sprite = textureArray[0];
            }
            CancelInvoke(nameof(AnimationProcess));
            currentAnimationState = ImageState.NONE;
            
            currentPhase = 1;
            phase1CurrentLoop = 0;
            phase2CurrentLoop = 0;
            currentLoopCount = 0;
        }
    }

    public void RevertToInitialState()
    {
        indexOfTexture = 0;
        currentPhase = 1;
        phase1CurrentLoop = 0;
        phase2CurrentLoop = 0;
        SetTextureOfIndex();
    }

    private void SetTextureOfIndex()
    {
        if (textureArray == null || textureArray.Count == 0 || indexOfTexture < 0 || indexOfTexture >= textureArray.Count) return;

        EnsureRenderer();
        if (rendererDelegate != null)
        {
            rendererDelegate.sprite = textureArray[indexOfTexture];
        }
    }
}
