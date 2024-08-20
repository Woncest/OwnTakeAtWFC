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
        GenerateGrid();
    }

    void GenerateGrid()
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
                PrintNeighbors(x, y);

                // Randomly select a tile from the remaining possible tiles
                GameObject selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, y);
                Instantiate(selectedTilePrefab, position, Quaternion.identity);
            }
        }
    }

    void PrintNeighbors(int x, int y)
    {
        // Check and process the above neighbor
        if (y > 0)
        {
            //Go through all cellGrid[x,y - 1].possibleTiles and compare to cellGrid[x,y] and only keep what is allowed above
            List<GameObject> allowedTiles = cellGrid[x, y].possibleTiles.First().GetComponent<Tile>().allowedAbove;

            // Create a temporary list to store the tiles that need to be removed
            List<GameObject> tilesToRemove = new List<GameObject>();

            // Iterate over the possible tiles in the cell above
            for (int i = 0; i < cellGrid[x, y - 1].possibleTiles.Count; i++)
            {
                GameObject tile = cellGrid[x, y - 1].possibleTiles[i];

                // If the tile is not allowed, add it to the list of tiles to remove
                if (!allowedTiles.Contains(tile))
                {
                    tilesToRemove.Add(tile);
                }
            }

            // Remove the marked tiles from the original list after the loop completes
            foreach (GameObject tileToRemove in tilesToRemove)
            {
                cellGrid[x, y - 1].possibleTiles.Remove(tileToRemove);
            }
            Debug.Log($"Cell exists above at ({x}, {y - 1}) with {cellGrid[x, y - 1].possibleTiles.Count} possible tiles.");
        }

        // Check and process the right neighbor
        if (x < gridSize - 1)
        {
            //Go through all cellGrid[x,y - 1].possibleTiles and compare to cellGrid[x,y] and only keep what is allowed above
            List<GameObject> allowedTiles = cellGrid[x, y].possibleTiles.First().GetComponent<Tile>().allowedRight;

            // Create a temporary list to store the tiles that need to be removed
            List<GameObject> tilesToRemove = new List<GameObject>();

            // Iterate over the possible tiles in the cell above
            for (int i = 0; i < cellGrid[x + 1, y].possibleTiles.Count; i++)
            {
                GameObject tile = cellGrid[x + 1,y].possibleTiles[i];

                // If the tile is not allowed, add it to the list of tiles to remove
                if (!allowedTiles.Contains(tile))
                {
                    tilesToRemove.Add(tile);
                }
            }

            // Remove the marked tiles from the original list after the loop completes
            foreach (GameObject tileToRemove in tilesToRemove)
            {
                cellGrid[x + 1, y].possibleTiles.Remove(tileToRemove);
            }
            Debug.Log($"Cell exists to the right at ({x + 1}, {y}) with {cellGrid[x + 1, y].possibleTiles.Count} possible tiles.");
        }

        // Check and process the below neighbor
        if (y < gridSize - 1)
        {
            //Go through all cellGrid[x,y - 1].possibleTiles and compare to cellGrid[x,y] and only keep what is allowed above
            List<GameObject> allowedTiles = cellGrid[x, y].possibleTiles.First().GetComponent<Tile>().allowedBelow;

            // Create a temporary list to store the tiles that need to be removed
            List<GameObject> tilesToRemove = new List<GameObject>();

            // Iterate over the possible tiles in the cell above
            for (int i = 0; i < cellGrid[x, y + 1].possibleTiles.Count; i++)
            {
                GameObject tile = cellGrid[x, y + 1].possibleTiles[i];

                // If the tile is not allowed, add it to the list of tiles to remove
                if (!allowedTiles.Contains(tile))
                {
                    tilesToRemove.Add(tile);
                }
            }

            // Remove the marked tiles from the original list after the loop completes
            foreach (GameObject tileToRemove in tilesToRemove)
            {
                cellGrid[x, y + 1].possibleTiles.Remove(tileToRemove);
            }
            Debug.Log($"Cell exists below at ({x}, {y + 1}) with {cellGrid[x, y + 1].possibleTiles.Count} possible tiles.");
        }

        // Check and process the left neighbor
        if (x > 0)
        {
            //Go through all cellGrid[x,y - 1].possibleTiles and compare to cellGrid[x,y] and only keep what is allowed above
            List<GameObject> allowedTiles = cellGrid[x, y].possibleTiles.First().GetComponent<Tile>().allowedLeft;

            // Create a temporary list to store the tiles that need to be removed
            List<GameObject> tilesToRemove = new List<GameObject>();

            // Iterate over the possible tiles in the cell above
            for (int i = 0; i < cellGrid[x - 1, y].possibleTiles.Count; i++)
            {
                GameObject tile = cellGrid[x - 1, y].possibleTiles[i];

                // If the tile is not allowed, add it to the list of tiles to remove
                if (!allowedTiles.Contains(tile))
                {
                    tilesToRemove.Add(tile);
                }
            }

            // Remove the marked tiles from the original list after the loop completes
            foreach (GameObject tileToRemove in tilesToRemove)
            {
                cellGrid[x - 1, y].possibleTiles.Remove(tileToRemove);
            }
            Debug.Log($"Cell exists to the left at ({x - 1}, {y}) with {cellGrid[x - 1, y].possibleTiles.Count} possible tiles.");
        }
    }
}
