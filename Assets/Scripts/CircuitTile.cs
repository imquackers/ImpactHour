using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CircuitTile : MonoBehaviour, IPointerClickHandler
{
    public enum TileType
    {
        Empty,
        Straight,
        Corner,
        Source,
        Destination
    }

    public TileType tileType = TileType.Empty;
    public int rotation = 0;
    
    private CircuitBoardPuzzle puzzle;
    private Vector2Int gridPosition;
    private Image image;
    private bool isPowered = false;
    private GameObject indicatorContainer;

    public void Initialize(CircuitBoardPuzzle parentPuzzle, Vector2Int pos)
    {
        puzzle = parentPuzzle;
        gridPosition = pos;
        image = GetComponent<Image>();
        
        if (image == null)
        {
            image = gameObject.AddComponent<Image>();
        }
        
        CreateIndicators();
    }

    private void CreateIndicators()
    {
        indicatorContainer = new GameObject("Indicators");
        indicatorContainer.transform.SetParent(transform, false);
        
        RectTransform containerRect = indicatorContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
    }

    private void UpdateIndicators()
    {
        // Temporarily disable indicators to test if core logic works
        if (indicatorContainer != null)
        {
            foreach (Transform child in indicatorContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private List<Vector2> GetConnectionPoints()
    {
        List<Vector2> points = new List<Vector2>();
        RectTransform rectTransform = GetComponent<RectTransform>();
        float halfWidth = rectTransform.sizeDelta.x / 2f;
        float halfHeight = rectTransform.sizeDelta.y / 2f;
        float offset = 8f;

        if (tileType == TileType.Straight)
        {
            points.Add(new Vector2(-halfWidth + offset, 0));
            points.Add(new Vector2(halfWidth - offset, 0));
        }
        else if (tileType == TileType.Corner)
        {
            points.Add(new Vector2(halfWidth - offset, 0));
            points.Add(new Vector2(0, halfHeight - offset));
        }

        return points;
    }

    public void SetTileType(TileType type, int rot)
    {
        tileType = type;
        rotation = rot;
        transform.rotation = Quaternion.Euler(0, 0, rotation * 90f);
        UpdateVisuals();
        UpdateIndicators();
    }

    public bool CanRotate()
    {
        return tileType == TileType.Straight || tileType == TileType.Corner;
    }

    public void RotateInstant()
    {
        rotation = (rotation + 1) % 4;
    }

    public IEnumerator RotateAnimated(float speed)
    {
        int targetRotation = (rotation + 1) % 4;
        float startAngle = rotation * 90f;
        float targetAngle = targetRotation * 90f;
        float elapsed = 0f;
        float duration = 90f / speed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, elapsed / duration);
            transform.rotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }

        rotation = targetRotation;
        transform.rotation = Quaternion.Euler(0, 0, rotation * 90f);
        UpdateVisuals();
        UpdateIndicators();
    }

    public void SetPowered(bool powered)
    {
        isPowered = powered;
        UpdateVisuals();
        UpdateIndicators();
    }

    public bool IsPowered()
    {
        return isPowered;
    }

    private void UpdateVisuals()
    {
        if (image == null) return;

        Color baseColor = Color.gray;
        Color poweredColor = new Color(0.2f, 1f, 0.3f);

        switch (tileType)
        {
            case TileType.Empty:
                image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                break;
            case TileType.Straight:
            case TileType.Corner:
                image.color = isPowered ? poweredColor : baseColor;
                break;
            case TileType.Source:
                image.color = new Color(1f, 1f, 0.2f, 1f);
                break;
            case TileType.Destination:
                image.color = isPowered ? new Color(0.2f, 1f, 0.3f, 1f) : new Color(1f, 0.3f, 0.2f, 1f);
                break;
        }
    }

    public List<Vector2Int> GetConnections(Vector2Int pos)
    {
        List<Vector2Int> connections = new List<Vector2Int>();
        
        // Get which edges have dots based on visual rotation
        List<int> dotEdges = GetDotEdges(); // 0=right, 1=up, 2=left, 3=down
        
        foreach (int edge in dotEdges)
        {
            switch (edge)
            {
                case 0: // Right
                    connections.Add(new Vector2Int(pos.x + 1, pos.y));
                    break;
                case 1: // Up
                    connections.Add(new Vector2Int(pos.x, pos.y + 1));
                    break;
                case 2: // Left
                    connections.Add(new Vector2Int(pos.x - 1, pos.y));
                    break;
                case 3: // Down
                    connections.Add(new Vector2Int(pos.x, pos.y - 1));
                    break;
            }
        }

        return connections;
    }
    
    private List<int> GetDotEdges()
    {
        // Returns which edges have dots: 0=right, 1=up, 2=left, 3=down
        List<int> edges = new List<int>();
        
        if (tileType == TileType.Source)
        {
            edges.Add(3); // Down (source at top goes down)
            return edges;
        }
        
        if (tileType == TileType.Destination)
        {
            // Use rotation to determine connection direction
            // 0=right, 1=up, 2=left, 3=down
            edges.Add(rotation);
            return edges;
        }
        
        if (tileType == TileType.Straight)
        {
            if (rotation % 2 == 0)
            {
                edges.Add(2); // Left
                edges.Add(0); // Right
            }
            else
            {
                edges.Add(1); // Up
                edges.Add(3); // Down
            }
            return edges;
        }
        
        if (tileType == TileType.Corner)
        {
            // Base corner: Right(0) and Up(1)
            // Transform rotates by +rotation*90° 
            
            int edge1 = (0 + rotation + 3) % 4; // Right + rotation - 1
            int edge2 = (1 + rotation + 3) % 4; // Up + rotation - 1
            edges.Add(edge1);
            edges.Add(edge2);
            return edges;
        }
        
        return edges;
    }

    public bool ConnectsTo(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> connections = GetConnections(from);
        return connections.Contains(to);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (puzzle != null)
        {
            puzzle.OnTileClicked(this);
        }
    }
}
