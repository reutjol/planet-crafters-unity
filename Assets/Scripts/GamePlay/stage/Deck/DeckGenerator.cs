using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Utility for generating randomized tile decks from available templates.
/// </summary>
public static class DeckGenerator
{
    public static List<string> GenerateRandom30(IReadOnlyDictionary<string, HexTileTemplateDto> templatesById, int count = 30)
    {
        var ids = templatesById.Keys.ToList();
        var deck = new List<string>(count);

        if (ids.Count == 0) return deck;

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, ids.Count);
            deck.Add(ids[idx]); 
        }

        return deck;
    }
}
