using UnityEngine;
using System.Collections.Generic;

// WirePuzzle
// A simple matching puzzle where the player must connect wires from the left side to the correct corresponding wires on the right side
// Left wires are fixed (0 / n)
// Right wires are shuffled
// A dictionary stores the correct matches
// Player selections are stored separately
// Completion is checked by comparing both dictionaries

public class WirePuzzle : PuzzleBase
{
    [Header("Wire Puzzle Settings")]

    // Total number of wires in the puzzle
    public int numberOfWires = 4;

    // Visual colors for wires (index corresponds to wire ID)
    public Color[] wireColors =
    {
        Color.red,
        Color.blue,
        Color.yellow,
        new Color(1f, 0f, 1f) // Magenta
    };

  

    // Left-side wire indices (0 to numberOfWires - 1)
    private List<int> leftConnections = new List<int>();

    // Right-side wire indices (shuffled)
    private List<int> rightConnections = new List<int>();

    // Stores the correct mapping:
    // key = left index
    // value = correct right index
    private Dictionary<int, int> correctMatches =
        new Dictionary<int, int>();

    // Stores the player's current selections
    // key = left index
    // value = chosen right index
    private Dictionary<int, int> currentMatches =
        new Dictionary<int, int>();


    // Initialisation
 

    private void Awake()
    {
        // Generate puzzle immediately when object loads.
        GeneratePuzzle();
    }

    // Creates a new randomized wire mapping
    private void GeneratePuzzle()
    {
        // Reset all data
        leftConnections.Clear();
        rightConnections.Clear();
        correctMatches.Clear();
        currentMatches.Clear();

        // fill left and right lists with base indices
        for (int i = 0; i < numberOfWires; i++)
        {
            leftConnections.Add(i);
            rightConnections.Add(i);
        }

        // Shuffle right side to randomize solution
        ShuffleList(rightConnections);

        // Pair left to shuffled right
        for (int i = 0; i < numberOfWires; i++)
        {
            correctMatches[leftConnections[i]] = rightConnections[i];
        }

   
    }


    // puzzle opened
 

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();

        Debug.Log($"Opening {puzzleName}. Match the wires by color!");
    }


    // player interaction


    // Called when player connects a left wire to a right wire.
    // leftIndex = selected left wire
    // rightIndex = selected right wire
    public void ConnectWire(int leftIndex, int rightIndex)
    {
        // Overwrites previous selection if one exists.
        currentMatches[leftIndex] = rightIndex;

        // Immediately check if puzzle is solved.
        CheckCompletion();
    }

 
    // completion check


    private void CheckCompletion()
    {
        // If player hasn't connected all wires yet, stop.
        if (currentMatches.Count != correctMatches.Count)
            return;

        // Compare every correct mapping
        foreach (var match in correctMatches)
        {
            // If missing key OR wrong value then not solved
            if (!currentMatches.ContainsKey(match.Key) ||
                currentMatches[match.Key] != match.Value)
            {
                return;
            }
        }

        // If all mappings match then puzzle solved
        CompletePuzzle();
    }

    // manual submission (may add later)
    public void SubmitPuzzle()
    {
        CheckCompletion();
    }

 
    // utility


    // Fisher–Yates algorithm shuffle
    // Randomizes list in-place
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];

            // Random index from current position onward
            int randomIndex = Random.Range(i, list.Count);

            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}