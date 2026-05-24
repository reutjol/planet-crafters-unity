using UnityEngine;

public sealed class PlayerPrefsMusicPreferenceStore : IMusicPreferenceStore
{
    private readonly string key;

    public PlayerPrefsMusicPreferenceStore(string key)
    {
        this.key = key;
    }

    public bool LoadMusicEnabled(bool defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }

    public void SaveMusicEnabled(bool isEnabled)
    {
        PlayerPrefs.SetInt(key, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
}
