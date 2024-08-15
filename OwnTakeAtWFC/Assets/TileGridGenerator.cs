using UnityEngine;

public class TileGridGenerator : MonoBehaviour
{
    // Grid size
    public int gridSize = 5;

    // Array to hold the different tile prefabs
    public GameObject[] tilePrefabs;

    // 2D array to hold references to the instantiated tiles
    private GameObject[,] tileGrid;

    // Start is called before the first frame update
    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Initialize the grid array
        tileGrid = new GameObject[gridSize, gridSize];

        // Loop through each grid cell
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Randomly choose a tile prefab from the available options
                GameObject tilePrefab = tilePrefabs[Random.Range(0, tilePrefabs.Length)];

                // Instantiate the tile at the correct position
                Vector3 position = new Vector3(x, 0, y); // Position on the x-z plane
                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);

                // Store the reference in the grid array
                tileGrid[x, y] = tile;

                // Optionally, you can name the tile in the hierarchy
                tile.name = $"Tile_{x}_{y}_{tilePrefab.name}";
            }
        }
    }
}
