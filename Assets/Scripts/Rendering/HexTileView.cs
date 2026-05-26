using UnityEngine;

public class HexTileView : MonoBehaviour
{
    private string _templateId;
    private string _templateType;
    private string[] _edges = new string[6];

    public string TemplateId => _templateId;
    public string TemplateType => _templateType;
    public string[] Edges => _edges;

    public void SetData(string templateId, string templateType, string[] edges)
    {
        _templateId = templateId;
        _templateType = templateType;
        _edges = edges;
    }

    public void RotateEdgesCW()
    {
        if (_edges == null || _edges.Length != 6) return;
        string last = _edges[5];
        for (int i = 5; i > 0; i--) _edges[i] = _edges[i - 1];
        _edges[0] = last;
    }
}
