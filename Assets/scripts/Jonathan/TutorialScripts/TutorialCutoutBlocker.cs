using UnityEngine;
using UnityEngine.UI;

public class TutorialCutoutBlocker : Graphic, ICanvasRaycastFilter
{
    [SerializeField] private RectTransform overlayRoot;

    [SerializeField] private RectTransform target;
    [SerializeField] private float padding = 0f;

    private bool hasHole;
    private bool blockClicksInsideHole;
    private Rect holeRect;

    protected override void Awake()
    {
        base.Awake();

        if (overlayRoot == null)
            overlayRoot = rectTransform;

        raycastTarget = true;
    }

    public void SetHole(RectTransform newTarget, bool blockClicksInsideHole = false)
    {
        target = newTarget;
        this.blockClicksInsideHole = blockClicksInsideHole;
        hasHole = target != null;

        if (hasHole)
            CalculateHole();

        SetVerticesDirty();
    }

    public void ClearHole()
    {
        target = null;
        hasHole = false;
        blockClicksInsideHole = false;
        SetVerticesDirty();
    }

    private void CalculateHole()
    {
        if (overlayRoot == null || target == null)
        {
            hasHole = false;
            return;
        }

        Canvas.ForceUpdateCanvases();

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        for (int i = 0; i < 4; i++)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRoot,
                screenPoint,
                cam,
                out Vector2 localPoint
            );

            min = Vector2.Min(min, localPoint);
            max = Vector2.Max(max, localPoint);
        }

        Rect rootRect = overlayRoot.rect;

        float xMin = Mathf.Clamp(min.x - padding, rootRect.xMin, rootRect.xMax);
        float xMax = Mathf.Clamp(max.x + padding, rootRect.xMin, rootRect.xMax);
        float yMin = Mathf.Clamp(min.y - padding, rootRect.yMin, rootRect.yMax);
        float yMax = Mathf.Clamp(max.y + padding, rootRect.yMin, rootRect.yMax);

        holeRect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect r = overlayRoot != null ? overlayRoot.rect : rectTransform.rect;

        if (!hasHole)
        {
            AddQuad(vh, r.xMin, r.yMin, r.xMax, r.yMax);
            return;
        }

        Rect h = holeRect;

        // Top
        AddQuad(vh, r.xMin, h.yMax, r.xMax, r.yMax);

        // Bottom
        AddQuad(vh, r.xMin, r.yMin, r.xMax, h.yMin);

        // Left
        AddQuad(vh, r.xMin, h.yMin, h.xMin, h.yMax);

        // Right
        AddQuad(vh, h.xMax, h.yMin, r.xMax, h.yMax);
    }

    private void AddQuad(VertexHelper vh, float xMin, float yMin, float xMax, float yMax)
    {
        if (xMax <= xMin || yMax <= yMin)
            return;

        int startIndex = vh.currentVertCount;

        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;

        vert.position = new Vector3(xMin, yMin);
        vh.AddVert(vert);

        vert.position = new Vector3(xMin, yMax);
        vh.AddVert(vert);

        vert.position = new Vector3(xMax, yMax);
        vh.AddVert(vert);

        vert.position = new Vector3(xMax, yMin);
        vh.AddVert(vert);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (!hasHole)
            return true; // block whole screen

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            overlayRoot,
            screenPoint,
            eventCamera,
            out Vector2 localPoint
        );

        bool insideHole = holeRect.Contains(localPoint);

        // Outside hole = always block.
        if (!insideHole)
            return true;

        // Inside hole
        // false = allow click through to the highlighted UI.
        // true = block click even inside the highlight.
        return blockClicksInsideHole;
    }
}