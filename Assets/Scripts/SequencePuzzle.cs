using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SequencePuzzle : PuzzleBase
{
    [Header("Sequence Puzzle Settings")]
    public int gridSize = 4;
    public Sprite puzzleImage;
    
    [Header("UI References")]
    public Transform gridContainer;
    public GameObject tilePrefab;
    public Text instructionText;
    
    private List<SequenceTile> tiles;
    private int currentSequenceIndex = 0;
    private int totalTiles;

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        GeneratePuzzle();
    }

    private void GeneratePuzzle()
    {
        ClearGrid();
        
        if (puzzleImage == null || tilePrefab == null)
        {
            Debug.LogError("Puzzle image or tile prefab not assigned!");
            return;
        }
        
        totalTiles = gridSize * gridSize;
        tiles = new List<SequenceTile>();
        
        Texture2D texture = puzzleImage.texture;
        int tileWidth = texture.width / gridSize;
        int tileHeight = texture.height / gridSize;
        
        List<int> numbers = new List<int>();
        for (int i = 1; i <= totalTiles; i++)
        {
            numbers.Add(i);
        }
        
        // Shuffle numbers
        for (int i = 0; i < numbers.Count; i++)
        {
            int temp = numbers[i];
            int randomIndex = Random.Range(i, numbers.Count);
            numbers[i] = numbers[randomIndex];
            numbers[randomIndex] = temp;
        }
        
        int tileIndex = 0;
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                GameObject tileObj = Instantiate(tilePrefab, gridContainer);
                
                Debug.Log($"Created tile object at grid ({x},{y}), active: {tileObj.activeSelf}");
                
                SequenceTile tile = tileObj.GetComponent<SequenceTile>();
                
                if (tile == null)
                {
                    tile = tileObj.AddComponent<SequenceTile>();
                    Debug.Log($"Added SequenceTile component to tile at ({x},{y})");
                }
                
                Sprite tileSprite = CreateTileSprite(texture, x, y, tileWidth, tileHeight);
                int sequenceNumber = numbers[tileIndex];
                
                Debug.Log($"Initializing tile {tileIndex} at grid ({x},{y}) with number {sequenceNumber}");
                
                tile.Initialize(this, sequenceNumber, tileSprite);
                tiles.Add(tile);
                
                tileIndex++;
            }
        }
        
        currentSequenceIndex = 1;
        UpdateInstruction();
        
        Debug.Log($"Sequence puzzle generated with {totalTiles} tiles");
    }

    private Sprite CreateTileSprite(Texture2D texture, int x, int y, int tileWidth, int tileHeight)
    {
        int pixelX = x * tileWidth;
        int pixelY = (gridSize - 1 - y) * tileHeight;
        
        Rect rect = new Rect(pixelX, pixelY, tileWidth, tileHeight);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
    }

    public void OnTileClicked(SequenceTile tile)
    {
        if (isSolved) return;
        
        if (tile.sequenceNumber == currentSequenceIndex)
        {
            tile.MarkAsClicked();
            currentSequenceIndex++;
            
            Debug.Log($"Correct! Clicked {tile.sequenceNumber}. Next: {currentSequenceIndex}");
            
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
            Debug.Log($"Wrong! Clicked {tile.sequenceNumber}, expected {currentSequenceIndex}");
            TriggerPenalty();
        }
    }

    private void UpdateInstruction()
    {
        if (instructionText != null)
        {
            instructionText.text = $"Click the tiles in order: {currentSequenceIndex}/{totalTiles}";
        }
    }

    private void TriggerPenalty()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyTimePenalty(0.10f);
        }
    }

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
