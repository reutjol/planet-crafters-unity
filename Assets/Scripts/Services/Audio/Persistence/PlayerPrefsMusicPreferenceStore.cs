public sealed class PlayerPrefsMusicPreferenceStore : IMusicPreferenceStore
{
    private readonly IBoolPreferenceStore preferenceStore;

    public PlayerPrefsMusicPreferenceStore(string key)
    {
        preferenceStore = new PlayerPrefsBoolPreferenceStore(key);
    }

    public bool LoadMusicEnabled(bool defaultValue)
    {
        return preferenceStore.Load(defaultValue);
    }

    public void SaveMusicEnabled(bool isEnabled)
    {
        preferenceStore.Save(isEnabled);
    }
}
