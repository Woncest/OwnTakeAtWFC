using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

public class TileGridGenerator : MonoBehaviour
{
    public int gridSize = 25;  // Size of the grid (gridSize x gridSize)

    [HideInInspector] public int newGridSize = 25;
    public CameraZoomController cameraZoomController;
    public List<GameObject> tilePrefabs;  // Array of tile prefabs to choose from
    public bool showGenerationProcess = true;  // New boolean to control visual generation
    public float generationDelay = 0.01f;  // Delay between each tile generation
    private Coroutine coroutine;

    private Cell[,] cellGrid;

    [HideInInspector] public int streetLength = 2;

    public GameObject street_straight;

    public GameObject street_straight1;

    public ProbabilityInputFieldManager probabilities;

    public float cellSize = 1.0f;

    public Color lineColor = Color.black;

    void Start()
    {
        SetProbabilities();
        InitializeCellGrid();
        //GenerateAndTimeGrid();  // Modified method call
        GenerateGridVisual();
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
            SetProbabilities();
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

        //method to set the street_straight and street:straight1
        SetSpecialTilesToCurrentTileSet();

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

                // Remove the selected tile from the possibleTiles list
                cellGrid[x, y].possibleTiles.RemoveAll(tile => tile != selectedTilePrefab);

                // Set the selected tile on the cell
                cellGrid[x, y].SetTile(selectedTilePrefab);

                // Proceed with setting neighbors and instantiating the tile
                SetNeighboursHorizontally(x, y);

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, y);
                cellGrid[x, y].instantiatedTile = Instantiate(selectedTilePrefab, position, Quaternion.identity);

                bool foundLoop = false;
                //Check if curve was placed if it is contained in a loop before it gets set
                if (selectedTilePrefab.name.Contains("Curve"))
                {
                    foundLoop = CheckForLoop(x,y, selectedTilePrefab);
                }

                //TODO does not work perfectly yet, often sets a wrong tile and I would not know why
                //TODO when removing tiles should check if vertical street is being removed, because that is wrong
                //TODO easiest solution would be just start anew when loop is found
                if(foundLoop){
                    ResetCurrentRow(y);
                    GoThroughEverything();
                    y--;
                    break;
                }

                //TODO only do stuff when you are setting a non straig street or empty tile
                if (selectedTilePrefab.GetComponent<Tile>().allowedAbove.Any(tile => tile.name == "Street_Straight") 
                && selectedTilePrefab.gameObject.name != "Street_Straight" && !foundLoop)
                {
                    DoSomethingHorizontalEverySecond1Space(x, y);
                }

