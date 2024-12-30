using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Unity.VisualScripting;

public class TileGridGenerator : MonoBehaviour
{
    public int gridSize = 5;  // Size of the grid (gridSize x gridSize)

    [HideInInspector] public int newGridSize = 5;
    public CameraZoomController cameraZoomController;
    [HideInInspector] public List<GameObject> tilePrefabs;  // Array of tile prefabs to choose from
    public bool showGenerationProcess = true;  // New boolean to control visual generation
    public float generationDelay = 0.01f;  // Delay between each tile generation
    private Coroutine coroutine;

    private Cell[,] cellGrid;

    [HideInInspector] public int streetLength = 2;

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

    void RepopulateCellGrid(){
        // First, initialize all the cells in the grid
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if(cellGrid[x, y].tileSet){
                    continue;
                }
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
            //GenerateGridLeastEntropy();
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
#region PastExample
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
#endregion
    //After each pass goes through each not set cell and each cell that has less than the original amount
    void GenerateGridGoThroughEverything()
    {
        // Iterate through each cell to print neighbors and randomly select a tile
        for (int y = 0; y < gridSize; y++)
        {
            //RepopulateCellGrid();

            GoThroughEverything();

            /*if (y > 0)
            {
                break;
            }*/

            for (int x = 0; x < gridSize; x++)
            {
                // Check if there are any possible tiles left
                if (cellGrid[x, y].possibleTiles.Count == 0 || cellGrid[x, y].tileSet)
                {
                    continue;
                }

                // Randomly select a tile from the remaining possible tiles
                GameObject selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                //More than rerolling 2 time affect the structure majorly, with 2 times it also affects it but less, 2 resulst in almost no loops that are not connected
                /*if (selectedTilePrefab.name.Contains("Curve"))
                {
                    // If it was a curve reselect a tile
                    selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];
                }

                if (selectedTilePrefab.name.Contains("Curve"))
                {
                    // If it was a curve reselect a tile
                    selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];
                }*/

                //Check if the selected Tile is acceptable for forcing the desired amount of streets
                List<GameObject> tilesRight = selectedTilePrefab.GetComponent<Tile>().allowedAbove;
                bool hasStreetStraight = tilesRight.Any(tile => tile.name == "Street_Straight");

                //Removes Curves/Threeways and Intersections if ahead is not clear for the length which the road has to be
                /*if(hasStreetStraight && !IsAheadClear(x,y,streetLength) &&
                selectedTilePrefab.gameObject.name != "Street_Empty" && selectedTilePrefab.gameObject.name != "Street_Straight"){
                    if (!IsAheadClear(x, y, streetLength))
                        {
                            // Filter the possibleTiles list to remove tiles that have "Street_Straight" in allowedAbove
                            cellGrid[x, y].possibleTiles = cellGrid[x, y].possibleTiles
                                .Where(tilePrefab =>
                                {
                                    // Access the Tile component
                                    Tile tileComponent = tilePrefab.GetComponent<Tile>();
                                    
                                    // Check if allowedAbove contains any GameObject with the name "Street_Straight"
                                    return tileComponent.allowedAbove.All(allowedTile => allowedTile.name != "Street_Straight");
                                })
                                .ToList();
                        }
                        cellGrid[x,y].possibleTiles.Remove(selectedTilePrefab);
                        selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];
                }*/

                //TODO fix the OVERCOOKED
                //TODO think if looking one back even makes sense 
                //TODO maybe make an exception if Street_Straigh is involved, in which manne makes sense ?!?
                /*if(x > 0){
                    if(!IsAheadClear(x,y,streetLength) && cellGrid[x - 1,y].instantiatedTile.gameObject.name != "Street_Straight"){
                        // Filter the possibleTiles list to remove tiles that have "Street_Straight" in allowedAbove
                        cellGrid[x, y].possibleTiles = cellGrid[x, y].possibleTiles
                            .Where(tilePrefab =>
                            {
                                // Access the Tile component
                                Tile tileComponent = tilePrefab.GetComponent<Tile>();
                                        
                                // Check if allowedAbove contains any GameObject with the name "Street_Straight"
                                return tileComponent.allowedAbove.All(allowedTile => allowedTile.name != "Street_Straight");
                            })
                            .ToList();
                        selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];
                    }else{
                        selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];
                    }
                }else{
                    if(!IsAheadClear(x,y,streetLength)){
                        // Filter the possibleTiles list to remove tiles that have "Street_Straight" in allowedAbove
                        cellGrid[x, y].possibleTiles = cellGrid[x, y].possibleTiles
                            .Where(tilePrefab =>
                            {
                                // Access the Tile component
                                Tile tileComponent = tilePrefab.GetComponent<Tile>();
                                        
                                // Check if allowedAbove contains any GameObject with the name "Street_Straight"
                                return tileComponent.allowedAbove.All(allowedTile => allowedTile.name != "Street_Straight");
                            })
                            .ToList();
                        selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];
                    }else{
                        selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];
                    }
                }*/

                //Go into the loop if necessary to loop until fitting tile is found
                if (hasStreetStraight && !IsAheadClear(x,y,streetLength) &&
                selectedTilePrefab.gameObject.name != "Street_Empty" && selectedTilePrefab.gameObject.name != "Street_Straight"){
                    while(true){
                        if(cellGrid[x,y].possibleTiles.Count == 1) break;
                        cellGrid[x,y].possibleTiles.Remove(selectedTilePrefab);
                        selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                        //Check if the selected Tile is acceptable for forcing the desired amount of streets
                        tilesRight = selectedTilePrefab.GetComponent<Tile>().allowedAbove;
                        hasStreetStraight = tilesRight.Any(tile => tile.name == "Street_Straight");

                        if (hasStreetStraight && !IsAheadClear(x,y,streetLength) &&
                        selectedTilePrefab.gameObject.name != "Street_Empty" && selectedTilePrefab.gameObject.name != "Street_Straight"){
                            continue;
                        }else{
                            break;
                        }
                    }
                }

                //Check if curve was placed if it is contained in a loop before it gets set
                if (selectedTilePrefab.name.Contains("Curve"))
                {
                    CheckForLoop(x,y, selectedTilePrefab);
                }

                // Remove the selected tile from the possibleTiles list
                cellGrid[x, y].possibleTiles.RemoveAll(tile => tile != selectedTilePrefab);

                // Set the selected tile on the cell
                cellGrid[x, y].SetTile(selectedTilePrefab);

                // Proceed with setting neighbors and instantiating the tile
                SetNeighboursHorizontally(x, y);

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, y);
                cellGrid[x, y].instantiatedTile = Instantiate(selectedTilePrefab, position, Quaternion.identity);

                //TODO only do stuff when you are setting a non straig street or empty tile

                if (selectedTilePrefab.GetComponent<Tile>().allowedAbove.Any(tile => tile.name == "Street_Straight") 
                && selectedTilePrefab.gameObject.name != "Street_Straight")
                {
                    DoSomethingHorizontal(x, y);
                }

                if (selectedTilePrefab.GetComponent<Tile>().allowedLeft.Any(tile => tile.name == "Street_Straight (1)")
                && selectedTilePrefab.gameObject.name != "Street_Straight (1)")
                {
                    DoSomethingVertical(x, y);
                }
            }
        }
    }

