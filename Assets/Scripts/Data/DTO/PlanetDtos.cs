using System.Collections.Generic;

// ==========================================
// Planet & Stage DTOs
// ==========================================

[System.Serializable]
public class PlanetDto
{
    public string planetId;
    public List<StageDto> stages;
    public int totalCoins;
}

[System.Serializable]
public class StageDto
{
    public string stageId;
    public StageMetaDto meta;
    public StageStateDto state;
}

[System.Serializable]
public class StageMetaDto
{
    public CoordDto coord;
    public string resourceType;
    public bool isUnlocked;
    public bool isStarted;
    public bool isCompleted;
    public int coinsAwarded;
    public string lastPlayedAt; // ISO string format
}

[System.Serializable]
public class CoordDto
{
    public int q;
    public int r;
}

// ==========================================
// Stage State DTOs
// ==========================================

[System.Serializable]
public class StageStateDto
{
    public MapDto map;
    public HandDto hand;
    public DeckDto deck;
    public ProgressDto progress;
}

[System.Serializable]
public class MapDto
{
    public List<PlacedTileDto> placedTiles;
}

[System.Serializable]
public class FaceDataDto
{
    public string resource;
    public int variant;
    public int level;
}

[System.Serializable]
public class CenterDataDto
{
    public string resource;
    public int level;
}

[System.Serializable]
public class PlacedTileDto
{
    public CoordDto coord;
    public int rotation;
    public string tileId;
    public System.Collections.Generic.List<FaceDataDto> faces;
    public CenterDataDto center;
}

[System.Serializable]
public class HandDto
{
    public int maxHandSize;
    public List<string> tilesInHand;
}

[System.Serializable]
public class DeckDto
{
    public List<string> remainingTiles;
}

[System.Serializable]
public class ProgressDto
{
    public float developedPercent;
    public int score;
    public bool isCompleted;
    public int baseResourceConnections;
}

// ==========================================
// API Request/Response DTOs
// ==========================================

[System.Serializable]
public class PlanetStageStateDto
{
    public string planetId;
    public string stageId;
    public MapDto map;
    public HandDto hand;
    public DeckDto deck;
    public ProgressDto progress;
    public int targetScore;
    public int coinsAwarded;
    public List<CoordDto> scoredConnections;
    public int bonusPoints;
}

[System.Serializable]
public class SaveStageStateRequestDto
{
    public string planetId;
    public string stageId;
    public StageStateInnerDto state;
}

[System.Serializable]
public class StageStateInnerDto
{
    public MapDto map;
    public HandDto hand;
    public DeckDto deck;
    public ProgressDto progress;
}

[System.Serializable]
public class PlaceTileRequestDto
{
    public string tileId;
    public CoordDto coord;
    public int rotation;
}
