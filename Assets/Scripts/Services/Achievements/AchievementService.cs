using System;
using System.Collections.Generic;
using System.Linq;

public class AchievementService
{
    private readonly IAchievementRepository achievementRepository;

    public AchievementService(IAchievementRepository achievementRepository)
    {
        this.achievementRepository = achievementRepository;
    }

    public List<AchievementDto> GetAllAchievements()
    {
        return achievementRepository.GetAll();
    }

    public List<AchievementDto> GetAchievementsByCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return new List<AchievementDto>();
        }

        return achievementRepository.GetAll()
            .Where(a => !string.IsNullOrWhiteSpace(a.category) &&
                        string.Equals(a.category, category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public List<AchievementDto> GetSingleAchievements()
    {
        return GetAchievementsByCategory("Single");
    }

    public List<AchievementDto> GetMultiAchievements()
    {
        return GetAchievementsByCategory("Multi");
    }
}