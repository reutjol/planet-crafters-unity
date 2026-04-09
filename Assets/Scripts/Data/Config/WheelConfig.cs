using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WheelConfig", menuName = "Game/Wheel/Wheel Config")]
public class WheelConfig : ScriptableObject
{
    public List<WheelRewardDto> rewards = new List<WheelRewardDto>();

    [Header("Wheel Layout")]
    public int sliceCount = 8;

    [Header("Spin Settings")]
    public float spinDuration = 3f;
    public int extraFullRotations = 4;

    [Header("Cooldown Settings")]
    public int cooldownHours = 24;
}