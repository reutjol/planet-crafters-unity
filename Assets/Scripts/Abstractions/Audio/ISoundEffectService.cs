using System;
using UnityEngine;

public interface ISoundEffectService
{
    event Action<bool> SoundEffectsEnabledChanged;

    bool AreSoundEffectsEnabled { get; }
    float MasterVolume { get; }

    void SetSoundEffectsEnabled(bool isEnabled);
    void ToggleSoundEffects();
    void Play(AudioClip clip, float volumeScale);
}
