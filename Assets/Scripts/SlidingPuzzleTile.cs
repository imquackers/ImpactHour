using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
//
//NOT USED ANYMORE
//
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class SlidingPuzzleTile : MonoBehaviour, IPointerClickHandler
{
    public Vector2Int gridPosition;
    public int correctIndex;
    
    private SlidingPuzzle puzzle;
    private Image tileImage;
    private RectTransform rectTransform;
    private Button button;
    private bool isAnimating = false;
    
    // Removed LateUpdate bc it was forcing tiles back to gridPosition
    
    public void Initialize(SlidingPuzzle puzzleRef, Vector2Int position, int index, Sprite sprite)
    {
        puzzle = puzzleRef;
        gridPosition = position;
        correctIndex = index;
        
        tileImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        
        tileImage.sprite = sprite;
        button.onClick.AddListener(OnClick);
    }
    
    public void MoveTo(Vector2Int newPosition, bool animate)
    {
        gridPosition = newPosition;
        
        Debug.Log($"[MoveTo] Moving to grid ({newPosition.x}, {newPosition.y}), animate={animate}");
        
        if (puzzle == null)
        {
            Debug.LogError("Puzzle reference is null!");
            return;
        }
        
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
        {
            Debug.LogError("RectTransform is null!");
            return;
        }
        
        // Calculate target position
        float totalSize = puzzle.tileSize + puzzle.tileSpacing;
        float startX = -(puzzle.gridSize - 1) * totalSize / 2f;
        float startY = (puzzle.gridSize - 1) * totalSize / 2f;
        
        Vector3 targetPos = new Vector3(
            startX + newPosition.x * totalSize,
            startY - newPosition.y * totalSize,
            0f
        );
        
        Debug.Log($"[MoveTo] Current localPosition: {transform.localPosition}, Target: {targetPos}");
        
        // Use localPosition instead of anchoredPosition
        transform.localPosition = targetPos;
        
        Debug.Log($"[MoveTo] Position set! New localPosition: {transform.localPosition}");
        
        // Disable the GameObject and re-enable it to force a refresh
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
    
    private IEnumerator AnimateToPosition(Vector2 targetPos)
    {
        isAnimating = true;
        RectTransform rect = GetComponent<RectTransform>();
        Vector2 startPos = rect.anchoredPosition;
        
        Debug.Log($"Animation starting: {startPos} -> {targetPos}");
        
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            
            // Force canvas to update
            Canvas.ForceUpdateCanvases();
            
            yield return null;
        }
        
        rect.anchoredPosition = targetPos;
        
        // Force final update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        
        isAnimating = false;
        
        Debug.Log($"Animation complete: final position {rect.anchoredPosition}");
    }
    
    private void OnClick()
    {
        if (puzzle != null)
        {
            puzzle.OnTileClicked(this);
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        OnClick();
    }
}
