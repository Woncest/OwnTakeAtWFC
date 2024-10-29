using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class TileGridGenerator : MonoBehaviour
{
    public int gridSize = 5;  // Size of the grid (gridSize x gridSize)

    [HideInInspector] public int newGridSize = 5;
    public CameraZoomController cameraZoomController;
    public List<GameObject> tilePrefabs;  // Array of tile prefabs to choose from
    public bool showGenerationProcess = true;  // New boolean to control visual generation
    public float generationDelay = 0.01f;  // Delay between each tile generation
    private Coroutine coroutine;

    private Cell[,] cellGrid;

    void Start()
    {
        InitializeCellGrid();
        GenerateAndTimeGrid();  // Modified method call
        newGridSize = gridSize;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            ClearGrid();
            gridSize = newGridSize;
            cameraZoomController.AdjustZoom();
            InitializeCellGrid();
            GenerateAndTimeGrid();  // Modified method call
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

    void GenerateAndTimeGrid()  // New method to handle timing
    {
        Stopwatch stopwatch = new Stopwatch();  // Start timing
        stopwatch.Start();

        if (showGenerationProcess)
        {
            coroutine = StartCoroutine(GenerateGridWithVisualProcess());  // Start coroutine for visual generation
        }
        else
        {
            GenerateGridGoThroughEverything();  // Generate the grid normally
        }

        stopwatch.Stop();  // Stop timing

        // Print the elapsed time in seconds with millisecond precision
        // TODO if you also want the time when showing the processs fix it for showing the process
        if(!showGenerationProcess) UnityEngine.Debug.Log($"GenerateGrid() took {stopwatch.Elapsed.TotalSeconds:F3} seconds ({stopwatch.Elapsed.TotalMilliseconds:F0} ms)");
    }

    IEnumerator GenerateGridWithVisualProcess()  // New coroutine for visual generation
    {
        // Iterate through each cell to print neighbors and randomly select a tile
        for (int y = 0; y < gridSize; y++)
        {
            for (int i = gridSize - 1; i > 0; i--){
                    for(int z = gridSize - 1; z > 0; z--){
                        if(!cellGrid[z,i].tileSet && cellGrid[z,i].possibleTiles.Count != tilePrefabs.Count){
                            SetNeighboursOnlyNeighbours(z,i);
                        }
                    }
                }
            for (int x = 0; x < gridSize; x++)
            {
                // Check if there are any possible tiles left
                if (cellGrid[x, y].possibleTiles.Count == 0)
                {
                    continue;
                }

                // Randomly select a tile from the remaining possible tiles
                GameObject selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                // Remove the selected tile from the possibleTiles list
                cellGrid[x, y].possibleTiles.RemoveAll(tile => tile != selectedTilePrefab);

                // Set the selected tile on the cell
                cellGrid[x, y].SetTile(selectedTilePrefab);

                // Proceed with setting neighbors and instantiating the tile
                SetNeighboursHorizontally(x, y);

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, y);
                cellGrid[x, y].instantiatedTile = Instantiate(selectedTilePrefab, position, Quaternion.identity);

                // Wait for a specified delay before moving to the next tile, allowing visualization
                yield return new WaitForSeconds(generationDelay);
            }
        }
    }

    //Going Through Bottom Left to Top Right
    void GenerateGrid()
    {
        // Iterate through each cell to print neighbors and randomly select a tile
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                // Check if there are any possible tiles left
                if (cellGrid[x, y].possibleTiles.Count == 0)
                {
                    continue;
                }

                // Randomly select a tile from the remaining possible tiles
                GameObject selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                // Remove the selected tile from the possibleTiles list
                cellGrid[x, y].possibleTiles.RemoveAll(tile => tile != selectedTilePrefab);

                // Set the selected tile on the cell
                cellGrid[x, y].SetTile(selectedTilePrefab);

                // Proceed with setting neighbors and instantiating the tile
                SetNeighboursHorizontally(x, y);

                for (int i = 0; i < gridSize; i++){
                    for(int z = 0; z < gridSize; z++){
                        if(!cellGrid[z,i].tileSet){
                            SetNeighboursOnlyNeighbours(z,i);
                        }
                    }
                }

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, y);
                cellGrid[x, y].instantiatedTile = Instantiate(selectedTilePrefab, position, Quaternion.identity);
            }
        }
    }

    //After each pass goes through each not set cell and each cell that has less than the original amount
    void GenerateGridGoThroughEverything()
    {
        // Iterate through each cell to print neighbors and randomly select a tile
        for (int y = 0; y < gridSize; y++)
        {
            for (int i = gridSize - 1; i > 0; i--){
                    for(int z = gridSize - 1; z > 0; z--){
                        if(!cellGrid[z,i].tileSet && cellGrid[z,i].possibleTiles.Count != tilePrefabs.Count){
                            SetNeighboursOnlyNeighbours(z,i);
                        }
                    }
                }
                
            for (int x = 0; x < gridSize; x++)
            {
                // Check if there are any possible tiles left
                if (cellGrid[x, y].possibleTiles.Count == 0)
                {
                    continue;
                }

                // Randomly select a tile from the remaining possible tiles
                GameObject selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                // Remove the selected tile from the possibleTiles list
                cellGrid[x, y].possibleTiles.RemoveAll(tile => tile != selectedTilePrefab);

                // Set the selected tile on the cell
                cellGrid[x, y].SetTile(selectedTilePrefab);

                // Proceed with setting neighbors and instantiating the tile
                SetNeighboursHorizontally(x, y);

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, y);
                cellGrid[x, y].instantiatedTile = Instantiate(selectedTilePrefab, position, Quaternion.identity);
            }
        }
    }

    //Next Tile set is the one with the least options or tied with least
    void GenerateGridLeastEntropy()
    {
        // Create a list of cells that need tiles to be set
        List<(int x, int y)> unprocessedCells = new List<(int x, int y)>();

        // Populate the list with all grid positions
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if (!cellGrid[x, y].IsTileSet())
                {
                    unprocessedCells.Add((x, y));
                }
            }
        }

        // While there are still unprocessed cells
        while (unprocessedCells.Count > 0)
        {
            // Sort the unprocessed cells by the number of possible tiles (ascending order)
            unprocessedCells.Sort((a, b) => cellGrid[a.x, a.y].possibleTiles.Count.CompareTo(cellGrid[b.x, b.y].possibleTiles.Count));

            // Get the cell with the fewest possible tiles
            var cellPosition = unprocessedCells[0];
            int x = cellPosition.x;
            int y = cellPosition.y;

            // If there are no possible tiles, skip this cell (this case shouldn't happen unless all options are eliminated)
            if (cellGrid[x, y].possibleTiles.Count == 0)
            {
                unprocessedCells.RemoveAt(0);
                continue;
            }

            // Randomly select a tile from the remaining possible tiles
            GameObject selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];
            cellGrid[x, y].possibleTiles.RemoveAll(tile => tile != selectedTilePrefab);

            // Set the selected tile on the cell
            cellGrid[x, y].SetTile(selectedTilePrefab);

            // Instantiate the selected tile at the grid position
            Vector3 position = new Vector3(x, 0, y);
            cellGrid[x, y].instantiatedTile = Instantiate(selectedTilePrefab, position, Quaternion.identity);

            // Set neighbors after placing the tile
            SetNeighboursHorizontally(x, y);

            // Remove this cell from the unprocessed list as it's now set
            unprocessedCells.RemoveAt(0);
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
                cellGrid[x,y].tileSet = false;
            }
        }
    }

    //ONLY NEIGHBOURS ARE BEING MODIFIED
    void SetNeighboursOnlyNeighbours(int x, int y)
    {
        // Define directions and corresponding allowed tile lists
        (int xOffset, int yOffset, System.Func<Tile, List<GameObject>> getAllowedTiles, string direction)[] directions = {
            (-1, 0, tile => tile.allowedBelow, "below"),
            (0, 1, tile => tile.allowedLeft, "left"),
            (1, 0, tile => tile.allowedAbove, "above"),
            (0, -1, tile => tile.allowedRight, "right")
        };

        // Ensure that the current cell has possible tiles
        if (cellGrid[x, y].possibleTiles.Count > 0)
        {
            // Check each direction for neighboring tiles
            foreach (var dir in directions)
            {
                int neighborX = x + dir.xOffset;
                int neighborY = y + dir.yOffset;

                // Check if the neighbor is within bounds
                if (neighborX >= 0 && neighborX < gridSize && neighborY >= 0 && neighborY < gridSize)
                {
                    if(cellGrid[neighborX, neighborY].tileSet) continue;
                    // Create a combined allowed tiles list for the current direction
                    List<GameObject> combinedAllowedTiles = new List<GameObject>();

                    // Gather allowed tiles from all possible tiles in the current cell
                    foreach (GameObject possibleTile in cellGrid[x, y].possibleTiles)
                    {
                        Tile tileComponent = possibleTile.GetComponent<Tile>();
                        combinedAllowedTiles.AddRange(dir.getAllowedTiles(tileComponent));
                    }

                    // Remove duplicates from combinedAllowedTiles (optional but recommended)
                    combinedAllowedTiles = new List<GameObject>(new HashSet<GameObject>(combinedAllowedTiles));

                    // Proceed only if the neighboring cell hasn't set a tile yet
                    if (!cellGrid[neighborX, neighborY].IsTileSet())
                    {
                        List<GameObject> tilesToRemove = new List<GameObject>();

                        // Iterate over the possible tiles in the neighboring cell
                        foreach (GameObject tile in cellGrid[neighborX, neighborY].possibleTiles)
                        {
                            // If the tile is not in the combined allowed list, mark it for removal
                            if (!combinedAllowedTiles.Contains(tile))
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

    //RECURSIVE PERHAPS FAULTY
    //TODO FIX?
    void SetNeighboursRecursive(int x, int y)
    {
        // Define directions and corresponding allowed tile lists
        (int xOffset, int yOffset, System.Func<Tile, List<GameObject>> getAllowedTiles, string direction)[] directions = {
            (-1, 0, tile => tile.allowedBelow, "below"),
            (0, 1, tile => tile.allowedLeft, "left"),
            (1, 0, tile => tile.allowedAbove, "above"),
            (0, -1, tile => tile.allowedRight, "right")
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
                    if(cellGrid[neighborX, neighborY].tileSet) continue;
                    // Only proceed if the neighboring cell doesn't have a tile set yet
                    if (!cellGrid[neighborX, neighborY].IsTileSet())
                    {
                        // Generate the list of all allowed tiles based on the possible tiles in the current cell
                        List<GameObject> allAllowedTiles = new List<GameObject>();
                        foreach (GameObject possibleTile in cellGrid[x, y].possibleTiles)
                        {
                            Tile tileComponent = possibleTile.GetComponent<Tile>();
                            List<GameObject> allowedTiles = dir.getAllowedTiles(tileComponent);
                            allAllowedTiles.AddRange(allowedTiles);
                        }

                        // Remove duplicates from the allowed tiles list
                        allAllowedTiles = new List<GameObject>(new HashSet<GameObject>(allAllowedTiles));

                        List<GameObject> tilesToRemove = new List<GameObject>();
                        bool tileChanged = false;

                        // Iterate over the possible tiles in the neighboring cell
                        foreach (GameObject tile in cellGrid[neighborX, neighborY].possibleTiles)
                        {
                            // If the tile is not in the allowed tiles list, mark it for removal
                            if (!allAllowedTiles.Contains(tile))
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
                            SetNeighboursRecursive(neighborX, neighborY);
                        }
                    }
                }
            }
        }
    }

    void SetNeighboursHorizontally(int x, int y)
    {
        // Define directions and corresponding allowed tile lists
        (int xOffset, int yOffset, System.Func<Tile, List<GameObject>> getAllowedTiles, string direction)[] directions = {
            (-1, 0, tile => tile.allowedBelow, "below"),  // Above Neighbor
            (1, 0, tile => tile.allowedAbove, "above"),  // Below Neighbor
            (0, -1, tile => tile.allowedRight, "right"),  // Left Neighbor
            (0, 1, tile => tile.allowedLeft, "left")  // Right Neighbor
        };

        // Ensure that there are possible tiles in the current cell
        if (cellGrid[x, y].possibleTiles.Count > 0)
        {
            // Loop through each direction (above, below, left, right)
            foreach (var dir in directions)
            {
                int neighborX = x + dir.xOffset;
                int neighborY = y + dir.yOffset;

                // Check if the neighbor is within bounds
                if (neighborX >= 0 && neighborX < gridSize && neighborY >= 0 && neighborY < gridSize)
                {
                    if(cellGrid[neighborX, neighborY].tileSet) continue;
                    // Proceed only if the neighboring cell hasn't set a tile yet
                    if (!cellGrid[neighborX, neighborY].IsTileSet())
                    {
                        // Create a list to store the allowed tiles from all possible tiles in the current cell
                        List<GameObject> combinedAllowedTiles = new List<GameObject>();

                        // Iterate over the possible tiles in the current cell
                        foreach (GameObject possibleTile in cellGrid[x, y].possibleTiles)
                        {
                            Tile tileComponent = possibleTile.GetComponent<Tile>();
                            List<GameObject> allowedTiles = dir.getAllowedTiles(tileComponent);

                            // Add allowed tiles from this possible tile to the combined list
                            combinedAllowedTiles.AddRange(allowedTiles);
                        }

                        // Remove duplicates from combinedAllowedTiles
                        combinedAllowedTiles = new List<GameObject>(new HashSet<GameObject>(combinedAllowedTiles));

                        List<GameObject> tilesToRemove = new List<GameObject>();

                        // Iterate over the possible tiles in the neighboring cell
                        foreach (GameObject tile in cellGrid[neighborX, neighborY].possibleTiles)
                        {
                            // If the tile is not in the combined allowed tiles list, mark it for removal
                            if (!combinedAllowedTiles.Contains(tile))
                            {
                                tilesToRemove.Add(tile);
                            }
                        }

                        // Remove the marked tiles from the neighboring cell
                        foreach (GameObject tileToRemove in tilesToRemove)
                        {
                            cellGrid[neighborX, neighborY].possibleTiles.Remove(tileToRemove);
                        }

                        // Recursively propagate the constraints horizontally or vertically
                        PropagateHorizontalOrVerticalConstraints(neighborX, neighborY);
                    }
                    if(neighborX != x){
                        if(y + 1< gridSize) SetNeighboursOnlyNeighbours(neighborX, y + 1);
                        if(y - 1>= 0) SetNeighboursOnlyNeighbours(neighborX, y - 1);
                    }else{
                        if(x + 1 < gridSize) SetNeighboursOnlyNeighbours(x + 1, neighborY);
                        if(x - 1>= 0) SetNeighboursOnlyNeighbours(x - 1, neighborY);
                    }
                }
            }
        }
    }

    // Helper method to propagate horizontal/vertical constraints on neighbors
    void PropagateHorizontalOrVerticalConstraints(int x, int y)
    {
        // Check horizontal neighbors (left and right) and propagate
        SetNeighboursOnlyNeighbours(x, y);  // Make sure to limit it to just the direct neighbors
    }

}