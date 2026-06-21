using System;
using System.Collections.Generic;

[Serializable]
public class MatchPlayerDto
{
    public string userId;
    public string username;
    public string planetId;
    public string stageId;
    public int score;
    public bool finished;
}

[Serializable]
public class MatchDto
{
    public string matchId;
    public string code;
    public string status; // "waiting" | "active" | "finished"
    public int duration;
    public long? startTime; // Unix ms — null when match is still waiting
    public List<MatchPlayerDto> players;
    public string winnerId;
    public string matchStageId;
    public string myUserId;
    public List<UnlockedAchievementDto> achievementRewards;
}

[Serializable]
public class LobbyPlayerDto
{
    public string userId;
    public string username;
    public string avatarId;
}
