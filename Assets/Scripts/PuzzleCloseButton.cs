using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PuzzleCloseButton : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(OnCloseClicked);
    }

    private void OnCloseClicked()
    {
        PuzzleBase[] allPuzzles = FindObjectsByType<PuzzleBase>(FindObjectsSortMode.None);
        
        foreach (PuzzleBase puzzle in allPuzzles)
        {
            if (puzzle.puzzleUI != null && puzzle.puzzleUI.activeSelf)
            {
                puzzle.ClosePuzzleButton();
                Debug.Log($"Closing puzzle: {puzzle.puzzleName}");
                return;
            }
        }
        
        Debug.LogWarning("No active puzzle found to close!");
    }
}
