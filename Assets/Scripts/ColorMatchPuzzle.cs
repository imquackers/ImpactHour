using UnityEngine;
using System.Collections.Generic;

public class ColorMatchPuzzle : PuzzleBase
{
    [Header("Color Match Settings")]
    public int gridSize = 4;
    public Color[] availableColors = { Color.red, Color.blue, Color.green, Color.yellow };
    public float timeToMemorize = 3f;

    private Color[,] targetPattern;
    private Color[,] currentPattern;
    private bool showingPattern = false;

    private void Awake()
    {
        GeneratePattern();
    }

    private void GeneratePattern()
    {
        targetPattern = new Color[gridSize, gridSize];
        currentPattern = new Color[gridSize, gridSize];

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                targetPattern[x, y] = availableColors[Random.Range(0, availableColors.Length)];
                currentPattern[x, y] = Color.gray;
            }
        }
    }

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        Debug.Log($"Opening {puzzleName}. Memorize the pattern!");
        showingPattern = true;
        Invoke(nameof(HidePattern), timeToMemorize);
    }

    private void HidePattern()
    {
        showingPattern = false;
        Debug.Log("Now recreate the pattern!");
    }

    public void SetTileColor(int x, int y, Color color)
    {
        if (showingPattern) return;

        currentPattern[x, y] = color;
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (currentPattern[x, y] != targetPattern[x, y])
                {
                    return;
                }
            }
        }

        CompletePuzzle();
    }

    public void SubmitPattern()
    {
        CheckCompletion();
    }
}
