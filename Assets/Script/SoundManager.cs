using UnityEngine;

/// <summary>
/// Simple Sound Manager handling 2 BGMs (Main Menu & In-Game) and 3 SFXs (Touch, Crash, Dead).
/// Assign the audio clips in the Inspector on this component.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("BGM Clips")]
    [Tooltip("Background Music for Main Menu scene")]
    [SerializeField] private AudioClip mainMenuBGM;

    [Tooltip("Background Music for In-Game scene")]
    [SerializeField] private AudioClip inGameBGM;

    [Header("SFX Clips")]
    [Tooltip("Sound played on button press or screen touch")]
    [SerializeField] private AudioClip touchSFX;

    [Tooltip("Sound played when player ship crashes into an enemy")]
    [SerializeField] private AudioClip crashSFX;

    [Tooltip("Sound played when player dies / game over")]
    [SerializeField] private AudioClip deadSFX;

    [Tooltip("Sound played when player wins the level")]
    [SerializeField] private AudioClip winSFX;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad so audio persists seamlessly across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        // Create audio sources automatically if not assigned in Inspector
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
    }

    private void OnValidate()
    {
        if (bgmSource != null) bgmSource.volume = bgmVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    #region BGM Methods

    public void PlayMainMenuBGM()
    {
        PlayBGM(mainMenuBGM);
    }

    public void PlayInGameBGM()
    {
        PlayBGM(inGameBGM);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;

        // If the same BGM clip is already playing, keep playing without restarting
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    #endregion

    #region SFX Methods

    public void PlayTouchSFX()
    {
        PlaySFX(touchSFX);
    }

    public void PlayCrashSFX()
    {
        PlaySFX(crashSFX);
    }

    public void PlayDeadSFX()
    {
        PlaySFX(deadSFX);
    }

    public void PlayWinSFX()
    {
        PlaySFX(winSFX);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    #endregion

    #region Settings

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null) bgmSource.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    #endregion
}
