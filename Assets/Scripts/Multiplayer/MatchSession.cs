using UnityEngine;

public class MatchSession : MonoBehaviour
{
    public static MatchSession Instance { get; private set; }

    public bool IsActive { get; private set; }
    public string MatchId { get; private set; }
    public string MatchCode { get; private set; }
    public int Duration { get; private set; }
    public long StartTimeMs { get; private set; } // 0 when match is waiting
    public string MyUserId { get; private set; }
    public string OpponentUserId { get; private set; }
    public int InitialScore { get; private set; }
    public int CurrentMatchScore { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartMatch(MatchDto match, string myUserId)
    {
        IsActive = true;
        MatchId = match.matchId;
        MatchCode = match.code;
        Duration = match.duration;
        StartTimeMs = match.startTime ?? 0;
        MyUserId = myUserId;

        foreach (var p in match.players)
        {
            if (p.userId != myUserId)
                OpponentUserId = p.userId;
        }
    }

    public void SetInitialScore(int score)
    {
        InitialScore = score;
        CurrentMatchScore = 0;
    }

    public void UpdateCurrentScore(int stageScore)
    {
        CurrentMatchScore = Mathf.Max(0, stageScore - InitialScore);
    }

    public float GetSecondsRemaining()
    {
        if (!IsActive || StartTimeMs == 0) return 0;
        var nowMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var elapsed = (nowMs - StartTimeMs) / 1000f;
        return Mathf.Max(0f, Duration - elapsed);
    }

    public void SetStartTime(long startTimeMs)
    {
        StartTimeMs = startTimeMs;
    }

    public void Clear()
    {
        IsActive = false;
        MatchId = null;
        MatchCode = null;
        Duration = 0;
        StartTimeMs = 0;
        MyUserId = null;
        OpponentUserId = null;
        InitialScore = 0;
    }
}
