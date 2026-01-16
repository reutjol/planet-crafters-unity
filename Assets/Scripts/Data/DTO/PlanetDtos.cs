using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlanetDto
{
    public string planetId;
    public List<StageDto> stages;
}

[System.Serializable]
public class StageDto
{
    public string stageId;
    public StageMetaDto meta;
    public StageStateDto state; // אפשר להתעלם במסך מפה
}

[System.Serializable]
public class StageMetaDto
{
    public CoordDto coord;
    public bool isUnlocked;
    public bool isStarted;
    public bool isCompleted;
    public string lastPlayedAt; // מגיע כ-ISO string, מספיק למסך מפה
}

[System.Serializable]
public class CoordDto
{
    public int q;
    public int r;
}

// למסך מפה לא חייבים להשתמש בזה, אבל זה יצליח לפרסר
[System.Serializable]
public class StageStateDto
{
    public MapDto map;
    public HandDto hand;
    public DeckDto deck;
    public ProgressDto progress;
}

[System.Serializable] public class MapDto { public List<PlacedTileDto> placedTiles; }
[System.Serializable] public class PlacedTileDto { public CoordDto coord; public int rotation; public string tileId; }
[System.Serializable] public class HandDto { public int maxHandSize; public List<string> tilesInHand; }
[System.Serializable] public class DeckDto { public List<string> remainingTiles; }
[System.Serializable] public class ProgressDto { public float developedPercent; public int score; public bool isCompleted; }
