using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class WheelSegmentGraphic : Graphic
{
    [SerializeField] private float startAngle;
    [SerializeField] private float endAngle = 45f;
    [SerializeField, Range(0f, 1f)] private float innerRadius = 0.08f;
    [SerializeField, Range(0f, 1f)] private float outerRadius = 0.48f;
    [SerializeField, Range(0f, 8f)] private float gapDegrees = 1.2f;
    [SerializeField, Range(2, 48)] private int segmentResolution = 12;

    public void Configure(float start, float end, float inner, float outer, float gap)
    {
        startAngle = start;
        endAngle = end;
        innerRadius = Mathf.Clamp01(inner);
        outerRadius = Mathf.Clamp01(outer);
        gapDegrees = Mathf.Clamp(gap, 0f, 8f);
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Rect rect = rectTransform.rect;
        float maxRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
        float inner = Mathf.Clamp01(innerRadius) * maxRadius;
        float outer = Mathf.Clamp01(outerRadius) * maxRadius;

        if (outer <= 0f || outer <= inner)
            return;

        float halfGap = gapDegrees * 0.5f;
        float start = startAngle + halfGap;
        float end = endAngle - halfGap;
        float sweep = end - start;

        if (Mathf.Abs(sweep) < 0.01f)
            return;

        int steps = Mathf.Max(2, Mathf.CeilToInt(Mathf.Abs(sweep) / segmentResolution));
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        Vector2 center = rect.center;

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            float angle = Mathf.Lerp(start, end, t) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            vertex.position = center + (direction * outer);
            vertexHelper.AddVert(vertex);

            vertex.position = center + (direction * inner);
            vertexHelper.AddVert(vertex);
        }

        for (int i = 0; i < steps; i++)
        {
            int outerA = i * 2;
            int innerA = outerA + 1;
            int outerB = outerA + 2;
            int innerB = outerA + 3;

            vertexHelper.AddTriangle(outerA, outerB, innerB);
            vertexHelper.AddTriangle(innerB, innerA, outerA);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
