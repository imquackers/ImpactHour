using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SlidingPuzzle : PuzzleBase
{
    [Header("Sliding Puzzle Settings")]
    public int gridSize = 4;
    public Sprite puzzleImage;
    public float tileSize = 145f;
    public float tileSpacing = 5f;
    
    [Header("UI References")]
    public Transform gridContainer;
    public GameObject tilePrefab;
    
    private SlidingPuzzleTile[,] tiles;
    private Vector2Int emptyPosition;
    private int totalTiles;
    
    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        GeneratePuzzle();
    }
    
    private void GeneratePuzzle()
    {
        ClearGrid();
        
        if (puzzleImage == null)
        {
            Debug.LogError("Puzzle image is not assigned!");
            return;
        }
        
        if (tilePrefab == null)
        {
            Debug.LogError("Tile prefab is not assigned!");
            return;
        }
        
        totalTiles = gridSize * gridSize;
        tiles = new SlidingPuzzleTile[gridSize, gridSize];
        
        Texture2D texture = puzzleImage.texture;
        
        if (texture == null)
        {
            Debug.LogError("Puzzle image texture is null! Make sure the texture is Read/Write enabled.");
            return;
        }
        
        Debug.Log($"Generating puzzle with texture size: {texture.width}x{texture.height}");
        
        int tileWidth = texture.width / gridSize;
        int tileHeight = texture.height / gridSize;
        
        int tileIndex = 0;
        
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (x == gridSize - 1 && y == gridSize - 1)
                {
                    emptyPosition = new Vector2Int(x, y);
                    tiles[x, y] = null;
                    Debug.Log($"Empty space at ({x}, {y})");
                    continue;
                }
                
                GameObject tileObj = Instantiate(tilePrefab, gridContainer);
                
                if (tileObj == null)
                {
                    Debug.LogError($"Failed to instantiate tile at ({x}, {y})");
                    continue;
                }
                
                // Set tile size
                RectTransform tileRect = tileObj.GetComponent<RectTransform>();
                if (tileRect != null)
                {
                    tileRect.sizeDelta = new Vector2(tileSize, tileSize);
                }
                
                // Add LayoutElement and set ignoreLayout to true
                LayoutElement layoutElement = tileObj.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = tileObj.AddComponent<LayoutElement>();
                }
                layoutElement.ignoreLayout = true;
                
                SlidingPuzzleTile tile = tileObj.GetComponent<SlidingPuzzleTile>();
                
                if (tile == null)
                {
                    tile = tileObj.AddComponent<SlidingPuzzleTile>();
                }
                
                Sprite tileSprite = CreateTileSprite(texture, x, y, tileWidth, tileHeight);
                tile.Initialize(this, new Vector2Int(x, y), tileIndex, tileSprite);
                
                // Position tile manually
                PositionTile(tile, x, y);
                
                tiles[x, y] = tile;
                tileIndex++;
                
                Debug.Log($"Created tile {tileIndex} at grid position ({x}, {y})");
            }
        }
        
        Debug.Log($"Total tiles created: {tileIndex}");
        ShufflePuzzle();
    }
    
    private Sprite CreateTileSprite(Texture2D texture, int x, int y, int tileWidth, int tileHeight)
    {
        int pixelX = x * tileWidth;
        int pixelY = (gridSize - 1 - y) * tileHeight;
        
        Rect rect = new Rect(pixelX, pixelY, tileWidth, tileHeight);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
    }
    
    private void ShufflePuzzle()
    {
        int shuffleMoves = gridSize * gridSize * 20;
        
        Debug.Log($"Starting shuffle with {shuffleMoves} moves");
        
        for (int i = 0; i < shuffleMoves; i++)
        {
            List<Vector2Int> validMoves = GetAdjacentTiles(emptyPosition);
            if (validMoves.Count > 0)
            {
                Vector2Int randomMove = validMoves[Random.Range(0, validMoves.Count)];
                SwapTiles(randomMove, emptyPosition, false);
            }
        }
        
        Debug.Log($"Shuffle complete. Empty space now at ({emptyPosition.x}, {emptyPosition.y})");
    }
    
    public void OnTileClicked(SlidingPuzzleTile tile)
    {
        Debug.Log($"Tile clicked at position ({tile.gridPosition.x}, {tile.gridPosition.y})");
        
        if (isSolved)
        {
            Debug.Log("Puzzle already solved, ignoring click");
            return;
        }
        
        Vector2Int tilePos = tile.gridPosition;
        
        if (IsAdjacentToEmpty(tilePos))
        {
            Debug.Log($"Tile is adjacent to empty space. Swapping...");
            SwapTiles(tilePos, emptyPosition, false); // FORCE NO ANIMATION
            
            if (CheckPuzzleComplete())
            {
                Debug.Log("Puzzle completed!");
                CompletePuzzle();
            }
        }
        else
        {
            Debug.Log($"Tile not adjacent to empty space at ({emptyPosition.x}, {emptyPosition.y})");
        }
    }
    
    private bool IsAdjacentToEmpty(Vector2Int pos)
    {
        int dx = Mathf.Abs(pos.x - emptyPosition.x);
        int dy = Mathf.Abs(pos.y - emptyPosition.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }
    
    private List<Vector2Int> GetAdjacentTiles(Vector2Int pos)
    {
        List<Vector2Int> adjacent = new List<Vector2Int>();
        
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = pos + dir;
            if (IsValidPosition(newPos) && tiles[newPos.x, newPos.y] != null)
            {
                adjacent.Add(newPos);
            }
        }
        
        return adjacent;
    }
    
    private void SwapTiles(Vector2Int pos1, Vector2Int pos2, bool animate)
    {
        SlidingPuzzleTile tile = tiles[pos1.x, pos1.y];
        
        tiles[pos1.x, pos1.y] = tiles[pos2.x, pos2.y];
        tiles[pos2.x, pos2.y] = tile;
        
        if (tile != null)
        {
            tile.MoveTo(pos2, animate);
            Debug.Log($"Swapped tile from ({pos1.x},{pos1.y}) to ({pos2.x},{pos2.y})");
        }
        
        emptyPosition = pos1;
    }
    
    private void PositionTile(SlidingPuzzleTile tile, int x, int y)
    {
        if (tile == null)
        {
            Debug.LogError("PositionTile: tile is null!");
            return;
        }
        
        float totalSize = tileSize + tileSpacing;
        float startX = -(gridSize - 1) * totalSize / 2f;
        float startY = (gridSize - 1) * totalSize / 2f;
        
        Vector3 position = new Vector3(
            startX + x * totalSize,
            startY - y * totalSize,
            0f
        );
        
        Debug.Log($"[PositionTile] Setting tile at grid ({x},{y}) to localPosition {position}");
        
        // Use localPosition
        tile.transform.localPosition = position;
        
        Debug.Log($"[PositionTile] AFTER setting, localPosition is now: {tile.transform.localPosition}");
    }
    
    public void AnimateTilePosition(SlidingPuzzleTile tile, Vector2Int targetGridPos)
    {
        Debug.Log($"Starting animation for tile to grid position ({targetGridPos.x}, {targetGridPos.y})");
        StartCoroutine(AnimateTileToPosition(tile, targetGridPos));
    }
    
    private System.Collections.IEnumerator AnimateTileToPosition(SlidingPuzzleTile tile, Vector2Int targetGridPos)
    {
        RectTransform rect = tile.GetComponent<RectTransform>();
        if (rect == null)
        {
            Debug.LogError("Tile has no RectTransform!");
            yield break;
        }
        
        Vector2 startPos = rect.anchoredPosition;
        
        float totalSize = tileSize + tileSpacing;
        float startX = -(gridSize - 1) * totalSize / 2f;
        float startY = (gridSize - 1) * totalSize / 2f;
        
        Vector2 targetPos = new Vector2(
            startX + targetGridPos.x * totalSize,
            startY - targetGridPos.y * totalSize
        );
        
        Debug.Log($"Animating from {startPos} to {targetPos}");
        
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        
        rect.anchoredPosition = targetPos;
        Debug.Log($"Animation complete. Final position: {rect.anchoredPosition}");
    }
    
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize;
    }
    
    private bool CheckPuzzleComplete()
    {
        if (emptyPosition.x != gridSize - 1 || emptyPosition.y != gridSize - 1)
        {
            return false;
        }
        
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (x == gridSize - 1 && y == gridSize - 1) continue;
                
                SlidingPuzzleTile tile = tiles[x, y];
                if (tile == null || tile.correctIndex != (y * gridSize + x))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    private void ClearGrid()
    {
        if (gridContainer == null) return;
        
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
