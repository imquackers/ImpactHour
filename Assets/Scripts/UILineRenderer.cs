using UnityEngine;
using UnityEngine.UI;

// Draws a straight UI line between two RectTransforms.
// Implementation detail:
// This uses a stretched + rotated UI Image instead of
// Unity's LineRenderer (which works in world space).
public class UILineRenderer : MonoBehaviour
{
    // Start and end UI elements
    public RectTransform startPoint;
    public RectTransform endPoint;

    // Visual settings
    public Color lineColor = Color.white;
    public float lineWidth = 5f;

    private Image lineImage;
    private RectTransform rectTransform;

    // Cached world positions and colour — UpdateLine only runs when these change.
    private Vector3 cachedStartWorld;
    private Vector3 cachedEndWorld;
    private Color cachedColor;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        lineImage = GetComponent<Image>();

        if (lineImage == null)
            lineImage = gameObject.AddComponent<Image>();
    }

    private void Update()
    {
        if (startPoint == null || endPoint == null) return;

        bool moved      = startPoint.position != cachedStartWorld
                       || endPoint.position   != cachedEndWorld;
        bool colorDirty = lineColor != cachedColor;

        if (moved || colorDirty)
            UpdateLine();
    }

    /// Assigns start and end points and immediately recalculates the line.
    public void SetPoints(RectTransform start, RectTransform end)
    {
        startPoint = start;
        endPoint   = end;
        UpdateLine();
    }

    /// Updates line colour at runtime.
    public void SetColor(Color color)
    {
        lineColor = color;
        if (lineImage != null)
            lineImage.color = color;
    }

    private void UpdateLine()
    {
        if (startPoint == null || endPoint == null || rectTransform == null) return;

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 startLocalPos;
        Vector2 endLocalPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(null, startPoint.position),
            null,
            out startLocalPos);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            RectTransformUtility.WorldToScreenPoint(null, endPoint.position),
            null,
            out endLocalPos);

        Vector2 direction = endLocalPos - startLocalPos;
        float   distance  = direction.magnitude;

        rectTransform.anchoredPosition = startLocalPos + direction * 0.5f;
        rectTransform.sizeDelta        = new Vector2(distance, lineWidth);
        rectTransform.rotation         = Quaternion.Euler(0f, 0f,
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        if (lineImage != null)
            lineImage.color = lineColor;

        cachedStartWorld = startPoint.position;
        cachedEndWorld   = endPoint.position;
        cachedColor      = lineColor;
    }
}