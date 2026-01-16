using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SpaceGame/Planet Visual Library", fileName = "PlanetVisualLibrary")]
public class PlanetVisualLibrary : ScriptableObject
{
    [Serializable]
    public class PlanetVisual
    {
        public int planetId;
        public Material planetMaterial;
    }

    [SerializeField] List<PlanetVisual> visuals = new List<PlanetVisual>();
    Dictionary<int, Material> cache;

    public Material GetMaterial(int planetId)
    {
        if (cache == null)
        {
            cache = new Dictionary<int, Material>();
            for (int i = 0; i < visuals.Count; i++) if (visuals[i] != null) cache[visuals[i].planetId] = visuals[i].planetMaterial;
        }

        cache.TryGetValue(planetId, out var mat);
        return mat;
    }
}
