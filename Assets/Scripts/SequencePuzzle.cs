using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SequencePuzzle : PuzzleBase
{
    // settings
    [Header("Sequence Puzzle Settings")]
    public int gridSize = 4;        // Grid size (4x4 = 16 tiles)
    public Sprite puzzleImage;      // Image that gets split into tiles

    // UI Refs
    [Header("UI References")]
    public Transform gridContainer; // Parent object for tiles
    public GameObject tilePrefab;   // Tile prefab
    public Text instructionText;    // Instruction text

    // Internal Variables
    private List<SequenceTile> tiles;
    private int currentSequenceIndex = 0; // What number player should click next
    private int totalTiles;

    // Called when puzzle opens
    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        GeneratePuzzle();
    }

    // Creates the puzzle grid
    private void GeneratePuzzle()
    {
        ClearGrid();

        // Safety check
        if (puzzleImage == null || tilePrefab == null)
        {
            Debug.LogError("Missing image or prefab!");
            return;
        }

        totalTiles = gridSize * gridSize;
        tiles = new List<SequenceTile>();

        // Get texture from sprite
        Texture2D texture = puzzleImage.texture;

        // Calculate tile size
        int tileWidth = texture.width / gridSize;
        int tileHeight = texture.height / gridSize;

        // Create numbers (1 to totalTiles)
        List<int> numbers = new List<int>();
        for (int i = 1; i <= totalTiles; i++)
        {
            numbers.Add(i);
        }

        // Shuffle numbers randomly
        for (int i = 0; i < numbers.Count; i++)
        {
            int temp = numbers[i];
            int randomIndex = Random.Range(i, numbers.Count);
            numbers[i] = numbers[randomIndex];
            numbers[randomIndex] = temp;
        }

        // Create tiles
        int tileIndex = 0;

        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Spawn tile
                GameObject tileObj = Instantiate(tilePrefab, gridContainer);

                // Get or add SequenceTile script
                SequenceTile tile = tileObj.GetComponent<SequenceTile>();
                if (tile == null)
                {
                    tile = tileObj.AddComponent<SequenceTile>();
                }

                // Create sprite piece from image
                Sprite tileSprite = CreateTileSprite(texture, x, y, tileWidth, tileHeight);

                // Assign shuffled number
                int sequenceNumber = numbers[tileIndex];

                // Initialize tile
                tile.Initialize(this, sequenceNumber, tileSprite);
                tiles.Add(tile);

                tileIndex++;
            }
        }

        // Start from number 1
        currentSequenceIndex = 1;
        UpdateInstruction();
    }

    // Cuts a piece of the image for each tile
    private Sprite CreateTileSprite(Texture2D texture, int x, int y, int tileWidth, int tileHeight)
    {
        int pixelX = x * tileWidth;

        // Flip Y so image appears correct
        int pixelY = (gridSize - 1 - y) * tileHeight;

        Rect rect = new Rect(pixelX, pixelY, tileWidth, tileHeight);

        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
    }

    // Called when player clicks a tile
    public void OnTileClicked(SequenceTile tile)
    {
        if (isSolved) return;

        // Correct tile clicked
        if (tile.sequenceNumber == currentSequenceIndex)
        {
            tile.MarkAsClicked();
            currentSequenceIndex++;

            // If all tiles clicked then win
            if (currentSequenceIndex > totalTiles)
            {
                CompletePuzzle();
            }
            else
            {
                UpdateInstruction();
            }
        }
        else
        {
            // Wrong tile then penalty
            TriggerPenalty();
        }
    }

    // Updates instruction text
    private void UpdateInstruction()
    {
        if (instructionText != null)
        {
            instructionText.text = $"Click in order: {currentSequenceIndex}/{totalTiles}";
        }
    }

    // Penalty
    private void TriggerPenalty()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyTimePenalty(0.10f); 
        }
    }

    // Clears all tiles
    private void ClearGrid()
    {
        if (gridContainer == null) return;

        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        tiles = new List<SequenceTile>();
    }
}