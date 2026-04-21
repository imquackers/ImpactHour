using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class SequenceTile : MonoBehaviour
{
    public int sequenceNumber;
    
    private SequencePuzzle puzzle;
    private Image tileImage;
    private Button button;
    private Text numberText;
    private bool isClicked = false;

    public void Initialize(SequencePuzzle puzzleRef, int number, Sprite sprite)
    {
        puzzle = puzzleRef;
        sequenceNumber = number;
        
        Debug.Log($"[SequenceTile] Initializing tile with number {number}");
        
        tileImage = GetComponent<Image>();
        button = GetComponent<Button>();
        
        if (tileImage == null)
        {
            Debug.LogError("Image component not found!");
            return;
        }
        
        if (button == null)
        {
            Debug.LogError("Button component not found!");
            return;
        }
        
        tileImage.sprite = sprite;
        button.onClick.AddListener(OnClick);
        
        Debug.Log($"[SequenceTile] Sprite set, sprite is null: {sprite == null}");
        
        // Create number text overlay
        GameObject textObj = new GameObject("NumberText");
        textObj.transform.SetParent(transform);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        numberText = textObj.AddComponent<Text>();
        numberText.text = number.ToString();
        numberText.fontSize = 36;
        numberText.fontStyle = FontStyle.Bold;
        numberText.alignment = TextAnchor.MiddleCenter;
        numberText.color = Color.white;
        
        // Add outline for visibility
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, 2);
        
        Debug.Log($"[SequenceTile] Number text created: {number}");
    }

    private void OnClick()
    {
        if (!isClicked && puzzle != null)
        {
            puzzle.OnTileClicked(this);
        }
    }

    public void MarkAsClicked()
    {
        isClicked = true;
        
        // Fade out the tile
        Color color = tileImage.color;
        color.a = 0.3f;
        tileImage.color = color;
        
        // Hide number
        if (numberText != null)
        {
            numberText.color = new Color(1, 1, 1, 0.3f);
        }
        
        button.interactable = false;
    }
}
