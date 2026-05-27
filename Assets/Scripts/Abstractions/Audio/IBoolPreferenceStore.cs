public interface IBoolPreferenceStore
{
    bool Load(bool defaultValue);
    void Save(bool value);
}
