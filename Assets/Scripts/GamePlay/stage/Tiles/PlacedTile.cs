using System;
using UnityEngine;

[Serializable]
public class PlacedTile
{
    // Which tile definition this instance uses
    public TileData tileId;

    // Axial coordinates on the hex grid
    public AxialCoord coord;

    // Rotation in steps of 60 degrees clockwise (0..5)
    public int rotation;

    public PlacedTile(TileData tileId, AxialCoord coord, int rotation)
    {
        this.tileId = tileId;
        this.coord = coord;
        this.rotation = rotation;
    }
}
