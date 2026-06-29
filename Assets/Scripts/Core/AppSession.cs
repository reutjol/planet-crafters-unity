using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages global application session state including authentication tokens and game context.
/// Persists across scenes and handles token storage via PlayerPrefs.
/// </summary>
public class AppSession : MonoBehaviour
{
    public static AppSession Instance { get; private set; }

    private const string AccessKey       = "JWT_ACCESS";
    private const string RefreshKey      = "JWT_REFRESH";
    private const string UserIdKey       = "USER_ID";
    private const string UsernameKey     = "USERNAME";
    private const string SelectedAvatarKey = "SELECTED_AVATAR";

    public string AccessToken { get; private set; }
    public string RefreshToken { get; private set; }
    public string UserId { get; private set; }
    public string Username { get; private set; }

    public string SelectedStageId { get; private set; }
    public PlanetDto ActivePlanet { get; set; }
    public string SelectedAvatar { get; private set; }

    private int previousSceneIndex = -1;
    public int PreviousSceneIndex => previousSceneIndex;

    public void NavigateTo(int sceneIndex)
    {
        previousSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }

    public void GoBack()
    {
        if (previousSceneIndex >= 0)
            SceneManager.LoadScene(previousSceneIndex);
    }

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

        AccessToken    = PlayerPrefs.GetString(AccessKey, "");
        RefreshToken   = PlayerPrefs.GetString(RefreshKey, "");
        UserId         = PlayerPrefs.GetString(UserIdKey, "");
        Username       = PlayerPrefs.GetString(UsernameKey, "");
        SelectedAvatar = PlayerPrefs.GetString(SelectedAvatarKey, "");
    }

    public bool HasAccess() => !string.IsNullOrEmpty(AccessToken);
    public bool HasRefresh() => !string.IsNullOrEmpty(RefreshToken);

    public void SetUser(string userId, string username, string selectedAvatar = null)
    {
        UserId = userId;
        Username = username;
        PlayerPrefs.SetString(UserIdKey, userId ?? "");
        PlayerPrefs.SetString(UsernameKey, username ?? "");
        if (!string.IsNullOrEmpty(selectedAvatar))
        {
            SelectedAvatar = selectedAvatar;
            PlayerPrefs.SetString(SelectedAvatarKey, selectedAvatar);
        }
        PlayerPrefs.Save();
    }

    public void SetSelectedAvatar(string avatarId)
    {
        if (string.IsNullOrEmpty(avatarId)) return;
        SelectedAvatar = avatarId;
        PlayerPrefs.SetString(SelectedAvatarKey, avatarId);
        PlayerPrefs.Save();
    }

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
        MapStageController.ResetPersistentState();
        AccessToken = "";
        RefreshToken = "";
        ActivePlanet = null;
        SelectedStageId = null;
        SelectedAvatar = "";
        PlayerPrefs.DeleteKey(AccessKey);
        PlayerPrefs.DeleteKey(RefreshKey);
        PlayerPrefs.DeleteKey(UserIdKey);
        PlayerPrefs.DeleteKey(UsernameKey);
        PlayerPrefs.DeleteKey(SelectedAvatarKey);
        PlayerPrefs.Save();
    }


}
