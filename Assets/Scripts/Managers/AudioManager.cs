using UnityEngine;

public class AudioManager : MonoBehaviour
{
    internal static AudioManager Instance ;

    private void Awake()
    {
        Instance = this;

        _musicEnabled = PlayerPrefs.GetInt(PrefKeyMusic, 1) == 1;
        _sfxEnabled   = PlayerPrefs.GetInt(PrefKeysfx,   1) == 1;
        _musicVolume  = PlayerPrefs.GetFloat(PrefKeyMusicVol, 0.5f);
        _sfxVolume    = PlayerPrefs.GetFloat(PrefKeySfxVol,   1.0f);

        ApplyMusicVolume();
        ApplySfxVolume();
    }

    private const string PrefKeyMusic    = "audio_music_enabled";
    private const string PrefKeysfx      = "audio_sfx_enabled";
    private const string PrefKeyMusicVol = "audio_music_volume";
    private const string PrefKeySfxVol   = "audio_sfx_volume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgMusicSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource reserveSource;
    [SerializeField] private AudioSource primaryButtonSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip clipGameMainBg;
    [SerializeField] private AudioClip clipBetPlusMinus;
    [SerializeField] private AudioClip clipMaxBetReached;
    [SerializeField] private AudioClip clipPrimaryActionButton;
    [SerializeField] private AudioClip clipGeneralButtonClick;
    [SerializeField] private AudioClip clipPopupOpenClose;
    [SerializeField] private AudioClip clipWinTypePopupOpen;
    [SerializeField] private AudioClip clipResultPopupOpen;
    [SerializeField] private AudioClip clipAutoplayPanelOpen;
    [SerializeField] private AudioClip clipWinLinePhase1Start;
    [SerializeField] private AudioClip clipReelStop;
    [SerializeField] private AudioClip clipTurboButtonClick;
    [SerializeField] private AudioClip clipTensionBuilder;
    [SerializeField] private AudioClip clipReelSpinLoop;
    [SerializeField] private AudioClip clipWheelTriggerWinLine;
    [SerializeField] private AudioClip clipFeatureBg;
    [SerializeField] private AudioClip clipWheelSpinBg;
    [SerializeField] private AudioClip clipWheelStop;

    private bool _musicEnabled = true;
    private bool _sfxEnabled   = true;
    private float _musicVolume = 0.5f;
    private float _sfxVolume   = 1.0f;

    internal bool MusicEnabled => _musicEnabled;
    internal bool SfxEnabled   => _sfxEnabled;
    internal float MusicVolume => _musicVolume;
    internal float SfxVolume   => _sfxVolume;

    internal AudioClip ClipTurboButtonClick => clipTurboButtonClick;
    internal AudioClip ClipTensionBuilder => clipTensionBuilder;
    internal AudioClip ClipReelSpinLoop => clipReelSpinLoop;
    internal AudioClip ClipWinTypePopupOpen => clipWinTypePopupOpen;
    internal AudioClip ClipResultPopupOpen => clipResultPopupOpen;
    internal AudioClip ClipWheelTriggerWinLine => clipWheelTriggerWinLine;
    internal AudioClip ClipFeatureBg => clipFeatureBg;
    internal AudioClip ClipWheelSpinBg => clipWheelSpinBg;
    internal AudioClip ClipWheelStop => clipWheelStop;

    internal void SetMusicEnabled(bool on)
    {
        _musicEnabled = on;
        PlayerPrefs.SetInt(PrefKeyMusic, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicVolume();
    }

    internal void SetSfxEnabled(bool on)
    {
        _sfxEnabled = on;
        PlayerPrefs.SetInt(PrefKeysfx, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplySfxVolume();
    }

    internal void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(PrefKeyMusicVol, _musicVolume);
        PlayerPrefs.Save();
        ApplyMusicVolume();
    }

    internal void SetSfxVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(PrefKeySfxVol, _sfxVolume);
        PlayerPrefs.Save();
        ApplySfxVolume();
    }

