using UnityEngine;

public class TileGridGenerator : MonoBehaviour
{
    public int gridSize = 5;  // Size of the grid (gridSize x gridSize)
    public GameObject[] tilePrefabs;  // Array of tile prefabs to choose from

    private Cell[,] cellGrid;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Initialize the cell grid
        cellGrid = new Cell[gridSize, gridSize];

        // Iterate through each cell in the grid
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Create a new Cell at the current grid position
                cellGrid[x, y] = new Cell(tilePrefabs);

                // Randomly select a tile from the tilePrefabs array
                GameObject selectedTilePrefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, y);
                Instantiate(selectedTilePrefab, position, Quaternion.identity);

                // Set the neighbors for this cell
                SetNeighbors(x, y);
            }
        }
    }

    void SetNeighbors(int x, int y)
    {
        // Check and process the above neighbor
        if (y > 0)
        {
            Debug.Log($"Cell exists above at ({x}, {y - 1})");
            // Additional logic to set the above neighbor if needed
        }

        // Check and process the right neighbor
        if (x < gridSize - 1)
        {
            Debug.Log($"Cell exists to the right at ({x + 1}, {y})");
            // Additional logic to set the right neighbor if needed
        }

        // Check and process the below neighbor
        if (y < gridSize - 1)
        {
            Debug.Log($"Cell exists below at ({x}, {y + 1})");
            // Additional logic to set the below neighbor if needed
        }

        // Check and process the left neighbor
        if (x > 0)
        {
            Debug.Log($"Cell exists to the left at ({x - 1}, {y})");
            // Additional logic to set the left neighbor if needed
        }
    }
}