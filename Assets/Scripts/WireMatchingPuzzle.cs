using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class WireMatchingPuzzle : PuzzleBase
{
    // settings
    [Header("Wire Puzzle Settings")]
    public int numberOfWires = 4; // How many wires to match
    public List<Color> availableColors = new List<Color>(); // Possible colors

    // UI Refs
    [Header("UI Elements")]
    public Transform leftConnectorParent;  // Left side container
    public Transform rightConnectorParent; // Right side container
    public GameObject connectorPrefab;     // Button/connector prefab
    public GameObject uiLinePrefab;        // Line renderer prefab

    // Internal variables
    private List<WireConnector> leftConnectors = new List<WireConnector>();
    private List<WireConnector> rightConnectors = new List<WireConnector>();
    private WireConnector selectedConnector; // Currently selected connector
    private UILineRenderer currentDragLine;  // Line while dragging
    private List<UIWireConnection> connections = new List<UIWireConnection>();

    private void Start()
    {
        // If no colors set, create default list
        if (availableColors.Count == 0)
        {
            availableColors = new List<Color>
            {
                Color.red, Color.blue, Color.yellow, Color.green,
                Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f), Color.white
            };
        }

        // Hide puzzle UI at start
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }
    }

    // Called when puzzle opens
    protected override void OpenPuzzle()
    {
        // If setup missing then auto-complete (prevents errors)
        if (connectorPrefab == null || uiLinePrefab == null || puzzleUI == null)
        {
            Debug.Log($"{puzzleName}: Auto-solving");
            Invoke(nameof(AutoSolve), 1f);
            return;
        }

        base.OpenPuzzle();
        SetupPuzzle();
    }

    private void AutoSolve()
    {
        CompletePuzzle();
    }

    // Create connectors and assign colors
    private void SetupPuzzle()
    {
        ClearPuzzle();

        // Pick random colors
        List<Color> selectedColors = availableColors.OrderBy(x => Random.value).Take(numberOfWires).ToList();

        // Shuffle right side
        List<Color> shuffledColors = selectedColors.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < numberOfWires; i++)
        {
            // LEFT SIDE
            GameObject leftObj = Instantiate(connectorPrefab, leftConnectorParent);
            WireConnector leftConn = leftObj.GetComponent<WireConnector>();
            if (leftConn == null) leftConn = leftObj.AddComponent<WireConnector>();

            leftConn.Initialize(selectedColors[i], true, this);
            leftConnectors.Add(leftConn);

            // RIGHT SIDE
            GameObject rightObj = Instantiate(connectorPrefab, rightConnectorParent);
            WireConnector rightConn = rightObj.GetComponent<WireConnector>();
            if (rightConn == null) rightConn = rightObj.AddComponent<WireConnector>();

            rightConn.Initialize(shuffledColors[i], false, this);
            rightConnectors.Add(rightConn);
        }
    }

    // Clears all connectors and lines
    private void ClearPuzzle()
    {
        foreach (var conn in leftConnectors)
            if (conn != null) Destroy(conn.gameObject);

        foreach (var conn in rightConnectors)
            if (conn != null) Destroy(conn.gameObject);

        foreach (var connection in connections)
            if (connection.line != null) Destroy(connection.line.gameObject);

        leftConnectors.Clear();
        rightConnectors.Clear();
        connections.Clear();
    }

    // click based connection
    public void OnConnectorClicked(WireConnector connector)
    {
        if (selectedConnector == null)
        {
            // First click = select
            selectedConnector = connector;
            connector.SetSelected(true);
        }
        else
        {
            // Second click = attempt connection
            if (selectedConnector.isLeftSide != connector.isLeftSide)
            {
                CreateConnection(selectedConnector, connector);

                selectedConnector.SetSelected(false);
                selectedConnector = null;
            }
            else
            {
                // Same side = switch selection
                selectedConnector.SetSelected(false);
                selectedConnector = connector;
                connector.SetSelected(true);
            }
        }
    }

    // drag start
    public void OnDragStart(WireConnector connector, PointerEventData eventData)
    {
        selectedConnector = connector;
        connector.SetSelected(true);

        // Create temporary drag line
        GameObject lineObj = Instantiate(uiLinePrefab, puzzleUI.transform);
        currentDragLine = lineObj.GetComponent<UILineRenderer>();

        currentDragLine.startPoint = connector.GetComponent<RectTransform>();
        currentDragLine.SetColor(connector.connectorColor);

        // Create temporary endpoint that follows mouse
        GameObject tempEnd = new GameObject("TempEndPoint");
        tempEnd.transform.SetParent(puzzleUI.transform, false);

        RectTransform tempRect = tempEnd.AddComponent<RectTransform>();
        currentDragLine.endPoint = tempRect;
    }

    // dragging
    public void OnDragging(PointerEventData eventData)
    {
        if (currentDragLine != null)
        {
            Vector2 localPoint;

            // Convert mouse position to UI position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                puzzleUI.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);

            currentDragLine.endPoint.anchoredPosition = localPoint;
        }
    }

    // drag end
    public void OnDragEnd(WireConnector fromConnector, PointerEventData eventData)
    {
        WireConnector targetConnector = null;

        // Check if dropped on a right connector
        foreach (var connector in rightConnectors)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                connector.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera))
            {
                targetConnector = connector;
                break;
            }
        }

        // If valid target then connect
        if (targetConnector != null)
        {
            CreateConnection(fromConnector, targetConnector);
        }

        // Clean up drag line
        if (currentDragLine != null)
        {
            Destroy(currentDragLine.gameObject);
            currentDragLine = null;
        }

        if (selectedConnector != null)
        {
            selectedConnector.SetSelected(false);
            selectedConnector = null;
        }
    }

    // Create a permanent connection
    private void CreateConnection(WireConnector from, WireConnector to)
    {
        if (isSolved) return;

        // Remove existing connections
        RemoveConnectionsFrom(from);
        RemoveConnectionsFrom(to);

        // Create line
        GameObject lineObj = Instantiate(uiLinePrefab, puzzleUI.transform);
        UILineRenderer line = lineObj.GetComponent<UILineRenderer>();

        line.SetPoints(from.GetComponent<RectTransform>(), to.GetComponent<RectTransform>());
        line.SetColor(from.connectorColor);

        // Save connection
        UIWireConnection connection = new UIWireConnection
        {
            leftConnector = from.isLeftSide ? from : to,
            rightConnector = from.isLeftSide ? to : from,
            line = line
        };

        connections.Add(connection);

        // Check if correct
        if (connection.leftConnector.connectorColor != connection.rightConnector.connectorColor)
        {
            if (PuzzlePenaltyManager.Instance != null)
                PuzzlePenaltyManager.Instance.TriggerPenalty();
        }

        CheckPuzzleCompletion();
    }

    // Remove old connections from a connector
    private void RemoveConnectionsFrom(WireConnector connector)
    {
        var toRemove = connections.Where(c => c.leftConnector == connector || c.rightConnector == connector).ToList();

        foreach (var conn in toRemove)
        {
            if (conn.line != null) Destroy(conn.line.gameObject);
            connections.Remove(conn);
        }
    }

    private void Update()
    {
        // Press ESC to close puzzle
        if (puzzleUI != null && puzzleUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePuzzleButton();
            }
        }
    }

    // Check if puzzle is complete
    private void CheckPuzzleCompletion()
    {
        if (connections.Count != numberOfWires) return;

        bool allCorrect = true;

        foreach (var connection in connections)
        {
            if (connection.leftConnector.connectorColor != connection.rightConnector.connectorColor)
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect)
        {
            CompletePuzzle();
        }
    }

    protected override void ClosePuzzle()
    {
        base.ClosePuzzle();

        if (!isSolved)
        {
            ClearPuzzle();
        }
    }
}

// Stores a connection between two connectors
public class UIWireConnection
{
    public WireConnector leftConnector;
    public WireConnector rightConnector;
    public UILineRenderer line;
}