    #region PastExample
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
            if (cellGrid[x, y].possibleTiles.Count == 0 || cellGrid[x,y].tileSet)
            {
                unprocessedCells.RemoveAt(0);
                continue;
            }

            // Randomly select a tile from the remaining possible tiles
            GameObject selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

            //Check if the selected Tile is acceptable for forcing the desired amount of streets
            List<GameObject> tilesRight = selectedTilePrefab.GetComponent<Tile>().allowedAbove;
            bool hasStreetStraight = tilesRight.Any(tile => tile.name == "Street_Straight");

            //Go into the loop if necessary to loop until fitting tile is found
            //TODO also look in the other directions as every direction is now relevant
            if (hasStreetStraight && !IsAheadClear(x,y,streetLength) &&
            selectedTilePrefab.gameObject.name != "Street_Empty" 
            && selectedTilePrefab.gameObject.name != "Street_Straight"
            && selectedTilePrefab.gameObject.name != "Street_Straight (1)"){
                while(true){
                    //if(cellGrid[x,y].possibleTiles.Count == 1) break;
                    cellGrid[x,y].possibleTiles.Remove(selectedTilePrefab);
                    if(cellGrid[x,y].possibleTiles.Count == 0) break;
                    selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                    //Check if the selected Tile is acceptable for forcing the desired amount of streets
                    tilesRight = selectedTilePrefab.GetComponent<Tile>().allowedAbove;
                    hasStreetStraight = tilesRight.Any(tile => tile.name == "Street_Straight");

                    if (hasStreetStraight && !IsAheadClear(x,y,streetLength) &&
                    selectedTilePrefab.gameObject.name != "Street_Empty"
                    && selectedTilePrefab.gameObject.name != "Street_Straight"
                    && selectedTilePrefab.gameObject.name != "Street_Straight (1)"){
                        continue;
                    }else{
                        break;
                    }
                }
            }

