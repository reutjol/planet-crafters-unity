using System;
using System.Collections.Generic;
using UnityEngine;


// ==========================================
// Achievement DTOs
// ==========================================

[System.Serializable]
public class AchievementDto
{
    public string id;            // Unique identifier of the achievement
    public string title;         // Visible name shown to the player
    public string description;   // Explanation of how to unlock the achievement
    public string category;      // "Single" or "Multi"
    public int targetValue;      // Required progress value to complete the achievement
    public string rewardType;    // Type of reward (Coins, Boost, etc.)
    public int rewardAmount;     // Amount of reward granted
}

[System.Serializable]
public class AchievementDatabaseDto
{
    public List<AchievementDto> achievements;
}