using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileGridGenerator : MonoBehaviour
{
    public int gridSize = 5;  // Size of the grid (gridSize x gridSize)
    public List<GameObject> tilePrefabs;  // Array of tile prefabs to choose from

    private Cell[,] cellGrid;

    public bool isKeyPressed = false;

    void Start()
    {
        StartCoroutine(GenerateGrid());
    }

    void Update(){
        /*if(Input.GetKeyDown(KeyCode.Space)){
            isKeyPressed = true;
        }*/
    }

    IEnumerator GenerateGrid()
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

        // Now, iterate through each cell to print neighbors and randomly select a tile
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Print out the neighbors for this cell
                //SetNeighbours(x, y);

                while (!isKeyPressed)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        isKeyPressed = true;
                    }

                    // Make sure to yield control so that Unity doesn't freeze
                    yield return null;
                }

                // Randomly select a tile from the remaining possible tiles
                GameObject selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                SetNeighbours(x, y);

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, y);
                Instantiate(selectedTilePrefab, position, Quaternion.identity);
                isKeyPressed = false;
            }
        }
    }

    void SetNeighbours(int x, int y)
    {
        // Define directions and corresponding allowed tile lists
        (int xOffset, int yOffset, System.Func<Tile, List<GameObject>> getAllowedTiles, string direction)[] directions = {
            (-1, 0, tile => tile.allowedAbove, "above"),
            (0, 1, tile => tile.allowedRight, "right"),
            (1, 0, tile => tile.allowedBelow, "below"),
            (0, -1, tile => tile.allowedLeft, "left")
        };

        foreach (var dir in directions)
        {
            int neighborX = x + dir.xOffset;
            int neighborY = y + dir.yOffset;

            // Check if the neighbor is within bounds
            if (neighborX >= 0 && neighborX < gridSize && neighborY >= 0 && neighborY < gridSize)
            {
                // Get the allowed tiles for the current direction from the current cell
                List<GameObject> allowedTiles = dir.getAllowedTiles(cellGrid[x, y].possibleTiles.First().GetComponent<Tile>());
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
