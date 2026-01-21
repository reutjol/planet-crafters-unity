using System;

/// <summary>
/// Data Transfer Object for hexagonal tile templates from the server.
/// Contains tile configuration including center, edges, type, and level.
/// </summary>

[Serializable]
public class HexTileTemplateDto
{
    public string _id;
    public string type;
    public string center;
    public string[] edges; // length 6
    public int level;
    public int __v;
    public string createdAt;
    public string updatedAt;
}

[Serializable]
public class HexTileTemplateListWrapper
{
    public HexTileTemplateDto[] items;
}