    private void ApplyMusicVolume()
    {
        if (bgMusicSource == null) return;
        bgMusicSource.volume = _musicEnabled ? _musicVolume : 0f;
    }

    private void ApplySfxVolume()
    {
        float v = _sfxEnabled ? _sfxVolume : 0f;
        if (uiSource            != null) uiSource.volume            = v;
        if (reserveSource       != null) reserveSource.volume       = v;
        if (primaryButtonSource != null) primaryButtonSource.volume = v;
    }

    private void PlayUISound(AudioClip clip)
    {
        if (!_sfxEnabled || clip == null) return;

        if (uiSource != null && !uiSource.isPlaying)
        {
            uiSource.PlayOneShot(clip);
        }
        else if (reserveSource != null)
        {
            reserveSource.PlayOneShot(clip);
        }
        else if (uiSource != null)
        {
            uiSource.PlayOneShot(clip);
        }
    }

    private void PlayLoop(AudioSource source, AudioClip clip)
    {
        if (source == null || clip == null) return;
        source.clip   = clip;
        source.loop   = true;
        source.volume = (source == bgMusicSource) ? (_musicEnabled ? _musicVolume : 0f) : (_sfxEnabled ? _sfxVolume : 0f);
        source.Play();
    }

    private void StopSource(AudioSource source)
    {
        if (source == null) return;
        source.Stop();
        source.loop = false;
    }

    internal void PlayBgMusic()
    {
        if (bgMusicSource == null || clipGameMainBg == null) return;
        if (bgMusicSource.isPlaying && bgMusicSource.clip == clipGameMainBg) return;

        bgMusicSource.clip   = clipGameMainBg;
        bgMusicSource.loop   = true;
        bgMusicSource.volume = _musicEnabled ? _musicVolume : 0f;
        bgMusicSource.Play();
    }

    internal void StopBgMusic()
    {
        StopSource(bgMusicSource);
    }

    internal void PlayBetPlusMinus()
    {
        PlayUISound(clipBetPlusMinus);
    }

    internal void PlayBetPlus()  => PlayBetPlusMinus();

    internal void PlayMaxBetReached()
    {
        PlayUISound(clipMaxBetReached);
    }

    internal void PlayWinTypePopupOpen()
    {
        if (!_sfxEnabled || clipWinTypePopupOpen == null) return;
        AudioSource targetSource = (reserveSource != null) ? reserveSource : uiSource;
        PlayLoop(targetSource, clipWinTypePopupOpen);
    }

    internal void StopWinTypePopupOpen()
    {
        if (reserveSource != null && reserveSource.clip == clipWinTypePopupOpen)
        {
            StopSource(reserveSource);
        }
        if (uiSource != null && uiSource.clip == clipWinTypePopupOpen)
        {
            StopSource(uiSource);
        }
    }

    internal void PlayResultPopupOpen()
    {
        PlayUISound(clipResultPopupOpen != null ? clipResultPopupOpen : clipPopupOpenClose);
    }

    internal void PlayPrimaryActionButton()
    {
        if (!_sfxEnabled) return;
        AudioClip clip = clipPrimaryActionButton != null ? clipPrimaryActionButton : clipGeneralButtonClick;
        if (clip == null) return;

        if (primaryButtonSource != null)
        {
            primaryButtonSource.PlayOneShot(clip);
        }
        else
        {
            PlayUISound(clip);
        }
    }

    internal void PlaySpinStart()    => PlayPrimaryActionButton();
    internal void PlaySpinStop()     => PlayPrimaryActionButton();
    internal void PlayTakeButton()   => PlayPrimaryActionButton();
    internal void PlayAutoplayStop() => PlayPrimaryActionButton();
    internal void PlayWheelStart()   => PlayPrimaryActionButton();

    internal void PlayButton()
    {
        PlayUISound(clipGeneralButtonClick);
    }

    internal void PlayPopupOpenClose()
    {
        PlayUISound(clipPopupOpenClose != null ? clipPopupOpenClose : clipGeneralButtonClick);
    }

    internal void PlayPopupClose() => PlayPopupOpenClose();
    internal void PlayPopupOpen()  => PlayPopupOpenClose();

