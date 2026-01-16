using UnityEngine;

public class AppSession : MonoBehaviour
{
    public static AppSession Instance { get; private set; }

    private const string AccessKey = "JWT_ACCESS";
    private const string RefreshKey = "JWT_REFRESH";

    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }

    public string SelectedStageId { get; private set; }
    public StageStateDto SelectedStageState { get; set; }
    public PlanetDto ActivePlanet { get; set; }

    public void SetSelectedStage(string stageId)
    {
        SelectedStageId = stageId;
    }
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ActivePlanet = null;   
        SelectedStageId = null;
        SelectedStageState = null;

        AccessToken = PlayerPrefs.GetString(AccessKey, "");
        RefreshToken = PlayerPrefs.GetString(RefreshKey, "");
    }

    public bool HasAccess() => !string.IsNullOrEmpty(AccessToken);
    public bool HasRefresh() => !string.IsNullOrEmpty(RefreshToken);

    public void SetTokens(string access, string refresh)
    {
        AccessToken = access;
        RefreshToken = refresh;

        PlayerPrefs.SetString(AccessKey, access);
        PlayerPrefs.SetString(RefreshKey, refresh);
        PlayerPrefs.Save();
    }

    public void SetAccess(string access)
    {
        AccessToken = access;
        PlayerPrefs.SetString(AccessKey, access);
        PlayerPrefs.Save();
    }

    public void Logout()
    {
        AccessToken = "";
        RefreshToken = "";
        ActivePlanet = null;
        SelectedStageId = null;
        SelectedStageState = null;
        PlayerPrefs.DeleteKey(AccessKey);
        PlayerPrefs.DeleteKey(RefreshKey);
        PlayerPrefs.Save();
    }


}
