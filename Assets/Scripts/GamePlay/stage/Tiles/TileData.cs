using System;
using UnityEngine;

/// <summary>
/// Logical definition of a hex tile (NO Unity logic).
/// This is pure data and can be loaded from JSON / DB.
/// </summary>
[Serializable]
public class TileData
{
    // Unique identifier (matches DB / catalog)
    public string tileId;

    // Element in the center of the tile
    public ElementType center;

    // Elements on the 6 edges (fixed order!)
    // Indexes: 0..5 clockwise
    public ElementType[] edges = new ElementType[6];

    // Rarity / level / difficulty weight
    public int rarity;

    // Visual references (resolved later by View layer)
    public string centerArtId;
    public string[] edgeArtIds = new string[6];

    // ----------- Helpers -----------

    /// <summary>
    /// Returns the element on a specific edge AFTER rotation.
    /// rotationSteps: 0..5 (each step = 60 degrees clockwise)
    /// </summary>
    public ElementType GetEdgeAfterRotation(int edgeIndex, int rotationSteps)
    {
        int idx = edgeIndex - rotationSteps;
        idx %= 6;
        if (idx < 0) idx += 6;
        return edges[idx];
    }
}

/// <summary>
/// Types of elements a tile edge/center can have.
/// Can be expanded freely later.
/// </summary>
public enum ElementType
{
    None,
    Rock,
    Gold,
    Bio,
    StarDast
}
