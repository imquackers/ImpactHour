using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class WireMatchingPuzzle : PuzzleBase
{
    [Header("Wire Puzzle Settings")]
    public int numberOfWires = 4;
    public List<Color> availableColors = new List<Color>();

    [Header("UI Elements")]
    public Transform leftConnectorParent;
    public Transform rightConnectorParent;
    public GameObject connectorPrefab;
    public GameObject uiLinePrefab;

    private List<WireConnector> leftConnectors = new List<WireConnector>();
    private List<WireConnector> rightConnectors = new List<WireConnector>();
    private WireConnector selectedConnector;
    private UILineRenderer currentDragLine;
    private List<UIWireConnection> connections = new List<UIWireConnection>();

    private void Start()
    {
        if (availableColors.Count == 0)
        {
            availableColors = new List<Color>
            {
                Color.red, Color.blue, Color.yellow, Color.green,
                Color.cyan, Color.magenta, new Color(1f, 0.5f, 0f), Color.white
            };
        }

        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }
    }

    protected override void OpenPuzzle()
    {
        if (connectorPrefab == null || uiLinePrefab == null || puzzleUI == null)
        {
            Debug.Log($"{puzzleName}: Auto-solving (UI not set up yet)");
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

    private void SetupPuzzle()
    {
        ClearPuzzle();

        List<Color> selectedColors = availableColors.OrderBy(x => Random.value).Take(numberOfWires).ToList();
        List<Color> shuffledColors = selectedColors.OrderBy(x => Random.value).ToList();

        for (int i = 0; i < numberOfWires; i++)
        {
            GameObject leftObj = Instantiate(connectorPrefab, leftConnectorParent);
            WireConnector leftConn = leftObj.GetComponent<WireConnector>();
            if (leftConn == null) leftConn = leftObj.AddComponent<WireConnector>();
            leftConn.Initialize(selectedColors[i], true, this);
            leftConnectors.Add(leftConn);

            GameObject rightObj = Instantiate(connectorPrefab, rightConnectorParent);
            WireConnector rightConn = rightObj.GetComponent<WireConnector>();
            if (rightConn == null) rightConn = rightObj.AddComponent<WireConnector>();
            rightConn.Initialize(shuffledColors[i], false, this);
            rightConnectors.Add(rightConn);
        }
    }

    private void ClearPuzzle()
    {
        foreach (var conn in leftConnectors)
        {
            if (conn != null) Destroy(conn.gameObject);
        }
        foreach (var conn in rightConnectors)
        {
            if (conn != null) Destroy(conn.gameObject);
        }
        foreach (var connection in connections)
        {
            if (connection.line != null) Destroy(connection.line.gameObject);
        }

        leftConnectors.Clear();
        rightConnectors.Clear();
        connections.Clear();
        
        if (leftConnectorParent != null)
        {
            foreach (Transform child in leftConnectorParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        if (rightConnectorParent != null)
        {
            foreach (Transform child in rightConnectorParent)
            {
                Destroy(child.gameObject);
            }
        }
        
        if (puzzleUI != null)
        {
            UILineRenderer[] lines = puzzleUI.GetComponentsInChildren<UILineRenderer>();
            foreach (UILineRenderer line in lines)
            {
                Destroy(line.gameObject);
            }
        }
    }

    public void OnConnectorClicked(WireConnector connector)
    {
        Debug.Log($"OnConnectorClicked called. Connector: {connector.connectorColor}, IsLeft: {connector.isLeftSide}");
        
        if (selectedConnector == null)
        {
            Debug.Log("First selection - selecting connector");
            selectedConnector = connector;
            connector.SetSelected(true);
        }
        else
        {
            Debug.Log($"Second click. Selected: {selectedConnector.connectorColor} (Left:{selectedConnector.isLeftSide}), New: {connector.connectorColor} (Left:{connector.isLeftSide})");
            
            if (selectedConnector.isLeftSide != connector.isLeftSide)
            {
                Debug.Log("Creating connection!");
                CreateConnection(selectedConnector, connector);
                selectedConnector.SetSelected(false);
                selectedConnector = null;
            }
            else
            {
                Debug.Log("Both connectors on same side - switching selection");
                selectedConnector.SetSelected(false);
                selectedConnector = connector;
                connector.SetSelected(true);
            }
        }
    }

    public void OnDragStart(WireConnector connector, PointerEventData eventData)
    {
        selectedConnector = connector;
        connector.SetSelected(true);
        
        if (currentDragLine != null)
        {
            Destroy(currentDragLine.gameObject);
        }
        
        GameObject lineObj = Instantiate(uiLinePrefab, puzzleUI.transform);
        currentDragLine = lineObj.GetComponent<UILineRenderer>();
        if (currentDragLine == null)
        {
            currentDragLine = lineObj.AddComponent<UILineRenderer>();
        }
        
        currentDragLine.startPoint = connector.GetComponent<RectTransform>();
        currentDragLine.SetColor(connector.connectorColor);
        currentDragLine.lineWidth = 8f;
        
        GameObject tempEndPoint = new GameObject("TempEndPoint");
        tempEndPoint.transform.SetParent(puzzleUI.transform, false);
        RectTransform tempRect = tempEndPoint.AddComponent<RectTransform>();
        tempRect.anchoredPosition = connector.GetComponent<RectTransform>().anchoredPosition;
        currentDragLine.endPoint = tempRect;
        
        Debug.Log("Drag started, UI line created");
    }

    public void OnDragging(PointerEventData eventData)
    {
        if (currentDragLine != null && currentDragLine.endPoint != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                puzzleUI.GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);
            
            currentDragLine.endPoint.anchoredPosition = localPoint;
        }
    }

    public void OnDragEnd(WireConnector fromConnector, PointerEventData eventData)
    {
        WireConnector targetConnector = null;
        
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
        
        if (targetConnector != null)
        {
            Debug.Log($"Dropped on: {targetConnector.connectorColor}");
            CreateConnection(fromConnector, targetConnector);
        }
        else
        {
            Debug.Log("Dropped on nothing");
        }
        
        if (currentDragLine != null)
        {
            if (currentDragLine.endPoint != null && currentDragLine.endPoint.gameObject.name == "TempEndPoint")
            {
                Destroy(currentDragLine.endPoint.gameObject);
            }
            Destroy(currentDragLine.gameObject);
            currentDragLine = null;
        }
        
        if (selectedConnector != null)
        {
            selectedConnector.SetSelected(false);
            selectedConnector = null;
        }
    }

    private void CreateConnection(WireConnector from, WireConnector to)
    {
        if (isSolved) return;
        
        Debug.Log($"CreateConnection: {from.connectorColor} to {to.connectorColor}");
        RemoveConnectionsFrom(from);
        RemoveConnectionsFrom(to);

        GameObject lineObj = Instantiate(uiLinePrefab, puzzleUI.transform);
        UILineRenderer line = lineObj.GetComponent<UILineRenderer>();
        if (line == null)
        {
            line = lineObj.AddComponent<UILineRenderer>();
        }
        
        line.SetPoints(from.GetComponent<RectTransform>(), to.GetComponent<RectTransform>());
        line.SetColor(from.connectorColor);
        line.lineWidth = 8f;

        UIWireConnection connection = new UIWireConnection
        {
            leftConnector = from.isLeftSide ? from : to,
            rightConnector = from.isLeftSide ? to : from,
            line = line
        };

        connections.Add(connection);
        
        if (connection.leftConnector.connectorColor != connection.rightConnector.connectorColor)
        {
            Debug.Log("Wrong connection! Colors don't match.");
            if (PuzzlePenaltyManager.Instance != null)
            {
                PuzzlePenaltyManager.Instance.TriggerPenalty();
            }
        }
        
        CheckPuzzleCompletion();
        
        Debug.Log($"Connection created. Total connections: {connections.Count}");
    }

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
        if (puzzleUI != null && puzzleUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClosePuzzleButton();
            }
        }
    }

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

public class UIWireConnection
{
    public WireConnector leftConnector;
    public WireConnector rightConnector;
    public UILineRenderer line;
}
