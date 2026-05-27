using System;
using System.Collections;

public class MatchApiClient : BaseApiClient
{
    public static MatchApiClient Instance { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator CreateMatch(string planetId, string stageId, int duration,
        string token, Action<MatchDto> onSuccess, Action<string> onError)
    {
        var body = new CreateMatchRequestDto { planetId = planetId, stageId = stageId, duration = duration };
        yield return PostRequest<CreateMatchRequestDto, MatchDto>("/api/matches/create", body, token, onSuccess, onError);
    }

    public IEnumerator JoinMatch(string code, string planetId, string stageId,
        string token, Action<MatchDto> onSuccess, Action<string> onError)
    {
        var body = new JoinMatchRequestDto { code = code, planetId = planetId, stageId = stageId };
        yield return PostRequest<JoinMatchRequestDto, MatchDto>("/api/matches/join", body, token, onSuccess, onError);
    }

    public IEnumerator GetMatch(string matchId, string token,
        Action<MatchDto> onSuccess, Action<string> onError)
    {
        yield return GetRequest<MatchDto>($"/api/matches/{matchId}", token, onSuccess, onError);
    }

    public IEnumerator UpdateScore(string matchId, int score, string token,
        Action<MatchDto> onSuccess, Action<string> onError)
    {
        var body = new MatchScoreRequestDto { score = score };
        yield return PostRequest<MatchScoreRequestDto, MatchDto>($"/api/matches/{matchId}/score", body, token, onSuccess, onError);
    }

    public IEnumerator SendReady(string matchId, string token,
        Action<MatchDto> onSuccess, Action<string> onError)
    {
        yield return PostRequest<MatchScoreRequestDto, MatchDto>(
            $"/api/matches/{matchId}/ready", new MatchScoreRequestDto(), token, onSuccess, onError);
    }

    public IEnumerator FinishMatch(string matchId, int finalScore, string token,
        Action<MatchDto> onSuccess, Action<string> onError)
    {
        var body = new MatchFinishRequestDto { finalScore = finalScore };
        yield return PostRequest<MatchFinishRequestDto, MatchDto>($"/api/matches/{matchId}/finish", body, token, onSuccess, onError);
    }
}
