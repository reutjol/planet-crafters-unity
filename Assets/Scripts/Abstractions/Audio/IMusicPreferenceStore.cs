public interface IMusicPreferenceStore
{
    bool LoadMusicEnabled(bool defaultValue);
    void SaveMusicEnabled(bool isEnabled);
}