            if (cellGrid[x, y].possibleTiles.Count == 0)
            {
                unprocessedCells.RemoveAt(0);
                continue;
            }

            cellGrid[x, y].possibleTiles.RemoveAll(tile => tile != selectedTilePrefab);

            // Set the selected tile on the cell
            cellGrid[x, y].SetTile(selectedTilePrefab);

            // Set neighbors after placing the tile
            SetNeighboursHorizontally(x, y);

            // Instantiate the selected tile at the grid position
            Vector3 position = new Vector3(x, 0, y);
            cellGrid[x, y].instantiatedTile = Instantiate(selectedTilePrefab, position, Quaternion.identity);

            //TODO also integrate IsAheadClear() because it assumes it is always clear ahead
            if (selectedTilePrefab.GetComponent<Tile>().allowedAbove.Any(tile => tile.name == "Street_Straight") 
            && selectedTilePrefab.gameObject.name != "Street_Straight")
            {
                DoSomethingHorizontal(x, y);
            }

            /*if (selectedTilePrefab.GetComponent<Tile>().allowedLeft.Any(tile => tile.name == "Street_Straight (1)")
            && selectedTilePrefab.gameObject.name != "Street_Straight (1)")
            {
                DoSomethingVertical(x, y);
            }*/

            // Remove this cell from the unprocessed list as it's now set
            unprocessedCells.RemoveAt(0);
            GoThroughEverything();
        }
    }
