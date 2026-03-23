using System.Collections.Generic;
using UnityEngine;

public class JsonAchievementRepository : IAchievementRepository
{
    private const string ResourcePath = "JSON/Achievements";

    public List<AchievementDto> GetAll()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(ResourcePath);

        if (jsonFile == null)
        {
            Debug.LogError($"JsonAchievementRepository: Could not find achievements JSON at Resources/{ResourcePath}.json");
            return new List<AchievementDto>();
        }

        AchievementDatabaseDto database = JsonUtility.FromJson<AchievementDatabaseDto>(jsonFile.text);

        if (database == null)
        {
            Debug.LogError("JsonAchievementRepository: Failed to parse achievements JSON. Database is null.");
            return new List<AchievementDto>();
        }

        if (database.achievements == null)
        {
            Debug.LogError("JsonAchievementRepository: Achievements list is null in JSON file.");
            return new List<AchievementDto>();
        }

        return database.achievements;
    }
}