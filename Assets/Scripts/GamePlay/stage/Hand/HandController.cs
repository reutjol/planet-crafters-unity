using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Manages the player's hand of tiles (3 slots).
/// Renders the hand state received from the server.
/// Only the first slot is draggable.
/// </summary>
public class HandController : MonoBehaviour
{
    [Header("Hand Slots (size = 3)")]
    public Transform[] slots; // Slot01, Slot02, Slot03

    [Header("Refs")]
    public MapController mapController;
    public TileFactory factory;

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

        for (int i = 0; i < 3; i++)
        {
            if (i < tilesInHand.Count && !string.IsNullOrEmpty(tilesInHand[i]))
                SpawnSpecificTemplateToSlot(i, tilesInHand[i]);
            else
                tiles[i] = null;
        }

        UpdateInteractivity();
        OnHandStateChanged?.Invoke();

        bool handEmpty = tilesInHand.Count == 0;
        bool deckEmpty = (deckDto?.remainingTiles == null || deckDto.remainingTiles.Count == 0);
        if (handEmpty && deckEmpty)
            OnHandAndDeckEmpty?.Invoke();
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

        // Move to "Hand" layer so the HandCamera renders it on top of the map.
        // OnMouseDown still fires (camera eventMask is separate from cullingMask).
        int handLayer = LayerMask.NameToLayer("Hand");
        if (handLayer != -1)
            SetLayerRecursively(go, handLayer);

        var drag = go.GetComponent<DraggableTile>();
        tiles[slotIndex] = drag;

        if (drag != null)
        {
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

    private void ClearHandVisuals()
    {
        for (int i = 0; i < 3; i++)
        {
            if (tiles[i] != null) Destroy(tiles[i].gameObject);
            tiles[i] = null;
        }
    }

    public void SimulateGameOver() => OnHandAndDeckEmpty?.Invoke();

    private void UpdateInteractivity()
    {
        for (int i = 0; i < 3; i++)
        {
            if (tiles[i] != null)
                tiles[i].SetDraggable(i == 0);
        }
    }
}
