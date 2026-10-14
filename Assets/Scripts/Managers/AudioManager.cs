using UnityEngine;

public class AudioManager : MonoBehaviour
{
    internal static AudioManager Instance { get; private set; }

    private const string PrefKeyMusic    = "audio_music_enabled";
    private const string PrefKeysfx      = "audio_sfx_enabled";
    private const string PrefKeyMusicVol = "audio_music_volume";
    private const string PrefKeySfxVol   = "audio_sfx_volume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgMusicSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource reserveSource;
    [SerializeField] private AudioSource primaryButtonSource;

    [Header("Core Gameplay Music & Ambient")]
    [SerializeField] private AudioClip clipGameMainBg;
    [SerializeField] private AudioClip clipReelSpinLoop;
    [SerializeField] private AudioClip clipReelStop;
    [SerializeField] private AudioClip clipTensionBuilder;

    [Header("HotChili Win & Feature Sounds")]
    [SerializeField] private AudioClip clipNormalWin;
    [SerializeField] private AudioClip clipBigWin2x;
    [SerializeField] private AudioClip clipWildChilliHit;
    [SerializeField] private AudioClip clipCongratulationBox;
    [SerializeField] private AudioClip clipScatterHit;

    [Header("UI & Button Sounds")]
    [SerializeField] private AudioClip clipSpinButton;
    [SerializeField] private AudioClip clipGeneralButtonClick;
    [SerializeField] private AudioClip clipBetPlusMinus;
    [SerializeField] private AudioClip clipMaxBetReached;
    [SerializeField] private AudioClip clipTurboButtonClick;
    [SerializeField] private AudioClip clipAutoplayPanelOpen;
    [SerializeField] private AudioClip clipPopupOpenClose;
    [SerializeField] private AudioClip clipWinTypePopupOpen;

    private bool _musicEnabled = true;
    private bool _sfxEnabled   = true;
    private float _musicVolume = 0.5f;
    private float _sfxVolume   = 1.0f;

    internal bool MusicEnabled => _musicEnabled;
    internal bool SfxEnabled   => _sfxEnabled;
    internal float MusicVolume => _musicVolume;
    internal float SfxVolume   => _sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Auto-configure AudioSources if not linked in inspector
        EnsureAudioSources();

        _musicEnabled = PlayerPrefs.GetInt(PrefKeyMusic, 1) == 1;
        _sfxEnabled   = PlayerPrefs.GetInt(PrefKeysfx,   1) == 1;
        _musicVolume  = PlayerPrefs.GetFloat(PrefKeyMusicVol, 0.5f);
        _sfxVolume    = PlayerPrefs.GetFloat(PrefKeySfxVol,   1.0f);

