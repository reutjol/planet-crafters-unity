using System.Collections.Generic;

public interface IAchievementRepository
{
    List<AchievementDto> GetAll();
}