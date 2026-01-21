using UnityEngine;

/// <summary>
/// Represents a single hex cell on the grid (typically a "plus" placeholder for tile placement).
/// Tracks occupancy state and provides visual highlighting.
/// </summary>
public class HexCell : MonoBehaviour
{
    [Header("Axial Coordinates")]
    public int q;
    public int r;

    [Header("State")]
    public bool isPlusCell = true;
    public bool occupied = false;

    [Header("Visual")]
    [SerializeField] private Renderer rend;

    private Color defaultColor;
    [SerializeField] private Color highlightColor = Color.yellow;

    private void Awake()
    {
        if (rend == null)
            rend = GetComponentInChildren<Renderer>();

        defaultColor = rend.material.color;
    }

    public void Init(int q, int r, bool isPlus)
    {
        this.q = q;
        this.r = r;
        isPlusCell = isPlus;
        occupied = false;
    }

    public void SetOccupied(bool value)
    {
        occupied = value;
        isPlusCell = !value;
        SetHighlight(false);
    }

    public void SetHighlight(bool on)
    {
        if (rend == null) return;
        rend.material.color = on ? highlightColor : defaultColor;
    }
}
