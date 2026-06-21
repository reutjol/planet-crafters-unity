using System.Collections.Generic;
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
    public string MyUsername { get; private set; }
    public string OpponentUserId { get; private set; }
    public string OpponentUsername { get; private set; }
    public int InitialScore { get; private set; }
    public int CurrentMatchScore { get; private set; }

    private readonly List<UnlockedAchievementDto> _pendingAchievements = new List<UnlockedAchievementDto>();

    public void AccumulateAchievements(List<UnlockedAchievementDto> unlocked)
    {
        if (unlocked == null || unlocked.Count == 0) return;
        _pendingAchievements.AddRange(unlocked);
    }

    public List<UnlockedAchievementDto> TakeAchievements()
    {
        var result = new List<UnlockedAchievementDto>(_pendingAchievements);
        _pendingAchievements.Clear();
        return result;
    }

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
        MyUsername = AppSession.Instance?.Username ?? myUserId;

        foreach (var p in match.players)
        {
            if (p.userId != myUserId)
            {
                OpponentUserId = p.userId;
                OpponentUsername = string.IsNullOrEmpty(p.username) ? p.userId : p.username;
            }
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
        MyUsername = null;
        OpponentUserId = null;
        OpponentUsername = null;
        InitialScore = 0;
        _pendingAchievements.Clear();
    }
}
