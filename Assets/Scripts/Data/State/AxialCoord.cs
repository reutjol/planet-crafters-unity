using System;
using UnityEngine;

/// <summary>
/// Axial coordinate system for hexagonal grids.
/// Uses q (column) and r (row) coordinates.
/// </summary>
[Serializable]
public struct AxialCoord
{
    public int q;
    public int r;

    public AxialCoord(int q, int r)
    {
        this.q = q;
        this.r = r;
    }
    public override string ToString() => $"({q},{r})";
}
