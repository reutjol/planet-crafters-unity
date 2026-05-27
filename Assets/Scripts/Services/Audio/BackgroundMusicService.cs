using System;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-1000)]
[RequireComponent(typeof(AudioSource))]
public sealed class BackgroundMusicService : MonoBehaviour, IBackgroundMusicService
{
    private const string DefaultPlayerPrefsKey = "audio.music.enabled";
    private const float MaximumBackgroundVolume = 0.5f;
    private const float DefaultBackgroundVolume = 0.35f;

    private static BackgroundMusicService instance;

    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, MaximumBackgroundVolume)] private float volume = DefaultBackgroundVolume;
    [SerializeField] private string playerPrefsKey = DefaultPlayerPrefsKey;
    [SerializeField] private bool playOnStart = true;

    private AudioSource audioSource;
    private IMusicPreferenceStore preferenceStore;
    private bool isInitialized;
    private bool isMusicEnabled = true;

    public static BackgroundMusicService Instance => instance;

    public event Action<bool> MusicEnabledChanged;

    public bool IsMusicEnabled => isMusicEnabled;
    public float Volume => volume;

    public static BackgroundMusicService EnsureInstance(AudioClip clip, float requestedVolume)
    {
        if (instance != null)
        {
            instance.Configure(clip, requestedVolume);
            return instance;
        }

        GameObject serviceObject = new GameObject("BackgroundMusicService");
        BackgroundMusicService service = serviceObject.AddComponent<BackgroundMusicService>();
        service.Configure(clip, requestedVolume);
        return service;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void Start()
    {
        if (playOnStart)
            ApplyPlaybackState();
    }

    private void OnValidate()
    {
        volume = Mathf.Clamp(volume, 0f, MaximumBackgroundVolume);
    }

    public void Configure(AudioClip clip, float requestedVolume)
    {
        Initialize();

        if (clip != null && musicClip != clip)
            musicClip = clip;

        volume = Mathf.Clamp(requestedVolume, 0f, MaximumBackgroundVolume);
        audioSource.volume = volume;

        if (musicClip != null)
            audioSource.clip = musicClip;

        ApplyPlaybackState();
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!isMusicEnabled);
    }

    public void SetMusicEnabled(bool isEnabled)
    {
        Initialize();

        if (isMusicEnabled == isEnabled)
        {
            ApplyPlaybackState();
            return;
        }

        isMusicEnabled = isEnabled;
        preferenceStore.SaveMusicEnabled(isMusicEnabled);

        ApplyPlaybackState();
        MusicEnabledChanged?.Invoke(isMusicEnabled);
    }

    private void Initialize()
    {
        if (isInitialized)
            return;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = Mathf.Clamp(volume, 0f, MaximumBackgroundVolume);

        if (musicClip != null)
            audioSource.clip = musicClip;

        preferenceStore = new PlayerPrefsMusicPreferenceStore(playerPrefsKey);
        isMusicEnabled = preferenceStore.LoadMusicEnabled(true);
        isInitialized = true;
    }

    private void ApplyPlaybackState()
    {
        if (audioSource == null)
            return;

        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.volume = Mathf.Clamp(volume, 0f, MaximumBackgroundVolume);

        if (musicClip != null && audioSource.clip != musicClip)
            audioSource.clip = musicClip;

        if (audioSource.clip == null)
            return;

        if (isMusicEnabled)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
}
