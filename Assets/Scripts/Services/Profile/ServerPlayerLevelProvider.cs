using UnityEngine;

public sealed class ServerPlayerLevelProvider : MonoBehaviour, IPlayerLevelProvider
{
    public int CurrentLevel
    {
        get
        {
            PlanetDto planet = AppSession.Instance?.ActivePlanet;

            if (planet == null)
                return 1;

            if (planet.playerLevel > 0)
                return planet.playerLevel;

            return CalculateFromCompletedStages(planet);
        }
    }

    private static int CalculateFromCompletedStages(PlanetDto planet)
    {
        int completedStages = 0;

        if (planet?.stages != null)
        {
            foreach (StageDto stage in planet.stages)
            {
                if (stage?.meta == null)
                    continue;

                if (!stage.meta.isMatchStage && stage.meta.isCompleted)
                    completedStages++;
            }
        }

        return Mathf.Max(1, completedStages + 1);
    }
}
