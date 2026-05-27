using System;

public interface IBackgroundMusicService
{
    event Action<bool> MusicEnabledChanged;

    bool IsMusicEnabled { get; }
    float Volume { get; }

    void SetMusicEnabled(bool isEnabled);
    void ToggleMusic();
}
