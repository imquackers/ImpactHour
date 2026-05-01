using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// WireConnector
// a single clickable / draggable wire endpoint
// - Displays the connector colour
// - Handles click input
// - Handles drag input (left side only)
// - Notifies the parent WireMatchingPuzzle of interactions
// It only forwards user input to the puzzle controller.

public class WireConnector : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    // The visual colour of this connector
    public Color connectorColor;

    // True if this connector is on the left side
    // Only left connectors can initiate dragging.
    public bool isLeftSide;

    // Reference to parent puzzle controller
    private WireMatchingPuzzle puzzle;

    // UI image component (controls connector appearance)
    private Image image;

    // Outline component used for selection highlight
    private Outline outline;

  
    // Initialisation


    // Called externally after instantiating the connector.
    // Sets up colour, side, and parent puzzle reference.
    public void Initialize(Color color, bool left, WireMatchingPuzzle parentPuzzle)
    {
        connectorColor = color;
        isLeftSide = left;
        puzzle = parentPuzzle;

        // Ensure Image component exists
        image = GetComponent<Image>();
        if (image == null)
            image = gameObject.AddComponent<Image>();

        image.color = color;

        // Ensure Outline component exists (used for selection highlight)
        outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(3, 3);
        outline.enabled = false; // Disabled by default
    }


    // Click Handling


    // Called when this UI element is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Connector clicked! Color: {connectorColor}, IsLeft: {isLeftSide}");

        if (puzzle != null)
        {
            // Inform puzzle controller
            puzzle.OnConnectorClicked(this);
        }
        else
        {
            Debug.LogError("Puzzle reference is null!");
        }
    }


    // Drag Handling

    // Called when user starts dragging
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only left-side connectors can start a wire drag
        if (!isLeftSide) return;

        Debug.Log($"Begin drag: {connectorColor}");

        if (puzzle != null)
        {
            puzzle.OnDragStart(this, eventData);
        }
    }

    // Called continuously while dragging
    public void OnDrag(PointerEventData eventData)
    {
        if (!isLeftSide) return;

        if (puzzle != null)
        {
            puzzle.OnDragging(eventData);
        }
    }

    // Called when drag ends (mouse released)
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isLeftSide) return;

        Debug.Log($"End drag: {connectorColor}");

        if (puzzle != null)
        {
            puzzle.OnDragEnd(this, eventData);
        }
    }


    // Visual Feedback


    // Enables or disables the white outline highlight.
    // Used to indicate selection state.
    public void SetSelected(bool selected)
    {
        if (outline != null)
        {
            outline.enabled = selected;
        }
    }
}