using UnityEngine;

public sealed class PlayerPrefsLevelProvider : MonoBehaviour, IPlayerLevelProvider
{
    [SerializeField] private string playerPrefsKey = "player.level";
    [SerializeField, Min(1)] private int defaultLevel = 12;

    public int CurrentLevel => Mathf.Max(1, PlayerPrefs.GetInt(playerPrefsKey, defaultLevel));

    public void SaveLevel(int level)
    {
        PlayerPrefs.SetInt(playerPrefsKey, Mathf.Max(1, level));
        PlayerPrefs.Save();
    }
}
