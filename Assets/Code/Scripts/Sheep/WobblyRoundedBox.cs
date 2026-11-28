using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Renders a UI rounded rectangle with an animated organic wobble effect along its perimeter.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class WobblyRoundedBox : MaskableGraphic
{
    [Header("Shape Settings")]
    [Tooltip("Radius of the rounded corners in pixels.")]
    [SerializeField] private float _cornerRadius = 25f;
    [Tooltip("Total number of vertices along the perimeter. Higher values produce smoother curves.")]
    [Range(16, 200)]
    [SerializeField] private int _pointCount = 64;

    [Header("Wobble Animation")]
    [Tooltip("Maximum distance in pixels that vertices displace along their normals.")]
    [SerializeField] private float _wobbleAmplitude = 4f;
    [Tooltip("Speed at which the organic wobble animation progresses over time.")]
    [SerializeField] private float _wobbleSpeed = 2.5f;
    [Tooltip("Spatial frequency of the Perlin noise. Higher values create tighter ripples.")]
    [SerializeField] private float _wobbleFrequency = 0.15f;

    private readonly List<Vector2> _basePoints = new List<Vector2>();
    private readonly List<Vector2> _normals = new List<Vector2>();
    private bool _isPerimeterDirty = true;

    /// <summary>
    /// Marks the mesh dirty every frame when wobble animation is active.
    /// </summary>
    private void Update()
    {
        if (_wobbleAmplitude > 0f)
        {
            SetVerticesDirty();
        }
    }

    /// <summary>
    /// Flags the base perimeter for recalculation when the UI element resizes.
    /// </summary>
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        _isPerimeterDirty = true;
    }

    /// <summary>
    /// Generates the mesh vertices and triangles, applying Perlin noise displacement to the perimeter.
    /// </summary>
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();
        if (rect.width <= 0f || rect.height <= 0f) return;

        if (_isPerimeterDirty || _basePoints.Count != _pointCount)
        {
            BuildRoundedPerimeter(rect);
            _isPerimeterDirty = false;
        }

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;
        vert.position = rect.center;
        vh.AddVert(vert);

        float time = Time.time * _wobbleSpeed;
        int count = _basePoints.Count;

        for (int i = 0; i < count; i++)
        {
            float noise = Mathf.PerlinNoise(i * _wobbleFrequency, time) * 2f - 1f;
            vert.position = _basePoints[i] + _normals[i] * (noise * _wobbleAmplitude);
            vh.AddVert(vert);

            int current = i + 1;
            int next = (i + 1) % count + 1;
            vh.AddTriangle(0, next, current);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Flags the perimeter for recalculation when values change in the Unity Inspector.
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
        _isPerimeterDirty = true;
    }
#endif

    /// <summary>
    /// Pre-calculates evenly spaced base positions and outward normals along the rounded rectangle.
    /// </summary>
    private void BuildRoundedPerimeter(Rect rect)
    {
        _basePoints.Clear();
        _normals.Clear();

        float r = Mathf.Clamp(_cornerRadius, 0f, Mathf.Min(rect.width, rect.height) * 0.5f);
        float w = rect.width - 2f * r;
        float h = rect.height - 2f * r;
        float arc = 0.5f * Mathf.PI * r;
        float perimeter = 2f * (w + h) + 4f * arc;

        if (perimeter <= 0f) return;

        Vector2 tr = new Vector2(rect.xMax - r, rect.yMax - r);
        Vector2 br = new Vector2(rect.xMax - r, rect.yMin + r);
        Vector2 bl = new Vector2(rect.xMin + r, rect.yMin + r);
        Vector2 tl = new Vector2(rect.xMin + r, rect.yMax - r);

        for (int i = 0; i < _pointCount; i++)
        {
            float d = (i / (float)_pointCount) * perimeter;
            EvaluatePerimeterPoint(d, r, w, h, arc, tr, br, bl, tl, out Vector2 pos, out Vector2 norm);
            _basePoints.Add(pos);
            _normals.Add(norm);
        }
    }

    /// <summary>
    /// Maps a linear distance along the perimeter to its corresponding edge or corner arc point.
    /// </summary>
    private void EvaluatePerimeterPoint(float d, float r, float w, float h, float arc, Vector2 tr, Vector2 br, Vector2 bl, Vector2 tl, out Vector2 pos, out Vector2 norm)
    {
        if (d < w) { pos = new Vector2(tl.x + d, tl.y + r); norm = Vector2.up; return; }
        d -= w;
        if (d < arc) { GetArcPoint(tr, r, Mathf.PI * 0.5f, d / arc, out pos, out norm); return; }
        d -= arc;
        if (d < h) { pos = new Vector2(tr.x + r, tr.y - d); norm = Vector2.right; return; }
        d -= h;
        if (d < arc) { GetArcPoint(br, r, 0f, d / arc, out pos, out norm); return; }
        d -= arc;
        if (d < w) { pos = new Vector2(br.x - d, br.y - r); norm = Vector2.down; return; }
        d -= w;
        if (d < arc) { GetArcPoint(bl, r, -Mathf.PI * 0.5f, d / arc, out pos, out norm); return; }
        d -= arc;
        if (d < h) { pos = new Vector2(bl.x - r, bl.y + d); norm = Vector2.left; return; }
        d -= h;
        GetArcPoint(tl, r, Mathf.PI, d / Mathf.Max(arc, 0.0001f), out pos, out norm);
    }

    /// <summary>
    /// Calculates a position and normal along a 90-degree clockwise corner arc.
    /// </summary>
    private void GetArcPoint(Vector2 center, float radius, float startAngle, float t, out Vector2 pos, out Vector2 norm)
    {
        float angle = startAngle - t * (Mathf.PI * 0.5f);
        norm = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        pos = center + norm * radius;
    }
}
