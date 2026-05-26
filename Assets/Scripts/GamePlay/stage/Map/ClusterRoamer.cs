using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterRoamer : MonoBehaviour
{
    private List<Vector3> _positions = new();
    private List<List<int>> _adj = new();
    private float _moveSpeed = 2.5f;
    private float _waitMin = 1f;
    private float _waitMax = 3f;
    private int _currentIndex = 0;

    public void Initialize(List<Vector3> positions, List<List<int>> adj, float moveSpeed = 2.5f)
    {
        _positions = new List<Vector3>(positions);
        _adj = adj;
        _moveSpeed = moveSpeed;
        _currentIndex = Random.Range(0, _positions.Count);
        transform.position = _positions[_currentIndex];
        StartCoroutine(Roam());
    }

    public void UpdatePositions(List<Vector3> positions, List<List<int>> adj)
    {
        _positions = new List<Vector3>(positions);
        _adj = adj;
        _currentIndex = Mathf.Clamp(_currentIndex, 0, _positions.Count - 1);
    }

    private List<int> BfsPath(int from, int to)
    {
        if (from == to) return new List<int> { from };

        var prev = new int[_positions.Count];
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
                prev[next] = curr;
                queue.Enqueue(next);
            }
        }

        if (prev[to] == -1) return new List<int> { to };

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

            int target = Random.Range(0, _positions.Count);
            if (target == _currentIndex) continue;

            var path = BfsPath(_currentIndex, target);

            foreach (int step in path)
            {
                Vector3 dest = _positions[step];
                float dist = Vector3.Distance(transform.position, dest);
                if (dist < 0.01f) { _currentIndex = step; continue; }

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
                _currentIndex = step;
            }
        }
    }
}
