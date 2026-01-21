using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lazy-loading material library with caching.
/// Loads materials from Resources folder on-demand and caches them for reuse.
/// </summary>
public class MaterialLibrary : MonoBehaviour
{
    public string resourcesFolder = "Materials";
    private readonly Dictionary<string, Material> cache = new();

    public Material Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        if (cache.TryGetValue(key, out var mat)) return mat;

        mat = Resources.Load<Material>($"{resourcesFolder}/{key}");
        if (mat == null)
            Debug.LogWarning($"Material missing: Resources/{resourcesFolder}/{key}.mat");

        cache[key] = mat;
        return mat;
    }
}
