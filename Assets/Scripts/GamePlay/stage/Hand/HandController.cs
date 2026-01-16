using System.Collections.Generic;
using UnityEngine;
using System;

public class HandController : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject tilePrefab;

    [Header("Hand Slots (size = 3)")]
    public Transform[] slots; // Slot01, Slot02, Slot03

    [Header("Refs")]
    public MapController mapController;
    public TileFactory factory;

    private List<string> deck = new();
    private int deckIndex = 0;

    private DraggableTile[] tiles = new DraggableTile[3];

    public event Action OnHandStateChanged;

    // ===============================
    // Load initial hand + deck from server
    // ===============================
    public void LoadFromServer(HandDto hand, DeckDto deckDto)
    {
        // deck
        deck = deckDto?.remainingTiles != null
            ? new List<string>(deckDto.remainingTiles)
            : new List<string>();
        deckIndex = 0;

        // clean visuals
        ClearHandVisuals();

        // ✅ FIX: tilesInHand is List<string> so default must also be List<string>
        List<string> tilesInHand = hand?.tilesInHand ?? new List<string>();

        // fill 3 slots
        for (int i = 0; i < 3; i++)
        {
            if (i < tilesInHand.Count && !string.IsNullOrEmpty(tilesInHand[i]))
                SpawnSpecificTemplateToSlot(i, tilesInHand[i]);
            else
                tiles[i] = null;
        }

        UpdateInteractivity();
        OnHandStateChanged?.Invoke();
    }

    private void SpawnSpecificTemplateToSlot(int slotIndex, string templateId)
    {
        if (factory == null)
        {
            Debug.LogError("[HandController] TileFactory is null");
            return;
        }
        if (slots == null || slotIndex < 0 || slotIndex >= slots.Length || slots[slotIndex] == null)
        {
            Debug.LogError($"[HandController] Invalid slot index {slotIndex} or slots not assigned");
            return;
        }

        var go = factory.CreateTileByTemplateId(templateId, slots[slotIndex]);
        if (go == null) return;

        var drag = go.GetComponent<DraggableTile>();
        tiles[slotIndex] = drag;

        if (drag != null)
        {
            drag.templateId = templateId;        // חשוב שיהיה
            drag.handController = this;
            drag.mapController = mapController;
            drag.SetHome(slots[slotIndex]);
            drag.SetDraggable(slotIndex == 0);   // רק סלוט 0
        }
    }

    private void ClearHandVisuals()
    {
        for (int i = 0; i < 3; i++)
        {
            if (tiles[i] != null) Destroy(tiles[i].gameObject);
            tiles[i] = null;
        }
    }

    private void SpawnNewToSlotFromDeck(int slotIndex)
    {
        if (factory == null)
        {
            Debug.LogError("[HandController] TileFactory is null");
            return;
        }

        if (deck == null || deckIndex >= deck.Count)
        {
            Debug.LogWarning("[HandController] Deck empty.");
            return;
        }

        string templateId = deck[deckIndex++];
        if (string.IsNullOrEmpty(templateId)) return;

        var go = factory.CreateTileByTemplateId(templateId, slots[slotIndex]);
        if (go == null) return;

        var drag = go.GetComponent<DraggableTile>();
        tiles[slotIndex] = drag;

        if (drag != null)
        {
            drag.templateId = templateId;
            drag.handController = this;
            drag.mapController = mapController;
            drag.SetHome(slots[slotIndex]);
        }
        OnHandStateChanged?.Invoke();
    }

    // ===============================
    // Called when slot 1 tile placed
    // ===============================
    public void OnTilePlacedFromSlot1(DraggableTile placedTile)
    {
        // Slot 1 is now on map → remove from hand
        tiles[0] = null;

        // Shift forward
        ShiftTile(1, 0);
        ShiftTile(2, 1);

        // Spawn new tile to slot 3
        SpawnNewToSlotFromDeck(2);

        UpdateInteractivity();
        OnHandStateChanged?.Invoke();
    }

    private void ShiftTile(int from, int to)
    {
        var tile = tiles[from];
        tiles[to] = tile;
        tiles[from] = null;

        if (tile == null) return;

        tile.transform.SetParent(slots[to], true);
        tile.transform.localPosition = Vector3.zero;
        tile.transform.localRotation = Quaternion.identity;
        tile.SetHome(slots[to]);
    }

    private void UpdateInteractivity()
    {
        for (int i = 0; i < 3; i++)
        {
            if (tiles[i] != null)
                tiles[i].SetDraggable(i == 0);
        }
    }

    public HandDto BuildHandDto()
    {
        var list = new List<string>(3);
        for (int i = 0; i < 3; i++)
            list.Add(tiles[i] != null ? tiles[i].templateId : null);

        return new HandDto
        {
            maxHandSize = 3,          
            tilesInHand = list
        };
    }


    public DeckDto BuildDeckDto()
    {
        var remaining = new List<string>();
        if (deck != null)
        {
            for (int i = deckIndex; i < deck.Count; i++)
                remaining.Add(deck[i]);
        }
        return new DeckDto { remainingTiles = remaining };
    }

}
