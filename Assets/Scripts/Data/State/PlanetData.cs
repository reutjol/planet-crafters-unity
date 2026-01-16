using UnityEngine;

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
