using System;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-990)]
[RequireComponent(typeof(AudioSource))]
public sealed class SoundEffectService : MonoBehaviour, ISoundEffectService
{
    private const string DefaultPlayerPrefsKey = "audio.sfx.enabled";
    private const float MaximumMasterVolume = 0.5f;
    private const float DefaultMasterVolume = 0.5f;

    private static SoundEffectService instance;

    [SerializeField, Range(0f, MaximumMasterVolume)] private float masterVolume = DefaultMasterVolume;
    [SerializeField] private string playerPrefsKey = DefaultPlayerPrefsKey;

    private AudioSource audioSource;
    private IBoolPreferenceStore preferenceStore;
    private bool isInitialized;
    private bool areSoundEffectsEnabled = true;

    public static SoundEffectService Instance => instance;

    public event Action<bool> SoundEffectsEnabledChanged;

    public bool AreSoundEffectsEnabled => areSoundEffectsEnabled;
    public float MasterVolume => masterVolume;

    public static SoundEffectService EnsureInstance(float requestedMasterVolume = DefaultMasterVolume)
    {
        if (instance != null)
        {
            instance.Configure(requestedMasterVolume);
            return instance;
        }

        GameObject serviceObject = new GameObject("SoundEffectService");
        SoundEffectService service = serviceObject.AddComponent<SoundEffectService>();
        service.Configure(requestedMasterVolume);
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

    private void OnValidate()
    {
        masterVolume = Mathf.Clamp(masterVolume, 0f, MaximumMasterVolume);
    }

    public void Configure(float requestedMasterVolume)
    {
        Initialize();
        masterVolume = Mathf.Clamp(requestedMasterVolume, 0f, MaximumMasterVolume);
        ApplyAudioSourceState();
    }

    public void ToggleSoundEffects()
    {
        SetSoundEffectsEnabled(!areSoundEffectsEnabled);
    }

    public void SetSoundEffectsEnabled(bool isEnabled)
    {
        Initialize();

        if (areSoundEffectsEnabled == isEnabled)
        {
            ApplyAudioSourceState();
            return;
        }

        areSoundEffectsEnabled = isEnabled;
        preferenceStore.Save(areSoundEffectsEnabled);

        ApplyAudioSourceState();
        SoundEffectsEnabledChanged?.Invoke(areSoundEffectsEnabled);
    }

    public void Play(AudioClip clip, float volumeScale)
    {
        Initialize();

        if (!areSoundEffectsEnabled || clip == null || audioSource == null)
            return;

        float safeVolumeScale = Mathf.Clamp01(volumeScale);
        audioSource.PlayOneShot(clip, masterVolume * safeVolumeScale);
    }

    private void Initialize()
    {
        if (isInitialized)
            return;

        audioSource = GetComponent<AudioSource>();
        preferenceStore = new PlayerPrefsBoolPreferenceStore(playerPrefsKey);
        areSoundEffectsEnabled = preferenceStore.Load(true);

        isInitialized = true;
        ApplyAudioSourceState();
    }

    private void ApplyAudioSourceState()
    {
        if (audioSource == null)
            return;

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
        audioSource.mute = !areSoundEffectsEnabled;

        if (!areSoundEffectsEnabled && audioSource.isPlaying)
            audioSource.Stop();
    }
}
