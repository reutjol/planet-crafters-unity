using UnityEngine;

public sealed class PlayerPrefsBoolPreferenceStore : IBoolPreferenceStore
{
    private readonly string key;

    public PlayerPrefsBoolPreferenceStore(string key)
    {
        this.key = key;
    }

    public bool Load(bool defaultValue)
    {
        return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
    }

    public void Save(bool value)
    {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}
