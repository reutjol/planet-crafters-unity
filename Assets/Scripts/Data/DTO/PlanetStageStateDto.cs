using System;

[Serializable]
public class SaveStageStateRequestDto
{
    public string planetId;
    public string stageId;
    public StageStateInnerDto state;
}

[Serializable]
public class StageStateInnerDto
{
    public MapDto map;
    public HandDto hand;
    public DeckDto deck;
    public ProgressDto progress;
}

[Serializable]
public class PlanetStageStateDto
{
    public string planetId;
    public string stageId;
    public MapDto map;
    public HandDto hand;
    public DeckDto deck;
    public ProgressDto progress;
}

