using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class StageState
{
    public string _id;
    public string userId;
    public string stageId;

    public MapState map;
    public HandState hand;
    public DeckState deck;
    public ProgressState progress;
}
