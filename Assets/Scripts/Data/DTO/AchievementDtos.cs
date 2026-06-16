using System;
using System.Collections.Generic;
using UnityEngine;


// ==========================================
// Achievement DTOs
// ==========================================

[System.Serializable]
public class AchievementDto
{
    public string id;
    public string title;
    public string description;
    public string category;      // "Single" or "Multi"
    public int targetValue;
    public string rewardType;
    public int rewardAmount;
    // Populated when loaded from server (0 / false when loaded from local JSON)
    public int currentProgress;
    public bool isCompleted;
    public bool rewardGranted;
}

[System.Serializable]
public class AchievementDatabaseDto
{
    public List<AchievementDto> achievements;
}

[System.Serializable]
public class UserAchievementsResponseDto
{
    public List<AchievementDto> achievements;
}

[System.Serializable]
public class UnlockedAchievementDto
{
    public string id;
    public string title;
    public string rewardType;
    public int rewardAmount;
    public bool rewardGranted;
}