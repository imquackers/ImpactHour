using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// CircuitBoardPuzzle
// Flow:
// OpenPuzzle, GenerateCircuit, Player rotates tiles,
//  Power is traced recursively, Destination powered, Puzzle complete

public class CircuitBoardPuzzle : PuzzleBase
{
    [Header("Circuit Settings")]

    // Size of the grid (gridSize x gridSize)
    public int gridSize = 4;

    // Speed of tile rotation animation (degrees per second)
    public float tileRotationSpeed = 180f;

    [Header("UI Elements")]

    // Parent transform that holds all tile instances
    public Transform gridContainer;

    // Prefab for individual circuit tiles
    public GameObject tilePrefab;

    // Optional UI elements, add later
    public Image powerSourceImage;
    public Image destinationImage;
    public Text statusText;

    // 2D array storing all generated tiles
    private CircuitTile[,] tiles;

    // Grid coordinates for source and destination
    private Vector2Int sourcePos;
    private Vector2Int destinationPos;

    // Prevents multiple tiles rotating at once
    private bool isRotating = false;

    
    // PUZZLE ENTRY POINT
   

    protected override void OpenPuzzle()
    {
        base.OpenPuzzle();
        GenerateCircuit();
    }

   
    // CIRCUIT GENERATION
    

    private void GenerateCircuit()
    {
        ClearGrid();

        // Create tile array
        tiles = new CircuitTile[gridSize, gridSize];

        // Source at top middle
        sourcePos = new Vector2Int(gridSize / 2, gridSize - 1);

        // Destination at bottom middle
        destinationPos = new Vector2Int(gridSize / 2, 0);

        // Generate a guaranteed valid path between source and destination
        List<Vector2Int> path = GeneratePath(sourcePos, destinationPos);

        // Instantiate and configure tiles
        PlaceTiles(path);

        // Scramble tile rotations (without breaking guaranteed path)
        RandomizeRotations();

        // Update visual power state
        UpdatePowerFlow();

        if (statusText != null)
        {
            statusText.text = $"connect the circuit ({gridSize}x{gridSize} Grid)";
        }
    }

    
    // PATH GENERATION
    

    // Creates a zig-zag downward path from start to end.
    // Ensures puzzle is always solvable.
    private List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;
        path.Add(current);

        // Random starting horizontal direction
        int leftRightDirection = Random.value > 0.5f ? 1 : -1;

        // Move downward row by row
        while (current.y > end.y + 1)
        {
            current.y--;
            path.Add(new Vector2Int(current.x, current.y));

            // Random horizontal steps before next downward move
            int horizontalSteps = Random.Range(2, 5);

            for (int i = 0; i < horizontalSteps; i++)
            {
                int newX = current.x + leftRightDirection;

                // Stay inside grid bounds
                if (newX >= 0 && newX < gridSize)
                {
                    current.x = newX;
                    path.Add(new Vector2Int(current.x, current.y));
                }
                else
                {
                    break;
                }
            }

            // Alternate horizontal direction
            leftRightDirection *= -1;
        }

        // Align horizontally with destination
        while (current.x != end.x)
        {
            current.x += current.x < end.x ? 1 : -1;
            path.Add(new Vector2Int(current.x, current.y));
        }

        // Final vertical alignment
        while (current.y > end.y)
        {
            current.y--;
            path.Add(new Vector2Int(current.x, current.y));
        }

        if (!path.Contains(end))
        {
            path.Add(end);
        }

