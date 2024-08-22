using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileGridGenerator : MonoBehaviour
{
    public int gridSize = 5;  // Size of the grid (gridSize x gridSize)
    public List<GameObject> tilePrefabs;  // Array of tile prefabs to choose from

    private Cell[,] cellGrid;

    void Start()
    {
        InitializeCellGrid();
        GenerateGrid();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ClearGrid();
            GenerateGrid();
        }
    }

    void InitializeCellGrid()
    {
        // Initialize the cell grid
        cellGrid = new Cell[gridSize, gridSize];

        // First, initialize all the cells in the grid
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Create a new Cell at the current grid position with all possible tiles
                cellGrid[x, y] = new Cell(tilePrefabs);
            }
        }
    }

    void GenerateGrid()
    {
        // Now, iterate through each cell to print neighbors and randomly select a tile
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Randomly select a tile from the remaining possible tiles
                GameObject selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];
                // Remove all other tiles from the list except the selected one
                cellGrid[x, y].possibleTiles.RemoveAll(tile => tile != selectedTilePrefab);
                // Set the selected tile on the cell
                cellGrid[x, y].SetTile(selectedTilePrefab);

                SetNeighbours(x, y);

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, y);
                cellGrid[x, y].instantiatedTile = Instantiate(selectedTilePrefab, position, Quaternion.identity);
            }
        }
    }

    void ClearGrid()
    {
        // Destroy all instantiated tiles and clear cell grid data
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Destroy the instantiated tile if it exists
                if (cellGrid[x, y].instantiatedTile != null)
                {
                    Destroy(cellGrid[x, y].instantiatedTile);
                }

                // Reinitialize the cell's possible tiles
                cellGrid[x, y].possibleTiles = new List<GameObject>(tilePrefabs);
                cellGrid[x, y].instantiatedTile = null;  // Clear reference to the instantiated tile
            }
        }
    }

    //ONLY NEIGHBOURS ARE BEING MODIFIED
    void SetNeighbours(int x, int y)
    {
        // Define directions and corresponding allowed tile lists
        (int xOffset, int yOffset, System.Func<Tile, List<GameObject>> getAllowedTiles, string direction)[] directions = {
            (-1, 0, tile => tile.allowedAbove, "above"),
            (0, 1, tile => tile.allowedRight, "right"),
            (1, 0, tile => tile.allowedBelow, "below"),
            (0, -1, tile => tile.allowedLeft, "left")
        };

        // Ensure that the tile in the current cell is set
        if (cellGrid[x, y].IsTileSet())
        {
            Tile currentTile = cellGrid[x, y].instantiatedTile.GetComponent<Tile>();

            // Check each direction for neighboring tiles
            foreach (var dir in directions)
            {
                int neighborX = x + dir.xOffset;
                int neighborY = y + dir.yOffset;

                // Check if the neighbor is within bounds
                if (neighborX >= 0 && neighborX < gridSize && neighborY >= 0 && neighborY < gridSize)
                {
                    // Get the allowed tiles for the current direction based on the current cell's tile
                    List<GameObject> allowedTiles = dir.getAllowedTiles(currentTile);

                    // Proceed only if the neighboring cell hasn't set a tile yet
                    if (!cellGrid[neighborX, neighborY].IsTileSet())
                    {
                        List<GameObject> tilesToRemove = new List<GameObject>();

                        // Iterate over the possible tiles in the neighboring cell
                        foreach (GameObject tile in cellGrid[neighborX, neighborY].possibleTiles)
                        {
                            // If the tile is not allowed, mark it for removal
                            if (!allowedTiles.Contains(tile))
                            {
                                tilesToRemove.Add(tile);
                            }
                        }

                        // Remove the marked tiles from the neighboring cell
                        foreach (GameObject tileToRemove in tilesToRemove)
                        {
                            cellGrid[neighborX, neighborY].possibleTiles.Remove(tileToRemove);
                        }
                    }
                }
            }
        }
    }

    //RECURSIVE UNTIL NO CHANGES
    /*void SetNeighbours(int x, int y)
    {
        // Define directions and corresponding allowed tile lists
        (int xOffset, int yOffset, System.Func<Tile, List<GameObject>> getAllowedTiles, string direction)[] directions = {
            (-1, 0, tile => tile.allowedAbove, "above"),
            (0, 1, tile => tile.allowedRight, "right"),
            (1, 0, tile => tile.allowedBelow, "below"),
            (0, -1, tile => tile.allowedLeft, "left")
        };

        // Ensure that the tile in the current cell is set
        if (cellGrid[x, y].IsTileSet())
        {
            Tile currentTile = cellGrid[x, y].instantiatedTile.GetComponent<Tile>();

            // Loop through each direction (above, below, left, right)
            foreach (var dir in directions)
            {
                int neighborX = x + dir.xOffset;
                int neighborY = y + dir.yOffset;

                // Check if the neighbor is within bounds
                if (neighborX >= 0 && neighborX < gridSize && neighborY >= 0 && neighborY < gridSize)
                {
                    // Only proceed if the neighboring cell doesn't have a tile set yet
                    if (!cellGrid[neighborX, neighborY].IsTileSet())
                    {
                        List<GameObject> allowedTiles = dir.getAllowedTiles(currentTile);
                        List<GameObject> tilesToRemove = new List<GameObject>();
                        bool tileChanged = false;

                        // Iterate over the possible tiles in the neighboring cell
                        foreach (GameObject tile in cellGrid[neighborX, neighborY].possibleTiles)
                        {
                            // If the tile is not allowed, mark it for removal
                            if (!allowedTiles.Contains(tile))
                            {
                                tilesToRemove.Add(tile);
                            }
                        }

                        // Remove the marked tiles from the neighboring cell
                        foreach (GameObject tileToRemove in tilesToRemove)
                        {
                            cellGrid[neighborX, neighborY].possibleTiles.Remove(tileToRemove);
                            tileChanged = true;  // Mark that a change occurred
                        }

                        // If any changes were made, recursively update the neighbors of the neighbor
                        if (tileChanged)
                        {
                            SetNeighbours(neighborX, neighborY);
                        }
                    }
                }
            }
        }
    }*/

}