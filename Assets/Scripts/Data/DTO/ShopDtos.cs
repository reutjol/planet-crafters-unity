using System;
using System.Collections.Generic;

[Serializable]
public class AvatarCatalogEntryDto
{
    public string id;
    public string unlockType; // "default", "purchase", "stage"
    public int stageRequired;
    public int price;
}

[Serializable]
public class ShopProfileDto
{
    public int coins;
    public string selectedAvatar;
    public List<string> ownedAvatars;
    public BoosterInventoryDto boosters;
    public int completedStages;
    public List<AvatarCatalogEntryDto> catalog;
}

[Serializable]
public class ShopPurchaseResultDto
{
    public bool success;
    public string reason;
    public int coins;
    public List<string> ownedAvatars;
    public BoosterInventoryDto boosters;
}

[Serializable]
public class ShopSetAvatarRequestDto
{
    public string avatarId;
}

[Serializable]
public class ShopSetAvatarResultDto
{
    public bool success;
    public string reason;
    public string selectedAvatar;
}

[Serializable]
public class ShopAddCoinsRequestDto
{
    public int amount;
}

[Serializable]
public class ShopAddCoinsResultDto
{
    public int coins;
}