        return path;
    }

    // Checks if coordinate is inside grid bounds
    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridSize &&
               pos.y >= 0 && pos.y < gridSize;
    }

  
    // TILE PLACEMENT


    // Instantiates every tile and assigns its type + correct rotation
    private void PlaceTiles(List<Vector2Int> path)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                GameObject tileObj = Instantiate(tilePrefab, gridContainer);
                CircuitTile tile = tileObj.GetComponent<CircuitTile>();

                if (tile == null)
                    tile = tileObj.AddComponent<CircuitTile>();

                tile.Initialize(this, pos);

                if (pos == sourcePos)
                {
                    // Source tile (fixed)
                    tile.SetTileType(CircuitTile.TileType.Source, 0);
                }
                else if (pos == destinationPos)
                {
                    // Destination tile (must connect to previous path tile)
                    Vector2Int beforeDest = path[path.Count - 2];
                    Vector2Int diff = beforeDest - destinationPos;

                    int connectionDir = 0;

                    // Determine connection direction
                    if (diff.x == -1) connectionDir = 2;
                    else if (diff.x == 1) connectionDir = 0;
                    else if (diff.y == -1) connectionDir = 3;
                    else if (diff.y == 1) connectionDir = 1;

                    tile.SetTileType(CircuitTile.TileType.Destination, connectionDir);
                }
                else if (path.Contains(pos))
                {
                    // Regular path tile
                    int index = path.IndexOf(pos);
                    Vector2Int prev = index > 0 ? path[index - 1] : pos;
                    Vector2Int next = index < path.Count - 1 ? path[index + 1] : pos;

                    CircuitTile.TileType type = DetermineTileType(pos, prev, next);
                    int rotation = DetermineRotation(pos, prev, next, type);

                    tile.SetTileType(type, rotation);
                }
                else
                {
                    // Non-path tile
                    tile.SetTileType(CircuitTile.TileType.Empty, 0);
                }

                tiles[x, y] = tile;
            }
        }
    }

    // Determines whether tile is straight or corner
    private CircuitTile.TileType DetermineTileType(Vector2Int current, Vector2Int prev, Vector2Int next)
    {
        Vector2Int dir1 = prev - current;
        Vector2Int dir2 = next - current;

        if ((dir1.x != 0 && dir2.x != 0) ||
            (dir1.y != 0 && dir2.y != 0))
            return CircuitTile.TileType.Straight;

        return CircuitTile.TileType.Corner;
    }

    // Determines correct rotation so tile connects prev and next
    private int DetermineRotation(Vector2Int current, Vector2Int prev, Vector2Int next, CircuitTile.TileType type)
    {
        Vector2Int dir1 = prev - current;
        Vector2Int dir2 = next - current;

        if (type == CircuitTile.TileType.Straight)
        {
            // Horizontal or vertical alignment
            return (dir1.x != 0 || dir2.x != 0) ? 0 : 1;
        }
        else
        {
            // Corner cases (based on direction combinations)
            if (dir1.x == 1 && dir2.y == 1 || dir1.y == 1 && dir2.x == 1) return 0;
            if (dir1.x == -1 && dir2.y == 1 || dir1.y == 1 && dir2.x == -1) return 1;
            if (dir1.x == -1 && dir2.y == -1 || dir1.y == -1 && dir2.x == -1) return 2;
            if (dir1.x == 1 && dir2.y == -1 || dir1.y == -1 && dir2.x == 1) return 3;
        }

        return 0;
    }


    // SCRAMBLING


    // Randomly rotates tiles to create the puzzle challenge.
    // Does NOT rotate tiles adjacent to source/destination, so puzzle is always possiblee
    // to avoid early accidental completion.
    private void RandomizeRotations()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                bool isAdjacentToSource =
                    Vector2Int.Distance(pos, sourcePos) <= 1.1f && pos != sourcePos;

                bool isAdjacentToDestination =
                    Vector2Int.Distance(pos, destinationPos) <= 1.1f && pos != destinationPos;

                if (tiles[x, y] != null &&
                    tiles[x, y].CanRotate() &&
                    !isAdjacentToSource &&
                    !isAdjacentToDestination)
                {
                    int randomRotations = Random.Range(1, 4);
                    for (int i = 0; i < randomRotations; i++)
                        tiles[x, y].RotateInstant();
                }
            }
        }
    }

  
    // TILE INTERACTION
 

    public void OnTileClicked(CircuitTile tile)
    {
        if (isRotating || isSolved) return;

        if (tile.CanRotate())
            StartCoroutine(RotateTileCoroutine(tile));
    }

    private System.Collections.IEnumerator RotateTileCoroutine(CircuitTile tile)
    {
        isRotating = true;

        // Smooth animated rotation
        yield return StartCoroutine(tile.RotateAnimated(tileRotationSpeed));

        isRotating = false;

        // Recalculate power after rotation
        UpdatePowerFlow();

        // Check if puzzle solved
        CheckCompletion();
    }


    // POWER TRACING SYSTEM
 

    // Recalculates which tiles are powered by tracing
    // from the source recursively.
    private void UpdatePowerFlow()
    {
        HashSet<Vector2Int> powered = new HashSet<Vector2Int>();

        // Start recursive trace from source
        TracePower(sourcePos, powered, new HashSet<Vector2Int>());

        // Update visual power state
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (tiles[x, y] != null)
                    tiles[x, y].SetPowered(powered.Contains(new Vector2Int(x, y)));
            }
        }
    }

    // Depth-first search that follows valid bidirectional connections.
    private void TracePower(Vector2Int pos,
                            HashSet<Vector2Int> powered,
                            HashSet<Vector2Int> visited)
    {
        if (!IsValidPosition(pos) || visited.Contains(pos)) return;

        visited.Add(pos);

        CircuitTile tile = tiles[pos.x, pos.y];
        if (tile == null || tile.tileType == CircuitTile.TileType.Empty) return;

        powered.Add(pos);

        // Get outgoing connections from this tile
        List<Vector2Int> connections = tile.GetConnections(pos);

        foreach (Vector2Int nextPos in connections)
        {
            if (!IsValidPosition(nextPos) || visited.Contains(nextPos))
                continue;

            CircuitTile nextTile = tiles[nextPos.x, nextPos.y];
            if (nextTile == null) continue;

            // Only continue if next tile connects back
            if (nextTile.ConnectsTo(nextPos, pos))
            {
                TracePower(nextPos, powered, visited);
            }
        }
    }

    // COMPLETION
   

    private void CheckCompletion()
    {
        CircuitTile destTile = tiles[destinationPos.x, destinationPos.y];

        if (destTile != null && destTile.IsPowered())
        {
            if (statusText != null)
                statusText.text = "Circuit Complete! Power Restored!";

            CompletePuzzle();
        }
    }


    // CLEANUP


    private void ClearGrid()
    {
        if (gridContainer != null)
        {
            foreach (Transform child in gridContainer)
                Destroy(child.gameObject);
        }
    }

    protected override void ClosePuzzle()
    {
        base.ClosePuzzle();
        ClearGrid();
    }
}