    internal void PlayAutoplayPanelOpen()
    {
        PlayUISound(clipAutoplayPanelOpen != null ? clipAutoplayPanelOpen : clipPopupOpenClose);
    }

    internal void PlayWheelTriggerWinLine()
    {
        if (!_sfxEnabled || clipWheelTriggerWinLine == null) return;
        AudioSource targetSource = (reserveSource != null) ? reserveSource : uiSource;
        PlayLoop(targetSource, clipWheelTriggerWinLine);
    }

    internal void StopWheelTriggerWinLine()
    {
        if (reserveSource != null && reserveSource.clip == clipWheelTriggerWinLine)
        {
            StopSource(reserveSource);
        }
        if (uiSource != null && uiSource.clip == clipWheelTriggerWinLine)
        {
            StopSource(uiSource);
        }
    }

    internal void PlayFeatureBg()
    {
        if (clipFeatureBg == null) return;
        PlayLoop(bgMusicSource, clipFeatureBg);
    }

    internal void StopFeatureBg()
    {
        if (bgMusicSource != null && bgMusicSource.clip == clipFeatureBg)
        {
            StopBgMusic();
            PlayBgMusic();
        }
    }

    internal void PlayWheelSpinBg()
    {
        if (!_sfxEnabled || clipWheelSpinBg == null) return;
        AudioSource targetSource = (reserveSource != null) ? reserveSource : uiSource;
        PlayLoop(targetSource, clipWheelSpinBg);
    }

    internal void StopWheelSpinBg()
    {
        if (reserveSource != null && reserveSource.clip == clipWheelSpinBg)
        {
            StopSource(reserveSource);
        }
        if (uiSource != null && uiSource.clip == clipWheelSpinBg)
        {
            StopSource(uiSource);
        }
    }

    internal void PlayWheelStop()
    {
        PlayUISound(clipWheelStop);
    }

    internal void PlayWinLinePhase1Start()
    {
        PlayUISound(clipWinLinePhase1Start);
    }

    internal void PlayReelStop()
    {
        PlayUISound(clipReelStop);
    }

    internal void PlayTurboButtonClick()
    {
        PlayUISound(clipTurboButtonClick != null ? clipTurboButtonClick : clipGeneralButtonClick);
    }

    internal void PlayTurboBtnClick() => PlayTurboButtonClick();

    internal void PlayTensionBuilder()
    {
        if (!_sfxEnabled || clipTensionBuilder == null) return;
        AudioSource targetSource = (reserveSource != null) ? reserveSource : uiSource;
        PlayLoop(targetSource, clipTensionBuilder);
    }

    internal void StopTensionBuilder()
    {
        if (reserveSource != null && reserveSource.clip == clipTensionBuilder)
        {
            StopSource(reserveSource);
        }
        if (uiSource != null && uiSource.clip == clipTensionBuilder)
        {
            StopSource(uiSource);
        }
    }

    internal void PlayReelSpinLoop()
    {
        if (!_sfxEnabled || clipReelSpinLoop == null) return;
        AudioSource targetSource = (reserveSource != null) ? reserveSource : uiSource;
        PlayLoop(targetSource, clipReelSpinLoop);
    }

    internal void StopReelSpinLoop()
    {
        if (reserveSource != null && reserveSource.clip == clipReelSpinLoop)
        {
            StopSource(reserveSource);
        }
        if (uiSource != null && uiSource.clip == clipReelSpinLoop)
        {
            StopSource(uiSource);
        }
    }

    internal void PlayReelSpin() => PlayReelSpinLoop();
    internal void StopReelSpin() => StopReelSpinLoop();

    private bool isForceMuted = false;

    internal void SetMuteAll(bool forceMute)
    {
        if (forceMute == isForceMuted) return;
        isForceMuted = forceMute;

        AudioListener.volume = forceMute ? 0f : 1f;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetMuteAll(!hasFocus);
    }

    private void OnApplicationPause(bool isPaused)
    {
        SetMuteAll(isPaused);
    }
}
