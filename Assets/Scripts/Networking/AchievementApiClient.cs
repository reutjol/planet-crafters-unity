using System;
using System.Collections;

public class AchievementApiClient : BaseApiClient
{
    public static AchievementApiClient Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator GetUserAchievements(
        string accessToken,
        Action<UserAchievementsResponseDto> onSuccess,
        Action<string> onError)
    {
        yield return GetRequest<UserAchievementsResponseDto>("/api/achievements", accessToken, onSuccess, onError);
    }
}
