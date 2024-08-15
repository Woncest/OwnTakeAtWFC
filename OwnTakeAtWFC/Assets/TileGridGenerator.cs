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
            }
        }
    }
}
