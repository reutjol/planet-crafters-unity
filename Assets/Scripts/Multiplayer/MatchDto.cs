using System;
using System.Collections.Generic;

[Serializable]
public class MatchPlayerDto
{
    public string userId;
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
    public string myUserId; // set by server based on authenticated user
}

[Serializable]
public class CreateMatchRequestDto
{
    public string planetId;
    public string stageId;
    public int duration;
}

[Serializable]
public class JoinMatchRequestDto
{
    public string code;
    public string planetId;
    public string stageId;
}

[Serializable]
public class MatchScoreRequestDto
{
    public int score;
}

[Serializable]
public class MatchFinishRequestDto
{
    public int finalScore;
}