                if (selectedTilePrefab.GetComponent<Tile>().allowedLeft.Any(tile => tile.name == "Street_Straight (1)")
                && selectedTilePrefab.gameObject.name != "Street_Straight (1)" && !foundLoop)
                {
                    DoSomethingVerticalEverySecond1Space(x, y);
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

    void ResetCurrentRow(int yOriginal){
        for (int y = yOriginal; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                if(cellGrid[x, y].instantiatedTile != null && cellGrid[x,y].instantiatedTile.gameObject.name.Contains("Street_Straight (1)")) continue;
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

            //Past Idea was the same in DoSomethingVertical
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

    private void DoSomethingHorizontalEverySecond(int x, int y){
        //Set the horizontal Line (currently fix 3 long) Number - 1 == the length (ex. 1 < 4 + y == 3)
        //counter to keep track which segment of the street we are looking at
        int counter = 1;
        SetSpecialTilesToCurrentTileSet();
        for (int i = x + 1; i < streetLength + 1 + x; i++) 
        {
            if(i >= gridSize){
                break;
            }

            if(counter%2 != 0){
                counter++;
                continue;
            }

            if(cellGrid[i, y].tileSet){
                UnityEngine.Debug.LogError("Should be empty ? " + cellGrid[i, y].instantiatedTile.name);
                break;
            }

            //Go into the special tile and set the sides to what this tileset would allow in the not forced directions
            cellGrid[i,y].possibleTiles.Clear();
            if(streetLength == counter){
                cellGrid[i,y].possibleTiles.Add(street_straight);
            }else{
                street_straight.GetComponent<Tile>().allowedAbove = street_straight.GetComponent<Tile>().allowedBelow;
                cellGrid[i,y].possibleTiles.Add(street_straight);
            }
            
            // Set the selected tile on the cell
            cellGrid[i, y].SetTile(cellGrid[i, y].possibleTiles.First());

            // Proceed with setting neighbors and instantiating the tile
            SetNeighboursHorizontally(i, y);

            // Instantiate the selected tile at the grid position
            Vector3 position = new Vector3(i, 0, y);
            cellGrid[i, y].instantiatedTile = Instantiate(cellGrid[i, y].possibleTiles.First(), position, Quaternion.identity);

            GoThroughEverything();
            SetSpecialTilesToCurrentTileSet();
            counter++;
        }
    }

    private void DoSomethingHorizontalEverySecond1Space(int x, int y){
        //Set the horizontal Line (currently fix 3 long) Number - 1 == the length (ex. 1 < 4 + y == 3)
        //counter to keep track which segment of the street we are looking at

        //TODO should also lookout if you are too near the edge
        if(streetLength <= 2 || x + 2 == gridSize - 1 || y + 2 == gridSize - 1){
            DoSomethingHorizontalEverySecond(x,y);
        }else{
            int counter = 3;
            SetSpecialTilesToCurrentTileSet();
            for (int i = x + 3; i < streetLength + 1 + x; i++) 
            {
                if(i >= gridSize){
                    break;
                }

                if(counter%2 == 0){
                    counter++;
                    continue;
                }

                if(cellGrid[i, y].tileSet){
                    UnityEngine.Debug.LogError("Should be empty ? " + cellGrid[i, y].instantiatedTile.name);
                    break;
                }

                //Go into the special tile and set the sides to what this tileset would allow in the not forced directions
                cellGrid[i,y].possibleTiles.Clear();
                if(streetLength == counter){
                    cellGrid[i,y].possibleTiles.Add(street_straight);
                }else{
                    street_straight.GetComponent<Tile>().allowedAbove = street_straight.GetComponent<Tile>().allowedBelow;
                    cellGrid[i,y].possibleTiles.Add(street_straight);
                }
                
                // Set the selected tile on the cell
                cellGrid[i, y].SetTile(cellGrid[i, y].possibleTiles.First());

                // Proceed with setting neighbors and instantiating the tile
                SetNeighboursHorizontally(i, y);

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(i, 0, y);
                cellGrid[i, y].instantiatedTile = Instantiate(cellGrid[i, y].possibleTiles.First(), position, Quaternion.identity);

                GoThroughEverything();

                SetSpecialTilesToCurrentTileSet();
                counter++;
            }
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
            UnityEngine.Debug.LogError("Should be empty ?" + cellGrid[x, i].instantiatedTile.name);
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

            GoThroughEverything();
        }
    }

    private void DoSomethingVerticalEverySecond(int x, int y){
        //Set the vertical Line (currently fix 3 long) Number - 1 == the length (ex. 1 < 4 + y == 3)
        int counter = 1;
        for (int i = y + 1; i < streetLength + 1 + y; i++) 
        {
            if(i >= gridSize){
                break;
            }

            if(counter%2 != 0){
                counter++;
                continue;
            }
            
            if(cellGrid[x, i].tileSet){
            UnityEngine.Debug.Log("Should be empty ?" + cellGrid[x, i].instantiatedTile.name);
            break;
            }

            //Go into the special tile and set the sides to what this tileset would allow in the not forced directions
            cellGrid[x,i].possibleTiles.Clear();
            if(streetLength == counter){
                cellGrid[x,i].possibleTiles.Add(street_straight1);
            }else{
                street_straight1.GetComponent<Tile>().allowedLeft = street_straight1.GetComponent<Tile>().allowedRight;
                cellGrid[x,i].possibleTiles.Add(street_straight1);
            }

            // Set the selected tile on the cell
            cellGrid[x, i].SetTile(cellGrid[x, i].possibleTiles.First());

            // Proceed with setting neighbors and instantiating the tile
            SetNeighboursHorizontally(x, i);

            // Instantiate the selected tile at the grid position
            Vector3 position = new Vector3(x, 0, i);
            cellGrid[x, i].instantiatedTile = Instantiate(cellGrid[x, i].possibleTiles.First(), position, Quaternion.identity);

            GoThroughEverything();
            SetSpecialTilesToCurrentTileSet();
            counter++;
        }
    }

    private void DoSomethingVerticalEverySecond1Space(int x, int y){
        //Set the vertical Line (currently fix 3 long) Number - 1 == the length (ex. 1 < 4 + y == 3)

        if(streetLength <= 2 || x + 2 == gridSize - 1 || y + 2 == gridSize - 1){
            DoSomethingVerticalEverySecond(x,y);
        }else{
            int counter = 3;
            SetSpecialTilesToCurrentTileSet();
            for (int i = y + 3; i < streetLength + 1 + y; i++) 
            {
                if(i >= gridSize){
                    break;
                }

                if(counter%2 == 0){
                    counter++;
                    continue;
                }
                
                if(cellGrid[x, i].tileSet){
                UnityEngine.Debug.LogError("Should be empty ?" + cellGrid[x, i].instantiatedTile.name);
                break;
                }

                //Go into the special tile and set the sides to what this tileset would allow in the not forced directions
                cellGrid[x,i].possibleTiles.Clear();
                if(streetLength == counter){
                    cellGrid[x,i].possibleTiles.Add(street_straight1);
                }else{
                    street_straight1.GetComponent<Tile>().allowedLeft = street_straight1.GetComponent<Tile>().allowedRight;
                    cellGrid[x,i].possibleTiles.Add(street_straight1);
                }

                // Set the selected tile on the cell
                cellGrid[x, i].SetTile(cellGrid[x, i].possibleTiles.First());

                // Proceed with setting neighbors and instantiating the tile
                SetNeighboursHorizontally(x, i);

                // Instantiate the selected tile at the grid position
                Vector3 position = new Vector3(x, 0, i);
                cellGrid[x, i].instantiatedTile = Instantiate(cellGrid[x, i].possibleTiles.First(), position, Quaternion.identity);

                GoThroughEverything();
                SetSpecialTilesToCurrentTileSet();
                counter++;
            }
        }
    }

    private void GoThroughEverything()
    {
        for (int i = gridSize - 1; i >= 0; i--)
        {
            for (int z = gridSize - 1; z >= 0; z--)
            {
                if (cellGrid[z, i].possibleTiles.Count != tilePrefabs.Count)
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







    void SetSpecialTilesToCurrentTileSet(){
        //Set street_straight
        GameObject street_straightFromTileSet = tilePrefabs.FirstOrDefault(prefab => prefab.name == "Street_Straight");
        street_straight.GetComponent<Tile>().allowedAbove = street_straightFromTileSet.GetComponent<Tile>().allowedAbove;
        street_straight.GetComponent<Tile>().allowedLeft = street_straightFromTileSet.GetComponent<Tile>().allowedLeft;
        street_straight.GetComponent<Tile>().allowedRight = street_straightFromTileSet.GetComponent<Tile>().allowedRight;
        street_straight.GetComponent<Tile>().allowedBelow.Clear();
        street_straight.GetComponent<Tile>().allowedBelow.Add(street_straightFromTileSet);

        //Set street_straight1
        GameObject street_straight1FromTileSet = tilePrefabs.FirstOrDefault(prefab => prefab.name == "Street_Straight (1)");
        street_straight1.GetComponent<Tile>().allowedAbove = street_straight1FromTileSet.GetComponent<Tile>().allowedAbove;
        street_straight1.GetComponent<Tile>().allowedLeft = street_straight1FromTileSet.GetComponent<Tile>().allowedLeft;
        street_straight1.GetComponent<Tile>().allowedBelow = street_straight1FromTileSet.GetComponent<Tile>().allowedBelow;
        street_straight1.GetComponent<Tile>().allowedRight.Clear();
        street_straight1.GetComponent<Tile>().allowedRight.Add(street_straight1FromTileSet);
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

    private bool CheckForLoop(int x, int y, GameObject tile)
    {
        // Initialize a dictionary to track traversed paths and an additional int value
        Dictionary<(int x, int y), (HashSet<Direction> directions, int amountOpenSides)> traversedPaths =
            new Dictionary<(int x, int y), (HashSet<Direction>, int)>();
        Direction direction = Direction.None;

        // Function to mark a tile as traversed
        void MarkTraversed(int currentX, int currentY, Direction dir, int value)
        {
            var key = (currentX, currentY);
            if (!traversedPaths.ContainsKey(key))
            {
                // Initialize the entry with an empty HashSet for directions and the provided value
                traversedPaths[key] = (new HashSet<Direction>(), value);
            }

            // Add the direction to the HashSet and update the int value
            traversedPaths[key].directions.Add(dir);
            traversedPaths[key] = (traversedPaths[key].directions, value);
        }

        // Function to check if a tile has already been traversed in a direction
        bool HasBeenTraversed(int currentX, int currentY, Direction dir)
        {
            return traversedPaths.TryGetValue((currentX, currentY), out var data) && data.directions.Contains(dir);
        }

        GameObject currentTile = tile;

        while (true)
        {
            // Check for above
            if (currentTile.GetComponent<Tile>().allowedLeft.Any(go => Regex.IsMatch(go.name, @"^Street_Straight \(1\)(\(Clone\))*$"))
                && direction != Direction.Down && !HasBeenTraversed(x, y, Direction.Up))
            {
                direction = Direction.Up;
                MarkTraversed(x, y, direction, CountOpenSides(x,y));
                if (y != 0) y++;
            }
            else
            // Check for right
            if (currentTile.GetComponent<Tile>().allowedAbove.Any(go => Regex.IsMatch(go.name, @"^Street_Straight(\(Clone\))*$"))
                && direction != Direction.Left && !HasBeenTraversed(x, y, Direction.Right))
            {
                direction = Direction.Right;
                MarkTraversed(x, y, direction, CountOpenSides(x,y));
                if (x != 0) x++;
            }
            else
            // Check for left
            if (currentTile.GetComponent<Tile>().allowedBelow.Any(go => Regex.IsMatch(go.name, @"^Street_Straight(\(Clone\))*$"))
                && direction != Direction.Right && !HasBeenTraversed(x, y, Direction.Left))
            {
                direction = Direction.Left;
                MarkTraversed(x, y, direction, CountOpenSides(x,y));
                if (x != gridSize - 1) x--;
            }
            else
            // Check for down
            if (currentTile.GetComponent<Tile>().allowedRight.Any(go => Regex.IsMatch(go.name, @"^Street_Straight \(1\)(\(Clone\))*$"))
                && direction != Direction.Up && !HasBeenTraversed(x, y, Direction.Down))
            {
                direction = Direction.Down;
                MarkTraversed(x, y, direction, CountOpenSides(x,y));
                if (y != gridSize - 1) y--;
            }
            else
            {
                //Last Checks before determening if it is a loop
                bool cut = false;
                foreach (var kvp in traversedPaths)
                {
                    var coordinates = kvp.Key;
                    var data = kvp.Value;

                    if(!cellGrid[coordinates.x, coordinates.y].instantiatedTile.gameObject.name.Contains("Street_Straight")){
                        if (data.amountOpenSides != data.directions.Count){
                            x = coordinates.x;
                            y = coordinates.y;
                            cut = true;
                            direction = Direction.None;
                            break;
                        }
                    }
                }
                if(!cut){
                    UnityEngine.Debug.Log("Fuck you loop found x: " + x + " y: " + y);
                    return true;
                }
            }

            if (x <= 0 || y <= 0 || x >= gridSize - 1 || y >= gridSize - 1)
            {
                return false;
            }
            else
            {
                if (cellGrid[x, y].tileSet)
                {
                    currentTile = cellGrid[x, y].instantiatedTile;
                }
                else
                {
                    return false;
                }
            }
        }

        // Log traversedPaths in a readable format
        /*if (loop)
        {
            string traversedPathsString = "Traversed Paths:\n";
            foreach (var kvp in traversedPaths)
            {
                var coordinates = kvp.Key;
                var data = kvp.Value;

                traversedPathsString += $"Tile ({coordinates.x}, {coordinates.y}): Directions - {string.Join(", ", data.directions)}, Value - {data.amountOpenSides}\n";
            }

            UnityEngine.Debug.Log(traversedPathsString + "\n Length of traversedPaths " + traversedPaths.Count);
        }*/
    }

    int CountOpenSides(int x, int y){
        int count = 0;
        GameObject currentTile = cellGrid[x,y].instantiatedTile;

        // Check for above
            if (currentTile.GetComponent<Tile>().allowedLeft.Any(go => Regex.IsMatch(go.name, @"^Street_Straight \(1\)(\(Clone\))*$")))
            {
                count++;
            }
            // Check for right
            if (currentTile.GetComponent<Tile>().allowedAbove.Any(go => Regex.IsMatch(go.name, @"^Street_Straight(\(Clone\))*$")))
            {
                count++;
            }
            // Check for left
            if (currentTile.GetComponent<Tile>().allowedBelow.Any(go => Regex.IsMatch(go.name, @"^Street_Straight(\(Clone\))*$")))
            {
                count++;
            }
            // Check for down
            if (currentTile.GetComponent<Tile>().allowedRight.Any(go => Regex.IsMatch(go.name, @"^Street_Straight \(1\)(\(Clone\))*$")))
            {
                count++;
            }

        return count;
    }



    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    }

    public void SetProbabilities()
    {
        // Local variables to store counts
        int fourWayCount = 0;
        int threeWayCount = 0;
        int curveCount = 0;
        int emptyCount = 0;
        int streetStraightCount = 0;

        // Iterate through the list of tilePrefabs
        foreach (GameObject tile in tilePrefabs)
        {
            if (tile != null)
            {
                string tileName = tile.name;

                // Check the name of the tile and increment the respective count
                if (tileName.Contains("4Way"))
                {
                    fourWayCount++;
                }
                else if (tileName.Contains("3Way"))
                {
                    threeWayCount++;
                }
                else if (tileName.Contains("Curve"))
                {
                    curveCount++;
                }
                else if (tileName.Contains("Empty"))
                {
                    emptyCount++;
                }
                else if (tileName.Contains("Straight"))
                {
                    streetStraightCount++;
                }
            }
        }

        if(fourWayCount != probabilities.fourWay){
            GameObject fourWayTile = tilePrefabs.Find(tile => tile.name.Contains("4Way"));
            AdjustInstances(fourWayTile, probabilities.fourWay);
        }

        if(threeWayCount/4 != probabilities.threeWay){
            List<GameObject> uniqueThreeWayTiles = GetUniqueGameObjectsByName(tilePrefabs, "3Way");
            foreach (GameObject tile in uniqueThreeWayTiles){
                AdjustInstances(tile, probabilities.threeWay);
            }
        }

        if(curveCount/4 != probabilities.curve){
            List<GameObject> uniqueCurveTiles = GetUniqueGameObjectsByName(tilePrefabs, "Curve");
            foreach (GameObject tile in uniqueCurveTiles){
                AdjustInstances(tile, probabilities.curve);
            }
        }

        if(emptyCount != probabilities.empty){
            GameObject emptyTile = tilePrefabs.Find(tile => tile.name.Contains("Empty"));
            AdjustInstances(emptyTile, probabilities.empty);
        }

        if(streetStraightCount/2 != probabilities.streetStraight){
            List<GameObject> uniqueStraightTiles = GetUniqueGameObjectsByName(tilePrefabs, "Straight");
            foreach (GameObject tile in uniqueStraightTiles){
                AdjustInstances(tile, probabilities.streetStraight);
            }
        }
    }

    void AdjustInstances(GameObject targetObject, int targetCount)
    {
        // Count how many instances currently exist
        int currentCount = CountInstances(targetObject);

        if (currentCount < targetCount)
        {
            // Add more instances if there are too few
            int instancesToAdd = targetCount - currentCount;
            for (int i = 0; i < instancesToAdd; i++)
            {
                tilePrefabs.Add(targetObject);
            }
        }
        else if (currentCount > targetCount)
        {
            // Remove extra instances if there are too many
            int instancesToRemove = currentCount - targetCount;
            for (int i = 0; i < instancesToRemove; i++)
            {
                tilePrefabs.Remove(targetObject);
            }
        }
    }

    int CountInstances(GameObject targetObject)
    {
        int count = 0;
        foreach (GameObject obj in tilePrefabs)
        {
            if (obj == targetObject)
            {
                count++;
            }
        }
        return count;
    }

    List<GameObject> GetUniqueGameObjectsByName(List<GameObject> gameObjects, string substring)
    {
        // Use a HashSet to ensure uniqueness
        HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

        foreach (GameObject obj in gameObjects)
        {
            if (obj.name.Contains(substring))
            {
                uniqueObjects.Add(obj);
            }
        }

        // Convert the HashSet back to a List
        return new List<GameObject>(uniqueObjects);
    }

    /// <summary>
    /// Generates a grid with clickable cells, each showing its coordinates when clicked.
    /// </summary>
    void GenerateGridVisual()
    {
        // Create a parent GameObject to organize the grid lines and cells
        GameObject gridParent = new GameObject("ClickableCellGrid");

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Calculate the center of the cell
                Vector3 cellCenter = new Vector3(x, 0, y);

                // Create the cell GameObject
                GameObject cell = new GameObject($"Cell_{x}_{y}");
                cell.transform.position = cellCenter;
                cell.transform.parent = gridParent.transform;

                // Add a collider for click detection
                BoxCollider collider = cell.AddComponent<BoxCollider>();
                collider.size = new Vector3(cellSize, 0.1f, cellSize); // Thin collider over the cell

                // Add a CellClickHandler to handle click events
                CellClickHandler clickHandler = cell.AddComponent<CellClickHandler>();
                clickHandler.coordinates = new Vector2Int(x, y);

                // Draw the cell's borders
                DrawCell(cellCenter, cell.transform);
            }
        }
    }

    /// <summary>
    /// Draws a single cell's border using LineRenderers.
    /// </summary>
    /// <param name="center">The center of the cell.</param>
    /// <param name="parent">The parent transform for organizing the lines in the hierarchy.</param>
    void DrawCell(Vector3 center, Transform parent)
    {
        // Calculate the corners of the cell based on its center
        float halfCellSize = cellSize / 2;
        Vector3 bottomLeft = center - new Vector3(halfCellSize, 0, halfCellSize);
        Vector3 bottomRight = center + new Vector3(halfCellSize, 0, -halfCellSize);
        Vector3 topRight = center + new Vector3(halfCellSize, 0, halfCellSize);
        Vector3 topLeft = center + new Vector3(-halfCellSize, 0, halfCellSize);

        // Draw the four borders of the cell
        DrawLine(bottomLeft, bottomRight, parent); // Bottom
        DrawLine(bottomRight, topRight, parent);   // Right
        DrawLine(topRight, topLeft, parent);       // Top
        DrawLine(topLeft, bottomLeft, parent);     // Left
    }

    /// <summary>
    /// Draws a single line using a LineRenderer.
    /// </summary>
    /// <param name="start">The starting point of the line.</param>
    /// <param name="end">The ending point of the line.</param>
    /// <param name="parent">The parent transform for organizing the line in the hierarchy.</param>
    void DrawLine(Vector3 start, Vector3 end, Transform parent)
    {
        // Create a new GameObject for the line
        GameObject lineObject = new GameObject("Line");
        lineObject.transform.parent = parent;

        // Add a LineRenderer component
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a default material
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        // Set the positions of the line
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        // Prevent the line from casting or receiving shadows
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
    }

}