using UnityEngine;

public class LocalWheelStateRepository : IWheelStateRepository
{
    private const string LastSpinTicksKey = "wheel.lastSpinTicks";
    private const string TotalSpinsKey = "wheel.totalSpins";
    private const string LastRewardIdKey = "wheel.lastRewardId";

    public WheelState Load()
    {
        WheelState state = new WheelState();

        state.LastSpinUtcTicks = long.Parse(PlayerPrefs.GetString(LastSpinTicksKey, "0"));
        state.TotalSpins = PlayerPrefs.GetInt(TotalSpinsKey, 0);
        state.LastRewardId = PlayerPrefs.GetString(LastRewardIdKey, string.Empty);

        return state;
    }

    public void Save(WheelState state)
    {
        if (state == null)
            return;

        PlayerPrefs.SetString(LastSpinTicksKey, state.LastSpinUtcTicks.ToString());
        PlayerPrefs.SetInt(TotalSpinsKey, state.TotalSpins);
        PlayerPrefs.SetString(LastRewardIdKey, state.LastRewardId ?? string.Empty);
        PlayerPrefs.Save();
    }
}