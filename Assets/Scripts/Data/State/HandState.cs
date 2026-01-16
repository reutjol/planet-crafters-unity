using System;
using System.Collections.Generic;

[Serializable]
public class HandState
{
    public int maxHandSize;
    public List<TileData> tilesInHand;
}