        ApplyMusicVolume();
        ApplySfxVolume();
    }

    private void Start()
    {
        if (_musicEnabled)
        {
            PlayBgMusic();
        }
    }

    private void EnsureAudioSources()
    {
        if (bgMusicSource == null)
        {
            bgMusicSource = gameObject.AddComponent<AudioSource>();
            bgMusicSource.playOnAwake = false;
            bgMusicSource.loop = true;
        }

        if (uiSource == null)
        {
            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;
            uiSource.loop = false;
        }

        if (reserveSource == null)
        {
            reserveSource = gameObject.AddComponent<AudioSource>();
            reserveSource.playOnAwake = false;
            reserveSource.loop = false;
        }

        if (primaryButtonSource == null)
        {
            primaryButtonSource = gameObject.AddComponent<AudioSource>();
            primaryButtonSource.playOnAwake = false;
            primaryButtonSource.loop = false;
        }
    }

    #region Volume & Settings Controls

    private void SetMusicEnabled(bool on)
    {
        _musicEnabled = on;
        PlayerPrefs.SetInt(PrefKeyMusic, on ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMusicVolume();
        if (on) PlayBgMusic();
        else StopBgMusic();
    }

    private void SetSfxEnabled(bool on)
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
        if (uiSource != null) uiSource.volume = v;
        if (reserveSource != null) reserveSource.volume = v;
        if (primaryButtonSource != null) primaryButtonSource.volume = v;
    }

    #endregion

    #region Core Audio Helpers

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

    #endregion

    #region Background Music

    internal void PlayBgMusic()
    {
        if (bgMusicSource == null || clipGameMainBg == null) return;
        if (bgMusicSource.isPlaying && bgMusicSource.clip == clipGameMainBg) return;

        bgMusicSource.clip   = clipGameMainBg;
        bgMusicSource.loop   = true;
        bgMusicSource.volume = _musicEnabled ? _musicVolume : 0f;
        bgMusicSource.Play();
    }

    private void StopBgMusic()
    {
        StopSource(bgMusicSource);
    }

    #endregion

    #region Reels & Gameplay Sounds

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

    internal void PlayReelStop()
    {
        PlayUISound(clipReelStop);
    }

    internal void PlayWildChilliHit()
    {
        PlayUISound(clipWildChilliHit != null ? clipWildChilliHit : clipScatterHit);
    }

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

    #endregion

    #region Win Celebrations & Popups

    private void PlayNormalWin()
    {
        PlayUISound(clipNormalWin != null ? clipNormalWin : clipCongratulationBox);
    }

    internal void PlayBigWin()
    {
        PlayUISound(clipBigWin2x != null ? clipBigWin2x : clipCongratulationBox);
    }

    internal void PlayWinLinePhase1Start()
    {
        PlayNormalWin();
    }

    internal void PlayWinTypePopupOpen()
    {
        if (!_sfxEnabled) return;
        AudioClip clip = clipWinTypePopupOpen != null ? clipWinTypePopupOpen : clipBigWin2x;
        if (clip == null) return;

        AudioSource targetSource = (reserveSource != null) ? reserveSource : uiSource;
        PlayLoop(targetSource, clip);
    }

    internal void StopWinTypePopupOpen()
    {
        if (reserveSource != null && (reserveSource.clip == clipWinTypePopupOpen || reserveSource.clip == clipBigWin2x))
        {
            StopSource(reserveSource);
        }
        if (uiSource != null && (uiSource.clip == clipWinTypePopupOpen || uiSource.clip == clipBigWin2x))
        {
            StopSource(uiSource);
        }
    }

    private void PlayResultPopupOpen()
    {
        PlayUISound(clipCongratulationBox != null ? clipCongratulationBox : clipPopupOpenClose);
    }

    #endregion

    #region UI & Button Clicks

    internal void PlayButton()
    {
        PlayUISound(clipGeneralButtonClick);
    }

    internal void PlaySpinButton()
    {
        if (!_sfxEnabled) return;
        AudioClip clip = clipSpinButton != null ? clipSpinButton : clipGeneralButtonClick;
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

    internal void PlaySpinStart() => PlaySpinButton();
    internal void PlaySpinStop()  => PlaySpinButton();
    internal void PlayStopButton() => PlaySpinButton();

    internal void PlayBetPlusMinus()
    {
        PlayUISound(clipBetPlusMinus != null ? clipBetPlusMinus : clipGeneralButtonClick);
    }

    internal void PlayBetPlus()  => PlayBetPlusMinus();
    internal void PlayBetMinus() => PlayBetPlusMinus();

    internal void PlayMaxBetReached()
    {
        PlayUISound(clipMaxBetReached != null ? clipMaxBetReached : clipBetPlusMinus);
    }

    internal void PlayTurboButtonClick()
    {
        PlayUISound(clipTurboButtonClick != null ? clipTurboButtonClick : clipGeneralButtonClick);
    }

    internal void PlayTurboBtnClick() => PlayTurboButtonClick();

    internal void PlayAutoplayPanelOpen()
    {
        PlayUISound(clipAutoplayPanelOpen != null ? clipAutoplayPanelOpen : clipPopupOpenClose);
    }

    internal void PlayAutoplayStop()
    {
        PlaySpinButton();
    }

    internal void PlayPopupOpenClose()
    {
        PlayUISound(clipPopupOpenClose != null ? clipPopupOpenClose : clipGeneralButtonClick);
    }

    internal void PlayPopupOpen()  => PlayPopupOpenClose();
    internal void PlayPopupClose() => PlayPopupOpenClose();

    #endregion

    #region Focus & Mute Handling

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

    #endregion
}