#endregion
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

    private void DoSomethingHorizontal(int x, int y){
        //Set the horizontal Line (currently fix 3 long) Number - 1 == the length (ex. 1 < 4 + y == 3)
        //UnityEngine.Debug.Log("Method Starts with x:" + x + " y:" + y);
        for (int i = x + 1; i < streetLength + 1 + x; i++) 
        {
            if(i >= gridSize){
                break;
            }

            if(cellGrid[i, y].tileSet){
                UnityEngine.Debug.LogError("Should be empty ? " + cellGrid[i, y].instantiatedTile.name);
                break;
            }

            cellGrid[i, y].possibleTiles = cellGrid[i, y].possibleTiles
            .Where(tile => Regex.IsMatch(tile.name, @"^Street_Straight(\(Clone\))*$"))
            .ToList();
            
            // Set the selected tile on the cell
            cellGrid[i, y].SetTile(cellGrid[i, y].possibleTiles.First());

            // Proceed with setting neighbors and instantiating the tile
            SetNeighboursHorizontally(i, y);

            // Instantiate the selected tile at the grid position
            Vector3 position = new Vector3(i, 0, y);
            cellGrid[i, y].instantiatedTile = Instantiate(cellGrid[i, y].possibleTiles.First(), position, Quaternion.identity);

            //TODO set allowedUp and allowedLeft and allowedRight
            /*if(i + 1 < gridSize){
                //allowedAbove
                cellGrid[i + 1, y].possibleTiles = cellGrid[i, y].instantiatedTile.GetComponent<Tile>().allowedAbove;
                
            }
            if(y + 1 < gridSize){
                //allowedLeft
                cellGrid[i, y + 1].possibleTiles = cellGrid[i, y].instantiatedTile.GetComponent<Tile>().allowedLeft;
            }
            if(y - 1 >= 0){
                //allowedRight
                cellGrid[i, y - 1].possibleTiles = cellGrid[i, y].instantiatedTile.GetComponent<Tile>().allowedRight;
            }*/
            GoThroughEverything();
        }
    }

    private void DoSomethingVertical(int x, int y){
        //Set the vertical Line (currently fix 3 long) Number - 1 == the length (ex. 1 < 4 + y == 3)
        for (int i = y + 1; i < streetLength + 1 + y; i++) 
        {
            if(i >= gridSize){
                break;
            }
            
            if(cellGrid[x, i].tileSet){
            UnityEngine.Debug.LogError("Should be empty ?" + cellGrid[i, y].instantiatedTile.name);
            break;
            }

            cellGrid[x, i].possibleTiles = cellGrid[x, i].possibleTiles
            .Where(tile => Regex.IsMatch(tile.name, @"^Street_Straight \(1\)(\(Clone\))*$"))
            .ToList();

            // Set the selected tile on the cell
            cellGrid[x, i].SetTile(cellGrid[x, i].possibleTiles.First());

            // Proceed with setting neighbors and instantiating the tile
            SetNeighboursHorizontally(x, i);

            // Instantiate the selected tile at the grid position
            Vector3 position = new Vector3(x, 0, i);
            cellGrid[x, i].instantiatedTile = Instantiate(cellGrid[x, i].possibleTiles.First(), position, Quaternion.identity);

            //TODO set allowedUp and allowedLeft and allowedDown
            /*if(i + 1 < gridSize){
                //allowedLeft
                cellGrid[x, i + 1].possibleTiles = cellGrid[x, i].instantiatedTile.GetComponent<Tile>().allowedLeft;

                // Get the allowedAbove list
                List<GameObject> allowedAbove2 = cellGrid[x, i].instantiatedTile.GetComponent<Tile>().allowedLeft;

                // Build a string with the names of the GameObjects in the list
                string allowedAboveNames2 = string.Join(", ", allowedAbove2.Select(tile => tile.name));

                // Log the names
                UnityEngine.Debug.Log("Allowed Left Tiles for cellGrid[" + x + ", " + i + "]: " + allowedAboveNames2);
            }
            if(x + 1 < gridSize){
                //allowedUp
                cellGrid[x + 1, i].possibleTiles = cellGrid[x, i].instantiatedTile.GetComponent<Tile>().allowedAbove;
            }
            if(x - 1 >= 0){
                //allowedDown
                cellGrid[x - 1, i].possibleTiles = cellGrid[x, i].instantiatedTile.GetComponent<Tile>().allowedBelow;
            }*/
            //TODO add that before we reset all options in cells
            GoThroughEverything();
        }
    }

    private void GoThroughEverything()
    {
        for (int i = gridSize - 1; i > 0; i--)
        {
            for (int z = gridSize - 1; z > 0; z--)
            {
                if (!cellGrid[z, i].tileSet && cellGrid[z, i].possibleTiles.Count != tilePrefabs.Count)
                {
                    SetNeighboursOnlyNeighbours(z, i);
                }
            }
        }
    }

    public bool IsAheadClear(int startX, int startY, int distance)
    {
        // Define the regex pattern to match "Street_Straight" with any number of "(Clone)"
        string pattern = @"^Street_Straight(\(Clone\))*$";

        // Loop through the next distance + 1 tiles in the x direction
        for (int i = 1; i <= distance + 1; i++)
        {
            int nextX = startX + i;

            // Check if the nextX exceeds the grid boundaries
            if (nextX >= cellGrid.GetLength(0))
            {
                // If we are out of bounds, stop checking
                break;
            }

            // Check the tileSet property of the current cell
            bool regexBool = false;
            if(cellGrid[nextX, startY].instantiatedTile != null) regexBool = Regex.IsMatch(cellGrid[nextX, startY].instantiatedTile.gameObject.name, @"^Street_Straight(\(Clone\))*$");
            if (cellGrid[nextX, startY].tileSet && !regexBool)
            {
                // If tileSet is true and the tile is NOT "Street_Straight" with or without (Clone), return false
                UnityEngine.Debug.Log("FUCK OFF " + cellGrid[nextX, startY].instantiatedTile.gameObject.name + " " + regexBool);
                return false;
            }

            // Check if "Street_Straight" with any "(Clone)" suffix exists in possibleTiles
            bool hasStreetStraight = cellGrid[nextX, startY].possibleTiles
                .Any(tile => Regex.IsMatch(tile.name, pattern));

            if (!hasStreetStraight && i <= distance)
            {
                // If no "Street_Straight" tile is found, return false
                return false;
            }
        }

        // If all checks pass, return true
        return true;
    }












    private void AttemptThatDidNotwork(){
        //Check if the selected Tile is acceptable for forcing the desired amount of streets
                /*List<GameObject> tilesRight = selectedTilePrefab.GetComponent<Tile>().allowedAbove;
                bool hasStreetStraight = tilesRight.Any(tile => tile.name == "Street_Straight");

                if (hasStreetStraight && selectedTilePrefab.gameObject.name != "Street_Empty" && selectedTilePrefab.gameObject.name != "Street_Straight")
                {
                    
                    UnityEngine.Debug.Log("1 Street_Straight found in tilesRight at x:" + x + " y:" + y + " IsAheadClear was: " + IsAheadClear(x,y,streetLength));
                    // Perform desired action
                    if(!IsAheadClear(x,y,3)){
                        cellGrid[x,y].possibleTiles.Remove(selectedTilePrefab);
                        selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                        //Check if the selected Tile is acceptable for forcing the desired amount of streets
                        tilesRight = selectedTilePrefab.GetComponent<Tile>().allowedAbove;
                        hasStreetStraight = tilesRight.Any(tile => tile.name == "Street_Straight");

                        if (hasStreetStraight && selectedTilePrefab.gameObject.name != "Street_Empty" && selectedTilePrefab.gameObject.name != "Street_Straight"){
                            UnityEngine.Debug.Log("2 Street_Straight found in tilesRight at x:" + x + " y:" + y + " IsAheadClear was: " + IsAheadClear(x,y,streetLength));
                            // Perform desired action
                            if(!IsAheadClear(x,y,3)){
                                cellGrid[x,y].possibleTiles.Remove(selectedTilePrefab);
                                selectedTilePrefab = cellGrid[x, y].possibleTiles[Random.Range(0, cellGrid[x, y].possibleTiles.Count)];

                                //Check if the selected Tile is acceptable for forcing the desired amount of streets
                                tilesRight = selectedTilePrefab.GetComponent<Tile>().allowedAbove;
                                hasStreetStraight = tilesRight.Any(tile => tile.name == "Street_Straight");

                                if (hasStreetStraight && selectedTilePrefab.gameObject.name != "Street_Empty" && selectedTilePrefab.gameObject.name != "Street_Straight"){
                                    UnityEngine.Debug.Log("3 Street_Straight found in tilesRight at x:" + x + " y:" + y + " IsAheadClear was: " + IsAheadClear(x,y,streetLength));
                                }
                            }
                        }
                    }
                }*/
    }

    private void CheckForLoop(int x, int y, GameObject tile)
    {
        // Initialize a dictionary to track traversed paths
        Dictionary<(int x, int y), HashSet<Direction>> traversedPaths = new Dictionary<(int x, int y), HashSet<Direction>>();
        Direction direction = Direction.None;

        // Function to mark a tile as traversed
        void MarkTraversed(int currentX, int currentY, Direction dir)
        {
            var key = (currentX, currentY);
            if (!traversedPaths.ContainsKey(key))
            {
                traversedPaths[key] = new HashSet<Direction>();
            }
            traversedPaths[key].Add(dir);
        }

        // Function to check if a tile has already been traversed in a direction
        bool HasBeenTraversed(int currentX, int currentY, Direction dir)
        {
            return traversedPaths.TryGetValue((currentX, currentY), out var directions) && directions.Contains(dir);
        }

        GameObject currentTile = tile;

        while(true){
            //TODO remembering of the direction should only occur when it is not a Straight Tile
            //TODO because otherwise double checking the crossings does not work
            // Check for above
            if (currentTile.GetComponent<Tile>().allowedLeft.Any(go => Regex.IsMatch(go.name, @"^Street_Straight \(1\)(\s\(Clone\))*$"))
                && direction != Direction.Down && !HasBeenTraversed(x, y, Direction.Up))
            {
                direction = Direction.Up;
                if(!currentTile.name.Contains("Street_Straight")) MarkTraversed(x, y, direction);
                //UnityEngine.Debug.Log("Can go up in x: " + x + " y: " + y);
                if(y != 0) y++;
            }
            else
            // Check for right
            if (currentTile.GetComponent<Tile>().allowedAbove.Any(go => Regex.IsMatch(go.name, @"^Street_Straight(\s\(Clone\))*$"))
                && direction != Direction.Left && !HasBeenTraversed(x, y, Direction.Right))
            {
                direction = Direction.Right;
                if(!currentTile.name.Contains("Street_Straight")) MarkTraversed(x, y, direction);
                //UnityEngine.Debug.Log("Can go right in x: " + x + " y: " + y);
                if(x != 0) x++;
            }
            else
            // Check for left
            if (currentTile.GetComponent<Tile>().allowedBelow.Any(go => Regex.IsMatch(go.name, @"^Street_Straight(\s\(Clone\))*$"))
                && direction != Direction.Right && !HasBeenTraversed(x, y, Direction.Left))
            {
                direction = Direction.Left;
                if(!currentTile.name.Contains("Street_Straight")) MarkTraversed(x, y, direction);
                //UnityEngine.Debug.Log("Can go left in x: " + x + " y: " + y);
                if(x != gridSize - 1) x--;
            }
            else
            // Check for down
            if (currentTile.GetComponent<Tile>().allowedRight.Any(go => Regex.IsMatch(go.name, @"^Street_Straight \(1\)(\s\(Clone\))*$"))
                && direction != Direction.Up && !HasBeenTraversed(x, y, Direction.Down))
            {
                direction = Direction.Down;
                if(!currentTile.name.Contains("Street_Straight")) MarkTraversed(x, y, direction);
                //UnityEngine.Debug.Log("Can go down in x: " + x + " y: " + y);
                if(y != gridSize - 1) y--;
            }
            else
            {
                UnityEngine.Debug.Log("Should only reach this in a loop");
                break;
            }

            //after traversing check if you are on the edge
            //if yes assume that street leads outside the grid
            //TODO check if the street also has an opening in that direction 
            if(x <= 0 || y <= 0 || x >= gridSize - 1 || y >= gridSize - 1){
                UnityEngine.Debug.Log("Reached the edge and assumes it goes beyond the grid size");
                break;
            }else{
                if(cellGrid[x,y].tileSet){
                    currentTile = cellGrid[x,y].instantiatedTile;
                }else{
                    UnityEngine.Debug.Log("Break as it has not finished building the loop");
                    break;
                }
            }
        }

        // Log traversedPaths in a readable format
        string traversedPathsString = "Traversed Paths:\n";
        foreach (var kvp in traversedPaths)
        {
            var coordinates = kvp.Key;
            var directions = kvp.Value;

            traversedPathsString += $"Tile ({coordinates.x}, {coordinates.y}): Directions - {string.Join(", ", directions)}\n";
        }

        UnityEngine.Debug.Log(traversedPathsString);
    }


    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    }



}