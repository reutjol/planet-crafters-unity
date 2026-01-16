using UnityEngine;

public class PlanetSpawner : MonoBehaviour
{
    [SerializeField] Transform anchor;
    GameObject current;

   public void Spawn(GameObject prefab, System.Action onClicked = null)
    {
        Clear();
        if (prefab == null) return;
        if (anchor == null) anchor = transform;

        current = Instantiate(prefab, anchor.position, anchor.rotation, anchor);

        var clickable = current.GetComponentInChildren<PlanetClickable>(true);
        if (clickable != null && onClicked != null)
            clickable.Clicked = onClicked;
        else
            Debug.LogError("[PlanetSpawner] PlanetClickable missing on prefab (root/children)");

    }


    public void Clear()
    {
        if (current != null)
            Destroy(current);

        current = null;
    }
}
