using System;

[Serializable]
public class BoosterInventoryDto
{
    public int doubleScore;
    public int cancelPlacement;
    public int addHex;
}

[Serializable]
public class AddCoinsRequestDto
{
    public int amount;
}

[Serializable]
public class AddCoinsResponseDto
{
    public int totalCoins;
}
