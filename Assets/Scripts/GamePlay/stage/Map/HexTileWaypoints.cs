using UnityEngine;

public class HexTileWaypoints : MonoBehaviour
{
    public string resourceType;
    public Transform center;
    public Transform[] exits;

    public bool IsTraversable => exits != null && exits.Length > 0;

    public Transform[] GetWaypointsThrough(Vector3 comingFrom, Vector3 goingTo)
    {
        var entry = GetClosestExit(comingFrom, null);
        var exit = GetClosestExit(goingTo, entry);
        if (entry == null || exit == null) return null;
        return new[] { entry, center, exit };
    }

    public Transform GetClosestExit(Vector3 position, Transform exclude = null)
    {
        Transform closest = null;
        float minDist = float.MaxValue;
        foreach (var e in exits)
        {
            if (e == exclude) continue;
            float d = Vector3.Distance(e.position, position);
            if (d < minDist) { minDist = d; closest = e; }
        }
        return closest;
    }
}
