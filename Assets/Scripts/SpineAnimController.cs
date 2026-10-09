using UnityEngine;
using Spine.Unity;


public class SpineAnimController : MonoBehaviour
{
    private SkeletonGraphic skeletonGraphic;
    [SerializeField] private string animName;
    private bool isPlaying;

    public SkeletonGraphic SkeletonGraphic
    {
        get
        {
            if (skeletonGraphic == null)
            {
                skeletonGraphic = GetComponentInChildren<SkeletonGraphic>(true);
            }
            return skeletonGraphic;
        }
    }

    void Awake()
    {
        var sg = SkeletonGraphic; // trigger the getter to ensure it exists
    }

    // ▶️ Play
    public void Play(bool loop)
    {
        if (skeletonGraphic == null)
            skeletonGraphic = SkeletonGraphic;

        if (skeletonGraphic != null && skeletonGraphic.SkeletonData != null && !string.IsNullOrEmpty(animName))
        {
            if (!isPlaying)
            {
                var track = skeletonGraphic.AnimationState.SetAnimation(0, animName, loop);
                if (track != null)
                {
                    track.TimeScale = 0.5f;
                }
                isPlaying = true;
                // log silenced
            }
            skeletonGraphic.freeze = false;
            skeletonGraphic.gameObject.SetActive(true);
        }
        else
        {
            // log silenced
        }
    }

    public void Pause()
    {
        if (skeletonGraphic == null)
            skeletonGraphic = SkeletonGraphic;

        if (skeletonGraphic != null)
        {
            if (skeletonGraphic.SkeletonData == null)
            {
                skeletonGraphic.Initialize(false);
            }
            skeletonGraphic.freeze = true;
        }
    }

    public void Resume()
    {
        if (skeletonGraphic == null)
            skeletonGraphic = SkeletonGraphic;

        if (skeletonGraphic != null)
        {
            if (skeletonGraphic.SkeletonData == null)
            {
                skeletonGraphic.Initialize(false);
            }

            if (!isPlaying && !string.IsNullOrEmpty(animName))
            {
                Play(true);
            }
            skeletonGraphic.freeze = false;
            skeletonGraphic.gameObject.SetActive(true);
        }
    }

    // ⏹️ Stop (clears animation completely)
    public void Stop()
    { 
        if(isPlaying)
        {
            var track = skeletonGraphic.AnimationState.GetCurrent(0);
            if (track != null)
            {
                track.TrackTime = track.Animation.Duration; // jump to last frame
                track.TimeScale = 0f;                       // freeze there
            }
            isPlaying = false;
            if (skeletonGraphic != null) skeletonGraphic.gameObject.SetActive(false);
        }
    }

    public float GetAnimationDuration()
    {
        if (skeletonGraphic == null)
            skeletonGraphic = SkeletonGraphic;

        if (skeletonGraphic != null)
        {
            if (skeletonGraphic.SkeletonData == null)
            {
                skeletonGraphic.Initialize(false);
            }
            
            if (skeletonGraphic.SkeletonData != null)
            {
                var anim = skeletonGraphic.SkeletonData.FindAnimation(animName);
                if (anim != null)
                {
                    return anim.Duration / 0.5f;
                }
            }
        }
        return 0f;
    }

    public void SetSkeletonData(SkeletonDataAsset skeletonDataAsset, string overrideAnimName = null, string skinName = null)
    {
        if (skeletonGraphic == null)
            skeletonGraphic = SkeletonGraphic;

        if (skeletonGraphic != null)
        {
            string oldAnimName = animName;
            if (overrideAnimName != null)
            {
                animName = overrideAnimName;
            }

            if (skeletonGraphic.skeletonDataAsset != skeletonDataAsset)
            {
                skeletonGraphic.Clear();
                skeletonGraphic.skeletonDataAsset = skeletonDataAsset;
                if (skeletonDataAsset != null) 
                {
                    skeletonGraphic.Initialize(true);
                }
                isPlaying = false;
                if (skeletonGraphic != null) skeletonGraphic.gameObject.SetActive(false);
            }
            
            // Check if the current animName is valid in the new skeleton data.
            // If it is not found, we fall back to the first animation in the new skeleton data.
            
            // Apply Skin if specified
            if (skeletonGraphic.SkeletonData != null && !string.IsNullOrEmpty(skinName))
            {
                skeletonGraphic.Skeleton.SetSkin(skinName);
                skeletonGraphic.Skeleton.SetSlotsToSetupPose();
                skeletonGraphic.LateUpdate();
            }
            if (skeletonGraphic.SkeletonData != null)
            {
                if (string.IsNullOrEmpty(animName) || skeletonGraphic.SkeletonData.FindAnimation(animName) == null)
                {
                    var animations = skeletonGraphic.SkeletonData.Animations;
                    if (animations.Count > 0)
                    {
                        animName = animations.Items[0].Name;
                    }
                    else
                    {
                        animName = "";
                    }
                }
            }

            if (animName != oldAnimName)
            {
                isPlaying = false;
            if (skeletonGraphic != null) skeletonGraphic.gameObject.SetActive(false);
            }
        }
    }
}
