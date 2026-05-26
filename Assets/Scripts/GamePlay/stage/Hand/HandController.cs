using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HandController : MonoBehaviour
{
    [FormerlySerializedAs("slots")] [SerializeField] private Transform[] _slots;

    private MapController _mapController;
    private TileFactory _factory;

    private readonly DraggableTile[] _tiles = new DraggableTile[3];

    public event Action OnHandStateChanged;
    public event Action OnHandAndDeckEmpty;

    public void Initialize(TileFactory factory, MapController mapController)
    {
        _factory = factory;
        _mapController = mapController;
    }

    public void LoadFromServer(HandDto hand, DeckDto deckDto)
    {
        ClearHandVisuals();

        List<string> tilesInHand = hand?.tilesInHand ?? new List<string>();

        for (int i = 0; i < 3; i++)
        {
            if (i < tilesInHand.Count && !string.IsNullOrEmpty(tilesInHand[i]))
                SpawnSpecificTemplateToSlot(i, tilesInHand[i]);
            else
                _tiles[i] = null;
        }

        UpdateInteractivity();
        OnHandStateChanged?.Invoke();

        bool handEmpty = tilesInHand.Count == 0;
        bool deckEmpty = deckDto?.remainingTiles == null || deckDto.remainingTiles.Count == 0;
        if (handEmpty && deckEmpty)
            OnHandAndDeckEmpty?.Invoke();
    }

    private void SpawnSpecificTemplateToSlot(int slotIndex, string templateId)
    {
        if (_factory == null)
        {
            Debug.LogError("[HandController] TileFactory is null");
            return;
        }
        if (_slots == null || slotIndex < 0 || slotIndex >= _slots.Length || _slots[slotIndex] == null)
        {
            Debug.LogError($"[HandController] Invalid slot index {slotIndex} or slots not assigned");
            return;
        }

        var go = _factory.CreateTileByTemplateId(templateId, _slots[slotIndex]);
        if (go == null) return;

        int handLayer = LayerMask.NameToLayer("Hand");
        if (handLayer != -1)
            SetLayerRecursively(go, handLayer);

        var drag = go.GetComponent<DraggableTile>();
        _tiles[slotIndex] = drag;

        if (drag != null)
        {
            drag.Initialize(templateId, this, _mapController);
            drag.SetHome(_slots[slotIndex]);
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
            if (_tiles[i] != null) Destroy(_tiles[i].gameObject);
            _tiles[i] = null;
        }
    }

    public void SimulateGameOver() => OnHandAndDeckEmpty?.Invoke();

    private void UpdateInteractivity()
    {
        for (int i = 0; i < 3; i++)
        {
            if (_tiles[i] != null)
                _tiles[i].SetDraggable(i == 0);
        }
    }
}
