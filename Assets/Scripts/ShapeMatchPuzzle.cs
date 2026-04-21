using UnityEngine;
using System.Collections.Generic;

public class ShapeMatchPuzzle : PuzzleBase
{
    [Header("Shape Match Settings")]
    public int numberOfShapes = 5;
    public string[] shapeTypes = { "Circle", "Square", "Triangle", "Star", "Diamond" };

    private List<string> targetSequence = new List<string>();
    private List<string> currentSequence = new List<string>();

    private void Awake()
    {
        GenerateSequence();
    }

    private void GenerateSequence()
    {
        targetSequence.Clear();
        for (int i = 0; i < numberOfShapes; i++)
        {
            targetSequence.Add(shapeTypes[Random.Range(0, shapeTypes.Length)]);
        }
    }

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        Debug.Log($"Opening {puzzleName}. Match the shapes in order!");
        currentSequence.Clear();
    }

    public void AddShape(string shape)
    {
        currentSequence.Add(shape);
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (currentSequence.Count != targetSequence.Count) return;

        for (int i = 0; i < targetSequence.Count; i++)
        {
            if (currentSequence[i] != targetSequence[i])
            {
                currentSequence.Clear();
                Debug.Log("Wrong sequence! Try again.");
                return;
            }
        }

        CompletePuzzle();
    }

    public void ResetSequence()
    {
        currentSequence.Clear();
        Debug.Log("Sequence reset. Try again.");
    }
}
