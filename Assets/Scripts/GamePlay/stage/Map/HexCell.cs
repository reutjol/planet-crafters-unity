using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HexCell : MonoBehaviour
{
    [Header("Axial Coordinates")]
    [FormerlySerializedAs("q")] [SerializeField] private int _q;
    [FormerlySerializedAs("r")] [SerializeField] private int _r;

    [Header("State")]
    [FormerlySerializedAs("isPlusCell")] [SerializeField] private bool _isPlusCell = true;
    [FormerlySerializedAs("occupied")]   [SerializeField] private bool _occupied = false;

    [Header("Visual")]
    [FormerlySerializedAs("rend")]           [SerializeField] private Renderer _rend;
    [FormerlySerializedAs("highlightColor")] [SerializeField] private Color _highlightColor = Color.yellow;

    public int q => _q;
    public int r => _r;
    public bool isPlusCell => _isPlusCell;
    public bool occupied => _occupied;

    private static readonly int ColorPropId = Shader.PropertyToID("_Color");
    private static readonly Dictionary<int, HexCell> s_ColliderMap = new();

    private MaterialPropertyBlock _mpb;
    private Color _defaultColor;

    private void Awake()
    {
        if (_rend == null)
            _rend = GetComponentInChildren<Renderer>();

        _mpb = new MaterialPropertyBlock();
        _rend.GetPropertyBlock(_mpb);
        _defaultColor = _rend.sharedMaterial.color;

        foreach (var col in GetComponentsInChildren<Collider>(true))
            s_ColliderMap[col.GetInstanceID()] = this;
    }

    private void OnDestroy()
    {
        foreach (var col in GetComponentsInChildren<Collider>(true))
            s_ColliderMap.Remove(col.GetInstanceID());
    }

    public static bool TryGetFromCollider(Collider col, out HexCell cell)
        => s_ColliderMap.TryGetValue(col.GetInstanceID(), out cell);

    public void Init(int q, int r, bool isPlus)
    {
        _q = q;
        _r = r;
        _isPlusCell = isPlus;
        _occupied = false;
    }

    public void SetHighlight(bool on)
    {
        if (_rend == null) return;
        _mpb.SetColor(ColorPropId, on ? _highlightColor : _defaultColor);
        _rend.SetPropertyBlock(_mpb);
    }
}
