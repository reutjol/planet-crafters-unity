using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointRoamer : MonoBehaviour
{
    private List<GameObject> _tiles = new();
    private List<List<int>> _adj = new();
    private string _resourceType;
    private float _moveSpeed = 2.5f;
    private float _waitMin = 1f;
    private float _waitMax = 3f;
    private int _currentIndex = 0;

    public void Initialize(List<GameObject> tiles, List<List<int>> adj, string resourceType, float moveSpeed = 2.5f)
    {
        _tiles = tiles;
        _adj = adj;
        _resourceType = resourceType;
        _moveSpeed = moveSpeed;
        _currentIndex = FindTraversableIndex(-1);
        if (_currentIndex < 0) return;

        var startWp = GetWaypoints(_tiles[_currentIndex]);
        transform.position = startWp != null && startWp.center != null
            ? startWp.center.position
            : _tiles[_currentIndex].transform.position;

        StartCoroutine(Roam());
    }

    public void UpdateTiles(List<GameObject> tiles, List<List<int>> adj)
    {
        var currentTile = (_currentIndex >= 0 && _currentIndex < _tiles.Count)
            ? _tiles[_currentIndex] : null;

        _tiles = tiles;
        _adj = adj;

        if (currentTile != null)
        {
            int newIdx = _tiles.IndexOf(currentTile);
            _currentIndex = newIdx >= 0 ? newIdx : Mathf.Clamp(_currentIndex, 0, _tiles.Count - 1);
        }
        else
        {
            _currentIndex = Mathf.Clamp(_currentIndex, 0, _tiles.Count - 1);
        }

        var wp = GetWaypoints(_tiles[_currentIndex]);
        if (wp == null || !wp.IsTraversable)
            _currentIndex = FindTraversableIndex(-1);
    }

    private HexTileWaypoints GetWaypoints(GameObject tile)
    {
        if (tile == null) return null;
        foreach (var wp in tile.GetComponents<HexTileWaypoints>())
            if (wp.resourceType == _resourceType) return wp;
        return null;
    }

    private int FindTraversableIndex(int exclude)
    {
        for (int i = 0; i < _tiles.Count; i++)
        {
            if (i == exclude) continue;
            var wp = GetWaypoints(_tiles[i]);
            if (wp != null && wp.IsTraversable) return i;
        }
        return -1;
    }

    private bool HasRoadToward(int fromIdx, int toIdx)
    {
        var fromTile = _tiles[fromIdx];
        var toTile = _tiles[toIdx];
        if (fromTile == null || toTile == null) return false;
        var wp = GetWaypoints(fromTile);
        if (wp == null || !wp.IsTraversable || wp.exits == null) return false;

        foreach (var exit in wp.exits)
        {
            if (exit == null) continue;
            float distToTo = Vector3.Distance(exit.position, toTile.transform.position);
            float distToFrom = Vector3.Distance(exit.position, fromTile.transform.position);
            if (distToTo >= distToFrom) continue;

            bool isClosest = true;
            foreach (int otherIdx in _adj[fromIdx])
            {
                if (otherIdx == toIdx) continue;
                var otherTile = _tiles[otherIdx];
                if (otherTile == null) continue;
                if (Vector3.Distance(exit.position, otherTile.transform.position) < distToTo)
                {
                    isClosest = false;
                    break;
                }
            }
            if (isClosest) return true;
        }
        return false;
    }

    private List<int> BfsPath(int from, int to)
    {
        if (from == to) return new List<int>();

        var prev = new int[_tiles.Count];
        for (int i = 0; i < prev.Length; i++) prev[i] = -1;
        var queue = new Queue<int>();
        queue.Enqueue(from);
        prev[from] = from;

        while (queue.Count > 0)
        {
            int curr = queue.Dequeue();
            if (curr == to) break;
            foreach (int next in _adj[curr])
            {
                if (prev[next] != -1) continue;
                var wp = GetWaypoints(_tiles[next]);
                if (wp == null || !wp.IsTraversable) continue;
                if (!HasRoadToward(curr, next) || !HasRoadToward(next, curr)) continue;
                prev[next] = curr;
                queue.Enqueue(next);
            }
        }

        if (prev[to] == -1) return new List<int>();

        var path = new List<int>();
        for (int at = to; at != from; at = prev[at])
            path.Add(at);
        path.Reverse();
        return path;
    }

    private IEnumerator Roam()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_waitMin, _waitMax));

            int target = Random.Range(0, _tiles.Count);
            if (target == _currentIndex) continue;
            var targetWp = GetWaypoints(_tiles[target]);
            if (targetWp == null || !targetWp.IsTraversable) continue;

            var path = BfsPath(_currentIndex, target);
            if (path.Count == 0) continue;

            for (int i = 0; i < path.Count; i++)
            {
                int stepIdx = path[i];
                var tile = _tiles[stepIdx];
                if (tile == null) break;

                var wp = GetWaypoints(tile);
                if (wp == null || !wp.IsTraversable) break;

                bool isFirst = i == 0;
                bool isLast = i == path.Count - 1;

                Transform entry = null;
                if (isFirst)
                {
                    entry = wp.GetClosestExit(transform.position);
                    if (entry != null) yield return MoveToPoint(entry.position);
                }

                Vector3 centerPos = wp.center != null ? wp.center.position : tile.transform.position;
                yield return MoveToPoint(centerPos);

                if (!isLast)
                {
                    var nextTile = _tiles[path[i + 1]];
                    Vector3 nextPos = nextTile != null ? nextTile.transform.position : tile.transform.position;
                    var exit = wp.GetClosestExit(nextPos, entry);
                    if (exit != null) yield return MoveToPoint(exit.position);
                }

                _currentIndex = stepIdx;
            }
        }
    }

    private IEnumerator MoveToPoint(Vector3 dest)
    {
        float dist = Vector3.Distance(transform.position, dest);
        if (dist < 0.01f) yield break;

        float duration = dist / _moveSpeed;
        float elapsed = 0f;
        Vector3 start = transform.position;

        Vector3 dir = new Vector3(dest.x - start.x, 0, dest.z - start.z).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, dest, elapsed / duration);
            yield return null;
        }

        transform.position = dest;
    }
}
