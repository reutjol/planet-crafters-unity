using UnityEngine;

/// <summary>
/// ScriptableObject defining planet metadata and visual prefab.
/// Used for planet selection screen configuration.
/// </summary>
[CreateAssetMenu(menuName = "SpaceGame/Planet Data", fileName = "NewPlanetData")]
public class PlanetData : ScriptableObject
{
    [Header("Identity")]
    public string planetId;

    [Header("Visuals")]
    public GameObject planetPrefab;

    [Header("Navigation")]
    public int targetSceneIndex = -1;
}
