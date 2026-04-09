<<<<<<< HEAD
using System.Collections.Generic;
=======
﻿using System.Collections.Generic;
>>>>>>> origin/main
using UnityEngine;
using System;

/// <summary>
/// Manages the player's hand of tiles (3 slots).
<<<<<<< HEAD
/// Renders the hand state received from the server.
=======
/// Handles drawing from deck, shifting tiles after placement, and syncing with server state.
>>>>>>> origin/main
/// Only the first slot is draggable.
/// </summary>
public class HandController : MonoBehaviour
{
<<<<<<< HEAD
=======
    [Header("Prefab")]
    public GameObject tilePrefab;

>>>>>>> origin/main
    [Header("Hand Slots (size = 3)")]
    public Transform[] slots; // Slot01, Slot02, Slot03

    [Header("Refs")]
    public MapController mapController;
    public TileFactory factory;

<<<<<<< HEAD
    private DraggableTile[] tiles = new DraggableTile[3];

    public event Action OnHandStateChanged;
    public event Action OnHandAndDeckEmpty;

    // ===============================
    // Load hand from server state
    // ===============================
    public void LoadFromServer(HandDto hand, DeckDto deckDto)
    {
        ClearHandVisuals();

        List<string> tilesInHand = hand?.tilesInHand ?? new List<string>();

=======
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
>>>>>>> origin/main
        for (int i = 0; i < 3; i++)
        {
            if (i < tilesInHand.Count && !string.IsNullOrEmpty(tilesInHand[i]))
                SpawnSpecificTemplateToSlot(i, tilesInHand[i]);
            else
                tiles[i] = null;
        }

        UpdateInteractivity();
        OnHandStateChanged?.Invoke();
<<<<<<< HEAD

        bool handEmpty = tilesInHand.Count == 0;
        bool deckEmpty = (deckDto?.remainingTiles == null || deckDto.remainingTiles.Count == 0);
        if (handEmpty && deckEmpty)
            OnHandAndDeckEmpty?.Invoke();
=======
>>>>>>> origin/main
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

<<<<<<< HEAD
        // Move to "Hand" layer so the HandCamera renders it on top of the map.
        // OnMouseDown still fires (camera eventMask is separate from cullingMask).
        int handLayer = LayerMask.NameToLayer("Hand");
        if (handLayer != -1)
            SetLayerRecursively(go, handLayer);

=======
>>>>>>> origin/main
        var drag = go.GetComponent<DraggableTile>();
        tiles[slotIndex] = drag;

        if (drag != null)
        {
<<<<<<< HEAD
            drag.templateId = templateId;
            drag.handController = this;
            drag.mapController = mapController;
            drag.SetHome(slots[slotIndex]);
            drag.SetDraggable(slotIndex == 0);
        }
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

=======
            drag.templateId = templateId;        // Important for tracking
            drag.handController = this;
            drag.mapController = mapController;
            drag.SetHome(slots[slotIndex]);
            drag.SetDraggable(slotIndex == 0);   // Only slot 0 is draggable
        }
    }

>>>>>>> origin/main
    private void ClearHandVisuals()
    {
        for (int i = 0; i < 3; i++)
        {
            if (tiles[i] != null) Destroy(tiles[i].gameObject);
            tiles[i] = null;
        }
    }

<<<<<<< HEAD
    public void SimulateGameOver() => OnHandAndDeckEmpty?.Invoke();
=======
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
>>>>>>> origin/main

    private void UpdateInteractivity()
    {
        for (int i = 0; i < 3; i++)
        {
            if (tiles[i] != null)
                tiles[i].SetDraggable(i == 0);
        }
    }
<<<<<<< HEAD
=======

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

>>>>>>> origin/main
}
